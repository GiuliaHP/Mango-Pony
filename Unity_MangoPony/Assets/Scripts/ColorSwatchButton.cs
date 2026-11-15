using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ColorSwatchButton : MonoBehaviour
{
    [SerializeField, HideInInspector] private Color defaultColor = Color.white;
    [SerializeField] private Image swatchPreviewImage;
    [SerializeField, HideInInspector] private Swapper.MaterialZone targetZone = Swapper.MaterialZone.BodyPattern;
    [SerializeField, HideInInspector] private string shaderColorProperty = "_BaseColor";

    private Color currentColor;
    private Button button;
    private CustomColorWheel colorWheel;
    private Swapper swapper;

    public Color CurrentColor => currentColor;

    private void Awake()
    {
        button = GetComponent<Button>();
        swapper = FindFirstObjectByType<Swapper>();
        colorWheel = FindNextColorWheelInHierarchy();

        if (swatchPreviewImage == null)
            swatchPreviewImage = GetComponent<Image>();

        currentColor = defaultColor;
        UpdateButtonVisual();
    }

    private void Start()
    {
        ApplyColorToShader();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnSwatchClicked);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnSwatchClicked);
    }

    public void OnSwatchClicked()
    {
        if (colorWheel == null)
            colorWheel = FindNextColorWheelInHierarchy();

        if (colorWheel != null)
        {
            colorWheel.SelectSwatchButton(this);
        }
        else
        {
            Debug.LogWarning($"[ColorSwatchButton] Aucun CustomColorWheel trouvé après '{gameObject.name}' dans la hiérarchie !");
        }
    }

    /// <summary>
    /// Parcourt la hiérarchie en descendant à partir de la position de ce bouton
    /// pour trouver le tout premier CustomColorWheel qui le suit.
    /// </summary>
    private CustomColorWheel FindNextColorWheelInHierarchy()
    {
        Transform current = transform;

        // On remonte d'étage en étage si nécessaire (au cas où les boutons sont dans un sous-groupe Layout)
        while (current.parent != null)
        {
            Transform parent = current.parent;
            int myIndex = current.GetSiblingIndex();

            // On inspecte tous les éléments situés APRÈS ce bouton dans le même parent
            for (int i = myIndex + 1; i < parent.childCount; i++)
            {
                Transform sibling = parent.GetChild(i);

                // Vérifie sur l'élément lui-même ou dans ses enfants (y compris inactifs)
                CustomColorWheel wheel = sibling.GetComponentInChildren<CustomColorWheel>(true);
                if (wheel != null)
                    return wheel;
            }

            current = parent;
        }

        // Fallback ultime si rien n'a été trouvé après
        return FindFirstObjectByType<CustomColorWheel>(FindObjectsInactive.Exclude);
    }

    /// <summary>
    /// Appelé en temps réel par le CustomColorWheel quand l'utilisateur bouge la roue ou le carré.
    /// </summary>
    public void ReceiveColorFromPicker(Color newColor)
    {
        currentColor = newColor;
        UpdateButtonVisual();
        ApplyColorToShader();
    }

    private void UpdateButtonVisual()
    {
        if (swatchPreviewImage != null)
        {
            swatchPreviewImage.color = currentColor;
        }
    }

    private void ApplyColorToShader()
    {
        if (swapper != null)
        {
            swapper.SwapZoneColor(targetZone, currentColor, shaderColorProperty);
        }
    }

    public void Setup(Swapper.MaterialZone zone, CustomizationCategoryData.ColorChannel channel)
    {
        targetZone = zone;
        shaderColorProperty = channel.shaderPropertyName;
        defaultColor = channel.defaultColor;
        currentColor = defaultColor;
        UpdateButtonVisual();
    }
}