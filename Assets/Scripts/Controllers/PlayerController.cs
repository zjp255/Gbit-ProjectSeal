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
    public bool isBad;
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
    private bool isPlaying = true;
    private int airTime = 0;
    private Vector3 airPos;
    public event Action<string> OnLead;
    public event Action OnLeadClosed;
    public Dictionary<string, bool> propsMapBool = new Dictionary<string, bool> {
        { "back",false },
        {"wasd",false},
        {"low",false },
        {"fish",false },
        {"toy",false}
    };

    public Dictionary<string, string> propsMap = new Dictionary<string, string> {
        { "back","危险的人类堵住了海豹们赖以生存的冰洞，作为海豹一族最勇敢的豹豹，你将踏上疏通冰洞的旅途" },
        {"wasd","听说你是《豹肚弹弹》比赛的冠军，你可以通过弹跳跨越障碍，躲避危险的人类,通过wasd你可以前后左右移动" },
        {"low","根据...能量%#￥%定律...你没法一直这样跳下去，看看你的蓄力槽，按下？？键，你能再次一飞冲天！" },
        {"fish","嘿，看你捡到了什么？神奇鱼鱼！吃了这东西，你将跳的更高，污染值也会下降”（（有点尬，这么说" },
        {"toy","嘿，这是什么！一个mini版的你。它可以让你抵挡一次暴怒人类抓捕，毕竟他们傻乎乎的分不清哪个是玩偶" }
    };

    public GameObject angerUIPrefab;
    public Action GameOver;





    private void Awake()
    {
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        controller = GetComponent<CharacterController>();
        airPos = checkGround.position;
    }

    private void OnEnable()
    {
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
        Instantiate(angerUIPrefab);
        if (GameManager.Instance.curLevel == 1)
        {
            if (!propsMapBool["back"])
            {
                OnLead?.Invoke(propsMap["back"]);
                propsMapBool["back"] = true;
            }       
        }
    }

    private void Update()
    {
        //FIXME:测试代码 记得删除
        if (Input.GetKeyDown(KeyCode.P))
        {
            isPlaying = !isPlaying;
        }
        if (isPlaying)
        {
            CheckPlayerCondition();
            SwitchAnimation();
            SimulatePhysics();
            TryToJump();
            ApplyJump();
            ApplySlide();
            CheckProps();
        }
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
                //GameOver?.Invoke();
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
        if (isGround == false)
        {
            RaycastHit hit;
            Vector3 origin = transform.position;
            Vector3 direction = transform.TransformDirection(new Vector3(0, -1, 0));
            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity, groundPlayer))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, -1, 0)));
                if (Vector3.Distance(hit.transform.position, checkGround.position) > 0)
                {
                    
                    if (airPos == checkGround.position && playerVelocity.y!=0)
                    {
                        airTime++;
                        Debug.Log(playerVelocity.y);
                    }
                    else
                    {
                        airPos = checkGround.position;
                        airTime = 0;
                        isGround = false;
                    }
                    // 异常卡死情况
                    if (airTime > 50)
                    {
                        isGround = true;
                        airTime = 0;
                    }
                }
            }
        }
        if ((isGround) && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }
    
    private bool TryJumpWhenStill()
    {
        // 静止在地面上，准备起跳第一次
        return !isJumpping && !isSliding && (isGround);
    }

    private bool TryJumpWhenSlide()
    {
        return !isJumpping && isSliding && (isGround);
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
            Vector3 moveDir = new Vector3(vertical, 0, horizontal).normalized;
            Vector3 targetDir = Vector3.Slerp(transform.forward, moveDir, 2 * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(targetDir);
            controller.Move(moveDir * speed * Time.deltaTime);
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
        //ui
        if (Input.GetKeyDown(KeyCode.C))
        {
            OnLeadClosed?.Invoke();
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
            if (!propsMapBool["fish"])
            {
                Debug.Log("1");
                OnLead?.Invoke(propsMap["fish"]);
                Debug.Log("2");
                propsMapBool["fish"] = true;
                Debug.Log("3");
            }
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
