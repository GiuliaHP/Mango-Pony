using System;
using UnityEngine;
using MangoPony.Customization;
using MangoPony.Save;
using MangoPony.Shaders;

namespace MangoPony.Core
{
    /// <summary>
    /// Chef d'orchestre central du système de customisation Mango Pony.
    /// Synchronise les meshes modulaires, les matériaux Toon/URP, les données sérialisées et l'interface utilisateur.
    /// </summary>
    [RequireComponent(typeof(PonyModularAttachmentController))]
    [RequireComponent(typeof(PonyMaterialController))]
    public class PonyCustomizerManager : MonoBehaviour
    {
        public static PonyCustomizerManager Instance { get; private set; }

        [Header("Database")]
        [SerializeField] private PonyCustomizationDatabase database;

        [Header("Sub-Controllers")]
        [SerializeField] private PonyModularAttachmentController attachmentController;
        [SerializeField] private PonyMaterialController materialController;

        [Header("State")]
        [SerializeField] private PonyData currentData;

        public PonyData CurrentData => currentData;
        public PonyCustomizationDatabase Database => database;
        public PonyModularAttachmentController AttachmentController => attachmentController;
        public PonyMaterialController MaterialController => materialController;

        public event Action<PonyData> OnPonyUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (attachmentController == null)
                attachmentController = GetComponent<PonyModularAttachmentController>();
            if (materialController == null)
                materialController = GetComponent<PonyMaterialController>();

            if (database == null)
            {
                database = Resources.Load<PonyCustomizationDatabase>("PonyCustomizationDatabase");
#if UNITY_EDITOR
                if (database == null)
                {
                    database = UnityEditor.AssetDatabase.LoadAssetAtPath<PonyCustomizationDatabase>("Assets/ScriptableObjects/PonyCustomizationDatabase.asset");
                }
#endif
            }

            if (currentData == null)
            {
                currentData = PonyData.CreateDefault();
            }
        }

        private void Start()
        {
            ApplyFullConfiguration();
        }

        /// <summary>
        /// Applique l'ensemble de la configuration actuelle (meshes et shaders).
        /// </summary>
        public void ApplyFullConfiguration()
        {
            if (database == null)
            {
                Debug.LogWarning("[PonyCustomizerManager] Aucune base de données assignée !");
                return;
            }

            // 1. Mise à jour des meshes modulaires
            UpdateMeshes();

            // 2. Mise à jour des matériaux & shaders
            if (materialController != null)
            {
                materialController.ApplyFullData(currentData, database);
            }

            OnPonyUpdated?.Invoke(currentData);
        }

        private void UpdateMeshes()
        {
            if (attachmentController == null || database == null) return;

            // Corps
            attachmentController.SetSlotPrefab(AttachmentSlot.Body, database.GetBodyPrefab(currentData.bodyType));

            // Tête
            attachmentController.SetSlotPrefab(AttachmentSlot.Head, database.GetHeadPrefab(currentData.headType));

            // Sabots
            attachmentController.SetSlotPrefab(AttachmentSlot.Hooves, database.GetHoofPrefab(currentData.hoofType));

            // Crinière (None géré si prefab == null)
            attachmentController.SetSlotPrefab(AttachmentSlot.Mane, database.GetManePrefab(currentData.maneType));

            // Queue
            attachmentController.SetSlotPrefab(AttachmentSlot.Tail, database.GetTailPrefab(currentData.tailType));

            // Cornes
            attachmentController.SetSlotPrefab(AttachmentSlot.Horns, database.GetHornPrefab(currentData.hornType));

            // Ailes
            attachmentController.SetSlotPrefab(AttachmentSlot.Wings, database.GetWingPrefab(currentData.wingType));
        }

        #region Mesh Selectors

        public void SetBodyType(BodyType type)
        {
            if (currentData.bodyType == type) return;
            currentData.bodyType = type;
            if (database != null)
            {
                attachmentController.SetSlotPrefab(AttachmentSlot.Body, database.GetBodyPrefab(type));
            }
            NotifyUpdate();
        }

        public void SetHeadType(HeadType type)
        {
            if (currentData.headType == type) return;
            currentData.headType = type;
            if (database != null)
            {
                attachmentController.SetSlotPrefab(AttachmentSlot.Head, database.GetHeadPrefab(type));
            }
            NotifyUpdate();
        }

        public void SetHoofType(HoofType type)
        {
            if (currentData.hoofType == type) return;
            currentData.hoofType = type;
            if (database != null)
            {
                attachmentController.SetSlotPrefab(AttachmentSlot.Hooves, database.GetHoofPrefab(type));
            }
            NotifyUpdate();
        }

        public void SetManeType(ManeType type)
        {
            if (currentData.maneType == type) return;
            currentData.maneType = type;
            if (database != null)
            {
                attachmentController.SetSlotPrefab(AttachmentSlot.Mane, database.GetManePrefab(type));
            }
            NotifyUpdate();
        }

