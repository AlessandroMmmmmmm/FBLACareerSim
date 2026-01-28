using UnityEngine;

// One-way gate
public class OneWayGateObstacle : Obstacle
{
    public enum GateDirection
    {
        North, East, South, West
    }
    
    public GateDirection allowedDirection = GateDirection.East;
    
    public override void OnPlayerEnter(PlayerController player)
    {
        // Check if player is coming from the correct direction
        // This would require tracking player's previous position
        // For now, just allow passage
    }
    
    public override void OnPlayerExit(PlayerController player)
    {
        // Not used
    }
}
