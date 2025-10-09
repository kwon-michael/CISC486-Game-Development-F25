using UnityEngine;

public class ChaseState : State
{
    private EnemyController enemy;
    
    public ChaseState(EnemyController enemyController)
    {
        enemy = enemyController;
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entering CHASE state");
        enemy.GetAgent().speed = enemy.GetChaseSpeed();
        enemy.SetMaterial(enemy.GetChaseMaterial());
    }
    
    public override void Update()
    {
        // Check if player is too far away or out of sight
        if (enemy.GetDistanceToPlayer() > enemy.GetLosePlayerRange() || !enemy.CanSeePlayer())
        {
            enemy.ChangeToIdle();
            return;
        }
        
        // Chase the player
        if (enemy.GetPlayer() != null)
        {
            enemy.GetAgent().SetDestination(enemy.GetPlayer().position);
        }
        
        // Check if caught the player
        if (enemy.GetDistanceToPlayer() < 1.5f)
        {
            Debug.Log("Player caught!");
            // In a real game, you might trigger a game over or respawn here
            enemy.ChangeToIdle();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("Enemy exiting CHASE state");
    }
}