        public void SetTailType(TailType type)
        {
            if (currentData.tailType == type) return;
            currentData.tailType = type;
            if (database != null)
            {
                attachmentController.SetSlotPrefab(AttachmentSlot.Tail, database.GetTailPrefab(type));
            }
            NotifyUpdate();
        }

        public void SetHornType(HornType type)
        {
            if (currentData.hornType == type) return;
            currentData.hornType = type;
            if (database != null)
            {
                attachmentController.SetSlotPrefab(AttachmentSlot.Horns, database.GetHornPrefab(type));
            }
            NotifyUpdate();
        }

        public void SetWingType(WingType type)
        {
            if (currentData.wingType == type) return;
            currentData.wingType = type;
            if (database != null)
            {
                attachmentController.SetSlotPrefab(AttachmentSlot.Wings, database.GetWingPrefab(type));
            }
            NotifyUpdate();
        }

        #endregion

        #region Mask & Markings Selectors

        public void SetFaceMark(FaceMarkType type)
        {
            currentData.faceMark = type;
            if (materialController != null && database != null)
            {
                materialController.SetFaceMarkTexture(database.GetFaceMarkTexture(type));
            }
            NotifyUpdate();
        }

        public void SetBodyPattern(BodyPatternType type)
        {
            currentData.bodyPattern = type;
            if (materialController != null && database != null)
            {
                materialController.SetBodyPatternTexture(database.GetBodyPatternTexture(type));
            }
            NotifyUpdate();
        }

        public void SetCoatNuance(CoatNuanceType type)
        {
            currentData.coatNuance = type;
            if (materialController != null && database != null)
            {
                materialController.SetCoatNuanceTexture(database.GetCoatNuanceTexture(type));
            }
            NotifyUpdate();
        }

        public void SetLegSync(bool syncAll)
        {
            currentData.syncAllLegs = syncAll;
            if (materialController != null)
            {
                materialController.SetLegSyncMode(syncAll);
                if (syncAll && database != null)
                {
                    materialController.SetAllLegMarksTexture(database.GetLegMarkTexture(currentData.legMarkFrontLeft));
                }
            }
            NotifyUpdate();
        }

        public void SetAllLegMarks(LegMarkType mark)
        {
            currentData.legMarkFrontLeft = mark;
            currentData.legMarkFrontRight = mark;
            currentData.legMarkBackLeft = mark;
            currentData.legMarkBackRight = mark;

            if (materialController != null && database != null)
            {
                materialController.SetAllLegMarksTexture(database.GetLegMarkTexture(mark));
            }
            NotifyUpdate();
        }

        public void SetLegMark(LegSlot slot, LegMarkType mark)
        {
            switch (slot)
            {
                case LegSlot.FrontLeft:
                    currentData.legMarkFrontLeft = mark;
                    break;
                case LegSlot.FrontRight:
                    currentData.legMarkFrontRight = mark;
                    break;
                case LegSlot.BackLeft:
                    currentData.legMarkBackLeft = mark;
                    break;
                case LegSlot.BackRight:
                    currentData.legMarkBackRight = mark;
                    break;
            }

            if (materialController != null && database != null)
            {
                if (currentData.syncAllLegs)
                {
                    SetAllLegMarks(mark);
                }
                else
                {
                    materialController.SetIndividualLegMarkTexture(slot, database.GetLegMarkTexture(mark));
                }
            }
            NotifyUpdate();
        }

        #endregion

        #region Color Setters

        public void SetBodyColor(Color color)
        {
            currentData.bodyColor = color;
            materialController?.SetBodyColor(color);
            NotifyUpdate();
        }

        public void SetManeColor(Color color)
        {
            currentData.maneColor = color;
            materialController?.SetManeColor(color);
            NotifyUpdate();
        }

        public void SetTailColor(Color color)
        {
            currentData.tailColor = color;
            materialController?.SetTailColor(color);
            NotifyUpdate();
        }

        public void SetEyeColor(Color color)
        {
            currentData.eyeColor = color;
            materialController?.SetEyeColor(color);
            NotifyUpdate();
        }

        public void SetHoofColor(Color color)
        {
            currentData.hoofColor = color;
            materialController?.SetHoofColor(color);
            NotifyUpdate();
        }

        public void SetHornColor(Color color)
        {
            currentData.hornColor = color;
            materialController?.SetHornColor(color);
            NotifyUpdate();
        }

        public void SetWingColor(Color color)
        {
            currentData.wingColor = color;
            materialController?.SetWingColor(color);
            NotifyUpdate();
        }

        public void SetMarkColor(Color color)
        {
            currentData.markColor = color;
            materialController?.SetMarkColor(color);
            NotifyUpdate();
        }

