using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JuicyButtonProfile))]
public class JuicyButtonProfileEditor : Editor
{
    private SerializedProperty useScaleAnimation;

    // Hover
    private SerializedProperty hoverScale;
    private SerializedProperty hoverSizeOffset;
    private SerializedProperty hoverDuration;
    private SerializedProperty hoverEase;

    // Punch
    private SerializedProperty punchScale;
    private SerializedProperty punchSize;
    private SerializedProperty punchDuration;
    private SerializedProperty punchVibrato;
    private SerializedProperty punchElasticity;

    // Color
    private SerializedProperty animateColor;
    private SerializedProperty normalColor;
    private SerializedProperty hoverColor;
    private SerializedProperty disabledColor;

    // Disabled
    private SerializedProperty disabledAlpha;
    private SerializedProperty disabledScale;
    private SerializedProperty disabledSizeOffset;

    private void OnEnable()
    {
        useScaleAnimation  = serializedObject.FindProperty("useScaleAnimation");

        hoverScale         = serializedObject.FindProperty("hoverScale");
        hoverSizeOffset    = serializedObject.FindProperty("hoverSizeOffset");
        hoverDuration      = serializedObject.FindProperty("hoverDuration");
        hoverEase          = serializedObject.FindProperty("hoverEase");

        punchScale         = serializedObject.FindProperty("punchScale");
        punchSize          = serializedObject.FindProperty("punchSize");
        punchDuration      = serializedObject.FindProperty("punchDuration");
        punchVibrato       = serializedObject.FindProperty("punchVibrato");
        punchElasticity    = serializedObject.FindProperty("punchElasticity");

        animateColor       = serializedObject.FindProperty("animateColor");
        normalColor        = serializedObject.FindProperty("normalColor");
        hoverColor         = serializedObject.FindProperty("hoverColor");
        disabledColor      = serializedObject.FindProperty("disabledColor");

        disabledAlpha      = serializedObject.FindProperty("disabledAlpha");
        disabledScale      = serializedObject.FindProperty("disabledScale");
        disabledSizeOffset = serializedObject.FindProperty("disabledSizeOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // --- Mode d'animation ---
        EditorGUILayout.LabelField("Mode d'Animation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useScaleAnimation, new GUIContent(
            useScaleAnimation.boolValue ? "Mode : Local Scale" : "Mode : RectTransform (Pixels)",
            "Coché = anime le LocalScale | Décoché = anime la taille du RectTransform en pixels"
        ));

        bool isScale = useScaleAnimation.boolValue;

        EditorGUILayout.Space(8);

        // --- Hover ---
        EditorGUILayout.LabelField("Hover Settings (Survol)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isScale ? hoverScale : hoverSizeOffset);
        EditorGUILayout.PropertyField(hoverDuration);
        EditorGUILayout.PropertyField(hoverEase);

        EditorGUILayout.Space(8);

        // --- Punch ---
        EditorGUILayout.LabelField("Click / Punch Settings (Pression)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isScale ? punchScale : punchSize);
        EditorGUILayout.PropertyField(punchDuration);
        EditorGUILayout.PropertyField(punchVibrato);
        EditorGUILayout.PropertyField(punchElasticity);

        EditorGUILayout.Space(8);

        // --- Color (en bonus : masque aussi les couleurs si animateColor est décoché) ---
        EditorGUILayout.LabelField("Color Feedback", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(animateColor);
        if (animateColor.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(normalColor);
            EditorGUILayout.PropertyField(hoverColor);
            EditorGUILayout.PropertyField(disabledColor);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(8);

        // --- Disabled ---
        EditorGUILayout.LabelField("Disabled State Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(disabledAlpha);
        EditorGUILayout.PropertyField(isScale ? disabledScale : disabledSizeOffset);

        serializedObject.ApplyModifiedProperties();
    }
}