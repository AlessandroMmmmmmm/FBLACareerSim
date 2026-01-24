using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NegotiationController : MonoBehaviour
{
    [Header("Customer")]
    [SerializeField] private CustomerData customer;

    [Header("Rounds (set size = totalRounds)")]
    [SerializeField] private RoundChoices[] rounds; // each round must have exactly 3 choices

    [Header("UI")]
    [SerializeField] private TMP_Text customerLineText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text profitText;
    [SerializeField] private TMP_Text dealChanceText;
    [SerializeField] private TMP_Text resultText;          // NEW: shows "Deal closed!" / "Not yet..."
    [SerializeField] private Slider patienceSlider;

    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private Button choiceCButton;

    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text choiceCText;

    [Header("Timing")]
    [SerializeField] private float betweenRoundsDelaySeconds = 0.7f;

    [Header("Deal Logic")]
    [SerializeField, Range(0f, 1f)] private float startingDealChance = 0.35f;
    [SerializeField] private bool allowEarlyClose = true;  // NEW: if true, can close before last round

    // Optional: if you want "must reach at least X chance before closing is possible"
    [SerializeField, Range(0f, 1f)] private float minChanceToClose = 0.15f;

    // runtime
    private int currentRoundIndex = 0;
    private int profit = 0;
    private float patience = 100f;
    private float patienceMax = 100f;
    private float dealChance = 0.35f;
    private bool ended = false;

    private void Awake()
    {
        if (rounds == null || rounds.Length == 0)
            Debug.LogError("NegotiationController: No rounds assigned.");
    }

    private void Start()
    {
        // Buttons must be assigned or you'll get null refs
        if (choiceAButton == null || choiceBButton == null || choiceCButton == null ||
            choiceAText == null || choiceBText == null || choiceCText == null)
        {
            Debug.LogError("NegotiationController: UI references missing. Assign buttons and TMP texts in Inspector.");
            enabled = false;
            return;
        }

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

        patienceMax = (customer != null) ? Mathf.Max(1, customer.PatienceMax) : 100f;
        patience = patienceMax;

        if (customerLineText != null)
        {
            string cname = (customer != null) ? customer.CustomerName : "Customer";
            customerLineText.text = $"{cname}: I'm interested, but I need the right deal.";
        }

        if (resultText != null) resultText.text = "";

        RefreshUI();
        LoadRound(currentRoundIndex);
    }

    private void LoadRound(int roundIndex)
    {

        if (ended) return;
        if (customerLineText != null)
        {
            string cname = (customer != null) ? customer.CustomerName : "Customer";
            customerLineText.text = $"{cname}: What can you offer this time?";
        }

        if (roundIndex < 0 || roundIndex >= rounds.Length)
        {
            EndNegotiation(false, "Negotiation data error.");
            return;
        }

        ChoiceData[] choices = rounds[roundIndex].choices;
        if (choices == null || choices.Length != 3 || choices[0] == null || choices[1] == null || choices[2] == null)
        {
            Debug.LogError($"Round {roundIndex + 1} must have exactly 3 non-null ChoiceData assets.");
            EndNegotiation(false, "Missing choices.");
            return;
        }

        if (roundText != null)
            roundText.text = $"Round {roundIndex + 1}/{rounds.Length}";

        choiceAText.text = choices[0].ButtonText;
        choiceBText.text = choices[1].ButtonText;
        choiceCText.text = choices[2].ButtonText;

        if (resultText != null) resultText.text = ""; // clear per round
        SetChoicesInteractable(true);
    }

    private void OnPickChoice(int index)
    {
        if (ended) return;

        SetChoicesInteractable(false);

        ChoiceData picked = rounds[currentRoundIndex].choices[index];

        // Apply deltas from the choice
        profit += picked.ProfitDelta;
        patience = Mathf.Clamp(patience + picked.PatienceDelta, 0f, patienceMax);
        dealChance = Mathf.Clamp01(dealChance + picked.DealChanceDelta);

        // Show customer response
        if (customerLineText != null)
        {
            string cname = (customer != null) ? customer.CustomerName : "Customer";
            customerLineText.text = $"{cname}: {picked.CustomerResponseText}";
        }

        RefreshUI();

        // Evaluate immediate outcomes (walk-away or deal close)
        StartCoroutine(ResolveChoiceAfterDelay());
    }

    private IEnumerator ResolveChoiceAfterDelay()
    {
        yield return new WaitForSeconds(betweenRoundsDelaySeconds);

        if (ended) yield break;

        // Walk away check
        if (patience <= 0f)
        {
            EndNegotiation(false, "Customer walked away.");
            yield break;
        }

        // Deal close check EACH round
        bool lastRound = currentRoundIndex >= rounds.Length - 1;
        bool canCloseNow = allowEarlyClose || lastRound;

        if (canCloseNow)
        {
            bool closed = TryCloseDeal();
            if (closed)
            {
                EndNegotiation(true, "Deal closed!");
                yield break;
            }
            else
            {
                if (resultText != null) resultText.text = "No deal yet… keep negotiating.";
            }
        }

        // If this was the last round and it didn’t close, end as failure
        if (lastRound)
        {
            EndNegotiation(false, "Negotiation ended with no deal.");
            yield break;
        }

        // Otherwise progress to next round
        currentRoundIndex++;
        LoadRound(currentRoundIndex);
    }

    private bool TryCloseDeal()
    {
        // Optional gate: if chance is extremely low, don't even roll
        if (dealChance < minChanceToClose)
            return false;

        // Roll random
        float roll = Random.value; // 0..1
        bool success = roll <= dealChance;

        // Feedback (helps player understand what happened)
        if (resultText != null)
        {
            string rollStr = roll.ToString("0.00");
            string chanceStr = dealChance.ToString("0.00");
            resultText.text = success
                ? $"Deal closed!)"
                : $" Not yet.)";
        }

        return success;
    }

    private void EndNegotiation(bool success, string message)
    {
        ended = true;

        if (customerLineText != null)
            customerLineText.text = $"Result: {message}";

        SetChoicesInteractable(false);

        Debug.Log($"Ended | success={success} | profit=${profit} | chance={dealChance:0.00} | patience={patience:0}/{patienceMax:0}");
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
    public ChoiceData[] choices = new ChoiceData[3];
}
