using UnityEngine;

/// <summary>
/// Simple 2D Character Controller for top-down or platformer movement
/// Supports both keyboard and gamepad input
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Optional Physics")]
    [SerializeField] private bool useRigidbody = false;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isMoving = false;

    // Animation parameter names
    private static readonly int IsRunning = Animator.StringToHash("isRunning");

    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();

        // Get Rigidbody2D if it exists and we want to use it
        if (useRigidbody)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("useRigidbody is true but no Rigidbody2D component found!");
                useRigidbody = false;
            }
        }
    }

    void Update()
    {
        // Get input from keyboard or gamepad
        GetInput();

        // Update animation state
        UpdateAnimation();

        // Move the character (if not using Rigidbody)
        if (!useRigidbody)
        {
            MoveCharacter();
        }
    }

    void FixedUpdate()
    {
        // Move using physics if Rigidbody is enabled
        if (useRigidbody && rb != null)
        {
            MoveCharacterPhysics();
        }
    }

    /// <summary>
    /// Get player input from keyboard or gamepad
    /// </summary>
    private void GetInput()
    {
        // Get input axes (supports both keyboard and gamepad)
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrows
        float vertical = Input.GetAxisRaw("Vertical");     // W/S or Up/Down arrows

        moveInput = new Vector2(horizontal, vertical);

        // Normalize diagonal movement so character doesn't move faster diagonally
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        // Check if character is moving
        isMoving = moveInput.magnitude > 0.1f;
    }

    /// <summary>
    /// Update animator parameters based on movement state
    /// </summary>
    private void UpdateAnimation()
    {
        // Set the running animation based on movement
        animator.SetBool(IsRunning, isMoving);

        // Optional: Flip character sprite based on horizontal movement direction
        FlipCharacter();
    }

    /// <summary>
    /// Move character using transform (non-physics)
    /// </summary>
    private void MoveCharacter()
    {
        if (isMoving)
        {
            // Calculate movement
            Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0f);

            // Apply movement
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
            // Set velocity directly
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    /// <summary>
    /// Flip character sprite to face movement direction
    /// </summary>
    private void FlipCharacter()
    {
        if (moveInput.x < -0.1f)
        {
            // Moving left - flip sprite
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (moveInput.x > 0.1f)
        {
            // Moving right - normal orientation
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        // If moving only vertically, don't change facing direction
    }

    /// <summary>
    /// Public method to get current movement direction
    /// Useful for other scripts that need to know where the character is moving
    /// </summary>
    public Vector2 GetMoveDirection()
    {
        return moveInput;
    }

    /// <summary>
    /// Public method to check if character is currently moving
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }

    /// <summary>
    /// Public method to set movement speed at runtime
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed); // Ensure speed is not negative
    }
}
