using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LegsPatternSelector : MonoBehaviour
{
    public enum ApplicationMode
    {
        FourLegs,
        FrontBack,
        Individual
    }

    [Header("Boutons de Mode (4 Legs / Front-Back / Individual)")]
    [SerializeField] private Button fourLegsModeButton;
    [SerializeField] private Button frontBackModeButton;
    [SerializeField] private Button individualModeButton;

    [Header("Sous-conteneurs de sélection de pattes")]
    [Tooltip("Conteneur avec les 2 boutons Front / Back")]
    [SerializeField] private GameObject frontBackSubContainer;
    [SerializeField] private Button frontLegsButton;
    [SerializeField] private Button backLegsButton;

    [Space(5)]
    [Tooltip("Conteneur avec les 4 boutons FL / FR / BL / BR")]
    [SerializeField] private GameObject individualSubContainer;
    [SerializeField] private Button frontLeftButton;
    [SerializeField] private Button frontRightButton;
    [SerializeField] private Button backLeftButton;
    [SerializeField] private Button backRightButton;

    [Header("Couleurs visuelles des boutons actifs/inactifs")]
    [SerializeField] private Color selectedButtonColor = new Color(1f, 0.8f, 0.35f, 1f);
    [SerializeField] private Color unselectedButtonColor = Color.white;

    private ApplicationMode currentMode = ApplicationMode.FourLegs;
    private int selectedSubIndex = 0; // FrontBack: 0=Front, 1=Back | Individual: 0=FL, 1=FR, 2=BL, 3=BR

    private Swapper swapper;
    private CustomizationButton linkedColorSwatch;
    private CustomColorWheel linkedColorWheel;

    private void Awake()
    {
        swapper = FindFirstObjectByType<Swapper>();

        if (fourLegsModeButton != null) fourLegsModeButton.onClick.AddListener(() => SetMode(ApplicationMode.FourLegs));
        if (frontBackModeButton != null) frontBackModeButton.onClick.AddListener(() => SetMode(ApplicationMode.FrontBack));
        if (individualModeButton != null) individualModeButton.onClick.AddListener(() => SetMode(ApplicationMode.Individual));

        if (frontLegsButton != null) frontLegsButton.onClick.AddListener(() => SelectSubTarget(0));
        if (backLegsButton != null) backLegsButton.onClick.AddListener(() => SelectSubTarget(1));

        if (frontLeftButton != null) frontLeftButton.onClick.AddListener(() => SelectSubTarget(0));
        if (frontRightButton != null) frontRightButton.onClick.AddListener(() => SelectSubTarget(1));
        if (backLeftButton != null) backLeftButton.onClick.AddListener(() => SelectSubTarget(2));
        if (backRightButton != null) backRightButton.onClick.AddListener(() => SelectSubTarget(3));
    }

    private void Start()
    {
        SetMode(ApplicationMode.FourLegs);
    }

    public void BindColorControls(CustomizationButton swatch, CustomColorWheel wheel)
    {
        linkedColorSwatch = swatch;
        linkedColorWheel = wheel;
    }

    public void SetMode(ApplicationMode mode)
    {
        currentMode = mode;
        selectedSubIndex = 0;

        if (frontBackSubContainer != null)
            frontBackSubContainer.SetActive(mode == ApplicationMode.FrontBack);

        if (individualSubContainer != null)
            individualSubContainer.SetActive(mode == ApplicationMode.Individual);

        RefreshButtonsVisuals();
        SyncColorSwatchWithActiveLeg();
    }

    public void SelectSubTarget(int index)
    {
        selectedSubIndex = index;
        RefreshButtonsVisuals();
        SyncColorSwatchWithActiveLeg();
    }

    /// <summary>
    /// Renvoie la liste des pattes actuellement ciblées par l'utilisateur.
    /// </summary>
    public List<Swapper.MaterialZone> GetActiveTargetZones()
    {
        switch (currentMode)
        {
            case ApplicationMode.FrontBack:
                return selectedSubIndex == 0
                    ? new List<Swapper.MaterialZone> { Swapper.MaterialZone.FrontLeftLeg, Swapper.MaterialZone.FrontRightLeg }
                    : new List<Swapper.MaterialZone> { Swapper.MaterialZone.BackLeftLeg, Swapper.MaterialZone.BackRightLeg };

            case ApplicationMode.Individual:
                return selectedSubIndex switch
                {
                    0 => new List<Swapper.MaterialZone> { Swapper.MaterialZone.FrontLeftLeg },
                    1 => new List<Swapper.MaterialZone> { Swapper.MaterialZone.FrontRightLeg },
                    2 => new List<Swapper.MaterialZone> { Swapper.MaterialZone.BackLeftLeg },
                    _ => new List<Swapper.MaterialZone> { Swapper.MaterialZone.BackRightLeg }
                };

            default:
                return new List<Swapper.MaterialZone>
                {
                    Swapper.MaterialZone.FrontLeftLeg,
                    Swapper.MaterialZone.FrontRightLeg,
                    Swapper.MaterialZone.BackLeftLeg,
                    Swapper.MaterialZone.BackRightLeg
                };
        }
    }

    private void SyncColorSwatchWithActiveLeg()
    {
        if (swapper == null || linkedColorSwatch == null) return;

        var zones = GetActiveTargetZones();
        if (zones.Count == 0) return;

        Color legColor = swapper.GetZoneColor(zones[0]);
        linkedColorSwatch.SyncColorSilently(legColor);

        if (linkedColorWheel != null)
            linkedColorWheel.SelectSwatchButton(linkedColorSwatch);
    }

    private void RefreshButtonsVisuals()
    {
        SetButtonTint(fourLegsModeButton, currentMode == ApplicationMode.FourLegs);
        SetButtonTint(frontBackModeButton, currentMode == ApplicationMode.FrontBack);
        SetButtonTint(individualModeButton, currentMode == ApplicationMode.Individual);

        if (currentMode == ApplicationMode.FrontBack)
        {
            SetButtonTint(frontLegsButton, selectedSubIndex == 0);
            SetButtonTint(backLegsButton, selectedSubIndex == 1);
        }
        else if (currentMode == ApplicationMode.Individual)
        {
            SetButtonTint(frontLeftButton, selectedSubIndex == 0);
            SetButtonTint(frontRightButton, selectedSubIndex == 1);
            SetButtonTint(backLeftButton, selectedSubIndex == 2);
            SetButtonTint(backRightButton, selectedSubIndex == 3);
        }
    }

    private void SetButtonTint(Button btn, bool isSelected)
    {
        if (btn != null && btn.targetGraphic != null)
            btn.targetGraphic.color = isSelected ? selectedButtonColor : unselectedButtonColor;
    }
}