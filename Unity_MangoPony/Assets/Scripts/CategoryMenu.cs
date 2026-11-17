using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CategoryMenu : MonoBehaviour
{
    public enum SmoothEase
    {
        OutSine,
        InOutSine,
        OutQuad,
        InOutQuad,
        OutCubic,
        InOutCubic
    }

    [Header("Data")]
    public List<CategoryData> categories = new List<CategoryData>();

    [Header("Containers")]
    public Transform tabsContainer;
    public Transform panelsContainer;
    [Tooltip("Stretch-right container whose width is animated")]
    [FormerlySerializedAs("categoryContainer")] [SerializeField] private RectTransform container;
    [Tooltip("Scroll rect that scrolls the active panel")]
    [FormerlySerializedAs("categoryScrollRect")] [SerializeField] private ScrollRect scrollRect;

    [Header("Layout Prefabs")]
    [FormerlySerializedAs("tabButtonPrefab")] public GameObject tabPrefab;
    [FormerlySerializedAs("categoryPanelPrefab")] public GameObject panelPrefab;
    [Tooltip("Path to the content inside the panel prefab (empty = root)")]
    public string panelContentPath = "Viewport/Content";
    [FormerlySerializedAs("legsSelectorPrefab")] public GameObject legSelectorPrefab;
    [FormerlySerializedAs("elementTitlePrefab")] public GameObject titlePrefab;
    public GameObject subtitlePrefab;
    [FormerlySerializedAs("gridLayoutPrefab")] public GameObject gridPrefab;
    [FormerlySerializedAs("layoutColorsPrefab")] public GameObject colorRowPrefab;

    [Header("Option Prefabs")]
    [FormerlySerializedAs("meshPreviewButtonPrefab")] public GameObject shapeButtonPrefab;
    [FormerlySerializedAs("patternPreviewButtonPrefab")] public GameObject patternButtonPrefab;
    [FormerlySerializedAs("colorSwatchButtonPrefab")] public GameObject swatchPrefab;
    [FormerlySerializedAs("colorWheelPrefab")] public GameObject wheelPrefab;

    [Header("Panel Animation")]
    [SerializeField] private float openWidth = 305f;
    [SerializeField] private float closeDuration = 0.2f;
    [SerializeField] private SmoothEase closeEase = SmoothEase.InOutCubic;
    [SerializeField] private float openDuration = 0.25f;
    [SerializeField] private SmoothEase openEase = SmoothEase.OutCubic;

    [Header("Tab Animation")]
    [Tooltip("Extra height of the active tab, in pixels")]
    [FormerlySerializedAs("activeTabHeightOffset")] [SerializeField] private float activeTabOffset = 10f;
    [FormerlySerializedAs("tabHeightDuration")] [SerializeField] private float tabDuration = 0.2f;
    [FormerlySerializedAs("tabHeightEase")] [SerializeField] private SmoothEase tabEase = SmoothEase.OutQuad;

    [Header("Options")]
    public bool hideInactivePanels = true;

    [FormerlySerializedAs("categoryPanels")] [SerializeField, HideInInspector] private List<GameObject> panels = new List<GameObject>();

    [Header("Camera")]
    [FormerlySerializedAs("cameraController")] [SerializeField] private CameraRig cameraRig;

    private int activeIndex = -1;
    private Sequence transition;
    private List<TabButton> tabs = new List<TabButton>();

    private void Start()
    {
        if (panels.Count == 0) return;

        ShowPanel(0);
        activeIndex = 0;

        if (cameraRig != null && categories.Count > 0 && categories[0] != null)
            cameraRig.Focus(categories[0].defaultCamera, immediate: true);
        if (container != null)
            SetContainerWidth(openWidth);

        RefreshTabs(immediate: true);
    }

    public void Register(TabButton tab)
    {
        if (tabs.Contains(tab)) return;

        tabs.Add(tab);

        if (activeIndex >= 0)
        {
            bool isSelected = tab.Index == activeIndex;
            tab.SetSelected(isSelected);
            tab.AnimateHeight(isSelected, activeTabOffset, tabDuration, ToEase(tabEase), immediate: true);
        }
    }

    public void Select(int index)
    {
        if (index == activeIndex || index < 0 || index >= panels.Count)
            return;

        if (transition != null && transition.IsActive())
            transition.Kill();

        activeIndex = index;
        RefreshTabs(immediate: false);

        if (container == null)
        {
            ShowPanel(index);
            return;
        }

        transition = DOTween.Sequence().SetUpdate(true);
        transition.Append(DOVirtual.Float(container.sizeDelta.x, 0f, closeDuration, SetContainerWidth).SetEase(ToEase(closeEase)));
        transition.AppendCallback(() => ShowPanel(index));
        transition.Append(DOVirtual.Float(0f, openWidth, openDuration, SetContainerWidth).SetEase(ToEase(openEase)));

        if (cameraRig != null && index < categories.Count && categories[index] != null)
            cameraRig.Focus(categories[index].defaultCamera);
    }

    private void SetContainerWidth(float width)
    {
        Vector2 size = container.sizeDelta;
        size.x = width;
        container.sizeDelta = size;
    }

    private void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i] == null) continue;

            bool isActive = i == index;
            panels[i].SetActive(isActive);

            if (isActive && scrollRect != null)
            {
                RectTransform panelRect = panels[i].GetComponent<RectTransform>();
                scrollRect.content = panelRect;

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }

    private void RefreshTabs(bool immediate)
    {
        Ease ease = ToEase(tabEase);

        foreach (var tab in tabs)
        {
            if (tab == null) continue;

            bool isSelected = tab.Index == activeIndex;
            tab.SetSelected(isSelected);
            tab.AnimateHeight(isSelected, activeTabOffset, tabDuration, ease, immediate);
        }
    }

    private static Ease ToEase(SmoothEase ease)
    {
        switch (ease)
        {
            case SmoothEase.OutSine:    return Ease.OutSine;
            case SmoothEase.InOutSine:  return Ease.InOutSine;
            case SmoothEase.OutQuad:    return Ease.OutQuad;
            case SmoothEase.InOutQuad:  return Ease.InOutQuad;
            case SmoothEase.OutCubic:   return Ease.OutCubic;
            case SmoothEase.InOutCubic: return Ease.InOutCubic;
            default:                    return Ease.OutQuad;
        }
    }

    private void OnDestroy()
    {
        transition?.Kill();
    }
}
