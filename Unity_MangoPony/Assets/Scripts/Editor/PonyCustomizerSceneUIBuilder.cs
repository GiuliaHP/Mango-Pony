#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using MangoPony.Core;
using MangoPony.CameraControl;
using MangoPony.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace MangoPony.Editor
{
    /// <summary>
    /// Générateur procédural de la hiérarchie UI complète du Customizer pour la scène de test.
    /// Crée un Canvas UGUI / TMPro moderne, réactif et directement fonctionnel.
    /// </summary>
    public static class PonyCustomizerSceneUIBuilder
    {
        public static GameObject BuildCustomizerCanvas(GameObject ponyTarget, PonyOrbitCamera orbitCamera)
        {
            // 1. EventSystem
            EnsureEventSystem();

            // 2. Canvas racine
            GameObject canvasObj = new GameObject("Customizer_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 3. Panneau latéral droit (Sidebar 420px de large)
            GameObject sidebar = CreateUIObject("Sidebar", canvasObj.transform);
            RectTransform sidebarRT = sidebar.GetComponent<RectTransform>();
            sidebarRT.anchorMin = new Vector2(1f, 0f);
            sidebarRT.anchorMax = new Vector2(1f, 1f);
            sidebarRT.pivot = new Vector2(1f, 0.5f);
            sidebarRT.sizeDelta = new Vector2(460, 0);

            Image sidebarBg = sidebar.AddComponent<Image>();
            sidebarBg.color = new Color(0.12f, 0.13f, 0.16f, 0.92f); // Sombre moderne translucide

            // Header Titre
            GameObject header = CreateUIObject("Header", sidebar.transform);
            RectTransform headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(0, 70);
            headerRT.anchoredPosition = new Vector2(0, 0);

            TextMeshProUGUI titleText = CreateTMPLabel(header.transform, "MANGO PONY", 28, FontStyles.Bold, new Color(1f, 0.65f, 0.30f));
            titleText.alignment = TextAlignmentOptions.Center;

            // Barre d'onglets (Navigation Tabs)
            GameObject tabGroup = CreateUIObject("TabBar", sidebar.transform);
            RectTransform tabGroupRT = tabGroup.GetComponent<RectTransform>();
            tabGroupRT.anchorMin = new Vector2(0f, 1f);
            tabGroupRT.anchorMax = new Vector2(1f, 1f);
            tabGroupRT.pivot = new Vector2(0.5f, 1f);
            tabGroupRT.sizeDelta = new Vector2(-20, 44);
            tabGroupRT.anchoredPosition = new Vector2(0, -75);

            HorizontalLayoutGroup tabHLG = tabGroup.AddComponent<HorizontalLayoutGroup>();
            tabHLG.spacing = 6;
            tabHLG.childControlWidth = true;
            tabHLG.childControlHeight = true;
            tabHLG.childForceExpandWidth = true;

            Button btnTab1 = CreateTabButton(tabGroup.transform, "Corps");
            Button btnTab2 = CreateTabButton(tabGroup.transform, "Tête & Crins");
            Button btnTab3 = CreateTabButton(tabGroup.transform, "Marques");
            Button btnTab4 = CreateTabButton(tabGroup.transform, "Accessoires");

            // Conteneur des Panneaux
            GameObject panelsContainer = CreateUIObject("PanelsContainer", sidebar.transform);
            RectTransform pcRT = panelsContainer.GetComponent<RectTransform>();
            pcRT.anchorMin = new Vector2(0f, 0f);
            pcRT.anchorMax = new Vector2(1f, 1f);
            pcRT.offsetMin = new Vector2(15, 120); // Espace pour bottom bar
            pcRT.offsetMax = new Vector2(-15, -130);

            // Tab 1 : Corps
            GameObject panelCorps = CreateTabScrollPanel("Panel_Corps", panelsContainer.transform);
            UIOptionSelector selBody = CreateOptionSelector(panelCorps.transform, "Morphologie");
            UIOptionSelector selHoof = CreateOptionSelector(panelCorps.transform, "Sabots");
            UIOptionSelector selPattern = CreateOptionSelector(panelCorps.transform, "Motif Corporel");
            UIOptionSelector selNuance = CreateOptionSelector(panelCorps.transform, "Nuance de Robe");
            UIFlexibleColorPicker pickBody = CreateColorPicker(panelCorps.transform, "Couleur Robe");
            UIFlexibleColorPicker pickHoof = CreateColorPicker(panelCorps.transform, "Couleur Sabots");

            // Tab 2 : Tête & Crins
            GameObject panelTete = CreateTabScrollPanel("Panel_Tete", panelsContainer.transform);
            UIOptionSelector selHead = CreateOptionSelector(panelTete.transform, "Profil Tête");
            UIOptionSelector selFaceMark = CreateOptionSelector(panelTete.transform, "Marque en Tête");
            UIOptionSelector selMane = CreateOptionSelector(panelTete.transform, "Crinière");
            UIOptionSelector selTail = CreateOptionSelector(panelTete.transform, "Queue");
            UIFlexibleColorPicker pickMane = CreateColorPicker(panelTete.transform, "Couleur Crins & Queue");
            UIFlexibleColorPicker pickEye = CreateColorPicker(panelTete.transform, "Couleur des Yeux");

            // Tab 3 : Marques & Couleurs
            GameObject panelMarques = CreateTabScrollPanel("Panel_Marques", panelsContainer.transform);
            Toggle toggleSync = CreateToggle(panelMarques.transform, "Synchroniser les 4 membres", true);

            GameObject syncPanel = CreateUIObject("SyncPanel", panelMarques.transform);
            syncPanel.AddComponent<VerticalLayoutGroup>().spacing = 8;
            UIOptionSelector selAllLegs = CreateOptionSelector(syncPanel.transform, "Balzanes (Toutes)");

            GameObject indivPanel = CreateUIObject("IndivPanel", panelMarques.transform);
            indivPanel.AddComponent<VerticalLayoutGroup>().spacing = 8;
            indivPanel.SetActive(false);
            UIOptionSelector selFL = CreateOptionSelector(indivPanel.transform, "Antérieur Gauche");
            UIOptionSelector selFR = CreateOptionSelector(indivPanel.transform, "Antérieur Droit");
            UIOptionSelector selBL = CreateOptionSelector(indivPanel.transform, "Postérieur Gauche");
            UIOptionSelector selBR = CreateOptionSelector(indivPanel.transform, "Postérieur Droit");

            UIFlexibleColorPicker pickMark = CreateColorPicker(panelMarques.transform, "Couleur Marques");
            UIFlexibleColorPicker pickPattern = CreateColorPicker(panelMarques.transform, "Couleur Motif");
            UIFlexibleColorPicker pickNuanceCol = CreateColorPicker(panelMarques.transform, "Couleur Nuance");

            // Tab 4 : Accessoires
            GameObject panelAccessoires = CreateTabScrollPanel("Panel_Accessoires", panelsContainer.transform);
            UIOptionSelector selHorn = CreateOptionSelector(panelAccessoires.transform, "Corne");
            UIOptionSelector selWing = CreateOptionSelector(panelAccessoires.transform, "Ailes");
            UIFlexibleColorPicker pickHorn = CreateColorPicker(panelAccessoires.transform, "Couleur Corne");
            UIFlexibleColorPicker pickWing = CreateColorPicker(panelAccessoires.transform, "Couleur Ailes");

            // Barre d'actions inférieure (Random, Reset, Save, Load, Turntable)
            GameObject bottomBar = CreateUIObject("BottomBar", sidebar.transform);
            RectTransform bbRT = bottomBar.GetComponent<RectTransform>();
            bbRT.anchorMin = new Vector2(0f, 0f);
            bbRT.anchorMax = new Vector2(1f, 0f);
            bbRT.sizeDelta = new Vector2(-20, 105);
            bbRT.anchoredPosition = new Vector2(0, 55);

            GridLayoutGroup bbGrid = bottomBar.AddComponent<GridLayoutGroup>();
            bbGrid.cellSize = new Vector2(215, 36);
            bbGrid.spacing = new Vector2(10, 8);

            Button btnRandom = CreateActionButton(bottomBar.transform, "🎲 Aléatoire", new Color(0.9f, 0.45f, 0.25f));
            Button btnReset = CreateActionButton(bottomBar.transform, "↺ Défaut", new Color(0.4f, 0.45f, 0.55f));
            Button btnSave = CreateActionButton(bottomBar.transform, "💾 Sauvegarder", new Color(0.25f, 0.65f, 0.4f));
            Button btnLoad = CreateActionButton(bottomBar.transform, "📂 Charger", new Color(0.3f, 0.5f, 0.75f));

            // Barre de notification & Turntable
            GameObject notifBar = CreateUIObject("NotifBar", sidebar.transform);
            RectTransform nbRT = notifBar.GetComponent<RectTransform>();
            nbRT.anchorMin = new Vector2(0f, 0f);
            nbRT.anchorMax = new Vector2(1f, 0f);
            nbRT.sizeDelta = new Vector2(-20, 35);
            nbRT.anchoredPosition = new Vector2(0, 20);

            TextMeshProUGUI notifText = CreateTMPLabel(notifBar.transform, "", 14, FontStyles.Normal, Color.yellow);
            notifText.alignment = TextAlignmentOptions.Center;

            // Turntable toggle en haut à gauche de l'écran
            GameObject ttObj = CreateUIObject("TurntableToggle", canvasObj.transform);
            RectTransform ttRT = ttObj.GetComponent<RectTransform>();
            ttRT.anchorMin = new Vector2(0f, 1f);
            ttRT.anchorMax = new Vector2(0f, 1f);
            ttRT.pivot = new Vector2(0f, 1f);
            ttRT.anchoredPosition = new Vector2(20, -20);
            ttRT.sizeDelta = new Vector2(180, 40);
            Toggle toggleTurntable = CreateToggle(ttObj.transform, "Rotation 360°", false);

            // Tab Controller
            PonyTabController tabCtrl = sidebar.AddComponent<PonyTabController>();
            var serTabCtrl = new SerializedObject(tabCtrl);
            var tabsProp = serTabCtrl.FindProperty("tabs");
            tabsProp.arraySize = 4;

            SetupTabProp(tabsProp.GetArrayElementAtIndex(0), "Corps", btnTab1, panelCorps, CameraFocusTarget.WholeBody);
            SetupTabProp(tabsProp.GetArrayElementAtIndex(1), "Tête & Crins", btnTab2, panelTete, CameraFocusTarget.Head);
            SetupTabProp(tabsProp.GetArrayElementAtIndex(2), "Marques", btnTab3, panelMarques, CameraFocusTarget.Hooves);
            SetupTabProp(tabsProp.GetArrayElementAtIndex(3), "Accessoires", btnTab4, panelAccessoires, CameraFocusTarget.Wings);

            serTabCtrl.FindProperty("orbitCamera").objectReferenceValue = orbitCamera;
            serTabCtrl.ApplyModifiedProperties();

            // PonyCustomizerUI Main Binding
            PonyCustomizerUI customizerUI = canvasObj.AddComponent<PonyCustomizerUI>();
            var serUI = new SerializedObject(customizerUI);

            serUI.FindProperty("manager").objectReferenceValue = ponyTarget.GetComponent<PonyCustomizerManager>();
            serUI.FindProperty("orbitCamera").objectReferenceValue = orbitCamera;
            serUI.FindProperty("tabController").objectReferenceValue = tabCtrl;

            serUI.FindProperty("bodyTypeSelector").objectReferenceValue = selBody;
            serUI.FindProperty("hoofTypeSelector").objectReferenceValue = selHoof;
            serUI.FindProperty("bodyPatternSelector").objectReferenceValue = selPattern;
            serUI.FindProperty("coatNuanceSelector").objectReferenceValue = selNuance;
            serUI.FindProperty("bodyColorPicker").objectReferenceValue = pickBody;
            serUI.FindProperty("hoofColorPicker").objectReferenceValue = pickHoof;

            serUI.FindProperty("headTypeSelector").objectReferenceValue = selHead;
            serUI.FindProperty("faceMarkSelector").objectReferenceValue = selFaceMark;
            serUI.FindProperty("maneTypeSelector").objectReferenceValue = selMane;
            serUI.FindProperty("tailTypeSelector").objectReferenceValue = selTail;
            serUI.FindProperty("maneColorPicker").objectReferenceValue = pickMane;
            serUI.FindProperty("eyeColorPicker").objectReferenceValue = pickEye;

            serUI.FindProperty("syncLegsToggle").objectReferenceValue = toggleSync;
            serUI.FindProperty("syncLegsPanel").objectReferenceValue = syncPanel;
            serUI.FindProperty("individualLegsPanel").objectReferenceValue = indivPanel;
            serUI.FindProperty("allLegsMarkSelector").objectReferenceValue = selAllLegs;
            serUI.FindProperty("frontLeftLegSelector").objectReferenceValue = selFL;
            serUI.FindProperty("frontRightLegSelector").objectReferenceValue = selFR;
            serUI.FindProperty("backLeftLegSelector").objectReferenceValue = selBL;
            serUI.FindProperty("backRightLegSelector").objectReferenceValue = selBR;
            serUI.FindProperty("markColorPicker").objectReferenceValue = pickMark;
            serUI.FindProperty("patternColorPicker").objectReferenceValue = pickPattern;
            serUI.FindProperty("nuanceColorPicker").objectReferenceValue = pickNuanceCol;

            serUI.FindProperty("hornTypeSelector").objectReferenceValue = selHorn;
            serUI.FindProperty("wingTypeSelector").objectReferenceValue = selWing;
            serUI.FindProperty("hornColorPicker").objectReferenceValue = pickHorn;
            serUI.FindProperty("wingColorPicker").objectReferenceValue = pickWing;

            serUI.FindProperty("randomizeButton").objectReferenceValue = btnRandom;
            serUI.FindProperty("resetButton").objectReferenceValue = btnReset;
            serUI.FindProperty("saveButton").objectReferenceValue = btnSave;
            serUI.FindProperty("loadButton").objectReferenceValue = btnLoad;
            serUI.FindProperty("turntableToggle").objectReferenceValue = toggleTurntable;
            serUI.FindProperty("statusNotificationLabel").objectReferenceValue = notifText;

            serUI.ApplyModifiedProperties();

            return canvasObj;
        }

        #region Helper Creators

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
                esObj.AddComponent<InputSystemUIInputModule>();
#else
                esObj.AddComponent<StandaloneInputModule>();
#endif
            }
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static TextMeshProUGUI CreateTMPLabel(Transform parent, string text, float size, FontStyles style, Color color)
        {
            GameObject textObj = CreateUIObject("Label", parent);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateTabButton(Transform parent, string title)
        {
            GameObject btnObj = CreateUIObject($"Tab_{title}", parent);
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.26f, 0.30f);
            Button btn = btnObj.AddComponent<Button>();

            TextMeshProUGUI tmp = CreateTMPLabel(btnObj.transform, title, 14, FontStyles.Bold, Color.white);
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform tmpRT = tmp.GetComponent<RectTransform>();
            tmpRT.anchorMin = Vector2.zero;
            tmpRT.anchorMax = Vector2.one;
            tmpRT.offsetMin = Vector2.zero;
            tmpRT.offsetMax = Vector2.zero;

            return btn;
        }

        private static GameObject CreateTabScrollPanel(string name, Transform parent)
        {
            GameObject panel = CreateUIObject(name, parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = panel.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return panel;
        }

        private static UIOptionSelector CreateOptionSelector(Transform parent, string title)
        {
            GameObject obj = CreateUIObject($"Selector_{title}", parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 48);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.20f, 0.24f, 0.8f);

            // Titre en haut
            TextMeshProUGUI lblTitle = CreateTMPLabel(obj.transform, title, 12, FontStyles.Normal, new Color(0.8f, 0.8f, 0.85f));
            RectTransform lblTitleRT = lblTitle.GetComponent<RectTransform>();
            lblTitleRT.anchorMin = new Vector2(0f, 1f);
            lblTitleRT.anchorMax = new Vector2(1f, 1f);
            lblTitleRT.pivot = new Vector2(0f, 1f);
            lblTitleRT.anchoredPosition = new Vector2(10, -2);
            lblTitleRT.sizeDelta = new Vector2(-20, 16);

            // Bouton précédent (<)
            GameObject prevObj = CreateUIObject("PrevBtn", obj.transform);
            RectTransform prevRT = prevObj.GetComponent<RectTransform>();
            prevRT.anchorMin = new Vector2(0f, 0f);
            prevRT.anchorMax = new Vector2(0f, 0f);
            prevRT.anchoredPosition = new Vector2(20, 14);
            prevRT.sizeDelta = new Vector2(30, 24);
            prevObj.AddComponent<Image>().color = new Color(0.28f, 0.30f, 0.36f);
            Button btnPrev = prevObj.AddComponent<Button>();
            TextMeshProUGUI prevTxt = CreateTMPLabel(prevObj.transform, "◀", 12, FontStyles.Normal, Color.white);
            prevTxt.alignment = TextAlignmentOptions.Center;
            prevTxt.rectTransform.anchorMin = Vector2.zero;
            prevTxt.rectTransform.anchorMax = Vector2.one;
            prevTxt.rectTransform.offsetMin = Vector2.zero;
            prevTxt.rectTransform.offsetMax = Vector2.zero;

            // Bouton suivant (>)
            GameObject nextObj = CreateUIObject("NextBtn", obj.transform);
            RectTransform nextRT = nextObj.GetComponent<RectTransform>();
            nextRT.anchorMin = new Vector2(1f, 0f);
            nextRT.anchorMax = new Vector2(1f, 0f);
            nextRT.anchoredPosition = new Vector2(-20, 14);
            nextRT.sizeDelta = new Vector2(30, 24);
            nextObj.AddComponent<Image>().color = new Color(0.28f, 0.30f, 0.36f);
            Button btnNext = nextObj.AddComponent<Button>();
            TextMeshProUGUI nextTxt = CreateTMPLabel(nextObj.transform, "▶", 12, FontStyles.Normal, Color.white);
            nextTxt.alignment = TextAlignmentOptions.Center;
            nextTxt.rectTransform.anchorMin = Vector2.zero;
            nextTxt.rectTransform.anchorMax = Vector2.one;
            nextTxt.rectTransform.offsetMin = Vector2.zero;
            nextTxt.rectTransform.offsetMax = Vector2.zero;

            // Valeur au centre
            TextMeshProUGUI lblValue = CreateTMPLabel(obj.transform, "Option", 14, FontStyles.Bold, Color.white);
            lblValue.alignment = TextAlignmentOptions.Center;
            RectTransform valRT = lblValue.GetComponent<RectTransform>();
            valRT.anchorMin = new Vector2(0f, 0f);
            valRT.anchorMax = new Vector2(1f, 0f);
            valRT.anchoredPosition = new Vector2(0, 14);
            valRT.sizeDelta = new Vector2(-80, 24);

            UIOptionSelector selector = obj.AddComponent<UIOptionSelector>();
            var serSel = new SerializedObject(selector);
            serSel.FindProperty("titleLabel").objectReferenceValue = lblTitle;
            serSel.FindProperty("valueLabel").objectReferenceValue = lblValue;
            serSel.FindProperty("prevButton").objectReferenceValue = btnPrev;
            serSel.FindProperty("nextButton").objectReferenceValue = btnNext;
            serSel.ApplyModifiedProperties();

            return selector;
        }

        private static UIFlexibleColorPicker CreateColorPicker(Transform parent, string title)
        {
            GameObject obj = CreateUIObject($"ColorPicker_{title}", parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 95);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.17f, 0.20f, 0.9f);

            // Titre & Swatch
            TextMeshProUGUI lblTitle = CreateTMPLabel(obj.transform, title, 13, FontStyles.Bold, Color.white);
            RectTransform lblTitleRT = lblTitle.GetComponent<RectTransform>();
            lblTitleRT.anchorMin = new Vector2(0f, 1f);
            lblTitleRT.anchorMax = new Vector2(0.7f, 1f);
            lblTitleRT.pivot = new Vector2(0f, 1f);
            lblTitleRT.anchoredPosition = new Vector2(12, -8);
            lblTitleRT.sizeDelta = new Vector2(0, 20);

            // Swatch Preview
            GameObject swatchObj = CreateUIObject("SwatchPreview", obj.transform);
            RectTransform swatchRT = swatchObj.GetComponent<RectTransform>();
            swatchRT.anchorMin = new Vector2(1f, 1f);
            swatchRT.anchorMax = new Vector2(1f, 1f);
            swatchRT.pivot = new Vector2(1f, 1f);
            swatchRT.anchoredPosition = new Vector2(-12, -8);
            swatchRT.sizeDelta = new Vector2(28, 20);
            Image swatchImg = swatchObj.AddComponent<Image>();
            swatchImg.color = Color.white;

            // Slider R
            Slider sliderR = CreateSlider(obj.transform, "Slider_R", new Vector2(0, -32), new Color(0.9f, 0.3f, 0.3f));
            // Slider G
            Slider sliderG = CreateSlider(obj.transform, "Slider_G", new Vector2(0, -52), new Color(0.3f, 0.85f, 0.3f));
            // Slider B
            Slider sliderB = CreateSlider(obj.transform, "Slider_B", new Vector2(0, -72), new Color(0.3f, 0.5f, 0.95f));

            UIFlexibleColorPicker picker = obj.AddComponent<UIFlexibleColorPicker>();
            var serPicker = new SerializedObject(picker);
            serPicker.FindProperty("titleLabel").objectReferenceValue = lblTitle;
            serPicker.FindProperty("previewImage").objectReferenceValue = swatchImg;
            serPicker.FindProperty("redSlider").objectReferenceValue = sliderR;
            serPicker.FindProperty("greenSlider").objectReferenceValue = sliderG;
            serPicker.FindProperty("blueSlider").objectReferenceValue = sliderB;
            serPicker.ApplyModifiedProperties();

            return picker;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 pos, Color handleColor)
        {
            GameObject sliderObj = CreateUIObject(name, parent);
            RectTransform rt = sliderObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, pos.y);
            rt.sizeDelta = new Vector2(-24, 14);

            // Background
            GameObject bgObj = CreateUIObject("Background", sliderObj.transform);
            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = new Vector2(0, 3);
            bgRT.offsetMax = new Vector2(0, -3);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.12f);

            // Fill Area
            GameObject fillArea = CreateUIObject("Fill Area", sliderObj.transform);
            RectTransform faRT = fillArea.GetComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = new Vector2(0, 3);
            faRT.offsetMax = new Vector2(-10, -3);

            GameObject fill = CreateUIObject("Fill", fillArea.transform);
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = handleColor;

            // Handle Area
            GameObject handleArea = CreateUIObject("Handle Slide Area", sliderObj.transform);
            RectTransform haRT = handleArea.GetComponent<RectTransform>();
            haRT.anchorMin = Vector2.zero;
            haRT.anchorMax = Vector2.one;
            haRT.offsetMin = new Vector2(5, 0);
            haRT.offsetMax = new Vector2(-5, 0);

            GameObject handle = CreateUIObject("Handle", handleArea.transform);
            RectTransform handleRT = handle.GetComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(14, 14);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.fillRect = fillRT;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return slider;
        }

        private static Toggle CreateToggle(Transform parent, string label, bool defaultVal)
        {
            GameObject toggleObj = CreateUIObject("Toggle_" + label, parent);
            RectTransform rt = toggleObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 30);

            // Checkbox
            GameObject checkBg = CreateUIObject("Background", toggleObj.transform);
            RectTransform bgRT = checkBg.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0.5f);
            bgRT.anchorMax = new Vector2(0f, 0.5f);
            bgRT.anchoredPosition = new Vector2(12, 0);
            bgRT.sizeDelta = new Vector2(20, 20);
            Image bgImg = checkBg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.22f, 0.28f);

            GameObject checkmark = CreateUIObject("Checkmark", checkBg.transform);
            RectTransform cmRT = checkmark.GetComponent<RectTransform>();
            cmRT.anchorMin = Vector2.zero;
            cmRT.anchorMax = Vector2.one;
            cmRT.offsetMin = new Vector2(3, 3);
            cmRT.offsetMax = new Vector2(-3, -3);
            Image cmImg = checkmark.AddComponent<Image>();
            cmImg.color = new Color(1f, 0.65f, 0.3f);

            // Label
            TextMeshProUGUI tmp = CreateTMPLabel(toggleObj.transform, label, 13, FontStyles.Normal, Color.white);
            RectTransform tmpRT = tmp.GetComponent<RectTransform>();
            tmpRT.anchorMin = new Vector2(0f, 0f);
            tmpRT.anchorMax = new Vector2(1f, 1f);
            tmpRT.offsetMin = new Vector2(32, 0);
            tmpRT.offsetMax = Vector2.zero;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.graphic = cmImg;
            toggle.targetGraphic = bgImg;
            toggle.isOn = defaultVal;

            return toggle;
        }

        private static Button CreateActionButton(Transform parent, string title, Color col)
        {
            GameObject btnObj = CreateUIObject($"Btn_{title}", parent);
            Image img = btnObj.AddComponent<Image>();
            img.color = col;
            Button btn = btnObj.AddComponent<Button>();

            TextMeshProUGUI tmp = CreateTMPLabel(btnObj.transform, title, 14, FontStyles.Bold, Color.white);
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform tmpRT = tmp.GetComponent<RectTransform>();
            tmpRT.anchorMin = Vector2.zero;
            tmpRT.anchorMax = Vector2.one;
            tmpRT.offsetMin = Vector2.zero;
            tmpRT.offsetMax = Vector2.zero;

            return btn;
        }

        private static void SetupTabProp(SerializedProperty tabProp, string name, Button btn, GameObject panel, CameraFocusTarget focus)
        {
            tabProp.FindPropertyRelative("tabName").stringValue = name;
            tabProp.FindPropertyRelative("tabButton").objectReferenceValue = btn;
            tabProp.FindPropertyRelative("tabPanel").objectReferenceValue = panel;
            tabProp.FindPropertyRelative("cameraFocusTarget").enumValueIndex = (int)focus;
        }

        #endregion
    }
}
#endif
