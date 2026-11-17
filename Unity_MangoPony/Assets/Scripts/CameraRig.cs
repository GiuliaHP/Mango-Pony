using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraRig : MonoBehaviour
{
    [System.Serializable]
    public struct Preset
    {
        public string id;
        public CinemachineCamera camera;
        public Transform pivot;

        [Header("Pitch Limits")]
        public float minPitch;
        public float maxPitch;

        [Header("Yaw Limits (detail views only)")]
        public float minYaw;
        public float maxYaw;

        [Header("Field of View")]
        public float defaultFov;
        public float minFov;
        public float maxFov;

        [Header("Pony Turn (detail views only)")]
        public float ponyYaw;
    }

    private struct NeutralPose
    {
        public Vector3 offset;
        public Quaternion rotation;
    }

    [Header("Cinemachine Priorities")]
    [SerializeField] private int inactivePriority = 10;
    [SerializeField] private int activePriority = 20;

    [Header("Reset View")]
    [Tooltip("Shown only in detail views, returns to the fullbody view")]
    [FormerlySerializedAs("resetViewButton")] [SerializeField] private Button resetButton;

    [Header("Cameras")]
    [SerializeField] private Preset fullbody;
    [SerializeField] private List<Preset> details = new List<Preset>();

    private Tween zoomTween;
    private NeutralPose fullbodyPose;
    private NeutralPose[] detailPoses;
    private NeutralPose activePose;
    private float yaw;
    private float pitch;

    public event System.Action<Preset> PresetChanged;

    public Preset ActivePreset { get; private set; }
    public string ActiveId { get; private set; }
    public bool IsFullbody { get; private set; } = true;
    public bool HasActivePreset { get; private set; }
    public float TargetFov { get; private set; }
    public float Yaw => yaw;
    public float Pitch => pitch;

    public IEnumerable<string> DetailIds
    {
        get
        {
            foreach (var detail in details)
                yield return detail.id;
        }
    }

    private void Awake()
    {
        fullbodyPose = Capture(fullbody);
        detailPoses = new NeutralPose[details.Count];
        for (int i = 0; i < details.Count; i++)
            detailPoses[i] = Capture(details[i]);

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetView);
            resetButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (HasActivePreset && !IsFullbody &&
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ResetView();
        }
    }

    private static NeutralPose Capture(Preset preset)
    {
        if (preset.camera == null)
            return new NeutralPose { offset = Vector3.zero, rotation = Quaternion.identity };

        Vector3 pivotPosition = preset.pivot != null ? preset.pivot.position : Vector3.zero;
        Transform cameraTransform = preset.camera.transform;
        return new NeutralPose
        {
            offset = cameraTransform.position - pivotPosition,
            rotation = cameraTransform.rotation
        };
    }

    public void ResetView()
    {
        Focus(string.Empty);
    }

    public void Focus(string id, bool immediate = false)
    {
        id = id ?? string.Empty;

        Preset preset = fullbody;
        NeutralPose pose = fullbodyPose;
        bool isFullbody = true;

        if (id.Length > 0)
        {
            int index = details.FindIndex(detail => detail.id == id);
            if (index >= 0 && details[index].camera != null)
            {
                preset = details[index];
                pose = detailPoses[index];
                isFullbody = false;
            }
            else
            {
                Debug.LogWarning($"CameraRig: unknown camera id '{id}', using fullbody", this);
                id = string.Empty;
            }
        }

        if (HasActivePreset && ActiveId == id && !immediate)
            return;

        if (preset.camera == null)
            return;

        zoomTween?.Kill();

        if (fullbody.camera != null && fullbody.camera != preset.camera)
            fullbody.camera.Priority = inactivePriority;
        foreach (var detail in details)
        {
            if (detail.camera != null && detail.camera != preset.camera)
                detail.camera.Priority = inactivePriority;
        }

        preset.camera.Priority = activePriority;
        ActivePreset = preset;
        ActiveId = id;
        IsFullbody = isFullbody;
        HasActivePreset = true;
        activePose = pose;

        if (resetButton != null)
            resetButton.gameObject.SetActive(!isFullbody);

        SetOrbit(0f, 0f);

        float fov = preset.defaultFov > 0f ? preset.defaultFov : preset.camera.Lens.FieldOfView;
        TargetFov = fov;

        if (immediate)
        {
            var lens = preset.camera.Lens;
            lens.FieldOfView = fov;
            preset.camera.Lens = lens;
        }
        else
        {
            SetFov(fov, 4f);
        }

        PresetChanged?.Invoke(preset);
    }

    public void SetOrbit(float newYaw, float newPitch)
    {
        if (!HasActivePreset || ActivePreset.camera == null) return;

        yaw = newYaw;
        pitch = newPitch;

        Vector3 pivotPosition = ActivePreset.pivot != null ? ActivePreset.pivot.position : Vector3.zero;

        Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);
        Vector3 rightAxis = (yawRotation * activePose.rotation) * Vector3.right;
        Quaternion pitchRotation = Quaternion.AngleAxis(pitch, rightAxis);
        Quaternion rotation = pitchRotation * yawRotation;

        Transform cameraTransform = ActivePreset.camera.transform;
        cameraTransform.position = pivotPosition + rotation * activePose.offset;
        cameraTransform.rotation = rotation * activePose.rotation;
    }

    public void SetFov(float fov, float damping = 10f)
    {
        if (!HasActivePreset || ActivePreset.camera == null) return;

        TargetFov = fov;
        zoomTween?.Kill();

        float startFov = ActivePreset.camera.Lens.FieldOfView;
        float duration = Mathf.Clamp(1f / Mathf.Max(damping, 0.1f), 0.05f, 0.45f);

        zoomTween = DOVirtual.Float(startFov, fov, duration, value =>
        {
            if (ActivePreset.camera != null)
            {
                var lens = ActivePreset.camera.Lens;
                lens.FieldOfView = value;
                ActivePreset.camera.Lens = lens;
            }
        }).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    private void OnDestroy()
    {
        zoomTween?.Kill();
    }
}
