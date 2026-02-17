using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneMusic
    {
        public int sceneIndex;
        public AudioClip musicClip;
    }

    public SceneMusic[] sceneMusicList;
    public float fadeDuration = 1f;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float maxVolume = 0.5f; // Adjustable max volume (default 50%)

    private AudioSource audioSource;
    private static MusicManager instance;
    private float currentTargetVolume; // Tracks what volume we're fading to

    void Awake()
    {
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

            // Load saved volume or use default
            maxVolume = PlayerPrefs.GetFloat("MusicVolume", maxVolume);
            audioSource.volume = maxVolume;
            currentTargetVolume = maxVolume;

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
        AudioClip newClip = null;
        foreach (SceneMusic sm in sceneMusicList)
        {
            if (sm.sceneIndex == sceneIndex)
            {
                newClip = sm.musicClip;
                break;
            }
        }

        if (newClip == null)
        {
            StopCoroutine("FadeMusic");
            StartCoroutine(FadeOut());
            return;
        }

        if (audioSource.clip == newClip && audioSource.isPlaying)
        {
            return;
        }

        StopCoroutine("FadeMusic");
        StartCoroutine(FadeMusic(newClip));
    }

    IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out current music
        for (float t = 0; t < fadeDuration / 2; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(currentTargetVolume, 0, t / (fadeDuration / 2));
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in new music to maxVolume
        for (float t = 0; t < fadeDuration / 2; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, maxVolume, t / (fadeDuration / 2));
            yield return null;
        }

        audioSource.volume = maxVolume;
        currentTargetVolume = maxVolume;
    }

    IEnumerator FadeOut()
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(currentTargetVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = maxVolume; // Reset to max for next track
        currentTargetVolume = maxVolume;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUBLIC VOLUME CONTROL (For Options Menu Slider)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Set music volume (0-1 range). Call this from Options menu slider.
    /// </summary>
    public void SetVolume(float volume)
    {
        maxVolume = Mathf.Clamp01(volume);
        audioSource.volume = maxVolume;
        currentTargetVolume = maxVolume;
        PlayerPrefs.SetFloat("MusicVolume", maxVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Get current music volume setting
    /// </summary>
    public float GetVolume()
    {
        return maxVolume;
    }

    public void Pause()
    {
        audioSource.Pause();
    }

    public void Resume()
    {
        audioSource.UnPause();
    }

    // Static accessor for easy access from anywhere
    public static MusicManager Instance => instance;
}
