using UnityEngine;

public enum PersonalityType { Impulsive, Cautious, Analytical }

[CreateAssetMenu(fileName = "CustomerData", menuName = "CarSales/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string customerName;

    [Header("Money")]
    [SerializeField] private int budget = 20000;
    [SerializeField] private int maxStretch = 2000; // how far above budget they might go

    [Header("Negotiation")]
    [SerializeField] private int patienceMax = 100;
    [SerializeField] private PersonalityType personalityType;
    [SerializeField, Range(0, 100)] private int friendliness = 50;

    [Header("Priorities (0-100)")]
    [SerializeField, Range(0, 100)] private int wantReliability = 70;
    [SerializeField, Range(0, 100)] private int wantSafety = 60;
    [SerializeField, Range(0, 100)] private int wantFuelEconomy = 70;
    [SerializeField, Range(0, 100)] private int wantTechnology = 40;
    [SerializeField, Range(0, 100)] private int wantSpace = 50;
    [SerializeField, Range(0, 100)] private int wantPerformance = 20;

    public string CustomerName => customerName;
    public int Budget => budget;
    public int MaxStretch => maxStretch;

    public int PatienceMax => patienceMax;
    public PersonalityType Personality => personalityType;
    public int Friendliness => friendliness;

    public int WantReliability => wantReliability;
    public int WantSafety => wantSafety;
    public int WantFuelEconomy => wantFuelEconomy;
    public int WantTechnology => wantTechnology;
    public int WantSpace => wantSpace;
    public int WantPerformance => wantPerformance;
}
