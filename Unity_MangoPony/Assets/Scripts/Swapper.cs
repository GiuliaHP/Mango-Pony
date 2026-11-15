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
        Mane,
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
        ManePattern,      // Motif crinière
        TailPattern       // Motif queue
    }

    [System.Serializable]
    public struct SlotBinding
    {
        public SlotType slotType;

        [Tooltip("Point d'ancrage / parent dans la hiérarchie du poney")]
        public Transform parentBone;

        [Tooltip("Objet déjà présent dans la scène au démarrage (ex: Body, Tail...) qui sera remplacé lors du swap")]
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

    [Tooltip("Optionnel : le SkinnedMeshRenderer principal du poney pour transférer les bones aux pièces riggées")]
    [SerializeField] private SkinnedMeshRenderer referenceRigRenderer;

    [Header("2. Configuration des Zones de Shaders")]
    [SerializeField] private List<MaterialZoneBinding> zoneBindings = new List<MaterialZoneBinding>();

    [Header("3. Transition des Motifs (DOTween)")]
    [Tooltip("Durée totale de la transition (moitié fade-out, moitié fade-in)")]
    [SerializeField] private float patternTransitionDuration = 0.3f;
    [SerializeField] private Ease patternEase = Ease.InOutSine;

    private Dictionary<SlotType, GameObject> currentEquippedObjects = new Dictionary<SlotType, GameObject>();
    private Dictionary<MaterialZone, Sequence> activePatternSequences = new Dictionary<MaterialZone, Sequence>();
    private Dictionary<string, Transform> rigBoneMap;

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

    /// <summary>
    /// Enregistre les objets déjà présents dans la scène pour qu'ils soient détruits au premier remplacement.
    /// </summary>
    private void RegisterInitialSceneObjects()
    {
        foreach (var binding in slotBindings)
        {
            if (binding.initialSceneObject != null && !currentEquippedObjects.ContainsKey(binding.slotType))
            {
                currentEquippedObjects[binding.slotType] = binding.initialSceneObject;
            }
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
                // 1. Équipe le Mesh par défaut
                if (element.shapes != null && element.defaultShapeIndex >= 0 && element.defaultShapeIndex < element.shapes.Count)
                {
                    SwapSlotMesh(element.targetMeshSlot, element.shapes[element.defaultShapeIndex].meshPrefab);
                }

                // 2. Applique le Pattern par défaut uniquement s'il y a une texture et que la zone est configurée
                if (element.patterns != null && element.defaultPatternIndex >= 0 && element.defaultPatternIndex < element.patterns.Count)
                {
                    Texture defaultTex = element.patterns[element.defaultPatternIndex].patternTexture;
                    if (defaultTex != null)
                    {
                        if (element.supportsFourLegsSync) SwapAllLegsPattern(defaultTex);
                        else SwapPatternTexture(element.targetMaterialZone, defaultTex);
                    }
                }

                // 3. Applique les Couleurs par défaut
                if (element.colorSettings != null && element.colorSettings.hasColors)
                {
                    foreach (var channel in element.colorSettings.colorChannels)
                    {
                        SwapZoneColor(element.targetMaterialZone, channel.defaultColor, channel.shaderPropertyName);
                    }
                }
            }
        }
    }

    #region Mesh Swapping

    /// <summary>
    /// Remplace le prefab actuellement équipé sur un emplacement spécifique (et détruit l'ancien, même si newPrefab est null).
    /// </summary>
    public void SwapSlotMesh(SlotType slot, GameObject newPrefab)
    {
        // 1. Supprime l'objet actuel (qu'il vienne de la scène initiale ou d'un swap précédent)
        if (currentEquippedObjects.TryGetValue(slot, out GameObject oldObj) && oldObj != null)
        {
            Destroy(oldObj);
        }
        currentEquippedObjects.Remove(slot);

        // 2. Si "None" (ex: Tail_None -> meshPrefab == null), on s'arrête ici : il n'y a plus rien !
        if (newPrefab == null) return;

        // 3. Instancie la nouvelle variante au bon endroit
        Transform parentAnchor = GetParentAnchor(slot);
        Transform targetParent = parentAnchor != null ? parentAnchor : transform;

        GameObject newObj = Instantiate(newPrefab, targetParent);
        newObj.name = newPrefab.name;
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.identity;
        newObj.transform.localScale = Vector3.one;

        // 4. Si le prefab contient un SkinnedMeshRenderer, on le raccorde au squelette du poney
        RebindSkinnedMeshesToRig(newObj);

        currentEquippedObjects[slot] = newObj;
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
                if (binding.parentBone != null)
                    return binding.parentBone;

                if (binding.initialSceneObject != null)
                    return binding.initialSceneObject.transform.parent;
            }
        }
        return null;
    }

    #endregion

    #region Material / Pattern Swapping

    public void SwapPatternTexture(MaterialZone zone, Texture newTexture)
    {
        if (!TryGetZoneBinding(zone, out MaterialZoneBinding binding))
        {
            Debug.LogWarning($"[Swapper] Aucune configuration trouvée pour la zone : {zone}");
            return;
        }

        List<Material> targetMaterials = GetTargetMaterials(binding);
        if (targetMaterials.Count == 0) return;

        string texPropName = string.IsNullOrEmpty(binding.shaderPropertyName) ? "_PatternTexture" : binding.shaderPropertyName;
        string intPropName = string.IsNullOrEmpty(binding.intensityPropertyName) ? "_Pattern_Intensity" : binding.intensityPropertyName;

        int texPropID = Shader.PropertyToID(texPropName);
        int intPropID = Shader.PropertyToID(intPropName);

        if (activePatternSequences.TryGetValue(zone, out Sequence existingSeq) && existingSeq != null && existingSeq.IsActive())
        {
            existingSeq.Kill();
        }

        float currentIntensity = 1f;
        if (targetMaterials[0].HasProperty(intPropID))
        {
            currentIntensity = targetMaterials[0].GetFloat(intPropID);
        }

        float halfDuration = patternTransitionDuration * 0.5f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        seq.Append(
            DOVirtual.Float(currentIntensity, 0f, halfDuration, val =>
            {
                SetMaterialsFloat(targetMaterials, intPropID, val);
            }).SetEase(patternEase)
        );

        seq.AppendCallback(() =>
        {
            SetMaterialsTexture(targetMaterials, texPropID, texPropName, newTexture);
        });

        float targetEndIntensity = newTexture != null ? 1f : 0f;
        if (targetEndIntensity > 0f)
        {
            seq.Append(
                DOVirtual.Float(0f, targetEndIntensity, halfDuration, val =>
                {
                    SetMaterialsFloat(targetMaterials, intPropID, val);
                }).SetEase(patternEase)
            );
        }

        activePatternSequences[zone] = seq;
    }

    public void SwapAllLegsPattern(Texture newTexture)
    {
        SwapPatternTexture(MaterialZone.FrontLeftLeg, newTexture);
        SwapPatternTexture(MaterialZone.FrontRightLeg, newTexture);
        SwapPatternTexture(MaterialZone.BackLeftLeg, newTexture);
        SwapPatternTexture(MaterialZone.BackRightLeg, newTexture);
    }

    public void SwapZoneColor(MaterialZone zone, Color newColor, string colorPropertyName = "_BaseColor")
    {
        if (!TryGetZoneBinding(zone, out MaterialZoneBinding binding)) return;

        List<Material> targetMaterials = GetTargetMaterials(binding);
        int colorPropID = Shader.PropertyToID(colorPropertyName);

        foreach (Material mat in targetMaterials)
        {
            if (mat.HasProperty(colorPropID))
                mat.SetColor(colorPropID, newColor);
        }
    }

    #region Helpers

    private List<Material> GetTargetMaterials(MaterialZoneBinding binding)
    {
        List<Material> results = new List<Material>();
        foreach (Renderer rend in binding.targetRenderers)
        {
            if (rend == null) continue;
            Material[] mats = rend.materials;
            if (binding.materialIndex >= 0 && binding.materialIndex < mats.Length)
            {
                results.Add(mats[binding.materialIndex]);
            }
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
            else
                Debug.LogWarning($"[Swapper] Le matériau '{mat.name}' ne possède pas la propriété '{propName}'.");
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
        {
            seq?.Kill();
        }
    }

    #endregion

    #endregion
}