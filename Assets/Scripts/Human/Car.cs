using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : Human
{
    [Header("Åñ³µ")]
    public GameObject hunterPrefab;
    private int hitCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        status = E_HumanStatus.idle;
        nextPatrolPoint = 0;
        characterController = transform.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (delayTime == 0 && hitCount < 2)
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
            {
                delayTime = 0;
                hitCount++;
                hitChange();
            }
        }
    }

    void hitChange()
    {
        if (hitCount == 1)
        {
            patrolSpeed /= 2;
        }
        if (hitCount == 2)
        {
            patrolSpeed = 0;
            createHunterPrefab();
            transform.Find("TriggerCollider").GetComponent<Collider>().isTrigger = false;
        }
    }

    void createHunterPrefab()
    {
        GameObject hunter = Instantiate(hunterPrefab) as GameObject;
        hunter.transform.position = transform.TransformPoint(new Vector3( + 2f / 2.5f,  0f, 2f / 5f));
        hunter.GetComponent<Hunter>().patrolPointS.Add(transform.TransformPoint( new Vector3(   3f / 2.5f, 0, -3.5f / 5f)));
        hunter.GetComponent<Hunter>().patrolPointS.Add(transform.TransformPoint(new Vector3(  3f / 2.5f, 0, +3.5f / 5f)));
        hunter.GetComponent<Hunter>().patrolPointS.Add(transform.TransformPoint(new Vector3(- 3f / 2.5f, 0, +3.5f / 5f)));
        hunter.GetComponent<Hunter>().patrolPointS.Add(transform.TransformPoint(new Vector3( - 3f / 2.5f, 0, -3.5f / 5f)));
        hunter.GetComponent<Hunter>().patrolType = E_PatrolType.loop;
    }
}
