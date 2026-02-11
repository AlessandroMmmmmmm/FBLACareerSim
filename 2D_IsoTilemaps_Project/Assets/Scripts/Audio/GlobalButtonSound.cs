using UnityEngine;
using UnityEngine.UI;

public class GlobalButtonSounds : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;
    
    void Start()
    {
        // Create AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        
        // Find all buttons in scene and add click sound
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(PlayClickSound);
        }
        
        Debug.Log($"Added click sound to {buttons.Length} buttons");
    }
    
    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}