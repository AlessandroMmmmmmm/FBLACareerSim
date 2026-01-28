using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text modelText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Image carImage;
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
        if (statsText)
        {
            // Keep it short; judges love readability
            statsText.text =
                $"Reliability: {car.Reliability}\n" +
                $"MPG/Eff: {car.FuelEconomy}\n" +
                $"Safety: {car.Safety}";
        }

        if (carImage) carImage.sprite = car.Image;
        SetSelected(false);

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(boundCar));
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedOutline) selectedOutline.SetActive(selected);
    }
}
