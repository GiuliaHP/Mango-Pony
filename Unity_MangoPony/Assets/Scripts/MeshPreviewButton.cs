using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MeshPreviewButton : MonoBehaviour
{
    [Header("Configuration du Mesh")]
    [Tooltip("L'emplacement à modifier (Crinière, Corne, Ailes, Sabots, etc.)")]
    [SerializeField] private Swapper.SlotType slotType;

    [Tooltip("Le Prefab 3D à instancier (Laisser vide pour une option 'Aucun / Vide')")]
    [SerializeField] private GameObject meshPrefab;

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
            Debug.LogWarning($"[MeshPreviewButton] Aucun Swapper trouvé dans la scène pour {gameObject.name} !");
            return;
        }

        customizerSwapper.SwapSlotMesh(slotType, meshPrefab);
    }

    public void Setup(Swapper.SlotType slot, CustomizationCategoryData.MeshShapeOption option)
    {
        slotType = slot;
        meshPrefab = option.meshPrefab;
    }
}