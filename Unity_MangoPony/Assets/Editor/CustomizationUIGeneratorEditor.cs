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
        GUI.backgroundColor = new Color(0.4f, 0.85f, 0.55f);
        if (GUILayout.Button("Générer l'Interface UI (Hors Play Mode)", GUILayout.Height(40)))
        {
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

        EditorUtility.SetDirty(gen.gameObject);
        Debug.Log("[CustomizationUIGenerator] Interface nettoyée.");
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
        if (gen.tabsContainer == null || gen.panelsContainer == null)
        {
            Debug.LogError("[CustomizationUIGenerator] Assigne 'tabsContainer' et 'panelsContainer' avant de générer !");
            return;
        }

        Undo.SetCurrentGroupName("Generate Customization UI");
        int undoGroup = Undo.GetCurrentGroup();

        ClearUI(gen);

        List<GameObject> createdPanels = new List<GameObject>();

        for (int cIndex = 0; cIndex < gen.categories.Count; cIndex++)
        {
            CustomizationCategoryData category = gen.categories[cIndex];
            if (category == null) continue;

            // 1. Création de l'onglet
            GameObject tabObj = InstantiatePrefabWithUndo(gen.tabButtonPrefab, gen.tabsContainer, $"Tab_{category.categoryName}");
            if (tabObj != null)
            {
                TextMeshProUGUI tabText = tabObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tabText != null) tabText.text = category.categoryName;

                if (category.categoryIcon != null)
                {
                    Image[] images = tabObj.GetComponentsInChildren<Image>(true);
                    // Si un enfant porte une icône dédiée (souvent le 2e Image après le fond du bouton)
                    if (images.Length > 1) images[1].sprite = category.categoryIcon;
                }
            }

            // 2. Création du panneau de catégorie
            GameObject panelObj = InstantiatePrefabWithUndo(gen.categoryPanelPrefab, gen.panelsContainer, $"Panel_{category.categoryName}");
            if (panelObj == null) continue;
            createdPanels.Add(panelObj);

            Transform panelContent = FindChildByPathOrSelf(panelObj.transform, gen.panelContentPath);

            // 3. Génération de chaque élément de la catégorie
            foreach (var element in category.elements)
            {
                Transform sectionParent = panelContent;

                if (gen.elementSectionPrefab != null)
                {
                    GameObject sectionObj = InstantiatePrefabWithUndo(gen.elementSectionPrefab, panelContent, $"Section_{element.elementName}");
                    if (sectionObj != null)
                    {
                        TextMeshProUGUI sectionTitle = sectionObj.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (sectionTitle != null) sectionTitle.text = element.elementName;
                        sectionParent = sectionObj.transform;
                    }
                }

                // 3A. Génération des boutons de Shapes / Motifs
                if (element.modificationType != CustomizationCategoryData.ElementModificationType.ColorOnly && element.shapes != null)
                {
                    Transform shapesGrid = FindChildByPathOrSelf(sectionParent, gen.shapesGridPath);

                    foreach (var shape in element.shapes)
                    {
                        bool isMesh = element.modificationType == CustomizationCategoryData.ElementModificationType.MeshOnly ||
                                      (element.modificationType == CustomizationCategoryData.ElementModificationType.MeshAndMaterial && shape.patternTexture == null && shape.meshPrefab != null) ||
                                      (element.modificationType == CustomizationCategoryData.ElementModificationType.MeshAndMaterial && shape.meshPrefab == null && shape.patternTexture == null);

                        if (isMesh)
                        {
                            CreateMeshButton(gen, shapesGrid, element, shape);
                        }
                        else
                        {
                            CreatePatternButton(gen, shapesGrid, element, shape);
                        }
                    }
                }

                // 3B. Génération des boutons de Couleur + CustomColorWheel
                if (element.colorSettings != null && element.colorSettings.hasColors && element.colorSettings.colorChannels != null && element.colorSettings.colorChannels.Count > 0)
                {
                    Transform colorsParent = FindChildByPathOrSelf(sectionParent, gen.colorsContainerPath);

                    GameObject swatchPrefab = element.colorSettings.colorButtonPrefab != null
                        ? element.colorSettings.colorButtonPrefab
                        : gen.colorSwatchButtonPrefab;

                    GameObject wheelPrefab = element.colorSettings.colorPickerPrefab != null
                        ? element.colorSettings.colorPickerPrefab
                        : gen.colorWheelPrefab;

                    foreach (var channel in element.colorSettings.colorChannels)
                    {
                        CreateColorSwatchButton(swatchPrefab, colorsParent, element.targetMaterialZone, channel);
                    }

                    // On instancie la roue chromatique juste APRÈS les boutons swatch pour que FindNextColorWheelInHierarchy() la trouve automatiquement !
                    if (wheelPrefab != null)
                    {
                        InstantiatePrefabWithUndo(wheelPrefab, colorsParent, $"ColorWheel_{element.elementName}");
                    }
                }
            }
        }

        // Afficher uniquement le 1er panneau par défaut si souhaité
        if (gen.hideInactivePanels)
        {
            for (int i = 0; i < createdPanels.Count; i++)
            {
                createdPanels[i].SetActive(i == 0);
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.SetDirty(gen.gameObject);
        Debug.Log($"[CustomizationUIGenerator] Interface générée avec succès ({createdPanels.Count} catégories) !");
    }

    private void CreateMeshButton(CustomizationUIGenerator gen, Transform parent, CustomizationCategoryData.CustomizationElement element, CustomizationCategoryData.ShapeOption shape)
    {
        if (gen.meshPreviewButtonPrefab == null) return;

        GameObject btnObj = InstantiatePrefabWithUndo(gen.meshPreviewButtonPrefab, parent, $"Btn_Mesh_{shape.shapeName}");
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

    private void CreatePatternButton(CustomizationUIGenerator gen, Transform parent, CustomizationCategoryData.CustomizationElement element, CustomizationCategoryData.ShapeOption shape)
    {
        if (gen.patternPreviewButtonPrefab == null) return;

        GameObject btnObj = InstantiatePrefabWithUndo(gen.patternPreviewButtonPrefab, parent, $"Btn_Pattern_{shape.shapeName}");
        ApplyButtonVisuals(btnObj, shape.shapeName, shape.previewIcon);

        PatternPreviewButton patternBtn = btnObj.GetComponent<PatternPreviewButton>();
        if (patternBtn != null)
        {
            SerializedObject so = new SerializedObject(patternBtn);
            so.FindProperty("targetZone").enumValueIndex = (int)element.targetMaterialZone;
            so.FindProperty("patternTexture").objectReferenceValue = shape.patternTexture;
            so.FindProperty("applyToAllLegs").boolValue = element.supportsFourLegsSync;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private void CreateColorSwatchButton(GameObject prefab, Transform parent, Swapper.MaterialZone zone, CustomizationCategoryData.ColorChannel channel)
    {
        if (prefab == null) return;

        GameObject swatchObj = InstantiatePrefabWithUndo(prefab, parent, $"Swatch_{channel.colorName}");
        ApplyButtonVisuals(swatchObj, channel.colorName, null);

        ColorSwatchButton swatchBtn = swatchObj.GetComponent<ColorSwatchButton>();
        if (swatchBtn != null)
        {
            SerializedObject so = new SerializedObject(swatchBtn);
            so.FindProperty("defaultColor").colorValue = channel.defaultColor;
            so.FindProperty("targetZone").enumValueIndex = (int)zone;
            so.FindProperty("shaderColorProperty").stringValue = channel.shaderPropertyName;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Met à jour la couleur visible immédiatement dans l'éditeur
            Image img = swatchObj.GetComponent<Image>();
            if (img != null) img.color = channel.defaultColor;
        }
    }

    private void ApplyButtonVisuals(GameObject btnObj, string label, Sprite icon)
    {
        TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null) tmp.text = label;

        if (icon != null)
        {
            Image[] images = btnObj.GetComponentsInChildren<Image>(true);
            if (images.Length > 1)
                images[1].sprite = icon;
            else if (images.Length == 1)
                images[0].sprite = icon;
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