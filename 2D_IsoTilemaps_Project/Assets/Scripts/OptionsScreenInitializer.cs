using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to the Options scene to reconnect buttons to the persistent KeybindManager
/// This fixes the issue where buttons lose their content when returning to the options menu
/// </summary>
public class OptionsSceneInitializer : MonoBehaviour
{
    [Header("Assign Your UI Buttons Here")]
    public Button upButton;
    public Button rightButton;
    public Button leftButton;
    public Button downButton;

    [Header("Assign Button Text Components Here")]
    public TextMeshProUGUI upButtonText;
    public TextMeshProUGUI rightButtonText;
    public TextMeshProUGUI leftButtonText;
    public TextMeshProUGUI downButtonText;

    void Start()
    {
        // If KeybindManager already exists (returning to options), reassign the buttons
        if (KeybindManager.Instance != null)
        {
            KeybindManager.Instance.ReassignButtons(
                upButton, rightButton, leftButton, downButton,
                upButtonText, rightButtonText, leftButtonText, downButtonText
            );

            Debug.Log("Reconnected buttons to existing KeybindManager");
        }
        else
        {
            Debug.LogWarning("No KeybindManager found! Make sure KeybindManager exists in the scene.");
        }
    }
}