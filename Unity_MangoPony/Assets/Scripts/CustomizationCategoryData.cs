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
        [Header("1. Titre de l'élément")]
        public string elementName;

        [Header("2. Lien direct avec le Poney (Swapper)")]
        public Swapper.SlotType targetMeshSlot;
        public Swapper.MaterialZone targetMaterialZone;
        public bool supportsFourLegsSync = false;

        [Header("3. Sous-section Shapes (Meshes 3D)")]
        public string shapesSubtitle = "Shapes";
        [Tooltip("Index de la shape équipée par défaut au lancement (-1 pour rien)")]
        public int defaultShapeIndex = 0;
        public List<MeshShapeOption> shapes = new List<MeshShapeOption>();

        [Header("4. Sous-section Patterns (Motifs Shader)")]
        public string patternsSubtitle = "Patterns";
        [Tooltip("Index du pattern appliqué par défaut au lancement (-1 pour rien)")]
        public int defaultPatternIndex = 0;
        public List<PatternOption> patterns = new List<PatternOption>();

        [Header("5. Sous-section Colors")]
        public string colorsSubtitle = "Colors";
        public ColorSection colorSettings = new ColorSection();
    }

    [System.Serializable]
    public class MeshShapeOption
    {
        public string shapeName;
        public Sprite previewIcon;
        public GameObject meshPrefab;
    }

    [System.Serializable]
    public class PatternOption
    {
        public string patternName;
        public Sprite previewIcon;
        public Texture patternTexture;
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