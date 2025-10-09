using UnityEngine;

public class SimplePlayerTest : MonoBehaviour
{
    public float speed = 5f;
    
    void Update()
    {
        // Simple transform-based movement for testing
        Vector3 move = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W)) move.z = 1;
        if (Input.GetKey(KeyCode.S)) move.z = -1;
        if (Input.GetKey(KeyCode.A)) move.x = -1;
        if (Input.GetKey(KeyCode.D)) move.x = 1;
        
        if (move != Vector3.zero)
        {
            Debug.Log("Input detected: " + move);
            transform.position += move * speed * Time.deltaTime;
        }
    }
}