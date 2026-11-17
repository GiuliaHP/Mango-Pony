using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CameraIdAttribute))]
public class CameraIdDrawer : PropertyDrawer
{
    private const string FullbodyLabel = "Fullbody";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var ids = new List<string>(rig.DetailIds);
        string current = property.stringValue ?? string.Empty;
        int found = ids.IndexOf(current);
        if (current.Length > 0 && found < 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        int currentIndex = current.Length == 0 ? 0 : found + 1;

        var options = new string[ids.Count + 1];
        options[0] = FullbodyLabel;
        for (int i = 0; i < ids.Count; i++)
            options[i + 1] = ids[i];

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        int selected = EditorGUI.Popup(position, label.text, currentIndex, options);
        if (EditorGUI.EndChangeCheck())
            property.stringValue = selected == 0 ? string.Empty : ids[selected - 1];
        EditorGUI.EndProperty();
    }
}
