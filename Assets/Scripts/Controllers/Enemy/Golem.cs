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

            //获取由指向玩家的向量
            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;

            //击飞受击对象
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
            //第三个参数是初始角度，我们选择维持原来的角度
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;

        }
    }
}