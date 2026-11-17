using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OptionButton : MonoBehaviour
{
    public enum OptionKind
    {
        Shape,
        Pattern,
        Swatch
    }

    [FormerlySerializedAs("optionType")] [SerializeField, HideInInspector] private OptionKind kind;
    [SerializeField, HideInInspector] private string cameraId;

    [FormerlySerializedAs("slotType")] [SerializeField, HideInInspector] private PonyCustomizer.Slot slot;
    [FormerlySerializedAs("meshPrefab")] [SerializeField, HideInInspector] private GameObject prefab;
    [SerializeField, HideInInspector] private bool hasLinkedShape;
    [SerializeField, HideInInspector] private PonyCustomizer.Slot linkedSlot;
    [SerializeField, HideInInspector] private GameObject linkedPrefab;

    [FormerlySerializedAs("targetZone")] [SerializeField, HideInInspector] private PonyCustomizer.Zone zone;
    [FormerlySerializedAs("isLegsPattern")] [SerializeField, HideInInspector] private bool isLegs;
    [FormerlySerializedAs("patternTexture")] [SerializeField, HideInInspector] private Texture texture;

    [Header("Swatch")]
    [FormerlySerializedAs("swatchPreviewImage")] [SerializeField] private Image swatchImage;
    [FormerlySerializedAs("shaderColorProperty")] [SerializeField, HideInInspector] private string colorProperty = "_BaseColor";
    [SerializeField, HideInInspector] private Color defaultColor = Color.white;
    [SerializeField, HideInInspector] private ColorWheel wheel;

    private Color currentColor;
    private Button button;
    private PonyCustomizer customizer;
    private CameraRig cameraRig;
    private LegSelector legSelector;

    public OptionKind Kind => kind;
    public Color CurrentColor => currentColor;
    public string ColorProperty => colorProperty;

    private void Awake()
    {
        button = GetComponent<Button>();
        customizer = FindAnyObjectByType<PonyCustomizer>();
        cameraRig = FindAnyObjectByType<CameraRig>();

        if (kind == OptionKind.Swatch)
        {
            if (swatchImage == null)
                swatchImage = GetComponent<Image>();

            currentColor = defaultColor;
            RefreshSwatch();
        }

        if (isLegs)
        {
            legSelector = GetComponentInParent<LegSelector>(true);
            if (legSelector == null)
                legSelector = FindAnyObjectByType<LegSelector>(FindObjectsInactive.Include);
        }
    }

    private void Start()
    {
        if (kind == OptionKind.Swatch)
            ApplyColorToPony();
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

    private void OnClick()
    {
        switch (kind)
        {
            case OptionKind.Shape:
                if (customizer != null)
                {
                    customizer.SetMesh(slot, prefab);
                    if (hasLinkedShape)
                        customizer.SetMesh(linkedSlot, linkedPrefab);
                }
                break;

            case OptionKind.Pattern:
                if (customizer != null)
                {
                    if (isLegs && legSelector != null)
                        customizer.SetPattern(legSelector.GetActiveZones(), texture);
                    else
                        customizer.SetPattern(zone, texture);
                }
                break;

            case OptionKind.Swatch:
                if (wheel != null)
                    wheel.SelectSwatch(this);
                break;
        }

        if (cameraRig != null)
            cameraRig.Focus(cameraId);
    }

    #region Swatch

    public void ApplyColor(Color color)
    {
        if (kind != OptionKind.Swatch) return;

        currentColor = color;
        RefreshSwatch();
        ApplyColorToPony();
    }

    public void SetColorSilently(Color color)
    {
        if (kind != OptionKind.Swatch) return;

        currentColor = color;
        RefreshSwatch();
    }

    private void RefreshSwatch()
    {
        if (swatchImage != null)
            swatchImage.color = currentColor;
    }

    private void ApplyColorToPony()
    {
        if (customizer == null) return;

        if (isLegs && legSelector != null)
            customizer.SetColor(legSelector.GetActiveZones(), currentColor, colorProperty);
        else
            customizer.SetColor(zone, currentColor, colorProperty);
    }

    public void SetWheel(ColorWheel colorWheel)
    {
        wheel = colorWheel;
    }

    #endregion

    #region Setup (called by CategoryMenuEditor)

    public void SetupShape(PonyCustomizer.Slot shapeSlot, CategoryData.ShapeOption option, string camera)
    {
        kind = OptionKind.Shape;
        slot = shapeSlot;
        prefab = option.prefab;
        cameraId = camera;
    }

    public void SetLinkedShape(PonyCustomizer.Slot shapeSlot, CategoryData.ShapeOption option)
    {
        hasLinkedShape = true;
        linkedSlot = shapeSlot;
        linkedPrefab = option.prefab;
    }

    public void SetupPattern(PonyCustomizer.Zone patternZone, CategoryData.PatternOption option, bool legs, string camera)
    {
        kind = OptionKind.Pattern;
        zone = patternZone;
        texture = option.texture;
        isLegs = legs;
        cameraId = camera;
    }

    public void SetupSwatch(PonyCustomizer.Zone colorZone, CategoryData.ColorChannel channel, bool legs, string camera)
    {
        kind = OptionKind.Swatch;
        zone = colorZone;
        colorProperty = channel.property;
        defaultColor = channel.defaultColor;
        currentColor = defaultColor;
        isLegs = legs;
        cameraId = camera;
        RefreshSwatch();
    }

    #endregion
}
