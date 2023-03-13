using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SlotType { BAG,WEAPON,ARMOR, ACTION }
public class SlotHolder : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public SlotType slotType;
    public ItemUI itemUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.clickCount % 2 == 0)
        {
            UseItem();
        }
    }

    public void UseItem()
    {
        if (itemUI.GetItem() == null) return;
        if(itemUI.GetItem().itemType == ItemType.Useable && itemUI.Bag.items[itemUI.Index].amount > 0)
        {
            GameManager.Instance.playerStats.ApplyHealth(itemUI.GetItem().useableItemData.healPoint);
            itemUI.Bag.items[itemUI.Index].amount -= 1;
        }
        UpdateItem();
    }

    public void UpdateItem()
    {
        switch (slotType)
        {
            case SlotType.BAG:
                itemUI.Bag = InventoryManager.Instance.inventoryData;
                break;
            case SlotType.WEAPON:
                itemUI.Bag = InventoryManager.Instance.equipmentData;
                //切换武器
                if(itemUI.Bag.items[itemUI.Index].itemData != null)
                {
                    GameManager.Instance.playerStats.ChangeWeapon(itemUI.Bag.items[itemUI.Index].itemData);
                }
                else
                {
                    //目前的设计中不会执行到这一部分
                    GameManager.Instance.playerStats.UnEquipWeapon();
                }
                break;
            case SlotType.ARMOR:
                itemUI.Bag = InventoryManager.Instance.equipmentData;
                break;
            case SlotType.ACTION:
                itemUI.Bag = InventoryManager.Instance.actionData;
                break;
        }
        var item = itemUI.Bag.items[itemUI.Index];
        itemUI.SetupItemUI(item.itemData, item.amount);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(itemUI.GetItem())
        {
            InventoryManager.Instance.tooltip.SetupTooltip(itemUI.GetItem());
            InventoryManager.Instance.tooltip.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.tooltip.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        InventoryManager.Instance.tooltip.gameObject.SetActive(false);
    }
}
