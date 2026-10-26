using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MangoPony.Core;
using MangoPony.CameraControl;
using MangoPony.Save;

namespace MangoPony.UI
{
    /// <summary>
    /// Contrôleur UI principal du Customizer Mango Pony.
    /// Fait le lien entre les sélecteurs, les palettes de couleurs, les boutons d'action
    /// et le PonyCustomizerManager.
    /// </summary>
    public class PonyCustomizerUI : MonoBehaviour
    {
        [Header("Manager Binding")]
        [SerializeField] private PonyCustomizerManager manager;
        [SerializeField] private PonyOrbitCamera orbitCamera;

        [Header("Tab Navigation")]
        [SerializeField] private PonyTabController tabController;

        [Header("--- Tab 1: Corps ---")]
        [SerializeField] private UIOptionSelector bodyTypeSelector;
        [SerializeField] private UIOptionSelector hoofTypeSelector;
        [SerializeField] private UIOptionSelector bodyPatternSelector;
        [SerializeField] private UIOptionSelector coatNuanceSelector;
        [SerializeField] private UIFlexibleColorPicker bodyColorPicker;
        [SerializeField] private UIFlexibleColorPicker hoofColorPicker;

        [Header("--- Tab 2: Tête & Crins ---")]
        [SerializeField] private UIOptionSelector headTypeSelector;
        [SerializeField] private UIOptionSelector faceMarkSelector;
        [SerializeField] private UIOptionSelector maneTypeSelector;
        [SerializeField] private UIOptionSelector tailTypeSelector;
        [SerializeField] private UIFlexibleColorPicker maneColorPicker;
        [SerializeField] private UIFlexibleColorPicker eyeColorPicker;

        [Header("--- Tab 3: Marques & Couleurs ---")]
        [SerializeField] private Toggle syncLegsToggle;
        [SerializeField] private GameObject syncLegsPanel;
        [SerializeField] private GameObject individualLegsPanel;
        [SerializeField] private UIOptionSelector allLegsMarkSelector;
        [SerializeField] private UIOptionSelector frontLeftLegSelector;
        [SerializeField] private UIOptionSelector frontRightLegSelector;
        [SerializeField] private UIOptionSelector backLeftLegSelector;
        [SerializeField] private UIOptionSelector backRightLegSelector;
        [SerializeField] private UIFlexibleColorPicker markColorPicker;
        [SerializeField] private UIFlexibleColorPicker patternColorPicker;
        [SerializeField] private UIFlexibleColorPicker nuanceColorPicker;

        [Header("--- Tab 4: Accessoires ---")]
        [SerializeField] private UIOptionSelector hornTypeSelector;
        [SerializeField] private UIOptionSelector wingTypeSelector;
        [SerializeField] private UIFlexibleColorPicker hornColorPicker;
        [SerializeField] private UIFlexibleColorPicker wingColorPicker;

        [Header("--- Global Action Bar ---")]
        [SerializeField] private Button randomizeButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Toggle turntableToggle;
        [SerializeField] private TextMeshProUGUI statusNotificationLabel;

        private bool isSyncingUI = false;

        private void Start()
        {
            if (manager == null)
            {
                manager = PonyCustomizerManager.Instance;
            }

            SetupSelectors();
            BindEvents();

            if (manager != null)
            {
                manager.OnPonyUpdated += SyncUIWithData;
                SyncUIWithData(manager.CurrentData);
            }
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnPonyUpdated -= SyncUIWithData;
            }
        }

