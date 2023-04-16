using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Animator anim;//���Ŷ���
    private CharacterStats characterStats;//����
    private bool isDead;//�Ƿ�����

    [Header("Gravity")]
    public float gravity = -9.81f;
    private Vector3 playerVelocity;
    [Header("OnGroundCheck")]
    public bool isGround;
    public float groundCheckRadius;
    public Transform checkGround;
    public LayerMask groundPlayer;
    [Header("PlayerControl")]
    private CharacterController controller;
    public float speed = 5f;        //ˮƽ�ƶ��ٶ�
    public float jumpHeight;        //��ߵ�߶�
    private float curJumpHeight;    //��ǰ��ߵ�߶�
    public float heightReduceFactor;//��ߵ�߶�˥��ϵ��
    public float jumpLowerLimit;    //��������͸߶�
    private bool isJumpping = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        controller = GetComponent<CharacterController>();
        curJumpHeight = jumpHeight;
    }

    private void OnEnable()
    {
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    // ��û������ת���������/////////////////////////////////
    private void Update()
    {
        CheckPlayerCondition();
        SwitchAnimation();
        SimulatePhysics();
        // ��ֹ״̬���ڵ���������
        if (!isJumpping && Input.GetButtonDown("Jump") && isGround)
        {
            isJumpping = true;
        }
        // ����״̬
        if (isJumpping)
        {
            if (!isGround)
            {
                // �Ϳ�ʱ�������÷��������ˮƽ�ƶ�
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;
                controller.Move(moveDir * speed * Time.deltaTime);
            }
            else
            {
                if(curJumpHeight >= jumpLowerLimit)
                {
                    // ������Ծ
                    playerVelocity.y = Mathf.Sqrt(-gravity * 2f * curJumpHeight);
                    //ÿ����أ����߶�˥��5%
                    curJumpHeight = curJumpHeight * (1 - heightReduceFactor);
                }
                else
                {
                    //����Ծ�߶�С���趨����Сֵ��С����ˮƽ�ƶ�
                    controller.Move(transform.forward * speed * Time.deltaTime);
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
