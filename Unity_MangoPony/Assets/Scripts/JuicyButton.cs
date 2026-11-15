using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Configuration")]
    [Tooltip("Profil ScriptableObject contenant tous les réglages d'animation DOTween")]
    [SerializeField] private JuicyButtonProfile profile;

    [Tooltip("Optionnel : élément spécifique du bouton (icône, bordure, texte TMP...) dont la couleur sera animée. Si vide, utilise le Target Graphic du bouton.")]
    [SerializeField] private Graphic customColorTarget;

    // Composants récupérés automatiquement en interne
    private Selectable targetSelectable;
    private Graphic resolvedColorGraphic;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    // References & Memory
    private Vector3 initialScale;
    private Vector2 initialSizeDelta;
    private bool hasCachedTransform;
    private Tween transformTween;
    private Tween colorTween;
    private Tween alphaTween;
    private bool isHovered;
    private bool isPressed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        CacheInitialTransform();

        targetSelectable = GetComponent<Selectable>();

        // Priorité à l'élément spécifique assigné dans l'Inspector, sinon fallback automatique
        if (customColorTarget != null)
            resolvedColorGraphic = customColorTarget;
        else if (targetSelectable != null && targetSelectable.targetGraphic != null)
            resolvedColorGraphic = targetSelectable.targetGraphic;
        else
            resolvedColorGraphic = GetComponent<Graphic>();

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void CacheInitialTransform()
    {
        if (!hasCachedTransform && rectTransform != null)
        {
            initialScale = rectTransform.localScale;
            initialSizeDelta = rectTransform.sizeDelta;
            hasCachedTransform = true;
        }
    }

    private void OnEnable()
    {
        CacheInitialTransform();
        rectTransform.localScale = initialScale;
        rectTransform.sizeDelta = initialSizeDelta;
        isHovered = false;
        isPressed = false;

        CheckInteractableState(true);
    }

    private void OnDisable()
    {
        KillTweens();
    }

    /// <summary>
    /// Vérifie et anime le passage actif/inactif (Disabled State)
    /// </summary>
    public void CheckInteractableState(bool immediate = false)
    {
        if (profile == null) return;

        CacheInitialTransform();
        bool isInteractable = IsInteractable();
        KillTweens();

        float targetAlpha = isInteractable ? 1f : profile.disabledAlpha;
        Color targetCol = isInteractable ? profile.normalColor : profile.disabledColor;

        if (profile.useScaleAnimation)
        {
            Vector3 targetScale = isInteractable ? initialScale : initialScale * profile.disabledScale;

            if (immediate)
                rectTransform.localScale = targetScale;
            else
                AnimateScale(targetScale, 0.25f, Ease.OutCubic);
        }
        else
        {
            Vector2 targetSize = isInteractable ? initialSizeDelta : initialSizeDelta + profile.disabledSizeOffset;

            if (immediate)
                SetSizeDelta(targetSize);
            else
                AnimateSize(targetSize, 0.25f, Ease.OutCubic);
        }

        if (immediate)
        {
            if (canvasGroup != null) canvasGroup.alpha = targetAlpha;
            if (profile.animateColor && resolvedColorGraphic != null) resolvedColorGraphic.color = targetCol;
        }
        else
        {
            if (canvasGroup != null)
                alphaTween = canvasGroup.DOFade(targetAlpha, 0.25f).SetUpdate(true);

            if (profile.animateColor && resolvedColorGraphic != null)
                colorTween = resolvedColorGraphic.DOColor(targetCol, 0.25f).SetUpdate(true);
        }
    }

    #region Pointer Events (UI Handlers)

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null) return;

        isHovered = true;
        if (isPressed) return;

        if (profile.useScaleAnimation)
            AnimateScale(Vector3.Scale(initialScale, profile.hoverScale), profile.hoverDuration, profile.hoverEase);
        else
            AnimateSize(initialSizeDelta + profile.hoverSizeOffset, profile.hoverDuration, profile.hoverEase);

        if (profile.animateColor && resolvedColorGraphic != null)
            AnimateColor(profile.hoverColor, profile.hoverDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null) return;

        isHovered = false;
        isPressed = false;

        if (profile.useScaleAnimation)
            AnimateScale(initialScale, profile.hoverDuration, Ease.OutQuad);
        else
            AnimateSize(initialSizeDelta, profile.hoverDuration, Ease.OutQuad);

        if (profile.animateColor && resolvedColorGraphic != null)
            AnimateColor(profile.normalColor, profile.hoverDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null || eventData.button != PointerEventData.InputButton.Left) return;

        isPressed = true;
        transformTween?.Kill();

        if (profile.useScaleAnimation)
        {
            transformTween = rectTransform.DOPunchScale(profile.punchScale, profile.punchDuration, profile.punchVibrato, profile.punchElasticity)
                                          .SetUpdate(true);
        }
        else
        {
            Vector2 punchOffset = Vector2.zero;
            transformTween = DOTween.Punch(() => punchOffset, x =>
            {
                punchOffset = x;
                SetSizeDelta(initialSizeDelta + punchOffset);
            }, profile.punchSize, profile.punchDuration, profile.punchVibrato, profile.punchElasticity)
            .SetUpdate(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null || !isPressed) return;

        isPressed = false;

        if (profile.useScaleAnimation)
        {
            Vector3 target = isHovered ? Vector3.Scale(initialScale, profile.hoverScale) : initialScale;
            AnimateScale(target, 0.15f, Ease.OutBack);
        }
        else
        {
            Vector2 target = isHovered ? initialSizeDelta + profile.hoverSizeOffset : initialSizeDelta;
            AnimateSize(target, 0.15f, Ease.OutBack);
        }
    }

    #endregion

    #region Helper Animations

    private void AnimateScale(Vector3 target, float duration, Ease ease)
    {
        transformTween?.Kill();
        transformTween = rectTransform.DOScale(target, duration)
                                      .SetEase(ease)
                                      .SetUpdate(true);
    }

    private void AnimateSize(Vector2 target, float duration, Ease ease)
    {
        transformTween?.Kill();
        transformTween = DOVirtual.Vector3(rectTransform.sizeDelta, target, duration, val => SetSizeDelta(val))
                                  .SetEase(ease)
                                  .SetUpdate(true);
    }

    private void SetSizeDelta(Vector2 size)
    {
        rectTransform.sizeDelta = size;
        NotifyParentLayout();
    }

    private void NotifyParentLayout()
    {
        if (rectTransform.parent is RectTransform parentRect)
            LayoutRebuilder.MarkLayoutForRebuild(parentRect);
    }

    private void AnimateColor(Color target, float duration)
    {
        colorTween?.Kill();
        colorTween = resolvedColorGraphic.DOColor(target, duration).SetUpdate(true);
    }

    private bool IsInteractable()
    {
        return targetSelectable == null || targetSelectable.interactable;
    }

    private void KillTweens()
    {
        transformTween?.Kill();
        colorTween?.Kill();
        alphaTween?.Kill();
    }

    #endregion
}