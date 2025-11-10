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
    public float fieldOfViewAngle = 110f; // Realistic FOV for detection
    
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float waitTime = 2f;
    
    [Header("Steering Behaviors")]
    public float accelerationSpeed = 3f; // Smooth acceleration
    public float decelerationSpeed = 2f; // Smooth deceleration
    public float rotationSpeed = 120f; // Smooth rotation
    public float arrivalDistance = 2f; // Distance to slow down
    public float stoppingDistance = 0.5f; // Final stopping distance
    
    [Header("Visual Feedback")]
    public Material idleMaterial;
    public Material patrolMaterial;
    public Material chaseMaterial;
    
    [Header("Debug Visualization")]
    public bool showDebugGizmos = true;
    
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private Renderer enemyRenderer;
    private int currentPatrolIndex = 0;
    private float currentSpeed = 0f; // For smooth acceleration/deceleration
    
    // States
    private IdleState idleState;
    private PatrolState patrolState;
    private ChaseState chaseState;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponent<Renderer>();
        
        // Configure NavMesh Agent for smooth movement
        agent.acceleration = accelerationSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.stoppingDistance = stoppingDistance;
        
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
        ApplySmoothMovement();
    }
    
    // Smooth acceleration and deceleration for believable movement
    private void ApplySmoothMovement()
    {
        float targetSpeed = agent.speed;
        
        if (agent.velocity.magnitude > 0.1f)
        {
            // Accelerate smoothly
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * accelerationSpeed);
        }
        else
        {
            // Decelerate smoothly
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * decelerationSpeed);
        }
    }
    
    public float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }
    
    // Enhanced line-of-sight check with field of view
    public bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        // Check if player is within field of view angle
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > fieldOfViewAngle * 0.5f)
            return false;
        
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
        {
            if (hit.transform == player)
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Check if path to destination is valid
    public bool IsPathValid(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        return agent.CalculatePath(destination, path) && path.status == NavMeshPathStatus.PathComplete;
    }
    
    // Smooth seeking behavior - move toward target with arrival
    public void SeekTarget(Vector3 targetPosition)
    {
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(targetPosition);
            
            // Apply arrival behavior - slow down as we approach
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            if (distanceToTarget < arrivalDistance)
            {
                float slowdownFactor = distanceToTarget / arrivalDistance;
                agent.speed = Mathf.Lerp(0f, agent.speed, slowdownFactor);
            }
        }
    }
    
    // Flee behavior - move away from target
    public void FleeFrom(Vector3 dangerPosition, float fleeDistance)
    {
        Vector3 fleeDirection = (transform.position - dangerPosition).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;
        
        if (IsPathValid(fleeTarget))
        {
            agent.SetDestination(fleeTarget);
        }
    }
    
    // Get current state name for debugging
    public string GetCurrentStateName()
    {
        return stateMachine.GetCurrentState()?.GetType().Name ?? "None";
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
    public float GetArrivalDistance() => arrivalDistance;
    
    public void SetMaterial(Material mat)
    {
        if (enemyRenderer != null && mat != null)
            enemyRenderer.material = mat;
    }
    
    public Material GetIdleMaterial() => idleMaterial;
    public Material GetPatrolMaterial() => patrolMaterial;
    public Material GetChaseMaterial() => chaseMaterial;
    
    // Debug visualization for detection range and FOV
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw lose player range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, losePlayerRange);
        
        // Draw field of view
        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * forward * detectionRange;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // Draw line to player if detected
        if (player != null && CanSeePlayer())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);
        }
        
        // Draw current path
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Vector3 previousCorner = transform.position;
            foreach (Vector3 corner in agent.path.corners)
            {
                Gizmos.DrawLine(previousCorner, corner);
                Gizmos.DrawSphere(corner, 0.2f);
                previousCorner = corner;
            }
        }
    }
}