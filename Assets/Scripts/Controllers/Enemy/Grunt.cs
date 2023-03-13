using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce = 10;

    public void KickOff()
    {
        if(attackTarget != null)
        {
            transform.LookAt(attackTarget.transform.position);
            
            //获取由当前兽人指向玩家的向量
            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            //击飞受击对象
            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }
}
