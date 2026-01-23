using UnityEngine;

[CreateAssetMenu(fileName = "ChoiceData", menuName = "CarSales/Choice Data")]
public class ChoiceData : ScriptableObject
{
    [Header("UI Text")]
    [SerializeField] private string buttonText;
    [SerializeField, TextArea] private string customerResponseText;

    [Header("Stat Changes")]
    [SerializeField] private int profitDelta;          // dollars
    [SerializeField] private int patienceDelta;        // +/- patience points
    [SerializeField, Range(-1f, 1f)] private float dealChanceDelta; // change in 0..1 scale

    // Read-only accessors (cleaner than public fields)
    public string ButtonText => buttonText;
    public string CustomerResponseText => customerResponseText;
    public int ProfitDelta => profitDelta;
    public int PatienceDelta => patienceDelta;
    public float DealChanceDelta => dealChanceDelta;
}
