using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PonyTurntableController : MonoBehaviour
{
    [Header("Cibles")]
    [SerializeField] private Transform ponyRoot;
    [SerializeField] private CameraController cameraController;

    [Header("Sensibilité Souris")]
    [SerializeField] private int mouseButton = 0; // 0 = Clic gauche, 1 = Clic droit
    [SerializeField] private float horizontalSpeed = 0.35f;
    [SerializeField] private float verticalSpeed = 0.2f;
    [SerializeField] private bool invertX = false;
    [SerializeField] private bool invertY = false;

    [Header("Turn Around Automatique (Mode Fullbody uniquement)")]
    [SerializeField] private bool enableAutoTurnAround = true;
    [SerializeField] private float idleDelayBeforeTurnAround = 3f;
    [SerializeField] private float turnAroundSpeed = 18f;

    [Header("Fluidité & Zoom")]
    [SerializeField] private float rotationDamping = 12f;
    [SerializeField] private float zoomStep = 4f;
    [SerializeField] private float zoomDamping = 10f;

    private float targetPonyYaw;
    private float currentPonyYaw;

    private float targetCamYaw;
    private float currentCamYaw;
    private float targetCamPitch;
    private float currentCamPitch;

    private bool isDragging;
    private float idleTimer;

    private void Start()
    {
        if (ponyRoot != null)
        {
            currentPonyYaw = ponyRoot.localEulerAngles.y;
            targetPonyYaw = currentPonyYaw;
        }
    }

    private void Update()
    {
        if (Mouse.current == null || cameraController == null || !cameraController.HasActivePreset)
            return;

        // Resynchronise les angles caméra si on vient de changer d'onglet
        if (!isDragging && Mathf.Abs(cameraController.CurrentYawOffset) < 0.001f && Mathf.Abs(cameraController.CurrentPitchOffset) < 0.001f)
        {
            if (Mathf.Abs(targetCamYaw) > 0.01f || Mathf.Abs(targetCamPitch) > 0.01f)
            {
                targetCamYaw = currentCamYaw = 0f;
                targetCamPitch = currentCamPitch = 0f;
            }
        }

        HandleDragInput();
        HandleAutoTurnAround();
        HandleZoomInput();
        ApplySmoothTransforms();
    }

    private void HandleDragInput()
    {
        var button = GetSelectedMouseButton();
        if (button == null) return;

        if (button.wasPressedThisFrame)
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = true;
                idleTimer = 0f;
            }
        }

        if (button.wasReleasedThisFrame)
            isDragging = false;

        if (!isDragging || !button.isPressed)
            return;

        idleTimer = 0f;
        Vector2 delta = Mouse.current.delta.ReadValue();
        float dirX = invertX ? 1f : -1f;
        float dirY = invertY ? 1f : -1f;

        var preset = cameraController.ActivePreset;

        if (preset.controlMode == CameraController.CameraControlMode.FullbodyTurntable)
        {
            // 1. Grand Angle : le personnage tourne en X sans limite, la caméra orbite en Y avec limites
            targetPonyYaw += delta.x * horizontalSpeed * dirX;
            targetCamYaw = 0f;
            targetCamPitch = Mathf.Clamp(targetCamPitch + delta.y * verticalSpeed * dirY, preset.minPitch, preset.maxPitch);
        }
        else
        {
            // 2. Détail : le personnage ne tourne pas, seule la caméra orbite en X et Y avec limites
            targetCamYaw = Mathf.Clamp(targetCamYaw - delta.x * horizontalSpeed * dirX, preset.minYaw, preset.maxYaw);
            targetCamPitch = Mathf.Clamp(targetCamPitch + delta.y * verticalSpeed * dirY, preset.minPitch, preset.maxPitch);
        }
    }

    private void HandleAutoTurnAround()
    {
        if (!enableAutoTurnAround || isDragging || ponyRoot == null) return;

        // Le turn-around ne s'active que sur les caméras Grand Angle
        if (cameraController.ActivePreset.controlMode != CameraController.CameraControlMode.FullbodyTurntable)
            return;

        idleTimer += Time.unscaledDeltaTime;
        if (idleTimer >= idleDelayBeforeTurnAround)
        {
            targetPonyYaw += turnAroundSpeed * Time.unscaledDeltaTime;
        }
    }

    private void HandleZoomInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        float scrollY = Mouse.current.scroll.y.ReadValue();
        if (Mathf.Abs(scrollY) > 0.01f)
        {
            var preset = cameraController.ActivePreset;
            float minFov = preset.minFOV > 0f ? preset.minFOV : 15f;
            float maxFov = preset.maxFOV > 0f ? preset.maxFOV : 60f;

            float normalizedScroll = Mathf.Sign(scrollY) * Mathf.Clamp(Mathf.Abs(scrollY) / 120f, 0.25f, 3f);
            float nextFov = Mathf.Clamp(cameraController.CurrentTargetFOV - normalizedScroll * zoomStep, minFov, maxFov);
            cameraController.SetZoomFOV(nextFov, zoomDamping);
        }
    }

    private void ApplySmoothTransforms()
    {
        float t = Time.unscaledDeltaTime * rotationDamping;

        if (ponyRoot != null)
        {
            currentPonyYaw = Mathf.Lerp(currentPonyYaw, targetPonyYaw, t);
            Vector3 euler = ponyRoot.localEulerAngles;
            euler.y = currentPonyYaw;
            ponyRoot.localEulerAngles = euler;
        }

        if (Mathf.Abs(currentCamYaw - targetCamYaw) > 0.001f || Mathf.Abs(currentCamPitch - targetCamPitch) > 0.001f)
        {
            currentCamYaw = Mathf.Lerp(currentCamYaw, targetCamYaw, t);
            currentCamPitch = Mathf.Lerp(currentCamPitch, targetCamPitch, t);
            cameraController.SetCameraOrbitAngles(currentCamYaw, currentCamPitch);
        }
    }

    private UnityEngine.InputSystem.Controls.ButtonControl GetSelectedMouseButton()
    {
        return mouseButton switch
        {
            0 => Mouse.current.leftButton,
            1 => Mouse.current.rightButton,
            2 => Mouse.current.middleButton,
            _ => Mouse.current.leftButton
        };
    }
}