using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(10, 15)]
    public string instructions = @"SETUP INSTRUCTIONS:

1. Create a 3D scene with a plane as the ground
2. Add a Capsule for the Player:
   - Add CharacterController component
   - Add PlayerController script
   - Tag as 'Player'

3. Add a Cube for the Enemy:
   - Add NavMeshAgent component
   - Add EnemyController script
   - Tag as 'Enemy'
   - Create 3 materials (different colors) for Idle, Patrol, Chase states
   - Create empty GameObjects as patrol points and assign to Enemy

4. Create NavMesh:
   - Select ground plane
   - Window > AI > Navigation
   - Mark ground as 'Navigation Static'
   - Click 'Bake'

5. Test the FSM:
   - Play the scene
   - Watch enemy change colors as it switches states
   - Move player near enemy to trigger chase
   - Move player away to return to patrol/idle";
}