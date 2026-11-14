using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizationUIGenerator : MonoBehaviour
{
    [Header("1. Données des Catégories (ScriptableObjects)")]
    [Tooltip("Liste des catégories à générer dans l'ordre des onglets (Body, Head, Hair, Accessories...)")]
    public List<CustomizationCategoryData> categories = new List<CustomizationCategoryData>();

    [Header("2. Conteneurs cibles dans le Canvas")]
    [Tooltip("Le parent (HorizontalLayoutGroup) qui recevra les boutons d'onglets")]
    public Transform tabsContainer;

    [Tooltip("Le parent qui recevra les panneaux de chaque catégorie")]
    public Transform panelsContainer;

    [Header("3. Prefabs de Structure (Onglets, Panneaux, Sections)")]
    [Tooltip("Prefab du bouton d'onglet (doit contenir un Button, et idéalement un TextMeshProUGUI / Image)")]
    public GameObject tabButtonPrefab;

    [Tooltip("Prefab du panneau d'une catégorie (ex: ScrollView -> Viewport -> Content)")]
    public GameObject categoryPanelPrefab;

    [Tooltip("Optionnel : Chemin relatif vers le Content du ScrollView dans categoryPanelPrefab (ex: 'Viewport/Content'). Laisser vide si le panel est directement le conteneur.")]
    public string panelContentPath = "Viewport/Content";

    [Tooltip("Prefab d'un bloc d'élément (ex: un conteneur vertical pour 'Crinière' avec un titre et une grille)")]
    public GameObject elementSectionPrefab;

    [Tooltip("Optionnel : Chemin relatif vers la grille de boutons dans elementSectionPrefab (ex: 'ShapesGrid'). Laisser vide pour instancier directement dans la section.")]
    public string shapesGridPath = "ShapesGrid";

    [Tooltip("Optionnel : Chemin relatif vers le conteneur des couleurs dans elementSectionPrefab (ex: 'ColorsContainer'). Laisser vide pour instancier à la suite.")]
    public string colorsContainerPath = "ColorsContainer";

    [Header("4. Prefabs de Boutons & Color Picker (Par défaut)")]
    [Tooltip("Prefab contenant MeshPreviewButton + Button + Image")]
    public GameObject meshPreviewButtonPrefab;

    [Tooltip("Prefab contenant PatternPreviewButton + Button + Image")]
    public GameObject patternPreviewButtonPrefab;

    [Tooltip("Prefab contenant ColorSwatchButton + Button + Image")]
    public GameObject colorSwatchButtonPrefab;

    [Tooltip("Prefab contenant CustomColorWheel")]
    public GameObject colorWheelPrefab;

    [Header("5. Options de Génération")]
    [Tooltip("Désactive automatiquement tous les panneaux sauf le premier après génération")]
    public bool hideInactivePanels = true;
}