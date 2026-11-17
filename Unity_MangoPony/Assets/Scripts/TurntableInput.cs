using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

public class TurntableInput : MonoBehaviour
{
    [Header("Targets")]
    [FormerlySerializedAs("ponyRoot")] [SerializeField] private Transform pony;
    [FormerlySerializedAs("cameraController")] [SerializeField] private CameraRig cameraRig;

    [Header("Mouse")]
    [Tooltip("0 = left, 1 = right, 2 = middle")]
    [SerializeField] private int mouseButton = 0;
    [FormerlySerializedAs("horizontalSpeed")] [SerializeField] private float yawSpeed = 0.35f;
    [FormerlySerializedAs("verticalSpeed")] [SerializeField] private float pitchSpeed = 0.2f;
    [SerializeField] private bool invertX = false;
    [SerializeField] private bool invertY = false;

    [Header("Auto Turn (fullbody only)")]
    [FormerlySerializedAs("enableAutoTurnAround")] [SerializeField] private bool autoTurn = true;
    [FormerlySerializedAs("idleDelayBeforeTurnAround")] [SerializeField] private float autoTurnDelay = 3f;
    [FormerlySerializedAs("turnAroundSpeed")] [SerializeField] private float autoTurnSpeed = 18f;

    [Header("Smoothing & Zoom")]
    [SerializeField] private float rotationDamping = 12f;
    [SerializeField] private float zoomStep = 4f;
    [SerializeField] private float zoomDamping = 10f;

    private float targetPonyYaw;
    private float ponyYaw;
    private float targetCamYaw;
    private float camYaw;
    private float targetCamPitch;
    private float camPitch;
    private bool isDragging;
    private float idleTime;

    private void Start()
    {
        if (pony != null)
        {
            ponyYaw = pony.localEulerAngles.y;
            targetPonyYaw = ponyYaw;
        }
    }

    private void OnEnable()
    {
        if (cameraRig != null)
            cameraRig.PresetChanged += OnPresetChanged;
    }

    private void OnDisable()
    {
        if (cameraRig != null)
            cameraRig.PresetChanged -= OnPresetChanged;
    }

    private void OnPresetChanged(CameraRig.Preset preset)
    {
        targetCamYaw = camYaw = 0f;
        targetCamPitch = camPitch = 0f;
        idleTime = 0f;

        if (!cameraRig.IsFullbody)
            targetPonyYaw = ponyYaw + Mathf.DeltaAngle(ponyYaw, preset.ponyYaw);
    }

    private void Update()
    {
        if (Mouse.current == null || cameraRig == null || !cameraRig.HasActivePreset)
            return;

        HandleDrag();
        HandleAutoTurn();
        HandleZoom();
        ApplySmoothing();
    }

    private void HandleDrag()
    {
        var button = GetMouseButton();
        if (button == null) return;

        if (button.wasPressedThisFrame && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            isDragging = true;
            idleTime = 0f;
        }

        if (button.wasReleasedThisFrame)
            isDragging = false;

        if (!isDragging || !button.isPressed)
            return;

        idleTime = 0f;
        Vector2 delta = Mouse.current.delta.ReadValue();
        float directionX = invertX ? 1f : -1f;
        float directionY = invertY ? 1f : -1f;

        var preset = cameraRig.ActivePreset;

        if (cameraRig.IsFullbody)
        {
            targetPonyYaw += delta.x * yawSpeed * directionX;
            targetCamYaw = 0f;
        }
        else
        {
            targetCamYaw = Mathf.Clamp(targetCamYaw - delta.x * yawSpeed * directionX, preset.minYaw, preset.maxYaw);
        }

        targetCamPitch = Mathf.Clamp(targetCamPitch + delta.y * pitchSpeed * directionY, preset.minPitch, preset.maxPitch);
    }

    private void HandleAutoTurn()
    {
        if (!autoTurn || isDragging || pony == null) return;
        if (!cameraRig.IsFullbody) return;

        idleTime += Time.unscaledDeltaTime;
        if (idleTime >= autoTurnDelay)
            targetPonyYaw += autoTurnSpeed * Time.unscaledDeltaTime;
    }

    private void HandleZoom()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        float scroll = Mouse.current.scroll.y.ReadValue();
        if (Mathf.Abs(scroll) <= 0.01f) return;

        var preset = cameraRig.ActivePreset;
        float minFov = preset.minFov > 0f ? preset.minFov : 15f;
        float maxFov = preset.maxFov > 0f ? preset.maxFov : 60f;

        float step = Mathf.Sign(scroll) * Mathf.Clamp(Mathf.Abs(scroll) / 120f, 0.25f, 3f);
        float fov = Mathf.Clamp(cameraRig.TargetFov - step * zoomStep, minFov, maxFov);
        cameraRig.SetFov(fov, zoomDamping);
    }

    private void ApplySmoothing()
    {
        float t = Time.unscaledDeltaTime * rotationDamping;

        if (pony != null)
        {
            ponyYaw = Mathf.Lerp(ponyYaw, targetPonyYaw, t);
            Vector3 euler = pony.localEulerAngles;
            euler.y = ponyYaw;
            pony.localEulerAngles = euler;
        }

        if (Mathf.Abs(camYaw - targetCamYaw) > 0.001f || Mathf.Abs(camPitch - targetCamPitch) > 0.001f)
        {
            camYaw = Mathf.Lerp(camYaw, targetCamYaw, t);
            camPitch = Mathf.Lerp(camPitch, targetCamPitch, t);
            cameraRig.SetOrbit(camYaw, camPitch);
        }
    }

    private ButtonControl GetMouseButton()
    {
        return mouseButton switch
        {
            1 => Mouse.current.rightButton,
            2 => Mouse.current.middleButton,
            _ => Mouse.current.leftButton
        };
    }
}
