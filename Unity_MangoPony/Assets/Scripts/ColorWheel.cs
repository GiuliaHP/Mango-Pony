using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ColorWheel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Hue Ring")]
    [SerializeField] private RectTransform wheelRect;
    [SerializeField] private RectTransform hueHandle;

    [Header("Saturation / Value Square")]
    [FormerlySerializedAs("svSquareRect")] [SerializeField] private RectTransform svRect;
    [Tooltip("Image tinted with the current hue")]
    [FormerlySerializedAs("svSquareHueBackground")] [SerializeField] private Image svBackground;
    [SerializeField] private RectTransform svHandle;

    [Header("Ring Geometry")]
    [Tooltip("Normalized inner radius of the ring (0-1)")]
    [FormerlySerializedAs("innerRadiusNormalized")] [Range(0f, 1f)] [SerializeField] private float innerRadius = 0.78f;
    [Tooltip("Normalized radius where the hue handle sits (0-1)")]
    [FormerlySerializedAs("handleOrbitNormalized")] [Range(0f, 1f)] [SerializeField] private float handleRadius = 0.89f;
    [Tooltip("Angle of pure red (90 = top)")]
    [FormerlySerializedAs("hueAngleOffset")] [SerializeField] private float hueOffset = 90f;

    [Header("Events")]
    public UnityEvent<Color> onColorChanged;

    [Header("Swatches")]
    [Tooltip("Parent of the swatches to auto-select (empty = this object's parent)")]
    [FormerlySerializedAs("swatchListContainer")] [SerializeField] private Transform swatchContainer;

    private enum DragMode { None, Wheel, Square }

    private float hue = 0f;
    private float saturation = 1f;
    private float brightness = 1f;
    private DragMode dragMode = DragMode.None;
    private Canvas canvas;
    private OptionButton activeSwatch;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        SelectFirstSwatch();
        RefreshVisuals();
    }

    public void Bind(Transform container)
    {
        swatchContainer = container;
    }

    public void SelectFirstSwatch()
    {
        Transform root = swatchContainer != null ? swatchContainer : transform.parent;
        if (root == null) return;

        foreach (var option in root.GetComponentsInChildren<OptionButton>(false))
        {
            if (option.Kind == OptionButton.OptionKind.Swatch)
            {
                SelectSwatch(option);
                return;
            }
        }
    }

    public void SelectSwatch(OptionButton swatch)
    {
        if (swatch == null || swatch.Kind != OptionButton.OptionKind.Swatch)
            return;

        activeSwatch = swatch;
        Color.RGBToHSV(activeSwatch.CurrentColor, out float h, out float s, out float v);
        if (s > 0.001f) hue = h;
        saturation = s;
        brightness = v;

        RefreshVisuals();
    }

    #region Pointer

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (RectTransformUtility.RectangleContainsScreenPoint(svRect, eventData.position, GetEventCamera(eventData)))
        {
            dragMode = DragMode.Square;
            UpdateSquare(eventData);
            return;
        }

        if (IsInRing(eventData))
        {
            dragMode = DragMode.Wheel;
            UpdateHue(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragMode == DragMode.Wheel)
            UpdateHue(eventData);
        else if (dragMode == DragMode.Square)
            UpdateSquare(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragMode = DragMode.None;
    }

    #endregion

    #region Picking

    private bool IsInRing(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRect, eventData.position, GetEventCamera(eventData), out Vector2 localPos))
            return false;

        float distance = localPos.magnitude / (wheelRect.rect.width * 0.5f);
        return distance >= innerRadius && distance <= 1.05f;
    }

    private void UpdateHue(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRect, eventData.position, GetEventCamera(eventData), out Vector2 localPos))
            return;

        float angle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        float adjusted = (hueOffset - angle) % 360f;
        if (adjusted < 0f) adjusted += 360f;

        hue = adjusted / 360f;

        RefreshVisuals();
        EmitColor();
    }

    private void UpdateSquare(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(svRect, eventData.position, GetEventCamera(eventData), out Vector2 localPos))
            return;

        Rect rect = svRect.rect;
        saturation = Mathf.Clamp01(Mathf.InverseLerp(rect.xMin, rect.xMax, localPos.x));
        brightness = Mathf.Clamp01(Mathf.InverseLerp(rect.yMin, rect.yMax, localPos.y));

        RefreshVisuals();
        EmitColor();
    }

    #endregion

    #region Output

    private void RefreshVisuals()
    {
        if (hueHandle != null && wheelRect != null)
        {
            float angle = (hueOffset - hue * 360f) * Mathf.Deg2Rad;
            float radius = wheelRect.rect.width * 0.5f * handleRadius;
            hueHandle.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        if (svBackground != null)
            svBackground.color = Color.HSVToRGB(hue, 1f, 1f);

        if (svHandle != null && svRect != null)
        {
            Rect rect = svRect.rect;
            svHandle.anchoredPosition = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, saturation), Mathf.Lerp(rect.yMin, rect.yMax, brightness));
        }
    }

    private void EmitColor()
    {
        Color color = Color.HSVToRGB(hue, saturation, brightness);
        onColorChanged?.Invoke(color);

        if (activeSwatch != null)
            activeSwatch.ApplyColor(color);
    }

    private Camera GetEventCamera(PointerEventData eventData)
    {
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;
        return eventData.pressEventCamera;
    }

    #endregion
}
