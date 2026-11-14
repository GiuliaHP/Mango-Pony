using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "NewJuicyButtonProfile", menuName = "Mango Pony/UI/Juicy Button Profile")]
public class JuicyButtonProfile : ScriptableObject
{
    [Header("Hover Settings (Survol)")]
    [Tooltip("Échelle lors du survol")]
    public Vector3 hoverScale = new Vector3(1.06f, 1.06f, 1.06f);
    public float hoverDuration = 0.2f;
    public Ease hoverEase = Ease.OutQuad;

    [Header("Click / Punch Settings (Pression)")]
    [Tooltip("Effet d'enfoncement dynamique lors du clic (squash & stretch)")]
    public Vector3 punchScale = new Vector3(-0.08f, -0.08f, 0f);
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
    public float disabledScale = 0.95f;
}