using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public enum CameraControlMode
    {
        FullbodyTurntable,
        DetailOrbit
    }

    [System.Serializable]
    public struct CameraPreset
    {
        public string presetName;
        public List<CustomizationCategoryData.CameraFocusZone> focusZones;
        public CinemachineCamera virtualCamera;
        public CameraControlMode controlMode;
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

    [Header("Sortie de Zoom Détail")]
    [Tooltip("Bouton UI affiché uniquement quand une caméra détail est active pour revenir en vue Fullbody")]
    [SerializeField] private Button resetViewButton;

    [Header("Presets de Caméras")]
    [SerializeField] private List<CameraPreset> presets = new List<CameraPreset>();

    private Tween zoomTween;
    private Vector3 initialCameraOffset;
    private Quaternion initialCameraRotation;
    private float currentYawOffset;
    private float currentPitchOffset;

    public CameraPreset ActivePreset { get; private set; }
    public CustomizationCategoryData.CameraFocusZone CurrentZone { get; private set; }
    public bool HasActivePreset { get; private set; }
    public float CurrentTargetFOV { get; private set; }
    public float CurrentYawOffset => currentYawOffset;
    public float CurrentPitchOffset => currentPitchOffset;

    private void Awake()
    {
        if (resetViewButton != null)
        {
            resetViewButton.onClick.AddListener(ResetToFullbody);
            resetViewButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (HasActivePreset && CurrentZone != CustomizationCategoryData.CameraFocusZone.Fullbody &&
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ResetToFullbody();
        }
    }

    public void ResetToFullbody()
    {
        FocusOnZone(CustomizationCategoryData.CameraFocusZone.Fullbody, immediate: false);
    }

    public void FocusOnZone(CustomizationCategoryData.CameraFocusZone zone, bool immediate = false)
    {
        // Si on est déjà sur cette zone, inutile de réinitialiser la caméra
        if (HasActivePreset && CurrentZone == zone && !immediate)
            return;

        for (int i = 0; i < presets.Count; i++)
        {
            var preset = presets[i];
            if (preset.focusZones == null || !preset.focusZones.Contains(zone) || preset.virtualCamera == null)
                continue;

            zoomTween?.Kill();

            foreach (var p in presets)
            {
                if (p.virtualCamera != null && p.virtualCamera != preset.virtualCamera)
                    p.virtualCamera.Priority = inactivePriority;
            }

            preset.virtualCamera.Priority = activePriority;
            ActivePreset = preset;
            CurrentZone = zone;
            HasActivePreset = true;

            if (resetViewButton != null)
                resetViewButton.gameObject.SetActive(zone != CustomizationCategoryData.CameraFocusZone.Fullbody);

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