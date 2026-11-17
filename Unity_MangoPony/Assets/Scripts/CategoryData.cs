using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewCategory", menuName = "Mango Pony/Category Data")]
public class CategoryData : ScriptableObject
{
    public enum ContentType
    {
        MeshOnly,
        ShaderOnly,
        MeshAndShader
    }

    [Header("Tab")]
    [FormerlySerializedAs("categoryName")] public string title = "New Category";
    [FormerlySerializedAs("categoryIcon")] public Sprite icon;
    [Tooltip("Camera used when this tab opens (empty = fullbody)")]
    [CameraId] public string defaultCamera;

    [Header("Elements")]
    public List<Element> elements = new List<Element>();

    [System.Serializable]
    public class Element
    {
        [Header("Identity")]
        [FormerlySerializedAs("elementName")] public string title;
        public ContentType contentType = ContentType.MeshOnly;
        [Tooltip("Camera used when an option of this element is clicked (empty = fullbody)")]
        [CameraId] public string camera;

        [Header("Shapes")]
        [FormerlySerializedAs("targetMeshSlot")] public PonyCustomizer.Slot slot;
        [FormerlySerializedAs("shapesSubtitle")] public string shapesLabel = "Shapes";
        [Tooltip("Shape equipped at start (-1 for none)")]
        public int defaultShapeIndex = 0;
        public List<ShapeOption> shapes = new List<ShapeOption>();

        [Header("Secondary Shapes")]
        public bool hasSecondaryShapes = false;
        [FormerlySerializedAs("secondaryMeshSlot")] public PonyCustomizer.Slot secondarySlot = PonyCustomizer.Slot.Forelock;
        [FormerlySerializedAs("secondaryShapesSubtitle")] public string secondaryShapesLabel = "Forelock";
        public int defaultSecondaryShapeIndex = 0;
        public List<ShapeOption> secondaryShapes = new List<ShapeOption>();

        [Header("Patterns")]
        [FormerlySerializedAs("targetMaterialZone")] public PonyCustomizer.Zone zone;
        [Tooltip("Shows the leg selector (All / Front-Back / Individual)")]
        [FormerlySerializedAs("isLegsPatternElement")] public bool isLegs = false;
        [FormerlySerializedAs("patternsSubtitle")] public string patternsLabel = "Patterns";
        [Tooltip("Pattern applied at start (-1 for none)")]
        public int defaultPatternIndex = 0;
        public List<PatternOption> patterns = new List<PatternOption>();

        [Header("Colors")]
        [FormerlySerializedAs("colorsSubtitle")] public string colorsLabel = "Colors";
        [FormerlySerializedAs("colorSettings")] public ColorSettings colors = new ColorSettings();
    }

    [System.Serializable]
    public class ShapeOption
    {
        [FormerlySerializedAs("shapeName")] public string label;
        [ReadOnly, FormerlySerializedAs("previewIcon")] public Sprite icon;
        [ReadOnly, FormerlySerializedAs("meshPrefab")] public GameObject prefab;
    }

    [System.Serializable]
    public class PatternOption
    {
        [FormerlySerializedAs("patternName")] public string label;
        [ReadOnly, FormerlySerializedAs("previewIcon")] public Sprite icon;
        [ReadOnly, FormerlySerializedAs("patternTexture")] public Texture texture;
    }

    [System.Serializable]
    public class ColorSettings
    {
        [FormerlySerializedAs("hasColors")] public bool enabled = true;
        [FormerlySerializedAs("colorButtonPrefab")] public GameObject swatchPrefab;
        [FormerlySerializedAs("colorPickerPrefab")] public GameObject wheelPrefab;
        [FormerlySerializedAs("colorChannels")] public List<ColorChannel> channels = new List<ColorChannel>();
    }

    [System.Serializable]
    public class ColorChannel
    {
        [Tooltip("Label shown next to the swatch")]
        [FormerlySerializedAs("colorName")] public string label = "Color";
        [Tooltip("Color property name in the shader (e.g. _BaseColor)")]
        [FormerlySerializedAs("shaderPropertyName")] public string property = "_BaseColor";
        public Color defaultColor = Color.white;
    }
}
