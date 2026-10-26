using System.Collections.Generic;
using UnityEngine;

namespace MangoPony.Customization
{
    /// <summary>
    /// Utilitaire de remappage osseux (Bone Remapping) pour lier dynamiquement des SkinnedMeshRenderers
    /// modulaires à une armature/squelette partagé sans briser le rigging ni l'animation.
    /// </summary>
    public static class SkinnedMeshBoneRemapper
    {
        /// <summary>
        /// Remappe tous les SkinnedMeshRenderers trouvés dans targetObject pour qu'ils utilisent les bones du sharedRoot.
        /// Nettoie ensuite les transforms superflus (anciennes armatures dupliquées).
        /// </summary>
        public static void RemapToSharedArmature(GameObject targetObject, Transform sharedRoot)
        {
            if (targetObject == null || sharedRoot == null) return;

            // Dictionnaire des bones du squelette principal : Nom du bone -> Transform
            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
            CollectBones(sharedRoot, boneMap);

            SkinnedMeshRenderer[] smrs = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach (var smr in smrs)
            {
                Transform[] currentBones = smr.bones;
                Transform[] remappedBones = new Transform[currentBones.Length];

                for (int i = 0; i < currentBones.Length; i++)
                {
                    Transform originalBone = currentBones[i];
                    if (originalBone != null && boneMap.TryGetValue(originalBone.name, out Transform matchingBone))
                    {
                        remappedBones[i] = matchingBone;
                    }
                    else
                    {
                        // Si le bone exact n'est pas trouvé, on garde l'original ou le sharedRoot en fallback
                        remappedBones[i] = originalBone != null ? originalBone : sharedRoot;
                    }
                }

                smr.bones = remappedBones;

                if (smr.rootBone != null && boneMap.TryGetValue(smr.rootBone.name, out Transform matchingRoot))
                {
                    smr.rootBone = matchingRoot;
                }
                else
                {
                    smr.rootBone = sharedRoot;
                }

                // Assure une mise à jour correcte des bounds du mesh
                smr.updateWhenOffscreen = true;
            }

            // Supprime les armatures locales dupliquées qui pourraient être restées dans le prefab instancié
            CleanDuplicateArmatures(targetObject, smrs);
        }

        private static void CollectBones(Transform current, Dictionary<string, Transform> map)
        {
            if (!map.ContainsKey(current.name))
            {
                map.Add(current.name, current);
            }

            for (int i = 0; i < current.childCount; i++)
            {
                CollectBones(current.GetChild(i), map);
            }
        }

        private static void CleanDuplicateArmatures(GameObject targetObject, SkinnedMeshRenderer[] smrs)
        {
            HashSet<Transform> smrTransforms = new HashSet<Transform>();
            foreach (var smr in smrs)
            {
                smrTransforms.Add(smr.transform);
            }

            List<Transform> toCheck = new List<Transform>();
            for (int i = 0; i < targetObject.transform.childCount; i++)
            {
                toCheck.Add(targetObject.transform.GetChild(i));
            }

            foreach (var child in toCheck)
            {
                string nameLower = child.name.ToLowerInvariant();
                // Si c'est une armature locale orpheline et qu'elle ne contient pas les SkinnedMeshRenderers eux-mêmes
                if ((nameLower.Contains("armature") || nameLower.Contains("root") || nameLower.Contains("hips")) && !smrTransforms.Contains(child))
                {
                    // Ne détruire que s'il n'y a aucun mesh descendant
                    if (child.GetComponentsInChildren<MeshRenderer>(true).Length == 0 &&
                        child.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 0)
                    {
                        Object.Destroy(child.gameObject);
                    }
                }
            }
        }
    }
}
