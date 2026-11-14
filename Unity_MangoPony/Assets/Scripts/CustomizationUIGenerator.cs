using System.Collections.Generic;
using UnityEngine;

public class CustomizationUIGenerator : MonoBehaviour
{
    [Header("1. Données des Catégories")]
    public List<CustomizationCategoryData> categories = new List<CustomizationCategoryData>();

    [Header("2. Conteneurs du Canvas")]
    public Transform tabsContainer;
    public Transform panelsContainer;

    [Header("3. Prefabs de Structure (Ton orga d'origine)")]
    public GameObject tabButtonPrefab;
    [Tooltip("Ton Category Panel Prefab (ScrollView / VerticalLayout)")]
    public GameObject categoryPanelPrefab;
    [Tooltip("Chemin vers le Content dans le Category Panel Prefab (ex: 'Viewport/Content', laisser vide si racine)")]
    public string panelContentPath = "Viewport/Content";

    [Space(5)]
    [Tooltip("Prefab du Title (TextMeshProUGUI avec le nom de l'élément)")]
    public GameObject elementTitlePrefab;

    [Tooltip("Prefab du Subtitle (TextMeshProUGUI : 'Shapes', 'Patterns', 'Colors')")]
    public GameObject subtitlePrefab;

    [Tooltip("Prefab de grille (GridLayoutGroup) pour les boutons Shapes et Patterns")]
    public GameObject gridLayoutPrefab;

    [Tooltip("Ton prefab 'Layout_Colors' (contient déjà un layout pour les boutons de couleur)")]
    public GameObject layoutColorsPrefab;

    [Header("4. Prefabs de Boutons & Color Picker")]
    public GameObject meshPreviewButtonPrefab;
    public GameObject patternPreviewButtonPrefab;
    public GameObject colorSwatchButtonPrefab;
    public GameObject colorWheelPrefab;

    [Header("5. Options")]
    public bool hideInactivePanels = true;
}