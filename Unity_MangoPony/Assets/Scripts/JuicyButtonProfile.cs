using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "NewJuicyButtonProfile", menuName = "Mango Pony/UI/Juicy Button Profile")]
public class JuicyButtonProfile : ScriptableObject
{
    [Header("Mode d'Animation")]
    [Tooltip("Coché = anime le LocalScale | Décoché = anime la taille réelle du RectTransform en pixels (sans déformation)")]
    public bool useScaleAnimation = false;

    [Header("Hover Settings (Survol)")]
    [Tooltip("Échelle cible lors du survol (si Use Scale Animation est coché)")]
    public Vector3 hoverScale = new Vector3(1.06f, 1.06f, 1.06f);
    [Tooltip("Variation de taille en pixels ajoutée au RectTransform (si Use Scale Animation est décoché)")]
    public Vector2 hoverSizeOffset = new Vector2(8f, 8f);
    public float hoverDuration = 0.2f;
    public Ease hoverEase = Ease.OutQuad;

    [Header("Click / Punch Settings (Pression)")]
    [Tooltip("Effet d'enfoncement en Scale (si Use Scale Animation est coché)")]
    public Vector3 punchScale = new Vector3(-0.08f, -0.08f, 0f);
    [Tooltip("Effet d'enfoncement en pixels sur le RectTransform (si Use Scale Animation est décoché)")]
    public Vector2 punchSize = new Vector2(-10f, -10f);
    public float punchDuration = 0.25f;
    public int punchVibrato = 5;
    public float punchElasticity = 0.5f;

    [Header("Color Feedback (Optionnel)")]
    public bool animateColor = false;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.95f, 0.95f, 1f, 1f);
    public Color disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);

    [Header("Disabled State Settings")]
    public float disabledAlpha = 0.5f;
    [Tooltip("Échelle lorsque le bouton est désactivé (si Use Scale Animation est coché)")]
    public float disabledScale = 0.95f;
    [Tooltip("Variation de taille en pixels lorsque le bouton est désactivé (si Use Scale Animation est décoché)")]
    public Vector2 disabledSizeOffset = new Vector2(-4f, -4f);
}