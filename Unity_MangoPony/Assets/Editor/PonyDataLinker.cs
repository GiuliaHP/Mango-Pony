#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class PonyDataLinker
{
    /// <summary>
    /// Normalise une chaîne de caractères (retire les accents, passe en minuscules, retire les espaces superflus).
    /// </summary>
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
        // Retire d'éventuels tirets ou underscores pour comparaison souple
        return result.Replace("-", "").Replace("_", "").Replace(" ", "");
    }

    [MenuItem("Mango Pony/Link Prefabs to Data Assets")]
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
            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        if (logResults)
            Debug.Log($"[PonyDataLinker] {prefabs.Count} prefabs trouvés dans 'Assets/Prefabs/Pony'.");

        // 2. Récupère tous les CustomizationCategoryData dans Assets/Scriptables
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

                    SerializedProperty shapesProp = elementProp.FindPropertyRelative("shapes");
                    if (shapesProp != null && shapesProp.isArray)
                    {
                        for (int s = 0; s < shapesProp.arraySize; s++)
                        {
                            SerializedProperty shapeProp = shapesProp.GetArrayElementAtIndex(s);
                            SerializedProperty shapeNameProp = shapeProp.FindPropertyRelative("shapeName");
                            SerializedProperty meshPrefabProp = shapeProp.FindPropertyRelative("meshPrefab");

                            string shapeName = shapeNameProp.stringValue;
                            string normShape = NormalizeName(shapeName);

                            // Cas "Aucune" / "Aucun" / vide : aucun prefab requis
                            if (normShape == "aucune" || normShape == "aucun" || normShape == "none" || string.IsNullOrEmpty(normShape))
                            {
                                if (meshPrefabProp.objectReferenceValue != null)
                                {
                                    meshPrefabProp.objectReferenceValue = null;
                                    isDirty = true;
                                }
                                continue;
                            }

                            // Recherche du Prefab correspondant
                            GameObject matchedPrefab = FindMatchingPrefab(prefabs, slot, shapeName);
                            if (matchedPrefab != null)
                            {
                                if (meshPrefabProp.objectReferenceValue != matchedPrefab)
                                {
                                    meshPrefabProp.objectReferenceValue = matchedPrefab;
                                    isDirty = true;
                                    linkedCount++;
                                    if (logResults)
                                    {
                                        Debug.Log($"<color=#5cd65c>[PonyDataLinker] Lié :</color> {categoryData.name} > {elementName} > <b>{shapeName}</b> => <b>{matchedPrefab.name}</b>");
                                    }
                                }
                            }
                            else if (logResults)
                            {
                                Debug.LogWarning($"[PonyDataLinker] Aucun prefab correspondant trouvé pour : {categoryData.name} > {elementName} > {shapeName} (Slot: {slot})");
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
            Debug.Log($"<color=#33ccff><b>[PonyDataLinker] Liaison terminée !</b></color> {linkedCount} prefabs assignés/mis à jour.");

        return linkedCount;
    }

    private static GameObject FindMatchingPrefab(List<GameObject> prefabs, Swapper.SlotType slot, string shapeName)
    {
        string normShape = NormalizeName(shapeName);
        string slotStr = slot.ToString().ToLowerInvariant(); // body, head, hooves, mane, tail, horn, wings

        // Variantes du nom de slot pour les préfixes de fichier (ex: wings -> wing)
        string slotPrefix = slotStr;
        if (slotStr == "wings") slotPrefix = "wing";

        foreach (var prefab in prefabs)
        {
            string normPrefab = NormalizeName(prefab.name); // ex: prefabhornchevre, prefabheadbombee

            // Vérifie que le prefab appartient bien au slot (ex: contient "head", "horn", "wing", etc.)
            bool matchesSlot = normPrefab.Contains(slotPrefix) || normPrefab.Contains(slotStr);
            if (!matchesSlot) continue;

            // Vérifie si le nom de forme est contenu dans le nom du prefab
            if (normPrefab.Contains(normShape))
            {
                return prefab;
            }

            // Gestion du pluriel / singulier (ex: "tresses" <-> "tresse", "classiques" <-> "classique")
            string singularShape = normShape.EndsWith("s") ? normShape.Substring(0, normShape.Length - 1) : normShape;
            if (normPrefab.Contains(singularShape))
            {
                return prefab;
            }

            string pluralShape = normShape + "s";
            if (normPrefab.Contains(pluralShape))
            {
                return prefab;
            }
        }

        // Deuxième passe plus tolérante si aucun slot n'a matché
        foreach (var prefab in prefabs)
        {
            string normPrefab = NormalizeName(prefab.name);
            if (normPrefab.Contains(normShape))
            {
                return prefab;
            }
        }

        return null;
    }
}
#endif
