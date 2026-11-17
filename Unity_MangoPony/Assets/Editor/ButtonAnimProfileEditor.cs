using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ButtonAnimProfile))]
public class ButtonAnimProfileEditor : Editor
{
    private SerializedProperty useScale;

    private SerializedProperty hoverScale;
    private SerializedProperty hoverSize;
    private SerializedProperty hoverDuration;
    private SerializedProperty hoverEase;

    private SerializedProperty punchScale;
    private SerializedProperty punchSize;
    private SerializedProperty punchDuration;
    private SerializedProperty punchVibrato;
    private SerializedProperty punchElasticity;

    private SerializedProperty animateColor;
    private SerializedProperty normalColor;
    private SerializedProperty hoverColor;
    private SerializedProperty disabledColor;

    private SerializedProperty disabledAlpha;
    private SerializedProperty disabledScale;
    private SerializedProperty disabledSize;

    private void OnEnable()
    {
        useScale        = serializedObject.FindProperty(nameof(ButtonAnimProfile.useScale));

        hoverScale      = serializedObject.FindProperty(nameof(ButtonAnimProfile.hoverScale));
        hoverSize       = serializedObject.FindProperty(nameof(ButtonAnimProfile.hoverSize));
        hoverDuration   = serializedObject.FindProperty(nameof(ButtonAnimProfile.hoverDuration));
        hoverEase       = serializedObject.FindProperty(nameof(ButtonAnimProfile.hoverEase));

        punchScale      = serializedObject.FindProperty(nameof(ButtonAnimProfile.punchScale));
        punchSize       = serializedObject.FindProperty(nameof(ButtonAnimProfile.punchSize));
        punchDuration   = serializedObject.FindProperty(nameof(ButtonAnimProfile.punchDuration));
        punchVibrato    = serializedObject.FindProperty(nameof(ButtonAnimProfile.punchVibrato));
        punchElasticity = serializedObject.FindProperty(nameof(ButtonAnimProfile.punchElasticity));

        animateColor    = serializedObject.FindProperty(nameof(ButtonAnimProfile.animateColor));
        normalColor     = serializedObject.FindProperty(nameof(ButtonAnimProfile.normalColor));
        hoverColor      = serializedObject.FindProperty(nameof(ButtonAnimProfile.hoverColor));
        disabledColor   = serializedObject.FindProperty(nameof(ButtonAnimProfile.disabledColor));

        disabledAlpha   = serializedObject.FindProperty(nameof(ButtonAnimProfile.disabledAlpha));
        disabledScale   = serializedObject.FindProperty(nameof(ButtonAnimProfile.disabledScale));
        disabledSize    = serializedObject.FindProperty(nameof(ButtonAnimProfile.disabledSize));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useScale, new GUIContent(
            useScale.boolValue ? "Mode: Local Scale" : "Mode: RectTransform (Pixels)",
            "On = animates local scale | Off = animates the RectTransform size in pixels"
        ));

        bool isScale = useScale.boolValue;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Hover", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isScale ? hoverScale : hoverSize);
        EditorGUILayout.PropertyField(hoverDuration);
        EditorGUILayout.PropertyField(hoverEase);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Press", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isScale ? punchScale : punchSize);
        EditorGUILayout.PropertyField(punchDuration);
        EditorGUILayout.PropertyField(punchVibrato);
        EditorGUILayout.PropertyField(punchElasticity);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
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
        EditorGUILayout.LabelField("Disabled", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(disabledAlpha);
        EditorGUILayout.PropertyField(isScale ? disabledScale : disabledSize);

        serializedObject.ApplyModifiedProperties();
    }
}
