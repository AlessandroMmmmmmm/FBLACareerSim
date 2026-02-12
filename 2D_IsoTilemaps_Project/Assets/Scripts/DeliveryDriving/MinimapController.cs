using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera minimapCamera;
    public float cameraHeight = 300f;
    public float viewSize = 130f;

    [Header("Render Texture")]
    public RenderTexture minimapRenderTexture;

    [Header("UI References")]
    public RawImage minimapDisplay;
    public RectTransform minimapBorder;

    [Header("Truck Marker")]
    public RectTransform truckMarker;
    public Transform truckTransform;
    public Color truckMarkerColor = new Color(1f, 0.15f, 0.15f); // Red

    [Header("Drop-off Zone Markers")]
    public RectTransform[] dropOffMarkers = new RectTransform[3];
    public Transform[] dropOffPoints = new Transform[3];
    public Color dropOffActiveColor = new Color(1f, 0.3f, 0.1f);
    public Color dropOffDeliveredColor = new Color(0.3f, 1f, 0.4f);

    [Header("Pulsing Effect")]
    public float pulseSpeed = 2f;
    public float pulseMinScale = 0.8f;
    public float pulseMaxScale = 1.2f;

    [Header("World Bounds")]
    [Tooltip("Center of the playable area in world X/Z")]
    public Vector2 manualWorldCenter = new Vector2(0f, -95f);
    [Tooltip("Additional offset applied to camera AND all markers together")]
    public Vector2 cameraOffset = new Vector2(0f, 50f);

    // The effective center the camera looks at AND markers calculate from
    private Vector2 effectiveCenter;

    private ArcadeTruck truck;

    void Start()
    {
        truck = FindObjectOfType<ArcadeTruck>();

        if (truckTransform == null && truck != null)
            truckTransform = truck.transform;

        // Effective center = manual center + offset (both camera and markers use this)
        effectiveCenter = manualWorldCenter + cameraOffset;

        if (minimapCamera != null)
        {
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = viewSize;
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            minimapCamera.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));

            minimapCamera.transform.position = new Vector3(effectiveCenter.x, cameraHeight, effectiveCenter.y);
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            if (minimapRenderTexture != null)
                minimapCamera.targetTexture = minimapRenderTexture;
        }

        if (minimapDisplay != null && minimapRenderTexture != null)
        {
            minimapDisplay.texture = minimapRenderTexture;
            minimapDisplay.uvRect = new Rect(0, 0, 1, 1);

            RectTransform rt = minimapDisplay.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        if (truckMarker != null)
        {
            Image img = truckMarker.GetComponent<Image>();
            if (img != null) img.color = truckMarkerColor;
        }

        for (int i = 0; i < dropOffMarkers.Length; i++)
        {
            if (dropOffMarkers[i] != null)
            {
                Image img = dropOffMarkers[i].GetComponent<Image>();
                if (img != null) img.color = dropOffActiveColor;
            }
        }

        Debug.Log($"Minimap effective center: {effectiveCenter}");
    }

    void Update()
    {
        if (minimapCamera == null) return;

        UpdateTruckMarker();
        UpdateDropOffMarkers();
        PulseMarkers();
    }

    private void UpdateTruckMarker()
    {
        if (truckTransform == null || minimapDisplay == null || truckMarker == null) return;

        float minimapSize = minimapDisplay.rectTransform.rect.width;
        float worldToMapScale = (minimapSize * 0.5f) / viewSize;

        // Position relative to effectiveCenter (same point camera looks at)
        float mapX = (truckTransform.position.x - effectiveCenter.x) * worldToMapScale;
        float mapY = (truckTransform.position.z - effectiveCenter.y) * worldToMapScale;

        float maxRange = minimapSize * 0.5f;
        mapX = Mathf.Clamp(mapX, -maxRange, maxRange);
        mapY = Mathf.Clamp(mapY, -maxRange, maxRange);

        truckMarker.localPosition = new Vector3(mapX, mapY, 0);
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

            // Position relative to effectiveCenter (same point camera looks at)
            float mapX = (dropOffPoints[i].position.x - effectiveCenter.x) * worldToMapScale;
            float mapY = (dropOffPoints[i].position.z - effectiveCenter.y) * worldToMapScale;

            dropOffMarkers[i].localPosition = new Vector3(mapX, mapY, 0);

            Image img = dropOffMarkers[i].GetComponent<Image>();
            if (img != null)
            {
                bool delivered = truck != null && i < truck.zoneDelivered.Length && truck.zoneDelivered[i];
                img.color = delivered ? dropOffDeliveredColor : dropOffActiveColor;
            }
        }
    }

    private void PulseMarkers()
    {
        for (int i = 0; i < dropOffMarkers.Length; i++)
        {
            if (dropOffMarkers[i] == null) continue;

            bool delivered = truck != null && i < truck.zoneDelivered.Length && truck.zoneDelivered[i];
            if (delivered)
            {
                dropOffMarkers[i].localScale = Vector3.one;
            }
            else
            {
                float staggeredPulse = Mathf.Lerp(pulseMinScale, pulseMaxScale,
                    (Mathf.Sin(Time.time * pulseSpeed + i * 1.5f) + 1f) * 0.5f);
                dropOffMarkers[i].localScale = Vector3.one * staggeredPulse;
            }
        }
    }

    public void OnDeliveryMade(int deliveryIndex)
    {
        if (deliveryIndex < dropOffMarkers.Length && dropOffMarkers[deliveryIndex] != null)
        {
            Image img = dropOffMarkers[deliveryIndex].GetComponent<Image>();
            if (img != null) img.color = dropOffDeliveredColor;
            dropOffMarkers[deliveryIndex].localScale = Vector3.one;
        }
    }
}
