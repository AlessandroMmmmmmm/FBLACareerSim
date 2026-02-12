using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimap system that renders the world to a UI texture,
/// highlighting drop-off zones and showing truck position.
/// 
/// SETUP INSTRUCTIONS:
/// 1. Create empty GameObject "MinimapSystem" → add this script
/// 2. Create a Camera child → assign to minimapCamera
/// 3. Create RenderTexture (256x256, R8G8B8A8) → assign to minimapRenderTexture
/// 4. Set camera Output Texture to that RenderTexture
/// 5. Build UI (see UI_MINIMAP_SETUP.md for positions)
/// </summary>
public class MinimapController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera minimapCamera; // Orthographic camera looking down
    public float cameraHeight = 300f; // How high above world to look down
    public float viewSize = 130f; // Exactly half of 260 (Z range: -225 to 35)

    [Header("Render Texture")]
    public RenderTexture minimapRenderTexture; // 256x256 render texture

    [Header("UI References")]
    public RawImage minimapDisplay; // UI element showing the render texture
    public RectTransform minimapBorder; // Optional border/frame around minimap

    [Header("Truck Marker")]
    public RectTransform truckMarker; // Arrow/dot showing player position
    public Transform truckTransform; // The actual truck in world
    public Color truckMarkerColor = new Color(0.2f, 0.8f, 1f); // Cyan

    [Header("Drop-off Zone Markers")]
    public RectTransform[] dropOffMarkers = new RectTransform[3]; // UI markers
    public Transform[] dropOffPoints = new Transform[3]; // World positions
    public Color dropOffActiveColor = new Color(1f, 0.3f, 0.1f); // Orange-red
    public Color dropOffDeliveredColor = new Color(0.3f, 1f, 0.4f); // Green

    [Header("Pulsing Effect")]
    public float pulseSpeed = 2f;
    public float pulseMinScale = 0.8f;
    public float pulseMaxScale = 1.2f;

    [Header("Camera Offset (tweak to center the map)")]
    [Tooltip("Fine-tune X/Z offset if map appears off-center in minimap")]
    public Vector2 cameraOffset = Vector2.zero;
    [Tooltip("Center of the playable area in world X/Z. Leave at 0,0 to auto-calculate from drop-offs.")]
    public Vector2 manualWorldCenter = new Vector2(0f, -45f); // Midpoint of Z=-225 to Z=35
    [Tooltip("If true, uses manualWorldCenter instead of auto-calculating")]
    public bool useManualCenter = true;

    private Vector2 worldCenter;
    private float worldSize;

    private ArcadeTruck truck;
    private int packagesDelivered = 0;

    void Start()
    {
        truck = FindObjectOfType<ArcadeTruck>();

        if (truckTransform == null && truck != null)
        {
            truckTransform = truck.transform;
        }

        // Set up minimap camera - FIXED POSITION, does not follow truck
        if (minimapCamera != null)
        {
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = viewSize;
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            minimapCamera.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));

            // Fix camera at world center looking straight down - NEVER MOVES
            RecalculateWorldCenter();
            minimapCamera.transform.position = new Vector3(
                worldCenter.x + cameraOffset.x,
                cameraHeight,
                worldCenter.y + cameraOffset.y
            );
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            if (minimapRenderTexture != null)
            {
                minimapCamera.targetTexture = minimapRenderTexture;
            }
        }

        // Apply render texture to UI and force it to fill the container
        if (minimapDisplay != null && minimapRenderTexture != null)
        {
            minimapDisplay.texture = minimapRenderTexture;
            minimapDisplay.uvRect = new Rect(0, 0, 1, 1);

            // Force stretch to fill parent
            RectTransform rt = minimapDisplay.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Color truck marker
        if (truckMarker != null)
        {
            Image img = truckMarker.GetComponent<Image>();
            if (img != null) img.color = truckMarkerColor;
        }

        // Color drop-off markers
        for (int i = 0; i < dropOffMarkers.Length; i++)
        {
            if (dropOffMarkers[i] != null)
            {
                Image img = dropOffMarkers[i].GetComponent<Image>();
                if (img != null) img.color = dropOffActiveColor;
            }
        }
    }

    void Update()
    {
        if (minimapCamera == null) return;

        // Update truck marker position on minimap (camera is fixed, marker moves)
        UpdateTruckMarker();

        // Update drop-off marker positions (fixed on minimap - they don't move)
        UpdateDropOffMarkers();

        // Pulse active drop-off markers
        PulseMarkers();

        // Sync delivered state
        if (truck != null)
        {
            packagesDelivered = truck.packagesDelivered;
        }
    }

    private void UpdateTruckMarker()
    {
        if (truckTransform == null || minimapDisplay == null || truckMarker == null) return;

        float minimapSize = minimapDisplay.rectTransform.rect.width;
        float worldToMapScale = (minimapSize * 0.5f) / viewSize;

        // Offset from world center to truck
        float mapX = (truckTransform.position.x - worldCenter.x) * worldToMapScale;
        float mapY = (truckTransform.position.z - worldCenter.y) * worldToMapScale;

        // Clamp to minimap edges
        float maxRange = minimapSize * 0.5f;
        mapX = Mathf.Clamp(mapX, -maxRange, maxRange);
        mapY = Mathf.Clamp(mapY, -maxRange, maxRange);

        truckMarker.localPosition = new Vector3(mapX, mapY, 0);

        // Rotate arrow to match truck direction
        truckMarker.localEulerAngles = new Vector3(0, 0, -truckTransform.eulerAngles.y);
    }

    private void UpdateDropOffMarkers()
    {
        if (minimapDisplay == null) return;

        float minimapSize = minimapDisplay.rectTransform.rect.width;
        float worldToMapScale = (minimapSize * 0.5f) / viewSize;

        for (int i = 0; i < dropOffPoints.Length; i++)
        {
            if (dropOffMarkers[i] == null || dropOffPoints[i] == null) continue;

            // Calculate offset from WORLD CENTER - markers are fixed on map
            float mapX = (dropOffPoints[i].position.x - worldCenter.x) * worldToMapScale;
            float mapY = (dropOffPoints[i].position.z - worldCenter.y) * worldToMapScale;

            dropOffMarkers[i].localPosition = new Vector3(mapX, mapY, 0);

            // Update color based on delivery status
            Image img = dropOffMarkers[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (i < packagesDelivered) ? dropOffDeliveredColor : dropOffActiveColor;
            }
        }
    }

    private void PulseMarkers()
    {
        float pulse = Mathf.Lerp(pulseMinScale, pulseMaxScale,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);

        for (int i = 0; i < dropOffMarkers.Length; i++)
        {
            if (dropOffMarkers[i] == null) continue;

            // Only pulse undelivered markers
            if (i < packagesDelivered)
            {
                dropOffMarkers[i].localScale = Vector3.one;
            }
            else
            {
                // Stagger pulse timing per marker
                float staggeredPulse = Mathf.Lerp(pulseMinScale, pulseMaxScale,
                    (Mathf.Sin(Time.time * pulseSpeed + i * 1.5f) + 1f) * 0.5f);
                dropOffMarkers[i].localScale = Vector3.one * staggeredPulse;
            }
        }
    }

    private void RecalculateWorldCenter()
    {
        if (useManualCenter)
        {
            worldCenter = manualWorldCenter;
            Debug.Log($"Minimap using manual world center: {worldCenter}");
            return;
        }

        // Auto-calculate center from drop-off points
        if (dropOffPoints == null || dropOffPoints.Length == 0) return;

        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (Transform t in dropOffPoints)
        {
            if (t != null)
            {
                center += t.position;
                count++;
            }
        }

        if (count > 0)
        {
            center /= count;
            worldCenter = new Vector2(center.x, center.z);
            Debug.Log($"Minimap auto-calculated world center: {worldCenter}");
        }
    }

    // Called when a delivery is made (optional - for instant color update)
    public void OnDeliveryMade(int deliveryIndex)
    {
        if (deliveryIndex < dropOffMarkers.Length && dropOffMarkers[deliveryIndex] != null)
        {
            Image img = dropOffMarkers[deliveryIndex].GetComponent<Image>();
            if (img != null)
            {
                img.color = dropOffDeliveredColor;
            }

            // Stop pulsing by fixing scale
            dropOffMarkers[deliveryIndex].localScale = Vector3.one;
        }
    }
}
