using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneMusic
    {
        public int sceneIndex;
        public AudioClip musicClip;
    }
    
    public SceneMusic[] sceneMusicList; // Assign music for each scene
    public float fadeDuration = 1f; // How long to fade between tracks
    
    private AudioSource audioSource;
    private static MusicManager instance;
    
    void Awake()
    {
        // Singleton - only one music manager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.buildIndex);
    }
    
    void PlayMusicForScene(int sceneIndex)
    {
        // Find music for this scene
        AudioClip newClip = null;
        foreach (SceneMusic sm in sceneMusicList)
        {
            if (sm.sceneIndex == sceneIndex)
            {
                newClip = sm.musicClip;
                break;
            }
        }
        
        // If no music assigned for this scene, stop music
        if (newClip == null)
        {
            StopCoroutine("FadeMusic");
            StartCoroutine(FadeOut());
            return;
        }
        
        // If same music is already playing, don't restart
        if (audioSource.clip == newClip && audioSource.isPlaying)
        {
            return;
        }
        
        // Change to new music
        StopCoroutine("FadeMusic");
        StartCoroutine(FadeMusic(newClip));
    }
    
    IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out current music
        float startVolume = audioSource.volume;
        
        for (float t = 0; t < fadeDuration / 2; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / (fadeDuration / 2));
            yield return null;
        }
        
        // Change music
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();
        
        // Fade in new music
        for (float t = 0; t < fadeDuration / 2; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, startVolume, t / (fadeDuration / 2));
            yield return null;
        }
        
        audioSource.volume = startVolume;
    }
    
    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
    
    // Public methods to control music
    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
    
    public void Pause()
    {
        audioSource.Pause();
    }
    
    public void Resume()
    {
        audioSource.UnPause();
    }
}