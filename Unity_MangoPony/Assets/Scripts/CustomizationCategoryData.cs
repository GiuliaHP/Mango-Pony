using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCustomizationCategory", menuName = "Mango Pony/Customization/Category Data")]
public class CustomizationCategoryData : ScriptableObject
{
    public enum CameraFocusZone
    {
        Fullbody,
        Head,
        Legs,
        Rump
    }

    public enum ElementContentType
    {
        MeshOnly,       // Ex: Body Shape, Head Shape, Hooves
        ShaderOnly,     // Ex: Color Repartition, Body Pattern, Head Pattern, Legs Pattern
        MeshAndShader   // Ex: Mane, Tail, Horn, Wings (Shapes + Couleurs/Highlights)
    }

    #region 1. Identité de la Catégorie (Onglet UI)

    [Header("Identité de la Catégorie")]
    [Tooltip("Nom affiché sur l'onglet (ex: Body, Features)")]
    public string categoryName = "New Category";

    [Tooltip("Icône optionnelle pour l'onglet de navigation")]
    public Sprite categoryIcon;

    [Tooltip("Caméra par défaut à l'ouverture de cet onglet")]
    public CameraFocusZone defaultCategoryCamera = CameraFocusZone.Fullbody;

    #endregion

    #region 2. Éléments de la Catégorie

    [Header("Éléments de la Catégorie")]
    public List<CustomizationElement> elements = new List<CustomizationElement>();

    #endregion

    [System.Serializable]
    public class CustomizationElement
    {
        [Header("1. Identité & Caméra")]
        public string elementName;
        public ElementContentType contentType = ElementContentType.MeshOnly;
        [Tooltip("Caméra activée lorsqu'on clique sur une option de cet élément")]
        public CameraFocusZone cameraFocus = CameraFocusZone.Fullbody;

        [Header("2. Meshes 3D (Shapes principales)")]
        public Swapper.SlotType targetMeshSlot;
        public string shapesSubtitle = "Shapes";
        [Tooltip("Index de la shape équipée par défaut au lancement (-1 pour rien)")]
        public int defaultShapeIndex = 0;
        public List<MeshShapeOption> shapes = new List<MeshShapeOption>();

        [Header("2b. Meshes 3D secondaires (ex: Forelock dans Mane)")]
        public bool hasSecondaryShapes = false;
        public Swapper.SlotType secondaryMeshSlot = Swapper.SlotType.Forelock;
        public string secondaryShapesSubtitle = "Forelock";
        public int defaultSecondaryShapeIndex = 0;
        public List<MeshShapeOption> secondaryShapes = new List<MeshShapeOption>();

        [Header("3. Shader & Patterns")]
        public Swapper.MaterialZone targetMaterialZone;
        [Tooltip("Coche uniquement pour 'Legs Pattern' : affiche le sélecteur 4 Legs / Front-Back / Individual")]
        public bool isLegsPatternElement = false;
        public string patternsSubtitle = "Patterns";
        [Tooltip("Index du pattern appliqué par défaut au lancement (-1 pour rien)")]
        public int defaultPatternIndex = 0;
        public List<PatternOption> patterns = new List<PatternOption>();

        [Header("4. Couleurs")]
        public string colorsSubtitle = "Colors";
        public ColorSection colorSettings = new ColorSection();
    }

    [System.Serializable]
    public class MeshShapeOption
    {
        public string shapeName;
        [HideInInspector] public Sprite previewIcon;
        [HideInInspector] public GameObject meshPrefab;
    }

    [System.Serializable]
    public class PatternOption
    {
        public string patternName;
        [HideInInspector] public Sprite previewIcon;
        [HideInInspector] public Texture patternTexture;
    }

    [System.Serializable]
    public class ColorSection
    {
        public bool hasColors = true;
        public GameObject colorButtonPrefab;
        public GameObject colorPickerPrefab;
        public List<ColorChannel> colorChannels = new List<ColorChannel>();
    }

    [System.Serializable]
    public class ColorChannel
    {
        [Tooltip("Nom affiché à côté du bouton (ex: Color, Highlights Color, Coat Color, Variant Color...)")]
        public string colorName = "Color";
        [Tooltip("Nom exact de la variable Color dans Shader Graph (ex: _BaseColor, _PatternColor)")]
        public string shaderPropertyName = "_BaseColor";
        public Color defaultColor = Color.white;
    }
}