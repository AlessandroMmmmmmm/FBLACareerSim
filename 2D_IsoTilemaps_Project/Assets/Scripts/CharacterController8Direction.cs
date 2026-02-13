using UnityEngine;

/// <summary>
/// 8-Directional Character Controller for top-down games
/// Uses custom keybinds from KeybindManager
/// Uses blend trees to smoothly transition between directional animations
/// Automatically flips sprite for left-facing directions
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterController8Direction : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation Settings")]
    [Tooltip("Minimum input magnitude to trigger movement animation")]
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Optional Physics")]
    [SerializeField] private bool useRigidbody = false;

    // Components
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Movement
    private Vector2 moveInput;
    private Vector2 lastMoveDirection;
    private bool isMoving;

    // Animation parameter hashes (more efficient than strings)
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize last direction as down
        lastMoveDirection = Vector2.down;

        // Setup physics if needed
        if (useRigidbody)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("useRigidbody is true but no Rigidbody2D found. Adding one...");
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    void Update()
    {
        // Get player input
        GetInput();

        // Update animation parameters
        UpdateAnimation();

        // Move character (if not using physics)
        if (!useRigidbody)
        {
            MoveCharacter();
        }
    }

    void FixedUpdate()
    {
        // Move using physics
        if (useRigidbody && rb != null)
        {
            MoveCharacterPhysics();
        }
    }

    /// <summary>
    /// Get movement input from custom keybinds or fallback to default input
    /// </summary>
    private void GetInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        // Use custom keybinds if KeybindManager exists, otherwise use default input
        if (KeybindManager.Instance != null)
        {
            horizontal = KeybindManager.Instance.GetHorizontalInput();
            vertical = KeybindManager.Instance.GetVerticalInput();
        }
        else
        {
            // Fallback to default Unity Input Manager
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }

        moveInput = new Vector2(horizontal, vertical);

        // Normalize diagonal movement
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        // Check if moving
        isMoving = moveInput.magnitude > moveThreshold;

        // Store last movement direction for idle facing
        if (isMoving)
        {
            lastMoveDirection = moveInput;
        }
    }

    /// <summary>
    /// Update animator parameters for blend tree
    /// </summary>
    private void UpdateAnimation()
    {
        // Determine which direction to show
        Vector2 animDirection = isMoving ? moveInput : lastMoveDirection;

        // Set animator parameters
        animator.SetFloat(MoveX, animDirection.x);
        animator.SetFloat(MoveY, animDirection.y);
        animator.SetBool(IsMoving, isMoving);

        // Handle sprite flipping for left-facing directions
        FlipSprite(animDirection);
    }

    /// <summary>
    /// Move character using transform (non-physics)
    /// </summary>
    private void MoveCharacter()
    {
        if (isMoving)
        {
            Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0f);
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Move character using Rigidbody2D (physics-based)
    /// </summary>
    private void MoveCharacterPhysics()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    /// <summary>
    /// Flip sprite for right-facing directions
    /// Since sprites are LEFT-facing by default, we flip them for right
    /// </summary>
    private void FlipSprite(Vector2 direction)
    {
        // Only flip based on horizontal direction
        if (direction.x > 0.01f)
        {
            // Moving right - flip sprite (since default is left)
            spriteRenderer.flipX = true;

            // Also flip the animator's X parameter so blend tree uses left-side animations
            animator.SetFloat(MoveX, -direction.x);
        }
        else if (direction.x < -0.01f)
        {
            // Moving left - normal orientation (default sprite faces left)
            spriteRenderer.flipX = false;
        }
        // If only moving vertically, maintain current flip state
    }

    /// <summary>
    /// Get current movement direction (for other scripts)
    /// </summary>
    public Vector2 GetMoveDirection()
    {
        return moveInput;
    }

    /// <summary>
    /// Get the last faced direction (useful for attacks, interactions)
    /// </summary>
    public Vector2 GetFacingDirection()
    {
        return lastMoveDirection;
    }

    /// <summary>
    /// Check if character is currently moving
    /// </summary>
    public bool GetIsMoving()
    {
        return isMoving;
    }

    /// <summary>
    /// Set movement speed at runtime
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// Get current movement speed
    /// </summary>
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
}