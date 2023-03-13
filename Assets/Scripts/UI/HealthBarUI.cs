using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;
    public Transform barPoint;
    public bool alwaysVisible;
    public float visibleTime;
    private float timeLeft;

    Image healtSlider;
    Transform UIBar;
    Transform cam;

    CharacterStats currentStats;

    private void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        cam = Camera.main.transform;

        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                UIBar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healtSlider = UIBar.GetChild(0).GetComponent<Image>();
                UIBar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth,int maxHealth)
    {
        if (currentHealth <= 0)
            Destroy(UIBar.gameObject);

        UIBar.gameObject.SetActive(true);
        timeLeft = visibleTime;
        float sliderPercent = (float)currentHealth / maxHealth;
        healtSlider.fillAmount = sliderPercent;
    }

    //在上一帧执行后才执行
    private void LateUpdate()
    {
        if(UIBar != null)
        {
            UIBar.position = barPoint.position;
            UIBar.forward = -cam.forward;

            if (timeLeft <= 0 && !alwaysVisible)
                UIBar.gameObject.SetActive(false);
            else
                timeLeft -= Time.deltaTime;
        }
    }
}
