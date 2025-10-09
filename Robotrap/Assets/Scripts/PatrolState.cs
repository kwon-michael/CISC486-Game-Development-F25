using UnityEngine;

public class PatrolState : State
{
    private EnemyController enemy;
    
    public PatrolState(EnemyController enemyController)
    {
        enemy = enemyController;
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entering PATROL state");
        enemy.GetAgent().speed = enemy.GetPatrolSpeed();
        enemy.SetMaterial(enemy.GetPatrolMaterial());
        
        // Set destination to current patrol point
        SetNextPatrolDestination();
    }
    
    public override void Update()
    {
        // Check if player is in detection range
        if (enemy.GetDistanceToPlayer() <= enemy.GetDetectionRange() && enemy.CanSeePlayer())
        {
            enemy.ChangeToChase();
            return;
        }
        
        // Check if reached patrol point
        if (!enemy.GetAgent().pathPending && enemy.GetAgent().remainingDistance < 0.5f)
        {
            // Move to next patrol point or go idle
            if (enemy.GetPatrolPoints().Length > 0)
            {
                // Cycle through patrol points
                int nextIndex = (enemy.GetCurrentPatrolIndex() + 1) % enemy.GetPatrolPoints().Length;
                enemy.SetCurrentPatrolIndex(nextIndex);
                SetNextPatrolDestination();
            }
            else
            {
                // No patrol points, go to idle
                enemy.ChangeToIdle();
            }
        }
    }
    
    public override void Exit()
    {
        Debug.Log("Enemy exiting PATROL state");
    }
    
    private void SetNextPatrolDestination()
    {
        if (enemy.GetPatrolPoints().Length > 0)
        {
            Transform targetPoint = enemy.GetPatrolPoints()[enemy.GetCurrentPatrolIndex()];
            enemy.GetAgent().SetDestination(targetPoint.position);
        }
    }
}