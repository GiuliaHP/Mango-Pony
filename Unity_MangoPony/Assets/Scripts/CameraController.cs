using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public enum CameraControlMode
    {
        FullbodyTurntable, // Personnage tourne en X sans limite, caméra orbite en Y avec limites
        DetailOrbit        // Personnage fixe, seule la caméra orbite en X et Y avec limites autour du focusPoint
    }

    [System.Serializable]
    public struct CameraPreset
    {
        public string presetName;

        [Tooltip("Zones qui utilisent cette caméra (ex: Fullbody, Head, Legs, Rump)")]
        public List<CustomizationCategoryData.CameraFocusZone> focusZones;

        [Tooltip("La CinemachineCamera associée à ce preset")]
        public CinemachineCamera virtualCamera;

        [Tooltip("Mode d'interaction souris sur cette caméra")]
        public CameraControlMode controlMode;

        [Tooltip("Point autour duquel la caméra orbite (centre du corps pour Fullbody, ou tête/pattes/croupe pour Détail)")]
        public Transform focusPoint;

        [Header("Limites Verticales Caméra (Pitch / Y)")]
        public float minPitch;
        public float maxPitch;

        [Header("Limites Horizontales Caméra (Yaw / X - Mode Détail uniquement)")]
        public float minYaw;
        public float maxYaw;

        [Header("Zoom (FOV)")]
        public float defaultFOV;
        public float minFOV;
        public float maxFOV;
    }

    [Header("Priorités Cinemachine")]
    [SerializeField] private int inactivePriority = 10;
    [SerializeField] private int activePriority = 20;

    [Header("Presets de Caméras")]
    [SerializeField] private List<CameraPreset> presets = new List<CameraPreset>();

    private Tween zoomTween;
    private Vector3 initialCameraOffset;
    private Quaternion initialCameraRotation;
    private float currentYawOffset;
    private float currentPitchOffset;

    public CameraPreset ActivePreset { get; private set; }
    public bool HasActivePreset { get; private set; }
    public float CurrentTargetFOV { get; private set; }
    public float CurrentYawOffset => currentYawOffset;
    public float CurrentPitchOffset => currentPitchOffset;

    public void FocusOnZone(CustomizationCategoryData.CameraFocusZone zone, bool immediate = false)
    {
        for (int i = 0; i < presets.Count; i++)
        {
            var preset = presets[i];
            if (preset.focusZones == null || !preset.focusZones.Contains(zone) || preset.virtualCamera == null)
                continue;

            zoomTween?.Kill();

            // Désactive les autres caméras Cinemachine en baissant leur priorité
            foreach (var p in presets)
            {
                if (p.virtualCamera != null && p.virtualCamera != preset.virtualCamera)
                    p.virtualCamera.Priority = inactivePriority;
            }

            // Active la caméra cible
            preset.virtualCamera.Priority = activePriority;
            ActivePreset = preset;
            HasActivePreset = true;

            // Réinitialise l'orbite sur la position d'origine de cette caméra
            currentYawOffset = 0f;
            currentPitchOffset = 0f;

            Transform camTrans = preset.virtualCamera.transform;
            Vector3 pivot = preset.focusPoint != null ? preset.focusPoint.position : Vector3.zero;
            initialCameraOffset = camTrans.position - pivot;
            initialCameraRotation = camTrans.rotation;

            float targetFov = preset.defaultFOV > 0f ? preset.defaultFOV : preset.virtualCamera.Lens.FieldOfView;
            CurrentTargetFOV = targetFov;

            if (immediate)
            {
                var lens = preset.virtualCamera.Lens;
                lens.FieldOfView = targetFov;
                preset.virtualCamera.Lens = lens;
            }
            else
            {
                SetZoomFOV(targetFov, 4f);
            }

            return;
        }
    }

    /// <summary>
    /// Applique l'orbite de la CinemachineCamera active autour de son focusPoint.
    /// En mode FullbodyTurntable, yawAngle reste à 0 (c'est le personnage qui tourne).
    /// En mode DetailOrbit, yawAngle et pitchAngle font tourner la caméra autour du point de focus.
    /// </summary>
    public void SetCameraOrbitAngles(float yawAngle, float pitchAngle)
    {
        if (!HasActivePreset || ActivePreset.virtualCamera == null) return;

        currentYawOffset = yawAngle;
        currentPitchOffset = pitchAngle;

        Vector3 pivot = ActivePreset.focusPoint != null ? ActivePreset.focusPoint.position : Vector3.zero;

        Quaternion yawRot = Quaternion.AngleAxis(currentYawOffset, Vector3.up);
        Vector3 rightAxis = (yawRot * initialCameraRotation) * Vector3.right;
        Quaternion pitchRot = Quaternion.AngleAxis(currentPitchOffset, rightAxis);

        Quaternion combinedRot = pitchRot * yawRot;

        Transform camTrans = ActivePreset.virtualCamera.transform;
        camTrans.position = pivot + (combinedRot * initialCameraOffset);
        camTrans.rotation = combinedRot * initialCameraRotation;
    }

    public void SetZoomFOV(float newFOV, float damping = 10f)
    {
        if (!HasActivePreset || ActivePreset.virtualCamera == null) return;

        CurrentTargetFOV = newFOV;
        zoomTween?.Kill();

        float startFov = ActivePreset.virtualCamera.Lens.FieldOfView;
        float duration = Mathf.Clamp(1f / Mathf.Max(damping, 0.1f), 0.05f, 0.45f);

        zoomTween = DOVirtual.Float(startFov, newFOV, duration, value =>
        {
            if (ActivePreset.virtualCamera != null)
            {
                var lens = ActivePreset.virtualCamera.Lens;
                lens.FieldOfView = value;
                ActivePreset.virtualCamera.Lens = lens;
            }
        }).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    private void OnDestroy()
    {
        zoomTween?.Kill();
    }
}