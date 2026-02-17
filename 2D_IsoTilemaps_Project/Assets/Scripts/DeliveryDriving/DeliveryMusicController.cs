using UnityEngine;

public class DeliveryMusicController : MonoBehaviour
{
    [Header("Music Clips")]
    public AudioClip mainLoopClip;
    public AudioClip successSting;
    public AudioClip failureTone;

    [Header("Volume Levels")]
    [Range(0f, 1f)] public float mainVolume = 0.6f;
    [Range(0f, 1f)] public float tensionVolume = 0.75f;

    [Header("Tension")]
    public float tensionThreshold = 30f;
    public float tensionHighPassCutoff = 800f;

    private AudioSource musicSource;
    private AudioSource stingSource;
    private AudioHighPassFilter highPassFilter;
    private DeliveryManager deliveryManager;
    private bool isTensionActive = false;
    private bool isPlaying = false;
    private MusicManager musicManager;

    void Awake()
    {
        // Suppress MusicManager first
        musicManager = FindObjectOfType<MusicManager>();
        if (musicManager != null)
            musicManager.Suppress();

        // Create audio sources in Awake so they exist when DeliveryManager.Start() runs
        AudioListener al = GetComponent<AudioListener>();
        if (al != null) Destroy(al);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = mainVolume;

        stingSource = gameObject.AddComponent<AudioSource>();
        stingSource.playOnAwake = false;
        stingSource.loop = false;
        stingSource.spatialBlend = 0f;

        highPassFilter = gameObject.AddComponent<AudioHighPassFilter>();
        highPassFilter.enabled = false;
        highPassFilter.cutoffFrequency = 10f;
    }

    void Start()
    {
        deliveryManager = FindObjectOfType<DeliveryManager>();
    }

    void Update()
    {
        if (!isPlaying || musicSource == null || deliveryManager == null) return;

        float timeLeft = deliveryManager.shiftTimer;

        if (timeLeft > 0 && timeLeft <= tensionThreshold && !isTensionActive)
            ActivateTension();
        else if (timeLeft > tensionThreshold && isTensionActive)
            DeactivateTension();
    }

    public void StartShiftMusic()
    {
        if (mainLoopClip == null || musicSource == null) return;

        StopAllCoroutines();
        isTensionActive = false;
        isPlaying = true;

        musicSource.clip = mainLoopClip;
        musicSource.loop = true;
        musicSource.volume = mainVolume;
        musicSource.Play();
    }

    public void PlaySuccessMusic()
    {
        isPlaying = false;
        DeactivateTension();
        if (successSting != null)
            stingSource.PlayOneShot(successSting, 0.7f);
    }

    public void PlayFailureMusic()
    {
        isPlaying = false;
        musicSource.Stop();
        DeactivateTension();
        if (failureTone != null)
        {
            stingSource.clip = failureTone;
            stingSource.volume = 0.4f;
            stingSource.Play();
        }
    }

    public void StopMusic()
    {
        isPlaying = false;
        StopAllCoroutines();
        if (musicSource != null) musicSource.Stop();
        if (stingSource != null) stingSource.Stop();
        DeactivateTension();
        if (musicManager != null) musicManager.Unsuppress();
    }

    private void ActivateTension()
    {
        isTensionActive = true;
        musicSource.volume = tensionVolume;
        if (highPassFilter != null)
        {
            highPassFilter.cutoffFrequency = tensionHighPassCutoff;
            highPassFilter.enabled = true;
        }
    }

    private void DeactivateTension()
    {
        isTensionActive = false;
        if (musicSource != null) musicSource.volume = mainVolume;
        if (highPassFilter != null) highPassFilter.enabled = false;
    }

    void OnDestroy()
    {
        if (musicManager != null) musicManager.Unsuppress();
        if (highPassFilter != null) highPassFilter.enabled = false;
    }
}