        public void SetBodyPatternColor(Color color)
        {
            currentData.patternColor = color;
            materialController?.SetBodyPatternColor(color);
            NotifyUpdate();
        }

        public void SetCoatNuanceColor(Color color)
        {
            currentData.nuanceColor = color;
            materialController?.SetCoatNuanceColor(color);
            NotifyUpdate();
        }

        #endregion

        #region Actions (Randomize, Reset, Save, Load)

        /// <summary>
        /// Génère une combinaison aléatoire cohérente et attrayante.
        /// </summary>
        public void RandomizePony()
        {
            // Morphologies
            currentData.bodyType = (BodyType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(BodyType)).Length);
            currentData.headType = (HeadType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(HeadType)).Length);
            currentData.hoofType = (HoofType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(HoofType)).Length);
            currentData.maneType = (ManeType)UnityEngine.Random.Range(1, Enum.GetValues(typeof(ManeType)).Length); // Éviter None en random
            currentData.tailType = (TailType)UnityEngine.Random.Range(1, Enum.GetValues(typeof(TailType)).Length);

            // Chance d'avoir des ailes ou corne (35%)
            currentData.hornType = UnityEngine.Random.value < 0.35f 
                ? (HornType)UnityEngine.Random.Range(1, Enum.GetValues(typeof(HornType)).Length) 
                : HornType.None;

            currentData.wingType = UnityEngine.Random.value < 0.35f 
                ? (WingType)UnityEngine.Random.Range(1, Enum.GetValues(typeof(WingType)).Length) 
                : WingType.None;

            // Marques
            currentData.faceMark = (FaceMarkType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(FaceMarkType)).Length);
            currentData.bodyPattern = (BodyPatternType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(BodyPatternType)).Length);
            currentData.coatNuance = (CoatNuanceType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CoatNuanceType)).Length);

            currentData.syncAllLegs = UnityEngine.Random.value > 0.5f;
            currentData.legMarkFrontLeft = (LegMarkType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(LegMarkType)).Length);
            if (currentData.syncAllLegs)
            {
                currentData.legMarkFrontRight = currentData.legMarkFrontLeft;
                currentData.legMarkBackLeft = currentData.legMarkFrontLeft;
                currentData.legMarkBackRight = currentData.legMarkFrontLeft;
            }
            else
            {
                currentData.legMarkFrontRight = (LegMarkType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(LegMarkType)).Length);
                currentData.legMarkBackLeft = (LegMarkType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(LegMarkType)).Length);
                currentData.legMarkBackRight = (LegMarkType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(LegMarkType)).Length);
            }

            // Couleurs
            Color[] coatPalette = new Color[]
            {
                new Color(0.85f, 0.55f, 0.32f), // Alezan
                new Color(0.42f, 0.23f, 0.12f), // Bai brun
                new Color(0.12f, 0.12f, 0.13f), // Noir ébène
                new Color(0.88f, 0.88f, 0.90f), // Gris clair
                new Color(0.92f, 0.78f, 0.48f), // Palomino doré
                new Color(0.95f, 0.65f, 0.75f), // Rose pastel fantaisie
                new Color(0.60f, 0.85f, 0.95f)  // Bleu ciel fantaisie
            };
            currentData.bodyColor = coatPalette[UnityEngine.Random.Range(0, coatPalette.Length)];
            currentData.eyeColor = new Color(UnityEngine.Random.Range(0.2f, 0.9f), UnityEngine.Random.Range(0.2f, 0.9f), UnityEngine.Random.Range(0.2f, 0.9f));
            currentData.maneColor = UnityEngine.Random.value > 0.5f ? currentData.bodyColor.ToColor() * 0.8f : Color.white;
            currentData.tailColor = currentData.maneColor;
            currentData.hoofColor = new Color(0.22f, 0.20f, 0.18f);
            currentData.hornColor = new Color(0.98f, 0.85f, 0.35f);
            currentData.wingColor = new Color(0.96f, 0.96f, 0.98f);
            currentData.markColor = Color.white;
            currentData.patternColor = Color.white;
            currentData.nuanceColor = Color.Lerp(currentData.bodyColor, Color.white, 0.3f);

            ApplyFullConfiguration();
        }

        public void ResetToDefaults()
        {
            currentData = PonyData.CreateDefault();
            ApplyFullConfiguration();
        }

        public void SavePony(string saveFileName = "pony_save.json")
        {
            PonySaveSystem.SavePony(currentData, saveFileName);
        }

        public void LoadPony(string saveFileName = "pony_save.json")
        {
            currentData = PonySaveSystem.LoadPony(saveFileName);
            ApplyFullConfiguration();
        }

        #endregion

        private void NotifyUpdate()
        {
            OnPonyUpdated?.Invoke(currentData);
        }
    }
}
