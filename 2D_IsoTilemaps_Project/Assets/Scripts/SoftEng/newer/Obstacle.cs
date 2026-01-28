using UnityEngine;

// Base class for all obstacles
public abstract class Obstacle : MonoBehaviour
{
    public abstract void OnPlayerEnter(PlayerController player);
    public abstract void OnPlayerExit(PlayerController player);
}
