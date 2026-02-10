using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProgramManager : MonoBehaviour
{
    private List<GameObject> uiLines = new List<GameObject>();
    public TMP_Dropdown moveDropdown;

    public GameObject programLinePrefab;
    public Transform contentParent;
    public PlayerController player;
    public float stepDelay = 0.6f;
    private List<MoveType> program = new List<MoveType>();
    private MoveType selectedMove;
    private bool isRunning = false;
    private bool programAborted = false; // NEW: Track if program was aborted (by bug collision)

    [Header("Cursor Settings")]
    public GameObject cursorIndicator; // Visual indicator for cursor position
    private int cursorPosition = 0; // Where new commands will be inserted (0 = start, program.Count = end)

    void Start()
    {
        Debug.Log("ProgramManager Start() called");

        // Check if player reference is assigned
        if (player == null)
        {
            Debug.Log("Player reference is null, trying to find it...");
            player = FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                Debug.LogError("ProgramManager: No PlayerController found in scene!");
            }
            else
            {
                Debug.Log("Found PlayerController: " + player.gameObject.name);
            }
        }
        else
        {
            Debug.Log("Player reference already assigned: " + player.gameObject.name);
        }

        moveDropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (MoveType move in System.Enum.GetValues(typeof(MoveType)))
        {
            options.Add(
                move.ToString()
                    .Replace("MoveForward", "Move Forward")
                    .Replace("TurnLeft", "Turn Left")
                    .Replace("TurnRight", "Turn Right")
            );
        }

        moveDropdown.AddOptions(options);

        moveDropdown.value = 0;
        moveDropdown.RefreshShownValue();

        selectedMove = MoveType.MoveForward;

        // Initialize cursor
        cursorPosition = 0;
        if (cursorIndicator != null)
        {
            cursorIndicator.SetActive(true);
            UpdateCursorVisual();
        }
        else
        {
            Debug.LogWarning("Cursor Indicator not assigned in ProgramManager!");
        }
    }

    // Called by Dropdown → On Value Changed
    public void OnMoveDropdownChanged(int _)
    {
        string label = moveDropdown.options[moveDropdown.value].text;

        switch (label)
        {
            case "Move Forward":
                selectedMove = MoveType.MoveForward;
                break;
            case "Turn Left":
                selectedMove = MoveType.TurnLeft;
                break;
            case "Turn Right":
                selectedMove = MoveType.TurnRight;
                break;
            case "Wait":
                selectedMove = MoveType.Wait;
                break;
        }

        Debug.Log("Selected move (label-based): " + selectedMove);
    }

    // Called by Add button
    public void AddSelectedMove()
    {
        if (isRunning) return; // Don't allow adding while program is running

        // Insert at cursor position
        program.Insert(cursorPosition, selectedMove);

        // Create UI line
        GameObject line = Instantiate(programLinePrefab, contentParent);
        uiLines.Insert(cursorPosition, line);
        line.GetComponent<ProgramLine>().SetMove(selectedMove);

        // Add click handler if not present
        ProgramLineClickHandler clickHandler = line.GetComponentInChildren<ProgramLineClickHandler>();
        if (clickHandler != null)
        {
            clickHandler.SetLineIndex(cursorPosition);
        }

        // Move cursor forward after inserting
        cursorPosition++;

        // Rebuild UI to maintain correct order
        RebuildProgramUI();
    }

    public void DeleteLastMove()
    {
        if (program.Count == 0 || isRunning || cursorPosition == 0) return;

        // Delete the item before the cursor
        int deleteIndex = cursorPosition - 1;
        program.RemoveAt(deleteIndex);

        GameObject lineToDelete = uiLines[deleteIndex];
        uiLines.RemoveAt(deleteIndex);
        Destroy(lineToDelete);

        // Move cursor back
        cursorPosition--;

        // Rebuild UI
        RebuildProgramUI();
    }

    public void RunProgram()
    {
        Debug.Log("RunProgram called");
        Debug.Log("Player is: " + (player == null ? "NULL" : player.gameObject.name));
        Debug.Log("Program count: " + program.Count);
        Debug.Log("IsRunning: " + isRunning);

        if (player == null)
        {
            Debug.LogError("ProgramManager: Player reference is missing!");
            return;
        }

        if (isRunning || program.Count == 0) return;

        programAborted = false; // Reset abort flag
        StartCoroutine(RunProgramRoutine());
    }

    private System.Collections.IEnumerator RunProgramRoutine()
    {
        isRunning = true;
        bool goalReached = false;
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();

        foreach (MoveType move in program)
        {
            // Check if program was aborted (e.g., by bug collision)
            if (programAborted)
            {
                Debug.Log("Program aborted - stopping execution");
                break;
            }

            yield return new WaitUntil(() => !player.IsMoving());

            // Check again after waiting (in case abort happened during wait)
            if (programAborted)
            {
                Debug.Log("Program aborted during wait - stopping execution");
                break;
            }

            switch (move)
            {
                case MoveType.MoveForward:
                    player.MoveForward();
                    break;

                case MoveType.TurnLeft:
                    player.TurnLeft();
                    break;

                case MoveType.TurnRight:
                    player.TurnRight();
                    break;

                case MoveType.Wait:
                    break;
            }

            // Move all bugs after ANY player action
            BugObstacle[] bugs = FindObjectsByType<BugObstacle>(FindObjectsSortMode.None);
            foreach (BugObstacle bug in bugs)
            {
                bug.StepForward();
            }

            yield return new WaitForSeconds(stepDelay);

            // Check if level complete after each step
            if (levelManager != null && levelManager.IsLevelComplete())
            {
                goalReached = true;
                break;
            }
        }

        isRunning = false;

        // Only reset if program completed normally (not aborted) and goal not reached
        if (!programAborted && !goalReached && levelManager != null)
        {
            Debug.Log("Program finished without reaching goal - resetting player position");
            yield return new WaitForSeconds(0.5f); // Small delay before reset
            levelManager.ResetPlayerPosition();
        }
        else if (programAborted)
        {
            Debug.Log("Program was aborted - no auto-reset (already handled by collision)");
        }

        // Reset abort flag for next run
        programAborted = false;
    }

    public void ClearProgram()
    {
        // Stop any running program first
        StopAllCoroutines();
        isRunning = false;
        programAborted = false;

        program.Clear();

        foreach (GameObject line in uiLines)
        {
            Destroy(line);
        }
        uiLines.Clear();

        cursorPosition = 0;
        UpdateCursorVisual();
    }

    public void StopProgram()
    {
        StopAllCoroutines();
        isRunning = false;
        programAborted = true; // Mark as aborted
    }

    /// <summary>
    /// Call this when player hits a bug to abort the program without auto-reset
    /// </summary>
    public void AbortProgram()
    {
        Debug.Log("ProgramManager: AbortProgram called");
        programAborted = true;
        StopAllCoroutines();
        isRunning = false;
    }

    // Called when user clicks between lines to set cursor position
    public void SetCursorPosition(int position)
    {
        cursorPosition = Mathf.Clamp(position, 0, program.Count);
        UpdateCursorVisual();
        Debug.Log($"Cursor moved to position {cursorPosition}");
    }

    // Move cursor to end
    public void MoveCursorToEnd()
    {
        cursorPosition = program.Count;
        UpdateCursorVisual();
    }

    // Move cursor to start
    public void MoveCursorToStart()
    {
        cursorPosition = 0;
        UpdateCursorVisual();
    }

    // Rebuild the UI to maintain correct hierarchy order
    void RebuildProgramUI()
    {
        for (int i = 0; i < uiLines.Count; i++)
        {
            uiLines[i].transform.SetSiblingIndex(i);

            // Update click handler index (might be on child GameObject)
            ProgramLineClickHandler clickHandler = uiLines[i].GetComponentInChildren<ProgramLineClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.SetLineIndex(i);
            }
            else
            {
                Debug.LogWarning($"No ProgramLineClickHandler found on line {i}");
            }
        }
        UpdateCursorVisual();
    }

    // Update visual indicator of cursor position
    void UpdateCursorVisual()
    {
        if (cursorIndicator == null) return;

        // Position cursor indicator at the correct location
        if (cursorPosition == 0)
        {
            // At the start
            cursorIndicator.transform.SetAsFirstSibling();
        }
        else if (cursorPosition >= uiLines.Count)
        {
            // At the end
            cursorIndicator.transform.SetAsLastSibling();
        }
        else
        {
            // Between items
            cursorIndicator.transform.SetSiblingIndex(cursorPosition);
        }
    }

    /// <summary>
    /// Check if program is currently running
    /// </summary>
    public bool IsRunning()
    {
        return isRunning;
    }
}
