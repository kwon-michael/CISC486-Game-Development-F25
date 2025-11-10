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
        Debug.Log("Enemy entering IDLE state - Standing guard");
        enemy.GetAgent().speed = 0f;
        enemy.GetAgent().isStopped = true;
        enemy.SetMaterial(enemy.GetIdleMaterial());
        idleTimer = 0f;
    }
    
    public override void Update()
    {
        idleTimer += Time.deltaTime;
        
        // Priority decision: Check if player is in detection range (FSM Decision-Making)
        if (enemy.GetDistanceToPlayer() <= enemy.GetDetectionRange() && enemy.CanSeePlayer())
        {
            Debug.Log("Player detected during idle! Switching to Chase");
            enemy.ChangeToChase();
            return;
        }
        
        // After waiting, start patrolling (FSM State Transition Logic)
        if (idleTimer >= enemy.GetWaitTime())
        {
            Debug.Log("Idle wait complete, beginning patrol");
            enemy.ChangeToPatrol();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("Enemy exiting IDLE state");
        enemy.GetAgent().isStopped = false;
    }
}