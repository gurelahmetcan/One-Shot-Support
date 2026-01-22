# Contract Negotiation System (SIMPLIFIED)

## Overview

The simplified contract negotiation system uses **only 3 variables** for contract offers:
1. **Signing Bonus** (integer gold, one-time payment)
2. **Salary** (integer gold per turn)
3. **Contract Length** (integer years)

**No more loot cut!** All compensation is handled through upfront payment and recurring salary.

## Core Formula

### Hero's Expected Value (Vexp)
```
Vexp = ((Prowess + Charisma + Vitality) × 2) × Lifecycle Multiplier + Greed Premium
```
- **Lifecycle Multipliers:** Rookie 0.8x, Prime 1.2x, Veteran 1.5x
- **Greed Premium:** Vexp × (Greed / 100)
- **Result:** Integer gold value

### Offer Value (Voff)
```
Voff = Signing Bonus + (Salary × 4 × Contract Length)
```
- **4 turns per year** (seasons)
- **Result:** Integer gold value

### Goal
Make `Voff ≥ Vexp` to satisfy the hero and avoid tension buildup.

## Player Strategy

Players must decide how to distribute the hero's expected value:

### Front-Load Strategy
- **High signing bonus**, low salary
- **Example:** 200g signing + 25g/turn × 2 years = 400g total
- **Good for:** When you have cash reserves, want predictable low recurring costs

### Spread Out Strategy
- **Low/zero signing bonus**, high salary
- **Example:** 0g signing + 50g/turn × 2 years = 400g total
- **Good for:** When you're low on cash now but expect steady income

### Long-Term Budget Strategy
- **Moderate signing**, low salary, **long contract**
- **Example:** 100g signing + 15g/turn × 5 years = 400g total
- **Good for:** Committing long-term with minimal per-turn costs

### Short Contract Premium
- **Very high signing**, moderate salary, **short contract**
- **Example:** 300g signing + 25g/turn × 1 year = 400g total
- **Good for:** Testing a hero without long commitment (but expensive upfront)

## Payment Preferences (NEW!)

Heroes have **payment preferences** based on their traits:

### Prefers Signing Bonus
- **Traits:** Greedy, Impulsive, Impatient
- **Wants:** ≥30% of total value as signing bonus
- **Penalty:** +10% tension if signing bonus < 30%

### Prefers Salary
- **Traits:** Cautious, Patient, Steady
- **Wants:** ≤20% of total value as signing bonus
- **Penalty:** +10% tension if signing bonus > 20%

### Neutral
- **All other heroes**
- **No penalty** for any payment structure

## Example Calculations

### Example Hero: Aria (Prime, Greedy)
**Stats:**
- Prowess: 12, Charisma: 10, Vitality: 50
- Greed: 60
- Stage: Prime (1.2x multiplier)
- Trait: Greedy (1.3x Vexp modifier, prefers signing bonus)

**Vexp Calculation:**
```
Core Stats = 12 + 10 + 50 = 72
Base = 72 × 2 = 144
Lifecycle = 144 × 1.2 = 172.8
Greed Premium = 172.8 × 0.6 = 103.68
Trait Modifier = (172.8 + 103.68) × 1.3 = 359g
```
**Vexp = 359g**

**Ideal Offer (respecting "Greedy" preference):**
- Signing: 40% = 144g
- Salary: 60% = 215g over contract
- Contract: 2 years (8 turns)
- Salary per turn: 215 ÷ 8 = 27g/turn

**Final Offer:** 144g signing + 27g/turn × 2 years = **360g total**

**Payment Preference Check:**
- Signing %: 144 / 360 = 40% ✅ (≥30% requirement met, no penalty)

## Tension Mechanics

### Starting Tension
- **75-100% Trust:** 0% tension
- **25-75% Trust:** Linear interpolation
- **0-25% Trust:** 25% tension

### Tension Delta
```
Base Delta = ((Vexp - Voff) / Vexp) × 100%
Contract Length Mitigation = (Length - 1) × 10%
Final Delta = Base × (1 - Mitigation) × Trait Modifier + Payment Penalty
```

**Contract Length Mitigation:**
- 1 year: 0% reduction
- 2 years: 10% reduction
- 3 years: 20% reduction
- 4 years: 30% reduction
- 5 years: 40% reduction (capped at 50%)

**Payment Penalty:**
- +10% tension if payment preference violated
- Only applies to heroes with preferences (Greedy/Impulsive/Cautious/Patient)

### Walk-Away
- Hero walks away at **100% tension**
- Hero stays in tavern but **grayed out**
- **4-turn lockout** before re-recruitment possible
- Removed during annual refresh

## Trait System

