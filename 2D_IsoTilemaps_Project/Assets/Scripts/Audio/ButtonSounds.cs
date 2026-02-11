using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        
        // Create or get AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        
        // Add click listener
        button.onClick.AddListener(PlayClickSound);
    }
    
    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
    
    void OnDestroy()
    {
        // Clean up listener
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }
}