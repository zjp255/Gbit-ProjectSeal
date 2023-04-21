using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : Human
{
    [Header("逮捕")]
    public float arrestSpeed;//抓捕速度
    public float outRange;//放弃抓捕范围

    [Header("视野")]
    public float sightR;//视野范围半径
    public float maxSightR;//最大视野范围半径
    public float addSightR;//苏醒后增加的半径
    public float sightAngle = 60f;//视野角度
    public bool isShowSight = false;//是否显示视野

    [Header("警戒条")]
    public float warningSpeed;//警戒值增长速度
    public GameObject warningStrip;//警戒条
    private Vector2 warningStripSize;//警戒条size
    private float warningPoint;//警戒值

    //private bool playerIsInRange = false;
    // Start is called before the first frame update
    void Start()
    {
        warningStrip = Instantiate(warningStrip);
        warningStrip.transform.SetParent(GameObject.Find("Canvas").transform);
        warningStripSize = warningStrip.GetComponent<RectTransform>().sizeDelta;
        status = E_HumanStatus.idle;
        nextPatrolPoint = 0;
        characterController = transform.GetComponent<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player");
        //angle_rightTan = (180 - sightAngle) / 2 / 180  * Mathf.PI;
        //angle_leftTan = ((180 - sightAngle) / 2 + sightAngle) / 180 * Mathf.PI;

    }

    // Update is called once per frame
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
                    inPatrol();
                    break;
                case E_HumanStatus.warning:
                    inWarning();
                    break;
                case E_HumanStatus.arrest:
                    inArrest();
                    break;
                case E_HumanStatus.backPatrol:
                    inBackPatrol();
                    break;
            }
        }
        else
        {
            delayTime -= Time.deltaTime;
            if (delayTime < 0)
            {
                delayTime = 0;
                aware();
            }
        }
    }
    #region universal
    /// <summary>
    /// 检测玩家是否在察觉范围内
    /// </summary>
    public bool playerIsInRange()
    {
        float angle = Vector3.Angle(transform.forward, player.transform.position - transform.position);
        if (Vector3.Distance(transform.position, player.transform.position) < sightR && angle < sightAngle)
        {
            RaycastHit hit;
            Physics.Raycast(new Ray(transform.position, player.transform.position - transform.position), out hit, outRange, 1 << 0);
            if (hit.transform.tag == "Player")
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region patrol
    /// <summary>
    /// 在巡逻状态下的行动
    /// </summary>
    public void inPatrol()
    {
        moveAtPatrol();
        if (playerIsInRange())
        {
            status = E_HumanStatus.warning;
        }
    }
    #endregion

    #region Warning
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

    #region WarningStrip

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
        Physics.Raycast(new Ray(transform.position, player.transform.position - transform.position), out hit, outRange, 1 << 0);
        if (Vector3.Distance(transform.position, player.transform.position) > outRange || hit.transform.tag != "Player")
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

    #region aware
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
