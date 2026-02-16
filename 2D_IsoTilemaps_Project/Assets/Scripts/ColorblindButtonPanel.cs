using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Alternative UI approach: Individual buttons for each colorblind type
/// Use this if you prefer a button panel instead of cycling arrows
/// </summary>
public class ColorblindButtonPanel : MonoBehaviour
{
    [Header("Button Setup")]
    public Button normalButton;
    public Button protanopiaButton;
    public Button deuteranopiaButton;
    public Button tritanopiaButton;
    public Button protanomalyButton;
    public Button deuteranomalyButton;
    public Button tritanomalyButton;
    public Button achromatopsiaButton;
    public Button achromatomalyButton;

    [Header("Optional: Highlight Selected Button")]
    public Color selectedColor = Color.green;
    public Color normalColor = Color.white;

    private Button currentSelectedButton;

    void Start()
    {
        SetupButton(normalButton, ColorblindManager.ColorblindType.Normal);
        SetupButton(protanopiaButton, ColorblindManager.ColorblindType.Protanopia);
        SetupButton(deuteranopiaButton, ColorblindManager.ColorblindType.Deuteranopia);
        SetupButton(tritanopiaButton, ColorblindManager.ColorblindType.Tritanopia);
        SetupButton(protanomalyButton, ColorblindManager.ColorblindType.Protanomaly);
        SetupButton(deuteranomalyButton, ColorblindManager.ColorblindType.Deuteranomaly);
        SetupButton(tritanomalyButton, ColorblindManager.ColorblindType.Tritanomaly);
        SetupButton(achromatopsiaButton, ColorblindManager.ColorblindType.Achromatopsia);
        SetupButton(achromatomalyButton, ColorblindManager.ColorblindType.Achromatomaly);

        // Highlight the currently selected type
        if (ColorblindManager.Instance != null)
        {
            HighlightButton(GetButtonForType(ColorblindManager.Instance.GetCurrentType()));
        }
    }

    void SetupButton(Button button, ColorblindManager.ColorblindType type)
    {
        if (button != null)
        {
            button.onClick.AddListener(() => OnColorblindTypeSelected(type, button));
        }
    }

    void OnColorblindTypeSelected(ColorblindManager.ColorblindType type, Button button)
    {
        if (ColorblindManager.Instance != null)
        {
            ColorblindManager.Instance.SetColorblindType(type);
            HighlightButton(button);
        }
    }

    void HighlightButton(Button button)
    {
        // Reset previous button
        if (currentSelectedButton != null)
        {
            ColorBlock colors = currentSelectedButton.colors;
            colors.normalColor = normalColor;
            currentSelectedButton.colors = colors;
        }

        // Highlight new button
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = selectedColor;
            button.colors = colors;
            currentSelectedButton = button;
        }
    }

    Button GetButtonForType(ColorblindManager.ColorblindType type)
    {
        switch (type)
        {
            case ColorblindManager.ColorblindType.Normal:
                return normalButton;
            case ColorblindManager.ColorblindType.Protanopia:
                return protanopiaButton;
            case ColorblindManager.ColorblindType.Deuteranopia:
                return deuteranopiaButton;
            case ColorblindManager.ColorblindType.Tritanopia:
                return tritanopiaButton;
            case ColorblindManager.ColorblindType.Protanomaly:
                return protanomalyButton;
            case ColorblindManager.ColorblindType.Deuteranomaly:
                return deuteranomalyButton;
            case ColorblindManager.ColorblindType.Tritanomaly:
                return tritanomalyButton;
            case ColorblindManager.ColorblindType.Achromatopsia:
                return achromatopsiaButton;
            case ColorblindManager.ColorblindType.Achromatomaly:
                return achromatomalyButton;
            default:
                return normalButton;
        }
    }

    void OnDestroy()
    {
        // Clean up all button listeners
        if (normalButton != null) normalButton.onClick.RemoveAllListeners();
        if (protanopiaButton != null) protanopiaButton.onClick.RemoveAllListeners();
        if (deuteranopiaButton != null) deuteranopiaButton.onClick.RemoveAllListeners();
        if (tritanopiaButton != null) tritanopiaButton.onClick.RemoveAllListeners();
        if (protanomalyButton != null) protanomalyButton.onClick.RemoveAllListeners();
        if (deuteranomalyButton != null) deuteranomalyButton.onClick.RemoveAllListeners();
        if (tritanomalyButton != null) tritanomalyButton.onClick.RemoveAllListeners();
        if (achromatopsiaButton != null) achromatopsiaButton.onClick.RemoveAllListeners();
        if (achromatomalyButton != null) achromatomalyButton.onClick.RemoveAllListeners();
    }
}
