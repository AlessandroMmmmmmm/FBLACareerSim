using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// This component goes on each ProgramLine to make it clickable
// Clicking will set the cursor position AFTER this line
public class ProgramLineClickHandler : MonoBehaviour, IPointerClickHandler
{
    private ProgramManager programManager;
    private int lineIndex = -1;
    
    void Start()
    {
        programManager = FindFirstObjectByType<ProgramManager>();
        if (programManager == null)
        {
            Debug.LogError("ProgramLineClickHandler: Could not find ProgramManager!");
        }
    }
    
    public void SetLineIndex(int index)
    {
        lineIndex = index;
        Debug.Log($"ProgramLineClickHandler: Set line index to {index}");
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"ProgramLineClickHandler: Clicked! Line index = {lineIndex}");
        
        if (programManager != null)
        {
            // Set cursor to position after this line
            programManager.SetCursorPosition(lineIndex + 1);
        }
        else
        {
            Debug.LogError("ProgramLineClickHandler: ProgramManager is null!");
        }
    }
}