using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text modelText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text statsText;

    [Header("Sprite Stack UI Settings")]
    [SerializeField] private RectTransform stackContainer;
    [SerializeField] private GameObject uiLayerPrefab;
    [SerializeField] private float uiHeightScale = 2f;
    [SerializeField] private float stackVerticalOffset = 0f;

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

        ApplyOffset();

        int childCount = stackContainer.childCount;

        for (int i = 0; i < car.stackLayers.Length; i++)
        {
            Image layerImg;

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
            layerImg.SetNativeSize();
            layerImg.rectTransform.anchoredPosition = new Vector2(0, i * uiHeightScale);
        }

        for (int i = car.stackLayers.Length; i < stackContainer.childCount; i++)
            stackContainer.GetChild(i).gameObject.SetActive(false);
    }

    private void ApplyOffset()
    {
        if (stackContainer == null) return;

        // Force this object to ignore the parent VerticalLayoutGroup
        LayoutElement le = stackContainer.GetComponent<LayoutElement>();
        if (le == null) le = stackContainer.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        // Center anchors so it stays horizontally centered on the card
        stackContainer.anchorMin = new Vector2(0.5f, 0.5f);
        stackContainer.anchorMax = new Vector2(0.5f, 0.5f);
        stackContainer.pivot = new Vector2(0.5f, 0.5f);

        // Only change Y, X stays at 0 (centered)
        stackContainer.anchoredPosition = new Vector2(0, stackVerticalOffset);
    }

    private void OnValidate()
    {
        ApplyOffset();
    }

    public void SetSelected(bool selected) => selectedOutline?.SetActive(selected);
}
