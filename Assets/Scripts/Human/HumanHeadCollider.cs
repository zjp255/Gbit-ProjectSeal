using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanHeadCollider : MonoBehaviour
{
    private int createToyCount = 0;
    public GameObject toy;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 500);

        int randomNumber = Random.Range(0, 10);
        transform.parent.GetComponent<Human>().dizzy();
        if(createToyCount == 0 && randomNumber <= 4 )
        {
            
            Instantiate(toy);
            createToyCount++;
        }
    }
}
