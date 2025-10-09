using UnityEngine;

public class IdleState : State
{
    private EnemyController enemy;
    private float idleTimer;
    
    public IdleState(EnemyController enemyController)
    {
        enemy = enemyController;
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entering IDLE state");
        enemy.GetAgent().speed = 0f;
        enemy.GetAgent().isStopped = true;
        enemy.SetMaterial(enemy.GetIdleMaterial());
        idleTimer = 0f;
    }
    
    public override void Update()
    {
        idleTimer += Time.deltaTime;
        
        // Check if player is in detection range
        if (enemy.GetDistanceToPlayer() <= enemy.GetDetectionRange() && enemy.CanSeePlayer())
        {
            enemy.ChangeToChase();
            return;
        }
        
        // After waiting, start patrolling
        if (idleTimer >= enemy.GetWaitTime())
        {
            enemy.ChangeToPatrol();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("Enemy exiting IDLE state");
        enemy.GetAgent().isStopped = false;
    }
}