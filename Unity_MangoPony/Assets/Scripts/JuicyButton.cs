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

    // Composants récupérés automatiquement en interne
    private Selectable targetSelectable;
    private Graphic targetGraphic;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    // References & Memory
    private Vector3 initialScale;
    private Tween scaleTween;
    private Tween colorTween;
    private Tween alphaTween;
    private bool isHovered;
    private bool isPressed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialScale = rectTransform.localScale;

        targetSelectable = GetComponent<Selectable>();
        if (targetSelectable != null)
            targetGraphic = targetSelectable.targetGraphic;

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        rectTransform.localScale = initialScale;
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

        bool isInteractable = IsInteractable();
        KillTweens();

        float targetAlpha = isInteractable ? 1f : profile.disabledAlpha;
        Vector3 targetScale = isInteractable ? initialScale : initialScale * profile.disabledScale;
        Color targetCol = isInteractable ? profile.normalColor : profile.disabledColor;

        if (immediate)
        {
            rectTransform.localScale = targetScale;
            if (canvasGroup != null) canvasGroup.alpha = targetAlpha;
            if (profile.animateColor && targetGraphic != null) targetGraphic.color = targetCol;
        }
        else
        {
            scaleTween = rectTransform.DOScale(targetScale, 0.25f).SetEase(Ease.OutCubic).SetUpdate(true);

            if (canvasGroup != null)
                alphaTween = canvasGroup.DOFade(targetAlpha, 0.25f).SetUpdate(true);

            if (profile.animateColor && targetGraphic != null)
                colorTween = targetGraphic.DOColor(targetCol, 0.25f).SetUpdate(true);
        }
    }

    #region Pointer Events (UI Handlers)

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null) return;

        isHovered = true;
        if (isPressed) return;

        AnimateScale(Vector3.Scale(initialScale, profile.hoverScale), profile.hoverDuration, profile.hoverEase);

        if (profile.animateColor && targetGraphic != null)
            AnimateColor(profile.hoverColor, profile.hoverDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null) return;

        isHovered = false;
        isPressed = false;

        AnimateScale(initialScale, profile.hoverDuration, Ease.OutQuad);

        if (profile.animateColor && targetGraphic != null)
            AnimateColor(profile.normalColor, profile.hoverDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null || eventData.button != PointerEventData.InputButton.Left) return;

        isPressed = true;

        scaleTween?.Kill();
        rectTransform.DOPunchScale(profile.punchScale, profile.punchDuration, profile.punchVibrato, profile.punchElasticity)
                     .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable() || profile == null || !isPressed) return;

        isPressed = false;

        Vector3 target = isHovered ? Vector3.Scale(initialScale, profile.hoverScale) : initialScale;
        AnimateScale(target, 0.15f, Ease.OutBack);
    }

    #endregion

    #region Helper Animations

    private void AnimateScale(Vector3 target, float duration, Ease ease)
    {
        scaleTween?.Kill();
        scaleTween = rectTransform.DOScale(target, duration)
                                 .SetEase(ease)
                                 .SetUpdate(true);
    }

    private void AnimateColor(Color target, float duration)
    {
        colorTween?.Kill();
        colorTween = targetGraphic.DOColor(target, duration).SetUpdate(true);
    }

    private bool IsInteractable()
    {
        return targetSelectable == null || targetSelectable.interactable;
    }

    private void KillTweens()
    {
        scaleTween?.Kill();
        colorTween?.Kill();
        alphaTween?.Kill();
    }

    #endregion
}