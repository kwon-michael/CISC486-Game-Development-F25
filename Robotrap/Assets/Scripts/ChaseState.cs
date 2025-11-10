using UnityEngine;

public class ChaseState : State
{
    private EnemyController enemy;
    private float updatePathTimer = 0f;
    private const float pathUpdateInterval = 0.2f; // Update path 5 times per second
    
    public ChaseState(EnemyController enemyController)
    {
        enemy = enemyController;
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entering CHASE state - Seeking player!");
        enemy.GetAgent().speed = enemy.GetChaseSpeed();
        enemy.GetAgent().acceleration = 8f; // Higher acceleration for pursuit
        enemy.SetMaterial(enemy.GetChaseMaterial());
        updatePathTimer = 0f;
    }
    
    public override void Update()
    {
        // Check if player is too far away or out of sight
        if (enemy.GetDistanceToPlayer() > enemy.GetLosePlayerRange() || !enemy.CanSeePlayer())
        {
            Debug.Log("Player escaped! Returning to Idle state");
            enemy.ChangeToIdle();
            return;
        }
        
        // Update path periodically for smooth following
        updatePathTimer += Time.deltaTime;
        if (updatePathTimer >= pathUpdateInterval)
        {
            updatePathTimer = 0f;
            
            // Use seek behavior to chase player smoothly
            if (enemy.GetPlayer() != null)
            {
                enemy.SeekTarget(enemy.GetPlayer().position);
            }
        }
        
        // Check if caught the player
        if (enemy.GetDistanceToPlayer() < 1.5f)
        {
            Debug.Log("Player caught!");
            // Handle player caught logic here
            enemy.ChangeToIdle();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("Enemy exiting CHASE state");
        enemy.GetAgent().acceleration = 3f; // Reset to normal acceleration
    }
}