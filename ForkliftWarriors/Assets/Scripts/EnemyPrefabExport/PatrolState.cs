using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : EnemyState
{
    [Header("Waypoints")]
    public List<Transform> waypoints;
    public Transform currentDestination;
    public int currentWaypointIndex = -1;

    [Header("References")]
    public NavMeshAgent navMeshAgent;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float waypointWaitTime = 5f;

    private bool isWaiting = false;

    private void Start()
    {
        if (waypoints.Count > 0)
        {
            MoveToNextWaypoint();
        }
        else
        {
            Debug.LogError($"[PatrolState] No waypoints assigned on {gameObject.name}.");
        }
    }

    public override EnemyState RunCurrentState()
    {
        
        if (isWaiting)
            return this;

        
        if (!navMeshAgent.isOnNavMesh)
            return this;

       
        if (!navMeshAgent.hasPath)
        {
            MoveToNextWaypoint();
            return this;
        }

        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(WaitAtWaypoint());
        }

       

        return this;
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        if (animator != null) SetAnimatorState("isIdle");

       

        yield return new WaitForSeconds(waypointWaitTime);

        isWaiting = false;
        if (animator != null) SetAnimatorState("isPatrolling");

     
        MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("[PatrolState] MoveToNextWaypoint called with no waypoints.");
            return;
        }

        if (!navMeshAgent.isOnNavMesh)
            return;

        int nextIndex;
        int attempts = 0;

        do
        {
            nextIndex = Random.Range(0, waypoints.Count);
            attempts++;
        }
        
        while (nextIndex == currentWaypointIndex && waypoints.Count > 1 && attempts < 10);

        currentWaypointIndex = nextIndex;
        currentDestination = waypoints[currentWaypointIndex];
        navMeshAgent.SetDestination(currentDestination.position);
    }

    
    private void SetAnimatorState(string state)
    {
        animator.SetBool("isPatrolling", state == "isPatrolling");
        animator.SetBool("isIdle",       state == "isIdle");
        animator.SetBool("isChase",      state == "isChase");
        animator.SetBool("isStunned",    state == "isStunned");
    }

}