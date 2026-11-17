using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(RectTransform))]
public class TabButton : MonoBehaviour
{
    [FormerlySerializedAs("categoryIndex")] [SerializeField, HideInInspector] private int index;

    [Tooltip("Makes the tab non-interactable while it is active")]
    [FormerlySerializedAs("disableInteractableWhenActive")] [SerializeField] private bool disableWhenActive = false;

    private Button button;
    private RectTransform rectTransform;
    private CategoryMenu menu;
    private ButtonAnimator buttonAnimator;
    private float baseHeight;
    private bool hasBaseHeight;
    private Tween heightTween;

    public int Index => index;

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        buttonAnimator = GetComponent<ButtonAnimator>();
        menu = FindAnyObjectByType<CategoryMenu>();

        CacheBaseHeight();
    }

    private void Start()
    {
        if (menu != null)
            menu.Register(this);
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    public void Setup(int newIndex)
    {
        index = newIndex;
    }

    private void CacheBaseHeight()
    {
        if (!hasBaseHeight && rectTransform != null)
        {
            baseHeight = rectTransform.sizeDelta.y;
            hasBaseHeight = true;
        }
    }

    private void OnClick()
    {
        if (menu == null)
            menu = FindAnyObjectByType<CategoryMenu>();

        if (menu != null)
            menu.Select(index);
    }

    public void SetSelected(bool isSelected)
    {
        if (disableWhenActive && button != null)
        {
            button.interactable = !isSelected;
            if (buttonAnimator != null)
                buttonAnimator.RefreshInteractable();
        }
    }

    public void AnimateHeight(bool isSelected, float offset, float duration, Ease ease, bool immediate = false)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        CacheBaseHeight();

        float targetHeight = isSelected ? baseHeight + offset : baseHeight;

        heightTween?.Kill();

        if (immediate || duration <= 0f)
        {
            SetHeight(targetHeight);
            return;
        }

        heightTween = DOVirtual.Float(rectTransform.sizeDelta.y, targetHeight, duration, SetHeight)
                               .SetEase(ease)
                               .SetUpdate(true);
    }

    private void SetHeight(float height)
    {
        Vector2 size = rectTransform.sizeDelta;
        size.y = height;
        rectTransform.sizeDelta = size;

        if (rectTransform.parent is RectTransform parent)
            LayoutRebuilder.MarkLayoutForRebuild(parent);
    }

    private void OnDestroy()
    {
        heightTween?.Kill();
    }
}
