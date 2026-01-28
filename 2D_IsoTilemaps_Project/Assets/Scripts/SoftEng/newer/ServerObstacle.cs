using UnityEngine;

// Server that must be activated
public class ServerObstacle : Obstacle
{
    private bool isActivated = false;
    public SpriteRenderer spriteRenderer;
    public Color inactiveColor = Color.red;
    public Color activeColor = Color.green;
    
    void Start()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = inactiveColor;
        }
    }
    
    public override void OnPlayerEnter(PlayerController player)
    {
        if (!isActivated)
        {
            isActivated = true;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = activeColor;
            }
            Debug.Log("Server activated!");
        }
    }
    
    public override void OnPlayerExit(PlayerController player)
    {
        // Server stays activated
    }
    
    public bool IsActivated()
    {
        return isActivated;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter(other.GetComponent<PlayerController>());
        }
    }
}
