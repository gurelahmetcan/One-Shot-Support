# Contract Negotiation System Documentation

## Overview

The Contract Negotiation System implements value-based hero recruitment with dynamic tension mechanics. Heroes now have expected values based on their stats and lifecycle stage, and negotiations become a balancing act between meeting hero demands and maintaining guild profit margins.

## Core Components

### 1. ContractNegotiationManager (`Core/ContractNegotiationManager.cs`)

Central manager for all negotiation mechanics. Handles:
- Value calculations (Vexp and Voff)
- Tension mechanics
- Contract finalization
- Walk-away logic

**Key Methods:**
- `CalculateHeroExpectedValue(hero)` - Calculates what a hero thinks they're worth
- `CalculateOfferValue(hero, offer)` - Calculates the value of a contract offer
- `CalculateIdealOffer(hero, length)` - Creates an offer where Voff = Vexp
- `CalculateTensionDelta(hero, offer, currentTension)` - Determines tension change
- `ApplyTensionChange(hero, delta, ref tension)` - Updates tension and checks walk-away

### 2. HeroData Extensions (`ScriptableObjects/HeroData.cs`)

Added negotiation state tracking:
- `hasWalkedAway` - Whether hero walked away from negotiations
- `walkAwayTurn` - Turn when walk-away occurred
- `isLockedFromRecruitment` - UI display flag
- `currentTension` - Current negotiation tension (0-100%)
- `trustLevel` - Guild trust level (0-100%, affects starting tension)

**New Methods:**
- `MarkAsWalkedAway(turn)` - Records walk-away event
- `CanBeReRecruited(currentTurn, lockout)` - Checks re-recruitment availability
- `ResetWalkAwayStatus()` - Clears walk-away flags (annual refresh)
- `InitializeNegotiation()` - Sets up negotiation with starting tension

### 3. ContractOffer Data Structure

Represents a contract proposal:
```csharp
public class ContractOffer
{
    public float signingBonus;          // One-time payment
    public float dailySalary;           // Gold per turn
    public float lootCutPercentage;     // 0-100%
    public int contractLengthYears;     // Years duration
}
```

## Value Calculation Formulas

### Hero's Expected Value (Vexp)

**Formula:**
```
Vexp = ((Prowess + Charisma + Vitality) × 2) × LifecycleMultiplier × TraitModifier
Greed Premium = Vexp × (Greed / 100)
Total Vexp = Vexp + Greed Premium
```

**Lifecycle Multipliers:**
- Rookie (20-25 years): 0.8x
- Prime (26-35 years): 1.2x
- Veteran (36-45 years): 1.5x

**Example:**
- Hero: Prowess 12, Charisma 10, Vitality 50, Greed 60, Stage: Prime
- Base: (12 + 10 + 50) × 2 = 144
- Lifecycle: 144 × 1.2 = 172.8
- Greed Premium: 172.8 × 0.6 = 103.68
- **Total Vexp: 276.48 gold**

### Offer Value (Voff)

**Formula:**
```
Voff = SigningBonus + (Salary × 4 × Length) + (LootCut% × ExpectedSeasonRevenue)
```

**Expected Season Revenue (per 1% Loot Cut):**
- Rookie: 50 gold
- Prime: 125 gold
- Veteran: 200 gold

**Example:**
- Offer: 50g signing, 15g/turn salary, 25% loot, 2 years
- Prime Hero Expected Revenue: 125g per 1% = 3125g for 25%
- Signing: 50g
- Salary: 15 × 4 × 2 = 120g
- Loot: 25 × 125 = 3125g
- **Total Voff: 3295 gold**

### Initial Demands (Ideal Offer)

The system calculates an "ideal offer" where Voff = Vexp:
- Signing Bonus: 10% of Vexp
- Salary: 60% of remaining value
- Loot Cut: 40% of remaining value

This provides the starting "anchor" for negotiations.

## Tension Mechanics

### Starting Tension

Based on hero's trust level with the guild:

**Formula:**
- **75-100% Trust:** 0% Tension
- **25-75% Trust:** Linear interpolation: `(75 - Trust) × 0.5`
- **0-25% Trust:** 25% Tension (maximum)

**Examples:**
- 100% Trust → 0% Tension
- 75% Trust → 0% Tension
- 50% Trust → 12.5% Tension
- 25% Trust → 25% Tension
- 0% Trust → 25% Tension

### Tension Delta

Calculated when making an offer:

