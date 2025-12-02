using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkedEnemy : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer enemyRenderer;
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float loseRange = 18f;
    [SerializeField] private float fieldOfView = 120f;
    
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointDistance = 1.5f;
    [SerializeField] private float waypointWaitTime = 2f;
    
    [Header("Materials")]
    [SerializeField] private Material idleMaterial;
    [SerializeField] private Material chaseMaterial;
    
    private NetworkVariable<EnemyState> state = new NetworkVariable<EnemyState>(
        EnemyState.Patrol,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private Transform target;
    private int waypointIndex = 0;
    private float waitTimer = 0f;
    
    private enum EnemyState { Idle, Patrol, Chase }
    
    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (enemyRenderer == null) enemyRenderer = GetComponent<Renderer>();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        state.OnValueChanged += OnStateChanged;
        
        if (IsServer)
        {
            InvokeRepeating(nameof(UpdateAI), 0f, 0.5f);
        }
        
        UpdateMaterial(state.Value);
    }
    
    private void UpdateAI()
    {
        if (!IsServer) return;
        
        FindNearestPlayer();
        
        switch (state.Value)
        {
            case EnemyState.Idle:
                IdleUpdate();
                break;
            case EnemyState.Patrol:
                PatrolUpdate();
                break;
            case EnemyState.Chase:
                ChaseUpdate();
                break;
        }
    }
    
    private void FindNearestPlayer()
    {
        var players = FindObjectsOfType<NetworkedPlayer>();
        float nearest = Mathf.Infinity;
        Transform nearestPlayer = null;
        
        foreach (var player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < detectionRange && CanSee(player.transform))
            {
                if (dist < nearest)
                {
                    nearest = dist;
                    nearestPlayer = player.transform;
                }
            }
        }
        
        if (nearestPlayer != null)
        {
            target = nearestPlayer;
            if (state.Value != EnemyState.Chase)
            {
                state.Value = EnemyState.Chase;
            }
        }
        else if (state.Value == EnemyState.Chase)
        {
            if (target != null)
            {
                float dist = Vector3.Distance(transform.position, target.position);
                if (dist > loseRange || !CanSee(target))
                {
                    state.Value = EnemyState.Patrol;
                    target = null;
                }
            }
        }
    }
    
    private bool CanSee(Transform t)
    {
        Vector3 dir = (t.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        
        if (angle < fieldOfView / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, dir, out hit, detectionRange))
            {
                return hit.transform == t || hit.transform.IsChildOf(t);
            }
        }
        return false;
    }
    
    private void IdleUpdate()
    {
        agent.isStopped = true;
        waitTimer += 0.5f;
        
        if (waitTimer >= waypointWaitTime)
        {
            waitTimer = 0f;
            state.Value = EnemyState.Patrol;
        }
    }
    
    private void PatrolUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        
        Transform wp = waypoints[waypointIndex];
        if (wp != null)
        {
            agent.SetDestination(wp.position);
            
            if (Vector3.Distance(transform.position, wp.position) <= waypointDistance)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
                state.Value = EnemyState.Idle;
            }
        }
    }
    
    private void ChaseUpdate()
    {
        if (target == null)
        {
            state.Value = EnemyState.Patrol;
            return;
        }
        
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
        
        if (Vector3.Distance(transform.position, target.position) < 1.5f)
        {
            CaughtPlayerClientRpc();
        }
    }
    
    [ClientRpc]
    private void CaughtPlayerClientRpc()
    {
        Debug.Log("GAME OVER - Player Caught!");
    }
    
    private void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        UpdateMaterial(newState);
    }
    
    private void UpdateMaterial(EnemyState s)
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material = s == EnemyState.Chase ? chaseMaterial : idleMaterial;
        }
    }
    
    public override void OnNetworkDespawn()
    {
        state.OnValueChanged -= OnStateChanged;
        base.OnNetworkDespawn();
    }
}