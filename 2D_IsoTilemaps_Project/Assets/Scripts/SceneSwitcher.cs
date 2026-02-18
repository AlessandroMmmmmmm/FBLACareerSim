using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portal system using GameObject with PolygonCollider2D
/// Place this script on the portal GameObject (not the player!)
/// The portal GameObject should have a PolygonCollider2D set to "Is Trigger"
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
public class PortalTrigger : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Scene build index to load when player enters")]
    public int sceneToLoad = 5;

    [Header("Activation Settings")]
    [Tooltip("Require player to press a button to activate")]
    public bool requireButtonPress = false;

    [Tooltip("Button to press (e.g., 'E', 'Space', 'Return')")]
    public KeyCode activationKey = KeyCode.E;

    [Header("Visual Feedback")]
    [Tooltip("Optional UI prompt (e.g., 'Press E to Enter')")]
    public GameObject promptUI;

    [Tooltip("Optional sprite/effect to show portal visual")]
    public SpriteRenderer portalVisual;

    [Header("Audio")]
    public AudioClip portalSound; // Sound when entering portal

    [Header("Debug")]
    public bool showDebugInfo = true;

    private PolygonCollider2D portalCollider;
    private bool playerInPortal = false;
    private GameObject playerObject;
    private AudioSource audioSource;

    private void Start()
    {
        portalCollider = GetComponent<PolygonCollider2D>();

        if (portalCollider == null)
        {
            Debug.LogError("PortalTrigger: PolygonCollider2D not found! Add one to this GameObject.");
            return;
        }

        if (!portalCollider.isTrigger)
        {
            Debug.LogWarning("PortalTrigger: PolygonCollider2D is not set to trigger! Fixing it now...");
            portalCollider.isTrigger = true;
        }

        if (promptUI != null)
            promptUI.SetActive(false);

        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        if (showDebugInfo)
            Debug.Log($"Portal '{gameObject.name}' initialized. Will load scene {sceneToLoad}");
    }

    private void Update()
    {
        if (requireButtonPress && playerInPortal)
        {
            if (Input.GetKeyDown(activationKey))
                ActivatePortal();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInPortal = true;
            playerObject = other.gameObject;

            if (showDebugInfo)
                Debug.Log($"Player entered portal '{gameObject.name}'");

            if (requireButtonPress)
            {
                if (promptUI != null)
                    promptUI.SetActive(true);
            }
            else
            {
                ActivatePortal();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInPortal = false;
            playerObject = null;

            if (showDebugInfo)
                Debug.Log($"Player exited portal '{gameObject.name}'");

            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    private void ActivatePortal()
    {
        if (showDebugInfo)
            Debug.Log($"Portal '{gameObject.name}' activated! Loading scene {sceneToLoad}");

        // Play portal sound before scene transition
        if (portalSound != null && audioSource != null)
            audioSource.PlayOneShot(portalSound, 0.7f);

        // Silence MusicManager before scene loads
        MusicManager mm = FindFirstObjectByType<MusicManager>();
        if (mm != null) mm.Suppress();

        // Small delay to let sound play before scene loads
        Invoke(nameof(LoadScene), 0.2f);
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnDrawGizmos()
    {
        PolygonCollider2D col = GetComponent<PolygonCollider2D>();
        if (col == null || col.pathCount == 0)
            return;

        Gizmos.color = playerInPortal ? Color.green : Color.cyan;

        Vector2[] points = col.GetPath(0);
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 start = transform.TransformPoint(points[i]);
            Vector3 end = transform.TransformPoint(points[(i + 1) % points.Length]);
            Gizmos.DrawLine(start, end);
        }

        if (playerInPortal)
            Gizmos.color = new Color(0, 1, 0, 0.2f);
        else
            Gizmos.color = new Color(0, 1, 1, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        PolygonCollider2D col = GetComponent<PolygonCollider2D>();
        if (col == null || col.pathCount == 0)
            return;

        Gizmos.color = Color.yellow;
        Vector2[] points = col.GetPath(0);
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 start = transform.TransformPoint(points[i]);
            Vector3 end = transform.TransformPoint(points[(i + 1) % points.Length]);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(start, 0.1f);
        }
    }
}
