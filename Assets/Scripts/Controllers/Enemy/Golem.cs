using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 25;
    public GameObject rockPrefab;
    public Transform handPos;

    //Animation Event
    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            //��ȡ��ָ����ҵ�����
            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;

            //�����ܻ�����
            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            targetStats.TakeDamage(characterStats, targetStats);
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }

    //Animation Event
    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            //�����������ǳ�ʼ�Ƕȣ�����ѡ��ά��ԭ���ĽǶ�
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;

        }
    }
}