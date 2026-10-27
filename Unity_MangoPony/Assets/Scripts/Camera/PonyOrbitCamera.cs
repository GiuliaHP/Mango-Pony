using UnityEngine;
using UnityEngine.EventSystems;
using MangoPony.Core;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MangoPony.CameraControl
{
    /// <summary>
    /// Caméra orbitale 3D fluide pour l'inspection du poney à 360°.
    /// Compatible nativement avec Unity 6 et le New Input System (UnityEngine.InputSystem).
    /// Supporte le zoom, l'inertie, le framing de points de focus (Tête, Corps, Sabots)
    /// et le mode turntable (rotation automatique).
    /// </summary>
    public class PonyOrbitCamera : MonoBehaviour
    {
        [Header("Target & Focus")]
        [Tooltip("Transform cible au centre de la rotation")]
        [SerializeField] private Transform target;
        [Tooltip("Décalage vertical du point visé")]
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0.9f, 0f);

        [Header("Orbit Angles & Distances")]
        [SerializeField] private float distance = 2.8f;
        [SerializeField] private float minDistance = 1.0f;
        [SerializeField] private float maxDistance = 5.0f;
        [SerializeField] private float minPitchAngle = -15.0f;
        [SerializeField] private float maxPitchAngle = 75.0f;

        [Header("Sensitivity & Smoothing")]
        [SerializeField] private float orbitSensitivity = 0.25f;
        [SerializeField] private float zoomSensitivity = 0.05f;
        [SerializeField] private float smoothSpeed = 10.0f;

        [Header("Turntable (Auto-Rotate)")]
        [SerializeField] private bool autoRotate = false;
        [SerializeField] private float autoRotateSpeed = 15.0f;

        // Angles cibles et courants (pour interpolation fluide)
        private float targetYaw = 45.0f;
        private float targetPitch = 15.0f;
        private float targetDistance;
        private Vector3 currentFocusOffset;
        private Vector3 targetFocusOffset;

        private float currentYaw;
        private float currentPitch;
        private float currentDistance;

        public bool AutoRotate
        {
            get => autoRotate;
            set => autoRotate = value;
        }

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Start()
        {
            targetDistance = distance;
            currentDistance = distance;
            currentYaw = targetYaw;
            currentPitch = targetPitch;
            targetFocusOffset = targetOffset;
            currentFocusOffset = targetOffset;

            // Initialiser la position immédiatement
            UpdateCameraTransform(1.0f);
        }

        private void LateUpdate()
        {
            HandleInput();
            UpdateCameraTransform(Time.unscaledDeltaTime * smoothSpeed);
        }

        private void HandleInput()
        {
            bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            // Gestion New Input System
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                // Rotation orbitale par glisser-déposer de la souris (clic gauche ou droit)
                bool isDragging = (mouse.leftButton.isPressed || mouse.rightButton.isPressed) && !isOverUI;

                if (isDragging)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    targetYaw += delta.x * orbitSensitivity;
                    targetPitch -= delta.y * orbitSensitivity;
                    targetPitch = Mathf.Clamp(targetPitch, minPitchAngle, maxPitchAngle);
                }
                else if (autoRotate)
                {
                    targetYaw += autoRotateSpeed * Time.unscaledDeltaTime;
                }

                // Zoom molette
                if (!isOverUI)
                {
                    float scroll = mouse.scroll.ReadValue().y;
                    if (Mathf.Abs(scroll) > 0.01f)
                    {
                        targetDistance -= (scroll / 120.0f) * zoomSensitivity * (targetDistance * 0.5f);
                        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
                    }
                }
            }
#else
            // Fallback Input Manager classique si activé
            if (!isOverUI)
            {
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    targetYaw += Input.GetAxis("Mouse X") * orbitSensitivity * 10f;
                    targetPitch -= Input.GetAxis("Mouse Y") * orbitSensitivity * 10f;
                    targetPitch = Mathf.Clamp(targetPitch, minPitchAngle, maxPitchAngle);
                }
                else if (autoRotate)
                {
                    targetYaw += autoRotateSpeed * Time.unscaledDeltaTime;
                }

                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.001f)
                {
                    targetDistance -= scroll * zoomSensitivity * 10f;
                    targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
                }
            }
#endif
        }

        private void UpdateCameraTransform(float t)
        {
            currentYaw = Mathf.Lerp(currentYaw, targetYaw, t);
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, t);
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, t);
            currentFocusOffset = Vector3.Lerp(currentFocusOffset, targetFocusOffset, t);

            Vector3 pivot = target != null ? target.position + currentFocusOffset : currentFocusOffset;

            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 position = pivot - (rotation * Vector3.forward * currentDistance);

            transform.rotation = rotation;
            transform.position = position;
        }

        /// <summary>
        /// Recadre la caméra vers une zone spécifique du poney (Tête, Corps, Sabots).
        /// </summary>
        public void FocusOn(CameraFocusTarget focusTarget)
        {
            switch (focusTarget)
            {
                case CameraFocusTarget.WholeBody:
                    targetFocusOffset = new Vector3(0f, 0.9f, 0f);
                    targetDistance = 2.8f;
                    targetPitch = 15f;
                    break;

                case CameraFocusTarget.Head:
                    targetFocusOffset = new Vector3(0f, 1.25f, 0.6f);
                    targetDistance = 1.35f;
                    targetPitch = 10f;
                    break;

                case CameraFocusTarget.Hooves:
                    targetFocusOffset = new Vector3(0f, 0.35f, 0.1f);
                    targetDistance = 1.6f;
                    targetPitch = 10f;
                    break;

                case CameraFocusTarget.Wings:
                    targetFocusOffset = new Vector3(0f, 1.05f, -0.2f);
                    targetDistance = 2.2f;
                    targetPitch = 25f;
                    break;
            }
        }
    }
}

