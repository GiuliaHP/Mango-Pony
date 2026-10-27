using System;
using System.IO;
using UnityEngine;

namespace MangoPony.Save
{
    /// <summary>
    /// Gestionnaire de persistance JSON pour la sauvegarde et le chargement des configurations de poneys.
    /// Écrit et lit de manière sécurisée dans Application.persistentDataPath.
    /// </summary>
    public static class PonySaveSystem
    {
        private const string DefaultFileName = "pony_customizer_save.json";
        private const string SaveFolderRelative = "SavedPonies";

        public static string SaveDirectoryPath
        {
            get
            {
                string path = Path.Combine(Application.persistentDataPath, SaveFolderRelative);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        /// <summary>
        /// Sauvegarde les données du poney sous le nom de fichier spécifié.
        /// </summary>
        public static bool SavePony(PonyData data, string fileName = DefaultFileName)
        {
            if (data == null)
            {
                Debug.LogError("[PonySaveSystem] Impossible de sauvegarder des données nulles.");
                return false;
            }

            try
            {
                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".json";
                }

                data.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string json = JsonUtility.ToJson(data, true);
                string filePath = Path.Combine(SaveDirectoryPath, fileName);

                File.WriteAllText(filePath, json);
                Debug.Log($"[PonySaveSystem] Poney sauvegardé avec succès dans : {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PonySaveSystem] Erreur lors de la sauvegarde : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Charge les données du poney depuis le fichier spécifié.
        /// </summary>
        public static PonyData LoadPony(string fileName = DefaultFileName)
        {
            try
            {
                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".json";
                }

                string filePath = Path.Combine(SaveDirectoryPath, fileName);
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[PonySaveSystem] Fichier introuvable : {filePath}. Création des données par défaut.");
                    return PonyData.CreateDefault();
                }

                string json = File.ReadAllText(filePath);
                PonyData data = JsonUtility.FromJson<PonyData>(json);

                if (data == null)
                {
                    Debug.LogWarning("[PonySaveSystem] Données JSON corrompues ou vides. Retour des valeurs par défaut.");
                    return PonyData.CreateDefault();
                }

                Debug.Log($"[PonySaveSystem] Poney chargé avec succès depuis : {filePath}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PonySaveSystem] Erreur lors du chargement : {ex.Message}");
                return PonyData.CreateDefault();
            }
        }

        /// <summary>
        /// Vérifie si un fichier de sauvegarde existe.
        /// </summary>
        public static bool SaveExists(string fileName = DefaultFileName)
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }
            string filePath = Path.Combine(SaveDirectoryPath, fileName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Récupère la liste de tous les fichiers de sauvegarde disponibles.
        /// </summary>
        public static string[] GetAllSaveFiles()
        {
            try
            {
                if (!Directory.Exists(SaveDirectoryPath))
                    return Array.Empty<string>();

                string[] files = Directory.GetFiles(SaveDirectoryPath, "*.json");
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = Path.GetFileName(files[i]);
                }
                return files;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PonySaveSystem] Impossible de lister les fichiers de sauvegarde : {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Supprime un fichier de sauvegarde.
        /// </summary>
        public static bool DeleteSave(string fileName)
        {
            try
            {
                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".json";
                }
                string filePath = Path.Combine(SaveDirectoryPath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[PonySaveSystem] Fichier supprimé : {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PonySaveSystem] Erreur lors de la suppression de {fileName} : {ex.Message}");
                return false;
            }
        }
    }
}

