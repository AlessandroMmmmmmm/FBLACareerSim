using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PackageScanner : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform barcode;
    public Slider progressBar;
    public Image fillImage;
    public WarehouseManager manager;

    [Header("Settings")]
    public float moveSpeed = 450f;
    public float scanRequirement = 0.8f;
    public Color failColor = Color.red;

    [Header("Audio")]
    public AudioClip missSound; // Drag your "miss" sound here
    public AudioClip successSound; // Drag your "success scan" sound here

    private bool waitingForRelease = false;
    private bool isResetting = false;
    private float currentProgress = 0f;
    private float internalTimer = 0f;
    private bool isLocked = false;
    private AudioSource audioSource;

    void Start()
    {
        // Create audio source for SFX
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    void Update()
    {
        if (isResetting || isLocked) return;

        bool isUserHolding = Input.GetMouseButton(0);

        if (waitingForRelease)
        {
            if (!isUserHolding) waitingForRelease = false;
            MoveBarcode();
            return;
        }

        if (!isUserHolding)
        {
            MoveBarcode();
            currentProgress = 0;
            progressBar.value = 0;
        }
        else
        {
            if (Mathf.Abs(barcode.anchoredPosition.x) < 35f)
            {
                currentProgress += Time.deltaTime;
                progressBar.value = currentProgress / scanRequirement;

                if (currentProgress >= scanRequirement)
                    StartCoroutine(HandleSuccess());
            }
            else
            {
                StartCoroutine(HandleFailure());
            }
        }
    }

    void MoveBarcode()
    {
        internalTimer += Time.deltaTime;
        float movement = Mathf.PingPong(internalTimer * moveSpeed, 340) - 170;
        barcode.anchoredPosition = new Vector2(movement, 0);
    }

    IEnumerator HandleFailure()
    {
        isLocked = true;
        currentProgress = 0;
        progressBar.value = 1;

        if (fillImage != null) fillImage.color = failColor;

        // Play miss sound
        if (missSound != null && audioSource != null)
            audioSource.PlayOneShot(missSound, 0.6f);

        Debug.Log("Scanner Error: Please wait 3 seconds for system reset.");

        yield return new WaitForSeconds(3.0f);

        if (fillImage != null) fillImage.color = Color.white;
        progressBar.value = 0;
        isLocked = false;
    }

    IEnumerator HandleSuccess()
    {
        isResetting = true;
        waitingForRelease = true;

        progressBar.value = 1;
        if (fillImage != null) fillImage.color = Color.green;

        // Play success sound
        if (successSound != null && audioSource != null)
            audioSource.PlayOneShot(successSound, 0.7f);

        manager.OnScanPackage();

        yield return new WaitForSeconds(0.3f);

        barcode.gameObject.SetActive(false);
        if (fillImage != null) fillImage.color = Color.white;
        progressBar.value = 0;

        yield return new WaitForSeconds(0.15f);

        if (manager.packagesScanned < manager.requiredPackages)
        {
            internalTimer = Random.Range(0f, 10f);
            barcode.gameObject.SetActive(true);
            isResetting = false;
        }
    }
}
