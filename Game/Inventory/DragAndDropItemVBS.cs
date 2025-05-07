using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class DragAndDropItemVBS : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    #region Переменные

    private QuickSlotInventoryVBS _quickslotInventory;
    private RectTransform _rectTransform;
    private Transform _transform;

    [SerializeField] private Image _icon;
    [SerializeField] private Slot oldSlot;
    [HideInInspector] public Transform player;

    #endregion

    #region Awake/Start

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _transform = transform;
    }

    private void Start()
    {
        _quickslotInventory = FindObjectOfType<QuickSlotInventoryVBS>();
    }

    #endregion

    #region Система перетаскивания

    public void OnPointerDown(PointerEventData eventData)
    {
        if (oldSlot.itemSO == null)
            return;

        SetIconAlpha(0.75f);
        _icon.raycastTarget = false;
        _transform.SetParent(_transform.parent.parent.parent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (oldSlot.itemSO == null)
            return;

        _rectTransform.position += (Vector3)eventData.delta;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (oldSlot.itemSO == null)
            return;

        SetIconAlpha(1f);
        _icon.raycastTarget = true;
        _transform.SetParent(oldSlot.transform);
        _transform.position = oldSlot.transform.position;

        var raycastObject = eventData.pointerCurrentRaycast.gameObject;
        if (eventData != null && eventData.pointerCurrentRaycast.gameObject && raycastObject.name == "InventoryBG [panel]")
        {
            DropItem();
        }
        else
        {
            Slot newSlot = raycastObject.transform.parent?.parent?.GetComponent<Slot>();
            if (newSlot)
            {
                ExchangeSlotData(newSlot);
                _quickslotInventory.CheckItemInHand();
            }
        }
    }

    #endregion

    #region События

    private void SetIconAlpha(float alpha)
    {
        _icon.color = new Color(1, 1, 1, alpha);
    }

    private void DropItem()
    {
        GameObject itemObject = PhotonNetwork.Instantiate(oldSlot.itemSO.DropPrefab.name, player.position + Vector3.up + player.forward, Quaternion.identity);
        itemObject.GetComponent<Item>().amount = oldSlot.amount;
        NullifySlotData();
        _quickslotInventory.CheckItemInHand();
    }

    public void NullifySlotData()
    {
        SetSlotData(oldSlot, null, 0, null, "");
    }

    private void ExchangeSlotData(Slot newSlot)
    {
        ItemSO oldItemSO = oldSlot.itemSO;
        int oldAmount = oldSlot.amount;
        Sprite oldIconSprite = oldSlot.icon.sprite;

        SetSlotData(oldSlot, newSlot.itemSO, newSlot.amount, newSlot.icon.sprite, GetAmountText(newSlot.itemSO, newSlot.amount));
        SetSlotData(newSlot, oldItemSO, oldAmount, oldIconSprite, GetAmountText(oldItemSO, oldAmount));
    }

    private void SetSlotData(Slot slot, ItemSO itemSO, int amount, Sprite sprite, string amountText)
    {
        slot.itemSO = itemSO;
        slot.amount = amount;
        slot.icon.sprite = sprite;
        slot.icon.color = itemSO != null ? Color.white : new Color(1, 1, 1, 0);
        slot.amountText.text = amountText;
    }

    private string GetAmountText(ItemSO itemSO, int amount)
    {
        return itemSO && itemSO.MaximumAmount != 1 ? amount.ToString() : "";
    }

    #endregion

    private struct SlotData
    {
        public ItemSO ItemSO;
        public int Amount;
        public Image Icon;
        public TMP_Text AmountText;
    }
}