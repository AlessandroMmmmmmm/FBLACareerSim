using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SalesEncounterController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CustomerData customer;
    [SerializeField] private List<CustomerData> customers = new List<CustomerData>();
    [SerializeField] private List<CarModelData> inventory = new List<CarModelData>();

    [Header("Car Cards (3)")]
    [SerializeField] private CarCardUI cardA;
    [SerializeField] private CarCardUI cardB;
    [SerializeField] private CarCardUI cardC;

    [Header("Choices")]
    [SerializeField] private ChoiceData choiceFairAsk;
    [SerializeField] private ChoiceData choiceDiscountAsk;
    [SerializeField] private ChoiceData choicePressureSale;

    [Header("UI")]
    [SerializeField] private TMP_Text customerLineText;
    [SerializeField] private TMP_Text hintText;
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
    [SerializeField] private TMP_Text walkAwayText;

    [Header("Salesperson Speech Bubble")]
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private TMP_Text speechBubbleText;
    [SerializeField] private float speechDisplayTime = 1.5f;

    [Header("Tuning")]
    [SerializeField] private int turnsPerCustomer = 5;
    [SerializeField, Range(0f, 1f)] private float baseCloseChance = 0.10f;
    [SerializeField] private int earliestCloseTurn = 3;

    [Header("Rounds")]
    [SerializeField] private int roundsTotal = 3;
    [SerializeField] private float nextRoundDelay = 1.5f;

    [Header("References")]
    [SerializeField] private SpriteStackRotator carDisplay;

    [Header("Dynamic Text Colors")]
    [SerializeField] private Color profitPositiveColor = new Color(0.48f, 0.88f, 0.54f); // #7AE08A green
    [SerializeField] private Color profitNeutralColor = new Color(0.94f, 0.93f, 0.91f); // #F0EDE8 off-white
    [SerializeField] private Color profitNegativeColor = new Color(0.88f, 0.48f, 0.48f); // #E07A7A red

    [SerializeField] private Color chanceHighColor = new Color(0.48f, 0.88f, 0.54f); // #7AE08A green
    [SerializeField] private Color chanceMidColor = new Color(0.96f, 0.84f, 0.48f); // #F5D67A yellow
    [SerializeField] private Color chanceLowColor = new Color(0.88f, 0.48f, 0.48f); // #E07A7A red
    [SerializeField] private Image backgroundImage; // Drag your background UI Image here
    [SerializeField] private float colorLerpSpeed = 2f; // How fast it transitions

    [SerializeField] private Color colorPickingCar = new Color(0.13f, 0.17f, 0.25f); // Dark blue - waiting
    [SerializeField] private Color colorNegotiating = new Color(0.15f, 0.22f, 0.18f); // Dark green - active deal
    [SerializeField] private Color colorThinking = new Color(0.20f, 0.18f, 0.10f); // Dark amber - suspense
    [SerializeField] private Color colorDealClosed = new Color(0.10f, 0.28f, 0.12f); // Bright green - success
    [SerializeField] private Color colorCustomerLeft = new Color(0.28f, 0.10f, 0.10f); // Dark red - failure
    [SerializeField] private Color colorGameOver = new Color(0.08f, 0.08f, 0.12f); // Near black - end

    private Color bgTargetColor;

    // Runtime state - per customer
    private CarModelData selectedCar;
    private readonly List<CarModelData> shownCars = new List<CarModelData>();
    private int currentOfferPrice;

    private int turnIndex = 0;
    private int profit = 0;
    private float patience;
    private float patienceMax;
    private float dealChance;
    private bool ended;

    // Runtime state - overall game
    private int currentRound = 1;
    private int totalProfit = 0;
    private int successfulSales = 0;

    private void Start()
    {
        if (customer == null) { Debug.LogError("SalesEncounterController: customer not assigned."); enabled = false; return; }
        if (inventory == null || inventory.Count < 3) { Debug.LogError("SalesEncounterController: inventory needs at least 3 cars."); enabled = false; return; }
        roundsTotal = Mathf.Min(roundsTotal, customers.Count);
        if (customers.Count < roundsTotal)
            Debug.LogWarning($"Not enough customers for {roundsTotal} rounds. Customers={customers.Count}");

        choiceAButton.onClick.AddListener(() => OnPickChoice(0));
        choiceBButton.onClick.AddListener(() => OnPickChoice(1));
        choiceCButton.onClick.AddListener(() => OnPickChoice(2));
        if (walkAwayButton) walkAwayButton.onClick.AddListener(() => End(false, "You walked away."));

        // Start at picking color
        if (backgroundImage != null)
            backgroundImage.color = colorPickingCar;
        bgTargetColor = colorPickingCar;

        StartEncounter();
    }

    private void Update()
    {
        // Smoothly lerp background toward target color
        if (backgroundImage != null)
            backgroundImage.color = Color.Lerp(backgroundImage.color, bgTargetColor, Time.deltaTime * colorLerpSpeed);
    }

    private void SetBackgroundStage(Color target)
    {
        bgTargetColor = target;
    }

    private CustomerData CurrentCustomer
    {
        get
        {
            if (customers == null || customers.Count == 0) return null;
            int idx = Mathf.Clamp(currentRound - 1, 0, customers.Count - 1);
            return customers[idx];
        }
    }

    private void StartEncounter()
    {
        customer = CurrentCustomer;
        if (customer == null)
        {
            Debug.LogError("SalesEncounterController: No customer found for this round. Fill the customers list.");
            enabled = false;
            return;
        }

        if (currentRound > roundsTotal)
        {
            EndGame();
            return;
        }

        ended = false;
        selectedCar = null;
        shownCars.Clear();

        profit = 0;
        turnIndex = 0;
        currentOfferPrice = 0;

        patienceMax = Mathf.Max(1, customer.PatienceMax);
        patience = patienceMax;

        dealChance = baseCloseChance;

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: {BuildCustomerIntroLine(customer)}";

        if (hintText)
            UpdateHint($"Round {currentRound}/{roundsTotal} — Pick a car to start.");

        if (walkAwayButton) walkAwayButton.gameObject.SetActive(false);
        if (speechBubble) speechBubble.SetActive(false);

        SetBackgroundStage(colorPickingCar); // Waiting for car selection

        RefreshHUD();
        ShowCarsForCustomer();
        SetupChoiceButtonsEnabled(false);
        SetChoiceTexts("—", "—", "—");
    }

    private string BuildCustomerIntroLine(CustomerData cust)
    {
        string patienceTier =
            cust.PatienceMax >= 80 ? "pretty patient" :
            cust.PatienceMax >= 50 ? "somewhat patient" :
            "not very patient";

        string priorities = GetTopPrioritiesText(cust, 3);

        return
            $"My budget is ${cust.Budget:N0}. I can stretch to ${cust.Budget + cust.MaxStretch:N0}. " +
            $"I'm {patienceTier} today (patience {cust.PatienceMax}). " +
            $"I care most about {priorities}.";
    }

    private string GetTopPrioritiesText(CustomerData cust, int topN)
    {
        var prefs = new List<(string label, int value)>
        {
            ("reliability", cust.WantReliability),
            ("safety", cust.WantSafety),
            ("fuel economy", cust.WantFuelEconomy),
            ("technology", cust.WantTechnology),
            ("space", cust.WantSpace),
            ("performance", cust.WantPerformance),
        };

        prefs.Sort((a, b) => b.value.CompareTo(a.value));
        topN = Mathf.Clamp(topN, 1, prefs.Count);
        var chosen = prefs.GetRange(0, topN);

        if (chosen.Count == 1) return $"{chosen[0].label} ({chosen[0].value})";
        if (chosen.Count == 2) return $"{chosen[0].label} ({chosen[0].value}) and {chosen[1].label} ({chosen[1].value})";
        return $"{chosen[0].label} ({chosen[0].value}), {chosen[1].label} ({chosen[1].value}), and {chosen[2].label} ({chosen[2].value})";
    }

    private void ShowCarsForCustomer()
    {
        var candidates = new List<CarModelData>(inventory);
        candidates.Sort((a, b) =>
        {
            int da = Mathf.Abs(a.MSRP - customer.Budget);
            int db = Mathf.Abs(b.MSRP - customer.Budget);
            return da.CompareTo(db);
        });

        for (int i = 0; i < candidates.Count && shownCars.Count < 3; i++)
        {
            if (!shownCars.Contains(candidates[i]))
                shownCars.Add(candidates[i]);
        }

        if (shownCars.Count < 3)
        {
            Debug.LogError("SalesEncounterController: need 3 unique cars in inventory.");
            enabled = false;
            return;
        }

        cardA.Bind(shownCars[0], OnSelectCar);
        cardB.Bind(shownCars[1], OnSelectCar);
        cardC.Bind(shownCars[2], OnSelectCar);
    }

    private void OnSelectCar(CarModelData car)
    {
        selectedCar = car;

        if (carDisplay != null)
            carDisplay.SetCarModel(car);

        currentOfferPrice = selectedCar.MSRP;
        selectedCar = car;
        currentOfferPrice = selectedCar.MSRP;

        cardA.SetSelected(shownCars[0] == car);
        cardB.SetSelected(shownCars[1] == car);
        cardC.SetSelected(shownCars[2] == car);

        RecomputeDealChance();

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: Tell me why {selectedCar.ModelName} is right for me.";

        if (hintText)
            UpdateHint($"Round {currentRound}/{roundsTotal} — Offer: ${currentOfferPrice:N0}");

        if (walkAwayButton) walkAwayButton.gameObject.SetActive(true);

        SetBackgroundStage(colorNegotiating); // Car selected, negotiation begins

        SetupChoicesForTurn();
        SetupChoiceButtonsEnabled(true);
        RefreshHUD();
        if (ended) return;
    }

    private void SetupChoicesForTurn()
    {
        if (choiceFairAsk == null || choiceDiscountAsk == null || choicePressureSale == null)
        {
            Debug.LogError("Assign choiceFairAsk, choiceDiscountAsk, choicePressureSale in Inspector.");
            return;
        }

        SetChoiceTexts(
            GetRandomResponse(GetFairAskButtonTexts()),
            GetRandomResponse(GetDiscountButtonTexts()),
            GetRandomResponse(GetPressureButtonTexts())
        );

        if (walkAwayText)
            walkAwayText.text = GetRandomResponse(GetWalkAwayButtonTexts());

        if (customerLineText)
        {
            string objection = GetObjectionForTurn(turnIndex);
            customerLineText.text = $"{customer.CustomerName}: {objection}";
        }

        if (hintText)
            UpdateHint($"Round {currentRound}/{roundsTotal} — Offer: ${currentOfferPrice:N0} | Turn {turnIndex + 1}/{turnsPerCustomer}");
    }

    private void OnEnable()
    {
        choiceAButton.onClick.RemoveAllListeners();
        choiceBButton.onClick.RemoveAllListeners();
        choiceCButton.onClick.RemoveAllListeners();

        choiceAButton.onClick.AddListener(() => OnPickChoice(0));
        choiceBButton.onClick.AddListener(() => OnPickChoice(1));
        choiceCButton.onClick.AddListener(() => OnPickChoice(2));
    }

    private void OnPickChoice(int idx)
    {
        if (ended) return;

        if (selectedCar == null)
        {
            if (hintText) UpdateHint($"Round {currentRound}/{roundsTotal} — Pick a car first.");
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

        int oldPrice = currentOfferPrice;
        currentOfferPrice += picked.PriceDelta;
        currentOfferPrice = Mathf.Max(currentOfferPrice, 100);

        int priceChange = currentOfferPrice - oldPrice;
        profit += priceChange;

        patience = Mathf.Clamp(patience + picked.PatienceDelta, 0f, patienceMax);

        float personalityMod = PersonalityModifier(customer.Personality, picked);
        dealChance = baseCloseChance;

        if (selectedCar != null)
        {
            dealChance += ComputeFitBoost(customer, selectedCar);
            dealChance -= ComputePricePenalty(customer, currentOfferPrice);
        }

        dealChance += picked.DealChanceDelta + personalityMod;
        dealChance = Mathf.Clamp01(dealChance);

        int discountAmount = Mathf.Max(0, oldPrice - currentOfferPrice);
        float discountBonus = Mathf.Clamp01(discountAmount / 10000f) * 0.15f;
        dealChance += discountBonus;

        ApplyAffordabilityGate();

        string salesLine = GenerateSalespersonLine(picked);
        ShowSpeechBubble(salesLine);

        SetBackgroundStage(colorThinking); // Waiting for customer response

        SetupChoiceButtonsEnabled(false);
        if (walkAwayButton) walkAwayButton.interactable = false;

        bool priceChanged = currentOfferPrice != oldPrice;
        string customerResponse = GenerateCustomerResponse(picked, priceChanged);
        StartCoroutine(ResolveChoiceAfterDelay(picked, customerResponse));
    }

    private System.Collections.IEnumerator ResolveChoiceAfterDelay(ChoiceData picked, string customerResponse)
    {
        yield return new WaitForSeconds(speechDisplayTime);

        if (ended) yield break;

        HideSpeechBubble();

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: {customerResponse}";

        if (hintText)
            UpdateHint($"Round {currentRound}/{roundsTotal} — Offer: ${currentOfferPrice:N0} | Turn {turnIndex + 1}/{turnsPerCustomer}");

        RefreshHUD();

        if (patience <= 0f)
        {
            End(false, "Customer walked away.");
            yield break;
        }

        bool canClose = (turnIndex + 1) >= earliestCloseTurn;

        if (canClose && TryCloseDeal())
        {
            profit += currentOfferPrice;
            End(true, $"Deal closed! Sold for ${currentOfferPrice:N0}");
            yield break;
        }

        SetBackgroundStage(colorNegotiating); // Back to active negotiation

        turnIndex++;
        if (turnIndex >= turnsPerCustomer)
        {
            End(false, "They didn't buy today.");
            yield break;
        }

        SetupChoiceButtonsEnabled(true);
        if (walkAwayButton) walkAwayButton.interactable = true;
        SetupChoicesForTurn();
    }

    private bool IsOfferAffordable()
    {
        int maxPay = customer.Budget + customer.MaxStretch;
        return currentOfferPrice <= maxPay;
    }

    private void ApplyAffordabilityGate()
    {
        if (!IsOfferAffordable())
            dealChance = 0f;
    }

    private void RecomputeDealChance()
    {
        dealChance = Mathf.Clamp01(baseCloseChance + ComputeFitBoost(customer, selectedCar) - ComputePricePenalty(customer, currentOfferPrice));
        ApplyAffordabilityGate();
    }

    private bool TryCloseDeal()
    {
        if (!IsOfferAffordable())
        {
            if (hintText)
                UpdateHint($"Round {currentRound}/{roundsTotal} — ❌ Too expensive (max ${customer.Budget + customer.MaxStretch:N0})");
            return false;
        }

        float roll = 0.50F;
        bool success = roll <= dealChance;

        if (hintText)
            UpdateHint(
              success ? $"Round {currentRound}/{roundsTotal} — ✅ Closed! (chance {(dealChance * 100f):0}%)"
                : $"Round {currentRound}/{roundsTotal} — ❌ Not yet… (chance {(dealChance * 100f):0}%)");

        return success;
    }

    private void End(bool success, string message)
    {
        ended = true;
        SetupChoiceButtonsEnabled(false);
        if (walkAwayButton) walkAwayButton.interactable = false;

        if (customerLineText)
            customerLineText.text = $"Round {currentRound}/{roundsTotal} Result: {message}";

        // Flash to success or failure color
        SetBackgroundStage(success ? colorDealClosed : colorCustomerLeft);

        if (success)
        {
            totalProfit += profit;
            successfulSales++;
        }

        currentRound++;

        if (currentRound > roundsTotal)
        {
            Invoke(nameof(EndGame), nextRoundDelay);
        }
        else
        {
            if (hintText)
                UpdateHint($"Next: Round {currentRound}/{roundsTotal} starting...");
            Invoke(nameof(StartEncounter), nextRoundDelay);
        }
    }

    private void EndGame()
    {
        SetupChoiceButtonsEnabled(false);
        if (walkAwayButton) walkAwayButton.interactable = false;

        if (customerLineText)
            customerLineText.text = $"Game Finished! Profit: ${totalProfit:N0} | Sales: {successfulSales}/{roundsTotal}";

        if (hintText)
            UpdateHint("Shift complete.");

        SetBackgroundStage(colorGameOver); // All rounds done
    }

    // ==========================
    // UI helpers
    // ==========================

    private void ShowSpeechBubble(string text)
    {
        if (speechBubble) speechBubble.SetActive(true);
        if (speechBubbleText) speechBubbleText.text = text;
    }

    private void HideSpeechBubble()
    {
        if (speechBubble) speechBubble.SetActive(false);
    }

    private string GetTopStat()
    {
        if (selectedCar == null) return "value";

        int max = selectedCar.Reliability;
        string top = "reliability";

        if (selectedCar.Safety > max) { max = selectedCar.Safety; top = "safety"; }
        if (selectedCar.FuelEconomy > max) { max = selectedCar.FuelEconomy; top = "fuel economy"; }
        if (selectedCar.Technology > max) { max = selectedCar.Technology; top = "tech features"; }
        if (selectedCar.Space > max) { max = selectedCar.Space; top = "cargo space"; }
        if (selectedCar.Performance > max) { max = selectedCar.Performance; top = "performance"; }

        return top;
    }

    private void RefreshHUD()
    {
        if (profitText)
        {
            profitText.text = $"Round Profit: ${profit:N0} | Total: ${totalProfit:N0}";
            // Color based on total profit across all rounds
            profitText.color = totalProfit > 0 ? profitPositiveColor
                             : totalProfit < 0 ? profitNegativeColor
                             : profitNeutralColor;
        }

        if (dealChanceText)
        {
            float shown = IsOfferAffordable() ? dealChance : 0f;
            dealChanceText.text = $"Close Chance: {(shown * 100f):0}%";
            // Color based on thresholds
            dealChanceText.color = shown >= 0.5f ? chanceHighColor
                                 : shown >= 0.2f ? chanceMidColor
                                 : chanceLowColor;
        }

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

    private void UpdateHint(string message)
    {
        if (!hintText) return;
        hintText.text = message;
    }

    // ==========================
    // Text arrays and response logic (unchanged)
    // ==========================

    private string[] GetFairAskButtonTexts() => new[]
    {
        "Present the price honestly", "Lay out the facts", "Give a straightforward pitch",
        "Be upfront about the value", "Explain what you're offering", "Present it at face value",
        "Keep it real and simple", "Walk them through the details"
    };

    private string[] GetDiscountButtonTexts() => new[]
    {
        "Offer a discount", "Lower the price a bit", "Sweeten the deal", "Come down on the price",
        "Throw in a price drop", "Meet them halfway on cost", "Negotiate the price down", "Make it more affordable"
    };

    private string[] GetPressureButtonTexts() => new[]
    {
        "Push for the sale", "Emphasize urgency", "Press for a decision", "Go for the close",
        "Apply some pressure", "Don't let them walk", "Seal the deal now", "Make your best pitch"
    };

    private string[] GetWalkAwayButtonTexts() => new[]
    {
        "Walk away from this one", "Move on to the next customer", "This one's not working out",
        "Cut your losses", "Try someone else", "Skip this customer", "Not worth your time", "Let this one go"
    };

    private string GetObjectionForTurn(int t)
    {
        string[] objections = t switch
        {
            0 => new[] { "What price are we talking?", "Alright, let's talk numbers.", "How much are we looking at here?" },
            1 => new[] { "I'm not sure about this...", "Can you do better on the price?", "Help me understand why it's worth it." },
            2 => new[] { "I need to think about this.", "There are other dealerships, you know.", "I'm comparing a few options." },
            3 => new[] { "You're running out of time to convince me.", "My patience is wearing thin.", "Give me a real reason to buy today." },
            4 => new[] { "This is your last chance.", "Make me an offer I can't refuse.", "Why should I buy right now?" },
            _ => new[] { "I'm listening...", "Go on.", "What else?" }
        };
        return GetRandomResponse(objections);
    }

    private string GetRandomResponse(string[] options)
    {
        if (options == null || options.Length == 0) return "";
        return options[Random.Range(0, options.Length)];
    }

    private string GenerateSalespersonLine(ChoiceData picked)
    {
        if (picked == choiceFairAsk)
            return GetRandomResponse(new[] {
                $"This {selectedCar?.ModelName} is priced at ${currentOfferPrice:N0} — solid value.",
                $"At ${currentOfferPrice:N0}, you're getting great {GetTopStat()}.",
                $"The {selectedCar?.ModelName} at ${currentOfferPrice:N0} is the right call.",
            });

        if (picked == choiceDiscountAsk)
            return GetRandomResponse(new[] {
                $"I can bring it down to ${currentOfferPrice:N0} just for you.",
                $"How about ${currentOfferPrice:N0}? That's a real deal.",
                $"I'll cut it to ${currentOfferPrice:N0} — best I can do.",
            });

        if (picked == choicePressureSale)
            return GetRandomResponse(new[] {
                "This deal won't last — someone else is looking at this model.",
                "I've got two other buyers interested today.",
                "If you walk, this price walks with you.",
            });

        return "Let's make this work.";
    }

    private string GenerateCustomerResponse(ChoiceData picked, bool priceChanged)
    {
        float patiencePercent = patience / patienceMax;

        bool isFairAsk = picked == choiceFairAsk;
        bool isDiscount = picked == choiceDiscountAsk;
        bool isPressure = picked == choicePressureSale;

        if (patiencePercent > 0.7f && dealChance > 0.5f)
        {
            if (isDiscount) return GetRandomResponse(GetPositiveDiscountResponses(customer.Personality));
            if (isPressure) return GetRandomResponse(GetPositivePressureResponses(customer.Personality));
            if (isFairAsk) return GetRandomResponse(GetPositiveFairResponses(customer.Personality));
        }

        if (patiencePercent > 0.4f)
        {
            if (priceChanged && isDiscount) return GetRandomResponse(GetNeutralDiscountResponses(customer.Personality));
            if (isPressure) return GetRandomResponse(GetNeutralPressureResponses(customer.Personality));
            if (isFairAsk) return GetRandomResponse(GetNeutralFairResponses(customer.Personality));
        }

        if (isPressure) return GetRandomResponse(GetNegativePressureResponses(customer.Personality));
        if (isDiscount) return GetRandomResponse(GetNegativeDiscountResponses(customer.Personality));
        if (isFairAsk) return GetRandomResponse(GetNegativeFairResponses(customer.Personality));

        return "Let me think about this...";
    }

    private float ComputeFitBoost(CustomerData cust, CarModelData car)
    {
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

        float normalized = match / (wSum * 100f);
        return normalized * 0.35f;
    }

    private float ComputePricePenalty(CustomerData cust, int offerPrice)
    {
        int over = offerPrice - cust.Budget;
        if (over <= 0) return 0f;
        if (over <= cust.MaxStretch) return 0.08f;
        return 0.20f;
    }

    private int GetMaxAffordablePrice() => customer.Budget + customer.MaxStretch;

    private float PersonalityModifier(PersonalityType type, ChoiceData picked)
    {
        if (type == PersonalityType.Cautious && picked == choicePressureSale) return -0.05f;
        if (type == PersonalityType.Impulsive && picked == choicePressureSale) return +0.03f;
        if (type == PersonalityType.Analytical && picked == choiceFairAsk) return +0.03f;
        if (type == PersonalityType.Cautious && picked == choiceDiscountAsk) return +0.03f;
        return 0f;
    }

    // ===== POSITIVE =====
    private string[] GetPositiveDiscountResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "Now we're talking!", "That price just changed everything.", "Okay, I'm interested now!" },
        PersonalityType.Cautious => new[] { "That's more reasonable. I'm warming up to it.", "The lower price helps my confidence.", "That makes it feel safer." },
        PersonalityType.Analytical => new[] { "That discount improves the value equation.", "The revised price fits my budget model better.", "Now the numbers make sense." },
        _ => new[] { "That's a good offer.", "I like where this is going.", "You've got my attention." }
    };

    private string[] GetPositivePressureResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "You know what, let's do it!", "Fine, I'm in. Let's close this.", "Okay okay, I'll take it!" },
        PersonalityType.Cautious => new[] { "Alright, if you say so...", "I'll trust your judgment on this.", "Okay, I'll go with it." },
        PersonalityType.Analytical => new[] { "Your urgency is noted. The data still supports this.", "If the opportunity is limited, I'll act now.", "Logic supports moving forward." },
        _ => new[] { "Okay, let's do it.", "You've convinced me.", "I'll go ahead." }
    };

    private string[] GetPositiveFairResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "I love your honesty. Let's go!", "Straight talk — I respect that.", "Simple and clear. I'm sold!" },
        PersonalityType.Cautious => new[] { "I appreciate the transparency.", "Your honesty makes me feel better about this.", "That straightforward approach builds trust." },
        PersonalityType.Analytical => new[] { "The data checks out. Good presentation.", "Logical and clear. I'm impressed.", "Your facts align with my research." },
        _ => new[] { "That's fair.", "I appreciate the honesty.", "Good pitch." }
    };

    // ===== NEUTRAL =====
    private string[] GetNeutralDiscountResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "Getting better... but I want more.", "A discount helps, but I'm not there yet.", "Nice, but can you do even better?" },
        PersonalityType.Cautious => new[] { "The lower price helps a little.", "I'm slightly more comfortable now.", "It's moving in the right direction." },
        PersonalityType.Analytical => new[] { "The discount improves cost-effectiveness somewhat.", "Price reduction noted. Still analyzing.", "Better, but not yet optimal." },
        _ => new[] { "Better, but I'm still thinking.", "Getting there...", "Not quite enough." }
    };

    private string[] GetNeutralPressureResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "Don't rush me.", "I need a moment.", "Pressure won't make me decide faster." },
        PersonalityType.Cautious => new[] { "I need more time than you're giving me.", "Don't push — I make better decisions without pressure.", "I'm cautious by nature. Give me space." },
        PersonalityType.Analytical => new[] { "Urgency is irrelevant to my analysis.", "I make decisions based on data, not urgency.", "Your sense of urgency doesn't align with my process." },
        _ => new[] { "Hold on, I'm still considering.", "Don't push me into this.", "I need more time." }
    };

    private string[] GetNeutralFairResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "I hear you, but I'm not jumping in yet.", "Sounds reasonable, but I'm not convinced.", "Fair enough, but show me more." },
        PersonalityType.Cautious => new[] { "I'm still weighing my options.", "That sounds fair, but I'm naturally cautious.", "I need to think this through carefully." },
        PersonalityType.Analytical => new[] { "I need more concrete data to decide.", "The logic is sound, but I want to verify.", "Let me run through the numbers one more time." },
        _ => new[] { "I'm listening, but not convinced yet.", "Okay, but I'm not ready to commit.", "I need more information." }
    };

    // ===== NEGATIVE =====
    private string[] GetNegativePressureResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "Back off. You're losing me here.", "Too much pressure. I'm out.", "You're being too aggressive." },
        PersonalityType.Cautious => new[] { "You're pushing too hard. I don't like this.", "Stop. You're making me uncomfortable.", "I'm walking if you keep this up." },
        PersonalityType.Analytical => new[] { "Pressure tactics are illogical and off-putting.", "Your urgency suggests desperation, not value.", "I don't make rushed decisions under pressure." },
        _ => new[] { "Stop pushing. It's not working.", "You're being too aggressive.", "This isn't how I want to do business." }
    };

    private string[] GetNegativeDiscountResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "It's not just about price anymore...", "Too little, too late.", "Price isn't the only issue here." },
        PersonalityType.Cautious => new[] { "The discount helps, but I'm still very uncertain.", "Even with the discount, I have doubts.", "I'm not feeling confident about this." },
        PersonalityType.Analytical => new[] { "The price reduction doesn't change the fundamental value equation.", "The math still doesn't work for me, even discounted." },
        _ => new[] { "It's not just about price at this point...", "The discount isn't enough.", "I have bigger concerns than cost." }
    };

    private string[] GetNegativeFairResponses(PersonalityType type) => type switch
    {
        PersonalityType.Impulsive => new[] { "I'm losing interest fast...", "I think I'm done here.", "I'm just not feeling it." },
        PersonalityType.Cautious => new[] { "I'm getting uncomfortable with this whole thing.", "My gut is telling me to walk away.", "This doesn't feel right to me." },
        PersonalityType.Analytical => new[] { "The value just isn't adding up for me.", "The data doesn't support moving forward.", "Logically, this isn't the right decision for me." },
        _ => new[] { "I don't know about this...", "I'm having serious doubts.", "This might not be for me." }
    };
}
