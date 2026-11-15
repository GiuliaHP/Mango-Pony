using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MeshPreviewButton : MonoBehaviour
{
    [SerializeField, HideInInspector] private Swapper.SlotType slotType;
    [SerializeField, HideInInspector] private GameObject meshPrefab;

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