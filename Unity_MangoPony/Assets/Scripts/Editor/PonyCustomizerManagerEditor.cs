#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MangoPony.Core;
using MangoPony.Customization;

namespace MangoPony.Editor
{
    /// <summary>
    /// Inspecteur personnalisé pour le PonyCustomizerManager permettant d'exécuter
    /// les fonctions clés directement depuis l'éditeur Unity (sans lancer le Play Mode).
    /// </summary>
    [CustomEditor(typeof(PonyCustomizerManager))]
    public class PonyCustomizerManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PonyCustomizerManager manager = (PonyCustomizerManager)target;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Outils & Actions Rapides Mango Pony", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button("🎨 Appliquer la Configuration Complète", GUILayout.Height(30)))
                {
                    manager.ApplyFullConfiguration();
                }

                EditorGUILayout.Space(5);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("🎲 Randomiser", GUILayout.Height(28)))
                    {
                        manager.RandomizePony();
                    }

                    if (GUILayout.Button("↺ Réinitialiser", GUILayout.Height(28)))
                    {
                        manager.ResetToDefaults();
                    }
                }

                EditorGUILayout.Space(5);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("💾 Sauvegarder (JSON)", GUILayout.Height(26)))
                    {
                        manager.SavePony();
                    }

                    if (GUILayout.Button("📂 Charger (JSON)", GUILayout.Height(26)))
                    {
                        manager.LoadPony();
                    }
                }
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button("⚡ Assistant de Génération Complète (Wizard)", GUILayout.Height(32)))
            {
                PonyCustomizerSetupWizard.RunFullSetup();
            }
        }
    }
}
#endif

