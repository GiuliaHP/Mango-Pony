using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(CategoryMenu))]
public class CategoryMenuEditor : Editor
{
    private const string PanelsProperty = "panels";
    private const string SwatchImageProperty = "swatchImage";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CategoryMenu menu = (CategoryMenu)target;

        EditorGUILayout.Space(15);
        GUI.backgroundColor = new Color(0.35f, 0.7f, 1f);
        if (GUILayout.Button("1. Link Prefabs, Icons & Textures (by name)", GUILayout.Height(32)))
            AssetLinker.LinkAll();

        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(0.4f, 0.85f, 0.55f);
        if (GUILayout.Button("2. Generate UI (outside Play Mode)", GUILayout.Height(40)))
        {
            AssetLinker.LinkAll(false);
            Generate(menu);
        }

        GUI.backgroundColor = new Color(0.95f, 0.45f, 0.45f);
        if (GUILayout.Button("Clear Generated UI", GUILayout.Height(28)))
            Clear(menu);

        GUI.backgroundColor = Color.white;
    }

    private void Clear(CategoryMenu menu)
    {
        if (menu.tabsContainer != null) DestroyChildren(menu.tabsContainer);
        if (menu.panelsContainer != null) DestroyChildren(menu.panelsContainer);

        SerializedObject menuObject = new SerializedObject(menu);
        SerializedProperty panels = menuObject.FindProperty(PanelsProperty);
        if (panels != null)
        {
            panels.ClearArray();
            menuObject.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(menu.gameObject);
    }

    private void DestroyChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
    }

    private void Generate(CategoryMenu menu)
    {
        if (menu.tabsContainer == null || menu.panelsContainer == null || menu.panelPrefab == null)
        {
            Debug.LogError("[CategoryMenu] Assign tabsContainer, panelsContainer and panelPrefab.");
            return;
        }

        Undo.SetCurrentGroupName("Generate Category Menu");
        int undoGroup = Undo.GetCurrentGroup();

        var rig = FindAnyObjectByType<CameraRig>();
        var validIds = new HashSet<string>();
        if (rig != null)
            validIds.UnionWith(rig.DetailIds);

        Clear(menu);
        List<GameObject> createdPanels = new List<GameObject>();

        foreach (var category in menu.categories)
        {
            if (category == null) continue;

            if (rig != null)
            {
                WarnIfUnknown(validIds, category.defaultCamera, $"{category.title} (default camera)");
                foreach (var element in category.elements)
                    WarnIfUnknown(validIds, element.camera, $"{category.title}/{element.title}");
            }

            GameObject tab = Spawn(menu.tabPrefab, menu.tabsContainer, $"Tab_{category.title}");
            if (tab != null)
            {
                SetText(tab, category.title);
                if (category.icon != null) SetIcon(tab, category.icon);

                if (tab.TryGetComponent<TabButton>(out var tabButton))
                {
                    tabButton.Setup(createdPanels.Count);
                    EditorUtility.SetDirty(tabButton);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(tabButton);
                }
            }

            GameObject panel = Spawn(menu.panelPrefab, menu.panelsContainer, $"Panel_{category.title}");
            if (panel == null) continue;
            createdPanels.Add(panel);

            Transform content = FindChildOrSelf(panel.transform, menu.panelContentPath);
            DestroyChildren(content);

            foreach (var element in category.elements)
                BuildElement(menu, content, element);
        }

        SerializedObject menuObject = new SerializedObject(menu);
        SerializedProperty panels = menuObject.FindProperty(PanelsProperty);
        if (panels != null)
        {
            panels.ClearArray();
            for (int i = 0; i < createdPanels.Count; i++)
            {
                panels.InsertArrayElementAtIndex(i);
                panels.GetArrayElementAtIndex(i).objectReferenceValue = createdPanels[i];
            }
            menuObject.ApplyModifiedPropertiesWithoutUndo();
        }

        Canvas.ForceUpdateCanvases();
        foreach (var panel in createdPanels)
        {
            if (panel == null) continue;
            Transform content = FindChildOrSelf(panel.transform, menu.panelContentPath);
            if (content != null && content.TryGetComponent<RectTransform>(out var contentRect))
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            if (panel.TryGetComponent<RectTransform>(out var panelRect))
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }

        if (menu.hideInactivePanels)
        {
            for (int i = 0; i < createdPanels.Count; i++)
                createdPanels[i].SetActive(i == 0);
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.SetDirty(menu.gameObject);
        if (!Application.isPlaying && menu.gameObject.scene.IsValid())
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);

        Debug.Log($"[CategoryMenu] UI generated ({createdPanels.Count} categories).");
    }

    private static void WarnIfUnknown(HashSet<string> validIds, string id, string context)
    {
        if (!string.IsNullOrEmpty(id) && !validIds.Contains(id))
            Debug.LogWarning($"[CategoryMenu] Camera id '{id}' ({context}) not found in the scene's CameraRig.");
    }

    private void BuildElement(CategoryMenu menu, Transform content, CategoryData.Element element)
    {
        GameObject title = Spawn(menu.titlePrefab, content, $"Title_{element.title}");
        SetText(title, element.title);

        bool hasMesh = element.contentType != CategoryData.ContentType.ShaderOnly;
        bool hasPattern = element.contentType != CategoryData.ContentType.MeshOnly;

        if (hasMesh && element.shapes != null && element.shapes.Count > 0)
        {
            Transform grid = SpawnSection(menu, content, element.shapesLabel);
            bool linkSecondary = element.hasSecondaryShapes && element.secondaryShapes != null && element.secondaryShapes.Count > 0;
            foreach (var shape in element.shapes)
            {
                CategoryData.ShapeOption linked = linkSecondary ? FindMatchingShape(element.secondaryShapes, shape.label) : null;
                CreateShapeButton(menu, grid, element.slot, shape, element.camera, linked, element.secondarySlot);
            }
        }

        if (hasMesh && element.hasSecondaryShapes && element.secondaryShapes != null && element.secondaryShapes.Count > 0)
        {
            Transform grid = SpawnSection(menu, content, element.secondaryShapesLabel);
            foreach (var shape in element.secondaryShapes)
                CreateShapeButton(menu, grid, element.secondarySlot, shape, element.camera);
        }

        LegSelector legSelector = null;
        if (element.contentType == CategoryData.ContentType.ShaderOnly && element.isLegs && menu.legSelectorPrefab != null)
        {
            GameObject selector = Spawn(menu.legSelectorPrefab, content, "LegSelector");
            if (selector != null)
                legSelector = selector.GetComponent<LegSelector>();
        }

        if (hasPattern && element.patterns != null && element.patterns.Count > 0)
        {
            Transform grid = SpawnSection(menu, content, element.patternsLabel);
            foreach (var pattern in element.patterns)
                CreatePatternButton(menu, grid, element, pattern);
        }

        var colors = element.colors;
        if (colors == null || !colors.enabled || colors.channels == null || colors.channels.Count == 0)
            return;

        GameObject subtitle = Spawn(menu.subtitlePrefab, content, $"Subtitle_{element.colorsLabel}");
        SetText(subtitle, element.colorsLabel);

        GameObject colorRow = Spawn(menu.colorRowPrefab, content, $"ColorRow_{element.title}");
        Transform swatchParent = colorRow != null ? colorRow.transform : content;
        if (colorRow != null) DestroyChildren(swatchParent);

        GameObject swatchPrefab = colors.swatchPrefab != null ? colors.swatchPrefab : menu.swatchPrefab;
        GameObject wheelPrefab = colors.wheelPrefab != null ? colors.wheelPrefab : menu.wheelPrefab;

        OptionButton firstSwatch = null;
        var swatches = new List<OptionButton>();
        foreach (var channel in colors.channels)
        {
            var swatch = CreateSwatch(swatchPrefab, swatchParent, element.zone, channel, element.isLegs, element.camera);
            if (swatch != null) swatches.Add(swatch);
            if (firstSwatch == null) firstSwatch = swatch;
        }

        ColorWheel wheel = null;
        if (wheelPrefab != null)
        {
            GameObject wheelObject = Spawn(wheelPrefab, content, $"ColorWheel_{element.title}");
            if (wheelObject != null) wheel = wheelObject.GetComponent<ColorWheel>();
        }

        if (wheel != null)
        {
            wheel.Bind(swatchParent);
            EditorUtility.SetDirty(wheel);
            PrefabUtility.RecordPrefabInstancePropertyModifications(wheel);

            foreach (var swatch in swatches)
            {
                swatch.SetWheel(wheel);
                EditorUtility.SetDirty(swatch);
                PrefabUtility.RecordPrefabInstancePropertyModifications(swatch);
            }
        }

        if (legSelector != null && firstSwatch != null)
        {
            legSelector.BindColor(firstSwatch, wheel);
            EditorUtility.SetDirty(legSelector);
            PrefabUtility.RecordPrefabInstancePropertyModifications(legSelector);
        }
    }

    private Transform SpawnSection(CategoryMenu menu, Transform content, string label)
    {
        GameObject subtitle = Spawn(menu.subtitlePrefab, content, $"Subtitle_{label}");
        SetText(subtitle, label);

        GameObject grid = Spawn(menu.gridPrefab, content, $"Grid_{label}");
        return grid != null ? grid.transform : content;
    }

    private void CreateShapeButton(CategoryMenu menu, Transform parent, PonyCustomizer.Slot slot, CategoryData.ShapeOption shape, string camera, CategoryData.ShapeOption linked = null, PonyCustomizer.Slot linkedSlot = default)
    {
        if (menu.shapeButtonPrefab == null) return;
        GameObject buttonObject = Spawn(menu.shapeButtonPrefab, parent, $"Shape_{shape.label}");
        SetLabelAndIcon(buttonObject, shape.label, shape.icon);

        if (buttonObject.TryGetComponent<OptionButton>(out var option))
        {
            option.SetupShape(slot, shape, camera);
            if (linked != null)
                option.SetLinkedShape(linkedSlot, linked);
            EditorUtility.SetDirty(option);
            PrefabUtility.RecordPrefabInstancePropertyModifications(option);
        }
    }

    private static CategoryData.ShapeOption FindMatchingShape(List<CategoryData.ShapeOption> options, string label)
    {
        string normalized = AssetLinker.Normalize(label);
        foreach (var option in options)
        {
            if (AssetLinker.Normalize(option.label) == normalized)
                return option;
        }
        return null;
    }

    private void CreatePatternButton(CategoryMenu menu, Transform parent, CategoryData.Element element, CategoryData.PatternOption pattern)
    {
        if (menu.patternButtonPrefab == null) return;
        GameObject buttonObject = Spawn(menu.patternButtonPrefab, parent, $"Pattern_{pattern.label}");
        SetLabelAndIcon(buttonObject, pattern.label, pattern.icon);

        if (buttonObject.TryGetComponent<OptionButton>(out var option))
        {
            option.SetupPattern(element.zone, pattern, element.isLegs, element.camera);
            EditorUtility.SetDirty(option);
            PrefabUtility.RecordPrefabInstancePropertyModifications(option);
        }
    }

    private OptionButton CreateSwatch(GameObject prefab, Transform parent, PonyCustomizer.Zone zone, CategoryData.ColorChannel channel, bool isLegs, string camera)
    {
        if (prefab == null) return null;
        GameObject swatchObject = Spawn(prefab, parent, $"Swatch_{channel.label}");
        SetLabelAndIcon(swatchObject, channel.label, null);

        if (!swatchObject.TryGetComponent<OptionButton>(out var swatch))
            return null;

        swatch.SetupSwatch(zone, channel, isLegs, camera);

        SerializedProperty imageProperty = new SerializedObject(swatch).FindProperty(SwatchImageProperty);
        if (imageProperty != null && imageProperty.objectReferenceValue is Image image)
        {
            image.color = channel.defaultColor;
            EditorUtility.SetDirty(image);
            PrefabUtility.RecordPrefabInstancePropertyModifications(image);
        }

        EditorUtility.SetDirty(swatch);
        PrefabUtility.RecordPrefabInstancePropertyModifications(swatch);
        return swatch;
    }

    private void SetText(GameObject target, string text)
    {
        if (target == null) return;
        TextMeshProUGUI label = target.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null) return;

        Undo.RecordObject(label, "Set Text");
        label.text = text;
        EditorUtility.SetDirty(label);
        PrefabUtility.RecordPrefabInstancePropertyModifications(label);
    }

    private void SetIcon(GameObject buttonObject, Sprite icon)
    {
        if (buttonObject == null) return;

        Transform preview = buttonObject.transform.Find("Preview");
        if (preview == null)
        {
            foreach (Transform child in buttonObject.GetComponentsInChildren<Transform>(true))
            {
                if (child != buttonObject.transform && child.name.Equals("Preview", System.StringComparison.OrdinalIgnoreCase))
                {
                    preview = child;
                    break;
                }
            }
        }

        Image image = preview != null ? preview.GetComponent<Image>() : null;
        if (image == null)
        {
            foreach (Image candidate in buttonObject.GetComponentsInChildren<Image>(true))
            {
                if (candidate.gameObject != buttonObject &&
                    !candidate.gameObject.name.Equals("Outline", System.StringComparison.OrdinalIgnoreCase) &&
                    !candidate.gameObject.name.Equals("Color", System.StringComparison.OrdinalIgnoreCase))
                {
                    image = candidate;
                    break;
                }
            }
        }

        if (image == null) return;

        Undo.RecordObject(image, "Set Icon");
        image.sprite = icon;
        image.enabled = icon != null;
        EditorUtility.SetDirty(image);
        PrefabUtility.RecordPrefabInstancePropertyModifications(image);
    }

    private void SetLabelAndIcon(GameObject buttonObject, string label, Sprite icon)
    {
        SetText(buttonObject, label);
        SetIcon(buttonObject, icon);
    }

    private GameObject Spawn(GameObject prefab, Transform parent, string objectName)
    {
        if (prefab == null) return null;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = objectName;
        Undo.RegisterCreatedObjectUndo(instance, $"Create {objectName}");
        return instance;
    }

    private Transform FindChildOrSelf(Transform root, string path)
    {
        if (string.IsNullOrEmpty(path)) return root;
        Transform found = root.Find(path);
        return found != null ? found : root;
    }
}
