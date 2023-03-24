using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum E_HumanStatus 
{ 
    idle,
    patrol,//巡逻
    arrest,//抓捕
    backPatrol
}

public class Human : MonoBehaviour
{
    private E_HumanStatus status;//人类状态
    private int nextPatrolPoint;
    public List<Vector3> patrolPointS;//巡逻点

    public float patrolSpeed;//巡逻速度
    public float arrestSpeed;//抓捕速度
    public float alertRange;//警觉范围
    public float outRange;//放弃抓捕范围
    //collider
    private CharacterController characterController;



    private bool isGrowth = true;
    private GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        status = E_HumanStatus.idle;
        nextPatrolPoint = 0;
        characterController = transform.GetComponent<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {

        switch (status)
        {
            case E_HumanStatus.idle:
                getNextPatrolPoint();
                break;
            case E_HumanStatus.patrol:
                inPatrol();
                break;
            case E_HumanStatus.arrest:
                inArrest();
                break;
            case E_HumanStatus.backPatrol:
                inBackPatrol();
                break;
        }
    }


    #region Universal 
    void move(Vector3 target,float speed)
    {
        gameObject.transform.forward = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z);
        characterController.SimpleMove(transform.forward * speed);
    }

    #endregion//通用的

    #region idle
    /// <summary>
    /// 找到下一个巡逻点
    /// </summary>
    void getNextPatrolPoint()
    {
        if (status == E_HumanStatus.idle)
        {
            if (isGrowth)
            {
                nextPatrolPoint += 1;
                if (nextPatrolPoint == patrolPointS.Count - 1 || nextPatrolPoint > patrolPointS.Count - 1)
                {
                    nextPatrolPoint = patrolPointS.Count - 1;
                    isGrowth = false;
                }
            }
            else
            {
                nextPatrolPoint -= 1;
                if (nextPatrolPoint == 0 || nextPatrolPoint < 0)
                {
                    nextPatrolPoint = 0;
                    isGrowth = true;
                }
            }
            status = E_HumanStatus.patrol;
        }
        else if (status == E_HumanStatus.backPatrol)
        {
            nextPatrolPoint = getClosestPatrolPoint();
        }
        //gameObject.transform.LookAt(patrolPointS[nextPatrolPoint]);        
       
    }
    /// <summary>
    /// 找到最近的巡逻点
    /// </summary>
    /// <returns></returns>
    int getClosestPatrolPoint()
    {
        int closestPoint = 0;
        float distance = 0;
        for (int i = 0 ; i < patrolPointS.Count; i++)
        {
            float tempDistance = Vector3.Distance(patrolPointS[i], this.gameObject.transform.position); ;
            if (i == 0)
            {
                distance = tempDistance;
                closestPoint = i;
            }
             else
            {
                if (distance > tempDistance)
                {
                    distance = tempDistance;
                    closestPoint = i;
                }
            }
        }
        return closestPoint;
    }
    #endregion

    #region Patrol
    /// <summary>
    /// 在巡逻状态下的行动
    /// </summary>
    void inPatrol()
    {
        moveAtPatrol();
        playerIsInRange();
    }
    /// <summary>
    /// 在patrol状态下的移动
    /// </summary>
    void moveAtPatrol()
    {
        if (patrolPointS[nextPatrolPoint].x + 0.1 > transform.position.x &&
                patrolPointS[nextPatrolPoint].x - 0.1 < transform.position.x &&
                patrolPointS[nextPatrolPoint].z + 0.1 > transform.position.z &&
                patrolPointS[nextPatrolPoint].z - 0.1 < transform.position.z
                )
        {
            status = E_HumanStatus.idle;
        }
        else
        {
            //gameObject.transform.forward = new Vector3(patrolPointS[nextPatrolPoint].x - transform.position.x, 0, patrolPointS[nextPatrolPoint].z - transform.position.z);
            //characterController.SimpleMove(transform.forward * patrolSpeed);
            move(patrolPointS[nextPatrolPoint], patrolSpeed);
        }
    }

    /// <summary>
    /// 检测玩家是否在察觉范围内
    /// </summary>
    void playerIsInRange()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < alertRange)
        {
            status = E_HumanStatus.arrest;
        }
    }
    #endregion


    #region Arrest
    void inArrest()
    {
        move(player.transform.position,arrestSpeed);
        playerOutRange();
    }

    void playerOutRange()
    {
        if (Vector3.Distance(transform.position, player.transform.position) > outRange)
        {
            status = E_HumanStatus.backPatrol;
        }
    }

    #endregion

    #region BackPatrol
    /// <summary>
    /// 在BackPatrol状态下的行动
    /// </summary>
    void inBackPatrol()
    {
        getNextPatrolPoint();
        moveAtBackPatrol();
    }

    /// <summary>
    /// 在BackPatrol状态下的移动
    /// </summary>
    void moveAtBackPatrol()
    {
        if (patrolPointS[nextPatrolPoint].x + 0.1 > transform.position.x &&
                patrolPointS[nextPatrolPoint].x - 0.1 < transform.position.x &&
                patrolPointS[nextPatrolPoint].z + 0.1 > transform.position.z &&
                patrolPointS[nextPatrolPoint].z - 0.1 < transform.position.z
                )
        {
            status = E_HumanStatus.idle;
        }
        else
        {
            //gameObject.transform.forward = new Vector3(patrolPointS[nextPatrolPoint].x - transform.position.x, 0, patrolPointS[nextPatrolPoint].z - transform.position.z);
            //characterController.SimpleMove(transform.forward * patrolSpeed);
            move(patrolPointS[nextPatrolPoint], patrolSpeed);
        }
    }
    #endregion

}
