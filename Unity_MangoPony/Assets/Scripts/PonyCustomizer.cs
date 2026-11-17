using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class PonyCustomizer : MonoBehaviour
{
    public enum Slot
    {
        Body,
        Head,
        Legs,
        Hooves,
        Mane,
        Forelock,
        Tail,
        Horn,
        Wings
    }

    public enum Zone
    {
        BodyPattern,
        CoatNuance,
        FaceMark,
        FrontLeftLeg,
        FrontRightLeg,
        BackLeftLeg,
        BackRightLeg,
        Mane,
        Tail,
        Horn,
        Wings,
        Hooves
    }

    public static readonly Zone[] LegZones = { Zone.FrontLeftLeg, Zone.FrontRightLeg, Zone.BackLeftLeg, Zone.BackRightLeg };

    [System.Serializable]
    public struct SlotBinding
    {
        [FormerlySerializedAs("slotType")] public Slot slot;
        [Tooltip("Parent transform of the meshes spawned in this slot")]
        [FormerlySerializedAs("parentBone")] public Transform anchor;
        [Tooltip("Object already in the scene, replaced on the first swap")]
        [FormerlySerializedAs("initialSceneObject")] public GameObject initialObject;
    }

    [System.Serializable]
    public struct ColorRemap
    {
        public string from;
        public string to;
    }

    [System.Serializable]
    public struct ZoneBinding
    {
        public Zone zone;
        [Tooltip("Slots whose equipped objects hold the material of this zone")]
        public List<Slot> slots;
        public int materialIndex;
        [Tooltip("Pattern texture property (default: _PatternTexture)")]
        [FormerlySerializedAs("shaderPropertyName")] public string textureProperty;
        [Tooltip("Pattern intensity property (default: _Pattern_Intensity)")]
        [FormerlySerializedAs("intensityPropertyName")] public string intensityProperty;
        [Tooltip("Redirects a color property sent by the UI to another shader property")]
        public List<ColorRemap> colorRemaps;
    }

    [Header("Data")]
    [SerializeField] private CategoryMenu menu;
    [FormerlySerializedAs("autoInitializeOnStart")] [SerializeField] private bool applyDefaultsOnStart = true;

    [Header("Meshes")]
    [SerializeField] private List<SlotBinding> slotBindings = new List<SlotBinding>();
    [FormerlySerializedAs("referenceRigRenderer")] [SerializeField] private SkinnedMeshRenderer rigRenderer;

    [Header("Material Zones")]
    [SerializeField] private List<ZoneBinding> zoneBindings = new List<ZoneBinding>();

    [Header("Pattern Fade")]
    [FormerlySerializedAs("patternTransitionDuration")] [SerializeField] private float patternFadeDuration = 0.3f;
    [FormerlySerializedAs("patternEase")] [SerializeField] private Ease patternFadeEase = Ease.InOutSine;

    private Dictionary<Slot, GameObject> equipped = new Dictionary<Slot, GameObject>();
    private Dictionary<Zone, Sequence> patternFades = new Dictionary<Zone, Sequence>();
    private Dictionary<string, Transform> boneMap;
    private Dictionary<Zone, Texture> zonePatterns = new Dictionary<Zone, Texture>();
    private Dictionary<(Zone, string), Color> zoneColors = new Dictionary<(Zone, string), Color>();

    private void Awake()
    {
        RegisterInitialObjects();
        BuildBoneMap();
    }

    private void Start()
    {
        if (applyDefaultsOnStart)
            ApplyDefaults();
    }

    private void RegisterInitialObjects()
    {
        foreach (var binding in slotBindings)
        {
            if (binding.initialObject != null && !equipped.ContainsKey(binding.slot))
                equipped[binding.slot] = binding.initialObject;
        }
    }

    private void BuildBoneMap()
    {
        if (rigRenderer == null) return;

        boneMap = new Dictionary<string, Transform>();
        foreach (Transform bone in rigRenderer.bones)
        {
            if (bone != null && !boneMap.ContainsKey(bone.name))
                boneMap[bone.name] = bone;
        }
    }

    public void ApplyDefaults()
    {
        CategoryMenu source = menu != null ? menu : FindAnyObjectByType<CategoryMenu>();
        if (source == null) return;

        foreach (var category in source.categories)
        {
            if (category == null) continue;

            foreach (var element in category.elements)
            {
                if (element.contentType != CategoryData.ContentType.ShaderOnly)
                {
                    if (IsValidIndex(element.shapes, element.defaultShapeIndex))
                        SetMesh(element.slot, element.shapes[element.defaultShapeIndex].prefab);

                    if (element.hasSecondaryShapes && IsValidIndex(element.secondaryShapes, element.defaultSecondaryShapeIndex))
                        SetMesh(element.secondarySlot, element.secondaryShapes[element.defaultSecondaryShapeIndex].prefab);
                }

                if (element.contentType != CategoryData.ContentType.MeshOnly && IsValidIndex(element.patterns, element.defaultPatternIndex))
                {
                    Texture texture = element.patterns[element.defaultPatternIndex].texture;
                    if (element.isLegs)
                        SetPattern(LegZones, texture);
                    else
                        SetPattern(element.zone, texture);
                }

                if (element.colors != null && element.colors.enabled)
                {
                    foreach (var channel in element.colors.channels)
                    {
                        if (element.isLegs)
                            SetColor(LegZones, channel.defaultColor, channel.property);
                        else
                            SetColor(element.zone, channel.defaultColor, channel.property);
                    }
                }
            }
        }
    }

    private static bool IsValidIndex<T>(List<T> list, int index)
        => list != null && index >= 0 && index < list.Count;

    #region Meshes

    public void SetMesh(Slot slot, GameObject prefab)
    {
        if (equipped.TryGetValue(slot, out GameObject oldObject) && oldObject != null)
            Destroy(oldObject);

        equipped.Remove(slot);

        if (prefab == null) return;

        Transform anchor = GetAnchor(slot);
        GameObject instance = Instantiate(prefab, anchor != null ? anchor : transform);
        instance.name = prefab.name;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        RebindToRig(instance);
        equipped[slot] = instance;

        ReapplyZones(slot);
    }

    private void ReapplyZones(Slot slot)
    {
        foreach (var binding in zoneBindings)
        {
            if (binding.slots == null || !binding.slots.Contains(slot)) continue;

            List<Material> materials = GetMaterials(binding);
            if (materials.Count == 0) continue;

            if (zonePatterns.TryGetValue(binding.zone, out Texture texture))
            {
                SetTexture(materials, GetTextureId(binding), texture);
                SetFloat(materials, GetIntensityId(binding), texture != null ? 1f : 0f);
            }

            foreach (var entry in zoneColors)
            {
                if (entry.Key.Item1 == binding.zone)
                    ApplyColor(materials, binding, entry.Key.Item2, entry.Value);
            }
        }
    }

    private void RebindToRig(GameObject instance)
    {
        if (rigRenderer == null || boneMap == null || boneMap.Count == 0) return;

        foreach (SkinnedMeshRenderer skinned in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            Transform[] oldBones = skinned.bones;
            Transform[] newBones = new Transform[oldBones.Length];

            for (int i = 0; i < oldBones.Length; i++)
            {
                if (oldBones[i] != null && boneMap.TryGetValue(oldBones[i].name, out Transform mapped))
                    newBones[i] = mapped;
                else
                    newBones[i] = oldBones[i];
            }

            skinned.bones = newBones;
            if (rigRenderer.rootBone != null)
                skinned.rootBone = rigRenderer.rootBone;
        }
    }

    private Transform GetAnchor(Slot slot)
    {
        foreach (var binding in slotBindings)
        {
            if (binding.slot != slot) continue;

            if (binding.anchor != null) return binding.anchor;
            if (binding.initialObject != null) return binding.initialObject.transform.parent;
        }
        return null;
    }

    #endregion

    #region Patterns & Colors

    public void SetPattern(Zone zone, Texture texture)
    {
        zonePatterns[zone] = texture;

        if (!TryGetBinding(zone, out ZoneBinding binding)) return;

        List<Material> materials = GetMaterials(binding);
        if (materials.Count == 0) return;

        int textureId = GetTextureId(binding);
        int intensityId = GetIntensityId(binding);

        if (patternFades.TryGetValue(zone, out Sequence running) && running != null && running.IsActive())
            running.Kill();

        float startIntensity = materials[0].HasProperty(intensityId) ? materials[0].GetFloat(intensityId) : 1f;
        float halfDuration = patternFadeDuration * 0.5f;

        Sequence fade = DOTween.Sequence().SetUpdate(true);

        fade.Append(DOVirtual.Float(startIntensity, 0f, halfDuration, v => SetFloat(materials, intensityId, v))
                             .SetEase(patternFadeEase));

        fade.AppendCallback(() => SetTexture(materials, textureId, texture));

        if (texture != null)
        {
            fade.Append(DOVirtual.Float(0f, 1f, halfDuration, v => SetFloat(materials, intensityId, v))
                                 .SetEase(patternFadeEase));
        }

        patternFades[zone] = fade;
    }

    public void SetPattern(IEnumerable<Zone> zones, Texture texture)
    {
        foreach (var zone in zones)
            SetPattern(zone, texture);
    }

    public void SetColor(Zone zone, Color color, string property = "_BaseColor")
    {
        zoneColors[(zone, property)] = color;

        if (!TryGetBinding(zone, out ZoneBinding binding)) return;

        ApplyColor(GetMaterials(binding), binding, property, color);
    }

    private static void ApplyColor(List<Material> materials, ZoneBinding binding, string property, Color color)
    {
        string target = property;
        if (binding.colorRemaps != null)
        {
            foreach (var remap in binding.colorRemaps)
            {
                if (remap.from == property)
                {
                    target = remap.to;
                    break;
                }
            }
        }

        int colorId = Shader.PropertyToID(target);
        foreach (Material material in materials)
        {
            if (material.HasProperty(colorId))
                material.SetColor(colorId, color);
        }
    }

    private static int GetTextureId(ZoneBinding binding)
        => Shader.PropertyToID(string.IsNullOrEmpty(binding.textureProperty) ? "_PatternTexture" : binding.textureProperty);

    private static int GetIntensityId(ZoneBinding binding)
        => Shader.PropertyToID(string.IsNullOrEmpty(binding.intensityProperty) ? "_Pattern_Intensity" : binding.intensityProperty);

    public void SetColor(IEnumerable<Zone> zones, Color color, string property = "_BaseColor")
    {
        foreach (var zone in zones)
            SetColor(zone, color, property);
    }

    public Texture GetPattern(Zone zone)
        => zonePatterns.TryGetValue(zone, out var texture) ? texture : null;

    public Color GetColor(Zone zone, string property = "_BaseColor")
        => zoneColors.TryGetValue((zone, property), out var color) ? color : Color.white;

    private List<Material> GetMaterials(ZoneBinding binding)
    {
        List<Material> results = new List<Material>();
        if (binding.slots == null) return results;

        foreach (Slot slot in binding.slots)
        {
            if (!equipped.TryGetValue(slot, out GameObject instance) || instance == null) continue;

            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.materials;
                if (binding.materialIndex >= 0 && binding.materialIndex < materials.Length)
                    results.Add(materials[binding.materialIndex]);
            }
        }
        return results;
    }

    private static void SetFloat(List<Material> materials, int propertyId, float value)
    {
        foreach (Material material in materials)
        {
            if (material.HasProperty(propertyId))
                material.SetFloat(propertyId, value);
        }
    }

    private static void SetTexture(List<Material> materials, int propertyId, Texture texture)
    {
        foreach (Material material in materials)
        {
            if (material.HasProperty(propertyId))
                material.SetTexture(propertyId, texture);
        }
    }

    private bool TryGetBinding(Zone zone, out ZoneBinding result)
    {
        foreach (var binding in zoneBindings)
        {
            if (binding.zone == zone)
            {
                result = binding;
                return true;
            }
        }
        result = default;
        return false;
    }

    #endregion

    private void OnDestroy()
    {
        foreach (var fade in patternFades.Values)
            fade?.Kill();
    }
}
