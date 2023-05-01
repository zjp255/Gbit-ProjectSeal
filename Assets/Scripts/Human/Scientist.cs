using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scientist : Human
{
    [Header("Animation")]
    public Animator animator;

    void Start()
    {
        status = E_HumanStatus.idle;
        nextPatrolPoint = 0;
        characterController = transform.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (delayTime == 0)
        {
            switch (status)
            {
                case E_HumanStatus.idle:
                    animator.SetBool("isRun", false);
                    getNextPatrolPoint();
                    break;
                case E_HumanStatus.patrol:
                    animator.SetBool("isRun", true);
                    moveAtPatrol();
                    break;
            }
        }
        else
        {
            animator.SetBool("isDizzy", true);
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

            // 判断动画是否已经播放完毕
            if (currentState.normalizedTime <= 0.55f && currentState.IsName("Hit To Head"))
            {
                characterController.SimpleMove(transform.forward * patrolSpeed);
            }
            else
            {
                delayTime -= Time.deltaTime;
                if (delayTime < 0)
                {
                    delayTime = 0;
                   
                    animator.SetBool("isDizzy", false);
                }
            }
        }
    
    }
}
