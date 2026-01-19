using System.Collections.Generic;
using UnityEngine;

public class ProgramManager : MonoBehaviour
{
    // This is your "ProgramList"
    public List<string> program = new List<string>();

    public void AddMove(string move)
    {
        program.Add(move);
        Debug.Log("Added: " + move);
    }

    public void DeleteLastMove()
    {
        if (program.Count > 0)
        {
            program.RemoveAt(program.Count - 1);
            Debug.Log("Removed");
        }
    }

    public void RunProgram()
    {
        Debug.Log("Running program...");
        foreach (string move in program)
        {
            Debug.Log(move);
        }
    }
}