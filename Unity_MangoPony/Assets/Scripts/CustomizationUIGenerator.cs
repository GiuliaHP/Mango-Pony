using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CustomizationUIGenerator : MonoBehaviour
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

    [Header("1. Données des Catégories")]
    public List<CustomizationCategoryData> categories = new List<CustomizationCategoryData>();

    [Header("2. Conteneurs du Canvas")]
    public Transform tabsContainer;
    public Transform panelsContainer;

    [Tooltip("Le RectTransform du Category Container (en Stretch Right) dont seule la Width sera animée")]
    [SerializeField] private RectTransform categoryContainer;

    [Tooltip("Le ScrollRect global qui scrolle le panel de catégorie actif")]
    [SerializeField] private ScrollRect categoryScrollRect;

    [Header("3. Prefabs de Structure")]
    public GameObject tabButtonPrefab;
    [Tooltip("Ton Category Panel Prefab (ScrollView / VerticalLayout)")]
    public GameObject categoryPanelPrefab;
    [Tooltip("Chemin vers le Content dans le Category Panel Prefab (ex: 'Viewport/Content', laisser vide si racine)")]
    public string panelContentPath = "Viewport/Content";

    [Space(5)]
    [Tooltip("Prefab du Title (TextMeshProUGUI avec le nom de l'élément)")]
    public GameObject elementTitlePrefab;

    [Tooltip("Prefab du Subtitle (TextMeshProUGUI : 'Shapes', 'Patterns', 'Colors')")]
    public GameObject subtitlePrefab;

    [Tooltip("Prefab de grille (GridLayoutGroup) pour les boutons Shapes et Patterns")]
    public GameObject gridLayoutPrefab;

    [Tooltip("Ton prefab 'Layout_Colors' (contient déjà un layout pour les boutons de couleur)")]
    public GameObject layoutColorsPrefab;

    [Header("4. Prefabs de Boutons & Color Picker")]
    public GameObject meshPreviewButtonPrefab;
    public GameObject patternPreviewButtonPrefab;
    public GameObject colorSwatchButtonPrefab;
    public GameObject colorWheelPrefab;

    [Header("5. Animation du Category Container & Onglets (DOTween)")]
    [Tooltip("Largeur ouverte du Category Container")]
    [SerializeField] private float openWidth = 305f;
    [Tooltip("Durée du repli vers la droite (width : 305 -> 0)")]
    [SerializeField] private float closeDuration = 0.2f;
    [SerializeField] private SmoothEase closeEase = SmoothEase.InOutCubic;

    [Tooltip("Durée du dépli depuis la droite (width : 0 -> 305)")]
    [SerializeField] private float openDuration = 0.25f;
    [SerializeField] private SmoothEase openEase = SmoothEase.OutCubic;

    [Space(5)]
    [Tooltip("Décalage de hauteur en pixels ajouté au bouton d'onglet actif")]
    [SerializeField] private float activeTabHeightOffset = 10f;
    [Tooltip("Durée de la glissade de hauteur des boutons d'onglet")]
    [SerializeField] private float tabHeightDuration = 0.2f;
    [SerializeField] private SmoothEase tabHeightEase = SmoothEase.OutQuad;

    [Header("6. Options")]
    public bool hideInactivePanels = true;

    // Liste remplie automatiquement par l'Editor lors de la génération (cachée dans l'Inspector)
    [SerializeField, HideInInspector]
    private List<GameObject> categoryPanels = new List<GameObject>();

    private int currentActiveIndex = -1;
    private Sequence transitionSequence;
    private List<CategoryTabButton> registeredButtons = new List<CategoryTabButton>();

    private void Start()
    {
        if (categoryPanels.Count > 0)
        {
            SetPanelsVisibility(0);
            currentActiveIndex = 0;

            if (categoryContainer != null)
            {
                SetContainerWidth(openWidth);
            }

            UpdateButtonsVisualState(immediate: true);
        }
    }

    public void RegisterButton(CategoryTabButton btn)
    {
        if (!registeredButtons.Contains(btn))
        {
            registeredButtons.Add(btn);

            // Si Start() a déjà tourné avant l'enregistrement tardif d'un bouton, on synchronise son état
            if (currentActiveIndex >= 0)
            {
                bool isSelected = btn.CategoryIndex == currentActiveIndex;
                btn.SetSelectedState(isSelected);
                btn.AnimateHeight(isSelected, activeTabHeightOffset, tabHeightDuration, ToDOTweenEase(tabHeightEase), immediate: true);
            }
        }
    }

    public void SelectCategory(int index)
    {
        // Si on reclique sur l'onglet déjà actif, on ne fait rien
        if (index == currentActiveIndex)
            return;

        if (index < 0 || index >= categoryPanels.Count)
            return;

        if (transitionSequence != null && transitionSequence.IsActive())
            transitionSequence.Kill();

        currentActiveIndex = index;
        UpdateButtonsVisualState(immediate: false);

        if (categoryContainer == null)
        {
            SetPanelsVisibility(index);
            return;
        }

        float currentWidth = categoryContainer.sizeDelta.x;

        transitionSequence = DOTween.Sequence().SetUpdate(true);

        // 1. Fold vers la droite : width passe de sa valeur actuelle à 0
        transitionSequence.Append(
            DOVirtual.Float(currentWidth, 0f, closeDuration, SetContainerWidth)
                     .SetEase(ToDOTweenEase(closeEase))
        );

        // 2. Swap des panels quand la largeur est à 0
        transitionSequence.AppendCallback(() =>
        {
            SetPanelsVisibility(index);
        });

        // 3. Unfold depuis la droite : width passe de 0 à 305
        transitionSequence.Append(
            DOVirtual.Float(0f, openWidth, openDuration, SetContainerWidth)
                     .SetEase(ToDOTweenEase(openEase))
        );
    }

    /// <summary>
    /// Modifie uniquement la Width (sizeDelta.x) en Stretch Right sans toucher à Y ni aux anchors.
    /// </summary>
    private void SetContainerWidth(float width)
    {
        Vector2 size = categoryContainer.sizeDelta;
        size.x = width;
        categoryContainer.sizeDelta = size;
    }

    private void SetPanelsVisibility(int activeIndex)
    {
        for (int i = 0; i < categoryPanels.Count; i++)
        {
            if (categoryPanels[i] == null) continue;

            bool isActive = (i == activeIndex);
            categoryPanels[i].SetActive(isActive);

            if (isActive && categoryScrollRect != null)
            {
                RectTransform activePanelRect = categoryPanels[i].GetComponent<RectTransform>();
                categoryScrollRect.content = activePanelRect;

                // Force le recalcul du ContentSizeFitter avant de remettre le scroll en haut
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(activePanelRect);
                categoryScrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }

    private void UpdateButtonsVisualState(bool immediate)
    {
        Ease ease = ToDOTweenEase(tabHeightEase);

        foreach (var btn in registeredButtons)
        {
            if (btn == null) continue;

            bool isSelected = btn.CategoryIndex == currentActiveIndex;
            btn.SetSelectedState(isSelected);
            btn.AnimateHeight(isSelected, activeTabHeightOffset, tabHeightDuration, ease, immediate);
        }
    }

    private static Ease ToDOTweenEase(SmoothEase smoothEase)
    {
        switch (smoothEase)
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
        transitionSequence?.Kill();
    }
}