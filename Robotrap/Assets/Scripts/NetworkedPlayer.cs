using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NetworkedPlayer : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    
    [Header("Camera")]
    [SerializeField] private float cameraDistance = 15f;
    [SerializeField] private float cameraHeight = 12f;
    [SerializeField] private float cameraPitch = 60f;
    
    [Header("Materials")]
    [SerializeField] private Material player1Material;
    [SerializeField] private Material player2Material;
    
    [Header("Controls - Player 1 (WASD)")]
    [SerializeField] private KeyCode p1Forward = KeyCode.W;
    [SerializeField] private KeyCode p1Back = KeyCode.S;
    [SerializeField] private KeyCode p1Left = KeyCode.A;
    [SerializeField] private KeyCode p1Right = KeyCode.D;
    [SerializeField] private KeyCode p1Jump = KeyCode.Space;
    
    [Header("Controls - Player 2 (Arrows)")]
    [SerializeField] private KeyCode p2Forward = KeyCode.UpArrow;
    [SerializeField] private KeyCode p2Back = KeyCode.DownArrow;
    [SerializeField] private KeyCode p2Left = KeyCode.LeftArrow;
    [SerializeField] private KeyCode p2Right = KeyCode.RightArrow;
    [SerializeField] private KeyCode p2Jump = KeyCode.RightControl;
    
    private CharacterController controller;
    private Camera playerCamera;
    private float verticalVelocity;
    private Renderer playerRenderer;
    
    // Track which control scheme to use
    private bool useWASD = true;
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerRenderer = GetComponent<Renderer>();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Assign material and controls based on client ID
        if (playerRenderer != null)
        {
            if (OwnerClientId == 0)
            {
                // Player 1 - HOST - Green - WASD
                if (player1Material != null)
                {
                    playerRenderer.material = player1Material;
                }
                useWASD = true;
                Debug.Log("Player 1 spawned - Using WASD controls");
            }
            else
            {
                // Player 2 - CLIENT - Cyan - Arrow Keys
                if (player2Material != null)
                {
                    playerRenderer.material = player2Material;
                }
                useWASD = false;
                Debug.Log("Player 2 spawned - Using ARROW KEY controls");
            }
        }
        
        // Only create camera for the local player (the one YOU control)
        if (IsOwner)
        {
            CreatePlayerCamera();
            CreateNameTag();
        }
    }
    
    private void CreatePlayerCamera()
    {
        // Create a new camera for this player
        GameObject camObj = new GameObject("PlayerCamera");
        playerCamera = camObj.AddComponent<Camera>();
        
        // Configure camera
        playerCamera.clearFlags = CameraClearFlags.Skybox;
        playerCamera.depth = 0;
        
        // Disable the Main Camera for this player
        Camera mainCam = Camera.main;
        if (mainCam != null && IsOwner)
        {
            mainCam.enabled = false;
        }
        
        Debug.Log($"Camera created for Player {OwnerClientId + 1}");
    }
    
    private void CreateNameTag()
    {
        GameObject nameTagObj = new GameObject("NameTag");
        nameTagObj.transform.SetParent(transform);
        nameTagObj.transform.localPosition = new Vector3(0, 2.5f, 0);
        
        TextMesh textMesh = nameTagObj.AddComponent<TextMesh>();
        textMesh.text = useWASD ? "YOU (WASD)" : "YOU (ARROWS)";
        textMesh.characterSize = 0.1f;
        textMesh.fontSize = 50;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = useWASD ? Color.green : Color.cyan;
        
        // Make text face camera
        nameTagObj.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        
        HandleMovement();
        UpdateCamera();
    }
    
    private void HandleMovement()
    {
        // Get input based on which player this is
        float horizontal = 0f;
        float vertical = 0f;
        bool jumpPressed = false;
        
        if (useWASD)
        {
            // Player 1 - WASD controls
            if (Input.GetKey(p1Forward)) vertical += 1f;
            if (Input.GetKey(p1Back)) vertical -= 1f;
            if (Input.GetKey(p1Left)) horizontal -= 1f;
            if (Input.GetKey(p1Right)) horizontal += 1f;
            jumpPressed = Input.GetKeyDown(p1Jump);
        }
        else
        {
            // Player 2 - Arrow key controls
            if (Input.GetKey(p2Forward)) vertical += 1f;
            if (Input.GetKey(p2Back)) vertical -= 1f;
            if (Input.GetKey(p2Left)) horizontal -= 1f;
            if (Input.GetKey(p2Right)) horizontal += 1f;
            jumpPressed = Input.GetKeyDown(p2Jump);
        }
        
        // Calculate movement direction
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        
        if (moveDirection.magnitude >= 0.1f)
        {
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            
            // Move
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        
        // Gravity and jumping
        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
            
            if (jumpPressed)
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        
        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }
    
    private void UpdateCamera()
    {
        if (playerCamera == null) return;
        
        // Position camera behind and above the player
        Vector3 targetPosition = transform.position - transform.forward * cameraDistance + Vector3.up * cameraHeight;
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPosition, 5f * Time.deltaTime);
        
        // Look at player
        Vector3 lookTarget = transform.position + Vector3.up * 2f;
        playerCamera.transform.LookAt(lookTarget);
    }
    
    public override void OnNetworkDespawn()
    {
        // Clean up camera
        if (playerCamera != null)
        {
            Destroy(playerCamera.gameObject);
        }
        
        base.OnNetworkDespawn();
    }
}