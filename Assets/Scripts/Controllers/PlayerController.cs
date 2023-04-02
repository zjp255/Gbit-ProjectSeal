using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStats characterStats;
    private GameObject attackTarget;
    private float lastAttackTime;
    private bool isDead;
    private float stopDistance;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
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
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }
    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
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
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        transform.LookAt(attackTarget.transform);
        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        agent.isStopped = true;

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
    //道具
    void OnTriggerEnter(Collider other)
    {
        //碰到物体 检测tag
        //床：bed 帐篷：tent 神奇小鱼：fish 海豹玩偶：toy 滑板车：scooter

        //帐篷：跳跃增幅20%，蓄力值+1，使用后变成bed
        if (other.gameObject.tag.CompareTo("tent") == 0)
        {
            Debug.Log("触发道具：帐篷");
            /*
             jump=jump*1.2;
             anger++;
             */
            other.gameObject.tag = "bed";//修改tag
                                         //可能还要修改贴图？
        }
        //床：跳跃增幅20%，蓄力值+1，使用后消失
        if (other.gameObject.tag.CompareTo("bed") == 0)
        {
            Debug.Log("触发道具：床");
            /*
             jump=jump*1.2;
             anger++;
             */
            other.gameObject.SetActive(false);//隐藏物体
        }
        //神奇小鱼：即用道具，清空污染槽，跳跃与水平移动均增幅30%，使用后消失
        if (other.gameObject.tag.CompareTo("fish") == 0)
        {
            Debug.Log("触发道具：神奇小鱼");
            /*
             dirty=0;
             jump=jump*1.3;
             walk=walk*1.3;
             */
            other.gameObject.SetActive(false);//隐藏物体
        }
        //海豹玩偶：即用道具，抵挡人类捕捉一次，使用后消失
        if (other.gameObject.tag.CompareTo("toy") == 0)
        {
            Debug.Log("触发道具：海豹玩偶");
            /*
             ifCaptured=0;//无法被捕捉
             */
            other.gameObject.SetActive(false);//隐藏物体
        }
        //滑板车：待定
        if (other.gameObject.tag.CompareTo("scooter") == 0)
        {
            Debug.Log("滑板车");
        }
        //MonoBehaviour.OnTriggerEnter(Collider other)//当进入触发器
        //MonoBehaviour.OnTriggerExit(Collider other)//当退出触发器
        //MonoBehaviour.OnTriggerStay(Collider other)//当逗留触发器
    }


}
