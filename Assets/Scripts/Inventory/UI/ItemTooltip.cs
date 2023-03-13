using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    public Text itemNameText;
    public Text itemInfoText;

    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        UpdatePosition();
    }
    private void Update()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        Vector3 mousePos = Input.mousePosition;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        //获取tooltip的宽高
        float width = corners[3].x - corners[0].x;
        float height = corners[1].y - corners[0].y;
        float horizentalGap = 0.1f * width;
        float verticalGap = 0.1f * height;

        rectTransform.position = mousePos;
        //保持垂直方向不超出屏幕
        if (mousePos.y - verticalGap - height > 0)
            rectTransform.position += Vector3.down * (height * 0.5f + verticalGap);
        else
            rectTransform.position += Vector3.up * (height * 0.5f + verticalGap);

        //保持水平方向不超出屏幕
        if (mousePos.x + horizentalGap + width < Screen.width)
            rectTransform.position += Vector3.right * (width * 0.5f);
        else
            rectTransform.position += Vector3.left * (width * 0.5f);
    }

    public void SetupTooltip(ItemData_SO item)
    {
        itemNameText.text = item.itemName;
        itemInfoText.text = item.description;
    }

}
