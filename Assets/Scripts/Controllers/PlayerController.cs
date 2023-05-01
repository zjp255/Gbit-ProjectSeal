using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Animator anim;//播放动画
    private CharacterStats characterStats;//属性
    private bool isDead;//是否死亡

    [Header("Gravity")]
    public float gravity = -9.81f;
    private Vector3 playerVelocity;

    [Header("OnGroundCheck")]
    public bool isGround;
    public float groundCheckRadius;         //检查半径
    public Transform checkGround;           
    public LayerMask groundPlayer;

    [Header("PlayerJumpControl")]
    private CharacterController controller;

    public float speed = 5f;                //ˮƽ�ƶ��ٶ�
    private float curJumpHeight;            //��ǰ��ߵ�߶�
    public float heightReduceFactor = 0.05f;//��ߵ�߶�˥��ϵ��
    public float jumpLowerLimit = 0.5f;     //�������͸߶�
    public float jumpOnHuman = 0.44f;       //�������
    public float jumpOnCar = 0.3f;          //�����
    public float jumpOnProps = 0.44f;        //�������
    private bool isJumpping = false;        //�Ƿ��ڵ���״̬
    private bool isSliding = false;         //�Ƿ��ڻ���״̬
    private bool isDirty = false;           //�Ƿ�����Ⱦ����
    private int inputFrames = 0;
    private float propsTime = 0f;           //��߳���ʱ��
    private float dirtyTime = 0f;           //����Ⱦ���е�ʱ��

    public GameObject angerUIPrefab;
    public Action GameOver;



    private void Awake()
    {
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
        Instantiate(angerUIPrefab);
    }

    private void Update()
    {
        CheckPlayerCondition();
        SwitchAnimation();
        SimulatePhysics();
        TryToJump();
        ApplyJump();
        ApplySlide();
        CheckProps(); 
    }


    private void CheckPlayerCondition()
    {
        if (isDead == false)
        {
            if (characterStats.BloodNum < 0)
            {
                isDead = true;
                Debug.Log("die because of being captured!!!!!");
            }
            if (characterStats.DirtyNum == Const.DIRTY_MAX)
            {
                isDead = true;
                Debug.Log("die because of being poisoned!!!!!");
            }
            if (isDead == true)
            {
                GameManager.Instance.NotifyObservers();
                Debug.Log("gameover");
                GameOver?.Invoke();
                Debug.Log("gameover11");
            }
        }   
    }

    private void SwitchAnimation()
    {
        if (isJumpping)
        {
            if (playerVelocity.y > 0)
            {
                if(anim.GetBool("isJumpUp") == false)
                {
                    anim.SetBool("isJumpUp", true);
                    anim.SetBool("isFallDown", false);
                    anim.SetBool("isSlide", false);
                    anim.SetTrigger("JumpUp");
                }
            }
            else
            {
                if(anim.GetBool("isFallDown") == false)
                {
                    anim.SetBool("isFallDown", true);
                    anim.SetBool("isJumpUp", false);
                    anim.SetBool("isSlide", false);
                    anim.SetTrigger("FallDown");
                }
            }
        }
        else
        {
            if (isSliding)
            {
                if (anim.GetBool("isSlide") == false)
                {
                    anim.SetBool("isSlide", true);
                    anim.SetBool("isJumpUp", false);
                    anim.SetBool("isFallDown", false);
                    anim.SetTrigger("Slide");
                }
            }
        }
    }

    private void SimulatePhysics()
    {
        playerVelocity.y += gravity * Time.deltaTime;
        isGround = Physics.CheckSphere(checkGround.position, groundCheckRadius, groundPlayer);
        if (isGround && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }
    
    private bool TryJumpWhenStill()
    {
        // 静止在地面上，准备起跳第一次
        return !isJumpping && !isSliding && isGround;
    }

    private bool TryJumpWhenSlide()
    {
        return !isJumpping && isSliding && isGround;
    }

    private void TryToJump()
    {
        if (TryJumpWhenStill() || TryJumpWhenSlide())
        {
            if (Input.GetButton("Jump"))
            {
                inputFrames++;
            }
            if (Input.GetButtonUp("Jump"))
            {
                if (inputFrames >= 100)
                {
                    JumpWithAllAnger();     // 长按超过100帧的计数全部释放
                }
                else
                {
                    if (TryJumpWhenStill())
                    {
                        JumpWithNoAnger();      // 第一次起跳不消耗蓄力条
                    }
                    else
                    {
                        JumpWithUnitAnger();    // 短按释放1格
                    }
                }
                inputFrames = 0;
                isJumpping = true;
                isSliding = false;
            }
        }
    }

    private void JumpWithAllAnger()
    {
        if (characterStats.AngerNum < Const.ANGER_UNIT)
        {
            Debug.Log("不足一格，不能释放");
        }
        else
        {
            int num = characterStats.AngerNum / Const.ANGER_UNIT;
            characterStats.AngerNum %= Const.ANGER_UNIT;
            curJumpHeight = Const.ANGER_HEIGHT[num];
        }
    }

    private void JumpWithUnitAnger()
    {
        if (characterStats.AngerNum < Const.ANGER_UNIT)
        {
            Debug.Log("不足一格，不能释放");
        }
        else
        {
            characterStats.AngerNum -= Const.ANGER_UNIT;
            curJumpHeight = Const.ANGER_HEIGHT[1];
        }
    }

    private void JumpWithNoAnger()
    {
        curJumpHeight = Const.ANGER_HEIGHT[1];
    }

    private void ApplyJump()
    {
        if (isJumpping && !isSliding)
        {
            if (!isGround)
            {
                // 滞空时，可以用方向键控制水平移动
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                Vector3 moveDir = new Vector3(vertical, 0, horizontal);
                Vector3 targetDir = Vector3.Slerp(transform.forward, moveDir, 2 * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(targetDir);
                controller.Move(moveDir * speed * Time.deltaTime);
            }
            else
            {
                if (curJumpHeight >= jumpLowerLimit)
                {
                    // 向上跳跃
                    playerVelocity.y = Mathf.Sqrt(-gravity * 2f * curJumpHeight);
                    // 每次落地，最大高度衰减5%
                    curJumpHeight = curJumpHeight * (1 - heightReduceFactor);
                }
                else
                {
                    //当跳跃高度小于设定的最小值，小海豹水平移动
                    isJumpping = false;
                    isSliding = true;
                }
            }
        }
    }

    private void ApplySlide()
    {
        if(!isJumpping && isSliding && isGround)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            if (horizontal != 0 || vertical != 0)
            {
                Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;
                Vector3 targetDir = Vector3.Slerp(transform.forward, moveDir, 2 * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(targetDir);
                controller.Move(moveDir * speed * Time.deltaTime);
            }
            else
            {
                controller.Move(transform.forward * speed * Time.deltaTime);
            }
        }
    }

    //和血量相关的函数
    //被人类捕捉||被汽车撞到
    void GetCaptured(Collider other)
    {
        if((other.gameObject.tag.CompareTo("enemy") == 0)|| (other.gameObject.tag.CompareTo("car") == 0))
        {
            characterStats.BloodNum--;
        }
    }
    void GetPoisioned(Collider other)
    {
        if (other.gameObject.tag.CompareTo("poison") == 0)
        {
            characterStats.DirtyNum++;
        }
    }

    //����Ⱦ�ﶾ��������һ����Ⱦ����һ������������
    //���
    private void CheckProps()

    {
        //��߳���ʱ��
        propsTime += Time.deltaTime;
        if (propsTime > 5)
        {
            speed = 5;
        }
        //��Ⱦ��
        if(isDirty)
        {
            dirtyTime += Time.deltaTime;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        //碰到物体 检测tag

        //帐篷：跳跃增幅20%，蓄力值+1，使用后变成bed
        if (other.gameObject.tag.CompareTo("tent") == 0)
        {
            Debug.Log("触发道具：帐篷");
            curJumpHeight = curJumpHeight * (1 + jumpOnProps);
            characterStats.AngerNum++;//蓄力值+1
            other.gameObject.SetActive(false);//隐藏物体
            other.gameObject.tag = "tent_disable";
        }
        //床：跳跃增幅20%，蓄力值+1，使用后变成bed_disable
        if (other.gameObject.tag.CompareTo("bed") == 0)
        {
            Debug.Log("触发道具：床");
            curJumpHeight = curJumpHeight * (1 + jumpOnProps);
            characterStats.AngerNum++;//蓄力值+1
            other.gameObject.tag = "bed_disable";
            Debug.Log("道具触发结束");
        }
        if (other.gameObject.tag.CompareTo("bed_disable") == 0)
        {
            Debug.Log("bed_disable");
        }
        //神奇小鱼：即用道具，清空污染槽，跳跃与水平移动均增幅30%，使用后消失
        if (other.gameObject.tag.CompareTo("fish") == 0)
        {
            Debug.Log("触发道具：神奇小鱼");
            curJumpHeight = curJumpHeight * (1 + jumpOnProps);
            characterStats.AngerNum++;//蓄力值+1
            propsTime = 0;
            speed = speed*(1+ jumpOnProps);
            characterStats.DirtyNum = 0;//污染条清零
            other.gameObject.SetActive(false);//隐藏物体
            Debug.Log("道具触发结束，消失");
        }
        //海豹玩偶：即用道具，抵挡人类捕捉一次，使用后消失
        if (other.gameObject.tag.CompareTo("toy") == 0)
        {
            Debug.Log("触发道具：海豹玩偶");
            characterStats.BloodNum++;//血条+1
            other.gameObject.SetActive(false);//隐藏物体
            Debug.Log("道具触发结束，消失");
        }
        //人类：速度增益20 %
        if (other.gameObject.tag.CompareTo("human_h1") == 0)
        {
            Debug.Log("触发道具：普通人类");
            curJumpHeight = curJumpHeight * (1 + jumpOnHuman);
            characterStats.AngerNum++;//蓄力值+1
        }
        if (other.gameObject.tag.CompareTo("human_h2") == 0)
        {
            Debug.Log("触发道具：狂暴人类");
            curJumpHeight = curJumpHeight * (1 + jumpOnHuman);
            characterStats.AngerNum+=3;//蓄力值+3
        }
        //篷车：速度增益30 %，使用一次后篷车移速减慢，使用两次后篷车停下，走出狂暴人类
        if (other.gameObject.tag.CompareTo("car") == 0)
        {
            Debug.Log("触发道具：篷车");
            curJumpHeight = curJumpHeight * (1 + jumpOnCar);
            characterStats.AngerNum+=4;//蓄力值+4
            Debug.Log("道具触发结束，消失");
        }
        
        if(other.gameObject.tag.CompareTo("dirty_pool") == 0)
        {
            Debug.Log("dirty_in");
            isDirty = true;
        }

        //冰洞：切换关卡
        if (other.gameObject.tag.CompareTo("iceHole") == 0)
        {
            if(other.gameObject.GetComponent<IceHole>().isUsed == false)
            {
                GameManager.Instance.curLevel += 1;
                other.gameObject.GetComponent<IceHole>().isUsed = true;
                if (GameManager.Instance.curLevel <= 4)
                {
                    SceneController.Instance.TransitionToLevel(GameManager.Instance.curLevel);
                }
            }
        }
    }
    void OnTriggerStay(Collider other)
    {
        if ((other.gameObject.tag.CompareTo("dirty_pool") == 0)&& isDirty)
        {
            characterStats.DirtyNum += (float)(0.001 *dirtyTime);
        }
    }
    void OnTriggerExit(Collider other)
    {
        isDirty = false;
        dirtyTime = 0;
        Debug.Log("dirty_out");
    }


}
