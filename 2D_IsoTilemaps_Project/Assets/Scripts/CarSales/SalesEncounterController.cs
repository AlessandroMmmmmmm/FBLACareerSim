using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SalesEncounterController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CustomerData customer;
    [SerializeField] private List<CarModelData> inventory = new List<CarModelData>();

    [Header("Car Cards (3)")]
    [SerializeField] private CarCardUI cardA;
    [SerializeField] private CarCardUI cardB;
    [SerializeField] private CarCardUI cardC;

    [Header("Choices (generic tactics you already made)")]
    [SerializeField] private ChoiceData choiceFairAsk;
    [SerializeField] private ChoiceData choiceDiscountAsk;
    [SerializeField] private ChoiceData choicePressureSale;

    [Header("UI")]
    [SerializeField] private TMP_Text customerLineText;
    [SerializeField] private TMP_Text hintText; // "Pick a car first", "Objection: price", etc.
    [SerializeField] private TMP_Text profitText;
    [SerializeField] private TMP_Text dealChanceText;
    [SerializeField] private Slider patienceSlider;

    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private Button choiceCButton;
    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text choiceCText;

    [SerializeField] private Button walkAwayButton;

    [Header("Tuning")]
    [SerializeField] private int turnsPerCustomer = 5;
    [SerializeField, Range(0f, 1f)] private float baseCloseChance = 0.10f;
    [SerializeField] private int earliestCloseTurn = 3; // prevents insta-close on turn 1

    // Runtime state
    private CarModelData selectedCar;
    private List<CarModelData> shownCars = new List<CarModelData>();

    private int turnIndex = 0;
    private int profit = 0;
    private float patience;
    private float patienceMax;
    private float dealChance; // 0..1
    private bool ended;

    private void Start()
    {
        // Basic guards
        if (customer == null) { Debug.LogError("SalesEncounterController: customer not assigned."); enabled = false; return; }
        if (inventory == null || inventory.Count < 3) { Debug.LogError("SalesEncounterController: inventory needs at least 3 cars."); enabled = false; return; }

        // Hook buttons
        choiceAButton.onClick.AddListener(() => OnPickChoice(0));
        choiceBButton.onClick.AddListener(() => OnPickChoice(1));
        choiceCButton.onClick.AddListener(() => OnPickChoice(2));
        if (walkAwayButton) walkAwayButton.onClick.AddListener(() => End(false, "You walked away."));

        StartEncounter();
    }

    private void StartEncounter()
    {
        ended = false;
        selectedCar = null;
        shownCars.Clear();

        profit = 0;
        turnIndex = 0;

        patienceMax = Mathf.Max(1, customer.PatienceMax);
        patience = patienceMax;

        dealChance = baseCloseChance; // will jump once car is selected & fit is computed

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: I'm looking for something that fits my budget and needs.";

        if (hintText)
            hintText.text = "Pick a car to start.";

        RefreshHUD();
        ShowCarsForCustomer();
        SetupChoiceButtonsEnabled(false);
        SetChoiceTexts("—", "—", "—");
    }

    private void ShowCarsForCustomer()
    {
        // Pick 3 cars: one under budget, one near budget, one stretch (if available)
        var candidates = new List<CarModelData>(inventory);
        candidates.Sort((a, b) =>
        {
            int da = Mathf.Abs(a.MSRP - customer.Budget);
            int db = Mathf.Abs(b.MSRP - customer.Budget);
            return da.CompareTo(db);
        });

        // Take top N near budget
        for (int i = 0; i < candidates.Count && shownCars.Count < 3; i++)
        {
            if (!shownCars.Contains(candidates[i]))
                shownCars.Add(candidates[i]);
        }

        // Bind to cards
        cardA.Bind(shownCars[0], OnSelectCar);
        cardB.Bind(shownCars[1], OnSelectCar);
        cardC.Bind(shownCars[2], OnSelectCar);
    }

    private void OnSelectCar(CarModelData car)
    {
        if (ended) return;

        selectedCar = car;

        // Visually select
        cardA.SetSelected(shownCars[0] == car);
        cardB.SetSelected(shownCars[1] == car);
        cardC.SetSelected(shownCars[2] == car);

        // Compute starting chance based on fit + price pressure
        dealChance = Mathf.Clamp01(baseCloseChance + ComputeFitBoost(customer, selectedCar) - ComputePricePenalty(customer, selectedCar));

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: Tell me why {selectedCar.ModelName} is right for me.";

        if (hintText)
            hintText.text = $"Selected: {selectedCar.ModelName} (${selectedCar.MSRP:N0})";

        // Now enable choices and start turn 1
        SetupChoicesForTurn();
        SetupChoiceButtonsEnabled(true);
        RefreshHUD();
    }

    private void SetupChoicesForTurn()
    {
        // For now: reuse your 3 tactic ChoiceData assets each turn.
        // Later you can swap based on objections/turnIndex/customer personality.
        if (choiceFairAsk == null || choiceDiscountAsk == null || choicePressureSale == null)
        {
            Debug.LogError("Assign choiceFairAsk, choiceDiscountAsk, choicePressureSale in Inspector.");
            return;
        }

        SetChoiceTexts(choiceFairAsk.ButtonText, choiceDiscountAsk.ButtonText, choicePressureSale.ButtonText);

        // Set opening line per turn (feels like real back-and-forth)
        if (customerLineText)
        {
            string objection = GetObjectionForTurn(turnIndex);
            customerLineText.text = $"{customer.CustomerName}: {objection}";
        }

        if (hintText)
            hintText.text = $"Turn {turnIndex + 1}/{turnsPerCustomer}";
    }

    private string GetObjectionForTurn(int t)
    {
        // Simple but effective; feels “real”
        return t switch
        {
            0 => "What price are we talking?",
            1 => "I care most about reliability and costs over time.",
            2 => "That still feels expensive. Can you do better?",
            3 => "What about warranty or add-ons?",
            _ => "Convince me this is the right choice."
        };
    }

    private void OnPickChoice(int idx)
    {
        if (ended) return;

        if (selectedCar == null)
        {
            if (hintText) hintText.text = "Pick a car first.";
            return;
        }

        ChoiceData picked = idx switch
        {
            0 => choiceFairAsk,
            1 => choiceDiscountAsk,
            2 => choicePressureSale,
            _ => null
        };

        if (picked == null) return;

        // Apply your existing deltas
        profit += picked.ProfitDelta;
        patience = Mathf.Clamp(patience + picked.PatienceDelta, 0f, patienceMax);

        float personalityMod = PersonalityModifier(customer.Personality, picked);
        dealChance = Mathf.Clamp01(dealChance + picked.DealChanceDelta + personalityMod);

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: {picked.CustomerResponseText}";

        RefreshHUD();

        if (patience <= 0f)
        {
            End(false, "Customer walked away.");
            return;
        }

        // Close check after each turn (but not too early)
        bool canClose = (turnIndex + 1) >= earliestCloseTurn;
        if (canClose && TryCloseDeal())
        {
            End(true, "Deal closed!");
            return;
        }

        // Next turn
        turnIndex++;
        if (turnIndex >= turnsPerCustomer)
        {
            End(false, "They didn’t buy today.");
            return;
        }

        SetupChoicesForTurn();
    }

    private bool TryCloseDeal()
    {
        float roll = Random.value;
        bool success = roll <= dealChance;

        if (hintText)
            hintText.text = success
                ? $"✅ Closed! (chance {(dealChance * 100f):0}%)"
                : $"❌ Not yet… (chance {(dealChance * 100f):0}%)";

        return success;
    }

    private void End(bool success, string message)
    {
        ended = true;
        SetupChoiceButtonsEnabled(false);

        if (customerLineText)
            customerLineText.text = $"Result: {message}";

        // TODO: send profit/xp/reputation to GameManager then load results scene
        Debug.Log($"END success={success} profit={profit} chance={dealChance:0.00}");
    }

    private void RefreshHUD()
    {
        if (profitText) profitText.text = $"Profit: ${profit}";
        if (dealChanceText) dealChanceText.text = $"Close Chance: {(dealChance * 100f):0}%";

        if (patienceSlider)
        {
            patienceSlider.maxValue = patienceMax;
            patienceSlider.value = patience;
        }
    }

    private void SetupChoiceButtonsEnabled(bool enabled)
    {
        if (choiceAButton) choiceAButton.interactable = enabled;
        if (choiceBButton) choiceBButton.interactable = enabled;
        if (choiceCButton) choiceCButton.interactable = enabled;
    }

    private void SetChoiceTexts(string a, string b, string c)
    {
        if (choiceAText) choiceAText.text = a;
        if (choiceBText) choiceBText.text = b;
        if (choiceCText) choiceCText.text = c;
    }

    private float ComputeFitBoost(CustomerData cust, CarModelData car)
    {
        // Weighted match (0..1-ish). Keep simple.
        float wSum =
            cust.WantReliability + cust.WantSafety + cust.WantFuelEconomy +
            cust.WantTechnology + cust.WantSpace + cust.WantPerformance;

        if (wSum <= 0.01f) return 0f;

        float match =
            cust.WantReliability * car.Reliability +
            cust.WantSafety * car.Safety +
            cust.WantFuelEconomy * car.FuelEconomy +
            cust.WantTechnology * car.Technology +
            cust.WantSpace * car.Space +
            cust.WantPerformance * car.Performance;

        // match is 0..(wSum*100). Normalize to 0..1
        float normalized = match / (wSum * 100f);

        // boost range ~ 0..0.35
        return normalized * 0.35f;
    }

    private float ComputePricePenalty(CustomerData cust, CarModelData car)
    {
        int over = car.MSRP - cust.Budget;
        if (over <= 0) return 0f;

        // If within stretch, small penalty; beyond stretch, big penalty
        if (over <= cust.MaxStretch) return 0.08f;
        return 0.20f;
    }

    private float PersonalityModifier(PersonalityType type, ChoiceData picked)
    {
        // Tiny modifiers that make personalities feel real without branching explosion
        // pressure tends to hurt cautious buyers, helps impulsive slightly
        // "discount ask" tends to help cautious, "fair ask" helps analytical
        if (type == PersonalityType.Cautious && picked == choicePressureSale) return -0.05f;
        if (type == PersonalityType.Impulsive && picked == choicePressureSale) return +0.03f;
        if (type == PersonalityType.Analytical && picked == choiceFairAsk) return +0.03f;
        if (type == PersonalityType.Cautious && picked == choiceDiscountAsk) return +0.03f;
        return 0f;
    }
}
