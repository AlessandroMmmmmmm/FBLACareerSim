using UnityEngine;

/// <summary>
/// Updated IsometricCharacterRenderer that works with Animator Blend Trees
/// Uses MoveX, MoveY, and IsMoving parameters instead of direct animation states
/// Compatible with 8-directional sprite sheets with left-facing animations
/// </summary>
public class IsometricCharacterRenderer : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private int lastDirection;

    // Animation parameter hashes (more efficient than strings)
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        // Cache components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            Debug.LogError("IsometricCharacterRenderer: No Animator found!");
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("IsometricCharacterRenderer: No SpriteRenderer found!");
        }
    }

    /// <summary>
    /// Set the character's movement direction and update animations
    /// </summary>
    /// <param name="direction">Movement direction vector. Zero = idle/static</param>
    public void SetDirection(Vector2 direction)
    {
        if (animator == null) return;

        bool isMoving = direction.magnitude > 0.01f;

        // Determine the animation direction
        Vector2 animDirection;
        if (isMoving)
        {
            // Moving - use the movement direction
            animDirection = direction.normalized;
            lastDirection = DirectionToIndex(direction, 8);
        }
        else
        {
            // Idle - use last facing direction
            animDirection = IndexToDirection(lastDirection);
        }

        // Set animator parameters for blend tree
        animator.SetFloat(MoveX, animDirection.x);
        animator.SetFloat(MoveY, animDirection.y);
        animator.SetBool(IsMoving, isMoving);

        // Handle sprite flipping for right-facing directions
        FlipSprite(animDirection);

        // Debug logging
        // Debug.Log($"SetDirection: dir={direction}, animDir={animDirection}, isMoving={isMoving}, lastDir={lastDirection}");
    }

    /// <summary>
    /// Flip sprite for right-facing directions
    /// Since sprites are LEFT-facing by default, we flip them for right
    /// </summary>
    private void FlipSprite(Vector2 direction)
    {
        if (spriteRenderer == null) return;

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
    /// Convert a direction vector to an index (0-7) representing one of 8 directions
    /// Goes counter-clockwise starting from North (up)
    /// </summary>
    public static int DirectionToIndex(Vector2 dir, int sliceCount)
    {
        // Get the normalized direction
        Vector2 normDir = dir.normalized;

        // Calculate how many degrees one slice is
        float step = 360f / sliceCount;

        // Calculate how many degrees half a slice is
        // We need this to offset the pie, so that the North (UP) slice is aligned in the center
        float halfstep = step / 2;

        // Get the angle from -180 to 180 of the direction vector relative to the Up vector
        // This will return the angle between dir and North
        float angle = Vector2.SignedAngle(Vector2.up, normDir);

        // Add the halfslice offset
        angle += halfstep;

        // If angle is negative, then let's make it positive by adding 360 to wrap it around
        if (angle < 0)
        {
            angle += 360;
        }

        // Calculate the amount of steps required to reach this angle
        float stepCount = angle / step;

        // Round it, and we have the answer!
        return Mathf.FloorToInt(stepCount);
    }

    /// <summary>
    /// Convert an index (0-7) back to a direction vector
    /// Useful for maintaining facing direction when idle
    /// </summary>
    public static Vector2 IndexToDirection(int index)
    {
        // 8 directions, counter-clockwise from North
        switch (index)
        {
            case 0: return Vector2.up;                      // N
            case 1: return new Vector2(-1, 1).normalized;   // NW
            case 2: return Vector2.left;                    // W
            case 3: return new Vector2(-1, -1).normalized;  // SW
            case 4: return Vector2.down;                    // S
            case 5: return new Vector2(1, -1).normalized;   // SE
            case 6: return Vector2.right;                   // E
            case 7: return new Vector2(1, 1).normalized;    // NE
            default: return Vector2.down;                   // Default to S
        }
    }

    /// <summary>
    /// Get the current facing direction index
    /// </summary>
    public int GetLastDirection()
    {
        return lastDirection;
    }

    /// <summary>
    /// Get the current facing direction as a Vector2
    /// </summary>
    public Vector2 GetLastDirectionVector()
    {
        return IndexToDirection(lastDirection);
    }

    /// <summary>
    /// Legacy function - converts string array to hash array
    /// Not needed with blend tree system, but kept for compatibility
    /// </summary>
    public static int[] AnimatorStringArrayToHashArray(string[] animationArray)
    {
        // Allocate the same array length for our hash array
        int[] hashArray = new int[animationArray.Length];

        // Loop through the string array
        for (int i = 0; i < animationArray.Length; i++)
        {
            // Do the hash and save it to our hash array
            hashArray[i] = Animator.StringToHash(animationArray[i]);
        }

        // We're done!
        return hashArray;
    }
}