**Formula:**
```
1. Value Difference = Vexp - Voff
2. Percentage Difference = (Value Difference / Vexp) × 100
3. Base Tension Delta = Percentage Difference
4. Contract Length Mitigation = (Contract Length - 1) × 0.10
5. Final Delta = Base Delta × (1 - Mitigation) × Trait Modifier
```

**Contract Length Mitigation:**
- 1 year: 0% reduction
- 2 years: 10% reduction
- 3 years: 20% reduction
- 4 years: 30% reduction
- 5 years: 40% reduction (capped at 50%)

**Example:**
- Vexp: 300, Voff: 240 (20% underpayment)
- Base Delta: +20% tension
- 2-year contract: 10% mitigation → 20% × 0.9 = +18% tension
- 3-year contract: 20% mitigation → 20% × 0.8 = +16% tension

### Walk-Away Threshold

- Heroes walk away at **100% tension**
- Walked-away heroes are **grayed out** in tavern
- They remain **locked from recruitment**
- **Re-recruitment lockout:** 4 turns (1 year/annual refresh)
- After lockout expires during annual refresh, heroes are removed from tavern

## Trait Modifiers

### Vexp Modifiers (Hero Expectations)

Traits affect what heroes expect to be paid:

| Trait Type | Modifier | Effect |
|------------|----------|--------|
| Greedy | 1.3× | Expects 30% more value |
| Ambitious | 1.2× | Expects 20% more value |
| Frugal/Humble | 0.7× | Expects 30% less value |
| Loyal | 0.85× | Expects 15% less value |

### Tension Modifiers (Negotiation Difficulty)

Traits affect how quickly tension builds:

| Trait Type | Modifier | Effect |
|------------|----------|--------|
| Hotheaded/Impatient | 1.5× | Tension builds 50% faster |
| Stubborn | 1.3× | Tension builds 30% faster |
| Patient/Calm | 0.7× | Tension builds 30% slower |
| Flexible | 0.8× | Tension builds 20% slower |

**Example:**
- Base tension delta: +20%
- Hero with "Hotheaded" trait: 20% × 1.5 = **+30% tension**
- Hero with "Patient" trait: 20% × 0.7 = **+14% tension**

## Implementation Guide

### Setting Up the System

1. **Add ContractNegotiationManager to scene:**
   ```csharp
   // Create empty GameObject
   GameObject negotiationObj = new GameObject("ContractNegotiationManager");
   negotiationObj.AddComponent<ContractNegotiationManager>();
   ```

2. **Configure values in Inspector:**
   - Lifecycle Multipliers (Rookie: 0.8, Prime: 1.2, Veteran: 1.5)
   - Expected Season Revenue (Rookie: 50, Prime: 125, Veteran: 200)
   - Tension settings (reduction per year: 0.10, thresholds, etc.)

### Using in Recruitment Flow

```csharp
// 1. Initialize negotiation when hero is selected
hero.InitializeNegotiation(); // Sets starting tension based on trust

// 2. Calculate ideal offer to show player
ContractOffer idealOffer = ContractNegotiationManager.Instance
    .CalculateIdealOffer(hero, desiredContractLength);

// 3. Set UI sliders to ideal values
signingBonusSlider.value = idealOffer.signingBonus;
salarySlider.value = idealOffer.dailySalary;
lootCutSlider.value = idealOffer.lootCutPercentage;

// 4. When player adjusts offer, calculate tension delta
ContractOffer playerOffer = new ContractOffer(
    currentSigningBonus,
    currentSalary,
    currentLootCut,
    currentLength
);

float tensionDelta = ContractNegotiationManager.Instance
    .CalculateTensionDelta(hero, playerOffer, hero.currentTension);

// 5. Apply tension change
bool walkedAway = ContractNegotiationManager.Instance
    .ApplyTensionChange(hero, tensionDelta, ref hero.currentTension);

if (walkedAway)
{
    // Mark hero as walked away
    hero.MarkAsWalkedAway(GameManager.Instance.currentTurn);
    // Gray out UI, show "Walked Away" message
}

// 6. Finalize contract when player confirms
if (!walkedAway && playerConfirms)
{
    ContractNegotiationManager.Instance.FinalizeContract(hero, playerOffer);
    // Add hero to roster, deduct signing bonus
}
```

### Walk-Away Handling

