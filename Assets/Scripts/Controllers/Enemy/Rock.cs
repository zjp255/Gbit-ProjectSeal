using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates { HitPlayer,HitEnemy,HitNothing}
    private Rigidbody rb;
    public RockStates rockState;

    [Header("Basic Settings")]
    public float force;
    public int damage;
    public GameObject target;
    private Vector3 direction;
    public GameObject breakEffect;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rockState = RockStates.HitPlayer;
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        //通过debug确定
        if (rb.velocity.sqrMagnitude < 1f)
        {
            rockState = RockStates.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        //极限情况下，生成石头后拉脱战，此时仍然去寻找一下玩家
        if (target == null)
            target = FindObjectOfType<PlayerController>().gameObject;
        //因为抛射有高度，因此防止直接砸到地上，所以添加了一个向上的分量
        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (rockState)
        {
            case RockStates.HitPlayer:
                if (other.gameObject.CompareTag("Player"))
                {
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;

                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());

                    rockState = RockStates.HitNothing;
                }
                break;
            case RockStates.HitEnemy:
                if (other.gameObject.GetComponent<Golem>())
                {
                    var otherStats = other.gameObject.GetComponent<CharacterStats>();
                    otherStats.TakeDamage(damage, otherStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
        }
    }
}
