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
        if (GUILayout.Button("1. Lier les Prefabs, Icônes & Textures (par nom)", GUILayout.Height(32)))
        {
            PonyDataLinker.LinkAllPrefabsToDataAssets();
        }

        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(0.4f, 0.85f, 0.55f);
        if (GUILayout.Button("2. Générer l'Interface UI (Hors Play Mode)", GUILayout.Height(40)))
        {
            PonyDataLinker.LinkAllPrefabsToDataAssets(false); // Synchronise automatiquement avant de générer
            GenerateUI(generator);
        }

        GUI.backgroundColor = new Color(0.95f, 0.45f, 0.45f);
        if (GUILayout.Button("Vider l'Interface UI générée", GUILayout.Height(28)))
        {
            ClearUI(generator);
        }

        GUI.backgroundColor = Color.white;
    }

    private void ClearUI(CustomizationUIGenerator gen)
    {
        if (gen.tabsContainer != null)
            ClearChildrenImmediate(gen.tabsContainer);

        if (gen.panelsContainer != null)
            ClearChildrenImmediate(gen.panelsContainer);

        // Vide également la liste interne categoryPanels sur le Generator
        SerializedObject genSO = new SerializedObject(gen);
        SerializedProperty panelsProp = genSO.FindProperty("categoryPanels");
        if (panelsProp != null)
        {
            panelsProp.ClearArray();
            genSO.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(gen.gameObject);
    }

    private void ClearChildrenImmediate(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        }
    }

    private void GenerateUI(CustomizationUIGenerator gen)
    {
        if (gen.tabsContainer == null || gen.panelsContainer == null || gen.categoryPanelPrefab == null)
        {
            Debug.LogError("[CustomizationUIGenerator] Assigne tabsContainer, panelsContainer et categoryPanelPrefab dans l'Inspecteur !");
            return;
        }

        Undo.SetCurrentGroupName("Generate Customization UI");
        int undoGroup = Undo.GetCurrentGroup();

        ClearUI(gen);

        List<GameObject> createdPanels = new List<GameObject>();

        for (int catIndex = 0; catIndex < gen.categories.Count; catIndex++)
        {
            var category = gen.categories[catIndex];
            if (category == null) continue;

            // 1. Création de l'onglet (Tab)
            GameObject tabObj = InstantiatePrefabWithUndo(gen.tabButtonPrefab, gen.tabsContainer, $"Tab_{category.categoryName}");
            if (tabObj != null)
            {
                SetTMPText(tabObj, category.categoryName);
                if (category.categoryIcon != null)
                    SetButtonIcon(tabObj, category.categoryIcon);

                CategoryTabButton tabBtn = tabObj.GetComponent<CategoryTabButton>();
                if (tabBtn != null)
                {
                    SerializedObject btnSO = new SerializedObject(tabBtn);
                    btnSO.FindProperty("categoryIndex").intValue = createdPanels.Count;
                    btnSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // 2. Création du panneau de catégorie (Category Panel Prefab)
            GameObject panelObj = InstantiatePrefabWithUndo(gen.categoryPanelPrefab, gen.panelsContainer, $"Panel_{category.categoryName}");
            if (panelObj == null) continue;
            createdPanels.Add(panelObj);

            Transform contentRoot = FindChildByPathOrSelf(panelObj.transform, gen.panelContentPath);
            ClearChildrenImmediate(contentRoot);

            // 3. Génération des éléments de la catégorie
            foreach (var element in category.elements)
            {
                // A. TITLE (Nom de l'élément)
                GameObject titleObj = InstantiatePrefabWithUndo(gen.elementTitlePrefab, contentRoot, $"Title_{element.elementName}");
                SetTMPText(titleObj, element.elementName);

                // B. SUBTITLE + GRID LAYOUT : SHAPES (Meshes 3D)
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

                // C. SUBTITLE + GRID LAYOUT : PATTERNS (Textures / Masques Shader)
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

                // D. SUBTITLE + LAYOUT_COLORS + COLOR PICKER
                var colSettings = element.colorSettings;
                if (colSettings != null && colSettings.hasColors && colSettings.colorChannels != null && colSettings.colorChannels.Count > 0)
                {
                    GameObject subColors = InstantiatePrefabWithUndo(gen.subtitlePrefab, contentRoot, $"Subtitle_{element.colorsSubtitle}");
                    SetTMPText(subColors, element.colorsSubtitle);

                    GameObject colorsLayoutObj = InstantiatePrefabWithUndo(gen.layoutColorsPrefab, contentRoot, $"Layout_Colors_{element.elementName}");
                    Transform colorsParent = colorsLayoutObj != null ? colorsLayoutObj.transform : contentRoot;

                    if (colorsLayoutObj != null)
                        ClearChildrenImmediate(colorsParent);

                    GameObject swatchPrefab = colSettings.colorButtonPrefab != null ? colSettings.colorButtonPrefab : gen.colorSwatchButtonPrefab;
                    GameObject wheelPrefab = colSettings.colorPickerPrefab != null ? colSettings.colorPickerPrefab : gen.colorWheelPrefab;

                    foreach (var channel in colSettings.colorChannels)
                    {
                        CreateColorSwatchButton(swatchPrefab, colorsParent, element.targetMaterialZone, channel);
                    }

                    if (wheelPrefab != null)
                    {
                        InstantiatePrefabWithUndo(wheelPrefab, contentRoot, $"ColorPicker_{element.elementName}");
                    }
                }
            }
        }

        // 4. Remplissage automatique de la liste cachée categoryPanels sur CustomizationUIGenerator
        SerializedObject genSO = new SerializedObject(gen);
        SerializedProperty panelsProp = genSO.FindProperty("categoryPanels");
        if (panelsProp != null)
        {
            panelsProp.ClearArray();
            for (int i = 0; i < createdPanels.Count; i++)
            {
                panelsProp.InsertArrayElementAtIndex(i);
                panelsProp.GetArrayElementAtIndex(i).objectReferenceValue = createdPanels[i];
            }
            genSO.ApplyModifiedPropertiesWithoutUndo();
        }

        // 5. Recalcul immédiat des hauteurs (ContentSizeFitter) hors Play Mode
        Canvas.ForceUpdateCanvases();
        foreach (var panel in createdPanels)
        {
            if (panel != null)
            {
                Transform contentRoot = FindChildByPathOrSelf(panel.transform, gen.panelContentPath);
                if (contentRoot != null && contentRoot.TryGetComponent<RectTransform>(out var contentRect))
                    LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

                if (panel.TryGetComponent<RectTransform>(out var panelRect))
                    LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            }
        }

        // 6. Masquer les panneaux inactifs (seul le 1er reste actif)
        if (gen.hideInactivePanels)
        {
            for (int i = 0; i < createdPanels.Count; i++)
            {
                createdPanels[i].SetActive(i == 0);
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.SetDirty(gen.gameObject);
        if (!Application.isPlaying && gen.gameObject.scene.IsValid())
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gen.gameObject.scene);

        Debug.Log($"[CustomizationUIGenerator] UI générée avec succès ({createdPanels.Count} catégories).");
    }

    private void CreateMeshButton(
        CustomizationUIGenerator gen,
        Transform parent,
        CustomizationCategoryData.CustomizationElement element,
        CustomizationCategoryData.MeshShapeOption shape)
    {
        if (gen.meshPreviewButtonPrefab == null) return;

        GameObject btnObj = InstantiatePrefabWithUndo(gen.meshPreviewButtonPrefab, parent, $"Btn_Shape_{shape.shapeName}");
        ApplyButtonVisuals(btnObj, shape.shapeName, shape.previewIcon);

        MeshPreviewButton meshBtn = btnObj.GetComponent<MeshPreviewButton>();
        if (meshBtn != null)
        {
            meshBtn.Setup(element.targetMeshSlot, shape);
            EditorUtility.SetDirty(meshBtn);
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshBtn);
        }
    }

    private void CreatePatternButton(
        CustomizationUIGenerator gen,
        Transform parent,
        CustomizationCategoryData.CustomizationElement element,
        CustomizationCategoryData.PatternOption pattern)
    {
        if (gen.patternPreviewButtonPrefab == null) return;

        GameObject btnObj = InstantiatePrefabWithUndo(gen.patternPreviewButtonPrefab, parent, $"Btn_Pattern_{pattern.patternName}");
        ApplyButtonVisuals(btnObj, pattern.patternName, pattern.previewIcon);

        PatternPreviewButton patternBtn = btnObj.GetComponent<PatternPreviewButton>();
        if (patternBtn != null)
        {
            patternBtn.Setup(element.targetMaterialZone, pattern, element.supportsFourLegsSync);
            EditorUtility.SetDirty(patternBtn);
            PrefabUtility.RecordPrefabInstancePropertyModifications(patternBtn);
        }
    }

    private void CreateColorSwatchButton(
        GameObject prefab,
        Transform parent,
        Swapper.MaterialZone zone,
        CustomizationCategoryData.ColorChannel channel)
    {
        if (prefab == null) return;

        GameObject swatchObj = InstantiatePrefabWithUndo(prefab, parent, $"Button_Color_{channel.colorName}");
        ApplyButtonVisuals(swatchObj, channel.colorName, null);

        ColorSwatchButton swatchBtn = swatchObj.GetComponent<ColorSwatchButton>();
        if (swatchBtn != null)
        {
            swatchBtn.Setup(zone, channel);

            SerializedObject so = new SerializedObject(swatchBtn);
            SerializedProperty previewImgProp = so.FindProperty("swatchPreviewImage");
            if (previewImgProp != null && previewImgProp.objectReferenceValue is Image previewImg)
            {
                previewImg.color = channel.defaultColor;
                EditorUtility.SetDirty(previewImg);
                PrefabUtility.RecordPrefabInstancePropertyModifications(previewImg);
            }

            EditorUtility.SetDirty(swatchBtn);
            PrefabUtility.RecordPrefabInstancePropertyModifications(swatchBtn);
        }
    }

    private void SetTMPText(GameObject obj, string text)
    {
        if (obj == null) return;
        TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            Undo.RecordObject(tmp, "Set TMP Text");
            tmp.text = text;
            EditorUtility.SetDirty(tmp);
            PrefabUtility.RecordPrefabInstancePropertyModifications(tmp);
        }
    }

    private void SetButtonIcon(GameObject btnObj, Sprite icon)
    {
        if (btnObj == null) return;

        Transform previewTransform = btnObj.transform.Find("Preview");
        if (previewTransform == null)
        {
            foreach (Transform child in btnObj.GetComponentsInChildren<Transform>(true))
            {
                if (child != btnObj.transform && child.name.Equals("Preview", System.StringComparison.OrdinalIgnoreCase))
                {
                    previewTransform = child;
                    break;
                }
            }
        }

        Image previewImage = null;
        if (previewTransform != null)
        {
            previewImage = previewTransform.GetComponent<Image>();
        }
        else
        {
            Image[] images = btnObj.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.gameObject != btnObj &&
                    !img.gameObject.name.Equals("Outline", System.StringComparison.OrdinalIgnoreCase) &&
                    !img.gameObject.name.Equals("Color", System.StringComparison.OrdinalIgnoreCase))
                {
                    previewImage = img;
                    break;
                }
            }
        }

        if (previewImage != null)
        {
            Undo.RecordObject(previewImage, "Set Preview Sprite");
            previewImage.sprite = icon;
            previewImage.enabled = (icon != null);
            EditorUtility.SetDirty(previewImage);
            PrefabUtility.RecordPrefabInstancePropertyModifications(previewImage);
        }
    }

    private void ApplyButtonVisuals(GameObject btnObj, string label, Sprite icon)
    {
        SetTMPText(btnObj, label);
        SetButtonIcon(btnObj, icon);
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