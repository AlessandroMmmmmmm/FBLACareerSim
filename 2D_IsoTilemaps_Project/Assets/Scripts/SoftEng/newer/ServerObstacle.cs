// using UnityEngine;

// // Server that must be activated
// public class ServerObstacle : Obstacle
// {
//     private bool isActivated = false;
//     public SpriteRenderer spriteRenderer;
    
//     [Header("Server Sprites")]
//     public Sprite inactiveSprite; // OFF state sprite
//     public Sprite activeSprite;   // ON state sprite
    
//     void Start()
//     {
//         if (spriteRenderer == null)
//         {
//             spriteRenderer = GetComponent<SpriteRenderer>();
//         }
        
//         // Start with inactive sprite
//         if (spriteRenderer != null && inactiveSprite != null)
//         {
//             spriteRenderer.sprite = inactiveSprite;
//         }
//     }
    
//     public override void OnPlayerEnter(PlayerController player)
//     {
//         if (!isActivated)
//         {
//             isActivated = true;
            
//             // Change to active sprite
//             if (spriteRenderer != null && activeSprite != null)
//             {
//                 spriteRenderer.sprite = activeSprite;
//             }
            
//             Debug.Log("Server activated!");
//         }
//     }
    
//     public override void OnPlayerExit(PlayerController player)
//     {
//         // Server stays activated
//     }
    
//     public bool IsActivated()
//     {
//         return isActivated;
//     }
    
//     void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             OnPlayerEnter(other.GetComponent<PlayerController>());
//         }
//     }
// }

using UnityEngine;

// Server that must be activated
public class ServerObstacle : Obstacle
{
    private bool isActivated = false;
    public SpriteRenderer spriteRenderer;
    
    [Header("Server Sprites")]
    public Sprite inactiveSprite; // OFF state sprite
    public Sprite activeSprite;   // ON state sprite
    
    [Header("Isometric Alignment")]
    [Tooltip("Y offset to align with tiles (should match player's yOffset)")]
    public float yOffset = -0.25f;
    
    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Start with inactive sprite
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
        
        // Apply Y offset to position
        ApplyPositionOffset();
    }
    
    void ApplyPositionOffset()
    {
        Vector3 pos = transform.position;
        pos.y += yOffset;
        transform.position = pos;
    }
    
    public override void OnPlayerEnter(PlayerController player)
    {
        if (!isActivated)
        {
            isActivated = true;
            
            // Change to active sprite
            if (spriteRenderer != null && activeSprite != null)
            {
                spriteRenderer.sprite = activeSprite;
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
        Debug.Log($"Server collided with: {other.gameObject.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player tag detected! Calling OnPlayerEnter");
            OnPlayerEnter(other.GetComponent<PlayerController>());
        }
        else
        {
            Debug.Log($"Not player tag. Expected 'Player', got '{other.tag}'");
        }
    }
}