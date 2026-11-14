using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCustomizationCategory", menuName = "Mango Pony/Customization/Category Data")]
public class CustomizationCategoryData : ScriptableObject
{
    #region 1. Identité de la Catégorie (Onglet UI)

    [Header("Identité de la Catégorie")]
    [Tooltip("Nom affiché sur l'onglet (ex: Head, Body, Hair, Accessories)")]
    public string categoryName = "New Category";

    [Tooltip("Icône optionnelle pour l'onglet de navigation")]
    public Sprite categoryIcon;

    #endregion

    #region 2. Éléments de la Catégorie

    [Header("Éléments de la Catégorie")]
    [Tooltip("Liste des sous-éléments personnalisables (ex: Hooves, Head, Eyes, Mane, Coat...)")]
    public List<CustomizationElement> elements = new List<CustomizationElement>();

    #endregion

    // =========================================================================
    // STRUCTURES DE DONNÉES IMBRIQUÉES (Inspecteur propre et repliable)
    // =========================================================================

    [System.Serializable]
    public class CustomizationElement
    {
        [Header("Informations Générales")]
        [Tooltip("Nom de l'élément affiché dans l'UI (ex: Crinière, Sabots, Marques du corps...)")]
        public string elementName;

        [Header("Zone & Cibles dans le Swapper")]
        [Tooltip("Type de modification que pilote cet élément")]
        public ElementModificationType modificationType = ElementModificationType.MeshAndMaterial;

        [Tooltip("Emplacement de Mesh concerné (si cet élément change un prefab 3D)")]
        public Swapper.SlotType targetMeshSlot;

        [Tooltip("Zone de Material concernée (si cet élément change un motif ou une couleur)")]
        public Swapper.MaterialZone targetMaterialZone;

        [Tooltip("Nom du Bone / Transform cible (utile pour le debug, l'auto-binding ou le focus caméra)")]
        public string targetBoneName;

        [Tooltip("Permet d'afficher un toggle 'Appliquer aux 4 pattes' dans l'UI pour cet élément")]
        public bool supportsFourLegsSync = false;

        [Header("Variations / Shapes Associées")]
        [Tooltip("Liste des formes, meshes ou motifs disponibles pour cet élément")]
        public List<ShapeOption> shapes = new List<ShapeOption>();

        [Header("Configuration des Couleurs")]
        public ColorSection colorSettings = new ColorSection();
    }

    public enum ElementModificationType
    {
        MeshOnly,           // Uniquement un modèle 3D (ex: Cornes, Ailes)
        MaterialPatternOnly,// Uniquement une texture/masque de shader (ex: Marques du front, Pommelé)
        ColorOnly,          // Uniquement des couleurs (ex: Yeux si pas de changement de forme)
        MeshAndMaterial     // Combine formes 3D + motifs/couleurs (ex: Crinière, Corps)
    }

    [System.Serializable]
    public class ShapeOption
    {
        [Tooltip("Nom de la variation (ex: Round, Big, Long, Tresse, Étoile, Aucune...)")]
        public string shapeName;

        [Tooltip("Icône affichée sur le bouton de prévisualisation")]
        public Sprite previewIcon;

        [Header("Donnée Appliquée (Remplir selon le type d'élément)")]
        [Tooltip("Prefab 3D à instancier (laisser vide si c'est une option 'Aucune' ou un motif 2D)")]
        public GameObject meshPrefab;

        [Tooltip("Texture/Masque de motif à envoyer au Shader (laisser vide si c'est un mesh 3D)")]
        public Texture patternTexture;
    }

    [System.Serializable]
    public class ColorSection
    {
        [Tooltip("Activer ou non la section couleur pour cet élément")]
        public bool hasColors = true;

        [Header("Prefabs UI à instancier automatiquement")]
        [Tooltip("Prefab contenant le composant ColorSwatchButton")]
        public GameObject colorButtonPrefab;

        [Tooltip("Prefab contenant le composant CustomColorWheel")]
        public GameObject colorPickerPrefab;

        [Header("Canaux de Couleurs Éditables")]
        [Tooltip("Liste des couleurs (ex: Main Color, Pattern Color, Highlight...)")]
        public List<ColorChannel> colorChannels = new List<ColorChannel>();
    }

    [System.Serializable]
    public class ColorChannel
    {
        [Tooltip("Nom affiché à côté du bouton (ex: Main Color, Pattern Color...)")]
        public string colorName = "Main Color";

        [Tooltip("Nom exact de la variable Color dans Shader Graph (ex: _BaseColor, _PatternColor)")]
        public string shaderPropertyName = "_BaseColor";

        [Tooltip("Couleur par défaut appliquée au démarrage")]
        public Color defaultColor = Color.white;
    }
}