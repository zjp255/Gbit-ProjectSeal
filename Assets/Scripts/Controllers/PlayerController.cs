using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    //private NavMeshAgent agent;//导航
    private Animator anim;//播放动画
    private CharacterStats characterStats;//属性
    private GameObject attackTarget;//攻击目标
    private float lastAttackTime;//攻击cd
    private bool isDead;//是否死亡
    private float stopDistance;//最近距离

    private float moveSpeed = 5f;
    private float rotateSpeed = 5f;
    private float jumpSpeed = 8f;
    private bool isOnGround = true;
    private Vector3 moveAmount;
    Rigidbody rb;

    private void Awake()
    {
        //agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        
        //stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    private void OnDisable()
    {
        if (!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    private void Update()
    {
        isDead = (characterStats.CurrentHealth == 0);
        if (isDead)
            GameManager.Instance.NotifyObservers();

        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }
    public void SwitchAnimation()
    {
        //anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }


    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead) return;
        //agent.stoppingDistance = stopDistance;
        //agent.isStopped = false;
        //agent.destination = target;
    }
    public void EventAttack(GameObject target)
    {
        if (isDead) return;
        if(target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }
    IEnumerator MoveToAttackTarget()
    {
        //agent.isStopped = false;
        //agent.stoppingDistance = characterStats.attackData.attackRange;
        transform.LookAt(attackTarget.transform);
        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
        {
            //agent.destination = attackTarget.transform.position;
            yield return null;
        }
        //agent.isStopped = true;

        if(lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //reset attack CD
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    //Animation Event
    void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockState == Rock.RockStates.HitNothing)
            {
                attackTarget.GetComponent<Rock>().rockState = Rock.RockStates.HitEnemy;

                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20,ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }
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
