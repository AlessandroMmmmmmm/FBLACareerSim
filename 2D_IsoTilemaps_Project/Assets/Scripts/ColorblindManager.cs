using UnityEngine;

public class ColorblindManager : MonoBehaviour
{
    public enum ColorblindType
    {
        Normal = 0,
        Protanopia = 1,        // Red-blind
        Deuteranopia = 2,      // Green-blind
        Tritanopia = 3,        // Blue-blind
        Protanomaly = 4,       // Red-weak
        Deuteranomaly = 5,     // Green-weak
        Tritanomaly = 6,       // Blue-weak
        Achromatopsia = 7,     // Total color blindness
        Achromatomaly = 8      // Blue cone monochromacy
    }

    public Material colorblindMaterial;
    private ColorblindType currentType = ColorblindType.Normal;

    private static ColorblindManager instance;

    void Awake()
    {
        // Singleton pattern to persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Load saved preference
            int savedType = PlayerPrefs.GetInt("ColorblindType", 0);
            SetColorblindType((ColorblindType)savedType);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (colorblindMaterial != null && currentType != ColorblindType.Normal)
        {
            Graphics.Blit(source, destination, colorblindMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    public void SetColorblindType(ColorblindType type)
    {
        currentType = type;
        if (colorblindMaterial != null)
        {
            colorblindMaterial.SetInt("_ColorblindType", (int)type);
        }
        
        // Save preference
        PlayerPrefs.SetInt("ColorblindType", (int)type);
        PlayerPrefs.Save();
    }

    public ColorblindType GetCurrentType()
    {
        return currentType;
    }

    public static ColorblindManager Instance
    {
        get { return instance; }
    }

    public static string GetColorblindTypeName(ColorblindType type)
    {
        switch (type)
        {
            case ColorblindType.Normal:
                return "Normal Vision";
            case ColorblindType.Protanopia:
                return "Protanopia (Red-blind)";
            case ColorblindType.Deuteranopia:
                return "Deuteranopia (Green-blind)";
            case ColorblindType.Tritanopia:
                return "Tritanopia (Blue-blind)";
            case ColorblindType.Protanomaly:
                return "Protanomaly (Red-weak)";
            case ColorblindType.Deuteranomaly:
                return "Deuteranomaly (Green-weak)";
            case ColorblindType.Tritanomaly:
                return "Tritanomaly (Blue-weak)";
            case ColorblindType.Achromatopsia:
                return "Achromatopsia (Total)";
            case ColorblindType.Achromatomaly:
                return "Achromatomaly (Partial)";
            default:
                return "Unknown";
        }
    }
}
