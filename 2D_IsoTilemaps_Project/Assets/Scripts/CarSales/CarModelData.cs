using UnityEngine;

public enum CarTag
{
    Sedan,
    SUV,
    Truck,
    Coupe,
    Hatchback,
    EV,
    Hybrid
}

[CreateAssetMenu(fileName = "CarModel", menuName = "CarSales/Car Model")]
public class CarModelData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string modelName;
    [SerializeField] private CarTag tag;

    [Header("Pricing")]
    [SerializeField] private int msrp;

    [Header("Ratings (0-100)")]
    [SerializeField, Range(0, 100)] private int reliability = 50;
    [SerializeField, Range(0, 100)] private int safety = 50;
    [SerializeField, Range(0, 100)] private int fuelEconomy = 50;
    [SerializeField, Range(0, 100)] private int technology = 50;
    [SerializeField, Range(0, 100)] private int space = 50;
    [SerializeField, Range(0, 100)] private int performance = 50;

    [Header("Optional Visual")]
    [SerializeField] private Sprite image;

    public string ModelName => modelName;
    public CarTag Tag => tag;
    public int MSRP => msrp;

    public int Reliability => reliability;
    public int Safety => safety;
    public int FuelEconomy => fuelEconomy;
    public int Technology => technology;
    public int Space => space;
    public int Performance => performance;

    // Inside your CarModelData.cs
    [Header("Visuals")]
    public Sprite[] stackLayers; // Drag the slices for THIS specific car here


}