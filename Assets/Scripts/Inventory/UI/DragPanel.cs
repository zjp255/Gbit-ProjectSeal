using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragPanel : MonoBehaviour, IDragHandler,IPointerDownHandler
{
    RectTransform rectTransform;
    Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = InventoryManager.Instance.GetComponent<Canvas>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta/canvas.scaleFactor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //使得当前选择的面板始终在最前面（但注意不能挡住DragCanvas）
        rectTransform.SetSiblingIndex(2);
    }
}
