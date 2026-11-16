using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public struct CameraPreset
    {
        public CustomizationCategoryData.CameraFocusZone focusZone;
        [Tooltip("Place un Empty ou une caméra désactivée dans la scène à la position/rotation voulue")]
        public Transform targetView;
        [Tooltip("Optionnel : FOV cible (laisser <= 0 pour garder le FOV actuel)")]
        public float targetFOV;
    }

    [Header("Références")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<CameraPreset> presets = new List<CameraPreset>();

    [Header("Transition DOTween")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Ease transitionEase = Ease.InOutCubic;

    private Sequence cameraSequence;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void FocusOnZone(CustomizationCategoryData.CameraFocusZone zone, bool immediate = false)
    {
        if (mainCamera == null) return;

        foreach (var preset in presets)
        {
            if (preset.focusZone != zone || preset.targetView == null) continue;

            if (cameraSequence != null && cameraSequence.IsActive())
                cameraSequence.Kill();

            Transform camTransform = mainCamera.transform;

            if (immediate)
            {
                camTransform.position = preset.targetView.position;
                camTransform.rotation = preset.targetView.rotation;
                if (preset.targetFOV > 0f)
                    mainCamera.fieldOfView = preset.targetFOV;
                return;
            }

            cameraSequence = DOTween.Sequence().SetUpdate(true);
            cameraSequence.Join(camTransform.DOMove(preset.targetView.position, transitionDuration).SetEase(transitionEase));
            cameraSequence.Join(camTransform.DORotateQuaternion(preset.targetView.rotation, transitionDuration).SetEase(transitionEase));

            if (preset.targetFOV > 0f)
            {
                cameraSequence.Join(mainCamera.DOFieldOfView(preset.targetFOV, transitionDuration).SetEase(transitionEase));
            }
            return;
        }
    }

    private void OnDestroy()
    {
        cameraSequence?.Kill();
    }
}