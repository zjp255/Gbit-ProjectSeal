using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Lead : MonoBehaviour
{
    private GameObject image;
    private GameObject text;
    //public GameObject[] sealFather;


    private void Awake()
    {
        image = GameObject.Find("talk_bar");
        text = GameObject.Find("word");
    }

    void setActiveTrue(string s)
    {
        image.SetActive(true);
        TextMeshProUGUI textComponent = text.GetComponent<TextMeshProUGUI>();
        if(textComponent != null)
        {
            textComponent.text = s;
        }
    }
    void setActiveFalse()
    {
        image.SetActive(false);
    }

   /* void setFather(int num) 
    {
        for(int i=0;i<4;i++)
        {
            if (i != num-1)
            {
                sealFather[i].SetActive(false);
            }
            else
            {
                sealFather[i].SetActive(true);
            }
        }
    }*/

    private void Start()
    {
        //sealFather = new GameObject[4];
        //setFather(0);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLead += setActiveTrue;

        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLeadClosed += setActiveFalse;
    }

    private void OnDestroy()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLead -= setActiveTrue;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLeadClosed -= setActiveFalse;
    }
}
