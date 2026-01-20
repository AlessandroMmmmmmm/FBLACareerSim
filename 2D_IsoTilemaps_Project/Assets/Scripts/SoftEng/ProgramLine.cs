using TMPro;
using UnityEngine;

public class ProgramLine : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void SetMove(MoveType move)
    {
        text.text = move.ToString().Replace("MoveForward", "Move Forward")
                                   .Replace("TurnLeft", "Turn Left")
                                   .Replace("TurnRight", "Turn Right");
    }
}