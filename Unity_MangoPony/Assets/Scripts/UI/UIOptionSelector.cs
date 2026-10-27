using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MangoPony.UI
{
    /// <summary>
    /// Composant UI générique de sélection carrousel (flèches Gauche / Droite + libellé central).
    /// </summary>
    public class UIOptionSelector : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI valueLabel;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        private readonly List<string> options = new List<string>();
        private int currentIndex = 0;

        public event Action<int> OnValueChanged;

        public int CurrentIndex => currentIndex;
        public int OptionCount => options.Count;

        private void Awake()
        {
            if (prevButton != null)
                prevButton.onClick.AddListener(SelectPrevious);
            if (nextButton != null)
                nextButton.onClick.AddListener(SelectNext);
        }

        public void Setup(string title, IEnumerable<string> newOptions, int initialIndex = 0)
        {
            if (titleLabel != null && !string.IsNullOrEmpty(title))
                titleLabel.text = title;

            options.Clear();
            if (newOptions != null)
            {
                options.AddRange(newOptions);
            }

            if (options.Count == 0)
            {
                options.Add("Aucun");
            }

            SetIndex(Mathf.Clamp(initialIndex, 0, options.Count - 1), false);
        }

        public void SetIndex(int newIndex, bool triggerCallback = true)
        {
            if (options.Count == 0) return;

            currentIndex = Mathf.Clamp(newIndex, 0, options.Count - 1);
            UpdateDisplay();

            if (triggerCallback)
            {
                OnValueChanged?.Invoke(currentIndex);
            }
        }

        public void SelectPrevious()
        {
            if (options.Count <= 1) return;
            int newIndex = (currentIndex - 1 + options.Count) % options.Count;
            SetIndex(newIndex, true);
        }

        public void SelectNext()
        {
            if (options.Count <= 1) return;
            int newIndex = (currentIndex + 1) % options.Count;
            SetIndex(newIndex, true);
        }

        private void UpdateDisplay()
        {
            if (valueLabel != null && options.Count > 0)
            {
                valueLabel.text = options[currentIndex];
            }
        }
    }
}

