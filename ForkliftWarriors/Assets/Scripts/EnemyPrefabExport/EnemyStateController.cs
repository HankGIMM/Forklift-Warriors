using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    public EnemyState currentState;
    public EnemyState startingState;
    public Animator animator;

    private EnemyState previousState;

    void Start()
    {
        currentState = startingState;
    }

    void Update()
    {
        RunStateMachine();

        if (currentState == null)
        {
            Debug.LogError("Current State is null");
        }
        else if (currentState is PatrolState)
        {
            animator.SetBool("isPatrolling", true);
        }
        // else if (currentState is ChaseState)
        // {
        //     animator.SetBool("isChase", true);
        // }
        // else if (currentState is AttackState)
        // {
        //     animator.SetBool("isAttack", true);
        // }
        // else if (currentState is StunnedState)
        // {
        //     animator.SetBool("isStunned", true);
        // }
    }

    private void RunStateMachine()
    {
        // Takes the current state and runs its logic
        EnemyState nextState = currentState?.RunCurrentState();
        if (nextState != null && nextState != currentState)
        {
            SwitchToNextState(nextState);
        }
    }

    private void SwitchToNextState(EnemyState nextState)
    {
        // Track the previous state
        previousState = currentState;

        // Update the animator to set the previous state's bool to false
        if (previousState is PatrolState)
        {
            animator.SetBool("isPatrolling", false);
        }
        // else if (previousState is ChaseState)
        // {
        //     animator.SetBool("isChase", false);
        // }
        // else if (previousState is AttackState)
        // {
        //     animator.SetBool("isAttack", false);
        // }
        // else if (previousState is StunnedState)
        // {
        //     animator.SetBool("isStunned", false);
        // }

        // Switch to the next state
        currentState = nextState;
    }
}