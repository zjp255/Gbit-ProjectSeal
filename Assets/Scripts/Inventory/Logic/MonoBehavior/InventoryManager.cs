using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : Singleton<InventoryManager>
{
    public class DragData
    {
        public SlotHolder originalHolder;
        public RectTransform originalParent;
    }

    [Header("Inventory Data")]
    public InventoryData_SO inventoryTemplate;
    public InventoryData_SO inventoryData;
    public InventoryData_SO actionTemplate;
    public InventoryData_SO actionData;
    public InventoryData_SO equipmentTemplate;
    public InventoryData_SO equipmentData;

    [Header("Containers")]
    public ContainerUI inventoryUI;
    public ContainerUI actionUI;
    public ContainerUI equipmentUI;

    [Header("Drag Canvas")]
    public Canvas dragCanvas;
    public DragData currentDrag;

    [Header("UI Panel")]
    public GameObject bagPanel;
    public GameObject statsPanel;
    bool isOpen = false;

    [Header("Stats Text")]
    public Text healthText;
    public Text attackText;

    [Header("Tooltip")]
    public ItemTooltip tooltip;

    protected override void Awake()
    {
        base.Awake();
        if (inventoryTemplate != null)
            inventoryData = Instantiate(inventoryTemplate);
        if (actionTemplate != null)
            actionData = Instantiate(actionTemplate);
        if (equipmentTemplate != null)
            equipmentData = Instantiate(equipmentTemplate);
    }

    public void SaveData()
    {
        SaveManager.Instance.Save(inventoryData, inventoryData.name);
        SaveManager.Instance.Save(actionData, actionData.name);
        SaveManager.Instance.Save(equipmentData, equipmentData.name);
    }

    public void LoadData()
    {
        SaveManager.Instance.Load(inventoryData, inventoryData.name);
        SaveManager.Instance.Load(actionData, actionData.name);
        SaveManager.Instance.Load(equipmentData, equipmentData.name);
    }

    private void Start()
    {
        LoadData();
        inventoryUI.RefreshUI();
        actionUI.RefreshUI();
        equipmentUI.RefreshUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;
            bagPanel.SetActive(isOpen);
            statsPanel.SetActive(isOpen);
        }
        UpdateStatsText(GameManager.Instance.playerStats.MaxHealth, GameManager.Instance.playerStats.attackData.minDamage,
            GameManager.Instance.playerStats.attackData.maxDamage);
    }

    public void UpdateStatsText(int health,int min,int max)
    {
        healthText.text = health.ToString();
        attackText.text = min + "-" + max;
    }

    #region 检查拖拽物品是否在每一个Slot范围内
    public bool CheckInInventoryUI(Vector3 position)
    {
        for(int i = 0; i < inventoryUI.slotHolders.Length; i++)
        {
            RectTransform t = inventoryUI.slotHolders[i].transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(t, position)) return true;
        }
        return false;
    }

    public bool CheckInActionUI(Vector3 position)
    {
        for (int i = 0; i < actionUI.slotHolders.Length; i++)
        {
            RectTransform t = actionUI.slotHolders[i].transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(t, position)) return true;
        }
        return false;
    }

    public bool CheckInEquipmentUI(Vector3 position)
    {
        for (int i = 0; i < equipmentUI.slotHolders.Length; i++)
        {
            RectTransform t = equipmentUI.slotHolders[i].transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(t, position)) return true;
        }
        return false;
    }

    #endregion
}
