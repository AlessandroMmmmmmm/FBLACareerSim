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
    private bool programAborted = false;

    [Header("Cursor Settings")]
    public GameObject cursorIndicator;
    private int cursorPosition = 0;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip buttonClickSound;
    public AudioClip levelCompleteSound;
    public AudioClip collisionFailureSound;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Start()
    {
        Debug.Log("ProgramManager Start() called");

        if (player == null)
        {
            Debug.Log("Player reference is null, trying to find it...");
            player = FindFirstObjectByType<PlayerController>();
            if (player == null)
                Debug.LogError("ProgramManager: No PlayerController found in scene!");
            else
                Debug.Log("Found PlayerController: " + player.gameObject.name);
        }
        else
        {
            Debug.Log("Player reference already assigned: " + player.gameObject.name);
        }

        // Create audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.35f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        // Start background music
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
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

    public void OnMoveDropdownChanged(int _)
    {
        PlayButtonClick();

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

    public void AddSelectedMove()
    {
        if (isRunning) return;

        PlayButtonClick();

        program.Insert(cursorPosition, selectedMove);

        GameObject line = Instantiate(programLinePrefab, contentParent);
        uiLines.Insert(cursorPosition, line);
        line.GetComponent<ProgramLine>().SetMove(selectedMove);

        ProgramLineClickHandler clickHandler = line.GetComponentInChildren<ProgramLineClickHandler>();
        if (clickHandler != null)
            clickHandler.SetLineIndex(cursorPosition);

        cursorPosition++;
        RebuildProgramUI();
    }

    public void DeleteLastMove()
    {
        if (program.Count == 0 || isRunning || cursorPosition == 0) return;

        PlayButtonClick();

        int deleteIndex = cursorPosition - 1;
        program.RemoveAt(deleteIndex);

        GameObject lineToDelete = uiLines[deleteIndex];
        uiLines.RemoveAt(deleteIndex);
        Destroy(lineToDelete);

        cursorPosition--;
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

        PlayButtonClick();

        SoftwareEngScoring scoring = FindFirstObjectByType<SoftwareEngScoring>();
        if (scoring != null)
            scoring.IncrementAttempts();

        programAborted = false;
        StartCoroutine(RunProgramRoutine());
    }

    private System.Collections.IEnumerator RunProgramRoutine()
    {
        isRunning = true;
        bool goalReached = false;
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();

        foreach (MoveType move in program)
        {
            if (programAborted)
            {
                Debug.Log("Program aborted - stopping execution");
                break;
            }

            yield return new WaitUntil(() => !player.IsMoving());

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

            BugObstacle[] bugs = FindObjectsByType<BugObstacle>(FindObjectsSortMode.None);
            foreach (BugObstacle bug in bugs)
                bug.StepForward();

            yield return new WaitForSeconds(stepDelay);

            if (levelManager != null && levelManager.IsLevelComplete())
            {
                goalReached = true;
                PlayLevelComplete();
                break;
            }
        }

        isRunning = false;

        if (!programAborted && !goalReached && levelManager != null)
        {
            Debug.Log("Program finished without reaching goal - resetting player position");
            yield return new WaitForSeconds(0.5f);
            levelManager.ResetPlayerPosition();
        }
        else if (programAborted)
        {
            Debug.Log("Program was aborted - no auto-reset (already handled by collision)");
        }

        programAborted = false;
    }

    public void ClearProgram()
    {
        PlayButtonClick();

        StopAllCoroutines();
        isRunning = false;
        programAborted = false;

        program.Clear();

        foreach (GameObject line in uiLines)
            Destroy(line);
        uiLines.Clear();

        cursorPosition = 0;
        UpdateCursorVisual();
    }

    public void StopProgram()
    {
        StopAllCoroutines();
        isRunning = false;
        programAborted = true;
    }

    public void AbortProgram()
    {
        Debug.Log("ProgramManager: AbortProgram called");
        programAborted = true;
        StopAllCoroutines();
        isRunning = false;
    }

    public void PlayCollisionFailure()
    {
        if (collisionFailureSound != null && sfxSource != null)
            sfxSource.PlayOneShot(collisionFailureSound, 0.7f);
    }

    public void SetCursorPosition(int position)
    {
        cursorPosition = Mathf.Clamp(position, 0, program.Count);
        UpdateCursorVisual();
        Debug.Log($"Cursor moved to position {cursorPosition}");
    }

    public void MoveCursorToEnd()
    {
        cursorPosition = program.Count;
        UpdateCursorVisual();
    }

    public void MoveCursorToStart()
    {
        cursorPosition = 0;
        UpdateCursorVisual();
    }

    void RebuildProgramUI()
    {
        for (int i = 0; i < uiLines.Count; i++)
        {
            uiLines[i].transform.SetSiblingIndex(i);

            ProgramLineClickHandler clickHandler = uiLines[i].GetComponentInChildren<ProgramLineClickHandler>();
            if (clickHandler != null)
                clickHandler.SetLineIndex(i);
            else
                Debug.LogWarning($"No ProgramLineClickHandler found on line {i}");
        }
        UpdateCursorVisual();
    }

    void UpdateCursorVisual()
    {
        if (cursorIndicator == null) return;

        if (cursorPosition == 0)
            cursorIndicator.transform.SetAsFirstSibling();
        else if (cursorPosition >= uiLines.Count)
            cursorIndicator.transform.SetAsLastSibling();
        else
            cursorIndicator.transform.SetSiblingIndex(cursorPosition);
    }

    private void PlayButtonClick()
    {
        if (buttonClickSound != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonClickSound, 0.4f);
    }

    private void PlayLevelComplete()
    {
        if (levelCompleteSound != null && sfxSource != null)
            sfxSource.PlayOneShot(levelCompleteSound, 0.8f);
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public int GetProgramLength()
    {
        return program.Count;
    }
}
