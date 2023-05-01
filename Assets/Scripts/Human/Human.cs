using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_HumanStatus 
{ 
    idle,
    patrol,//巡逻
    warning,//警觉
    arrest,//抓捕
    loseSight1,//追丢
    loseSight2,
    backPatrol
}

public enum E_PatrolType
{
   loop,//循环
   palindrome//回文
}

public class Human : MonoBehaviour
{
    public E_HumanStatus status;//人类状态
    

    [Header("巡逻")]
    public List<Vector3> patrolPointS;//巡逻点
    public float patrolSpeed;//巡逻速度
    public E_PatrolType patrolType = E_PatrolType.palindrome;
    protected int nextPatrolPoint; 

    //[Header("逮捕")]
    //public float arrestSpeed;//抓捕速度
    //public float outRange;//放弃抓捕范围

    //[Header("视野")]
    //public float sightR;//视野范围半径
    //public float sightAngle = 60f;//视野角度
    //public bool isShowSight = false;//是否显示视野

    //[Header("警戒条")]
    //public float warningSpeed;//警戒值增长速度
    //public GameObject warningStrip;//警戒条
    //private Vector2 warningStripSize;//警戒条size
    //private float warningPoint;//警戒值
    ////private bool playerIsInRange = false;

    [Header("眩晕")]
    public float dizzyTime = 8f;//眩晕时间
    //collider

    [Header("转身")]
    public float turningSpeed = 50;

    protected CharacterController characterController;
    protected bool isGrowth = true;//判断巡逻路线
    protected bool isTurning = false;//判断是否在转身
    protected float startTurnAngle = 0;
    protected float delayTime = 0;
    protected GameObject player;

    //float angle_rightTan = 0;//第一象tan
    //float angle_leftTan = 0;//第二象tan

    // Start is called before the first frame update
    // void Start()
    //{
    //    warningStripSize = warningStrip.GetComponent<RectTransform>().sizeDelta;
    //    status = E_HumanStatus.idle;
    //    nextPatrolPoint = 0;
    //    characterController = transform.GetComponent<CharacterController>();
    //    player = GameObject.FindGameObjectWithTag("Player");
    //    //angle_rightTan = (180 - sightAngle) / 2 / 180  * Mathf.PI;
    //    //angle_leftTan = ((180 - sightAngle) / 2 + sightAngle) / 180 * Mathf.PI;
      
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    if (delayTime == 0)
    //    {
    //        switch (status)
    //        {
    //            case E_HumanStatus.idle:
    //                getNextPatrolPoint();
    //                break;
    //            case E_HumanStatus.patrol:
    //                inPatrol();
    //                break;
    //            case E_HumanStatus.warning:
    //                inWarning();
    //                break;
    //            case E_HumanStatus.arrest:
    //                inArrest();
    //                break;
    //            case E_HumanStatus.backPatrol:
    //                inBackPatrol();
    //                break;
    //        }
    //    }
    //    else 
    //    {
    //        delayTime -= Time.deltaTime;
    //        if (delayTime < 0)
    //            delayTime = 0;
    //    }
    //}

    #region Universal 



    /// <summary>
    /// 改变方向
    /// </summary>
    /// <param name="target">transform.forward point to target</param>
    public void changeDir(Vector3 target)
    {
        float dirAngle = Vector3.SignedAngle(transform.forward, new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z), Vector3.up);
        if (isTurning == false)
        {
            startTurnAngle = dirAngle;
        }
        if (dirAngle > 5 || dirAngle < -5)
        {
            isTurning = true;
            if (startTurnAngle >= 0)
            {
                this.transform.Rotate(Vector3.up, turningSpeed * Time.deltaTime);
            }
            else
            {
                
                this.transform.Rotate(Vector3.up, -turningSpeed * Time.deltaTime);
            }
        }
        else
        {
            isTurning = false;
        }

    }
    /// <summary>
    /// 控制移动
    /// </summary>
    /// <param name="target">目标坐标</param>
    /// <param name="speed">移动速度</param>
    public void move(Vector3 target,float speed)
    {
        changeDir(target);
        if (isTurning == false)
        {
            gameObject.transform.forward = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z);
            characterController.SimpleMove(transform.forward * speed);
        }
    }
    /// <summary>
    /// 控制眩晕
    /// </summary>
    public void dizzy()
    {
        delayTime = dizzyTime;
    }

   

    //public void setPlayerIsInRange(bool bol)
    //{
    //    playerIsInRange = bol;
    //}
    #endregion//通用的

    #region idle
    /// <summary>
    /// 找到下一个巡逻点
    /// </summary>
    public void getNextPatrolPoint()
    {
        if (status == E_HumanStatus.idle)
        {
            if (patrolType == E_PatrolType.palindrome)
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
            }
            else if (patrolType == E_PatrolType.loop)
            {
                nextPatrolPoint = ++nextPatrolPoint % patrolPointS.Count;
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
    /// 在patrol状态下的移动
    /// </summary>
   public void moveAtPatrol()
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
