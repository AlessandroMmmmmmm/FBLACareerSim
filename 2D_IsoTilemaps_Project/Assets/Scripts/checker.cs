
using UnityEngine;
using UnityEngine.UI;

public class CheckCanvasScalers : MonoBehaviour
{
    void Start()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);

        foreach (Canvas canvas in allCanvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Debug.Log($"Canvas: {canvas.name}");
                Debug.Log($"  Scale Mode: {scaler.uiScaleMode}");
                Debug.Log($"  Reference Resolution: {scaler.referenceResolution}");
                Debug.Log($"  Match: {scaler.matchWidthOrHeight}");
            }
        }
    }
}
