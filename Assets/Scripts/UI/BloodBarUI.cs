using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodBarUI : MonoBehaviour
{
    public Image fillImage;

    private void Start()
    {
        GameManager.Instance.playerStats.OnBloodChanged += UpdateUI;
    }

    private void OnDestroy()
    {
        GameManager.Instance.playerStats.OnBloodChanged -= UpdateUI;
    }

    void UpdateUI(int curBlood)
    {
        fillImage.fillAmount = 1 - (float)curBlood / Const.BLOOD_MAX;
    }
}
