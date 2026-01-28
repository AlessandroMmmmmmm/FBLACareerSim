// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// public class ProgramManager : MonoBehaviour
// {
//     private List<GameObject> uiLines = new List<GameObject>();
//     public TMP_Dropdown moveDropdown;

//     public GameObject programLinePrefab;
//     public Transform contentParent;
//     public PlayerController player;
//     public float stepDelay = 0.6f;
//     private List<MoveType> program = new List<MoveType>();
//     private MoveType selectedMove;

// void Start()
// {
//     moveDropdown.ClearOptions();

//     List<string> options = new List<string>();
//     foreach (MoveType move in System.Enum.GetValues(typeof(MoveType)))
//     {
//         options.Add(
//             move.ToString()
//                 .Replace("MoveForward", "Move Forward")
//                 .Replace("TurnLeft", "Turn Left")
//                 .Replace("TurnRight", "Turn Right")
//         );
//     }

//     moveDropdown.AddOptions(options);

//     // 🔥 IMPORTANT FIX
//     moveDropdown.value = 0;
//     moveDropdown.RefreshShownValue();

//     selectedMove = MoveType.MoveForward;
// }

//     // Called by Dropdown → On Value Changed
// public void OnMoveDropdownChanged(int _)
// {
//     string label = moveDropdown.options[moveDropdown.value].text;

//     switch (label)
//     {
//         case "Move Forward":
//             selectedMove = MoveType.MoveForward;
//             break;
//         case "Turn Left":
//             selectedMove = MoveType.TurnLeft;
//             break;
//         case "Turn Right":
//             selectedMove = MoveType.TurnRight;
//             break;
//         case "Wait":
//             selectedMove = MoveType.Wait;
//             break;
//     }

//     Debug.Log("Selected move (label-based): " + selectedMove);
// }

//     // Called by Add button
//     public void AddSelectedMove()
//     {
//         program.Add(selectedMove);
//         GameObject line = Instantiate(programLinePrefab, contentParent);
//         uiLines.Add(line);
//         line.GetComponent<ProgramLine>().SetMove(selectedMove);
//     }


//     public void DeleteLastMove()
//     {
//         if (program.Count == 0) return;

//         program.RemoveAt(program.Count - 1);

//         GameObject lastLine = uiLines[uiLines.Count - 1];
//         uiLines.RemoveAt(uiLines.Count - 1);
//         Destroy(lastLine);
//     }

//     // public void RunProgram()
//     // {
//     //     Debug.Log("Running program:");
//     //     foreach (MoveType move in program)
//     //     {
//     //         Debug.Log(move.ToString());
//     //     }
//     // }

//         public void RunProgram()
//     {
//         StartCoroutine(RunProgramRoutine());
//     }

//     private System.Collections.IEnumerator RunProgramRoutine()
//     {
//         foreach (MoveType move in program)
//         {
//             yield return new WaitUntil(() => !player.IsMoving());

//             switch (move)
//             {
//                 case MoveType.MoveForward:
//                     player.MoveForward();
//                     break;

//                 case MoveType.TurnLeft:
//                     player.TurnLeft();
//                     break;

//                 case MoveType.TurnRight:
//                     player.TurnRight();
//                     break;

//                 case MoveType.Wait:
//                     break;
//             }

//             yield return new WaitForSeconds(stepDelay);
//         }

//     }

// }