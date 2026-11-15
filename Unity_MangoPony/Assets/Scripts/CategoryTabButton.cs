using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(RectTransform))]
public class CategoryTabButton : MonoBehaviour
{
    [SerializeField, HideInInspector] private int categoryIndex;

    [Tooltip("Optionnel : si coché, passe le bouton en non-interactable quand son onglet est actif")]
    [SerializeField] private bool disableInteractableWhenActive = false;

    private Button button;
    private RectTransform rectTransform;
    private CustomizationUIGenerator uiManager;
    private JuicyButton juicyButton;

    private float baseHeight;
    private bool baseHeightInitialized;
    private Tween heightTween;

    public int CategoryIndex => categoryIndex;

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        juicyButton = GetComponent<JuicyButton>();
        uiManager = FindFirstObjectByType<CustomizationUIGenerator>();

        CacheBaseHeight();
    }

    private void CacheBaseHeight()
    {
        if (!baseHeightInitialized && rectTransform != null)
        {
            baseHeight = rectTransform.sizeDelta.y;
            baseHeightInitialized = true;
        }
    }

    private void Start()
    {
        if (uiManager != null)
            uiManager.RegisterButton(this);
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnTabClicked);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnTabClicked);
    }

    public void Setup(int index, CustomizationUIGenerator manager)
    {
        categoryIndex = index;
        uiManager = manager;
    }

    private void OnTabClicked()
    {
        if (uiManager == null)
            uiManager = FindFirstObjectByType<CustomizationUIGenerator>();

        if (uiManager != null)
            uiManager.SelectCategory(categoryIndex);
    }

    public void SetSelectedState(bool isSelected)
    {
        if (disableInteractableWhenActive && button != null)
        {
            button.interactable = !isSelected;
            if (juicyButton != null)
                juicyButton.CheckInteractableState();
        }
    }

    /// <summary>
    /// Anime la hauteur du RectTransform de sa hauteur d'origine à +heightOffset (ou inversement).
    /// Piloté directement par CustomizationUIGenerator.
    /// </summary>
    public void AnimateHeight(bool isSelected, float heightOffset, float duration, Ease ease, bool immediate = false)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        CacheBaseHeight();

        float targetHeight = isSelected ? baseHeight + heightOffset : baseHeight;

        heightTween?.Kill();

        if (immediate || duration <= 0f)
        {
            SetHeight(targetHeight);
            return;
        }

        float startHeight = rectTransform.sizeDelta.y;
        heightTween = DOVirtual.Float(startHeight, targetHeight, duration, SetHeight)
                               .SetEase(ease)
                               .SetUpdate(true);
    }

    private void SetHeight(float height)
    {
        Vector2 size = rectTransform.sizeDelta;
        size.y = height;
        rectTransform.sizeDelta = size;

        // Permet au LayoutGroup parent (si présent) de suivre le mouvement en douceur
        if (rectTransform.parent is RectTransform parentRect)
            LayoutRebuilder.MarkLayoutForRebuild(parentRect);
    }

    private void OnDestroy()
    {
        heightTween?.Kill();
    }
}