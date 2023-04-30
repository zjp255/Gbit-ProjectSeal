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
    public float groundCheckRadius;         //���뾶
    public Transform checkGround;           
    public LayerMask groundPlayer;

    [Header("PlayerJumpControl")]
    private CharacterController controller;
    public float speed = 5f;                //ˮƽ�ƶ��ٶ�
    private float curJumpHeight;            //��ǰ��ߵ�߶�
    public float heightReduceFactor = 0.05f;//��ߵ�߶�˥��ϵ��
    public float jumpLowerLimit = 0.5f;     //��������͸߶�
    private bool isJumpping = false;        //�Ƿ��ڵ���״̬
    private bool isSliding = false;         //�Ƿ��ڻ���״̬
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
        // ��ֹ�ڵ����ϣ�׼��������һ��
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
                    JumpWithAllAnger();     // ��������100֡�ļ���ȫ���ͷ�
                }
                else
                {
                    if (TryJumpWhenStill())
                    {
                        JumpWithNoAnger();      // ��һ������������������
                    }
                    else
                    {
                        JumpWithUnitAnger();    // �̰��ͷ�1��
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
            Debug.Log("����һ�񣬲����ͷ�");
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
            Debug.Log("����һ�񣬲����ͷ�");
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
                // �Ϳ�ʱ�������÷��������ˮƽ�ƶ�
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
                    // ������Ծ
                    playerVelocity.y = Mathf.Sqrt(-gravity * 2f * curJumpHeight);
                    // ÿ����أ����߶�˥��5%
                    curJumpHeight = curJumpHeight * (1 - heightReduceFactor);
                }
                else
                {
                    //����Ծ�߶�С���趨����Сֵ��С����ˮƽ�ƶ�
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

    //��Ѫ����صĺ���
    //�����ಶ׽||������ײ��
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
            curJumpHeight = curJumpHeight * (1 + 0.8f);
            characterStats.characterData.angerNum++;//����ֵ+1
            other.gameObject.SetActive(false);//��������
            //other.gameObject.tag = "bed";
        }
        //������Ծ����20%������ֵ+1��ʹ�ú���bed_disable
        if (other.gameObject.tag.CompareTo("bed") == 0)
        {
            Debug.Log("�������ߣ���");
            curJumpHeight = curJumpHeight * (1 + 0.8f);
            characterStats.characterData.angerNum++;//����ֵ+1
            other.gameObject.tag = "bed_disable";
            Debug.Log("���ߴ�������");
        }
        if (other.gameObject.tag.CompareTo("bed_disable") == 0)
        {
            Debug.Log("bed_disable");
        }
        //����С�㣺���õ��ߣ������Ⱦ�ۣ���Ծ��ˮƽ�ƶ�������30%��ʹ�ú���ʧ
        if (other.gameObject.tag.CompareTo("fish") == 0)
        {
            Debug.Log("�������ߣ�����С��");
            curJumpHeight = curJumpHeight * (1 + 0.9f);
            //TODO:ˮƽ
            characterStats.characterData.dirtyNum = 0;//��Ⱦ������
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
        if (other.gameObject.tag.CompareTo("human") == 0)
        {
            Debug.Log("�������ߣ�����");
            curJumpHeight = curJumpHeight * (1 + 0.2f);
            Debug.Log("���ߴ�����������ʧ");
        }
        //�񳵣��ٶ�����30 %��ʹ��һ�κ������ټ�����ʹ�����κ���ͣ�£��߳�������
        if (other.gameObject.tag.CompareTo("car") == 0)
        {
            Debug.Log("�������ߣ���");
            curJumpHeight = curJumpHeight * (1 + 0.3f);
            Debug.Log("���ߴ�����������ʧ");
        }
        //MonoBehaviour.OnTriggerEnter(Collider other)//�����봥����
        //MonoBehaviour.OnTriggerExit(Collider other)//���˳�������
        //MonoBehaviour.OnTriggerStay(Collider other)//������������
    }


}
