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
    [SerializeField] private int customersPerShift = 5; // NEW: how many customers before shift ends

    // Runtime state - Single Customer
    private CarModelData selectedCar;
    private List<CarModelData> shownCars = new List<CarModelData>();
    private int currentOfferPrice; // NEW: dynamic pricing

    private int turnIndex = 0;
    private int profit = 0;
    private float patience;
    private float patienceMax;
    private float dealChance; // 0..1
    private bool ended;

    // Runtime state - Shift Progress
    private int customersServed = 0;
    private int totalShiftProfit = 0;
    private int successfulSales = 0;

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
        currentOfferPrice = 0;

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
        currentOfferPrice = selectedCar.MSRP; // Start at MSRP

        // Visually select
        cardA.SetSelected(shownCars[0] == car);
        cardB.SetSelected(shownCars[1] == car);
        cardC.SetSelected(shownCars[2] == car);

        // Compute starting chance based on fit + price pressure
        dealChance = Mathf.Clamp01(baseCloseChance + ComputeFitBoost(customer, selectedCar) - ComputePricePenalty(customer, currentOfferPrice));

        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: Tell me why {selectedCar.ModelName} is right for me.";

        if (hintText)
            hintText.text = $"Offer: ${currentOfferPrice:N0}";

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
            hintText.text = $"Offer: ${currentOfferPrice:N0} | Turn {turnIndex + 1}/{turnsPerCustomer}";
    }

    private string GetObjectionForTurn(int t)
    {
        // Simple but effective; feels "real"
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

        // Store old price for penalty recalculation
        int oldPrice = currentOfferPrice;

        // Apply price change
        currentOfferPrice += picked.PriceDelta;
        currentOfferPrice = Mathf.Max(currentOfferPrice, 100); // Can't go negative or too low

        // Apply your existing deltas
        profit += picked.ProfitDelta; // For add-ons/extras
        patience = Mathf.Clamp(patience + picked.PatienceDelta, 0f, patienceMax);

        float personalityMod = PersonalityModifier(customer.Personality, picked);

        // Recalculate price penalty if price changed
        float pricePenaltyAdjustment = 0f;
        if (currentOfferPrice != oldPrice)
        {
            float oldPenalty = ComputePricePenalty(customer, oldPrice);
            float newPenalty = ComputePricePenalty(customer, currentOfferPrice);
            pricePenaltyAdjustment = oldPenalty - newPenalty; // Lowering price helps (positive)
        }

        dealChance = Mathf.Clamp01(dealChance + picked.DealChanceDelta + personalityMod + pricePenaltyAdjustment);

        // Generate dynamic customer response
        string response = GenerateCustomerResponse(picked, currentOfferPrice != oldPrice);
        if (customerLineText)
            customerLineText.text = $"{customer.CustomerName}: {response}";

        if (hintText)
            hintText.text = $"Offer: ${currentOfferPrice:N0} | Turn {turnIndex + 1}/{turnsPerCustomer}";

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
            // Profit is the final sale price plus any extras
            profit += currentOfferPrice;
            End(true, "Deal closed!");
            return;
        }

        // Next turn
        turnIndex++;
        if (turnIndex >= turnsPerCustomer)
        {
            End(false, "They didn't buy today.");
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

        // Track shift progress
        if (success)
        {
            totalShiftProfit += profit;
            successfulSales++;
            Debug.Log($"SALE! Profit: ${profit} | Shift Total: ${totalShiftProfit} | Sales: {successfulSales}/{customersServed + 1}");
        }
        else
        {
            Debug.Log($"NO SALE | Customer {customersServed + 1}/{customersPerShift}");
        }

        customersServed++;

        // Check if shift is over
        if (customersServed >= customersPerShift)
        {
            EndShift();
        }
        else
        {
            // Move to next customer after a delay
            Invoke(nameof(StartNextCustomer), 2f);
        }
    }

    private void StartNextCustomer()
    {
        // For now, restart with the same customer
        // TODO: Randomly select from a customer pool
        StartEncounter();
    }

    private void EndShift()
    {
        Debug.Log($"========== SHIFT COMPLETE ==========");
        Debug.Log($"Customers Served: {customersServed}");
        Debug.Log($"Successful Sales: {successfulSales}");
        Debug.Log($"Total Profit: ${totalShiftProfit}");
        Debug.Log($"====================================");

        if (customerLineText)
            customerLineText.text = $"Shift Complete! Total Profit: ${totalShiftProfit} | Sales: {successfulSales}/{customersServed}";

        // TODO: Load results scene
        // SceneManager.LoadScene("ResultsScene");
    }

    private void RefreshHUD()
    {
        if (profitText) profitText.text = $"Current: ${profit} | Shift: ${totalShiftProfit}";
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

    private float ComputePricePenalty(CustomerData cust, int offerPrice)
    {
        int over = offerPrice - cust.Budget;
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

    private string GenerateCustomerResponse(ChoiceData picked, bool priceChanged)
    {
        float patiencePercent = patience / patienceMax;
        float dealPercent = dealChance;

        bool isFairAsk = picked == choiceFairAsk;
        bool isDiscount = picked == choiceDiscountAsk;
        bool isPressure = picked == choicePressureSale;

        // HIGH PATIENCE + GOOD DEAL CHANCE (Positive, warming up)
        if (patiencePercent > 0.7f && dealPercent > 0.5f)
        {
            if (isDiscount)
                return GetRandomResponse(GetPositiveDiscountResponses(customer.Personality));
            if (isPressure)
                return GetRandomResponse(GetPositivePressureResponses(customer.Personality));
            if (isFairAsk)
                return GetRandomResponse(GetPositiveFairResponses(customer.Personality));
        }

        // MEDIUM PATIENCE (Neutral, considering)
        if (patiencePercent > 0.4f)
        {
            if (priceChanged && isDiscount)
                return GetRandomResponse(GetNeutralDiscountResponses(customer.Personality));
            if (isPressure)
                return GetRandomResponse(GetNeutralPressureResponses(customer.Personality));
            if (isFairAsk)
                return GetRandomResponse(GetNeutralFairResponses(customer.Personality));
        }

        // LOW PATIENCE (Frustrated, skeptical)
        if (patiencePercent <= 0.4f)
        {
            if (isPressure)
                return GetRandomResponse(GetNegativePressureResponses(customer.Personality));
            if (isDiscount)
                return GetRandomResponse(GetNegativeDiscountResponses(customer.Personality));
            if (isFairAsk)
                return GetRandomResponse(GetNegativeFairResponses(customer.Personality));
        }

        return "Let me think about this...";
    }

    // Helper to pick random response from array
    private string GetRandomResponse(string[] responses)
    {
        if (responses == null || responses.Length == 0) return "Hmm...";
        return responses[Random.Range(0, responses.Length)];
    }

    // ===== POSITIVE RESPONSES (High patience + good deal chance) =====
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
            _ => new[] {
                "That helps a lot, actually.",
                "Better! I'm warming up to this.",
                "The discount makes a difference."
            }
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
            _ => new[] {
                "You make a good point.",
                "I'm starting to see it your way.",
                "That's a fair argument."
            }
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
            _ => new[] {
                "I appreciate the honesty.",
                "You're being fair with me.",
                "That's a reasonable approach."
            }
        };
    }

    // ===== NEUTRAL RESPONSES (Medium patience) =====
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
            _ => new[] {
                "Okay, that's a step in the right direction.",
                "It helps, but I'm not there yet.",
                "Better, I guess."
            }
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
            _ => new[] {
                "Hold on, I'm still considering.",
                "Don't push me into this.",
                "I need more time."
            }
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
            _ => new[] {
                "I'm listening, but not convinced yet.",
                "Okay, but I'm not ready to commit.",
                "I need more information."
            }
        };
    }

    // ===== NEGATIVE RESPONSES (Low patience) =====
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
            _ => new[] {
                "Stop pushing. It's not working.",
                "You're being too aggressive.",
                "This isn't how I want to do business."
            }
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
            _ => new[] {
                "It's not just about price at this point...",
                "The discount isn't enough.",
                "I have bigger concerns than cost."
            }
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
            _ => new[] {
                "I don't know about this...",
                "I'm having serious doubts.",
                "This might not be for me."
            }
        };
    }
}