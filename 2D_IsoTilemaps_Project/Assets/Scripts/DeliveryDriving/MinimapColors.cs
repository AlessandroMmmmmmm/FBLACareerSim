using UnityEngine;

/// <summary>
/// Assign this to road and grass tiles after generation
/// so the minimap camera can render them in distinct flat colors.
///
/// How to use:
/// - RoadGenerator will call SetupMinimapColors() automatically after generating
/// - You can also call it manually from the editor
/// </summary>
public class MinimapColors : MonoBehaviour
{
    [Header("Minimap Layer Colors")]
    public Color roadColor = new Color(0.25f, 0.27f, 0.3f); // Dark asphalt
    public Color grassColor = new Color(0.15f, 0.28f, 0.15f); // Dark green
    public Color buildingColor = new Color(0.3f, 0.28f, 0.25f); // Warm gray
    
    /// <summary>
    /// Call after GenerateRoadNetwork() to apply flat colors to all tiles
    /// for better minimap readability
    /// </summary>
    public void ApplyMinimapColors()
    {
        GameObject roadsParent = GameObject.Find("Roads_Generated");
        if (roadsParent == null) return;
        
        foreach (Transform child in roadsParent.transform)
        {
            string name = child.name.ToLower();
            Color targetColor;
            
            if (name.Contains("road"))
            {
                targetColor = roadColor;
            }
            else if (name.Contains("grass"))
            {
                targetColor = grassColor;
            }
            else if (name.Contains("building"))
            {
                targetColor = buildingColor;
            }
            else continue;
            
            // Apply color to all renderers in this object
            foreach (Renderer r in child.GetComponentsInChildren<Renderer>())
            {
                // Create a material instance to avoid modifying the shared asset
                Material mat = r.material;
                mat.color = targetColor;
            }
        }
    }
}
