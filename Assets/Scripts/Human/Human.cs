using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum E_HumanStatus 
{ 
    idle,
    patrol,//Ѳ��
    arrest,//ץ��
    backPatrol
}

public class Human : MonoBehaviour
{
    private E_HumanStatus status;//����״̬
    private int nextPatrolPoint;
    public List<Vector3> patrolPointS;//Ѳ�ߵ�

    public float patrolSpeed;//Ѳ���ٶ�
    public float arrestSpeed;//ץ���ٶ�
    public float alertRange;//������Χ
    public float outRange;//����ץ����Χ
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

    #endregion//ͨ�õ�

    #region idle
    /// <summary>
    /// �ҵ���һ��Ѳ�ߵ�
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
    /// �ҵ������Ѳ�ߵ�
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
    /// ��Ѳ��״̬�µ��ж�
    /// </summary>
    void inPatrol()
    {
        moveAtPatrol();
        playerIsInRange();
    }
    /// <summary>
    /// ��patrol״̬�µ��ƶ�
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
    /// �������Ƿ��ڲ����Χ��
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
    /// ��BackPatrol״̬�µ��ж�
    /// </summary>
    void inBackPatrol()
    {
        getNextPatrolPoint();
        moveAtBackPatrol();
    }

    /// <summary>
    /// ��BackPatrol״̬�µ��ƶ�
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
