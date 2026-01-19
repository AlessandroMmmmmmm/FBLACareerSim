using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProgramManager : MonoBehaviour
{
    public List<string> program = new List<string>();

    [Header("UI")]
    public Transform contentParent;
    public GameObject programLinePrefab;

    private List<GameObject> uiLines = new List<GameObject>();

    public void AddMove(string move)
    {
        program.Add(move);

        GameObject line = Instantiate(programLinePrefab, contentParent);
        line.GetComponentInChildren<TextMeshProUGUI>().text = move;
        Debug.Log(move);
        uiLines.Add(line);
    }
    
    public void DeleteLastMove()
    {
        if (program.Count == 0) return;

        program.RemoveAt(program.Count - 1);

        GameObject lastLine = uiLines[uiLines.Count - 1];
        uiLines.RemoveAt(uiLines.Count - 1);
        Destroy(lastLine);
    }

    public void RunProgram()
    {
        Debug.Log("Running program:");
        foreach (string move in program)
        {
            Debug.Log(move);
        }
    }
}