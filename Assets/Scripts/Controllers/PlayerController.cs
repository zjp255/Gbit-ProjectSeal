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
    private float gravity = -9.81f;
    private Vector3 playerVelocity;
    [Header("OnGroundCheck")]
    public bool isGround;
    public float groundCheckRadius;
    public Transform checkGround;
    public LayerMask groundPlayer;
    [Header("PlayerControl")]
    private CharacterController controller;
    public float speed = 5f;
    public float jumpHeight;

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
    }

    private void Update()
    {
        CheckPlayerCondition();
        SwitchAnimation();
        PlayerPhysics();
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;
        controller.Move(moveDir * speed * Time.deltaTime);
        if (Input.GetButtonDown("Jump") && isGround)
        {
            playerVelocity.y = Mathf.Sqrt(-gravity * 2f * jumpHeight);
        }
    }

    private void PlayerPhysics()
    {
        playerVelocity.y += gravity * Time.deltaTime;
        isGround = Physics.CheckSphere(checkGround.position, groundCheckRadius, groundPlayer);
        if (isGround && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
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
        //anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    //和血量相关的函数
    //被人类捕捉||被汽车撞到
    void GetCaptured(Collider other)
    {
        if((other.gameObject.tag.CompareTo("enemy") == 0)|| (other.gameObject.tag.CompareTo("car") == 0))
        {
            characterStats.bloodNum--;
        }
    }
    void GetPoisioned(Collider other)
    {
        if (other.gameObject.tag.CompareTo("poison") == 0)
        {
            characterStats.dirtyNum++;
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
            /*
             jump=jump*1.2;
            */
            characterStats.characterData.angerNum++;//蓄力值+1

            other.gameObject.tag = "bed";//修改tag
                                         //可能还要修改贴图？
        }
        //床：跳跃增幅20%，蓄力值+1，使用后变成bed_disable
        if (other.gameObject.tag.CompareTo("bed") == 0)
        {
            Debug.Log("触发道具：床");
            /*
             jump=jump*1.2;
             */
            characterStats.characterData.angerNum++;//蓄力值+1
            other.gameObject.tag = "bed_disable";
            //other.gameObject.SetActive(false);//隐藏物体
            Debug.Log("道具触发结束");
        }
        //神奇小鱼：即用道具，清空污染槽，跳跃与水平移动均增幅30%，使用后消失
        if (other.gameObject.tag.CompareTo("fish") == 0)
        {
            Debug.Log("触发道具：神奇小鱼");
            characterStats.characterData.dirtyNum = 0;//污染条清零
            /*
             jump=jump*1.3;
             walk=walk*1.3;
             */
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
        //篷车：速度增益30 %，使用一次后篷车移速减慢，使用两次后篷车停下，走出狂暴人类
        if (other.gameObject.tag.CompareTo("scooter") == 0)
        {
            Debug.Log("滑板车");
        }
        //MonoBehaviour.OnTriggerEnter(Collider other)//当进入触发器
        //MonoBehaviour.OnTriggerExit(Collider other)//当退出触发器
        //MonoBehaviour.OnTriggerStay(Collider other)//当逗留触发器
    }


}
