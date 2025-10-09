using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float gravity = -9.81f;
    
    [Header("Debug")]
    public bool debugInput = true;
    
    private CharacterController characterController;
    private Vector3 movement;
    private Vector3 velocity;
    private Animator animator;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController component missing on " + gameObject.name);
        }
        
        animator = GetComponent<Animator>();
        velocity = Vector3.zero;
    }
    
    void Update()
    {
        HandleInput();
        MovePlayer();
        UpdateAnimations();
    }
    
    void HandleInput()
    {
        // Get input for movement - using both Input.GetAxis and direct key detection
        float horizontal = 0f;
        float vertical = 0f;
        
        // Direct key input (more reliable)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;
            
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        
        // Fallback to Input.GetAxis if direct keys don't work
        if (horizontal == 0f && vertical == 0f)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        
        // Calculate movement direction (world space for simplicity)
        movement = new Vector3(horizontal, 0f, vertical).normalized;
        
        // Debug input
        if (debugInput && (horizontal != 0f || vertical != 0f))
        {
            Debug.Log($"Input detected - H: {horizontal}, V: {vertical}, Movement: {movement}");
        }
        
        // Rotate the player
        if (Input.GetKey(KeyCode.Q))
            transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.E))
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
    
    void MovePlayer()
    {
        if (characterController == null) return;
        
        // Calculate horizontal movement
        Vector3 move = movement * moveSpeed;
        
        // Apply gravity to vertical velocity
        if (characterController.isGrounded)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        
        // Combine horizontal movement with vertical velocity
        Vector3 finalMovement = move + Vector3.up * velocity.y;
        
        // Apply movement
        characterController.Move(finalMovement * Time.deltaTime);
        
        // Debug movement
        if (debugInput && move.magnitude > 0f)
        {
            Debug.Log($"Moving player: {finalMovement} (Speed: {moveSpeed})");
        }
    }
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Set animation parameters
            animator.SetFloat("Speed", movement.magnitude);
            animator.SetBool("IsMoving", movement.magnitude > 0.1f);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Player caught by enemy!");
            // Handle game over or respawn logic here
        }
        
        if (other.CompareTag("Goal"))
        {
            Debug.Log("Player reached the goal!");
            // Handle victory logic here
        }
    }
}