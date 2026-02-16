using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use this if you're using TextMeshPro
// using UnityEngine.UI.Text; // Use this if you're using standard UI Text

public class ColorblindUI : MonoBehaviour
{
    [Header("UI References")]
    public Button previousButton;
    public Button nextButton;
    public TextMeshProUGUI displayText; // Change to Text if not using TextMeshPro
    
    private ColorblindManager.ColorblindType[] allTypes;
    private int currentIndex = 0;

    void Start()
    {
        // Get all colorblind types
        allTypes = (ColorblindManager.ColorblindType[])System.Enum.GetValues(
            typeof(ColorblindManager.ColorblindType)
        );

        // Load current setting
        if (ColorblindManager.Instance != null)
        {
            ColorblindManager.ColorblindType currentType = ColorblindManager.Instance.GetCurrentType();
            currentIndex = System.Array.IndexOf(allTypes, currentType);
        }

        // Setup button listeners
        if (previousButton != null)
            previousButton.onClick.AddListener(OnPreviousClicked);
        
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);

        UpdateDisplay();
    }

    void OnPreviousClicked()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = allTypes.Length - 1;
        
        ApplyColorblindType();
        UpdateDisplay();
    }

    void OnNextClicked()
    {
        currentIndex++;
        if (currentIndex >= allTypes.Length)
            currentIndex = 0;
        
        ApplyColorblindType();
        UpdateDisplay();
    }

    void ApplyColorblindType()
    {
        if (ColorblindManager.Instance != null)
        {
            ColorblindManager.Instance.SetColorblindType(allTypes[currentIndex]);
        }
    }

    void UpdateDisplay()
    {
        if (displayText != null)
        {
            string typeName = ColorblindManager.GetColorblindTypeName(allTypes[currentIndex]);
            displayText.text = typeName;
        }
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (previousButton != null)
            previousButton.onClick.RemoveListener(OnPreviousClicked);
        
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextClicked);
    }
}
