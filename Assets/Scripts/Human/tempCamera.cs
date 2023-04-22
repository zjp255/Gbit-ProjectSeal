using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempCamera : MonoBehaviour
{
    public Transform playerTrsfm;
    private Transform camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = this.gameObject.transform;
        
    }

    // Update is called once per frame
    void Update()
    {
        camera.position = new Vector3(playerTrsfm.position.x, playerTrsfm.position.y + 10, playerTrsfm.position.z - 10);
    }
}
