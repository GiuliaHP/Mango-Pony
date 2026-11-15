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
        Hooves,
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
        public Transform parentBone; // Point d'ancrage sur l'armature/rig
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

    private void Start()
    {
        if (autoInitializeOnStart)
            InitializeFromCategories();
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

                // 2. Applique le Pattern par défaut
                if (element.patterns != null && element.defaultPatternIndex >= 0 && element.defaultPatternIndex < element.patterns.Count)
                {
                    Texture defaultTex = element.patterns[element.defaultPatternIndex].patternTexture;
                    if (element.supportsFourLegsSync) SwapAllLegsPattern(defaultTex);
                    else SwapPatternTexture(element.targetMaterialZone, defaultTex);
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

    [Header("1. Configuration des Meshes Modulaires")]
    [SerializeField] private List<SlotBinding> slotBindings = new List<SlotBinding>();

    [Header("2. Configuration des Zones de Shaders")]
    [SerializeField] private List<MaterialZoneBinding> zoneBindings = new List<MaterialZoneBinding>();

    [Header("3. Transition des Motifs (DOTween)")]
    [Tooltip("Durée totale de la transition (moitié fade-out, moitié fade-in)")]
    [SerializeField] private float patternTransitionDuration = 0.3f;
    [SerializeField] private Ease patternEase = Ease.InOutSine;

    private Dictionary<SlotType, GameObject> currentEquippedObjects = new Dictionary<SlotType, GameObject>();
    
    // Stocke les séquences actives par zone pour éviter les superpositions si le joueur clique vite
    private Dictionary<MaterialZone, Sequence> activePatternSequences = new Dictionary<MaterialZone, Sequence>();

    #region Mesh Swapping

    /// <summary>
    /// Remplace le prefab actuellement équipé sur un emplacement spécifique.
    /// </summary>
    public void SwapSlotMesh(SlotType slot, GameObject newPrefab)
    {
        if (currentEquippedObjects.TryGetValue(slot, out GameObject oldObj) && oldObj != null)
        {
            Destroy(oldObj);
            currentEquippedObjects.Remove(slot);
        }

        if (newPrefab == null) return;

        Transform parentAnchor = GetParentAnchor(slot);
        Transform targetParent = parentAnchor != null ? parentAnchor : transform;

        GameObject newObj = Instantiate(newPrefab, targetParent);
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.identity;
        newObj.transform.localScale = Vector3.one;

        currentEquippedObjects[slot] = newObj;
    }

    private Transform GetParentAnchor(SlotType slot)
    {
        foreach (var binding in slotBindings)
        {
            if (binding.slotType == slot)
                return binding.parentBone;
        }
        return null;
    }

    #endregion

    #region Material / Pattern Swapping

    /// <summary>
    /// Anime _Pattern_Intensity de 1 à 0, change la texture, puis anime de 0 à 1.
    /// </summary>
    public void SwapPatternTexture(MaterialZone zone, Texture newTexture)
    {
        if (!TryGetZoneBinding(zone, out MaterialZoneBinding binding))
        {
            Debug.LogWarning($"[Swapper] Aucune configuration trouvée pour la zone : {zone}");
            return;
        }

        // Récupérer la liste valide des instances de matériaux ciblés
        List<Material> targetMaterials = GetTargetMaterials(binding);
        if (targetMaterials.Count == 0) return;

        string texPropName = string.IsNullOrEmpty(binding.shaderPropertyName) ? "_PatternTexture" : binding.shaderPropertyName;
        string intPropName = string.IsNullOrEmpty(binding.intensityPropertyName) ? "_Pattern_Intensity" : binding.intensityPropertyName;

        int texPropID = Shader.PropertyToID(texPropName);
        int intPropID = Shader.PropertyToID(intPropName);

        // Si une transition est déjà en cours sur cette zone, on l'interrompt proprement
        if (activePatternSequences.TryGetValue(zone, out Sequence existingSeq) && existingSeq != null && existingSeq.IsActive())
        {
            existingSeq.Kill();
        }

        // Récupérer l'intensité actuelle (utile si le joueur interrompt une transition en cours)
        float currentIntensity = 1f;
        if (targetMaterials[0].HasProperty(intPropID))
        {
            currentIntensity = targetMaterials[0].GetFloat(intPropID);
        }

        float halfDuration = patternTransitionDuration * 0.5f;

        // Création de la séquence DOTween
        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // Étape 1 : Fade-out (Intensité actuelle -> 0)
        seq.Append(
            DOVirtual.Float(currentIntensity, 0f, halfDuration, val =>
            {
                SetMaterialsFloat(targetMaterials, intPropID, val);
            }).SetEase(patternEase)
        );

        // Étape 2 : Changement de la texture au moment exact où l'intensité est à 0
        seq.AppendCallback(() =>
        {
            SetMaterialsTexture(targetMaterials, texPropID, texPropName, newTexture);
        });

        // Étape 3 : Fade-in (0 -> 1) — Sauf si newTexture est null (option "Aucun motif"), on reste à 0 !
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

    /// <summary>
    /// Applique d'un seul coup le même motif aux 4 pattes avec la transition fluide.
    /// </summary>
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