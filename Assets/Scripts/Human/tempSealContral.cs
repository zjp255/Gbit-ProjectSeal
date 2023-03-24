using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempSealContral : MonoBehaviour
{
    public float speed = 1;
    private Transform playerTransform;
    private CharacterController characterController;
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = this.gameObject.transform;
        //characterController = this.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            //characterController.Move(Vector3.forward * speed * Time.deltaTime);
            playerTransform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.D))
        {
            //characterController.Move(Vector3.right * speed * Time.deltaTime);
           playerTransform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.A))
        {
            //characterController.Move(Vector3.left * speed * Time.deltaTime);
            playerTransform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.S))
        {
            //characterController.Move(Vector3.back * speed * Time.deltaTime);
            playerTransform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerTransform.GetComponent<Rigidbody>().AddForce(Vector3.up * 500);
        }

    }
}
