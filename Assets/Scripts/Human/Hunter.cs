using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_hunterType
{ 
    normal,//正常的猎人
    car//车上下来的猎人
}

public class Hunter : Human
{
    [Header("Hunter")]
    public E_hunterType hunterType = E_hunterType.normal;
    private int loopCount = 0;

    [Header("逮捕")]
    public float arrestSpeed;//抓捕速度
    public float outRange;//放弃抓捕范围

    [Header("LoseSight")]
    private Vector3 playerMissPoint;//player被建筑物遮挡前的点
    private int changeDirCount = 0;
    Vector3 lookAtLeft;
    Vector3 lookAtRight;

    [Header("视野")]
    public float sightR;//视野范围半径
    public float maxSightR;//最大视野范围半径
    public float addSightR;//苏醒后增加的半径
    public float sightAngle = 60f;//视野角度
    public bool isShowSight = false;//是否显示视野
    int layerMask = 0;
    [Header("警戒条")]
    public float warningSpeed;//警戒值增长速度
    public GameObject warningStrip;//警戒条
    private Vector2 warningStripSize;//警戒条size
    private float warningPoint;//警戒值

    [Header("Animation")]
    public Animator animator;

    //private bool playerIsInRange = false;
    // Start is called before the first frame update
    void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        layerMask = ~layerMask;

        warningStrip = Instantiate(warningStrip);
        warningStrip.transform.SetParent(GameObject.Find("HumanCanvas").transform);
        warningStripSize = warningStrip.GetComponent<RectTransform>().sizeDelta;
        status = E_HumanStatus.idle;
        nextPatrolPoint = 0;
        characterController = transform.GetComponent<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player");
        //angle_rightTan = (180 - sightAngle) / 2 / 180  * Mathf.PI;
        //angle_leftTan = ((180 - sightAngle) / 2 + sightAngle) / 180 * Mathf.PI;
       

        if (hunterType == E_hunterType.car)
        {
            warningPoint = 100;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (delayTime == 0)
        {
            switch (status)
            {
                case E_HumanStatus.idle:
                    animator.SetBool("isRun", false);
                    getNextPatrolPoint();
                    if (hunterType == E_hunterType.car && nextPatrolPoint == 0)
                    {
                        loopCount++;
                    }
                    break;
                case E_HumanStatus.patrol:
                    animator.SetBool("isRun", true);
                    inPatrol();
                    break;
                case E_HumanStatus.warning:
                    animator.SetBool("isRun",false);
                    inWarning();
                    break;
                case E_HumanStatus.arrest:
                    animator.SetBool("isRun",true);
                    inArrest();
                    break;
                case E_HumanStatus.loseSight1:
                    inLoseSight1();
                    break;
                case E_HumanStatus.loseSight2:
                    animator.SetBool("isRun", false);
                    inLoseSight2();
                    break;
                case E_HumanStatus.backPatrol:
                    animator.SetBool("isRun", true);
                    inBackPatrol();
                    break;
            }
        }
        else
        {
            closeWarningStrip();
            animator.SetBool("isDizzy", true);
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

            // 判断动画是否已经播放完毕
            if (currentState.normalizedTime <= 0.55f && currentState.IsName("Hit To Head"))
            {
                characterController.SimpleMove(transform.forward * patrolSpeed);
            }
            else
            {
                delayTime -= Time.deltaTime;
                if (delayTime < 0)
                {
                    delayTime = 0;
                    aware();
                    animator.SetBool("isDizzy", false);
                }
            }
        }
    }
    #region universal
    /// <summary>
    /// 检测玩家是否在察觉范围内
    /// </summary>
    public bool playerIsInRange()
    {
        if (player != null)
        {
            float angle = Vector3.Angle(transform.forward, player.transform.position - transform.position);

            if (Vector3.Distance(transform.position, player.transform.position) < sightR && angle < sightAngle)
            {
                RaycastHit hit;
                bool a = Physics.Raycast(new Ray(transform.position, player.transform.position + new Vector3(0,0.5f,0) - transform.position), out hit, outRange, layerMask);              
                if (hit.transform.tag == "Player")
                {

                    return true;
                }
            }
            return false;
        }
        else {
            return false;
        }
    }
    #endregion

    #region patrol
    /// <summary>
    /// 在巡逻状态下的行动
    /// </summary>
    public void inPatrol()
    {
        moveAtPatrol();
        if (loopCount == 2 && hunterType == E_hunterType.car)
        {
            GameObject.Destroy(this.gameObject);
        }
        if (playerIsInRange())
        {
            
            status = E_HumanStatus.warning;
        }
    }
    #endregion

