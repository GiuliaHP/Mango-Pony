using System;
using System.Collections.Generic;
using UnityEngine;
using MangoPony.Core;
using MangoPony.Customization;
using MangoPony.Save;

namespace MangoPony.Shaders
{
    /// <summary>
    /// Contrôleur central des matériaux et shaders pour la personnalisation en temps réel.
    /// Gère les couleurs, les masques de textures multicouches (tête, balzanes, motifs, nuances)
    /// et applique les changements dynamiquement sur l'ensemble des parties modulaires du Poney.
    /// </summary>
    public class PonyMaterialController : MonoBehaviour
    {
        [Header("Shader References")]
        [SerializeField] private Shader toonMasterShader;

        [Header("Materials (Instances)")]
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private Material maneMaterial;
        [SerializeField] private Material tailMaterial;
        [SerializeField] private Material eyesMaterial;
        [SerializeField] private Material hoovesMaterial;
        [SerializeField] private Material hornsMaterial;
        [SerializeField] private Material wingsMaterial;

        [Header("Modular Controller Binding")]
        [SerializeField] private PonyModularAttachmentController attachmentController;

        // Identifiants ShaderPropertyToID pour un accès ultra-rapide
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int ShadeColorID = Shader.PropertyToID("_ShadeColor");

        private static readonly int FaceMarkTexID = Shader.PropertyToID("_FaceMarkTex");
        private static readonly int FaceMarkColorID = Shader.PropertyToID("_FaceMarkColor");
        private static readonly int FaceMarkIntensityID = Shader.PropertyToID("_FaceMarkIntensity");

        private static readonly int LegMarkGlobalTexID = Shader.PropertyToID("_LegMarkGlobalTex");
        private static readonly int LegMarkFLTexID = Shader.PropertyToID("_LegMarkFLTex");
        private static readonly int LegMarkFRTexID = Shader.PropertyToID("_LegMarkFRTex");
        private static readonly int LegMarkBLTexID = Shader.PropertyToID("_LegMarkBLTex");
        private static readonly int LegMarkBRTexID = Shader.PropertyToID("_LegMarkBRTex");
        private static readonly int LegMarkColorID = Shader.PropertyToID("_LegMarkColor");
        private static readonly int LegMarkIntensityID = Shader.PropertyToID("_LegMarkIntensity");
        private static readonly int UseIndividualLegsID = Shader.PropertyToID("_UseIndividualLegs");

        private static readonly int BodyPatternTexID = Shader.PropertyToID("_BodyPatternTex");
        private static readonly int BodyPatternColorID = Shader.PropertyToID("_BodyPatternColor");
        private static readonly int BodyPatternIntensityID = Shader.PropertyToID("_BodyPatternIntensity");

        private static readonly int CoatNuanceTexID = Shader.PropertyToID("_CoatNuanceTex");
        private static readonly int CoatNuanceColorID = Shader.PropertyToID("_CoatNuanceColor");
        private static readonly int CoatNuanceIntensityID = Shader.PropertyToID("_CoatNuanceIntensity");

        public Material BodyMaterial => bodyMaterial;
        public Material ManeMaterial => maneMaterial;
        public Material TailMaterial => tailMaterial;
        public Material EyesMaterial => eyesMaterial;
        public Material HoovesMaterial => hoovesMaterial;
        public Material HornsMaterial => hornsMaterial;
        public Material WingsMaterial => wingsMaterial;

        private void Awake()
        {
            if (attachmentController == null)
            {
                attachmentController = GetComponent<PonyModularAttachmentController>();
            }

            InitializeMaterials();

            if (attachmentController != null)
            {
                attachmentController.OnRenderersChanged += ApplyMaterialsToCurrentSlots;
            }
        }

        private void OnDestroy()
        {
            if (attachmentController != null)
            {
                attachmentController.OnRenderersChanged -= ApplyMaterialsToCurrentSlots;
            }
        }

        /// <summary>
        /// Initialise les matériaux dynamiques à partir du shader ToonMaster s'ils ne sont pas déjà assignés.
        /// </summary>
        public void InitializeMaterials()
        {
            if (toonMasterShader == null)
            {
                toonMasterShader = Shader.Find("MangoPony/ToonMaster");
                if (toonMasterShader == null)
                {
                    toonMasterShader = Shader.Find("Universal Render Pipeline/Lit");
                }
            }

            if (bodyMaterial == null && toonMasterShader != null)
            {
                bodyMaterial = new Material(toonMasterShader) { name = "M_Pony_Body_Dynamic" };
            }
            if (maneMaterial == null && toonMasterShader != null)
            {
                maneMaterial = new Material(toonMasterShader) { name = "M_Pony_Mane_Dynamic" };
            }
            if (tailMaterial == null && toonMasterShader != null)
            {
                tailMaterial = new Material(toonMasterShader) { name = "M_Pony_Tail_Dynamic" };
            }
            if (eyesMaterial == null && toonMasterShader != null)
            {
                eyesMaterial = new Material(toonMasterShader) { name = "M_Pony_Eyes_Dynamic" };
            }
            if (hoovesMaterial == null && toonMasterShader != null)
            {
                hoovesMaterial = new Material(toonMasterShader) { name = "M_Pony_Hooves_Dynamic" };
            }
            if (hornsMaterial == null && toonMasterShader != null)
            {
                hornsMaterial = new Material(toonMasterShader) { name = "M_Pony_Horns_Dynamic" };
            }
            if (wingsMaterial == null && toonMasterShader != null)
            {
                wingsMaterial = new Material(toonMasterShader) { name = "M_Pony_Wings_Dynamic" };
            }

            ApplyMaterialsToCurrentSlots();
        }

