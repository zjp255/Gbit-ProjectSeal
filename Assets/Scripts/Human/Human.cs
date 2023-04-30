using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_HumanStatus 
{ 
    idle,
    patrol,//Ѳ��
    warning,//����
    arrest,//ץ��
    loseSight1,//׷��
    loseSight2,
    backPatrol
}

public enum E_PatrolType
{
   loop,//ѭ��
   palindrome//����
}

public class Human : MonoBehaviour
{
    public E_HumanStatus status;//����״̬
    

    [Header("Ѳ��")]
    public List<Vector3> patrolPointS;//Ѳ�ߵ�
    public float patrolSpeed;//Ѳ���ٶ�
    public E_PatrolType patrolType = E_PatrolType.palindrome;
    protected int nextPatrolPoint; 

    //[Header("����")]
    //public float arrestSpeed;//ץ���ٶ�
    //public float outRange;//����ץ����Χ

    //[Header("��Ұ")]
    //public float sightR;//��Ұ��Χ�뾶
    //public float sightAngle = 60f;//��Ұ�Ƕ�
    //public bool isShowSight = false;//�Ƿ���ʾ��Ұ

    //[Header("������")]
    //public float warningSpeed;//����ֵ�����ٶ�
    //public GameObject warningStrip;//������
    //private Vector2 warningStripSize;//������size
    //private float warningPoint;//����ֵ
    ////private bool playerIsInRange = false;

    [Header("ѣ��")]
    public float dizzyTime = 8f;//ѣ��ʱ��
    //collider

    [Header("ת��")]
    public float turningSpeed = 50;

    protected CharacterController characterController;
    protected bool isGrowth = true;//�ж�Ѳ��·��
    protected bool isTurning = false;//�ж��Ƿ���ת��
    protected float startTurnAngle = 0;
    protected float delayTime = 0;
    protected GameObject player;

    //float angle_rightTan = 0;//��һ��tan
    //float angle_leftTan = 0;//�ڶ���tan

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
    /// �ı䷽��
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
    /// �����ƶ�
    /// </summary>
    /// <param name="target">Ŀ������</param>
    /// <param name="speed">�ƶ��ٶ�</param>
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
    /// ����ѣ��
    /// </summary>
    public void dizzy()
    {
        delayTime = dizzyTime;
    }

   

    //public void setPlayerIsInRange(bool bol)
    //{
    //    playerIsInRange = bol;
    //}
    #endregion//ͨ�õ�

    #region idle
    /// <summary>
    /// �ҵ���һ��Ѳ�ߵ�
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
    /// ��patrol״̬�µ��ƶ�
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
