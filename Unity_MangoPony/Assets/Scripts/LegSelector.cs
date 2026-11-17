using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LegSelector : MonoBehaviour
{
    public enum Mode
    {
        All,
        FrontBack,
        Individual
    }

    [Header("Mode Buttons")]
    [FormerlySerializedAs("fourLegsModeButton")] [SerializeField] private Button allButton;
    [FormerlySerializedAs("frontBackModeButton")] [SerializeField] private Button frontBackButton;
    [FormerlySerializedAs("individualModeButton")] [SerializeField] private Button individualButton;

    [Header("Front / Back")]
    [FormerlySerializedAs("frontBackSubContainer")] [SerializeField] private GameObject frontBackGroup;
    [FormerlySerializedAs("frontLegsButton")] [SerializeField] private Button frontButton;
    [FormerlySerializedAs("backLegsButton")] [SerializeField] private Button backButton;

    [Header("Individual")]
    [FormerlySerializedAs("individualSubContainer")] [SerializeField] private GameObject individualGroup;
    [SerializeField] private Button frontLeftButton;
    [SerializeField] private Button frontRightButton;
    [SerializeField] private Button backLeftButton;
    [SerializeField] private Button backRightButton;

    [Header("Button Tints")]
    [FormerlySerializedAs("selectedButtonColor")] [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.35f, 1f);
    [FormerlySerializedAs("unselectedButtonColor")] [SerializeField] private Color unselectedColor = Color.white;

    private Mode mode = Mode.All;
    private int subIndex = 0;
    private PonyCustomizer customizer;
    [SerializeField, HideInInspector] private OptionButton linkedSwatch;
    [SerializeField, HideInInspector] private ColorWheel linkedWheel;

    private void Awake()
    {
        customizer = FindAnyObjectByType<PonyCustomizer>();

        if (allButton != null) allButton.onClick.AddListener(() => SetMode(Mode.All));
        if (frontBackButton != null) frontBackButton.onClick.AddListener(() => SetMode(Mode.FrontBack));
        if (individualButton != null) individualButton.onClick.AddListener(() => SetMode(Mode.Individual));

        if (frontButton != null) frontButton.onClick.AddListener(() => SelectLeg(0));
        if (backButton != null) backButton.onClick.AddListener(() => SelectLeg(1));

        if (frontLeftButton != null) frontLeftButton.onClick.AddListener(() => SelectLeg(0));
        if (frontRightButton != null) frontRightButton.onClick.AddListener(() => SelectLeg(1));
        if (backLeftButton != null) backLeftButton.onClick.AddListener(() => SelectLeg(2));
        if (backRightButton != null) backRightButton.onClick.AddListener(() => SelectLeg(3));
    }

    private void Start()
    {
        SetMode(Mode.All);
    }

    public void BindColor(OptionButton swatch, ColorWheel wheel)
    {
        linkedSwatch = swatch;
        linkedWheel = wheel;
    }

    public void SetMode(Mode newMode)
    {
        mode = newMode;
        subIndex = 0;

        if (frontBackGroup != null)
            frontBackGroup.SetActive(mode == Mode.FrontBack);

        if (individualGroup != null)
            individualGroup.SetActive(mode == Mode.Individual);

        RefreshTints();
        SyncSwatch();
    }

    public void SelectLeg(int index)
    {
        subIndex = index;
        RefreshTints();
        SyncSwatch();
    }

    public List<PonyCustomizer.Zone> GetActiveZones()
    {
        switch (mode)
        {
            case Mode.FrontBack:
                return subIndex == 0
                    ? new List<PonyCustomizer.Zone> { PonyCustomizer.Zone.FrontLeftLeg, PonyCustomizer.Zone.FrontRightLeg }
                    : new List<PonyCustomizer.Zone> { PonyCustomizer.Zone.BackLeftLeg, PonyCustomizer.Zone.BackRightLeg };

            case Mode.Individual:
                return new List<PonyCustomizer.Zone> { PonyCustomizer.LegZones[Mathf.Clamp(subIndex, 0, 3)] };

            default:
                return new List<PonyCustomizer.Zone>(PonyCustomizer.LegZones);
        }
    }

    private void SyncSwatch()
    {
        if (customizer == null || linkedSwatch == null) return;

        var zones = GetActiveZones();
        if (zones.Count == 0) return;

        linkedSwatch.SetColorSilently(customizer.GetColor(zones[0], linkedSwatch.ColorProperty));

        if (linkedWheel != null)
            linkedWheel.SelectSwatch(linkedSwatch);
    }

    private void RefreshTints()
    {
        SetTint(allButton, mode == Mode.All);
        SetTint(frontBackButton, mode == Mode.FrontBack);
        SetTint(individualButton, mode == Mode.Individual);

        if (mode == Mode.FrontBack)
        {
            SetTint(frontButton, subIndex == 0);
            SetTint(backButton, subIndex == 1);
        }
        else if (mode == Mode.Individual)
        {
            SetTint(frontLeftButton, subIndex == 0);
            SetTint(frontRightButton, subIndex == 1);
            SetTint(backLeftButton, subIndex == 2);
            SetTint(backRightButton, subIndex == 3);
        }
    }

    private void SetTint(Button button, bool isSelected)
    {
        if (button != null && button.targetGraphic != null)
            button.targetGraphic.color = isSelected ? selectedColor : unselectedColor;
    }
}
