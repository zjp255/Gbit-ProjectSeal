using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBed : MonoBehaviour
{
    E_HumanStatus status;
    public List<Vector3> patrolPointS;//巡逻点
    public float patrolSpeed;//巡逻速度
    protected int nextPatrolPoint;
    protected bool isGrowth = true;

    // Start is called before the first frame update
    void Start()
    {
        
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
                moveAtPatrol();
                break;
        }
    }

    public void move(Vector3 target, float speed)
    {

        transform.Translate((target - transform.position).normalized * speed * Time.deltaTime,Space.World);
       
    }

  
    /// <summary>
    /// 找到下一个巡逻点
    /// </summary>
    public void getNextPatrolPoint()
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
        

    }

    public void moveAtPatrol()
    {
        Vector3 position = transform.position;
        if (patrolPointS[nextPatrolPoint].x + 0.1 > position.x &&
                patrolPointS[nextPatrolPoint].x - 0.1 < position.x &&
                patrolPointS[nextPatrolPoint].z + 0.1 > position.z &&
                patrolPointS[nextPatrolPoint].z - 0.1 < position.z
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

}
