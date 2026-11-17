using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewButtonAnimProfile", menuName = "Mango Pony/Button Anim Profile")]
public class ButtonAnimProfile : ScriptableObject
{
    [Header("Mode")]
    [Tooltip("On = animates local scale | Off = animates the RectTransform size in pixels")]
    [FormerlySerializedAs("useScaleAnimation")] public bool useScale = false;

    [Header("Hover")]
    public Vector3 hoverScale = new Vector3(1.06f, 1.06f, 1.06f);
    [FormerlySerializedAs("hoverSizeOffset")] public Vector2 hoverSize = new Vector2(8f, 8f);
    public float hoverDuration = 0.2f;
    public Ease hoverEase = Ease.OutQuad;

    [Header("Press")]
    public Vector3 punchScale = new Vector3(-0.08f, -0.08f, 0f);
    public Vector2 punchSize = new Vector2(-10f, -10f);
    public float punchDuration = 0.25f;
    public int punchVibrato = 5;
    public float punchElasticity = 0.5f;

    [Header("Color")]
    public bool animateColor = false;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.95f, 0.95f, 1f, 1f);
    public Color disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);

    [Header("Disabled")]
    public float disabledAlpha = 0.5f;
    public float disabledScale = 0.95f;
    [FormerlySerializedAs("disabledSizeOffset")] public Vector2 disabledSize = new Vector2(-4f, -4f);
}