### Vexp Modifiers (Expectations)
| Trait | Modifier | Effect |
|-------|----------|--------|
| Greedy | 1.3× | Expects 30% more value |
| Ambitious | 1.2× | Expects 20% more value |
| Frugal/Humble | 0.7× | Expects 30% less value |
| Loyal | 0.85× | Expects 15% less value |

### Tension Modifiers (Negotiation Difficulty)
| Trait | Modifier | Effect |
|-------|----------|--------|
| Hotheaded/Impatient | 1.5× | Tension builds 50% faster |
| Stubborn | 1.3× | Tension builds 30% faster |
| Patient/Calm | 0.7× | Tension builds 30% slower |
| Flexible | 0.8× | Tension builds 20% slower |

### Payment Preferences (NEW!)
| Trait | Preference | Requirement |
|-------|------------|-------------|
| Greedy/Impulsive/Impatient | Signing Bonus | ≥30% upfront |
| Cautious/Patient/Steady | Salary | ≤20% upfront |
| Others | Neutral | No requirement |

## Usage in Code

### Calculate Expected Value
```csharp
int vexp = ContractNegotiationManager.Instance.CalculateHeroExpectedValue(hero);
// Returns: 400
```

### Calculate Offer Value
```csharp
ContractOffer offer = new ContractOffer(200, 25, 2); // signing, salary, years
int voff = ContractNegotiationManager.Instance.CalculateOfferValue(offer);
// Returns: 200 + (25 × 4 × 2) = 400
```

### Get Ideal Offer
```csharp
ContractOffer ideal = ContractNegotiationManager.Instance
    .CalculateIdealOffer(hero, desiredLength: 2);
// Automatically adjusts for payment preference
```

### Check Payment Preference
```csharp
PaymentPreference pref = ContractNegotiationManager.Instance
    .GetPaymentPreference(hero);
// Returns: PrefersSigningBonus, PrefersSalary, or Neutral
```

### Calculate Tension
```csharp
int tensionDelta = ContractNegotiationManager.Instance
    .CalculateTensionDelta(hero, offer, currentTension);
// Includes payment preference penalty if violated
```

## Benefits of Simplified System

### For Players
1. **Easier to understand:** 3 variables instead of 4
2. **Clear trade-offs:** Pay now vs pay later
3. **Budget management:** Manage cash flow like a real business
4. **Strategic depth:** Risk management (hero might die = lost signing bonus)

### For Designers
1. **Simpler balancing:** One less variable to tune
2. **Integer math:** No float rounding issues
3. **Clear formula:** Voff = Signing + (Salary × 4 × Length)
4. **Easy testing:** All calculations in console logs

### For Implementation
1. **Less UI complexity:** 3 sliders instead of 4
2. **Faster calculations:** No loot cut revenue lookup
3. **Cleaner code:** Removed GetExpectedSeasonRevenue logic
4. **Integer-only:** No need for float formatting

## Testing

Use `ContractNegotiationTester` component:

### Quick Test
1. Assign test hero in Inspector
2. Right-click component → "Quick Test: Current Configuration"
3. See Vexp, ideal offer, payment preference

### Full Test Suite
1. Right-click component → "Run Complete Negotiation Test"
2. Runs 9 comprehensive tests
3. All results in console with detailed breakdowns

### Individual Tests
- Test 1: Hero Information
- Test 2: Expected Value (Vexp)
- Test 3: Payment Preference (**NEW!**)
- Test 4: Ideal Offer
- Test 5: Custom Offer Evaluation
- Test 6: Starting Tension
- Test 7: Tension Delta
- Test 8: Payment Preference Violation (**NEW!**)
- Test 9: Walk-Away Mechanics

## Configuration

All values configurable in Inspector:

### Lifecycle Multipliers
- Rookie: 0.8 (default)
- Prime: 1.2 (default)
- Veteran: 1.5 (default)

### Tension Settings
- Reduction per year: 0.10 (10% per year)
- Zero tension threshold: 75% trust
- Max tension threshold: 25% trust
- Max starting tension: 25%
- Walk-away threshold: 100%

### Payment Preference Settings (**NEW!**)
- Min signing bonus (Greedy): 30% (0.3)
- Max signing bonus (Cautious): 20% (0.2)
- Payment preference penalty: 10%
- Re-recruitment lockout: 4 turns

## Summary

The simplified system achieves the same strategic depth with:
- **3 variables** instead of 4 (removed loot cut)
- **Integer math** throughout (no floats)
- **Payment preferences** add new layer of complexity
- **Clearer player decisions** (front-load vs spread out vs long-term)
- **Simpler implementation** (less code, easier balancing)

Players must balance:
1. **Cash flow:** Do I have gold now or later?
2. **Risk:** What if hero dies? (Lost signing bonus)
3. **Commitment:** Long contract = tension mitigation but locked in
4. **Preferences:** Respect hero's payment style or pay tension penalty

This creates a compelling negotiation mini-game with clear inputs and strategic depth!
