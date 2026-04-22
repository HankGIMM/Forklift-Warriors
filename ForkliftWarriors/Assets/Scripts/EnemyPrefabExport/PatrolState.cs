using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : EnemyState
{
    public List<Transform> waypoints;
    public Transform currentDestination;
    public int currentWaypointIndex = -1;
    //  public ChaseState chaseState;
    // public EnemySight enemySight;
    //public StunnedState stunnedState;
    public NavMeshAgent navMeshAgent;
    //    public EnemyHealth enemyHealth;
    private bool isWaiting = false;
    [SerializeField] private Animator animator;
    public AudioSource walkSound;

    private void Start()
    {
        if (waypoints.Count > 0)
        {
            MoveToNextWaypoint();
            walkSound.Play();
        }
        else
        {
            Debug.LogError("No waypoints set for PatrolState.");
        }
    }

    public override EnemyState RunCurrentState()
    {
        // if (enemyHealth.stunned)
        // {
        //     animator.SetBool("isPatrolling", false);
        //     animator.SetBool("isIdle", false);
        //     animator.SetBool("isStunned", true);
        //     return stunnedState;
        // }

        if (isWaiting)
        {
            return this; // Stay in the current state while waiting
        }

        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !isWaiting)
        {
            StartCoroutine(WaitAtWaypoint());
        }

        // if (enemySight.canSeePlayer)
        // {
        //     animator.SetBool("isPatrolling", false);
        //     animator.SetBool("isIdle", false);
        //     animator.SetBool("isChase", true);
        //     return chaseState;
        // }
        // else
        // {
        //     return this;
        // }
        return this;
    }

    private IEnumerator WaitAtWaypoint()
    {
        StartCoroutine(AudioFade());
        isWaiting = true;
        animator.SetBool("isPatrolling", false);
        animator.SetBool("isIdle", true);
        yield return new WaitForSeconds(5.0f);
        isWaiting = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isPatrolling", true);
        MoveToNextWaypoint();
        walkSound.volume = 1f;
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints.Count == 0)
        {
            return;
        }

        int nextWaypointIndex;
        do
        {
            nextWaypointIndex = Random.Range(0, waypoints.Count);
        } while (nextWaypointIndex == currentWaypointIndex);

        currentWaypointIndex = nextWaypointIndex;
        currentDestination = waypoints[currentWaypointIndex];
        navMeshAgent.SetDestination(currentDestination.position);
    }
    private IEnumerator AudioFade()
    {
        while (walkSound.volume > 0)
        {
            walkSound.volume -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}