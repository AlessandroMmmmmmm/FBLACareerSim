using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use this if you're using TextMeshPro, otherwise use UnityEngine.UI.Text

/// <summary>
/// Persistent keybind manager that survives scene changes
/// Allows rebinding of movement keys and provides input checking for other scripts
/// </summary>
public class KeybindManager : MonoBehaviour
{
    // Singleton instance
    public static KeybindManager Instance { get; private set; }

    [Header("UI Buttons (Optional - only needed in settings scene)")]
    public Button upButton;
    public Button rightButton;
    public Button leftButton;
    public Button downButton;

    [Header("Button Text Components (Optional - only needed in settings scene)")]
    public TextMeshProUGUI upButtonText; // Change to Text if using standard UI Text
    public TextMeshProUGUI rightButtonText;
    public TextMeshProUGUI leftButtonText;
    public TextMeshProUGUI downButtonText;

    [Header("Keybinds")]
    public KeyCode upKey = KeyCode.W;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode downKey = KeyCode.S;

    private bool isWaitingForKey = false;
    private string currentAction = "";

    void Awake()
    {
        // Singleton pattern - only one KeybindManager exists across all scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            LoadKeybinds();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
            return;
        }
    }

    void Start()
    {
        // Only set up buttons if they exist (for settings scene)
        if (upButton != null)
        {
            SetupButtons();
        }
    }

    // Call this method to reassign buttons when returning to options scene
    public void ReassignButtons(Button up, Button right, Button left, Button down,
                                 TextMeshProUGUI upText, TextMeshProUGUI rightText,
                                 TextMeshProUGUI leftText, TextMeshProUGUI downText)
    {
        upButton = up;
        rightButton = right;
        leftButton = left;
        downButton = down;

        upButtonText = upText;
        rightButtonText = rightText;
        leftButtonText = leftText;
        downButtonText = downText;

        SetupButtons();
    }

    void SetupButtons()
    {
        // Add button listeners
        upButton.onClick.AddListener(() => StartRebinding("Up"));
        rightButton.onClick.AddListener(() => StartRebinding("Right"));
        leftButton.onClick.AddListener(() => StartRebinding("Left"));
        downButton.onClick.AddListener(() => StartRebinding("Down"));

        // Initialize button texts
        UpdateButtonText();
    }

    void Update()
    {
        if (isWaitingForKey)
        {
            // Check for any key press
            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        AssignKey(keyCode);
                        break;
                    }
                }
            }
        }
    }

    void StartRebinding(string action)
    {
        isWaitingForKey = true;
        currentAction = action;

        // Update the button text to show waiting state
        switch (action)
        {
            case "Up":
                if (upButtonText != null) upButtonText.text = "Press a key...";
                break;
            case "Right":
                if (rightButtonText != null) rightButtonText.text = "Press a key...";
                break;
            case "Left":
                if (leftButtonText != null) leftButtonText.text = "Press a key...";
                break;
            case "Down":
                if (downButtonText != null) downButtonText.text = "Press a key...";
                break;
        }
    }

    void AssignKey(KeyCode newKey)
    {
        // Assign the new key to the appropriate action
        switch (currentAction)
        {
            case "Up":
                upKey = newKey;
                break;
            case "Right":
                rightKey = newKey;
                break;
            case "Left":
                leftKey = newKey;
                break;
            case "Down":
                downKey = newKey;
                break;
        }

        isWaitingForKey = false;
        currentAction = "";
        UpdateButtonText();
        SaveKeybinds();
    }

    void UpdateButtonText()
    {
        if (upButtonText != null) upButtonText.text = upKey.ToString() + " - Up";
        if (rightButtonText != null) rightButtonText.text = rightKey.ToString() + " - Right";
        if (leftButtonText != null) leftButtonText.text = leftKey.ToString() + " - Left";
        if (downButtonText != null) downButtonText.text = downKey.ToString() + " - Down";
    }

    // Optional: Save and load keybinds
    void SaveKeybinds()
    {
        PlayerPrefs.SetString("UpKey", upKey.ToString());
        PlayerPrefs.SetString("RightKey", rightKey.ToString());
        PlayerPrefs.SetString("LeftKey", leftKey.ToString());
        PlayerPrefs.SetString("DownKey", downKey.ToString());
        PlayerPrefs.Save();
    }

    void LoadKeybinds()
    {
        if (PlayerPrefs.HasKey("UpKey"))
            upKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("UpKey"));
        if (PlayerPrefs.HasKey("RightKey"))
            rightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("RightKey"));
        if (PlayerPrefs.HasKey("LeftKey"))
            leftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LeftKey"));
        if (PlayerPrefs.HasKey("DownKey"))
            downKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DownKey"));
    }

    // Public methods to get input (use these instead of Input.GetAxisRaw)
    public float GetHorizontalInput()
    {
        float horizontal = 0f;
        if (Input.GetKey(rightKey)) horizontal += 1f;
        if (Input.GetKey(leftKey)) horizontal -= 1f;
        return horizontal;
    }

    public float GetVerticalInput()
    {
        float vertical = 0f;
        if (Input.GetKey(upKey)) vertical += 1f;
        if (Input.GetKey(downKey)) vertical -= 1f;
        return vertical;
    }

    // Individual key check methods
    public bool IsUpPressed()
    {
        return Input.GetKey(upKey);
    }

    public bool IsRightPressed()
    {
        return Input.GetKey(rightKey);
    }

    public bool IsLeftPressed()
    {
        return Input.GetKey(leftKey);
    }

    public bool IsDownPressed()
    {
        return Input.GetKey(downKey);
    }
}