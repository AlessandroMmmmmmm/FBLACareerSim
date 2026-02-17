using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/// <summary>
/// Centralized audio system for Career Quest game
/// Manages music, SFX, and UI sounds with volume control and transitions
/// Singleton pattern - persists across scenes
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mixer;
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;
    public AudioSource ambientSource; // For loops like engine, environment

    [Header("Music Tracks")]
    public AudioClip titleMusic;
    public AudioClip deliveryMusic;
    public AudioClip salesMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;

    [Header("UI Sounds")]
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    public AudioClip toggleSwitch;
    public AudioClip menuOpen;
    public AudioClip menuClose;

    [Header("Delivery Truck Sounds")]
    public AudioClip packagePickup;
    public AudioClip packageDropoff;
    public AudioClip offRoadPenalty;
    public AudioClip timerWarning;
    public AudioClip timerUrgent;
    public AudioClip engineLoop; // Continuous engine sound
    public AudioClip[] starChimes = new AudioClip[5]; // One per star

    [Header("Car Salesman Sounds")]
    public AudioClip doorChime;
    public AudioClip customerPositive;
    public AudioClip customerNeutral;
    public AudioClip customerNegative;
    public AudioClip dealClosed;
    public AudioClip customerLeaves;

    [Header("Settings")]
    public float musicFadeDuration = 1.5f;
    public float defaultSFXVolume = 0.8f;
    public float defaultUIVolume = 0.6f;

    private Coroutine musicFadeCoroutine;
    private bool isEngineRunning = false;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved volumes from PlayerPrefs
        LoadVolumes();
    }

    // ══════════════════════════════════════════════════════════════════════
    // MUSIC CONTROL
    // ══════════════════════════════════════════════════════════════════════

    public void PlayMusic(AudioClip clip, bool loop = true, bool fadeIn = true)
    {
        if (clip == null) return;

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        if (fadeIn && musicSource.isPlaying)
            musicFadeCoroutine = StartCoroutine(CrossfadeMusic(clip, loop));
        else
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic(bool fade = true)
    {
        if (fade)
            musicFadeCoroutine = StartCoroutine(FadeOutMusic());
        else
            musicSource.Stop();
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop)
    {
        // Fade out current
        float startVol = musicSource.volume;
        for (float t = 0; t < musicFadeDuration / 2; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0, t / (musicFadeDuration / 2));
            yield return null;
        }
        musicSource.Stop();

        // Switch and fade in
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();
        for (float t = 0; t < musicFadeDuration / 2; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, startVol, t / (musicFadeDuration / 2));
            yield return null;
        }
        musicSource.volume = startVol;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVol = musicSource.volume;
        for (float t = 0; t < musicFadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0, t / musicFadeDuration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = startVol;
    }

    // ══════════════════════════════════════════════════════════════════════
    // SFX CONTROL
    // ══════════════════════════════════════════════════════════════════════

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume * defaultSFXVolume);
    }

    public void PlaySFXWithPitchVariation(AudioClip clip, float pitchMin = 0.9f, float pitchMax = 1.1f, float volume = 1f)
    {
        if (clip == null) return;
        float originalPitch = sfxSource.pitch;
        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, volume * defaultSFXVolume);
        sfxSource.pitch = originalPitch; // Reset after
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI SOUNDS
    // ══════════════════════════════════════════════════════════════════════

    public void PlayButtonClick() => uiSource.PlayOneShot(buttonClick, defaultUIVolume);
    public void PlayButtonHover() => uiSource.PlayOneShot(buttonHover, defaultUIVolume * 0.5f);
    public void PlayToggle() => uiSource.PlayOneShot(toggleSwitch, defaultUIVolume);
    public void PlayMenuOpen() => uiSource.PlayOneShot(menuOpen, defaultUIVolume);
    public void PlayMenuClose() => uiSource.PlayOneShot(menuClose, defaultUIVolume);

    // ══════════════════════════════════════════════════════════════════════
    // DELIVERY TRUCK SPECIFIC
    // ══════════════════════════════════════════════════════════════════════

    public void StartEngine()
    {
        if (isEngineRunning || engineLoop == null) return;
        ambientSource.clip = engineLoop;
        ambientSource.loop = true;
        ambientSource.Play();
        isEngineRunning = true;
    }

    public void StopEngine()
    {
        if (!isEngineRunning) return;
        ambientSource.Stop();
        isEngineRunning = false;
    }

    public void SetEnginePitch(float speed, float maxSpeed)
    {
        if (!isEngineRunning) return;
        // Map speed to pitch range (0.7 to 1.3)
        float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);
        ambientSource.pitch = Mathf.Lerp(0.7f, 1.3f, normalizedSpeed);
    }

    public void PlayPackagePickup() => PlaySFXWithPitchVariation(packagePickup, 0.95f, 1.05f);
    public void PlayPackageDropoff() => PlaySFX(packageDropoff);
    public void PlayOffRoadPenalty() => PlaySFX(offRoadPenalty);
    public void PlayTimerWarning() => PlaySFX(timerWarning);
    public void PlayTimerUrgent() => PlaySFX(timerUrgent);

    public void PlayStarChimes(int stars)
    {
        StartCoroutine(PlayStarSequence(stars));
    }

    private IEnumerator PlayStarSequence(int stars)
    {
        for (int i = 0; i < stars && i < starChimes.Length; i++)
        {
            if (starChimes[i] != null)
                PlaySFX(starChimes[i], 0.7f);
            yield return new WaitForSecondsRealtime(0.15f); // Short delay between chimes
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // CAR SALESMAN SPECIFIC
    // ══════════════════════════════════════════════════════════════════════

    public void PlayDoorChime() => PlaySFX(doorChime);
    public void PlayCustomerPositive() => PlaySFX(customerPositive);
    public void PlayCustomerNeutral() => PlaySFX(customerNeutral);
    public void PlayCustomerNegative() => PlaySFX(customerNegative);
    public void PlayDealClosed() => PlaySFX(dealClosed);
    public void PlayCustomerLeaves() => PlaySFX(customerLeaves);

    // ══════════════════════════════════════════════════════════════════════
    // VOLUME CONTROL (For Options Menu)
    // ══════════════════════════════════════════════════════════════════════

    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetUIVolume(float volume)
    {
        mixer.SetFloat("UIVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("UIVolume", volume);
    }

    private void LoadVolumes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        float ui = PlayerPrefs.GetFloat("UIVolume", 0.6f);

        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
        SetUIVolume(ui);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPER: SCENE-SPECIFIC MUSIC SHORTCUTS
    // ══════════════════════════════════════════════════════════════════════

    public void PlayTitleMusic() => PlayMusic(titleMusic);
    public void PlayDeliveryMusic() => PlayMusic(deliveryMusic);
    public void PlaySalesMusic() => PlayMusic(salesMusic);
    public void PlayVictoryMusic() => PlayMusic(victoryMusic, loop: false);
    public void PlayDefeatMusic() => PlayMusic(defeatMusic, loop: false);
}
