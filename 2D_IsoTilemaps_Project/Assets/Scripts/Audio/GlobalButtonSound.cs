using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalButtonSounds : MonoBehaviour
{
    public AudioClip clickSound;
    public float soundDelay = 0.15f;
    private AudioSource audioSource;
    private static GlobalButtonSounds instance;
    
    void Awake()
    {
        // Singleton pattern - only one instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        
        // Add sounds to buttons in current scene
        AddSoundsToAllButtons();
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    // Called when a new scene loads
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Wait a frame for scene to fully load, then add sounds to buttons
        StartCoroutine(AddSoundsNextFrame());
    }
    
    IEnumerator AddSoundsNextFrame()
    {
        yield return null; // Wait one frame
        AddSoundsToAllButtons();
    }
    
    void AddSoundsToAllButtons()
    {
        // Find all buttons in scene
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        
        foreach (Button button in buttons)
        {
            // Remove old listeners to avoid duplicates
            button.onClick.RemoveListener(PlayClickSound);
            // Add new listener
            button.onClick.AddListener(PlayClickSound);
        }
        
        Debug.Log($"Added click sound to {buttons.Length} buttons in scene: {SceneManager.GetActiveScene().name}");
    }
    
    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}