using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirtyBarUI : MonoBehaviour
{
    public Image fillImage;

    private void Start()
    {
        GameManager.Instance.playerStats.OnDirtyChanged += UpdateUI;
    }

    private void OnDestroy()
    {
        GameManager.Instance.playerStats.OnDirtyChanged -= UpdateUI;
    }

    void UpdateUI(float curDirty)
    {
        fillImage.fillAmount = 1- curDirty / Const.DIRTY_MAX;
    }
}
