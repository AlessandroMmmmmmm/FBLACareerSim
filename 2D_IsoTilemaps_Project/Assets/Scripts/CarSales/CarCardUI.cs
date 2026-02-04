using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text modelText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text statsText;
    
    [Header("Sprite Stack UI Settings")]
    [SerializeField] private RectTransform stackContainer; // A child UI object to hold layers
    [SerializeField] private GameObject uiLayerPrefab;    // A UI Image prefab
    [SerializeField] private float uiHeightScale = 2f;     // UI uses pixels, so this is usually 1-5

    [SerializeField] private GameObject selectedOutline;
    [SerializeField] private Button button;

    private CarModelData boundCar;
    private System.Action<CarModelData> onClick;

    public void Bind(CarModelData car, System.Action<CarModelData> onClickCallback)
    {
        boundCar = car;
        onClick = onClickCallback;

        if (modelText) modelText.text = car.ModelName;
        if (priceText) priceText.text = $"${car.MSRP:N0}";
        if (statsText) statsText.text = $"Rel: {car.Reliability} | MPG: {car.FuelEconomy}";

        RenderStack(car);

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(boundCar));
        }
    }

    private void RenderStack(CarModelData car)
    {
        if (stackContainer == null || car.stackLayers == null) return;

        // Clear existing layers or reuse them
        int childCount = stackContainer.childCount;
        
        for (int i = 0; i < car.stackLayers.Length; i++)
        {
            Image layerImg;
            
            // Reuse existing image or instantiate new one
            if (i < childCount)
            {
                layerImg = stackContainer.GetChild(i).GetComponent<Image>();
                layerImg.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = Instantiate(uiLayerPrefab, stackContainer);
                layerImg = go.GetComponent<Image>();
            }

            layerImg.sprite = car.stackLayers[i];
            layerImg.SetNativeSize(); // Keeps sprite proportions

            // Stack strictly upward in UI space (anchoredPosition)
            layerImg.rectTransform.anchoredPosition = new Vector2(0, i * uiHeightScale);
        }

        // Hide unused layers if the new car has fewer layers than the previous one
        for (int i = car.stackLayers.Length; i < stackContainer.childCount; i++)
        {
            stackContainer.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void SetSelected(bool selected) => selectedOutline?.SetActive(selected);
}
