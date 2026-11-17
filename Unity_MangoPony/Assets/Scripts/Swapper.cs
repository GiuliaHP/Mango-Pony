using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Swapper : MonoBehaviour
{
    #region Enums & Structs

    public enum SlotType
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

    public enum MaterialZone
    {
        BodyPattern,      // Marques du corps (Appaloosa, Tobiano...)
        CoatNuance,       // Répartition (Pommelé, Pangaré...)
        FaceMark,         // Marque du front (Étoile, Liste...)
        FrontLeftLeg,     // Patte avant gauche
        FrontRightLeg,    // Patte avant droite
        BackLeftLeg,      // Patte arrière gauche
        BackRightLeg,     // Patte arrière droite
        ManePattern,      // Crinière + Toupet (partagent le même matériau)
        TailPattern,      // Queue
        HornColor,        // Corne
        WingsColor,       // Ailes
        HoovesColor       // Sabots
    }

    [System.Serializable]
    public struct SlotBinding
    {
        public SlotType slotType;
        [Tooltip("Point d'ancrage / parent dans la hiérarchie du poney")]
        public Transform parentBone;
        [Tooltip("Objet déjà présent dans la scène au démarrage qui sera remplacé lors du swap")]
        public GameObject initialSceneObject;
    }

    [System.Serializable]
    public struct MaterialZoneBinding
    {
        public MaterialZone zone;
        [Tooltip("Les Renderers (SkinnedMeshRenderer / MeshRenderer) concernés par cette zone")]
        public List<Renderer> targetRenderers;
        [Tooltip("Index du matériau sur le Renderer (0 par défaut)")]
        public int materialIndex;
        [Tooltip("Nom de la propriété Texture dans le Shader Graph (ex: _PatternTexture)")]
        public string shaderPropertyName;
        [Tooltip("Nom de la propriété Float d'intensité dans le Shader Graph (ex: _Pattern_Intensity)")]
        public string intensityPropertyName;
    }

    #endregion

    [Header("0. Source de Données (Auto-assemblage du Poney)")]
    [SerializeField] private List<CustomizationCategoryData> categoriesData = new List<CustomizationCategoryData>();
    [SerializeField] private bool autoInitializeOnStart = true;

    [Header("1. Configuration des Meshes Modulaires")]
    [SerializeField] private List<SlotBinding> slotBindings = new List<SlotBinding>();
    [SerializeField] private SkinnedMeshRenderer referenceRigRenderer;

    [Header("2. Configuration des Zones de Shaders")]
    [SerializeField] private List<MaterialZoneBinding> zoneBindings = new List<MaterialZoneBinding>();

    [Header("3. Transition des Motifs (DOTween)")]
    [SerializeField] private float patternTransitionDuration = 0.3f;
    [SerializeField] private Ease patternEase = Ease.InOutSine;

    private Dictionary<SlotType, GameObject> currentEquippedObjects = new Dictionary<SlotType, GameObject>();
    private Dictionary<MaterialZone, Sequence> activePatternSequences = new Dictionary<MaterialZone, Sequence>();
    private Dictionary<string, Transform> rigBoneMap;
    private Dictionary<MaterialZone, Texture> currentZoneTextures = new Dictionary<MaterialZone, Texture>();
    private Dictionary<(MaterialZone, string), Color> currentZoneColors = new Dictionary<(MaterialZone, string), Color>();

    private void Awake()
    {
        RegisterInitialSceneObjects();
        BuildRigBoneMap();
    }

    private void Start()
    {
        if (autoInitializeOnStart)
            InitializeFromCategories();
    }

    private void RegisterInitialSceneObjects()
    {
        foreach (var binding in slotBindings)
        {
            if (binding.initialSceneObject != null && !currentEquippedObjects.ContainsKey(binding.slotType))
                currentEquippedObjects[binding.slotType] = binding.initialSceneObject;
        }
    }

    private void BuildRigBoneMap()
    {
        if (referenceRigRenderer == null) return;

        rigBoneMap = new Dictionary<string, Transform>();
        foreach (Transform bone in referenceRigRenderer.bones)
        {
            if (bone != null && !rigBoneMap.ContainsKey(bone.name))
                rigBoneMap[bone.name] = bone;
        }
    }

    public void InitializeFromCategories()
    {
        foreach (var category in categoriesData)
        {
            if (category == null) continue;

            foreach (var element in category.elements)
            {
                bool hasMesh = element.contentType == CustomizationCategoryData.ElementContentType.MeshOnly ||
                               element.contentType == CustomizationCategoryData.ElementContentType.MeshAndShader;

                if (hasMesh)
                {
                    if (element.shapes != null && element.defaultShapeIndex >= 0 && element.defaultShapeIndex < element.shapes.Count)
                        SwapSlotMesh(element.targetMeshSlot, element.shapes[element.defaultShapeIndex].meshPrefab);

                    if (element.hasSecondaryShapes && element.secondaryShapes != null &&
                        element.defaultSecondaryShapeIndex >= 0 && element.defaultSecondaryShapeIndex < element.secondaryShapes.Count)
                    {
                        SwapSlotMesh(element.secondaryMeshSlot, element.secondaryShapes[element.defaultSecondaryShapeIndex].meshPrefab);
                    }
                }

                if (element.contentType != CustomizationCategoryData.ElementContentType.MeshOnly &&
                    element.patterns != null && element.defaultPatternIndex >= 0 && element.defaultPatternIndex < element.patterns.Count)
                {
                    Texture defaultTex = element.patterns[element.defaultPatternIndex].patternTexture;
                    if (element.isLegsPatternElement)
                    {
                        SwapZonesPattern(new[] { MaterialZone.FrontLeftLeg, MaterialZone.FrontRightLeg, MaterialZone.BackLeftLeg, MaterialZone.BackRightLeg }, defaultTex);
                    }
                    else
                    {
                        SwapPatternTexture(element.targetMaterialZone, defaultTex);
                    }
                }

                if (element.colorSettings != null && element.colorSettings.hasColors)
                {
                    foreach (var channel in element.colorSettings.colorChannels)
                    {
                        if (element.isLegsPatternElement)
                        {
                            SwapZonesColor(new[] { MaterialZone.FrontLeftLeg, MaterialZone.FrontRightLeg, MaterialZone.BackLeftLeg, MaterialZone.BackRightLeg },
                                           channel.defaultColor, channel.shaderPropertyName);
                        }
                        else
                        {
                            SwapZoneColor(element.targetMaterialZone, channel.defaultColor, channel.shaderPropertyName);
                        }
                    }
                }
            }
        }
    }

    #region Mesh Swapping

    public void SwapSlotMesh(SlotType slot, GameObject newPrefab)
    {
        if (currentEquippedObjects.TryGetValue(slot, out GameObject oldObj) && oldObj != null)
            Destroy(oldObj);

        currentEquippedObjects.Remove(slot);

        if (newPrefab == null) return;

        Transform parentAnchor = GetParentAnchor(slot);
        Transform targetParent = parentAnchor != null ? parentAnchor : transform;

        GameObject newObj = Instantiate(newPrefab, targetParent);
        newObj.name = newPrefab.name;
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.identity;
        newObj.transform.localScale = Vector3.one;

        RebindSkinnedMeshesToRig(newObj);
        currentEquippedObjects[slot] = newObj;

        // Synchronise les Renderers dynamiques avec la zone matérielle correspondante
        SyncDynamicSlotRenderers(slot, newObj);
    }

    private void SyncDynamicSlotRenderers(SlotType slot, GameObject newObj)
    {
        MaterialZone? targetZone = slot switch
        {
            SlotType.Mane => MaterialZone.ManePattern,
            SlotType.Forelock => MaterialZone.ManePattern,
            SlotType.Tail => MaterialZone.TailPattern,
            SlotType.Horn => MaterialZone.HornColor,
            SlotType.Wings => MaterialZone.WingsColor,
            SlotType.Hooves => MaterialZone.HoovesColor,
            _ => null
        };

        if (!targetZone.HasValue) return;

        Renderer[] newRenderers = newObj.GetComponentsInChildren<Renderer>(true);
        if (newRenderers.Length == 0) return;

        for (int i = 0; i < zoneBindings.Count; i++)
        {
            if (zoneBindings[i].zone != targetZone.Value) continue;

            var binding = zoneBindings[i];
            binding.targetRenderers.RemoveAll(r => r == null);

            // Récupère l'instance de matériau existante (ex: pour que Forelock partage exactement le matériau de Mane)
            Material sharedRuntimeMat = null;
            foreach (var existingRend in binding.targetRenderers)
            {
                if (existingRend != null && binding.materialIndex < existingRend.materials.Length)
                {
                    sharedRuntimeMat = existingRend.materials[binding.materialIndex];
                    break;
                }
            }

            foreach (var rend in newRenderers)
            {
                if (sharedRuntimeMat != null && binding.materialIndex < rend.materials.Length)
                {
                    Material[] mats = rend.materials;
                    mats[binding.materialIndex] = sharedRuntimeMat;
                    rend.materials = mats;
                }

                if (!binding.targetRenderers.Contains(rend))
                    binding.targetRenderers.Add(rend);
            }

            zoneBindings[i] = binding;
            break;
        }
    }

    private void RebindSkinnedMeshesToRig(GameObject instance)
    {
        if (referenceRigRenderer == null || rigBoneMap == null || rigBoneMap.Count == 0) return;

        SkinnedMeshRenderer[] smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in smrs)
        {
            Transform[] oldBones = smr.bones;
            Transform[] newBones = new Transform[oldBones.Length];

            for (int i = 0; i < oldBones.Length; i++)
            {
                if (oldBones[i] != null && rigBoneMap.TryGetValue(oldBones[i].name, out Transform mappedBone))
                    newBones[i] = mappedBone;
                else
                    newBones[i] = oldBones[i];
            }

            smr.bones = newBones;
            if (referenceRigRenderer.rootBone != null)
                smr.rootBone = referenceRigRenderer.rootBone;
        }
    }

    private Transform GetParentAnchor(SlotType slot)
    {
        foreach (var binding in slotBindings)
        {
            if (binding.slotType == slot)
            {
                if (binding.parentBone != null) return binding.parentBone;
                if (binding.initialSceneObject != null) return binding.initialSceneObject.transform.parent;
            }
        }
        return null;
    }

    #endregion

    #region Material / Pattern Swapping

    public void SwapPatternTexture(MaterialZone zone, Texture newTexture)
    {
        currentZoneTextures[zone] = newTexture;

        if (!TryGetZoneBinding(zone, out MaterialZoneBinding binding))
            return;

        List<Material> targetMaterials = GetTargetMaterials(binding);
        if (targetMaterials.Count == 0) return;

        string texPropName = string.IsNullOrEmpty(binding.shaderPropertyName) ? "_PatternTexture" : binding.shaderPropertyName;
        string intPropName = string.IsNullOrEmpty(binding.intensityPropertyName) ? "_Pattern_Intensity" : binding.intensityPropertyName;

        int texPropID = Shader.PropertyToID(texPropName);
        int intPropID = Shader.PropertyToID(intPropName);

        if (activePatternSequences.TryGetValue(zone, out Sequence existingSeq) && existingSeq != null && existingSeq.IsActive())
            existingSeq.Kill();

        float currentIntensity = targetMaterials[0].HasProperty(intPropID) ? targetMaterials[0].GetFloat(intPropID) : 1f;
        float halfDuration = patternTransitionDuration * 0.5f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        seq.Append(
            DOVirtual.Float(currentIntensity, 0f, halfDuration, val => SetMaterialsFloat(targetMaterials, intPropID, val))
                     .SetEase(patternEase)
        );

        seq.AppendCallback(() => SetMaterialsTexture(targetMaterials, texPropID, texPropName, newTexture));

        float targetEndIntensity = newTexture != null ? 1f : 0f;
        if (targetEndIntensity > 0f)
        {
            seq.Append(
                DOVirtual.Float(0f, targetEndIntensity, halfDuration, val => SetMaterialsFloat(targetMaterials, intPropID, val))
                         .SetEase(patternEase)
            );
        }

        activePatternSequences[zone] = seq;
    }

    public void SwapZonesPattern(IEnumerable<MaterialZone> zones, Texture newTexture)
    {
        foreach (var zone in zones)
            SwapPatternTexture(zone, newTexture);
    }

    public void SwapZoneColor(MaterialZone zone, Color newColor, string colorPropertyName = "_BaseColor")
    {
        currentZoneColors[(zone, colorPropertyName)] = newColor;

        if (!TryGetZoneBinding(zone, out MaterialZoneBinding binding)) return;

        List<Material> targetMaterials = GetTargetMaterials(binding);
        int colorPropID = Shader.PropertyToID(colorPropertyName);

        foreach (Material mat in targetMaterials)
        {
            if (mat.HasProperty(colorPropID))
                mat.SetColor(colorPropID, newColor);
        }
    }

    public void SwapZonesColor(IEnumerable<MaterialZone> zones, Color newColor, string colorPropertyName = "_BaseColor")
    {
        foreach (var zone in zones)
            SwapZoneColor(zone, newColor, colorPropertyName);
    }

    public Texture GetZoneTexture(MaterialZone zone) 
        => currentZoneTextures.TryGetValue(zone, out var tex) ? tex : null;

    public Color GetZoneColor(MaterialZone zone, string colorPropertyName = "_BaseColor") 
        => currentZoneColors.TryGetValue((zone, colorPropertyName), out var col) ? col : Color.white;

    private List<Material> GetTargetMaterials(MaterialZoneBinding binding)
    {
        List<Material> results = new List<Material>();
        foreach (Renderer rend in binding.targetRenderers)
        {
            if (rend == null) continue;
            Material[] mats = rend.materials;
            if (binding.materialIndex >= 0 && binding.materialIndex < mats.Length)
                results.Add(mats[binding.materialIndex]);
        }
        return results;
    }

    private void SetMaterialsFloat(List<Material> materials, int propertyID, float value)
    {
        foreach (Material mat in materials)
        {
            if (mat.HasProperty(propertyID))
                mat.SetFloat(propertyID, value);
        }
    }

    private void SetMaterialsTexture(List<Material> materials, int propertyID, string propName, Texture tex)
    {
        foreach (Material mat in materials)
        {
            if (mat.HasProperty(propertyID))
                mat.SetTexture(propertyID, tex);
        }
    }

    private bool TryGetZoneBinding(MaterialZone zone, out MaterialZoneBinding result)
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

    private void OnDestroy()
    {
        foreach (var seq in activePatternSequences.Values)
            seq?.Kill();
    }

    #endregion
}