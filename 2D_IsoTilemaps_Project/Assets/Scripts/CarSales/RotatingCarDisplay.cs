using UnityEngine;
public class SpriteStackRotator : MonoBehaviour
{
    public float heightScale = 0.0625f;
    public float rotationSpeed = 50f;

    // Call this from the SalesEncounterController
    public void SetCarModel(CarModelData carData)
    {
        if (carData == null || carData.stackLayers == null) return;

        Sprite[] sprites = carData.stackLayers;
        int i = 0;

        foreach (Transform child in transform)
        {
            if (i < sprites.Length)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = sprites[i];
                child.gameObject.SetActive(true); // Show used layers
            }
            else
            {
                child.gameObject.SetActive(false); // Hide extra layers if this car has fewer
            }
            i++;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        int index = 0;
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf) continue;

            // World-space stacking as we discussed
            child.position = new Vector3(
                transform.position.x,
                transform.position.y + (index * heightScale),
                transform.position.z + (index * 0.01f)
            );
            index++;
        }
    }
}