#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;

// Handles drag-and-drop reordering of items in the inventory UI.
// Attached at runtime by InventoryUI to each inventory slot.
public class InventoryDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas? _canvas = null;
    private RectTransform _rectTransform = null!;
    private CanvasGroup _canvasGroup = null!;

    private Vector3 _startPosition;
    private Transform _originalParent = null!;
    private int _originalSiblingIndex;

    public int SlotIndex { get; set; }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = _rectTransform.position;
        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();

        _canvasGroup.alpha = 0.6f;
        _canvasGroup.blocksRaycasts = false;

        // move to top of canvas so it renders above other items
        transform.SetParent(_canvas!.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.alpha = 1.0f;
        _canvasGroup.blocksRaycasts = true;

        // return to original parent
        transform.SetParent(_originalParent);
        transform.SetSiblingIndex(_originalSiblingIndex);
        _rectTransform.position = _startPosition;

        // check if we dropped onto another inventory slot
        if (eventData.pointerEnter != null)
        {
            var targetHelper = eventData.pointerEnter.GetComponentInParent<InventoryItemHelper>();
            if (targetHelper != null)
            {
                var targetDragHandler = targetHelper.GetComponent<InventoryDragHandler>();
                if (targetDragHandler != null && targetDragHandler.SlotIndex != SlotIndex)
                {
                    InventoryUI.Instance.OnSlotSwapRequested(SlotIndex, targetDragHandler.SlotIndex);
                }
            }
        }
    }
}
