using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AngerBarUI : MonoBehaviour
{
    public Image fillImage;

    private void OnEnable()
    {
        GameManager.Instance.playerStats.OnAngerChanged += UpdateUI;
    }

    private void OnDisable()
    {
        GameManager.Instance.playerStats.OnAngerChanged -= UpdateUI;
    }

    void UpdateUI(int curAnger)
    {
        fillImage.fillAmount = 1 - (float)curAnger / Const.ANGER_MAX;
    }
}