    #region Warning//警觉相关
    /// <summary>
    /// 在警戒状态下的行动
    /// </summary>
    void inWarning()
    {
        warningPointChange();
        warningAction();
    }
    /// <summary>
    /// 警戒值的变化
    /// </summary>
    void warningPointChange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (playerIsInRange())
        {
            if (distance < sightR / 2)
            {
                warningPoint = 100;
            }
            if (distance >= sightR / 2)
            {
                warningPoint += (warningSpeed * Time.deltaTime);
            }
            opeanWarningStrip();
            gameObject.transform.forward = new Vector3(player.transform.position.x - transform.position.x, 0, player.transform.position.z - transform.position.z);
        }
        else
        {
            warningPoint -= (warningSpeed * Time.deltaTime);
        }
        WarningStripUI();
    }
    /// <summary>
    /// 行动
    /// </summary>
    void warningAction()
    {
        if (warningPoint > 100)
        {
            status = E_HumanStatus.arrest;
            closeWarningStrip();
            warningPoint = 0;
        }
        if (warningPoint < 0)
        {
            status = E_HumanStatus.patrol;
            closeWarningStrip();
            warningPoint = 0;
        }

    }

    #region WarningStrip//警戒条相关

    /// <summary>
    /// 控制警戒条的UI
    /// </summary>
    void WarningStripUI()
    {
        warningStrip.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 30, 0);
        warningStrip.transform.Find("Image").GetComponent<RectTransform>().sizeDelta = new Vector2(warningStripSize.x, warningStripSize.y * (warningPoint / 100));
    }
    /// <summary>
    /// 激活警戒条
    /// </summary>
    void opeanWarningStrip()
    {
        warningStrip.SetActive(true);
    }
    /// <summary>
    /// 关闭警戒条
    /// </summary>
    void closeWarningStrip()
    {
        warningStrip.SetActive(false);
    }
    #endregion

    #endregion

    #region Arrest
    void inArrest()
    {
        move(player.transform.position, arrestSpeed);
        playerOutRange();
    }

    void playerOutRange()
    {
        RaycastHit hit;
        Physics.Raycast(new Ray(transform.position, player.transform.position + new Vector3(0, 0.5f, 0) - transform.position), out hit, outRange, layerMask);
        if (Vector3.Distance(transform.position, player.transform.position) > outRange || hit.transform.tag != "Player")
        {
            playerMissPoint = player.transform.position;
            status = E_HumanStatus.loseSight1;
        }
    }

    #endregion

    #region LoseSight//丢失玩家视野后的行为
    void inLoseSight1()
    {
        moveAtLoseSight1();
        if (playerIsInRange())
        {
            status = E_HumanStatus.arrest;
        }
    }

    void moveAtLoseSight1()
    {
        if (    playerMissPoint.x + 0.1 > transform.position.x &&
                playerMissPoint.x - 0.1 < transform.position.x &&
                playerMissPoint.z + 0.1 > transform.position.z &&
                playerMissPoint.z - 0.1 < transform.position.z
                )
        {
            status = E_HumanStatus.loseSight2;
            lookAtRight = transform.TransformPoint(new Vector3(2, 0, 2));
            lookAtLeft =   transform.TransformPoint(new Vector3(-2, 0, 2));
        }
        else
        {
            gameObject.transform.forward = new Vector3(patrolPointS[nextPatrolPoint].x - transform.position.x, 0, patrolPointS[nextPatrolPoint].z - transform.position.z);
            characterController.SimpleMove(transform.forward * patrolSpeed);
            move(playerMissPoint, patrolSpeed);
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, transform.TransformPoint(transform.forward)), out hit, outRange, layerMask) && hit.transform.tag != "Player")
            {
                status = E_HumanStatus.loseSight2;
                lookAtRight = transform.TransformPoint(new Vector3(2, 0, -2));
                lookAtLeft = transform.TransformPoint(new Vector3(-2, 0, -2));

            }
            else
            {
                status = E_HumanStatus.backPatrol;
            }
        }
    }

    void inLoseSight2()
    {
        moveAtLoseSight2();
        if (playerIsInRange())
        {
            status = E_HumanStatus.arrest;
        }
    }

    void moveAtLoseSight2()
    {
        if (changeDirCount == 0)
        {
            changeDir(lookAtRight);
            if (isTurning == false)
            {
                changeDirCount++;
            }
        }
        else if(changeDirCount == 1) 
        {
            changeDir(lookAtLeft);
            if (isTurning == false)
            {
                status = E_HumanStatus.backPatrol;
                changeDirCount = 0;
            }
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
            if (hunterType == E_hunterType.car)
            {
                GameObject.Destroy(this.gameObject);
            }
        }
        else
        {
            //gameObject.transform.forward = new Vector3(patrolPointS[nextPatrolPoint].x - transform.position.x, 0, patrolPointS[nextPatrolPoint].z - transform.position.z);
            //characterController.SimpleMove(transform.forward * patrolSpeed);
            move(patrolPointS[nextPatrolPoint], patrolSpeed);
        }
    }
    #endregion



    #region aware//从眩晕中苏醒
    void aware()
    {
        sightR += addSightR;
        if (sightR > maxSightR)
        {
            sightR = maxSightR;
        }
    }
    #endregion

}
