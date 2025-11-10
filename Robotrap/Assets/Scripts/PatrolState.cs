using UnityEngine;

public class PatrolState : State
{
    private EnemyController enemy;
    private bool isWaitingAtPoint = false;
    private float waitTimer = 0f;
    private const float waitAtPointDuration = 1f; // Brief pause at patrol points
    
    public PatrolState(EnemyController enemyController)
    {
        enemy = enemyController;
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entering PATROL state - Beginning patrol route");
        enemy.GetAgent().speed = enemy.GetPatrolSpeed();
        enemy.GetAgent().isStopped = false;
        enemy.SetMaterial(enemy.GetPatrolMaterial());
        isWaitingAtPoint = false;
        
        // Set destination to current patrol point
        SetNextPatrolDestination();
    }
    
    public override void Update()
    {
        // Priority: Check if player is in detection range (Decision-making + Pathfinding integration)
        if (enemy.GetDistanceToPlayer() <= enemy.GetDetectionRange() && enemy.CanSeePlayer())
        {
            Debug.Log("Player detected during patrol! Switching to Chase");
            enemy.ChangeToChase();
            return;
        }
        
        // Handle patrol movement
        if (!isWaitingAtPoint)
        {
            // Check if reached patrol point (using NavMesh pathfinding)
            if (!enemy.GetAgent().pathPending && enemy.GetAgent().remainingDistance <= enemy.GetArrivalDistance())
            {
                // Arrived at patrol point, start waiting
                isWaitingAtPoint = true;
                waitTimer = 0f;
                enemy.GetAgent().isStopped = true;
                Debug.Log($"Reached patrol point {enemy.GetCurrentPatrolIndex()}, waiting briefly");
            }
        }
        else
        {
            // Waiting at patrol point
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAtPointDuration)
            {
                // Resume patrol
                isWaitingAtPoint = false;
                enemy.GetAgent().isStopped = false;
                
                if (enemy.GetPatrolPoints().Length > 0)
                {
                    // Cycle through patrol points
                    int nextIndex = (enemy.GetCurrentPatrolIndex() + 1) % enemy.GetPatrolPoints().Length;
                    enemy.SetCurrentPatrolIndex(nextIndex);
                    SetNextPatrolDestination();
                    Debug.Log($"Moving to next patrol point: {nextIndex}");
                }
                else
                {
                    // No patrol points, go to idle
                    Debug.Log("No patrol points available, switching to Idle");
                    enemy.ChangeToIdle();
                }
            }
        }
    }
    
    public override void Exit()
    {
        Debug.Log("Enemy exiting PATROL state");
        enemy.GetAgent().isStopped = false;
        isWaitingAtPoint = false;
    }
    
    private void SetNextPatrolDestination()
    {
        if (enemy.GetPatrolPoints().Length > 0)
        {
            Transform targetPoint = enemy.GetPatrolPoints()[enemy.GetCurrentPatrolIndex()];
            
            // Verify path is valid before setting destination (Pathfinding validation)
            if (enemy.IsPathValid(targetPoint.position))
            {
                enemy.SeekTarget(targetPoint.position); // Use smooth seeking behavior
                Debug.Log($"Path set to patrol point {enemy.GetCurrentPatrolIndex()} at {targetPoint.position}");
            }
            else
            {
                Debug.LogWarning($"Cannot reach patrol point {enemy.GetCurrentPatrolIndex()}, skipping");
                // Skip to next patrol point if current one is unreachable
                int nextIndex = (enemy.GetCurrentPatrolIndex() + 1) % enemy.GetPatrolPoints().Length;
                enemy.SetCurrentPatrolIndex(nextIndex);
            }
        }
    }
}