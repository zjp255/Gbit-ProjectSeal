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
    public float speed = 5f;                //水平移动速度
    private float curJumpHeight;            //当前最高点高度
    public float heightReduceFactor = 0.05f;//最高点高度衰减系数
    public float jumpLowerLimit = 0.5f;     //弹跳的最低高度
    private bool isJumpping = false;        //是否处于弹跳状态
    private bool isSliding = false;         //是否处于滑行状态
    private int inputFrames = 0;
    public GameObject angerUIPrefab;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        GameManager.Instance.RigisterPlayer(characterStats);
        Instantiate(angerUIPrefab);
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    private void Update()
    {
        CheckPlayerCondition();
        SwitchAnimation();
        SimulatePhysics();
        TryToJump();
        ApplyJump();
        ApplySlide();
    }

    private void CheckPlayerCondition()
    {
        if (isDead == false)
        {
            isDead = (characterStats.CurrentHealth == 0);
        }
        if (isDead == true)
        {
            GameManager.Instance.NotifyObservers();
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
    //被污染物毒害：触碰一次污染槽涨一格，满三格后死亡
    //道具
    void OnTriggerEnter(Collider other)
    {
        //TODO:jump相关还没写

        //碰到物体 检测tag
        //床：bed 帐篷：tent 神奇小鱼：fish 海豹玩偶：toy 滑板车：scooter

        //帐篷：跳跃增幅20%，蓄力值+1，使用后变成bed
        if (other.gameObject.tag.CompareTo("tent") == 0)
        {
            Debug.Log("触发道具：帐篷");
            curJumpHeight = curJumpHeight * (1 + 0.8f);
            characterStats.characterData.angerNum++;//蓄力值+1
            other.gameObject.SetActive(false);//隐藏物体
            //other.gameObject.tag = "bed";
        }
        //床：跳跃增幅20%，蓄力值+1，使用后变成bed_disable
        if (other.gameObject.tag.CompareTo("bed") == 0)
        {
            Debug.Log("触发道具：床");
            curJumpHeight = curJumpHeight * (1 + 0.8f);
            characterStats.characterData.angerNum++;//蓄力值+1
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
            curJumpHeight = curJumpHeight * (1 + 0.9f);
            //TODO:水平
            characterStats.characterData.dirtyNum = 0;//污染条清零
            other.gameObject.SetActive(false);//隐藏物体
            Debug.Log("道具触发结束，消失");
        }
        //海豹玩偶：即用道具，抵挡人类捕捉一次，使用后消失
        if (other.gameObject.tag.CompareTo("toy") == 0)
        {
            Debug.Log("触发道具：海豹玩偶");
            characterStats.characterData.bloodNum++;//血条+1
            other.gameObject.SetActive(false);//隐藏物体
            Debug.Log("道具触发结束，消失");
        }
        //TODO：这两个怎么判定是击中还是捕捉
        //人类：速度增益20 %
        if (other.gameObject.tag.CompareTo("human") == 0)
        {
            Debug.Log("触发道具：人类");
            curJumpHeight = curJumpHeight * (1 + 0.2f);
            Debug.Log("道具触发结束，消失");
        }
        //篷车：速度增益30 %，使用一次后篷车移速减慢，使用两次后篷车停下，走出狂暴人类
        if (other.gameObject.tag.CompareTo("car") == 0)
        {
            Debug.Log("触发道具：篷车");
            curJumpHeight = curJumpHeight * (1 + 0.3f);
            Debug.Log("道具触发结束，消失");
        }
        //MonoBehaviour.OnTriggerEnter(Collider other)//当进入触发器
        //MonoBehaviour.OnTriggerExit(Collider other)//当退出触发器
        //MonoBehaviour.OnTriggerStay(Collider other)//当逗留触发器
    }


}
