using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MangoPony.UI
{
    /// <summary>
    /// Composant UI de sélection de couleur interactif avec curseurs RGB,
    /// palette de raccourcis prédéfinis et prévisualisation instantanée.
    /// </summary>
    public class UIFlexibleColorPicker : MonoBehaviour
    {
        [Header("UI Header")]
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private Image previewImage;
        [SerializeField] private TextMeshProUGUI hexCodeLabel;

        [Header("RGB Sliders")]
        [SerializeField] private Slider redSlider;
        [SerializeField] private Slider greenSlider;
        [SerializeField] private Slider blueSlider;

        [Header("Quick Palette (Optional)")]
        [SerializeField] private Transform paletteContainer;
        [SerializeField] private GameObject paletteButtonPrefab;

        private Color currentColor = Color.white;
        private bool isInternalUpdate = false;

        public event Action<Color> OnColorChanged;
        public Color CurrentColor => currentColor;

        private void Awake()
        {
            if (redSlider != null)
                redSlider.onValueChanged.AddListener(_ => OnSliderChanged());
            if (greenSlider != null)
                greenSlider.onValueChanged.AddListener(_ => OnSliderChanged());
            if (blueSlider != null)
                blueSlider.onValueChanged.AddListener(_ => OnSliderChanged());
        }

        public void Setup(string title, Color initialColor, Color[] quickPalette = null)
        {
            if (titleLabel != null && !string.IsNullOrEmpty(title))
                titleLabel.text = title;

            SetColor(initialColor, false);

            if (quickPalette != null && quickPalette.Length > 0 && paletteContainer != null)
            {
                PopulatePalette(quickPalette);
            }
        }

        public void SetColor(Color color, bool triggerCallback = true)
        {
            isInternalUpdate = true;
            currentColor = color;

            if (redSlider != null) redSlider.value = color.r;
            if (greenSlider != null) greenSlider.value = color.g;
            if (blueSlider != null) blueSlider.value = color.b;

            UpdateVisuals();
            isInternalUpdate = false;

            if (triggerCallback)
            {
                OnColorChanged?.Invoke(currentColor);
            }
        }

        private void OnSliderChanged()
        {
            if (isInternalUpdate) return;

            float r = redSlider != null ? redSlider.value : currentColor.r;
            float g = greenSlider != null ? greenSlider.value : currentColor.g;
            float b = blueSlider != null ? blueSlider.value : currentColor.b;

            currentColor = new Color(r, g, b, 1.0f);
            UpdateVisuals();

            OnColorChanged?.Invoke(currentColor);
        }

        private void UpdateVisuals()
        {
            if (previewImage != null)
            {
                previewImage.color = currentColor;
            }

            if (hexCodeLabel != null)
            {
                hexCodeLabel.text = "#" + ColorUtility.ToHtmlStringRGB(currentColor);
            }
        }

        private void PopulatePalette(Color[] palette)
        {
            // Nettoyage ancien contenu
            for (int i = paletteContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(paletteContainer.GetChild(i).gameObject);
            }

            foreach (var col in palette)
            {
                GameObject btnObj;
                if (paletteButtonPrefab != null)
                {
                    btnObj = Instantiate(paletteButtonPrefab, paletteContainer);
                }
                else
                {
                    btnObj = new GameObject("Swatch", typeof(RectTransform), typeof(Image), typeof(Button));
                    btnObj.transform.SetParent(paletteContainer, false);
                    var rt = btnObj.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(30, 30);
                }

                Image img = btnObj.GetComponent<Image>();
                if (img != null) img.color = col;

                Button btn = btnObj.GetComponent<Button>();
                Color targetCol = col;
                if (btn != null)
                {
                    btn.onClick.AddListener(() => SetColor(targetCol, true));
                }
            }
        }
    }
}