        /// <summary>
        /// Réassigne les matériaux appropriés sur chaque renderer selon son slot.
        /// </summary>
        public void ApplyMaterialsToCurrentSlots()
        {
            if (attachmentController == null) return;

            AssignMaterialToSlot(AttachmentSlot.Body, bodyMaterial);
            AssignMaterialToSlot(AttachmentSlot.Head, bodyMaterial);
            AssignMaterialToSlot(AttachmentSlot.Mane, maneMaterial);
            AssignMaterialToSlot(AttachmentSlot.Tail, tailMaterial);
            AssignMaterialToSlot(AttachmentSlot.Hooves, hoovesMaterial);
            AssignMaterialToSlot(AttachmentSlot.Horns, hornsMaterial);
            AssignMaterialToSlot(AttachmentSlot.Wings, wingsMaterial);
        }

        private void AssignMaterialToSlot(AttachmentSlot slot, Material mat)
        {
            if (mat == null) return;

            var renderers = attachmentController.GetRenderersForSlot(slot);
            foreach (var r in renderers)
            {
                if (r == null) continue;

                // Si l'objet est un sous-mesh spécifique (ex: yeux dans la tête)
                if (r.name.ToLowerInvariant().Contains("eye") || r.name.ToLowerInvariant().Contains("oeil"))
                {
                    r.material = eyesMaterial != null ? eyesMaterial : mat;
                }
                else
                {
                    r.material = mat;
                }
            }
        }

        #region Color Setters

        public void SetBodyColor(Color color)
        {
            if (bodyMaterial != null)
            {
                bodyMaterial.SetColor(BaseColorID, color);
                // Dérive automatiquement une couleur d'ombrage légèrement plus saturée et sombre
                Color shade = Color.Lerp(color, Color.black, 0.35f);
                bodyMaterial.SetColor(ShadeColorID, shade);
            }
        }

        public void SetManeColor(Color color)
        {
            if (maneMaterial != null)
            {
                maneMaterial.SetColor(BaseColorID, color);
                Color shade = Color.Lerp(color, Color.black, 0.30f);
                maneMaterial.SetColor(ShadeColorID, shade);
            }
        }

        public void SetTailColor(Color color)
        {
            if (tailMaterial != null)
            {
                tailMaterial.SetColor(BaseColorID, color);
                Color shade = Color.Lerp(color, Color.black, 0.30f);
                tailMaterial.SetColor(ShadeColorID, shade);
            }
        }

        public void SetEyeColor(Color color)
        {
            if (eyesMaterial != null)
            {
                eyesMaterial.SetColor(BaseColorID, color);
            }
        }

        public void SetHoofColor(Color color)
        {
            if (hoovesMaterial != null)
            {
                hoovesMaterial.SetColor(BaseColorID, color);
                Color shade = Color.Lerp(color, Color.black, 0.30f);
                hoovesMaterial.SetColor(ShadeColorID, shade);
            }
        }

        public void SetHornColor(Color color)
        {
            if (hornsMaterial != null)
            {
                hornsMaterial.SetColor(BaseColorID, color);
                Color shade = Color.Lerp(color, Color.black, 0.25f);
                hornsMaterial.SetColor(ShadeColorID, shade);
            }
        }

        public void SetWingColor(Color color)
        {
            if (wingsMaterial != null)
            {
                wingsMaterial.SetColor(BaseColorID, color);
                Color shade = Color.Lerp(color, Color.black, 0.25f);
                wingsMaterial.SetColor(ShadeColorID, shade);
            }
        }

        public void SetMarkColor(Color color)
        {
            if (bodyMaterial != null)
            {
                bodyMaterial.SetColor(FaceMarkColorID, color);
                bodyMaterial.SetColor(LegMarkColorID, color);
            }
        }

        public void SetBodyPatternColor(Color color)
        {
            if (bodyMaterial != null)
            {
                bodyMaterial.SetColor(BodyPatternColorID, color);
            }
        }

        public void SetCoatNuanceColor(Color color)
        {
            if (bodyMaterial != null)
            {
                bodyMaterial.SetColor(CoatNuanceColorID, color);
            }
        }

        #endregion

        #region Mask Texture Setters

