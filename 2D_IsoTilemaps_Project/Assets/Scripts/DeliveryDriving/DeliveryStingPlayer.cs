using UnityEngine;

/// <summary>
/// Plays success/failure one-shot sounds in the delivery scene.
/// Does NOT touch the main music - MusicManager handles that.
/// Attach to any GameObject in the delivery scene.
/// </summary>
public class DeliveryStingPlayer : MonoBehaviour
{
    [Header("Sting Clips")]
    public AudioClip successSting;
    public AudioClip failureTone;

    [Range(0f, 1f)] public float stingVolume = 0.8f;

    private AudioSource source;

    void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    public void PlaySuccess()
    {
        if (successSting != null)
            source.PlayOneShot(successSting, stingVolume);
    }

    public void PlayFailure()
    {
        if (failureTone != null)
            source.PlayOneShot(failureTone, stingVolume);
    }
}
