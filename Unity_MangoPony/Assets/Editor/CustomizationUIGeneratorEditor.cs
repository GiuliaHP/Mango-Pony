#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[CustomEditor(typeof(CustomizationUIGenerator))]
public class CustomizationUIGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CustomizationUIGenerator generator = (CustomizationUIGenerator)target;

        EditorGUILayout.Space(15);
        GUI.backgroundColor = new Color(0.35f, 0.7f, 1f);
        if (GUILayout.Button("Lier les Prefabs aux ScriptableObjects (par nom)", GUILayout.Height(32)))
            PonyDataLinker.LinkAllPrefabsToDataAssets();

        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(0.4f, 0.85f, 0.55f);
        if (GUILayout.Button("Générer l'Interface UI (Hors Play Mode)", GUILayout.Height(40)))
            GenerateUI(generator);

        GUI.backgroundColor = new Color(0.95f, 0.45f, 0.45f);
        if (GUILayout.Button("Vider l'Interface UI générée", GUILayout.Height(28)))
            ClearUI(generator);

        GUI.backgroundColor = Color.white;
    }

    private void ClearUI(CustomizationUIGenerator gen)
    {
        if (gen.tabsContainer != null) ClearChildrenImmediate(gen.tabsContainer);
        if (gen.panelsContainer != null) ClearChildrenImmediate(gen.panelsContainer);
        EditorUtility.SetDirty(gen.gameObject);
    }

    private void ClearChildrenImmediate(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
    }

    private void GenerateUI(CustomizationUIGenerator gen)
    {
        if (gen.tabsContainer == null || gen.panelsContainer == null || gen.categoryPanelPrefab == null)
        {
            Debug.LogError("[Generator] Assigne tabsContainer, panelsContainer et categoryPanelPrefab !");
            return;
        }

        Undo.SetCurrentGroupName("Generate Customization UI");
        int undoGroup = Undo.GetCurrentGroup();
        ClearUI(gen);

        List<GameObject> createdPanels = new List<GameObject>();

        foreach (var category in gen.categories)
        {
            if (category == null) continue;

            // Onglet
            GameObject tabObj = InstantiatePrefabWithUndo(gen.tabButtonPrefab, gen.tabsContainer, $"Tab_{category.categoryName}");
            SetTMPText(tabObj, category.categoryName);

            // Panneau de catégorie
            GameObject panelObj = InstantiatePrefabWithUndo(gen.categoryPanelPrefab, gen.panelsContainer, $"Panel_{category.categoryName}");
            createdPanels.Add(panelObj);

            Transform contentRoot = FindChildByPathOrSelf(panelObj.transform, gen.panelContentPath);
            // Nettoie les éventuels éléments d'exemple déjà présents dans le prefab
            ClearChildrenImmediate(contentRoot);

            foreach (var element in category.elements)
            {
                // 1. TITLE (Nom de l'élément)
                GameObject titleObj = InstantiatePrefabWithUndo(gen.elementTitlePrefab, contentRoot, $"Title_{element.elementName}");
                SetTMPText(titleObj, element.elementName);

                // 2. SUBTITLE + GRID LAYOUT : SHAPES
                if (element.shapes != null && element.shapes.Count > 0)
                {
                    GameObject subShapes = InstantiatePrefabWithUndo(gen.subtitlePrefab, contentRoot, $"Subtitle_{element.shapesSubtitle}");
                    SetTMPText(subShapes, element.shapesSubtitle);

                    GameObject gridShapes = InstantiatePrefabWithUndo(gen.gridLayoutPrefab, contentRoot, $"Grid_{element.shapesSubtitle}");
                    Transform gridParent = gridShapes != null ? gridShapes.transform : contentRoot;

                    foreach (var shape in element.shapes)
                    {
                        CreateMeshButton(gen, gridParent, element, shape);
                    }
                }

                // 3. SUBTITLE + GRID LAYOUT : PATTERNS
                if (element.patterns != null && element.patterns.Count > 0)
                {
                    GameObject subPatterns = InstantiatePrefabWithUndo(gen.subtitlePrefab, contentRoot, $"Subtitle_{element.patternsSubtitle}");
                    SetTMPText(subPatterns, element.patternsSubtitle);

                    GameObject gridPatterns = InstantiatePrefabWithUndo(gen.gridLayoutPrefab, contentRoot, $"Grid_{element.patternsSubtitle}");
                    Transform gridParent = gridPatterns != null ? gridPatterns.transform : contentRoot;

                    foreach (var pattern in element.patterns)
                    {
                        CreatePatternButton(gen, gridParent, element, pattern);
                    }
                }

                // 4. SUBTITLE + LAYOUT_COLORS + COLOR PICKER
                var colSettings = element.colorSettings;
                if (colSettings != null && colSettings.hasColors && colSettings.colorChannels != null && colSettings.colorChannels.Count > 0)
                {
                    GameObject subColors = InstantiatePrefabWithUndo(gen.subtitlePrefab, contentRoot, $"Subtitle_{element.colorsSubtitle}");
                    SetTMPText(subColors, element.colorsSubtitle);

                    // Instancie ton Layout_Colors et adapte automatiquement le nombre de boutons de couleur
                    GameObject colorsLayoutObj = InstantiatePrefabWithUndo(gen.layoutColorsPrefab, contentRoot, $"Layout_Colors_{element.elementName}");
                    Transform colorsParent = colorsLayoutObj != null ? colorsLayoutObj.transform : contentRoot;

                    // Vide les boutons placeholders du prefab Layout_Colors pour générer le nombre exact requis (1, 2, 3...)
                    if (colorsLayoutObj != null)
                        ClearChildrenImmediate(colorsParent);

                    GameObject swatchPrefab = colSettings.colorButtonPrefab != null ? colSettings.colorButtonPrefab : gen.colorSwatchButtonPrefab;
                    GameObject wheelPrefab = colSettings.colorPickerPrefab != null ? colSettings.colorPickerPrefab : gen.colorWheelPrefab;

                    foreach (var channel in colSettings.colorChannels)
                    {
                        CreateColorSwatchButton(swatchPrefab, colorsParent, element.targetMaterialZone, channel);
                    }

                    // 5. COLOR PICKER placé juste après Layout_Colors (trouvé automatiquement par ColorSwatchButton)
                    if (wheelPrefab != null)
                    {
                        InstantiatePrefabWithUndo(wheelPrefab, contentRoot, $"ColorPicker_{element.elementName}");
                    }
                }
            }
        }

        if (gen.hideInactivePanels)
        {
            for (int i = 0; i < createdPanels.Count; i++)
                createdPanels[i].SetActive(i == 0);
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.SetDirty(gen.gameObject);
    }

    private void CreateMeshButton(CustomizationUIGenerator gen, Transform parent, CustomizationCategoryData.CustomizationElement element, CustomizationCategoryData.MeshShapeOption shape)
    {
        if (gen.meshPreviewButtonPrefab == null) return;
        GameObject btnObj = InstantiatePrefabWithUndo(gen.meshPreviewButtonPrefab, parent, $"Btn_Shape_{shape.shapeName}");
        ApplyButtonVisuals(btnObj, shape.shapeName, shape.previewIcon);

        MeshPreviewButton meshBtn = btnObj.GetComponent<MeshPreviewButton>();
        if (meshBtn != null)
        {
            SerializedObject so = new SerializedObject(meshBtn);
            so.FindProperty("slotType").enumValueIndex = (int)element.targetMeshSlot;
            so.FindProperty("meshPrefab").objectReferenceValue = shape.meshPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private void CreatePatternButton(CustomizationUIGenerator gen, Transform parent, CustomizationCategoryData.CustomizationElement element, CustomizationCategoryData.PatternOption pattern)
    {
        if (gen.patternPreviewButtonPrefab == null) return;
        GameObject btnObj = InstantiatePrefabWithUndo(gen.patternPreviewButtonPrefab, parent, $"Btn_Pattern_{pattern.patternName}");
        ApplyButtonVisuals(btnObj, pattern.patternName, pattern.previewIcon);

        PatternPreviewButton patternBtn = btnObj.GetComponent<PatternPreviewButton>();
        if (patternBtn != null)
        {
            SerializedObject so = new SerializedObject(patternBtn);
            so.FindProperty("targetZone").enumValueIndex = (int)element.targetMaterialZone;
            so.FindProperty("patternTexture").objectReferenceValue = pattern.patternTexture;
            so.FindProperty("applyToAllLegs").boolValue = element.supportsFourLegsSync;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private void CreateColorSwatchButton(GameObject prefab, Transform parent, Swapper.MaterialZone zone, CustomizationCategoryData.ColorChannel channel)
    {
        if (prefab == null) return;
        GameObject swatchObj = InstantiatePrefabWithUndo(prefab, parent, $"Button_Color_{channel.colorName}");
        ApplyButtonVisuals(swatchObj, channel.colorName, null);

        ColorSwatchButton swatchBtn = swatchObj.GetComponent<ColorSwatchButton>();
        if (swatchBtn != null)
        {
            SerializedObject so = new SerializedObject(swatchBtn);
            so.FindProperty("defaultColor").colorValue = channel.defaultColor;
            so.FindProperty("targetZone").enumValueIndex = (int)zone;
            so.FindProperty("shaderColorProperty").stringValue = channel.shaderPropertyName;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Met à jour l'Image swatchPreviewImage (ex: ton enfant "Color" dans ton prefab Button_Color)
            SerializedProperty previewImgProp = so.FindProperty("swatchPreviewImage");
            if (previewImgProp != null && previewImgProp.objectReferenceValue is Image previewImg)
                previewImg.color = channel.defaultColor;
        }
    }

    private void SetTMPText(GameObject obj, string text)
    {
        if (obj == null) return;
        TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null) tmp.text = text;
    }

    private void ApplyButtonVisuals(GameObject btnObj, string label, Sprite icon)
    {
        SetTMPText(btnObj, label);
        if (icon != null)
        {
            Image[] images = btnObj.GetComponentsInChildren<Image>(true);
            if (images.Length > 1) images[1].sprite = icon;
            else if (images.Length == 1) images[0].sprite = icon;
        }
    }

    private GameObject InstantiatePrefabWithUndo(GameObject prefab, Transform parent, string objectName)
    {
        if (prefab == null) return null;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = objectName;
        Undo.RegisterCreatedObjectUndo(instance, $"Create {objectName}");
        return instance;
    }

    private Transform FindChildByPathOrSelf(Transform root, string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return root;
        Transform found = root.Find(relativePath);
        return found != null ? found : root;
    }
}
#endif