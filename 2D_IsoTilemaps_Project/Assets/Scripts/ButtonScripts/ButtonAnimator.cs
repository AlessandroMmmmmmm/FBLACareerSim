using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Add this to any button to give it smooth hover and click animations
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Color Animation (Optional)")]
    [SerializeField] private bool useColorAnimation = false;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float volume = 0.5f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Color targetColor;
    private bool isHovering = false;
    private bool isPressed = false;
    private AudioSource audioSource;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Get button image for color animation
        buttonImage = GetComponent<Image>();
        if (buttonImage != null && useColorAnimation)
        {
            normalColor = buttonImage.color;
            targetColor = normalColor;
        }

        // Setup audio source if sounds are assigned
        if (hoverSound != null || clickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = volume;
        }
    }

    void Update()
    {
        // Smooth scale animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);

        // Smooth color animation
        if (buttonImage != null && useColorAnimation)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (!isPressed)
        {
            targetScale = originalScale * hoverScale;
            if (useColorAnimation) targetColor = hoverColor;

            // Play hover sound
            if (hoverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (!isPressed)
        {
            targetScale = originalScale;
            if (useColorAnimation) targetColor = normalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetScale = originalScale * clickScale;

        // Play click sound
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if (isHovering)
        {
            targetScale = originalScale * hoverScale;
            if (useColorAnimation) targetColor = hoverColor;
        }
        else
        {
            targetScale = originalScale;
            if (useColorAnimation) targetColor = normalColor;
        }
    }

    // Public method to trigger a "pulse" animation
    public void Pulse()
    {
        StartCoroutine(PulseCoroutine());
    }

    private System.Collections.IEnumerator PulseCoroutine()
    {
        Vector3 original = targetScale;
        targetScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        targetScale = original;
    }
}