        private void SetupSelectors()
        {
            // Corps & Sabots
            if (bodyTypeSelector != null)
                bodyTypeSelector.Setup("Morphologie", Enum.GetNames(typeof(BodyType)));
            if (hoofTypeSelector != null)
                hoofTypeSelector.Setup("Sabots", Enum.GetNames(typeof(HoofType)));

            // Tête & Crins
            if (headTypeSelector != null)
                headTypeSelector.Setup("Profil de Tête", Enum.GetNames(typeof(HeadType)));
            if (maneTypeSelector != null)
                maneTypeSelector.Setup("Crinière", Enum.GetNames(typeof(ManeType)));
            if (tailTypeSelector != null)
                tailTypeSelector.Setup("Queue", Enum.GetNames(typeof(TailType)));

            // Motifs & Nuances
            if (faceMarkSelector != null)
                faceMarkSelector.Setup("Marque en Tête", Enum.GetNames(typeof(FaceMarkType)));
            if (bodyPatternSelector != null)
                bodyPatternSelector.Setup("Motif Corporel", Enum.GetNames(typeof(BodyPatternType)));
            if (coatNuanceSelector != null)
                coatNuanceSelector.Setup("Nuance de Robe", Enum.GetNames(typeof(CoatNuanceType)));

            // Balzanes aux membres
            string[] legMarkNames = Enum.GetNames(typeof(LegMarkType));
            if (allLegsMarkSelector != null)
                allLegsMarkSelector.Setup("Balzanes (4 jambes)", legMarkNames);
            if (frontLeftLegSelector != null)
                frontLeftLegSelector.Setup("Antérieur Gauche", legMarkNames);
            if (frontRightLegSelector != null)
                frontRightLegSelector.Setup("Antérieur Droit", legMarkNames);
            if (backLeftLegSelector != null)
                backLeftLegSelector.Setup("Postérieur Gauche", legMarkNames);
            if (backRightLegSelector != null)
                backRightLegSelector.Setup("Postérieur Droit", legMarkNames);

            // Accessoires fantastiques
            if (hornTypeSelector != null)
                hornTypeSelector.Setup("Corne", Enum.GetNames(typeof(HornType)));
            if (wingTypeSelector != null)
                wingTypeSelector.Setup("Ailes", Enum.GetNames(typeof(WingType)));

            // Setup palettes par défaut
            Color[] coatPalette = new Color[]
            {
                new Color(0.85f, 0.55f, 0.32f), // Alezan
                new Color(0.40f, 0.22f, 0.12f), // Bai
                new Color(0.12f, 0.12f, 0.13f), // Noir
                new Color(0.88f, 0.88f, 0.90f), // Gris
                new Color(0.92f, 0.78f, 0.48f), // Palomino
                new Color(0.95f, 0.65f, 0.75f)  // Rose pastel
            };

            if (bodyColorPicker != null) bodyColorPicker.Setup("Couleur de Robe", coatPalette[0], coatPalette);
            if (maneColorPicker != null) maneColorPicker.Setup("Couleur Crins", Color.white, coatPalette);
            if (hoofColorPicker != null) hoofColorPicker.Setup("Couleur Sabots", new Color(0.25f, 0.22f, 0.20f));
            if (eyeColorPicker != null) eyeColorPicker.Setup("Couleur des Yeux", new Color(0.2f, 0.45f, 0.85f));
            if (markColorPicker != null) markColorPicker.Setup("Couleur des Balzanes", Color.white);
            if (patternColorPicker != null) patternColorPicker.Setup("Couleur du Motif", Color.white);
            if (nuanceColorPicker != null) nuanceColorPicker.Setup("Couleur de la Nuance", new Color(0.92f, 0.75f, 0.55f));
            if (hornColorPicker != null) hornColorPicker.Setup("Couleur de Corne", new Color(0.98f, 0.88f, 0.40f));
            if (wingColorPicker != null) wingColorPicker.Setup("Couleur des Ailes", new Color(0.95f, 0.95f, 0.98f));
        }

