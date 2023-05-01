using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lead : MonoBehaviour
{
    public GameObject image;
    public GameObject text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void setActiveTrue(string s)
    {
        Debug.Log("!!");
        text.GetComponent<Text>().text = s;
        image.SetActive(true);
    }
    void setActiveFalse()
    {
        image.SetActive(false);
    }

    private void OnEnable()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLead += setActiveTrue;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLeadClosed += setActiveFalse;
    }

    private void OnDisable()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLead -= setActiveTrue;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().OnLeadClosed -= setActiveFalse;
    }
}