```csharp
// Check if hero can be re-recruited
bool canRecruit = hero.CanBeReRecruited(
    GameManager.Instance.currentTurn,
    lockoutTurns: 4
);

// During annual refresh (every 4 turns)
if (GameManager.Instance.currentTurn % 4 == 0)
{
    foreach (var hero in tavernHeroes)
    {
        if (hero.hasWalkedAway)
        {
            // Remove walked-away heroes during annual refresh
            tavernHeroes.Remove(hero);
        }
    }
}
```

### UI Display Recommendations

1. **Negotiation Screen:**
   - Show Vexp prominently ("Hero expects: 300 gold value")
   - Show current Voff in real-time ("Your offer: 280 gold value")
   - Display tension meter (0-100%) with color coding
   - Show "Ideal" button to reset sliders to ideal offer

2. **Tension Meter Colors:**
   - 0-25%: Green (happy)
   - 25-50%: Yellow (cautious)
   - 50-75%: Orange (upset)
   - 75-100%: Red (angry)

3. **Walked-Away Heroes:**
   - Gray out portrait and stats
   - Show "Walked Away" badge
   - Display lockout timer: "Available in X turns"
   - Disable "Recruit" button

## Testing

### Using ContractNegotiationTester

Add `ContractNegotiationTester` component to test the system:

```csharp
// Assign a test hero in Inspector
// Enable "Run Test On Start" for automatic testing

// Or use Context Menu options:
// - "Run Complete Negotiation Test" - Full test suite
// - "Quick Test: Current Configuration" - Fast summary
// - Individual tests (Test 1-7) for specific mechanics
```

**Test Coverage:**
1. Hero Information Display
2. Expected Value Calculation (Vexp)
3. Ideal Offer Calculation
4. Custom Offer Evaluation
5. Starting Tension Calculation
6. Tension Delta Calculation
7. Walk-Away Mechanics Simulation

### Debug Console Output

All calculations log detailed information:
```
[ContractNegotiation] Aria Vexp: Base=144.0, Lifecycle=172.8,
    Greed Premium=103.7, Trait Modifier=1.00, Total=276.5
[ContractNegotiation] Aria Voff: Signing=50.0, Salary=120.0,
    LootCut=3125.0, Total=3295.0
[ContractNegotiation] Aria Tension Delta: ValueDiff=-3018.5 (-1091.9%),
    LengthMitigation=0.10, TraitMod=1.00, Delta=-982.7
```

## Future Extensions (Not Yet Implemented)

These features are designed but not yet implemented:

### Contract Extension/Renegotiation (Answer 8)
- Notification when contract expires
- Options: Re-negotiate, Release, or Retire (46+ age)
- Higher base demands for leveled-up heroes

### Bond Level Effects (Answer 9)
- High bond reduces initial tension
- Effective Greed formula: `BaseGreed × (1 - Bond/200)`
- At 100 Bond, 50% less greedy during negotiations

## Configuration Recommendations

### Balanced Settings (Default)
```
Lifecycle Multipliers:
- Rookie: 0.8x
- Prime: 1.2x
- Veteran: 1.5x

Expected Season Revenue:
- Rookie: 50 gold/1%
- Prime: 125 gold/1%
- Veteran: 200 gold/1%

Tension:
- Reduction per year: 10%
- Zero tension threshold: 75% trust
- Max tension threshold: 25% trust
- Max starting tension: 25%
- Walk-away threshold: 100%
- Re-recruitment lockout: 4 turns
```

### Difficulty Variants

**Easy Mode (Player-Friendly):**
- Rookie: 0.7x, Prime: 1.0x, Veteran: 1.2x
- Tension reduction: 15% per year
- Walk-away threshold: 120%

**Hard Mode (Challenge):**
- Rookie: 0.9x, Prime: 1.4x, Veteran: 1.8x
- Tension reduction: 5% per year
- Walk-away threshold: 80%
- Max starting tension: 40%

## Summary

The Contract Negotiation System transforms hero recruitment from a static transaction into a dynamic negotiation mini-game. Players must balance:

1. **Profit Margins** - Paying too much eats into mission profits
2. **Hero Satisfaction** - Underpaying increases tension
3. **Contract Length** - Longer contracts mitigate tension but lock in terms
4. **Hero Traits** - Some heroes are easier/harder to negotiate with
5. **Trust Building** - High trust heroes start with less tension

The system uses backend logic first, making it easy to test in the console before adding UI polish. All formulas are configurable through the Inspector for easy balancing.
