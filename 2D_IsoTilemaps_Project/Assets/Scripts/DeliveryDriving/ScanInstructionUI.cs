using UnityEngine;
using TMPro;

/// <summary>
/// Attach this to your loading zone / scan minigame GameObject.
/// Displays a "Hold to scan" instruction when the player is in range.
/// Call ShowInstruction() when player enters the zone,
/// HideInstruction() when they leave or scanning completes.
/// </summary>
public class ScanInstructionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionPanel; // Panel containing the instruction text
    public TextMeshProUGUI instructionText; // The text element
    public TextMeshProUGUI subText; // Optional smaller sub-text

    [Header("Instruction Content")]
    [TextArea]
    public string mainInstruction = "Hold LEFT CLICK to scan package";
    [TextArea]
    public string subInstruction = "Keep held until scan completes";

    [Header("Text Colors")]
    public Color mainTextColor = Color.white;
    public Color subTextColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Auto-Show When Player Enters")]
    [Tooltip("If true, shows instruction automatically when Player tag enters trigger")]
    public bool autoShowOnTrigger = true;

    void Start()
    {
        // Hide at start
        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        // Apply colors
        if (instructionText != null)
        {
            instructionText.text = mainInstruction;
            instructionText.color = mainTextColor;
        }

        if (subText != null)
        {
            subText.text = subInstruction;
            subText.color = subTextColor;
        }
    }

    public void ShowInstruction()
    {
        if (instructionPanel != null)
            instructionPanel.SetActive(true);
    }

    public void HideInstruction()
    {
        if (instructionPanel != null)
            instructionPanel.SetActive(false);
    }

    // Optional: auto-trigger when truck drives into zone
    private void OnTriggerEnter(Collider other)
    {
        if (autoShowOnTrigger && other.CompareTag("Player"))
            ShowInstruction();
    }

    private void OnTriggerExit(Collider other)
    {
        if (autoShowOnTrigger && other.CompareTag("Player"))
            HideInstruction();
    }
}
