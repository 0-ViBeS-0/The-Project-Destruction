using System.Collections.Generic;
using UnityEngine;

public class InventoryManagerVBS : MonoBehaviour
{
    #region Переменные

    [SerializeField] private GameObject[] _inventoryUI;
    [SerializeField] private Transform _inventorySlotsContainer;
    [SerializeField] private Transform _inventoryFastSlotsContainer;

    [HideInInspector] public List<Slot> slots;
    private List<DragAndDropItemVBS> dragAndDropItems;
    [HideInInspector] public bool isOpen;

    #endregion

    #region Start

    private void Start()
    {
        InitializeSlots();
    }

    #endregion

    #region Инициализация

    private void InitializeSlots()
    {
        int slotsCount = _inventorySlotsContainer.childCount;
        int fastSlotsCount = _inventoryFastSlotsContainer.childCount;

        slots = new List<Slot>(slotsCount + fastSlotsCount);
        dragAndDropItems = new List<DragAndDropItemVBS>(slotsCount + fastSlotsCount);

        for (int i = 0; i < slotsCount; i++)
        {
            slots.Add(_inventorySlotsContainer.GetChild(i).GetComponent<Slot>());
            dragAndDropItems.Add(_inventorySlotsContainer.GetChild(i).GetChild(0).GetComponent<DragAndDropItemVBS>());
        }
        for (int i = 0; i < fastSlotsCount; i++)
        {
            slots.Add(_inventoryFastSlotsContainer.GetChild(i).GetComponent<Slot>());
            dragAndDropItems.Add(_inventoryFastSlotsContainer.GetChild(i).GetChild(0).GetComponent<DragAndDropItemVBS>());
        }
    }

    #endregion

    public void SetDragAndDropPLAYER(GameObject player)
    {
        foreach (DragAndDropItemVBS dadi in dragAndDropItems)
        {
            dadi.player = player.transform;
        }
    }

    public void SetInventoryActive(bool active)
    {
        isOpen = active;
        foreach (GameObject ui in _inventoryUI)
            ui.SetActive(active);
    }

    public void AddItem(ItemSO itemSO, int amount)
    {
        int remainingAmount = amount;

        foreach (Slot slot in slots)
        {
            if (slot.itemSO == itemSO)
            {
                int spaceAvailable = itemSO.MaximumAmount - slot.amount;

                if (remainingAmount <= spaceAvailable)
                {
                    slot.amount += remainingAmount;
                    slot.amountText.text = slot.amount.ToString();
                    return;
                }
                else
                {
                    slot.amount = itemSO.MaximumAmount;
                    slot.amountText.text = slot.amount.ToString();
                    remainingAmount -= spaceAvailable;
                }
            }
        }

        foreach (Slot slot in slots)
        {
            if (slot.itemSO == null)
            {
                slot.itemSO = itemSO;
                slot.amount = Mathf.Min(remainingAmount, itemSO.MaximumAmount);
                slot.SetIcon(itemSO.SpriteIcon);
                slot.amountText.text = itemSO.MaximumAmount != 1 ? slot.amount.ToString() : "";
                return;
            }
        }

        Debug.LogWarning("No empty slots available for the item.");
    }
}