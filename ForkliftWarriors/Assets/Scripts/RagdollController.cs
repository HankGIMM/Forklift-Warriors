using System;
using UnityEngine;
using UnityEngine.AI;

public class RagdollController : MonoBehaviour
{
    
    [Header("Testing")]
    public bool testRagdoll = false;
    private bool lastTestRagdoll = false;

    [Header("References")]
    public Animator animator;

    [Header("Settings")]
    public float collisionForceThreshold = 5f; 
    public float ragdollRecoveryTime = 3f;

    [Header("AI Components")]
    public NavMeshAgent navMeshAgent;
    public PatrolState patrolState;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private bool isRagdoll = false;
    private float ragdollTimer = 0f;
    public Transform hipsBone;

    void Awake()
    {
        
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        
        SetRagdoll(false);
    }

    void Update()
    {
        
        if (testRagdoll != lastTestRagdoll)
        {
            lastTestRagdoll = testRagdoll;
            SetRagdoll(testRagdoll);
            ragdollTimer = 0f;
        }

        if (isRagdoll && ragdollRecoveryTime > 0f)
        {
            ragdollTimer += Time.deltaTime;
            if (ragdollTimer >= ragdollRecoveryTime)
            {
                SetRagdoll(false);
                testRagdoll = false;
                lastTestRagdoll = false;
            }
        }
    }

    
    void OnTriggerEnter(Collider col)
    {
        if (!isRagdoll)
        {
            Rigidbody otherRb = col.attachedRigidbody;
            if (otherRb != null)
            {
                ActivateRagdoll(otherRb.linearVelocity * otherRb.mass);
            }
        }
    }

    private void ActivateRagdoll(object value)
    {
        throw new NotImplementedException();
    }

    public void ActivateRagdoll(Vector3 force = default)
    {
        SetRagdoll(true);
        ragdollTimer = 0f;

       
        if (force != Vector3.zero && ragdollRigidbodies.Length > 0)
        {
            ragdollRigidbodies[0].AddForce(force, ForceMode.Impulse);
            ForkliftEvents.RaiseWorkerHit();
        }
    }

    void SetRagdoll(bool state)
    {
        
        if (!state && hipsBone != null)
        {
            transform.position = hipsBone.position;
        }

        isRagdoll = state;

        animator.enabled = !state;

        if (navMeshAgent != null)
            navMeshAgent.enabled = !state;

        if (patrolState != null)
            patrolState.enabled = !state;

        foreach (Rigidbody rb in ragdollRigidbodies)
            rb.isKinematic = !state;

        foreach (Collider col in ragdollColliders)
        {
            if (col == GetComponent<Collider>()) continue;
            col.enabled = state;
        }
    }
    
}