using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scientist : Human
{
     void Start()
    {
        status = E_HumanStatus.idle;
        nextPatrolPoint = 0;
        characterController = transform.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (delayTime == 0)
        {
            switch (status)
            {
                case E_HumanStatus.idle:
                    getNextPatrolPoint();
                    break;
                case E_HumanStatus.patrol:
                    moveAtPatrol();
                    break;
            }
        }
        else
        {
            delayTime -= Time.deltaTime;
            if (delayTime < 0)
                delayTime = 0;
        }
    }
}
