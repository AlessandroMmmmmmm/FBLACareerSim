using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    [SerializeField] private int roundsTotal = 3;          // ✅ Round 1..3 then game ends
    [SerializeField] private float nextRoundDelay = 1.5f;  // small delay after result before next customer
    [Header("References")]
    [SerializeField] private SpriteStackRotator carDisplay; // Drag your 3D display object here

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
    private int currentRound = 1; // ✅ 1..roundsTotal
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

        StartEncounter();
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

        // ✅ Stop if rounds done
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

        // Reset baseline chance for this customer/round
        dealChance = baseCloseChance;

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: {BuildCustomerIntroLine(customer)}";

        if (hintText)
            UpdateHint($"Round {currentRound}/{roundsTotal} — Pick a car to start.");

        if (walkAwayButton) walkAwayButton.gameObject.SetActive(false);
        if (speechBubble) speechBubble.SetActive(false);

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

        // NEW: Update the visual stack
        if (carDisplay != null)
        {
            carDisplay.SetCarModel(car);
        }

        currentOfferPrice = selectedCar.MSRP;

        selectedCar = car;
        currentOfferPrice = selectedCar.MSRP;

        cardA.SetSelected(shownCars[0] == car);
        cardB.SetSelected(shownCars[1] == car);
        cardC.SetSelected(shownCars[2] == car);

        RecomputeDealChance(); // ✅ includes affordability gate

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: Tell me why {selectedCar.ModelName} is right for me.";

        if (hintText)
            UpdateHint($"Round {currentRound}/{roundsTotal} — Offer: ${currentOfferPrice:N0}");

        if (walkAwayButton) walkAwayButton.gameObject.SetActive(true);

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

        // Apply price change
        currentOfferPrice += picked.PriceDelta;
        currentOfferPrice = Mathf.Max(currentOfferPrice, 100);

        // ✅ PROFIT RULE: discounts lose money (negative), price increases gain money (positive)
        int priceChange = currentOfferPrice - oldPrice;
        profit += priceChange;
        // Example: old 20000 -> new 18500 => priceChange = -1500 => profit -= 1500 (lost money)
        // Example: old 20000 -> new 21000 => priceChange = +1000 => profit += 1000 (gained money)

        // Apply deltas
        patience = Mathf.Clamp(patience + picked.PatienceDelta, 0f, patienceMax);

        // ✅ Recompute chance (includes affordability gate)
        float personalityMod = PersonalityModifier(customer.Personality, picked);

        // ✅ Recompute chance from scratch at the NEW price (stable + intuitive)
        dealChance = baseCloseChance;

        if (selectedCar != null)
        {
            dealChance += ComputeFitBoost(customer, selectedCar);
            dealChance -= ComputePricePenalty(customer, currentOfferPrice);
        }

        // ✅ Add the “tactic” effect (discount/fair/pressure) + personality effect
        dealChance += picked.DealChanceDelta + personalityMod;

        // Clamp and then hard-gate affordability
        dealChance = Mathf.Clamp01(dealChance);
        int discountAmount = Mathf.Max(0, oldPrice - currentOfferPrice);
        float discountBonus = Mathf.Clamp01(discountAmount / 10000f) * 0.15f;
        // ex: $1000 off => (0.1)*0.15 = +0.015 (1.5%)

        dealChance += discountBonus;

        ApplyAffordabilityGate();


        // Speech bubble flow
        string salesLine = GenerateSalespersonLine(picked);
        ShowSpeechBubble(salesLine);

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

        // ✅ If price is over budget+stretch, canClose may be true but deal is forced impossible.
        if (canClose && TryCloseDeal())
        {
            // ✅ PROFIT RULE: profit is sale price; discounts already reduced profit earlier via priceChange
            profit += currentOfferPrice;

            End(true, $"Deal closed! Sold for ${currentOfferPrice:N0}");
            yield break;
        }


        // Next turn
        turnIndex++;
        if (turnIndex >= turnsPerCustomer)
        {
            End(false, "They didn't buy today.");
            yield break;
        }

        // Re-enable
        SetupChoiceButtonsEnabled(true);
        if (walkAwayButton) walkAwayButton.interactable = true;
        SetupChoicesForTurn();
    }

    // ==========================
    // ✅ KEY FIX: affordability gate
    // ==========================

    private bool IsOfferAffordable()
    {
        int maxPay = customer.Budget + customer.MaxStretch;
        return currentOfferPrice <= maxPay;
    }

    private void ApplyAffordabilityGate()
    {
        if (!IsOfferAffordable())
        {
            dealChance = 0f; // ✅ hard stop
        }
    }

    private void RecomputeDealChance()
    {
        dealChance = Mathf.Clamp01(baseCloseChance + ComputeFitBoost(customer, selectedCar) - ComputePricePenalty(customer, currentOfferPrice));
        ApplyAffordabilityGate();
    }

    private bool TryCloseDeal()
    {
        // ✅ Hard block first
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
              success  ? $"Round {currentRound}/{roundsTotal} — ✅ Closed! (chance {(dealChance * 100f):0}%)"
                : $"Round {currentRound}/{roundsTotal} — ❌ Not yet… (chance {(dealChance * 100f):0}%)");

        return success;
    }

    // ==========================
    // ✅ End / Round progression
    // ==========================

    private void End(bool success, string message)
    {
        ended = true;
        SetupChoiceButtonsEnabled(false);
        if (walkAwayButton) walkAwayButton.interactable = false;

        if (customerLineText)
            customerLineText.text = $"Round {currentRound}/{roundsTotal} Result: {message}";

        if (success)
        {
            totalProfit += profit;
            successfulSales++;
        }

        // ✅ Advance round NOW when encounter ends (success OR failure)
        currentRound++;

        // ✅ Immediately move on after deal closes (and also after failures)
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
        if (profitText) profitText.text = $"Round Profit: ${profit:N0} | Total: ${totalProfit:N0}";

        if (dealChanceText)
        {
            float shown = IsOfferAffordable() ? dealChance : 0f;
            dealChanceText.text = $"Close Chance: {(shown * 100f):0}%";
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

    // ==========================
    // Your existing text arrays and response logic
    // (keep these exactly as you already have them)
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
            1 => new[] { "Reliability matters to me.", "How's this going to hold up over the years?" },
            2 => new[] { "That feels expensive. Can you do better?", "I'm not sure I can stretch that far." },
            3 => new[] { "What about warranty or add-ons?", "What extras are included?" },
            _ => new[] { "Convince me this is the right choice.", "I'm still on the fence." }
        };
        return GetRandomResponse(objections);
    }

    private string GetRandomResponse(string[] responses)
    {
        if (responses == null || responses.Length == 0) return "Hmm...";
        return responses[Random.Range(0, responses.Length)];
    }

    // NOTE: keep your existing GenerateSalespersonLine / customer responses arrays here:
    private string GenerateSalespersonLine(ChoiceData picked)
    {
        if (picked == choiceFairAsk)
            return GetRandomResponse(GetSalespersonFairLines());
        if (picked == choiceDiscountAsk)
            return GetRandomResponse(GetSalespersonDiscountLines());
        if (picked == choicePressureSale)
            return GetRandomResponse(GetSalespersonPressureLines());

        return "...";
    }

    private string[] GetSalespersonFairLines()
    {
        return new[] {
        $"You: This {selectedCar.ModelName} is priced at ${currentOfferPrice:N0}. It's fair, and here's why...",
        $"You: Let me be straight with you — ${currentOfferPrice:N0} is what this {selectedCar.ModelName} is worth.",
        $"You: I'll be honest. At ${currentOfferPrice:N0}, this {selectedCar.ModelName} is a solid deal.",
        $"You: Here's the deal — this {selectedCar.ModelName} has great {GetTopStat()}. ${currentOfferPrice:N0} is the price.",
        $"You: No games here. The {selectedCar.ModelName} at ${currentOfferPrice:N0} speaks for itself.",
        $"You: Let me walk you through what you're getting with the {selectedCar.ModelName}...",
        $"You: The {selectedCar.ModelName} is a great car. At ${currentOfferPrice:N0}, I think it's a fair ask.",
        $"You: I want you to feel good about this. Here's what the {selectedCar.ModelName} brings to the table..."
    };
    }

    private string[] GetSalespersonDiscountLines()
    {
        return new[] {
        $"You: Tell you what — I can bring it down to ${currentOfferPrice:N0} for you.",
        $"You: I see you're hesitating. How about I knock it down to ${currentOfferPrice:N0}?",
        $"You: Let me sweeten this up — ${currentOfferPrice:N0}. That's my best offer.",
        $"You: I've got some room to work with. ${currentOfferPrice:N0} — what do you think?",
        $"You: I can do ${currentOfferPrice:N0}. That's a solid saving on this {selectedCar.ModelName}.",
        $"You: Let's meet in the middle. I'll take it down to ${currentOfferPrice:N0}.",
        $"You: You drive a hard bargain, but I can do ${currentOfferPrice:N0}.",
        $"You: How about ${currentOfferPrice:N0}? I think that's fair for both of us."
    };
    }

    private string[] GetSalespersonPressureLines()
    {
        return new[] {
        $"You: I gotta be real with you — this {selectedCar.ModelName} won't last long at ${currentOfferPrice:N0}.",
        $"You: I've had three other people ask about this one today. ${currentOfferPrice:N0} is the price.",
        $"You: This is one of the better {selectedCar.Tag}s we've had come in. You'd be smart to move on it.",
        $"You: At ${currentOfferPrice:N0}, this {selectedCar.ModelName} is going fast. Don't want to miss out.",
        $"You: Between you and me, I can't hold this price forever. What's it going to take?",
        $"You: Someone's coming back for this one tomorrow. Are you in or not?",
        $"You: The {selectedCar.ModelName} is in high demand. This price won't stick around.",
        $"You: I'd hate for you to lose out on this. ${currentOfferPrice:N0} — let's lock it in."
    };
    }
    private string[] GetPositiveDiscountResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "That discount sweetens the deal! I'm getting excited.",
            "Now we're talking! That price works for me.",
            "Perfect! Lower price means I can say yes faster.",
            "You got me! That discount sealed it.",
            "I love a good deal. This is looking better and better!"
        },
            PersonalityType.Cautious => new[] {
            "The lower price makes me feel better about this.",
            "That's more in line with what I was hoping for.",
            "A discount helps me justify this purchase.",
            "Okay, the price is starting to feel right.",
            "I appreciate you working with me on the cost."
        },
            PersonalityType.Analytical => new[] {
            "The revised price improves the value proposition significantly.",
            "Now the numbers are starting to align with market data.",
            "That discount brings it into my acceptable range.",
            "The price-to-value ratio is looking better.",
            "I can work with these numbers."
        },
            _ => new[] { "That helps a lot, actually.", "Better! I'm warming up to this.", "The discount makes a difference." }
        };
    }

    private string[] GetPositivePressureResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "You know what? You're right. Let's do this!",
            "Okay, okay, you convinced me. I'm in!",
            "I can see myself driving this already!",
            "Your enthusiasm is contagious. I'm excited!",
            "You make a compelling case. Let's move forward."
        },
            PersonalityType.Cautious => new[] {
            "You're persuasive, I'll give you that.",
            "I suppose you have a point...",
            "Your confidence is reassuring.",
            "Maybe I am overthinking this.",
            "Alright, you're starting to win me over."
        },
            PersonalityType.Analytical => new[] {
            "Your arguments are logically sound.",
            "The data supports what you're saying.",
            "I can't argue with those facts.",
            "You've addressed my concerns effectively.",
            "The evidence is compelling."
        },
            _ => new[] { "You make a good point.", "I'm starting to see it your way.", "That's a fair argument." }
        };
    }

    private string[] GetPositiveFairResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "I like your straightforward approach!",
            "Honesty goes a long way with me.",
            "You're not playing games. I respect that.",
            "Finally, someone who tells it like it is!",
            "Your transparency is refreshing."
        },
            PersonalityType.Cautious => new[] {
            "I appreciate your honesty. That helps a lot.",
            "Thank you for being upfront with me.",
            "Your straightforward approach makes me more comfortable.",
            "I trust someone who's honest about the details.",
            "This feels like a fair conversation."
        },
            PersonalityType.Analytical => new[] {
            "The numbers are starting to make sense.",
            "I appreciate the transparent breakdown.",
            "Your logical presentation is effective.",
            "The facts speak for themselves.",
            "You've given me the data I need to decide."
        },
            _ => new[] { "I appreciate the honesty.", "You're being fair with me.", "That's a reasonable approach." }
        };
    }

    // ===== NEUTRAL =====
    private string[] GetNeutralDiscountResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "Better! But I need to see more value.",
            "That helps, but what else can you offer?",
            "The price is improving, but I'm not sold yet.",
            "Good start. Keep going."
        },
            PersonalityType.Cautious => new[] {
            "That helps, but I'm still not sure...",
            "It's a step in the right direction, I suppose.",
            "The discount is nice, but I have other concerns.",
            "I'm still weighing my options here."
        },
            PersonalityType.Analytical => new[] {
            "The revised price is more reasonable.",
            "That improves the equation somewhat.",
            "Better, but I'm still calculating the total value.",
            "The discount helps the math, but there's more to consider."
        },
            _ => new[] { "Okay, that's a step in the right direction.", "It helps, but I'm not there yet.", "Better, I guess." }
        };
    }

    private string[] GetNeutralPressureResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "You're making a good case, but...",
            "Hold on, let me think about this.",
            "You're pushy, but I get it.",
            "Slow down, I need a minute."
        },
            PersonalityType.Cautious => new[] {
            "Don't rush me. I need time to think.",
            "I don't respond well to pressure.",
            "Give me space to make this decision.",
            "Pushing me won't help your case."
        },
            PersonalityType.Analytical => new[] {
            "Pressure tactics won't work on me.",
            "I make decisions based on data, not urgency.",
            "Your sense of urgency doesn't align with my process.",
            "I need time to analyze, not be rushed."
        },
            _ => new[] { "Hold on, I'm still considering.", "Don't push me into this.", "I need more time." }
        };
    }

    private string[] GetNeutralFairResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "I hear you, but I'm not jumping in yet.",
            "Sounds reasonable, but I'm not convinced.",
            "Fair enough, but show me more.",
            "Okay, but what's the catch?"
        },
            PersonalityType.Cautious => new[] {
            "I'm still weighing my options.",
            "That sounds fair, but I'm naturally cautious.",
            "I need to think this through carefully.",
            "It's reasonable, but I want to be sure."
        },
            PersonalityType.Analytical => new[] {
            "I need more concrete data to decide.",
            "The logic is sound, but I want to verify the details.",
            "Let me run through the numbers one more time.",
            "I need to compare this against other options."
        },
            _ => new[] { "I'm listening, but not convinced yet.", "Okay, but I'm not ready to commit.", "I need more information." }
        };
    }

    // ===== NEGATIVE =====
    private string[] GetNegativePressureResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "Back off. You're losing me here.",
            "Too much pressure. I'm out.",
            "You're being too aggressive. Not interested.",
            "This pushy approach is turning me off."
        },
            PersonalityType.Cautious => new[] {
            "You're pushing too hard. I don't like this.",
            "Stop. You're making me uncomfortable.",
            "This aggressive approach is a dealbreaker for me.",
            "I'm walking if you keep this up.",
            "Your tactics are making me want to leave."
        },
            PersonalityType.Analytical => new[] {
            "This aggressive approach isn't working.",
            "Pressure tactics are illogical and off-putting.",
            "Your urgency suggests desperation, not value.",
            "I don't make rushed decisions under pressure."
        },
            _ => new[] { "Stop pushing. It's not working.", "You're being too aggressive.", "This isn't how I want to do business." }
        };
    }

    private string[] GetNegativeDiscountResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "It's not just about price anymore...",
            "The discount doesn't fix my other concerns.",
            "Too little, too late.",
            "Price isn't the only issue here."
        },
            PersonalityType.Cautious => new[] {
            "The discount helps, but I'm still very uncertain.",
            "Even with the discount, I have doubts.",
            "Lower price doesn't address all my concerns.",
            "I'm not feeling confident about this."
        },
            PersonalityType.Analytical => new[] {
            "The price reduction doesn't change the fundamental value equation.",
            "Discount aside, the overall package still doesn't compute.",
            "You're addressing price, but not the core issues.",
            "The math still doesn't work for me, even discounted."
        },
            _ => new[] { "It's not just about price at this point...", "The discount isn't enough.", "I have bigger concerns than cost." }
        };
    }

    private string[] GetNegativeFairResponses(PersonalityType type)
    {
        return type switch
        {
            PersonalityType.Impulsive => new[] {
            "I'm losing interest fast...",
            "This isn't exciting me anymore.",
            "I think I'm done here.",
            "I'm just not feeling it."
        },
            PersonalityType.Cautious => new[] {
            "I'm getting uncomfortable with this whole thing.",
            "My gut is telling me to walk away.",
            "This doesn't feel right to me.",
            "I think this isn't the right fit."
        },
            PersonalityType.Analytical => new[] {
            "The value just isn't adding up for me.",
            "I've analyzed this thoroughly and it's not working.",
            "The data doesn't support moving forward.",
            "Logically, this isn't the right decision for me."
        },
            _ => new[] { "I don't know about this...", "I'm having serious doubts.", "This might not be for me." }
        };
    }

    private string GenerateCustomerResponse(ChoiceData picked, bool priceChanged)
    {
        float patiencePercent = patience / patienceMax;

        bool isFairAsk = picked == choiceFairAsk;
        bool isDiscount = picked == choiceDiscountAsk;
        bool isPressure = picked == choicePressureSale;

        // HIGH PATIENCE + GOOD DEAL CHANCE
        if (patiencePercent > 0.7f && dealChance > 0.5f)
        {
            if (isDiscount)
                return GetRandomResponse(GetPositiveDiscountResponses(customer.Personality));
            if (isPressure)
                return GetRandomResponse(GetPositivePressureResponses(customer.Personality));
            if (isFairAsk)
                return GetRandomResponse(GetPositiveFairResponses(customer.Personality));
        }

        // MEDIUM PATIENCE
        if (patiencePercent > 0.4f)
        {
            if (priceChanged && isDiscount)
                return GetRandomResponse(GetNeutralDiscountResponses(customer.Personality));
            if (isPressure)
                return GetRandomResponse(GetNeutralPressureResponses(customer.Personality));
            if (isFairAsk)
                return GetRandomResponse(GetNeutralFairResponses(customer.Personality));
        }

        // LOW PATIENCE
        if (isPressure)
            return GetRandomResponse(GetNegativePressureResponses(customer.Personality));
        if (isDiscount)
            return GetRandomResponse(GetNegativeDiscountResponses(customer.Personality));
        if (isFairAsk)
            return GetRandomResponse(GetNegativeFairResponses(customer.Personality));

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
    private int GetMaxAffordablePrice()
    {
        return customer.Budget + customer.MaxStretch;
    }

    private float PersonalityModifier(PersonalityType type, ChoiceData picked)
    {
        if (type == PersonalityType.Cautious && picked == choicePressureSale) return -0.05f;
        if (type == PersonalityType.Impulsive && picked == choicePressureSale) return +0.03f;
        if (type == PersonalityType.Analytical && picked == choiceFairAsk) return +0.03f;
        if (type == PersonalityType.Cautious && picked == choiceDiscountAsk) return +0.03f;
        return 0f;
    }
    private void UpdateHint(string message)
    {
        if (!hintText) return;
        hintText.text = message;
    }




}