using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NegotiationController : MonoBehaviour
{
    [Header("Customer")]
    [SerializeField] private CustomerData customer; // assign a CustomerData asset in Inspector

    [Header("Rounds (set size = totalRounds)")]
    [SerializeField] private RoundChoices[] rounds; // each round should have exactly 3 choices

    [Header("UI")]
    [SerializeField] private TMP_Text customerLineText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text profitText;
    [SerializeField] private TMP_Text dealChanceText;
    [SerializeField] private Slider patienceSlider;

    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private Button choiceCButton;

    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text choiceCText;

    [Header("Timing")]
    [SerializeField] private float betweenRoundsDelaySeconds = 0.6f;

    [Header("Win Conditions")]
    [SerializeField, Range(0f, 1f)] private float startingDealChance = 0.35f;
    [SerializeField, Range(0f, 1f)] private float winDealChanceThreshold = 0.65f;

    // runtime state
    private int currentRoundIndex = 0;
    private int profit = 0;
    private float patience = 100f;
    private float patienceMax = 100f;
    private float dealChance = 0.35f;
    private bool ended = false;

    private void Awake()
    {
        // Safety checks help you catch missing references early
        if (customer == null) Debug.LogError("NegotiationController: No CustomerData assigned.");
        if (rounds == null || rounds.Length == 0) Debug.LogError("NegotiationController: No rounds assigned.");
    }

    private void Start()
    {
        // Hook up button listeners once
        choiceAButton.onClick.AddListener(() => OnPickChoice(0));
        choiceBButton.onClick.AddListener(() => OnPickChoice(1));
        choiceCButton.onClick.AddListener(() => OnPickChoice(2));

        StartNegotiation();
    }

    private void StartNegotiation()
    {
        ended = false;
        currentRoundIndex = 0;

        profit = 0;
        dealChance = Mathf.Clamp01(startingDealChance);

        // Pull patience from customer data
        patienceMax = (customer != null) ? Mathf.Max(1, customer.PatienceMax) : 100f;
        patience = patienceMax;

        // Intro line (you can make this data-driven later)
        if (customerLineText != null)
        {
            string cname = (customer != null) ? customer.CustomerName : "Customer";
            customerLineText.text = $"{cname}: I'm interested, but I need the right deal.";
        }

        RefreshUI();
        LoadRound(currentRoundIndex);
    }

    private void LoadRound(int roundIndex)
    {
        if (ended) return;

        if (roundIndex < 0 || rounds == null || roundIndex >= rounds.Length)
        {
            Debug.LogError("NegotiationController: Round index out of range. Check your rounds array size.");
            EndNegotiation(false, "Negotiation data error.");
            return;
        }

        // Expect exactly 3 choices per round
        ChoiceData[] choices = rounds[roundIndex].choices;
        if (choices == null || choices.Length != 3 || choices[0] == null || choices[1] == null || choices[2] == null)
        {
            Debug.LogError($"NegotiationController: Round {roundIndex + 1} must have exactly 3 non-null ChoiceData assets.");
            EndNegotiation(false, "Missing choices.");
            return;
        }

        // Update round label
        if (roundText != null)
            roundText.text = $"Round {roundIndex + 1}/{rounds.Length}";

        // Set button texts
        choiceAText.text = choices[0].ButtonText;
        choiceBText.text = choices[1].ButtonText;
        choiceCText.text = choices[2].ButtonText;

        SetChoicesInteractable(true);
    }

    private void OnPickChoice(int index)
    {
        if (ended) return;

        SetChoicesInteractable(false);

        ChoiceData[] choices = rounds[currentRoundIndex].choices;
        ChoiceData picked = choices[index];

        // Apply deltas
        profit += picked.ProfitDelta;
        patience = Mathf.Clamp(patience + picked.PatienceDelta, 0f, patienceMax);
        dealChance = Mathf.Clamp01(dealChance + picked.DealChanceDelta);

        // Optional: personality effect (simple example)
        ApplyPersonalityModifier(picked);

        // Show customer response
        if (customerLineText != null)
        {
            string cname = (customer != null) ? customer.CustomerName : "Customer";
            customerLineText.text = $"{cname}: {picked.CustomerResponseText}";
        }

        RefreshUI();

        // Advance after a short beat (lets player read response)
        StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        yield return new WaitForSeconds(betweenRoundsDelaySeconds);

        if (ended) yield break;

        if (patience <= 0f)
        {
            EndNegotiation(false, "Customer walked away.");
            yield break;
        }

        // If last round, decide outcome
        bool lastRound = currentRoundIndex >= rounds.Length - 1;
        if (lastRound)
        {
            bool success = dealChance >= winDealChanceThreshold;
            EndNegotiation(success, success ? "Deal closed!" : "No deal this time.");
            yield break;
        }

        currentRoundIndex++;
        LoadRound(currentRoundIndex);
    }

    // Keeps things “simulation-y” without getting complicated.
    // You can delete this if you want purely ChoiceData-driven outcomes.
    private void ApplyPersonalityModifier(ChoiceData picked)
    {
        if (customer == null) return;

        // Example: Impulsive customers hate long haggling (patience drains faster on negative patience choices)
        // Analytical customers respond better to options that increase deal chance.
        // Cautious customers are more sensitive to pressure (negative dealChance choices hurt more).
        switch (customer.Personality)
        {
            case PersonalityType.Impulsive:
                if (picked.PatienceDelta < 0) patience = Mathf.Clamp(patience - 3f, 0f, patienceMax);
                break;

            case PersonalityType.Analytical:
                if (picked.DealChanceDelta > 0f) dealChance = Mathf.Clamp01(dealChance + 0.03f);
                break;

            case PersonalityType.Cautious:
                if (picked.DealChanceDelta < 0f) dealChance = Mathf.Clamp01(dealChance - 0.03f);
                break;
        }
    }

    private void EndNegotiation(bool success, string message)
    {
        ended = true;

        if (customerLineText != null)
            customerLineText.text = $"Result: {message}";

        SetChoicesInteractable(false);

        // Send results to your GameManager (if you have one)
        // Example (uncomment if you made GameManager.Instance like earlier):
        /*
        if (GameManager.Instance != null)
        {
            if (success) GameManager.Instance.money += Mathf.Max(0, profit);
            GameManager.Instance.xp += success ? 100 : 30;
            GameManager.Instance.reputation += success ? 5 : -2;
        }
        */

        Debug.Log($"Negotiation ended | success={success} | profit=${profit} | dealChance={dealChance:0.00} | patience={patience:0}/{patienceMax:0}");
    }

    private void RefreshUI()
    {
        if (profitText != null) profitText.text = $"Profit: ${profit}";
        if (dealChanceText != null) dealChanceText.text = $"Deal Chance: {(dealChance * 100f):0}%";

        if (patienceSlider != null)
        {
            patienceSlider.maxValue = patienceMax;
            patienceSlider.value = patience;
        }
    }

    private void SetChoicesInteractable(bool value)
    {
        if (choiceAButton != null) choiceAButton.interactable = value;
        if (choiceBButton != null) choiceBButton.interactable = value;
        if (choiceCButton != null) choiceCButton.interactable = value;
    }
}

[System.Serializable]
public class RoundChoices
{
    // Must be EXACTLY 3 ChoiceData assets in each round for this controller.
    public ChoiceData[] choices = new ChoiceData[3];
}