        private void BindEvents()
        {
            // Sélecteurs de meshes
            if (bodyTypeSelector != null)
                bodyTypeSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetBodyType((BodyType)idx); };
            if (headTypeSelector != null)
                headTypeSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetHeadType((HeadType)idx); };
            if (hoofTypeSelector != null)
                hoofTypeSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetHoofType((HoofType)idx); };
            if (maneTypeSelector != null)
                maneTypeSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetManeType((ManeType)idx); };
            if (tailTypeSelector != null)
                tailTypeSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetTailType((TailType)idx); };
            if (hornTypeSelector != null)
                hornTypeSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetHornType((HornType)idx); };
            if (wingTypeSelector != null)
                wingTypeSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetWingType((WingType)idx); };

            // Sélecteurs de masques
            if (faceMarkSelector != null)
                faceMarkSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetFaceMark((FaceMarkType)idx); };
            if (bodyPatternSelector != null)
                bodyPatternSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetBodyPattern((BodyPatternType)idx); };
            if (coatNuanceSelector != null)
                coatNuanceSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetCoatNuance((CoatNuanceType)idx); };

            // Synchronisation des membres
            if (syncLegsToggle != null)
            {
                syncLegsToggle.onValueChanged.AddListener(sync =>
                {
                    if (syncLegsPanel != null) syncLegsPanel.SetActive(sync);
                    if (individualLegsPanel != null) individualLegsPanel.SetActive(!sync);
                    if (!isSyncingUI) manager.SetLegSync(sync);
                });
            }

            if (allLegsMarkSelector != null)
                allLegsMarkSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetAllLegMarks((LegMarkType)idx); };
            if (frontLeftLegSelector != null)
                frontLeftLegSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetLegMark(LegSlot.FrontLeft, (LegMarkType)idx); };
            if (frontRightLegSelector != null)
                frontRightLegSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetLegMark(LegSlot.FrontRight, (LegMarkType)idx); };
            if (backLeftLegSelector != null)
                backLeftLegSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetLegMark(LegSlot.BackLeft, (LegMarkType)idx); };
            if (backRightLegSelector != null)
                backRightLegSelector.OnValueChanged += idx => { if (!isSyncingUI) manager.SetLegMark(LegSlot.BackRight, (LegMarkType)idx); };

            // Sélecteurs de couleur
            if (bodyColorPicker != null)
                bodyColorPicker.OnColorChanged += col => { if (!isSyncingUI) manager.SetBodyColor(col); };
            if (maneColorPicker != null)
                maneColorPicker.OnColorChanged += col => { if (!isSyncingUI) { manager.SetManeColor(col); manager.SetTailColor(col); } };
            if (hoofColorPicker != null)
                hoofColorPicker.OnColorChanged += col => { if (!isSyncingUI) manager.SetHoofColor(col); };
            if (eyeColorPicker != null)
                eyeColorPicker.OnColorChanged += col => { if (!isSyncingUI) manager.SetEyeColor(col); };
            if (markColorPicker != null)
                markColorPicker.OnColorChanged += col => { if (!isSyncingUI) manager.SetMarkColor(col); };
            if (patternColorPicker != null)
                patternColorPicker.OnColorChanged += col => { if (!isSyncingUI) manager.SetBodyPatternColor(col); };
            if (nuanceColorPicker != null)
                nuanceColorPicker.OnColorChanged += col => { if (!isSyncingUI) manager.SetCoatNuanceColor(col); };
            if (hornColorPicker != null)
                hornColorPicker.OnColorChanged += col => { if (!isSyncingUI) manager.SetHornColor(col); };
            if (wingColorPicker != null)
                wingColorPicker.OnColorChanged += col => { if (!isSyncingUI) manager.SetWingColor(col); };

            // Boutons d'action globale
            if (randomizeButton != null)
                randomizeButton.onClick.AddListener(OnRandomizeClicked);
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveClicked);
            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadClicked);

            if (turntableToggle != null)
            {
                turntableToggle.onValueChanged.AddListener(val =>
                {
                    if (orbitCamera != null) orbitCamera.AutoRotate = val;
                });
            }
        }

        private void SyncUIWithData(PonyData data)
        {
            if (data == null) return;
            isSyncingUI = true;

            // Sélecteurs d'options
            if (bodyTypeSelector != null) bodyTypeSelector.SetIndex((int)data.bodyType, false);
            if (headTypeSelector != null) headTypeSelector.SetIndex((int)data.headType, false);
            if (hoofTypeSelector != null) hoofTypeSelector.SetIndex((int)data.hoofType, false);
            if (maneTypeSelector != null) maneTypeSelector.SetIndex((int)data.maneType, false);
            if (tailTypeSelector != null) tailTypeSelector.SetIndex((int)data.tailType, false);
            if (hornTypeSelector != null) hornTypeSelector.SetIndex((int)data.hornType, false);
            if (wingTypeSelector != null) wingTypeSelector.SetIndex((int)data.wingType, false);

            if (faceMarkSelector != null) faceMarkSelector.SetIndex((int)data.faceMark, false);
            if (bodyPatternSelector != null) bodyPatternSelector.SetIndex((int)data.bodyPattern, false);
            if (coatNuanceSelector != null) coatNuanceSelector.SetIndex((int)data.coatNuance, false);

            if (syncLegsToggle != null) syncLegsToggle.isOn = data.syncAllLegs;
            if (syncLegsPanel != null) syncLegsPanel.SetActive(data.syncAllLegs);
            if (individualLegsPanel != null) individualLegsPanel.SetActive(!data.syncAllLegs);

            if (allLegsMarkSelector != null) allLegsMarkSelector.SetIndex((int)data.legMarkFrontLeft, false);
            if (frontLeftLegSelector != null) frontLeftLegSelector.SetIndex((int)data.legMarkFrontLeft, false);
            if (frontRightLegSelector != null) frontRightLegSelector.SetIndex((int)data.legMarkFrontRight, false);
            if (backLeftLegSelector != null) backLeftLegSelector.SetIndex((int)data.legMarkBackLeft, false);
            if (backRightLegSelector != null) backRightLegSelector.SetIndex((int)data.legMarkBackRight, false);

            // Couleurs
            if (bodyColorPicker != null) bodyColorPicker.SetColor(data.bodyColor, false);
            if (maneColorPicker != null) maneColorPicker.SetColor(data.maneColor, false);
            if (hoofColorPicker != null) hoofColorPicker.SetColor(data.hoofColor, false);
            if (eyeColorPicker != null) eyeColorPicker.SetColor(data.eyeColor, false);
            if (markColorPicker != null) markColorPicker.SetColor(data.markColor, false);
            if (patternColorPicker != null) patternColorPicker.SetColor(data.patternColor, false);
            if (nuanceColorPicker != null) nuanceColorPicker.SetColor(data.nuanceColor, false);
            if (hornColorPicker != null) hornColorPicker.SetColor(data.hornColor, false);
            if (wingColorPicker != null) wingColorPicker.SetColor(data.wingColor, false);

            isSyncingUI = false;
        }

        private void OnRandomizeClicked()
        {
            manager.RandomizePony();
            ShowStatusMessage("Poney aléatoire généré !");
        }

        private void OnResetClicked()
        {
            manager.ResetToDefaults();
            ShowStatusMessage("Configuration réinitialisée !");
        }

        private void OnSaveClicked()
        {
            manager.SavePony();
            ShowStatusMessage("Configuration sauvegardée !");
        }

        private void OnLoadClicked()
        {
            manager.LoadPony();
            ShowStatusMessage("Configuration chargée !");
        }

        private void ShowStatusMessage(string message)
        {
            if (statusNotificationLabel != null)
            {
                statusNotificationLabel.text = message;
                CancelInvoke(nameof(ClearStatusMessage));
                Invoke(nameof(ClearStatusMessage), 3.0f);
            }
        }

        private void ClearStatusMessage()
        {
            if (statusNotificationLabel != null)
            {
                statusNotificationLabel.text = "";
            }
        }
    }
}

