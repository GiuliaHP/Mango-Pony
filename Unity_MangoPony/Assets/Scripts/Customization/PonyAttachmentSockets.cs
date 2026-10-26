using System;
using UnityEngine;

namespace MangoPony.Customization
{
    /// <summary>
    /// Définit les points d'ancrage (Sockets) et le squelette de référence (Armature)
    /// pour attacher les éléments modulaires (Cornes, Ailes, Crinières, Queues, Sabots).
    /// </summary>
    public class PonyAttachmentSockets : MonoBehaviour
    {
        [Header("Root & Armature")]
        [Tooltip("Transform racine de l'armature / squelette (ex: Hips, Root, Armature)")]
        [SerializeField] private Transform rootBone;

        [Header("Rigid Attachment Sockets")]
        [Tooltip("Point d'ancrage au niveau du front pour les cornes")]
        [SerializeField] private Transform hornSocket;

        [Tooltip("Point d'ancrage le long de l'encolure pour la crinière")]
        [SerializeField] private Transform maneSocket;

        [Tooltip("Point d'ancrage au croupion pour la queue")]
        [SerializeField] private Transform tailSocket;

        [Tooltip("Point d'ancrage de l'aile gauche (scapula / dos)")]
        [SerializeField] private Transform leftWingSocket;

        [Tooltip("Point d'ancrage de l'aile droite")]
        [SerializeField] private Transform rightWingSocket;

        [Header("Head & Face Socket")]
        [Tooltip("Point d'ancrage de la tête (pour morphologie ou accessoires de tête)")]
        [SerializeField] private Transform headSocket;

        [Header("Hoof Sockets")]
        [SerializeField] private Transform frontLeftHoofSocket;
        [SerializeField] private Transform frontRightHoofSocket;
        [SerializeField] private Transform backLeftHoofSocket;
        [SerializeField] private Transform backRightHoofSocket;

        public Transform RootBone => rootBone != null ? rootBone : transform;
        public Transform HornSocket => hornSocket != null ? hornSocket : (headSocket != null ? headSocket : transform);
        public Transform ManeSocket => maneSocket != null ? maneSocket : transform;
        public Transform TailSocket => tailSocket != null ? tailSocket : transform;
        public Transform LeftWingSocket => leftWingSocket != null ? leftWingSocket : transform;
        public Transform RightWingSocket => rightWingSocket != null ? rightWingSocket : transform;
        public Transform HeadSocket => headSocket != null ? headSocket : transform;
        public Transform FrontLeftHoofSocket => frontLeftHoofSocket;
        public Transform FrontRightHoofSocket => frontRightHoofSocket;
        public Transform BackLeftHoofSocket => backLeftHoofSocket;
        public Transform BackRightHoofSocket => backRightHoofSocket;

        private void Reset()
        {
            AutoDetectSockets();
        }

        /// <summary>
        /// Tente de détecter automatiquement les sockets par convention de nommage dans la hiérarchie.
        /// </summary>
        public void AutoDetectSockets()
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);

            foreach (var child in allChildren)
            {
                string nameLower = child.name.ToLowerInvariant();

                if (rootBone == null && (nameLower.Contains("root") || nameLower.Contains("hips") || nameLower.Contains("armature") || nameLower.Contains("pelvis")))
                    rootBone = child;

                if (headSocket == null && (nameLower == "head" || nameLower.EndsWith("_head") || nameLower.Contains("tete")))
                    headSocket = child;

                if (hornSocket == null && (nameLower.Contains("horn") || nameLower.Contains("corne") || nameLower.Contains("forehead")))
                    hornSocket = child;

                if (maneSocket == null && (nameLower.Contains("mane") || nameLower.Contains("neck") || nameLower.Contains("criniere") || nameLower.Contains("encolure")))
                    maneSocket = child;

                if (tailSocket == null && (nameLower.Contains("tail") || nameLower.Contains("queue") || nameLower.Contains("croup")))
                    tailSocket = child;

                if (leftWingSocket == null && ((nameLower.Contains("wing") || nameLower.Contains("aile")) && (nameLower.Contains("l") || nameLower.Contains("left") || nameLower.Contains("gauche"))))
                    leftWingSocket = child;

                if (rightWingSocket == null && ((nameLower.Contains("wing") || nameLower.Contains("aile")) && (nameLower.Contains("r") || nameLower.Contains("right") || nameLower.Contains("droite"))))
                    rightWingSocket = child;
            }
        }
    }
}

