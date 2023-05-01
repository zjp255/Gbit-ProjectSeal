using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodBarUI : MonoBehaviour
{
    public Image fillImage;

    private void OnEnable()
    {
        GameManager.Instance.playerStats.OnBloodChanged += UpdateUI;
    }

    private void OnDisable()
    {
        GameManager.Instance.playerStats.OnBloodChanged -= UpdateUI;
    }

    void UpdateUI(int curBlood)
    {
        fillImage.fillAmount = 1 - (float)curBlood / Const.BLOOD_MAX;
    }
}
