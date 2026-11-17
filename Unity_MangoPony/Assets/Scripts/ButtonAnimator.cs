using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private ButtonAnimProfile profile;
    [Tooltip("Graphic whose color is animated (empty = the button's target graphic)")]
    [FormerlySerializedAs("customColorTarget")] [SerializeField] private Graphic colorTarget;

    private Selectable selectable;
    private Graphic colorGraphic;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Vector2 baseSize;
    private bool hasBaseTransform;
    private Tween transformTween;
    private Tween colorTween;
    private Tween alphaTween;
    private bool isHovered;
    private bool isPressed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        CacheBaseTransform();

        selectable = GetComponent<Selectable>();

        if (colorTarget != null)
            colorGraphic = colorTarget;
        else if (selectable != null && selectable.targetGraphic != null)
            colorGraphic = selectable.targetGraphic;
        else
            colorGraphic = GetComponent<Graphic>();

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void CacheBaseTransform()
    {
        if (!hasBaseTransform && rectTransform != null)
        {
            baseScale = rectTransform.localScale;
            baseSize = rectTransform.sizeDelta;
            hasBaseTransform = true;
        }
    }

    private void OnEnable()
    {
        CacheBaseTransform();
        rectTransform.localScale = baseScale;
        rectTransform.sizeDelta = baseSize;
        isHovered = false;
        isPressed = false;

        RefreshInteractable(true);
    }

    private void OnDisable()
    {
        KillTweens();
    }

    public void RefreshInteractable(bool immediate = false)
    {
        if (profile == null) return;

        CacheBaseTransform();
        bool isInteractable = IsInteractable();
        KillTweens();

        float targetAlpha = isInteractable ? 1f : profile.disabledAlpha;
        Color targetColor = isInteractable ? profile.normalColor : profile.disabledColor;

        if (profile.useScale)
        {
            Vector3 targetScale = isInteractable ? baseScale : baseScale * profile.disabledScale;

            if (immediate)
                rectTransform.localScale = targetScale;
            else
                AnimateScale(targetScale, 0.25f, Ease.OutCubic);
        }
        else
        {
            Vector2 targetSize = isInteractable ? baseSize : baseSize + profile.disabledSize;

            if (immediate)
                SetSize(targetSize);
            else
                AnimateSize(targetSize, 0.25f, Ease.OutCubic);
        }

        if (immediate)
        {
            if (canvasGroup != null) canvasGroup.alpha = targetAlpha;
            if (profile.animateColor && colorGraphic != null) colorGraphic.color = targetColor;
        }
        else
        {
            if (canvasGroup != null)
                alphaTween = canvasGroup.DOFade(targetAlpha, 0.25f).SetUpdate(true);

            if (profile.animateColor && colorGraphic != null)
                colorTween = colorGraphic.DOColor(targetColor, 0.25f).SetUpdate(true);
        }
    }

    #region Pointer

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null) return;

        isHovered = true;
        if (isPressed) return;

        if (profile.useScale)
            AnimateScale(Vector3.Scale(baseScale, profile.hoverScale), profile.hoverDuration, profile.hoverEase);
        else
            AnimateSize(baseSize + profile.hoverSize, profile.hoverDuration, profile.hoverEase);

        if (profile.animateColor && colorGraphic != null)
            AnimateColor(profile.hoverColor, profile.hoverDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null) return;

        isHovered = false;
        isPressed = false;

        if (profile.useScale)
            AnimateScale(baseScale, profile.hoverDuration, Ease.OutQuad);
        else
            AnimateSize(baseSize, profile.hoverDuration, Ease.OutQuad);

        if (profile.animateColor && colorGraphic != null)
            AnimateColor(profile.normalColor, profile.hoverDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null || eventData.button != PointerEventData.InputButton.Left) return;

        isPressed = true;
        transformTween?.Kill();

        if (profile.useScale)
        {
            transformTween = rectTransform.DOPunchScale(profile.punchScale, profile.punchDuration, profile.punchVibrato, profile.punchElasticity)
                                          .SetUpdate(true);
        }
        else
        {
            Vector2 punch = Vector2.zero;
            transformTween = DOTween.Punch(() => punch, x =>
            {
                punch = x;
                SetSize(baseSize + punch);
            }, profile.punchSize, profile.punchDuration, profile.punchVibrato, profile.punchElasticity)
            .SetUpdate(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null || !isPressed) return;

        isPressed = false;

        if (profile.useScale)
            AnimateScale(isHovered ? Vector3.Scale(baseScale, profile.hoverScale) : baseScale, 0.15f, Ease.OutBack);
        else
            AnimateSize(isHovered ? baseSize + profile.hoverSize : baseSize, 0.15f, Ease.OutBack);
    }

    #endregion

    #region Tweens

    private void AnimateScale(Vector3 target, float duration, Ease ease)
    {
        transformTween?.Kill();
        transformTween = rectTransform.DOScale(target, duration).SetEase(ease).SetUpdate(true);
    }

    private void AnimateSize(Vector2 target, float duration, Ease ease)
    {
        transformTween?.Kill();
        transformTween = DOVirtual.Vector3(rectTransform.sizeDelta, target, duration, value => SetSize(value))
                                  .SetEase(ease)
                                  .SetUpdate(true);
    }

    private void SetSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;

        if (rectTransform.parent is RectTransform parent)
            LayoutRebuilder.MarkLayoutForRebuild(parent);
    }

    private void AnimateColor(Color target, float duration)
    {
        colorTween?.Kill();
        colorTween = colorGraphic.DOColor(target, duration).SetUpdate(true);
    }

    private bool IsInteractable()
    {
        return selectable == null || selectable.interactable;
    }

    private void KillTweens()
    {
        transformTween?.Kill();
        colorTween?.Kill();
        alphaTween?.Kill();
    }

    #endregion
}
