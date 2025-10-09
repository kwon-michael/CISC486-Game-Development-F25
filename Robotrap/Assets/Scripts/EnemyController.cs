using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("FSM Settings")]
    public Transform player;
    public Transform[] patrolPoints;
    
    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public float losePlayerRange = 12f;
    
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float waitTime = 2f;
    
    [Header("Visual Feedback")]
    public Material idleMaterial;
    public Material patrolMaterial;
    public Material chaseMaterial;
    
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private Renderer enemyRenderer;
    private int currentPatrolIndex = 0;
    
    // States
    private IdleState idleState;
    private PatrolState patrolState;
    private ChaseState chaseState;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponent<Renderer>();
        
        // Find player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Initialize state machine
        stateMachine = new StateMachine();
        
        // Create states
        idleState = new IdleState(this);
        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        
        // Start with idle state
        stateMachine.ChangeState(idleState);
    }
    
    void Update()
    {
        stateMachine.Update();
        
        // Debug information
        Debug.Log($"Enemy State: {stateMachine.GetCurrentState().GetType().Name}");
    }
    
    public float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }
    
    public bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            return hit.transform == player;
        }
        
        return false;
    }
    
    public void ChangeToIdle() => stateMachine.ChangeState(idleState);
    public void ChangeToPatrol() => stateMachine.ChangeState(patrolState);
    public void ChangeToChase() => stateMachine.ChangeState(chaseState);
    
    public NavMeshAgent GetAgent() => agent;
    public Transform GetPlayer() => player;
    public Transform[] GetPatrolPoints() => patrolPoints;
    public int GetCurrentPatrolIndex() => currentPatrolIndex;
    public void SetCurrentPatrolIndex(int index) => currentPatrolIndex = index;
    public float GetDetectionRange() => detectionRange;
    public float GetLosePlayerRange() => losePlayerRange;
    public float GetPatrolSpeed() => patrolSpeed;
    public float GetChaseSpeed() => chaseSpeed;
    public float GetWaitTime() => waitTime;
    
    public void SetMaterial(Material mat)
    {
        if (enemyRenderer != null && mat != null)
            enemyRenderer.material = mat;
    }
    
    public Material GetIdleMaterial() => idleMaterial;
    public Material GetPatrolMaterial() => patrolMaterial;
    public Material GetChaseMaterial() => chaseMaterial;
}