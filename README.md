# CISC486-Game-Development-F25
# Robotrap
## Finite State Machine (FSM) Design
## Assignment 4 YT Vid (NEW)
https://youtu.be/yGl_PdU22Ak
## Assignment 3 YT Vid 
https://youtu.be/0Hff_iobn7U
## Assignment 2 YT Vid
https://youtu.be/jX8C1Z_nSFo
### FSM Diagram
```
    [IDLE] 
      |  
      | (wait time expires)
      v  
   [PATROL] ←→ [CHASE]
      |           |
      |           | (player out of range)
      |           v
      └─────→ [IDLE]
```

### State Descriptions and Transitions

#### 1. IDLE State
- **Behavior**: Enemy stops moving and waits in place
- **Visual**: Red color
- **Transitions**:
  - To PATROL: When wait time expires
  - To CHASE: If player enters detection range (8 units) and is visible

#### 2. PATROL State  
- **Behavior**: Enemy moves between predefined patrol points at normal speed
- **Visual**: Blue color
- **Transitions**:
  - To CHASE: If player enters detection range (8 units) and is visible
  - To IDLE: If no patrol points are set

#### 3. CHASE State
- **Behavior**: Enemy actively pursues the player at increased speed
- **Visual**: Yellow color  
- **Transitions**:
  - To IDLE: If player moves beyond lose range (12 units) or becomes invisible
  - To IDLE: If enemy catches player (within 1.5 units)

### Technical Implementation
- Uses Unity's NavMeshAgent for pathfinding
- Line-of-sight checking with raycasting
- Distance-based detection system
- Visual state feedback through material changes

## Original Game Concept

## Overview
Robotrap is a 2D maze game where the players goal is to navigate a maze (as a robot) and rescue a trapped ally while avoiding patrolling enemy drones.

## Core Gameplay
1. Main Character (Controllable Unit)
- Player controls a robot using arrow keys or WASD.
- Movement is simple: up, down, left, right.
2. Non-Player Characters (NPCs)
- Enemy drones patrol set paths using a simple Finite State Machine.
- Trapped ally robot waits at the end of the maze.

## Game Type
Survival and puzzle game

## AI Design
FSM States for Enemy Drones:
- Patrol: Move along a fixed path.
- Chase: If player enters detection range, chase them.
= Return: If player escapes, return to patrol path.
The decision-making for the enemy drones is triggered by distance checks using Unity’s built-in physics (no external AI libraries).

## Scripted Events
When the player reaches the trapped ally 
- Trigger rescue event: The ally robot will follow the player.
- The exit door will open at the start of the maze.
- Victory message will appear when the player and the ally robot escape through the exit door.
  
## Multiplayer Plan:
Second player will operate a 2nd robot. There is an option to play co-op mode where the two players can work together to rescue the robot.
Alternatively, users can play vs mode where the 2 robots will compete to rescue the trapped ally.

## Environment & Assets
Maze environment built using Unity’s Tilemap system.
Assets:
- Robot sprites (can use Unity's free assets or simple shapes).
- Maze tiles.
- Drones and ally robot.
- Exit door and UI elements.








