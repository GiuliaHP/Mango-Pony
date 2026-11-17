#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class PonyDataLinker
{
    public static string NormalizeName(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in normalizedString)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        string result = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();
        return result.Replace("-", "").Replace("_", "").Replace(" ", "").Replace("&", "");
    }

    [MenuItem("Mango Pony/Link Prefabs, Icons & Textures to Data Assets")]
    public static int LinkAllPrefabsToDataAssets()
    {
        return LinkAllPrefabsToDataAssets(true);
    }

    public static int LinkAllPrefabsToDataAssets(bool logResults)
    {
        // 1. Récupère tous les prefabs dans Assets/Prefabs/Pony
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Pony" });
        List<GameObject> prefabs = new List<GameObject>();
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) prefabs.Add(prefab);
        }

        // 2. Récupère tous les sprites de PreviewIcons.psd
        string psdPath = "Assets/Sprites/PreviewIcons.psd";
        UnityEngine.Object[] psdAssets = AssetDatabase.LoadAllAssetsAtPath(psdPath);
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        if (psdAssets != null)
        {
            foreach (var asset in psdAssets)
            {
                if (asset is Sprite sprite)
                {
                    sprites[sprite.name] = sprite;
                    sprites[NormalizeName(sprite.name)] = sprite;
                }
            }
        }

        // 3. Récupère toutes les textures de masques (Textures/Masks)
        string[] maskGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Textures/Masks" });
        List<Texture2D> maskTextures = new List<Texture2D>();
        foreach (string guid in maskGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null) maskTextures.Add(tex);
        }

        if (logResults)
            Debug.Log($"[PonyDataLinker] {prefabs.Count} prefabs, {sprites.Count / 2} sprites d'icônes, {maskTextures.Count} textures de masque trouvés.");

        // 4. Parcourt tous les CustomizationCategoryData
        string[] dataGuids = AssetDatabase.FindAssets("t:CustomizationCategoryData", new[] { "Assets/Scriptables" });
        int linkedCount = 0;

        foreach (string guid in dataGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CustomizationCategoryData categoryData = AssetDatabase.LoadAssetAtPath<CustomizationCategoryData>(path);
            if (categoryData == null) continue;

            bool isDirty = false;
            SerializedObject serializedData = new SerializedObject(categoryData);
            serializedData.Update();

            SerializedProperty elementsProp = serializedData.FindProperty("elements");
            if (elementsProp != null)
            {
                for (int e = 0; e < elementsProp.arraySize; e++)
                {
                    SerializedProperty elementProp = elementsProp.GetArrayElementAtIndex(e);
                    SerializedProperty slotProp = elementProp.FindPropertyRelative("targetMeshSlot");
                    SerializedProperty elementNameProp = elementProp.FindPropertyRelative("elementName");
                    Swapper.SlotType slot = (Swapper.SlotType)slotProp.enumValueIndex;
                    string elementName = elementNameProp != null ? elementNameProp.stringValue : $"Element_{e}";

                    // A1. Shapes principales (Meshes 3D + Preview Icons)
                    SerializedProperty shapesProp = elementProp.FindPropertyRelative("shapes");
                    LinkShapesArray(shapesProp, slot, prefabs, sprites, categoryData.name, elementName, logResults, ref isDirty, ref linkedCount);

                    // A2. Shapes secondaires (ex: Forelock dans Mane)
                    SerializedProperty hasSecondaryProp = elementProp.FindPropertyRelative("hasSecondaryShapes");
                    if (hasSecondaryProp != null && hasSecondaryProp.boolValue)
                    {
                        SerializedProperty secondarySlotProp = elementProp.FindPropertyRelative("secondaryMeshSlot");
                        SerializedProperty secondaryShapesProp = elementProp.FindPropertyRelative("secondaryShapes");
                        SerializedProperty secondarySubProp = elementProp.FindPropertyRelative("secondaryShapesSubtitle");

                        Swapper.SlotType secondarySlot = (Swapper.SlotType)secondarySlotProp.enumValueIndex;
                        string secondaryLabel = secondarySubProp != null && !string.IsNullOrEmpty(secondarySubProp.stringValue)
                            ? $"{elementName} ({secondarySubProp.stringValue})"
                            : $"{elementName} (Secondary)";

                        LinkShapesArray(secondaryShapesProp, secondarySlot, prefabs, sprites, categoryData.name, secondaryLabel, logResults, ref isDirty, ref linkedCount);
                    }

                    // B. Patterns (Textures Shaders + Preview Icons)
                    SerializedProperty patternsProp = elementProp.FindPropertyRelative("patterns");
                    if (patternsProp != null && patternsProp.isArray)
                    {
                        for (int p = 0; p < patternsProp.arraySize; p++)
                        {
                            SerializedProperty patternProp = patternsProp.GetArrayElementAtIndex(p);
                            SerializedProperty patternNameProp = patternProp.FindPropertyRelative("patternName");
                            SerializedProperty patternTextureProp = patternProp.FindPropertyRelative("patternTexture");
                            SerializedProperty patternIconProp = patternProp.FindPropertyRelative("previewIcon");

                            string patternName = patternNameProp.stringValue;
                            string normPattern = NormalizeName(patternName);

                            bool isNone = normPattern == "none" || normPattern == "aucun" || normPattern == "aucune" || normPattern == "solid";
                            if (isNone)
                            {
                                if (patternTextureProp.objectReferenceValue != null)
                                {
                                    patternTextureProp.objectReferenceValue = null;
                                    isDirty = true;
                                }
                            }
                            else
                            {
                                Texture2D matchedTexture = FindMatchingTexture(maskTextures, elementName, patternName);
                                if (matchedTexture != null && patternTextureProp.objectReferenceValue != matchedTexture)
                                {
                                    patternTextureProp.objectReferenceValue = matchedTexture;
                                    isDirty = true;
                                    linkedCount++;
                                    if (logResults)
                                        Debug.Log($"<color=#ffcc00>[PonyDataLinker] Texture liée :</color> {categoryData.name} > {elementName} > <b>{patternName}</b> => <b>{matchedTexture.name}</b>");
                                }
                            }

                            Sprite matchedIcon = FindMatchingSprite(sprites, elementName, patternName);
                            if (matchedIcon != null && patternIconProp.objectReferenceValue != matchedIcon)
                            {
                                patternIconProp.objectReferenceValue = matchedIcon;
                                isDirty = true;
                                linkedCount++;
                                if (logResults)
                                    Debug.Log($"<color=#33ccff>[PonyDataLinker] Icône de pattern liée :</color> {categoryData.name} > {elementName} > <b>{patternName}</b> => <b>{matchedIcon.name}</b>");
                            }
                        }
                    }
                }
            }

            if (isDirty)
            {
                serializedData.ApplyModifiedProperties();
                EditorUtility.SetDirty(categoryData);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (logResults)
            Debug.Log($"<color=#33ccff><b>[PonyDataLinker] Liaison terminée !</b></color> {linkedCount} références assignées.");

        return linkedCount;
    }

    private static void LinkShapesArray(
        SerializedProperty shapesProp,
        Swapper.SlotType slot,
        List<GameObject> prefabs,
        Dictionary<string, Sprite> sprites,
        string categoryName,
        string elementLabel,
        bool logResults,
        ref bool isDirty,
        ref int linkedCount)
    {
        if (shapesProp == null || !shapesProp.isArray) return;

        for (int s = 0; s < shapesProp.arraySize; s++)
        {
            SerializedProperty shapeProp = shapesProp.GetArrayElementAtIndex(s);
            SerializedProperty shapeNameProp = shapeProp.FindPropertyRelative("shapeName");
            SerializedProperty meshPrefabProp = shapeProp.FindPropertyRelative("meshPrefab");
            SerializedProperty previewIconProp = shapeProp.FindPropertyRelative("previewIcon");

            string shapeName = shapeNameProp.stringValue;
            string normShape = NormalizeName(shapeName);

            bool isNone = normShape == "none" || normShape == "aucune" || normShape == "aucun" || string.IsNullOrEmpty(normShape);
            if (isNone)
            {
                if (meshPrefabProp.objectReferenceValue != null)
                {
                    meshPrefabProp.objectReferenceValue = null;
                    isDirty = true;
                }
            }
            else
            {
                GameObject matchedPrefab = FindMatchingPrefab(prefabs, slot, shapeName);
                if (matchedPrefab != null && meshPrefabProp.objectReferenceValue != matchedPrefab)
                {
                    meshPrefabProp.objectReferenceValue = matchedPrefab;
                    isDirty = true;
                    linkedCount++;
                    if (logResults)
                        Debug.Log($"<color=#5cd65c>[PonyDataLinker] Mesh lié :</color> {categoryName} > {elementLabel} > <b>{shapeName}</b> => <b>{matchedPrefab.name}</b>");
                }
            }

            Sprite matchedIcon = FindMatchingSprite(sprites, slot.ToString(), shapeName);
            if (matchedIcon != null && previewIconProp.objectReferenceValue != matchedIcon)
            {
                previewIconProp.objectReferenceValue = matchedIcon;
                isDirty = true;
                linkedCount++;
                if (logResults)
                    Debug.Log($"<color=#33ccff>[PonyDataLinker] Icône liée :</color> {categoryName} > {elementLabel} > <b>{shapeName}</b> => <b>{matchedIcon.name}</b>");
            }
        }
    }

    private static GameObject FindMatchingPrefab(List<GameObject> prefabs, Swapper.SlotType slot, string shapeName)
    {
        string normShape = NormalizeName(shapeName);
        string slotStr = slot.ToString().ToLowerInvariant();
        string slotPrefix = slotStr == "wings" ? "wing" : slotStr;

        var synonyms = GetSynonyms(normShape);

        foreach (var prefab in prefabs)
        {
            string normPrefab = NormalizeName(prefab.name);
            bool matchesSlot = normPrefab.Contains(slotPrefix) || normPrefab.Contains(slotStr);

            if (!matchesSlot && slot == Swapper.SlotType.Forelock)
                matchesSlot = normPrefab.Contains("forelock") || normPrefab.Contains("toupet");

            if (!matchesSlot) continue;

            foreach (var syn in synonyms)
            {
                if (normPrefab.Contains(syn))
                    return prefab;
            }
        }

        return null;
    }

    private static Sprite FindMatchingSprite(Dictionary<string, Sprite> sprites, string categoryOrSlot, string itemName)
    {
        string normItem = NormalizeName(itemName);
        string normCat = NormalizeName(categoryOrSlot);
        var synonyms = GetSynonyms(normItem);

        List<string> allowedPrefixes = new List<string>();
        if (normCat.Contains("headpattern") || normCat.Contains("facemark") || (normCat.Contains("marque") && normCat.Contains("front")))
            allowedPrefixes.Add("headpattern");
        else if (normCat.Contains("leg") || normCat.Contains("balzane") || normCat.Contains("membre"))
        {
            allowedPrefixes.Add("legspaterns");
            allowedPrefixes.Add("legspattern");
            allowedPrefixes.Add("legs");
            allowedPrefixes.Add("hooves");
        }
        else if (normCat.Contains("head") || normCat.Contains("tete"))
            allowedPrefixes.Add("head");
        else if (normCat.Contains("bodypattern") || normCat.Contains("robe") || normCat.Contains("coat") || normCat.Contains("pattern"))
            allowedPrefixes.Add("bodypattern");
        else if (normCat.Contains("bodypattern") || normCat.Contains("robe") || normCat.Contains("coat") || normCat.Contains("pattern"))
            allowedPrefixes.Add("bodypattern");
        else if (normCat.Contains("body") || normCat.Contains("corps"))
            allowedPrefixes.Add("body");
        else if (normCat.Contains("hooves") || normCat.Contains("sabot"))
            allowedPrefixes.Add("hooves");
        else if (normCat.Contains("horn") || normCat.Contains("corne"))
            allowedPrefixes.Add("horn");
        else if (normCat.Contains("wing") || normCat.Contains("aile"))
            allowedPrefixes.Add("wings");
        else if (normCat.Contains("forelock") || normCat.Contains("toupet"))
        {
            allowedPrefixes.Add("forelock");
            allowedPrefixes.Add("mane");
        }
        else if (normCat.Contains("mane") || normCat.Contains("criniere"))
        {
            allowedPrefixes.Add("mane");
            allowedPrefixes.Add("forelock");
        }
        else if (normCat.Contains("tail") || normCat.Contains("queue"))
            allowedPrefixes.Add("tail");
        else if (normCat.Contains("colorrepartition") || normCat.Contains("repartition") || normCat.Contains("nuance"))
            allowedPrefixes.Add("colorrepartition");
        else
        {
            return null;
        }

        // 1. Clés directes avec les préfixes autorisés
        foreach (var syn in synonyms)
        {
            string exactKey = normCat + syn;
            if (sprites.TryGetValue(exactKey, out Sprite s)) return s;

            foreach (var p in allowedPrefixes)
            {
                if (sprites.TryGetValue(p + syn, out Sprite sp)) return sp;
            }
        }

        // 2. Recherche par suffixe uniquement parmi les sprites avec un préfixe autorisé
        foreach (var kvp in sprites)
        {
            bool hasAllowedPrefix = false;
            foreach (var p in allowedPrefixes)
            {
                if (kvp.Key.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    hasAllowedPrefix = true;
                    break;
                }
            }

            if (!hasAllowedPrefix) continue;

            // Évite que "head" matche "headpattern..." et "body" matche "bodypattern..."
            if (allowedPrefixes.Contains("head") && kvp.Key.StartsWith("headpattern", StringComparison.OrdinalIgnoreCase))
                continue;
            if (allowedPrefixes.Contains("body") && kvp.Key.StartsWith("bodypattern", StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var syn in synonyms)
            {
                if (kvp.Key.EndsWith(syn, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
        }

        return null;
    }

    private static Texture2D FindMatchingTexture(List<Texture2D> textures, string elementName, string patternName)
    {
        string normPattern = NormalizeName(patternName);
        var synonyms = GetSynonyms(normPattern);

        foreach (var tex in textures)
        {
            string normTex = NormalizeName(tex.name);
            foreach (var syn in synonyms)
            {
                if (normTex.Contains(syn))
                    return tex;
            }
        }

        return null;
    }

    private static List<string> GetSynonyms(string normalized)
    {
        var list = new List<string> { normalized };

        switch (normalized)
        {
            // Horns
            case "straight": list.AddRange(new[] { "droite", "straight" }); break;
            case "curved": list.AddRange(new[] { "courbee", "curved" }); break;
            case "goat": list.AddRange(new[] { "chevre", "goat" }); break;
            case "rhino": list.Add("rhino"); break;

            // Wings
            case "feathered": list.AddRange(new[] { "feathers", "plumes", "fanons" }); break;
            case "small": list.AddRange(new[] { "mini", "small" }); break;
            case "dragon": list.Add("dragon"); break;

            // Mane, Forelock & Tail
            case "brush": list.AddRange(new[] { "broom", "brosse" }); break;
            case "braided": list.AddRange(new[] { "braid", "tresse", "tresses" }); break;
            case "wild": list.AddRange(new[] { "long", "sauvage" }); break;
            case "classic": list.AddRange(new[] { "classique", "classiques" }); break;
            case "wavy": list.Add("wavy"); break;
            case "unicorn": list.AddRange(new[] { "licorne", "unicorn" }); break;
            case "mermaid": list.AddRange(new[] { "sirene", "mermaid" }); break;

            // Head Shapes
            case "roman": list.AddRange(new[] { "bombee", "roman", "draft" }); break;
            case "plate": list.AddRange(new[] { "straight", "plate", "base" }); break;
            case "dished": list.AddRange(new[] { "creuse", "dished", "slim" }); break;

            // Body
            case "draft": list.AddRange(new[] { "trait", "draft" }); break;
            case "arabian": list.AddRange(new[] { "arabe", "arabian", "slim" }); break;

            // Hooves
            case "cloven": list.AddRange(new[] { "fendus", "cloven" }); break;

            // Patterns & Nuances
            case "solid": list.AddRange(new[] { "solid", "uni" }); break;
            case "dappled": list.AddRange(new[] { "pommele", "dappled" }); break;
            case "pangare": list.AddRange(new[] { "pangare" }); break;
            case "buckskin": list.AddRange(new[] { "isabelle", "buckskin" }); break;
            case "caped": list.AddRange(new[] { "cape", "caped" }); break;
            case "minitobiano": list.Add("minitobiano"); break;
            case "tobiano": list.Add("tobiano"); break;
            case "appaloosa": list.Add("appaloosa"); break;

            // Legs
            case "coronet": list.AddRange(new[] { "couronne", "crown" }); break;
            case "pastern": list.AddRange(new[] { "petitebalzane", "smallbalzanne" }); break;
            case "sock": list.AddRange(new[] { "moyennebalzane", "mediumbalzanne" }); break;
            case "stocking": list.AddRange(new[] { "grandebalzane", "longbalzanne" }); break;

            // Face
            case "star": list.AddRange(new[] { "etoile", "star" }); break;
            case "snip": list.AddRange(new[] { "pelote", "pelotte", "snip" }); break;
            case "stripe": list.AddRange(new[] { "listefine", "stripe" }); break;
            case "blaze": list.AddRange(new[] { "listelarge", "blaze" }); break;
            case "starstripe": list.AddRange(new[] { "etoilelistee", "starliste" }); break;
        }

        return list;
    }
}
#endif