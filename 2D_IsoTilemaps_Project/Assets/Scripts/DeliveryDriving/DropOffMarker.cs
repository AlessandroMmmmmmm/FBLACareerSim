using UnityEngine;

/// <summary>
/// Visual marker for drop-off points in the editor
/// Helps you see where delivery locations are while building your city
/// </summary>
public class DropOffMarker : MonoBehaviour
{
    [Header("Marker Settings")]
    public Color markerColor = Color.red;
    public float markerSize = 3f;
    public string locationLabel = "Drop-off 1";
    
    [Header("Visual")]
    public bool showLabel = true;
    public bool showRadius = true;
    public float deliveryRadius = 10f;
    
    private void OnDrawGizmos()
    {
        // Draw marker sphere
        Gizmos.color = markerColor;
        Gizmos.DrawWireSphere(transform.position, markerSize);
        
        // Draw delivery radius
        if (showRadius)
        {
            Gizmos.color = new Color(markerColor.r, markerColor.g, markerColor.b, 0.3f);
            Gizmos.DrawWireSphere(transform.position, deliveryRadius);
        }
        
        // Draw vertical line
        Gizmos.color = markerColor;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 10f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Highlight when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + Vector3.up * 10f, 1f);
        Gizmos.DrawWireSphere(transform.position, deliveryRadius);
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-name the GameObject
        if (!string.IsNullOrEmpty(locationLabel))
        {
            gameObject.name = locationLabel;
        }
    }
#endif
}
