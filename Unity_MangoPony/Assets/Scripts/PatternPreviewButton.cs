using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PatternPreviewButton : MonoBehaviour
{
    [SerializeField, HideInInspector] private Swapper.MaterialZone targetZone;
    [SerializeField, HideInInspector] private Texture patternTexture;
    [SerializeField, HideInInspector] private bool applyToAllLegs = false;

    private Button button;
    private Swapper customizerSwapper;

    private void Awake()
    {
        button = GetComponent<Button>();
        customizerSwapper = FindFirstObjectByType<Swapper>();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnPreviewButtonClicked);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnPreviewButtonClicked);
    }

    public void OnPreviewButtonClicked()
    {
        if (customizerSwapper == null)
        {
            Debug.LogWarning($"[PatternPreviewButton] Aucun Swapper trouvé dans la scène pour {gameObject.name} !");
            return;
        }

        if (applyToAllLegs)
        {
            customizerSwapper.SwapAllLegsPattern(patternTexture);
        }
        else
        {
            customizerSwapper.SwapPatternTexture(targetZone, patternTexture);
        }
    }

    public void Setup(Swapper.MaterialZone zone, CustomizationCategoryData.PatternOption option, bool syncFourLegs)
    {
        targetZone = zone;
        patternTexture = option.patternTexture;
        applyToAllLegs = syncFourLegs;
    }
}