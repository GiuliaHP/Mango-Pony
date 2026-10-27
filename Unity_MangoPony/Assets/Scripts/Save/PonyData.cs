using System;
using UnityEngine;
using MangoPony.Core;

namespace MangoPony.Save
{
    /// <summary>
    /// Données sérialisables complètes représentant la configuration personnalisée d'un Poney.
    /// Stocké et rechargé via JSON par le PonySaveSystem.
    /// </summary>
    [Serializable]
    public class PonyData
    {
        [Header("Metadata")]
        public string ponyName = "Mango";
        public string saveDate = "";
        public int dataVersion = 1;

        [Header("Mesh Morphologies & Attachments")]
        public BodyType bodyType = BodyType.Classique;
        public HeadType headType = HeadType.Plate;
        public HoofType hoofType = HoofType.Classiques;
        public ManeType maneType = ManeType.Classique;
        public TailType tailType = TailType.Classique;
        public HornType hornType = HornType.None;
        public WingType wingType = WingType.None;

        [Header("Texture Markings & Patterns")]
        public FaceMarkType faceMark = FaceMarkType.None;
        public BodyPatternType bodyPattern = BodyPatternType.None;
        public CoatNuanceType coatNuance = CoatNuanceType.None;

        [Header("Leg Markings (Balzanes)")]
        public bool syncAllLegs = true;
        public LegMarkType legMarkFrontLeft = LegMarkType.None;
        public LegMarkType legMarkFrontRight = LegMarkType.None;
        public LegMarkType legMarkBackLeft = LegMarkType.None;
        public LegMarkType legMarkBackRight = LegMarkType.None;

        [Header("Colors (RGBA)")]
        public SerializableColor bodyColor = new SerializableColor(0.85f, 0.55f, 0.32f, 1f); // Alezan / Caramel
        public SerializableColor eyeColor = new SerializableColor(0.2f, 0.45f, 0.85f, 1f);   // Bleu azur
        public SerializableColor maneColor = new SerializableColor(0.95f, 0.90f, 0.70f, 1f); // Crins lavés / Blond
        public SerializableColor tailColor = new SerializableColor(0.95f, 0.90f, 0.70f, 1f);
        public SerializableColor hoofColor = new SerializableColor(0.25f, 0.22f, 0.20f, 1f); // Corne sombre
        public SerializableColor hornColor = new SerializableColor(0.98f, 0.88f, 0.40f, 1f); // Doré
        public SerializableColor wingColor = new SerializableColor(0.95f, 0.95f, 0.98f, 1f); // Blanc plume
        public SerializableColor markColor = new SerializableColor(0.98f, 0.98f, 0.98f, 1f); // Blanc pur (marques)
        public SerializableColor patternColor = new SerializableColor(0.95f, 0.95f, 0.95f, 1f); // Blanc pie
        public SerializableColor nuanceColor = new SerializableColor(0.92f, 0.75f, 0.55f, 1f);  // Pangaré / pommelé

        /// <summary>
        /// Crée une copie profonde des données actuelles.
        /// </summary>
        public PonyData Clone()
        {
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<PonyData>(json);
        }

        /// <summary>
        /// Réinitialise l'instance avec les valeurs par défaut.
        /// </summary>
        public static PonyData CreateDefault()
        {
            return new PonyData
            {
                ponyName = "Mango",
                saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                bodyType = BodyType.Classique,
                headType = HeadType.Plate,
                hoofType = HoofType.Classiques,
                maneType = ManeType.Classique,
                tailType = TailType.Classique,
                hornType = HornType.None,
                wingType = WingType.None,
                faceMark = FaceMarkType.Etoile,
                bodyPattern = BodyPatternType.None,
                coatNuance = CoatNuanceType.None,
                syncAllLegs = true,
                legMarkFrontLeft = LegMarkType.PetiteBalzane,
                legMarkFrontRight = LegMarkType.PetiteBalzane,
                legMarkBackLeft = LegMarkType.PetiteBalzane,
                legMarkBackRight = LegMarkType.PetiteBalzane,
                bodyColor = new Color(0.85f, 0.55f, 0.32f, 1f),
                eyeColor = new Color(0.2f, 0.45f, 0.85f, 1f),
                maneColor = new Color(0.95f, 0.90f, 0.70f, 1f),
                tailColor = new Color(0.95f, 0.90f, 0.70f, 1f),
                hoofColor = new Color(0.25f, 0.22f, 0.20f, 1f),
                hornColor = new Color(0.98f, 0.88f, 0.40f, 1f),
                wingColor = new Color(0.95f, 0.95f, 0.98f, 1f),
                markColor = new Color(0.98f, 0.98f, 0.98f, 1f),
                patternColor = new Color(0.95f, 0.95f, 0.95f, 1f),
                nuanceColor = new Color(0.92f, 0.75f, 0.55f, 1f)
            };
        }
    }
}

