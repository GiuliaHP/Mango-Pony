using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MangoPony.CameraControl;
using MangoPony.Core;

namespace MangoPony.UI
{
    [Serializable]
    public class CustomizerTabEntry
    {
        public string tabName;
        public Button tabButton;
        public GameObject tabPanel;
        public CameraFocusTarget cameraFocusTarget = CameraFocusTarget.WholeBody;
    }

    /// <summary>
    /// Contrôleur de navigation par onglets pour l'interface de personnalisation.
    /// Aligne les panneaux de customisation et déclenche le recentrage de la caméra 3D.
    /// </summary>
    public class PonyTabController : MonoBehaviour
    {
        [Header("Tabs Configuration")]
        [SerializeField] private List<CustomizerTabEntry> tabs = new List<CustomizerTabEntry>();
        [SerializeField] private Color activeTabColor = new Color(0.95f, 0.60f, 0.35f, 1f); // Orange mangue
        [SerializeField] private Color inactiveTabColor = new Color(0.35f, 0.35f, 0.40f, 0.8f);

        [Header("Camera Link (Optional)")]
        [SerializeField] private PonyOrbitCamera orbitCamera;

        private int activeTabIndex = 0;

        public event Action<int> OnTabChanged;

        private void Start()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                if (tabs[i].tabButton != null)
                {
                    tabs[i].tabButton.onClick.AddListener(() => SwitchToTab(index));
                }
            }

            SwitchToTab(0);
        }

        public void SwitchToTab(int index)
        {
            if (index < 0 || index >= tabs.Count) return;

            activeTabIndex = index;

            for (int i = 0; i < tabs.Count; i++)
            {
                bool isActive = (i == index);

                // Activer/Désactiver le panneau
                if (tabs[i].tabPanel != null)
                {
                    tabs[i].tabPanel.SetActive(isActive);
                }

                // Mettre à jour la couleur du bouton
                if (tabs[i].tabButton != null)
                {
                    var colors = tabs[i].tabButton.colors;
                    colors.normalColor = isActive ? activeTabColor : inactiveTabColor;
                    tabs[i].tabButton.colors = colors;

                    // Si Image principale
                    var img = tabs[i].tabButton.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = isActive ? activeTabColor : inactiveTabColor;
                    }
                }
            }

            // Recadrage caméra automatique si configuré
            if (orbitCamera != null)
            {
                orbitCamera.FocusOn(tabs[index].cameraFocusTarget);
            }

            OnTabChanged?.Invoke(activeTabIndex);
        }
    }
}

