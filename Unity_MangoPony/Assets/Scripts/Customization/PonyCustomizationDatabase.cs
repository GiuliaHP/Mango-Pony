using System;
using System.Collections.Generic;
using UnityEngine;
using MangoPony.Core;

namespace MangoPony.Customization
{
    /// <summary>
    /// Classe de base pour une entrée de customisation visuelle dans la base de données.
    /// </summary>
    [Serializable]
    public abstract class BaseCustomizationEntry
    {
        public string displayName;
        public Sprite icon;
        [TextArea(1, 3)]
        public string description;
    }

    [Serializable]
    public class BodyMeshEntry : BaseCustomizationEntry
    {
        public BodyType type;
        public GameObject prefab;
    }

    [Serializable]
    public class HeadMeshEntry : BaseCustomizationEntry
    {
        public HeadType type;
        public GameObject prefab;
    }

    [Serializable]
    public class HoofMeshEntry : BaseCustomizationEntry
    {
        public HoofType type;
        public GameObject prefab;
    }

    [Serializable]
    public class ManeMeshEntry : BaseCustomizationEntry
    {
        public ManeType type;
        public GameObject prefab;
    }

    [Serializable]
    public class TailMeshEntry : BaseCustomizationEntry
    {
        public TailType type;
        public GameObject prefab;
    }

    [Serializable]
    public class HornMeshEntry : BaseCustomizationEntry
    {
        public HornType type;
        public GameObject prefab;
    }

    [Serializable]
    public class WingMeshEntry : BaseCustomizationEntry
    {
        public WingType type;
        public GameObject prefab;
    }

    [Serializable]
    public class FaceMarkTextureEntry : BaseCustomizationEntry
    {
        public FaceMarkType type;
        public Texture2D maskTexture;
    }

    [Serializable]
    public class LegMarkTextureEntry : BaseCustomizationEntry
    {
        public LegMarkType type;
        public Texture2D maskTexture;
    }

    [Serializable]
    public class BodyPatternTextureEntry : BaseCustomizationEntry
    {
        public BodyPatternType type;
        public Texture2D maskTexture;
    }

    [Serializable]
    public class CoatNuanceTextureEntry : BaseCustomizationEntry
    {
        public CoatNuanceType type;
        public Texture2D maskTexture;
    }

    [Serializable]
    public class ColorPresetEntry
    {
        public string presetName;
        public Color color;
    }

    /// <summary>
    /// ScriptableObject centralisant l'ensemble des meshes, prefabs et textures de masques
    /// pour le prototype du customizer Mango Pony.
    /// </summary>
    [CreateAssetMenu(fileName = "PonyCustomizationDatabase", menuName = "Mango Pony/Customization Database", order = 1)]
    public class PonyCustomizationDatabase : ScriptableObject
    {
        [Header("Modular Meshes")]
        [SerializeField] private List<BodyMeshEntry> bodyMeshes = new List<BodyMeshEntry>();
        [SerializeField] private List<HeadMeshEntry> headMeshes = new List<HeadMeshEntry>();
        [SerializeField] private List<HoofMeshEntry> hoofMeshes = new List<HoofMeshEntry>();
        [SerializeField] private List<ManeMeshEntry> maneMeshes = new List<ManeMeshEntry>();
        [SerializeField] private List<TailMeshEntry> tailMeshes = new List<TailMeshEntry>();
        [SerializeField] private List<HornMeshEntry> hornMeshes = new List<HornMeshEntry>();
        [SerializeField] private List<WingMeshEntry> wingMeshes = new List<WingMeshEntry>();

        [Header("Mask Textures")]
        [SerializeField] private List<FaceMarkTextureEntry> faceMarkTextures = new List<FaceMarkTextureEntry>();
        [SerializeField] private List<LegMarkTextureEntry> legMarkTextures = new List<LegMarkTextureEntry>();
        [SerializeField] private List<BodyPatternTextureEntry> bodyPatternTextures = new List<BodyPatternTextureEntry>();
        [SerializeField] private List<CoatNuanceTextureEntry> coatNuanceTextures = new List<CoatNuanceTextureEntry>();

        [Header("Color Palette Presets")]
        [SerializeField] private List<ColorPresetEntry> coatColorPresets = new List<ColorPresetEntry>();
        [SerializeField] private List<ColorPresetEntry> maneColorPresets = new List<ColorPresetEntry>();
        [SerializeField] private List<ColorPresetEntry> eyeColorPresets = new List<ColorPresetEntry>();

        public IReadOnlyList<BodyMeshEntry> BodyMeshes => bodyMeshes;
        public IReadOnlyList<HeadMeshEntry> HeadMeshes => headMeshes;
        public IReadOnlyList<HoofMeshEntry> HoofMeshes => hoofMeshes;
        public IReadOnlyList<ManeMeshEntry> ManeMeshes => maneMeshes;
        public IReadOnlyList<TailMeshEntry> TailMeshes => tailMeshes;
        public IReadOnlyList<HornMeshEntry> HornMeshes => hornMeshes;
        public IReadOnlyList<WingMeshEntry> WingMeshes => wingMeshes;

        public IReadOnlyList<FaceMarkTextureEntry> FaceMarkTextures => faceMarkTextures;
        public IReadOnlyList<LegMarkTextureEntry> LegMarkTextures => legMarkTextures;
        public IReadOnlyList<BodyPatternTextureEntry> BodyPatternTextures => bodyPatternTextures;
        public IReadOnlyList<CoatNuanceTextureEntry> CoatNuanceTextures => coatNuanceTextures;

        public IReadOnlyList<ColorPresetEntry> CoatColorPresets => coatColorPresets;
        public IReadOnlyList<ColorPresetEntry> ManeColorPresets => maneColorPresets;
        public IReadOnlyList<ColorPresetEntry> EyeColorPresets => eyeColorPresets;

