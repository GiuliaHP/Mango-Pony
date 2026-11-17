using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CustomizationButton : MonoBehaviour
{
    public enum OptionType
    {
        MeshShape,
        Pattern,
        ColorSwatch
    }

    [SerializeField, HideInInspector] private OptionType optionType;
    [SerializeField, HideInInspector] private CustomizationCategoryData.CameraFocusZone focusZone;

    [Header("Données MeshShape")]
    [SerializeField, HideInInspector] private Swapper.SlotType slotType;
    [SerializeField, HideInInspector] private GameObject meshPrefab;

    [Header("Données Pattern & Color")]
    [SerializeField, HideInInspector] private Swapper.MaterialZone targetZone;
    [SerializeField, HideInInspector] private bool isLegsPattern;

    [Header("Données Pattern")]
    [SerializeField, HideInInspector] private Texture patternTexture;

    [Header("Données ColorSwatch")]
    [SerializeField] private Image swatchPreviewImage;
    [SerializeField, HideInInspector] private string shaderColorProperty = "_BaseColor";
    [SerializeField, HideInInspector] private Color defaultColor = Color.white;

    private Color currentColor;
    private Button button;
    private Swapper swapper;
    private CameraController cameraController;
    private CustomColorWheel colorWheel;
    private LegsPatternSelector legsSelector;

    public OptionType Type => optionType;
    public Color CurrentColor => currentColor;

    private void Awake()
    {
        button = GetComponent<Button>();
        swapper = FindFirstObjectByType<Swapper>();
        cameraController = FindFirstObjectByType<CameraController>();

        if (optionType == OptionType.ColorSwatch)
        {
            colorWheel = FindNextColorWheelInHierarchy();
            if (swatchPreviewImage == null)
                swatchPreviewImage = GetComponent<Image>();

            currentColor = defaultColor;
            UpdateSwatchVisual();
        }

        if (isLegsPattern)
        {
            legsSelector = GetComponentInParent<LegsPatternSelector>(true);
            if (legsSelector == null)
                legsSelector = FindFirstObjectByType<LegsPatternSelector>(FindObjectsInactive.Include);
        }
    }

    private void Start()
    {
        if (optionType == OptionType.ColorSwatch)
            ApplyColorToShader();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        switch (optionType)
        {
            case OptionType.MeshShape:
                if (swapper != null)
                    swapper.SwapSlotMesh(slotType, meshPrefab);
                break;

            case OptionType.Pattern:
                if (swapper != null)
                {
                    if (isLegsPattern && legsSelector != null)
                        swapper.SwapZonesPattern(legsSelector.GetActiveTargetZones(), patternTexture);
                    else
                        swapper.SwapPatternTexture(targetZone, patternTexture);
                }
                break;

            case OptionType.ColorSwatch:
                if (colorWheel == null)
                    colorWheel = FindNextColorWheelInHierarchy();

                if (colorWheel != null)
                    colorWheel.SelectSwatchButton(this);
                break;
        }

        if (cameraController != null)
            cameraController.FocusOnZone(focusZone);
    }

    #region Méthodes Spécifiques ColorSwatch

    public void ReceiveColorFromPicker(Color newColor)
    {
        if (optionType != OptionType.ColorSwatch) return;

        currentColor = newColor;
        UpdateSwatchVisual();
        ApplyColorToShader();
    }

    public void SyncColorSilently(Color newColor)
    {
        if (optionType != OptionType.ColorSwatch) return;

        currentColor = newColor;
        UpdateSwatchVisual();
    }

    private void UpdateSwatchVisual()
    {
        if (swatchPreviewImage != null)
            swatchPreviewImage.color = currentColor;
    }

    private void ApplyColorToShader()
    {
        if (swapper == null) return;

        if (isLegsPattern && legsSelector != null)
            swapper.SwapZonesColor(legsSelector.GetActiveTargetZones(), currentColor, shaderColorProperty);
        else
            swapper.SwapZoneColor(targetZone, currentColor, shaderColorProperty);
    }

    private CustomColorWheel FindNextColorWheelInHierarchy()
    {
        Transform current = transform;
        while (current.parent != null)
        {
            Transform parent = current.parent;
            int myIndex = current.GetSiblingIndex();

            for (int i = myIndex + 1; i < parent.childCount; i++)
            {
                CustomColorWheel wheel = parent.GetChild(i).GetComponentInChildren<CustomColorWheel>(true);
                if (wheel != null)
                    return wheel;
            }
            current = parent;
        }

        return FindFirstObjectByType<CustomColorWheel>(FindObjectsInactive.Exclude);
    }

    #endregion

    #region Méthodes Setup (appelées par CustomizationUIGeneratorEditor)

    public void SetupAsMesh(Swapper.SlotType slot, CustomizationCategoryData.MeshShapeOption option, CustomizationCategoryData.CameraFocusZone camZone)
    {
        optionType = OptionType.MeshShape;
        slotType = slot;
        meshPrefab = option.meshPrefab;
        focusZone = camZone;
    }

    public void SetupAsPattern(Swapper.MaterialZone zone, CustomizationCategoryData.PatternOption option, bool legsMode, CustomizationCategoryData.CameraFocusZone camZone)
    {
        optionType = OptionType.Pattern;
        targetZone = zone;
        patternTexture = option.patternTexture;
        isLegsPattern = legsMode;
        focusZone = camZone;
    }

    public void SetupAsColorSwatch(Swapper.MaterialZone zone, CustomizationCategoryData.ColorChannel channel, bool legsMode, CustomizationCategoryData.CameraFocusZone camZone)
    {
        optionType = OptionType.ColorSwatch;
        targetZone = zone;
        shaderColorProperty = channel.shaderPropertyName;
        defaultColor = channel.defaultColor;
        currentColor = defaultColor;
        isLegsPattern = legsMode;
        focusZone = camZone;
        UpdateSwatchVisual();
    }

    #endregion
}