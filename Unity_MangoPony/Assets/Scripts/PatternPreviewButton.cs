using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PatternPreviewButton : MonoBehaviour
{
    [Header("Configuration du Motif")]
    [Tooltip("La zone du poney à modifier (Corps, Front, Patte, Crinière...)")]
    [SerializeField] private Swapper.MaterialZone targetZone;

    [Tooltip("La Texture / Masque à envoyer dans le Shader (Laisser vide pour 'Aucun motif')")]
    [SerializeField] private Texture patternTexture;

    [Tooltip("Si coché, applique le motif aux 4 pattes simultanément")]
    [SerializeField] private bool applyToAllLegs = false;

    // Références récupérées automatiquement en interne
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
}