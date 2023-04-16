using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    //private NavMeshAgent agent;//����
    private Animator anim;//���Ŷ���
    private CharacterStats characterStats;//����
    private GameObject attackTarget;//����Ŀ��
    private float lastAttackTime;//����cd
    private bool isDead;//�Ƿ�����
    private float stopDistance;//�������

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
    //��Ѫ����صĺ���
    //�����ಶ׽||������ײ��
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
    //����Ⱦ�ﶾ��������һ����Ⱦ����һ�������������
    //����
    void OnTriggerEnter(Collider other)
    {
        //TODO:jump��ػ�ûд

        //�������� ���tag
        //����bed ����tent ����С�㣺fish ������ż��toy ���峵��scooter

        //������Ծ����20%������ֵ+1��ʹ�ú���bed
        if (other.gameObject.tag.CompareTo("tent") == 0)
        {
            Debug.Log("�������ߣ�����");
            /*
             jump=jump*1.2;
            */
            characterStats.characterData.angerNum++;//����ֵ+1

            other.gameObject.tag = "bed";//�޸�tag
                                         //���ܻ�Ҫ�޸���ͼ��
        }
        //������Ծ����20%������ֵ+1��ʹ�ú���bed_disable
        if (other.gameObject.tag.CompareTo("bed") == 0)
        {
            Debug.Log("�������ߣ���");
            /*
             jump=jump*1.2;
             */
            characterStats.characterData.angerNum++;//����ֵ+1
            other.gameObject.tag = "bed_disable";
            //other.gameObject.SetActive(false);//��������
            Debug.Log("���ߴ�������");
        }
        //����С�㣺���õ��ߣ������Ⱦ�ۣ���Ծ��ˮƽ�ƶ�������30%��ʹ�ú���ʧ
        if (other.gameObject.tag.CompareTo("fish") == 0)
        {
            Debug.Log("�������ߣ�����С��");
            characterStats.characterData.dirtyNum = 0;//��Ⱦ������
            /*
             jump=jump*1.3;
             walk=walk*1.3;
             */
            other.gameObject.SetActive(false);//��������
            Debug.Log("���ߴ�����������ʧ");
        }
        //������ż�����õ��ߣ��ֵ����ಶ׽һ�Σ�ʹ�ú���ʧ
        if (other.gameObject.tag.CompareTo("toy") == 0)
        {
            Debug.Log("�������ߣ�������ż");
            characterStats.characterData.bloodNum++;//Ѫ��+1
            other.gameObject.SetActive(false);//��������
            Debug.Log("���ߴ�����������ʧ");
        }
        //TODO����������ô�ж��ǻ��л��ǲ�׽
        //���ࣺ�ٶ�����20 %
        //�񳵣��ٶ�����30 %��ʹ��һ�κ������ټ�����ʹ�����κ���ͣ�£��߳�������
        if (other.gameObject.tag.CompareTo("scooter") == 0)
        {
            Debug.Log("���峵");
        }
        //MonoBehaviour.OnTriggerEnter(Collider other)//�����봥����
        //MonoBehaviour.OnTriggerExit(Collider other)//���˳�������
        //MonoBehaviour.OnTriggerStay(Collider other)//������������
    }


}
