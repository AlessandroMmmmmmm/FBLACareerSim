using UnityEngine;

public enum PersonalityType
{
    Impulsive,
    Cautious,
    Analytical
}

[CreateAssetMenu(fileName = "CustomerData", menuName = "CarSales/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string customerName;

    [Header("Negotiation Stats")]
    [SerializeField] private int patienceMax = 100;
    [SerializeField] private int minAcceptPrice = 20000;

    [Header("Personality")]
    [SerializeField] private PersonalityType personalityType;
    [SerializeField, Range(0, 100)] private int friendliness = 50;

    // Accessors
    public string CustomerName => customerName;
    public int PatienceMax => patienceMax;
    public int MinAcceptPrice => minAcceptPrice;
    public PersonalityType Personality => personalityType;
    public int Friendliness => friendliness;
}
