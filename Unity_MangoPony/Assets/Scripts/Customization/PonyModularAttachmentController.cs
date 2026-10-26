using System;
using System.Collections.Generic;
using UnityEngine;
using MangoPony.Core;

namespace MangoPony.Customization
{
    public enum AttachmentSlot
    {
        Body = 0,
        Head = 1,
        Hooves = 2,
        Mane = 3,
        Tail = 4,
        Horns = 5,
        Wings = 6
    }

    /// <summary>
    /// Gestionnaire d'instanciation, d'attachement et de remplacement des meshes modulaires du Poney.
    /// Gère à la fois les meshes rigides sur sockets (cornes) et les SkinnedMeshes partagés (corps, tête, crinières, queues).
    /// </summary>
    public class PonyModularAttachmentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PonyAttachmentSockets sockets;

        // Dictionnaire des instances actives par slot
        private readonly Dictionary<AttachmentSlot, GameObject> activeSlotInstances = new Dictionary<AttachmentSlot, GameObject>();

        // Cache des renderers pour l'application des matériaux
        private readonly List<Renderer> cachedRenderers = new List<Renderer>();

        public event Action OnRenderersChanged;

        private void Awake()
        {
            if (sockets == null)
            {
                sockets = GetComponent<PonyAttachmentSockets>();
                if (sockets == null)
                {
                    sockets = gameObject.AddComponent<PonyAttachmentSockets>();
                }
            }
        }

        /// <summary>
        /// Attache ou remplace le prefab associé à un slot donné.
        /// Si prefab est null, le slot est vidé (utilisé pour les options 'None').
        /// </summary>
        public GameObject SetSlotPrefab(AttachmentSlot slot, GameObject prefab)
        {
            // 1. Nettoyer l'instance existante
            ClearSlot(slot);

            if (prefab == null)
            {
                RefreshRendererCache();
                return null;
            }

            // 2. Déterminer le transform parent selon le type de slot
            Transform targetParent = GetSocketForSlot(slot);

            // 3. Instancier le nouveau prefab
            GameObject newInstance = Instantiate(prefab, targetParent);
            newInstance.name = $"{slot}_{prefab.name}";

            // 4. Si le prefab contient des SkinnedMeshRenderers, remapper vers l'armature partagée
            SkinnedMeshRenderer[] smrs = newInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (smrs.Length > 0 && sockets != null && sockets.RootBone != null)
            {
                SkinnedMeshBoneRemapper.RemapToSharedArmature(newInstance, sockets.RootBone);
            }
            else
            {
                // Pour les objets rigides (cornes, accessoires), réinitialiser le positionnement local
                newInstance.transform.localPosition = Vector3.zero;
                newInstance.transform.localRotation = Quaternion.identity;
                newInstance.transform.localScale = Vector3.one;
            }

            activeSlotInstances[slot] = newInstance;

            RefreshRendererCache();
            return newInstance;
        }

        /// <summary>
        /// Supprime l'instance présente dans un slot.
        /// </summary>
        public void ClearSlot(AttachmentSlot slot)
        {
            if (activeSlotInstances.TryGetValue(slot, out GameObject existing) && existing != null)
            {
                Destroy(existing);
                activeSlotInstances.Remove(slot);
            }
        }

        /// <summary>
        /// Vide l'ensemble des slots d'attachements.
        /// </summary>
        public void ClearAllSlots()
        {
            foreach (var kvp in activeSlotInstances)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
            activeSlotInstances.Clear();
            RefreshRendererCache();
        }

        /// <summary>
        /// Retourne l'instance actuelle pour un slot donné.
        /// </summary>
        public GameObject GetSlotInstance(AttachmentSlot slot)
        {
            activeSlotInstances.TryGetValue(slot, out GameObject instance);
            return instance;
        }

        /// <summary>
        /// Retourne la liste de tous les renderers actifs sur l'ensemble des éléments modulaires.
        /// </summary>
        public IReadOnlyList<Renderer> GetAllRenderers()
        {
            return cachedRenderers;
        }

        /// <summary>
        /// Retourne les renderers associés à un slot spécifique.
        /// </summary>
        public List<Renderer> GetRenderersForSlot(AttachmentSlot slot)
        {
            List<Renderer> list = new List<Renderer>();
            if (activeSlotInstances.TryGetValue(slot, out GameObject obj) && obj != null)
            {
                obj.GetComponentsInChildren(true, list);
            }
            return list;
        }

        private Transform GetSocketForSlot(AttachmentSlot slot)
        {
            if (sockets == null) return transform;

            switch (slot)
            {
                case AttachmentSlot.Body:
                case AttachmentSlot.Head:
                case AttachmentSlot.Hooves:
                    return transform; // Ces éléments partagent l'armature globale

                case AttachmentSlot.Horns:
                    return sockets.HornSocket;

                case AttachmentSlot.Mane:
                    return sockets.ManeSocket;

                case AttachmentSlot.Tail:
                    return sockets.TailSocket;

                case AttachmentSlot.Wings:
                    return sockets.LeftWingSocket != null ? sockets.LeftWingSocket.parent : transform;

                default:
                    return transform;
            }
        }

        private void RefreshRendererCache()
        {
            cachedRenderers.Clear();
            GetComponentsInChildren(true, cachedRenderers);
            OnRenderersChanged?.Invoke();
        }
    }
}

