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
    public float maxVolume = 0.5f;

    private AudioSource audioSource;
    private static MusicManager instance;
    private float currentTargetVolume;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.loop = true;
            audioSource.playOnAwake = false;

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
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool _suppressed = false;

    public void Suppress()
    {
        _suppressed = true;
        AudioSource src = GetComponent<AudioSource>();
        if (src != null) src.Stop();
    }

    public void Unsuppress()
    {
        _suppressed = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_suppressed) return;
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
            return;

        StopCoroutine("FadeMusic");
        StartCoroutine(FadeMusic(newClip));
    }

    IEnumerator FadeMusic(AudioClip newClip)
    {
        for (float t = 0; t < fadeDuration / 2; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(currentTargetVolume, 0, t / (fadeDuration / 2));
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

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
        audioSource.volume = maxVolume;
        currentTargetVolume = maxVolume;
    }

    public void SetVolume(float volume)
    {
        maxVolume = Mathf.Clamp01(volume);
        audioSource.volume = maxVolume;
        currentTargetVolume = maxVolume;
        PlayerPrefs.SetFloat("MusicVolume", maxVolume);
        PlayerPrefs.Save();
    }

    public float GetVolume() => maxVolume;

    public void Pause() => audioSource.Pause();
    public void Resume() => audioSource.UnPause();

    public static MusicManager Instance => instance;
}