        #region Lookups

        public GameObject GetBodyPrefab(BodyType type)
        {
            var match = bodyMeshes.Find(e => e.type == type);
            return match != null ? match.prefab : null;
        }

        public GameObject GetHeadPrefab(HeadType type)
        {
            var match = headMeshes.Find(e => e.type == type);
            return match != null ? match.prefab : null;
        }

        public GameObject GetHoofPrefab(HoofType type)
        {
            var match = hoofMeshes.Find(e => e.type == type);
            return match != null ? match.prefab : null;
        }

        public GameObject GetManePrefab(ManeType type)
        {
            var match = maneMeshes.Find(e => e.type == type);
            return match != null ? match.prefab : null;
        }

        public GameObject GetTailPrefab(TailType type)
        {
            var match = tailMeshes.Find(e => e.type == type);
            return match != null ? match.prefab : null;
        }

        public GameObject GetHornPrefab(HornType type)
        {
            var match = hornMeshes.Find(e => e.type == type);
            return match != null ? match.prefab : null;
        }

        public GameObject GetWingPrefab(WingType type)
        {
            var match = wingMeshes.Find(e => e.type == type);
            return match != null ? match.prefab : null;
        }

        public Texture2D GetFaceMarkTexture(FaceMarkType type)
        {
            var match = faceMarkTextures.Find(e => e.type == type);
            return match != null ? match.maskTexture : null;
        }

        public Texture2D GetLegMarkTexture(LegMarkType type)
        {
            var match = legMarkTextures.Find(e => e.type == type);
            return match != null ? match.maskTexture : null;
        }

        public Texture2D GetBodyPatternTexture(BodyPatternType type)
        {
            var match = bodyPatternTextures.Find(e => e.type == type);
            return match != null ? match.maskTexture : null;
        }

        public Texture2D GetCoatNuanceTexture(CoatNuanceType type)
        {
            var match = coatNuanceTextures.Find(e => e.type == type);
            return match != null ? match.maskTexture : null;
        }

        #endregion

        #region Editor Setup Helpers

        public void RegisterBody(BodyType type, string name, GameObject prefab, Sprite icon = null)
        {
            bodyMeshes.RemoveAll(e => e.type == type);
            bodyMeshes.Add(new BodyMeshEntry { type = type, displayName = name, prefab = prefab, icon = icon });
        }

        public void RegisterHead(HeadType type, string name, GameObject prefab, Sprite icon = null)
        {
            headMeshes.RemoveAll(e => e.type == type);
            headMeshes.Add(new HeadMeshEntry { type = type, displayName = name, prefab = prefab, icon = icon });
        }

        public void RegisterHoof(HoofType type, string name, GameObject prefab, Sprite icon = null)
        {
            hoofMeshes.RemoveAll(e => e.type == type);
            hoofMeshes.Add(new HoofMeshEntry { type = type, displayName = name, prefab = prefab, icon = icon });
        }

        public void RegisterMane(ManeType type, string name, GameObject prefab, Sprite icon = null)
        {
            maneMeshes.RemoveAll(e => e.type == type);
            maneMeshes.Add(new ManeMeshEntry { type = type, displayName = name, prefab = prefab, icon = icon });
        }

        public void RegisterTail(TailType type, string name, GameObject prefab, Sprite icon = null)
        {
            tailMeshes.RemoveAll(e => e.type == type);
            tailMeshes.Add(new TailMeshEntry { type = type, displayName = name, prefab = prefab, icon = icon });
        }

        public void RegisterHorn(HornType type, string name, GameObject prefab, Sprite icon = null)
        {
            hornMeshes.RemoveAll(e => e.type == type);
            hornMeshes.Add(new HornMeshEntry { type = type, displayName = name, prefab = prefab, icon = icon });
        }

        public void RegisterWing(WingType type, string name, GameObject prefab, Sprite icon = null)
        {
            wingMeshes.RemoveAll(e => e.type == type);
            wingMeshes.Add(new WingMeshEntry { type = type, displayName = name, prefab = prefab, icon = icon });
        }

        public void RegisterFaceMark(FaceMarkType type, string name, Texture2D tex, Sprite icon = null)
        {
            faceMarkTextures.RemoveAll(e => e.type == type);
            faceMarkTextures.Add(new FaceMarkTextureEntry { type = type, displayName = name, maskTexture = tex, icon = icon });
        }

        public void RegisterLegMark(LegMarkType type, string name, Texture2D tex, Sprite icon = null)
        {
            legMarkTextures.RemoveAll(e => e.type == type);
            legMarkTextures.Add(new LegMarkTextureEntry { type = type, displayName = name, maskTexture = tex, icon = icon });
        }

        public void RegisterBodyPattern(BodyPatternType type, string name, Texture2D tex, Sprite icon = null)
        {
            bodyPatternTextures.RemoveAll(e => e.type == type);
            bodyPatternTextures.Add(new BodyPatternTextureEntry { type = type, displayName = name, maskTexture = tex, icon = icon });
        }

        public void RegisterCoatNuance(CoatNuanceType type, string name, Texture2D tex, Sprite icon = null)
        {
            coatNuanceTextures.RemoveAll(e => e.type == type);
            coatNuanceTextures.Add(new CoatNuanceTextureEntry { type = type, displayName = name, maskTexture = tex, icon = icon });
        }

        public void AddPreset(string name, Color color, int category) // 0: Coat, 1: Mane, 2: Eye
        {
            var list = category == 0 ? coatColorPresets : (category == 1 ? maneColorPresets : eyeColorPresets);
            list.Add(new ColorPresetEntry { presetName = name, color = color });
        }

        #endregion
    }
}