        public void SetFaceMarkTexture(Texture2D texture)
        {
            if (bodyMaterial == null) return;

            if (texture != null)
            {
                bodyMaterial.SetTexture(FaceMarkTexID, texture);
                bodyMaterial.SetFloat(FaceMarkIntensityID, 1.0f);
            }
            else
            {
                bodyMaterial.SetTexture(FaceMarkTexID, Texture2D.blackTexture);
                bodyMaterial.SetFloat(FaceMarkIntensityID, 0.0f);
            }
        }

        public void SetBodyPatternTexture(Texture2D texture)
        {
            if (bodyMaterial == null) return;

            if (texture != null)
            {
                bodyMaterial.SetTexture(BodyPatternTexID, texture);
                bodyMaterial.SetFloat(BodyPatternIntensityID, 1.0f);
            }
            else
            {
                bodyMaterial.SetTexture(BodyPatternTexID, Texture2D.blackTexture);
                bodyMaterial.SetFloat(BodyPatternIntensityID, 0.0f);
            }
        }

        public void SetCoatNuanceTexture(Texture2D texture)
        {
            if (bodyMaterial == null) return;

            if (texture != null)
            {
                bodyMaterial.SetTexture(CoatNuanceTexID, texture);
                bodyMaterial.SetFloat(CoatNuanceIntensityID, 1.0f);
            }
            else
            {
                bodyMaterial.SetTexture(CoatNuanceTexID, Texture2D.blackTexture);
                bodyMaterial.SetFloat(CoatNuanceIntensityID, 0.0f);
            }
        }

        public void SetAllLegMarksTexture(Texture2D texture)
        {
            if (bodyMaterial == null) return;

            bodyMaterial.SetFloat(UseIndividualLegsID, 0.0f);
            if (texture != null)
            {
                bodyMaterial.SetTexture(LegMarkGlobalTexID, texture);
                bodyMaterial.SetFloat(LegMarkIntensityID, 1.0f);
            }
            else
            {
                bodyMaterial.SetTexture(LegMarkGlobalTexID, Texture2D.blackTexture);
                bodyMaterial.SetFloat(LegMarkIntensityID, 0.0f);
            }
        }

        public void SetIndividualLegMarkTexture(LegSlot slot, Texture2D texture)
        {
            if (bodyMaterial == null) return;

            bodyMaterial.SetFloat(UseIndividualLegsID, 1.0f);
            bodyMaterial.SetFloat(LegMarkIntensityID, 1.0f);

            Texture2D texToSet = texture != null ? texture : Texture2D.blackTexture;

            switch (slot)
            {
                case LegSlot.FrontLeft:
                    bodyMaterial.SetTexture(LegMarkFLTexID, texToSet);
                    break;
                case LegSlot.FrontRight:
                    bodyMaterial.SetTexture(LegMarkFRTexID, texToSet);
                    break;
                case LegSlot.BackLeft:
                    bodyMaterial.SetTexture(LegMarkBLTexID, texToSet);
                    break;
                case LegSlot.BackRight:
                    bodyMaterial.SetTexture(LegMarkBRTexID, texToSet);
                    break;
            }
        }

        public void SetLegSyncMode(bool syncAll)
        {
            if (bodyMaterial == null) return;
            bodyMaterial.SetFloat(UseIndividualLegsID, syncAll ? 0.0f : 1.0f);
        }

        #endregion

        /// <summary>
        /// Applique l'ensemble d'une configuration PonyData en une seule opération.
        /// </summary>
        public void ApplyFullData(PonyData data, PonyCustomizationDatabase database)
        {
            if (data == null || database == null) return;

            // Couleurs
            SetBodyColor(data.bodyColor);
            SetManeColor(data.maneColor);
            SetTailColor(data.tailColor);
            SetEyeColor(data.eyeColor);
            SetHoofColor(data.hoofColor);
            SetHornColor(data.hornColor);
            SetWingColor(data.wingColor);
            SetMarkColor(data.markColor);
            SetBodyPatternColor(data.patternColor);
            SetCoatNuanceColor(data.nuanceColor);

            // Masques
            SetFaceMarkTexture(database.GetFaceMarkTexture(data.faceMark));
            SetBodyPatternTexture(database.GetBodyPatternTexture(data.bodyPattern));
            SetCoatNuanceTexture(database.GetCoatNuanceTexture(data.coatNuance));

            SetLegSyncMode(data.syncAllLegs);
            if (data.syncAllLegs)
            {
                SetAllLegMarksTexture(database.GetLegMarkTexture(data.legMarkFrontLeft));
            }
            else
            {
                SetIndividualLegMarkTexture(LegSlot.FrontLeft, database.GetLegMarkTexture(data.legMarkFrontLeft));
                SetIndividualLegMarkTexture(LegSlot.FrontRight, database.GetLegMarkTexture(data.legMarkFrontRight));
                SetIndividualLegMarkTexture(LegSlot.BackLeft, database.GetLegMarkTexture(data.legMarkBackLeft));
                SetIndividualLegMarkTexture(LegSlot.BackRight, database.GetLegMarkTexture(data.legMarkBackRight));
            }
        }
    }
}

