using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CustomColorWheel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("1. Références UI (Visuels)")]
    [Tooltip("Le RectTransform de l'image de l'anneau chromatique")]
    [SerializeField] private RectTransform wheelRect;
    [Tooltip("Le curseur (Handle) qui tourne autour de l'anneau")]
    [SerializeField] private RectTransform hueHandle;

    [Space(5)]
    [Tooltip("Le RectTransform du carré Saturation/Luminosité")]
    [SerializeField] private RectTransform svSquareRect;
    [Tooltip("L'Image du carré SV dont la couleur changera selon la Teinte (Hue)")]
    [SerializeField] private Image svSquareHueBackground;
    [Tooltip("Le curseur (Pointer) qui se déplace dans le carré")]
    [SerializeField] private RectTransform svHandle;

    [Header("2. Réglages Géométriques de l'Anneau")]
    [Tooltip("Rayon intérieur normalisé de l'anneau (0 à 1, ex: 0.78 selon ton image)")]
    [Range(0f, 1f)] [SerializeField] private float innerRadiusNormalized = 0.78f;
    [Tooltip("Rayon où se positionne le curseur circulaire sur l'anneau (milieu de la bande)")]
    [Range(0f, 1f)] [SerializeField] private float handleOrbitNormalized = 0.89f;
    [Tooltip("Décalage d'angle si le rouge pur n'est pas à droite (90° = Rouge en haut comme sur ton image)")]
    [SerializeField] private float hueAngleOffset = 90f;

    [Header("3. Événement Dynamique (Optionnel)")]
    public UnityEvent<Color> onColorChanged;

    [Header("4. Auto-sélection au démarrage")]
    [Tooltip("Optionnel : Glisse ici le conteneur parent des boutons pour limiter la recherche (si vide, cherche dans toute la scène)")]
    [SerializeField] private Transform swatchListContainer;

    // État interne HSV
    private float currentHue = 0f;
    private float currentSaturation = 1f;
    private float currentValue = 1f;

    private enum DragMode { None, Wheel, Square }
    private DragMode currentDragMode = DragMode.None;

    private Canvas parentCanvas;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        SelectFirstActiveSwatch();
        UpdateVisuals();
    }

    private CustomizationButton activeSwatchButton;

    public void SelectFirstActiveSwatch()
    {
        Transform searchRoot = swatchListContainer != null ? swatchListContainer : transform.parent;
        if (searchRoot != null)
        {
            foreach (var btn in searchRoot.GetComponentsInChildren<CustomizationButton>(false))
            {
                if (btn.Type == CustomizationButton.OptionType.ColorSwatch)
                {
                    SelectSwatchButton(btn);
                    return;
                }
            }
        }
    }

    public void SelectSwatchButton(CustomizationButton swatchButton)
    {
        if (swatchButton == null || swatchButton.Type != CustomizationButton.OptionType.ColorSwatch)
            return;

        activeSwatchButton = swatchButton;
        Color.RGBToHSV(activeSwatchButton.CurrentColor, out float h, out float s, out float v);
        if (s > 0.001f) currentHue = h;
        currentSaturation = s;
        currentValue = v;

        UpdateVisuals();
    }

    #region Gestion des Clics & Glissements

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (RectTransformUtility.RectangleContainsScreenPoint(svSquareRect, eventData.position, GetEventCamera(eventData)))
        {
            currentDragMode = DragMode.Square;
            UpdateSquareFromPointer(eventData);
            return;
        }

        if (IsPointerInWheelRing(eventData))
        {
            currentDragMode = DragMode.Wheel;
            UpdateWheelFromPointer(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentDragMode == DragMode.Wheel)
        {
            UpdateWheelFromPointer(eventData);
        }
        else if (currentDragMode == DragMode.Square)
        {
            UpdateSquareFromPointer(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        currentDragMode = DragMode.None;
    }

    #endregion

    #region Calculs Mathématiques (Roue & Carré)

    private bool IsPointerInWheelRing(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRect, eventData.position, GetEventCamera(eventData), out Vector2 localPos))
            return false;

        float maxRadius = wheelRect.rect.width * 0.5f;
        float distNormalized = localPos.magnitude / maxRadius;

        return distNormalized >= innerRadiusNormalized && distNormalized <= 1.05f;
    }

    private void UpdateWheelFromPointer(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRect, eventData.position, GetEventCamera(eventData), out Vector2 localPos))
            return;

        float angle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        float adjustedAngle = (hueAngleOffset - angle) % 360f;
        if (adjustedAngle < 0f) adjustedAngle += 360f;

        currentHue = adjustedAngle / 360f;

        UpdateVisuals();
        EmitColor();
    }

    private void UpdateSquareFromPointer(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(svSquareRect, eventData.position, GetEventCamera(eventData), out Vector2 localPos))
            return;

        Rect rect = svSquareRect.rect;
        float normX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPos.x);
        float normY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPos.y);

        currentSaturation = Mathf.Clamp01(normX);
        currentValue = Mathf.Clamp01(normY);

        UpdateVisuals();
        EmitColor();
    }

    #endregion

    #region Mise à jour Visuelle & Diffusion

    private void UpdateVisuals()
    {
        if (hueHandle != null && wheelRect != null)
        {
            float angleDeg = hueAngleOffset - (currentHue * 360f);
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float orbitRadius = (wheelRect.rect.width * 0.5f) * handleOrbitNormalized;

            Vector2 handlePos = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * orbitRadius;
            hueHandle.anchoredPosition = handlePos;
        }

        if (svSquareHueBackground != null)
        {
            svSquareHueBackground.color = Color.HSVToRGB(currentHue, 1f, 1f);
        }

        if (svHandle != null && svSquareRect != null)
        {
            Rect rect = svSquareRect.rect;
            float posX = Mathf.Lerp(rect.xMin, rect.xMax, currentSaturation);
            float posY = Mathf.Lerp(rect.yMin, rect.yMax, currentValue);
            svHandle.anchoredPosition = new Vector2(posX, posY);
        }
    }

    private void EmitColor()
    {
        Color finalColor = Color.HSVToRGB(currentHue, currentSaturation, currentValue);
        onColorChanged?.Invoke(finalColor);

        // Transmet la couleur uniquement au bouton actuellement sélectionné
        if (activeSwatchButton != null)
        {
            activeSwatchButton.ReceiveColorFromPicker(finalColor);
        }
    }

    private Camera GetEventCamera(PointerEventData eventData)
    {
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;
        return eventData.pressEventCamera;
    }

    #endregion
}