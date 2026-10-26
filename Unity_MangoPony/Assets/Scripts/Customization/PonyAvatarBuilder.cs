using System;
using UnityEngine;

namespace MangoPony.Customization
{
    /// <summary>
    /// Constructeur de squelette et d'armature partagée pour le Poney Mango Pony.
    /// Peut générer l'arborescence complète des os et sockets à l'exécution ou en éditeur.
    /// </summary>
    public static class PonyAvatarBuilder
    {
        public static GameObject CreateBasePonyRig(string name = "Pony_Rig_Mango")
        {
            GameObject rigRoot = new GameObject(name);
            var sockets = rigRoot.AddComponent<PonyAttachmentSockets>();

            // Hiérarchie du squelette
            Transform hips = CreateBone(rigRoot.transform, "Hips", new Vector3(0f, 0.85f, 0f));
            Transform spine = CreateBone(hips, "Spine", new Vector3(0f, 0.05f, 0.25f));
            Transform chest = CreateBone(spine, "Chest", new Vector3(0f, 0.08f, 0.25f));
            Transform neck = CreateBone(chest, "Neck", new Vector3(0f, 0.25f, 0.20f));
            Transform head = CreateBone(neck, "Head", new Vector3(0f, 0.20f, 0.25f));

            // Sockets Tête & Crinière
            Transform hornSocket = CreateBone(head, "Socket_Horn", new Vector3(0f, 0.25f, 0.15f));
            hornSocket.localRotation = Quaternion.Euler(20f, 0f, 0f);

            Transform maneSocket = CreateBone(neck, "Socket_Mane", new Vector3(0f, 0.10f, -0.05f));

            // Sockets Ailes (Gauche / Droite)
            Transform leftWingSocket = CreateBone(chest, "Socket_Wing_L", new Vector3(-0.35f, 0.15f, -0.1f));
            leftWingSocket.localRotation = Quaternion.Euler(0f, -25f, 15f);

            Transform rightWingSocket = CreateBone(chest, "Socket_Wing_R", new Vector3(0.35f, 0.15f, -0.1f));
            rightWingSocket.localRotation = Quaternion.Euler(0f, 25f, -15f);

            // Arrière-main & Queue
            Transform croup = CreateBone(hips, "Croup", new Vector3(0f, 0.05f, -0.35f));
            Transform tailSocket = CreateBone(croup, "Socket_Tail", new Vector3(0f, -0.05f, -0.15f));
            tailSocket.localRotation = Quaternion.Euler(-35f, 0f, 0f);

            // Membres antérieurs (Jambes avant)
            Transform frontLegL = CreateBone(chest, "Leg_Front_L", new Vector3(-0.25f, -0.2f, 0.1f));
            Transform hoofFL = CreateBone(frontLegL, "Socket_Hoof_FL", new Vector3(0f, -0.7f, 0f));

            Transform frontLegR = CreateBone(chest, "Leg_Front_R", new Vector3(0.25f, -0.2f, 0.1f));
            Transform hoofFR = CreateBone(frontLegR, "Socket_Hoof_FR", new Vector3(0f, -0.7f, 0f));

            // Membres postérieurs (Jambes arrière)
            Transform backLegL = CreateBone(croup, "Leg_Back_L", new Vector3(-0.25f, -0.1f, -0.05f));
            Transform hoofBL = CreateBone(backLegL, "Socket_Hoof_BL", new Vector3(0f, -0.75f, 0f));

            Transform backLegR = CreateBone(croup, "Leg_Back_R", new Vector3(0.25f, -0.1f, -0.05f));
            Transform hoofBR = CreateBone(backLegR, "Socket_Hoof_BR", new Vector3(0f, -0.75f, 0f));

            // Détection automatique des sockets
            sockets.AutoDetectSockets();

            return rigRoot;
        }

        private static Transform CreateBone(Transform parent, string name, Vector3 localPosition)
        {
            GameObject bone = new GameObject(name);
            bone.transform.SetParent(parent, false);
            bone.transform.localPosition = localPosition;
            bone.transform.localRotation = Quaternion.identity;
            bone.transform.localScale = Vector3.one;
            return bone.transform;
        }
    }
}

