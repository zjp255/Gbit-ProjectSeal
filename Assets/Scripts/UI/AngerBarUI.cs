using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AngerBarUI : MonoBehaviour
{
    public Image fillImage;

    private void Start()
    {
        GameManager.Instance.playerStats.OnAngerChanged += UpdateUI;
        UpdateUI(GameManager.Instance.playerStats.AngerNum);
    }

    private void OnDestroy()
    {
        GameManager.Instance.playerStats.OnAngerChanged -= UpdateUI;
    }

    void UpdateUI(int curAnger)
    {
        fillImage.fillAmount = 1 - (float)curAnger / Const.ANGER_MAX;
    }
}
