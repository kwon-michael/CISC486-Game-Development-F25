using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("UI References - Optional")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject hud;
    [SerializeField] private Text statusText;
    
    private void Start()
    {
        // Check if NetworkManager exists
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null! Make sure NetworkManager GameObject is in the scene.");
            return;
        }

        // Setup button listeners only if buttons are assigned
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(StartHost);
        }
        
        if (clientButton != null)
        {
            clientButton.onClick.AddListener(StartClient);
        }
        
        // Show menu at start
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
        
        if (hud != null)
        {
            hud.SetActive(false);
        }
    }
    
    private void StartHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            
            if (menuPanel != null) menuPanel.SetActive(false);
            if (hud != null) hud.SetActive(true);
            
            UpdateStatus("Connected as HOST");
            Debug.Log("Started as Host");
        }
    }
    
    private void StartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
            
            if (menuPanel != null) menuPanel.SetActive(false);
            if (hud != null) hud.SetActive(true);
            
            UpdateStatus("Connected as CLIENT");
            Debug.Log("Started as Client");
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null && NetworkManager.Singleton != null)
        {
            statusText.text = message + $"\nPlayers: {NetworkManager.Singleton.ConnectedClients.Count}";
        }
    }
    
    // OnGUI provides a fallback UI that always works
    private void OnGUI()
    {
        // Create a custom GUI style
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 20;
        buttonStyle.padding = new RectOffset(10, 10, 10, 10);
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 18;
        labelStyle.normal.textColor = Color.white;
        
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 16;
        boxStyle.normal.textColor = Color.white;
        
        // If not connected, show connection menu
        if (NetworkManager.Singleton != null && 
            !NetworkManager.Singleton.IsClient && 
            !NetworkManager.Singleton.IsServer)
        {
            // Center menu
            float menuWidth = 500;
            float menuHeight = 400;
            float x = (Screen.width - menuWidth) / 2;
            float y = (Screen.height - menuHeight) / 2;
            
            GUILayout.BeginArea(new Rect(x, y, menuWidth, menuHeight));
            
            // Background box
            GUI.Box(new Rect(0, 0, menuWidth, menuHeight), "");
            
            GUILayout.Space(20);
            
            // Title
            GUIStyle titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = 32;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("ROBOTRAP MULTIPLAYER", titleStyle);
            
            GUILayout.Space(40);
            
            // Instructions
            GUILayout.Label("Select your role:", labelStyle);
            GUILayout.Space(20);
            
            // Host button
            if (GUILayout.Button("HOST GAME (Player 1)", buttonStyle, GUILayout.Height(70)))
            {
                StartHost();
            }
            
            GUILayout.Space(20);
            
            // Client button
            if (GUILayout.Button("JOIN GAME (Player 2)", buttonStyle, GUILayout.Height(70)))
            {
                StartClient();
            }
            
            GUILayout.Space(30);
            
            // Controls info
            GUILayout.Label("Controls: WASD - Move | Mouse - Look | ESC - Toggle Cursor", labelStyle);
            
            GUILayout.EndArea();
        }
        // If connected, show status
        else if (NetworkManager.Singleton != null && 
                (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 200));
            
            string role = NetworkManager.Singleton.IsHost ? "HOST" : "CLIENT";
            
            GUILayout.Box($"=== CONNECTED AS {role} ===", boxStyle, GUILayout.Width(340));
            GUILayout.Box($"Players Connected: {NetworkManager.Singleton.ConnectedClients.Count}", boxStyle, GUILayout.Width(340));
            GUILayout.Box($"Your Client ID: {NetworkManager.Singleton.LocalClientId}", boxStyle, GUILayout.Width(340));
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("DISCONNECT", buttonStyle, GUILayout.Height(50), GUILayout.Width(340)))
            {
                NetworkManager.Singleton.Shutdown();
                
                if (menuPanel != null) menuPanel.SetActive(true);
                if (hud != null) hud.SetActive(false);
                
                Debug.Log("Disconnected from game");
            }
            
            GUILayout.EndArea();
        }
    }
}