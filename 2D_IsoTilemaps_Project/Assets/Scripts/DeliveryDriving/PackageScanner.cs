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
    public Color failColor = Color.red; // Color for the penalty
    private bool waitingForRelease = false;
    private bool isResetting = false;
    private float currentProgress = 0f;
    private float internalTimer = 0f;
    private bool isLocked = false; // New: Prevents action during penalty

    void Update()
    {
        // If we are resetting or in a 3s penalty, do nothing
        if (isResetting || isLocked) return;

        bool isUserHolding = Input.GetMouseButton(0);

        // Force release check from previous success
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
            // CHECK FOR OVERLAP
            if (Mathf.Abs(barcode.anchoredPosition.x) < 35f)
            {
                currentProgress += Time.deltaTime;
                progressBar.value = currentProgress / scanRequirement;

                if (currentProgress >= scanRequirement)
                    StartCoroutine(HandleSuccess());
            }
            else
            {
                // FAILURE: Trigger the 3-second lockout
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
        progressBar.value = 1; // Fill the bar to show the error color

        if (fillImage != null) fillImage.color = failColor;

        Debug.Log("Scanner Error: Please wait 3 seconds for system reset.");

        // Wait for the penalty duration
        yield return new WaitForSeconds(3.0f);

        // Reset to normal
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

        // Just tell the manager to increment the count
        manager.OnScanPackage();

        yield return new WaitForSeconds(0.3f);

        // Reset visuals for the NEXT box (if there is one)
        barcode.gameObject.SetActive(false);
        if (fillImage != null) fillImage.color = Color.white;
        progressBar.value = 0;

        yield return new WaitForSeconds(0.15f);

        // Only bring the barcode back if we still need more scans
        if (manager.packagesScanned < manager.requiredPackages)
        {
            internalTimer = Random.Range(0f, 10f);
            barcode.gameObject.SetActive(true);
            isResetting = false;
        }
    }

}
