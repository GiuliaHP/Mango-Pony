using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class AssetLinker
{
    private const string PrefabsFolder = "Assets/Prefabs/Pony";
    private const string IconsPath = "Assets/Sprites/PreviewIcons.psd";
    private const string MasksFolder = "Assets/Textures/Masks";
    private const string DataFolder = "Assets/Scriptables";

    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        StringBuilder builder = new StringBuilder();
        foreach (char c in text.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        string result = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();
        return result.Replace("-", "").Replace("_", "").Replace(" ", "").Replace("&", "");
    }

    [MenuItem("Mango Pony/Link Prefabs, Icons & Textures")]
    public static int LinkAll()
    {
        return LinkAll(true);
    }

    public static int LinkAll(bool log)
    {
        List<GameObject> prefabs = LoadAssets<GameObject>("t:Prefab", PrefabsFolder);
        List<Texture2D> masks = LoadAssets<Texture2D>("t:Texture2D", MasksFolder);

        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(IconsPath))
        {
            if (asset is Sprite sprite)
            {
                sprites[sprite.name] = sprite;
                sprites[Normalize(sprite.name)] = sprite;
            }
        }

        if (log)
            Debug.Log($"[AssetLinker] Found {prefabs.Count} prefabs, {sprites.Count / 2} icons, {masks.Count} mask textures.");

        int linkedCount = 0;

        foreach (CategoryData data in LoadAssets<CategoryData>($"t:{nameof(CategoryData)}", DataFolder))
        {
            bool isDirty = false;
            SerializedObject dataObject = new SerializedObject(data);
            dataObject.Update();

            SerializedProperty elements = dataObject.FindProperty(nameof(CategoryData.elements));
            if (elements != null)
            {
                for (int e = 0; e < elements.arraySize; e++)
                {
                    SerializedProperty element = elements.GetArrayElementAtIndex(e);
                    SerializedProperty titleProperty = element.FindPropertyRelative(nameof(CategoryData.Element.title));
                    string title = titleProperty != null ? titleProperty.stringValue : $"Element_{e}";
                    var slot = (PonyCustomizer.Slot)element.FindPropertyRelative(nameof(CategoryData.Element.slot)).enumValueIndex;

                    LinkShapes(element.FindPropertyRelative(nameof(CategoryData.Element.shapes)), slot, prefabs, sprites, data.name, title, log, ref isDirty, ref linkedCount);

                    if (element.FindPropertyRelative(nameof(CategoryData.Element.hasSecondaryShapes)).boolValue)
                    {
                        var secondarySlot = (PonyCustomizer.Slot)element.FindPropertyRelative(nameof(CategoryData.Element.secondarySlot)).enumValueIndex;
                        string secondaryLabel = element.FindPropertyRelative(nameof(CategoryData.Element.secondaryShapesLabel)).stringValue;
                        string label = string.IsNullOrEmpty(secondaryLabel) ? $"{title} (Secondary)" : $"{title} ({secondaryLabel})";

                        LinkShapes(element.FindPropertyRelative(nameof(CategoryData.Element.secondaryShapes)), secondarySlot, prefabs, sprites, data.name, label, log, ref isDirty, ref linkedCount);
                    }

                    LinkPatterns(element.FindPropertyRelative(nameof(CategoryData.Element.patterns)), masks, sprites, data.name, title, log, ref isDirty, ref linkedCount);
                }
            }

            if (isDirty)
            {
                dataObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(data);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (log)
            Debug.Log($"<color=#33ccff><b>[AssetLinker] Done.</b></color> {linkedCount} references assigned.");

        return linkedCount;
    }

    private static List<T> LoadAssets<T>(string filter, string folder) where T : UnityEngine.Object
    {
        List<T> results = new List<T>();
        foreach (string guid in AssetDatabase.FindAssets(filter, new[] { folder }))
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null) results.Add(asset);
        }
        return results;
    }

    private static void LinkShapes(
        SerializedProperty shapes,
        PonyCustomizer.Slot slot,
        List<GameObject> prefabs,
        Dictionary<string, Sprite> sprites,
        string dataName,
        string elementLabel,
        bool log,
        ref bool isDirty,
        ref int linkedCount)
    {
        if (shapes == null || !shapes.isArray) return;

        for (int s = 0; s < shapes.arraySize; s++)
        {
            SerializedProperty shape = shapes.GetArrayElementAtIndex(s);
            SerializedProperty prefabProperty = shape.FindPropertyRelative(nameof(CategoryData.ShapeOption.prefab));
            SerializedProperty iconProperty = shape.FindPropertyRelative(nameof(CategoryData.ShapeOption.icon));
            string label = shape.FindPropertyRelative(nameof(CategoryData.ShapeOption.label)).stringValue;

            if (IsNone(label))
            {
                if (prefabProperty.objectReferenceValue != null)
                {
                    prefabProperty.objectReferenceValue = null;
                    isDirty = true;
                }
            }
            else
            {
                GameObject prefab = FindPrefab(prefabs, slot, label);
                if (prefab != null && prefabProperty.objectReferenceValue != prefab)
                {
                    prefabProperty.objectReferenceValue = prefab;
                    isDirty = true;
                    linkedCount++;
                    if (log)
                        Debug.Log($"<color=#5cd65c>[AssetLinker] Mesh:</color> {dataName} > {elementLabel} > <b>{label}</b> => <b>{prefab.name}</b>");
                }
            }

            Sprite icon = FindSprite(sprites, slot.ToString(), label);
            if (icon != null && iconProperty.objectReferenceValue != icon)
            {
                iconProperty.objectReferenceValue = icon;
                isDirty = true;
                linkedCount++;
                if (log)
                    Debug.Log($"<color=#33ccff>[AssetLinker] Icon:</color> {dataName} > {elementLabel} > <b>{label}</b> => <b>{icon.name}</b>");
            }
        }
    }

    private static void LinkPatterns(
        SerializedProperty patterns,
        List<Texture2D> masks,
        Dictionary<string, Sprite> sprites,
        string dataName,
        string elementTitle,
        bool log,
        ref bool isDirty,
        ref int linkedCount)
    {
        if (patterns == null || !patterns.isArray) return;

        for (int p = 0; p < patterns.arraySize; p++)
        {
            SerializedProperty pattern = patterns.GetArrayElementAtIndex(p);
            SerializedProperty textureProperty = pattern.FindPropertyRelative(nameof(CategoryData.PatternOption.texture));
            SerializedProperty iconProperty = pattern.FindPropertyRelative(nameof(CategoryData.PatternOption.icon));
            string label = pattern.FindPropertyRelative(nameof(CategoryData.PatternOption.label)).stringValue;

            if (IsNone(label) || Normalize(label) == "solid")
            {
                if (textureProperty.objectReferenceValue != null)
                {
                    textureProperty.objectReferenceValue = null;
                    isDirty = true;
                }
            }
            else
            {
                Texture2D texture = FindTexture(masks, label);
                if (texture != null && textureProperty.objectReferenceValue != texture)
                {
                    textureProperty.objectReferenceValue = texture;
                    isDirty = true;
                    linkedCount++;
                    if (log)
                        Debug.Log($"<color=#ffcc00>[AssetLinker] Texture:</color> {dataName} > {elementTitle} > <b>{label}</b> => <b>{texture.name}</b>");
                }
            }

            Sprite icon = FindSprite(sprites, elementTitle, label);
            if (icon != null && iconProperty.objectReferenceValue != icon)
            {
                iconProperty.objectReferenceValue = icon;
                isDirty = true;
                linkedCount++;
                if (log)
                    Debug.Log($"<color=#33ccff>[AssetLinker] Icon:</color> {dataName} > {elementTitle} > <b>{label}</b> => <b>{icon.name}</b>");
            }
        }
    }

    private static bool IsNone(string label)
    {
        string normalized = Normalize(label);
        return string.IsNullOrEmpty(normalized) || normalized == "none" || normalized == "aucun" || normalized == "aucune";
    }

    private static GameObject FindPrefab(List<GameObject> prefabs, PonyCustomizer.Slot slot, string label)
    {
        string slotName = slot.ToString().ToLowerInvariant();
        string slotPrefix = slotName == "wings" ? "wing" : slotName;
        var synonyms = GetSynonyms(Normalize(label));

        foreach (var prefab in prefabs)
        {
            string prefabName = Normalize(prefab.name);
            bool matchesSlot = prefabName.Contains(slotPrefix) || prefabName.Contains(slotName);

            if (!matchesSlot && slot == PonyCustomizer.Slot.Forelock)
                matchesSlot = prefabName.Contains("forelock") || prefabName.Contains("toupet");

            if (!matchesSlot) continue;

            foreach (var synonym in synonyms)
            {
                if (prefabName.Contains(synonym))
                    return prefab;
            }
        }

        return null;
    }

    private static Sprite FindSprite(Dictionary<string, Sprite> sprites, string category, string label)
    {
        string normalizedCategory = Normalize(category);
        var synonyms = GetSynonyms(Normalize(label));
        List<string> prefixes = GetIconPrefixes(normalizedCategory);
        if (prefixes == null) return null;

        foreach (var synonym in synonyms)
        {
            if (sprites.TryGetValue(normalizedCategory + synonym, out Sprite exact)) return exact;

            foreach (var prefix in prefixes)
            {
                if (sprites.TryGetValue(prefix + synonym, out Sprite prefixed)) return prefixed;
            }
        }

        foreach (var pair in sprites)
        {
            if (!prefixes.Exists(prefix => pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (prefixes.Contains("head") && pair.Key.StartsWith("headpattern", StringComparison.OrdinalIgnoreCase))
                continue;
            if (prefixes.Contains("body") && pair.Key.StartsWith("bodypattern", StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var synonym in synonyms)
            {
                if (pair.Key.EndsWith(synonym, StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }
        }

        return null;
    }

    private static List<string> GetIconPrefixes(string category)
    {
        if (category.Contains("headpattern") || category.Contains("facemark") || (category.Contains("marque") && category.Contains("front")))
            return new List<string> { "headpattern" };
        if (category.Contains("leg") || category.Contains("balzane") || category.Contains("membre"))
            return new List<string> { "legspaterns", "legspattern", "legs", "hooves" };
        if (category.Contains("head") || category.Contains("tete"))
            return new List<string> { "head" };
        if (category.Contains("bodypattern") || category.Contains("robe") || category.Contains("coat") || category.Contains("pattern"))
            return new List<string> { "bodypattern" };
        if (category.Contains("body") || category.Contains("corps"))
            return new List<string> { "body" };
        if (category.Contains("hooves") || category.Contains("sabot"))
            return new List<string> { "hooves" };
        if (category.Contains("horn") || category.Contains("corne"))
            return new List<string> { "horn" };
        if (category.Contains("wing") || category.Contains("aile"))
            return new List<string> { "wings" };
        if (category.Contains("forelock") || category.Contains("toupet"))
            return new List<string> { "forelock", "mane" };
        if (category.Contains("mane") || category.Contains("criniere"))
            return new List<string> { "mane", "forelock" };
        if (category.Contains("tail") || category.Contains("queue"))
            return new List<string> { "tail" };
        if (category.Contains("colorrepartition") || category.Contains("repartition") || category.Contains("nuance"))
            return new List<string> { "colorrepartition" };
        return null;
    }

    private static Texture2D FindTexture(List<Texture2D> textures, string label)
    {
        var synonyms = GetSynonyms(Normalize(label));

        Texture2D best = null;
        int bestLength = int.MaxValue;

        foreach (var texture in textures)
        {
            string textureName = Normalize(texture.name);
            foreach (var synonym in synonyms)
            {
                if (textureName.Contains(synonym))
                {
                    if (textureName.Length < bestLength)
                    {
                        best = texture;
                        bestLength = textureName.Length;
                    }
                    break;
                }
            }
        }

        return best;
    }

    private static List<string> GetSynonyms(string normalized)
    {
        var list = new List<string> { normalized };

        switch (normalized)
        {
            case "straight": list.AddRange(new[] { "droite", "straight" }); break;
            case "curved": list.AddRange(new[] { "courbee", "curved" }); break;
            case "goat": list.AddRange(new[] { "chevre", "goat" }); break;
            case "rhino": list.Add("rhino"); break;

            case "feathered": list.AddRange(new[] { "feathers", "plumes", "fanons" }); break;
            case "small": list.AddRange(new[] { "mini", "small" }); break;
            case "dragon": list.Add("dragon"); break;

            case "brush": list.AddRange(new[] { "broom", "brosse" }); break;
            case "braided": list.AddRange(new[] { "braid", "tresse", "tresses" }); break;
            case "wild": list.AddRange(new[] { "long", "sauvage" }); break;
            case "classic": list.AddRange(new[] { "classique", "classiques" }); break;
            case "wavy": list.Add("wavy"); break;
            case "unicorn": list.AddRange(new[] { "licorne", "unicorn" }); break;
            case "mermaid": list.AddRange(new[] { "sirene", "mermaid" }); break;

            case "roman": list.AddRange(new[] { "bombee", "roman", "draft" }); break;
            case "plate": list.AddRange(new[] { "straight", "plate", "base" }); break;
            case "dished": list.AddRange(new[] { "creuse", "dished", "slim" }); break;

            case "draft": list.AddRange(new[] { "trait", "draft" }); break;
            case "arabian": list.AddRange(new[] { "arabe", "arabian", "slim" }); break;

            case "cloven": list.AddRange(new[] { "fendus", "cloven" }); break;

            case "solid": list.AddRange(new[] { "solid", "uni" }); break;
            case "dappled": list.AddRange(new[] { "pommele", "dappled" }); break;
            case "pangare": list.Add("pangare"); break;
            case "buckskin": list.AddRange(new[] { "isabelle", "buckskin" }); break;
            case "caped": list.AddRange(new[] { "cape", "caped" }); break;
            case "minitobiano": list.Add("minitobiano"); break;
            case "tobiano": list.Add("tobiano"); break;
            case "appaloosa": list.Add("appaloosa"); break;

            case "coronet": list.AddRange(new[] { "couronne", "crown" }); break;
            case "pastern": list.AddRange(new[] { "petitebalzane", "smallbalzanne" }); break;
            case "sock": list.AddRange(new[] { "moyennebalzane", "mediumbalzanne" }); break;
            case "stocking": list.AddRange(new[] { "grandebalzane", "longbalzanne" }); break;

            case "star": list.AddRange(new[] { "etoile", "star" }); break;
            case "snip": list.AddRange(new[] { "pelote", "pelotte", "snip" }); break;
            case "stripe": list.AddRange(new[] { "listefine", "stripe" }); break;
            case "blaze": list.AddRange(new[] { "listelarge", "blaze" }); break;
            case "starstripe": list.AddRange(new[] { "etoilelistee", "starliste" }); break;
        }

        return list;
    }
}
