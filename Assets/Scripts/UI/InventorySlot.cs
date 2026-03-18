using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    public Image frame;
    public Image itemIcon;

    public Sprite emptyFrame;
    public Sprite filledFrame;

    private Item currentItem;
    private Coroutine animCoroutine;

    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.cyan;

    private bool isSelected = false;

    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private InventoryUI inventoryUI;

    public int slotIndex;

    void Awake()
    {
        frame.sprite = emptyFrame;
        itemIcon.enabled = false;

        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // ✅ lấy InventoryUI 1 lần duy nhất
        inventoryUI = GetComponentInParent<InventoryUI>();
    }
    public void SetItem(Item item)
    {
        currentItem = item;

        // 🛑 stop animation cũ (QUAN TRỌNG)
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
        }

        if (item == null)
        {
            frame.sprite = emptyFrame;
            itemIcon.enabled = false;
            itemIcon.sprite = null; // ✅ thêm dòng này
        }
        else
        {
            frame.sprite = filledFrame;
            itemIcon.enabled = true;

            // 👉 nếu là item có animation (Rune)
            if (item.animationFrames != null && item.animationFrames.Length > 0)
            {
                animCoroutine = StartCoroutine(PlayAnimation(item));
            }
            else
            {
                // 👉 item tĩnh (Potion)
                itemIcon.sprite = item.icon;
            }
        }
    }

    IEnumerator PlayAnimation(Item item)
    {
        int index = 0;

        while (true)
        {
            itemIcon.sprite = item.animationFrames[index];
            index = (index + 1) % item.animationFrames.Length;

            yield return new WaitForSeconds(item.animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
            frame.color = hoverColor;

        // 🔥 phóng to nhẹ
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            frame.color = normalColor;

        // 🔥 về bình thường
        transform.localScale = Vector3.one;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        originalParent = transform.parent;
        transform.SetParent(canvas.transform);

        LayoutElement layout = gameObject.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        canvasGroup.blocksRaycasts = false;

        // 🔥 hiệu ứng drag
        canvasGroup.alpha = 0.6f;
        transform.localScale = Vector3.one * 1.2f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isSelected = !isSelected;

        frame.color = isSelected ? selectedColor : normalColor;

        // 🔥 giữ scale khi selected
        transform.localScale = isSelected ? Vector3.one * 1.15f : Vector3.one;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;

        LayoutElement layout = GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.ignoreLayout = false;
            Destroy(layout); // 👉 QUAN TRỌNG: xóa luôn cho sạch
        }

        canvasGroup.blocksRaycasts = true;

        // 🔥 reset effect
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot draggedSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (draggedSlot == null) return;

        var items = InventoryManager.Instance.items;

        int from = draggedSlot.slotIndex;
        int to = slotIndex;

        // ❗ check hợp lệ
        if (from >= items.Count) return;

        // 👉 nếu drop vào slot trống
        if (to >= items.Count)
        {
            // ❗ KHÔNG remove → chỉ swap với null
            items.Add(null); // mở rộng list nếu cần
        }

        // đảm bảo to nằm trong list
        if (to >= items.Count) return;

        // swap
        Item temp = items[to];
        items[to] = items[from];
        items[from] = temp;

        inventoryUI.Refresh();
    }
}