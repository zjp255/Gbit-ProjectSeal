using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Lead : MonoBehaviour
{
    public GameObject image;
    public GameObject text;

    void setActiveTrue(string s)
    {
        text.GetComponent<TextMeshProUGUI>().text = s;
        image.SetActive(true);
    }
    void setActiveFalse()
    {
        image.SetActive(false);
    }

    private void Start()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLead += setActiveTrue;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLeadClosed += setActiveFalse;
    }

    private void OnDestroy()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLead -= setActiveTrue;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLeadClosed -= setActiveFalse;
    }
}
