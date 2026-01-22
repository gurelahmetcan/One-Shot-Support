# Example Negotiation Traits

This document provides example trait configurations that affect contract negotiations. Use these as templates when creating HeroTrait ScriptableObjects in Unity.

## How Traits Affect Negotiations

Traits are detected by the ContractNegotiationManager using **name-based matching**. The system checks if the trait name contains specific keywords (case-insensitive) and applies the corresponding modifiers.

### Vexp Modifiers (Hero Expectations)
These traits change what heroes expect to be paid:
- **"Greedy"**: 1.3× multiplier (expects 30% more)
- **"Ambitious"**: 1.2× multiplier (expects 20% more)
- **"Frugal" or "Humble"**: 0.7× multiplier (expects 30% less)
- **"Loyal"**: 0.85× multiplier (expects 15% less)

### Tension Modifiers (Negotiation Difficulty)
These traits change how quickly tension builds:
- **"Hotheaded" or "Impatient"**: 1.5× multiplier (tension builds 50% faster)
- **"Stubborn"**: 1.3× multiplier (tension builds 30% faster)
- **"Patient" or "Calm"**: 0.7× multiplier (tension builds 30% slower)
- **"Flexible"**: 0.8× multiplier (tension builds 20% slower)

## Example Trait Configurations

### Negotiation-Focused Traits

#### 1. Greedy
**Makes negotiations harder - hero expects much more**
```
Trait Name: Greedy
Description: This hero has expensive tastes and demands premium compensation.

Stat Modifiers:
- Prowess: 0
- Charisma: 0
- Vitality: 0

Contract Modifiers:
- Salary Modifier: +30% (+0.3)
- Loot Cut Modifier: +30% (+0.3)

Mission Modifiers:
- Mission Success: 0%
- Damage Taken: 0%
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 1.3× (expects 30% more value)
- Tension Modifier: 1.0× (normal)
```

#### 2. Frugal
**Makes negotiations easier - hero expects less**
```
Trait Name: Frugal
Description: A modest hero who lives within their means and doesn't ask for much.

Stat Modifiers:
- Prowess: 0
- Charisma: +2
- Vitality: 0

Contract Modifiers:
- Salary Modifier: -20% (-0.2)
- Loot Cut Modifier: -20% (-0.2)

Mission Modifiers:
- Mission Success: 0%
- Damage Taken: 0%
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 0.7× (expects 30% less value)
- Tension Modifier: 1.0× (normal)
```

#### 3. Ambitious
**Moderate difficulty - hero wants to prove themselves**
```
Trait Name: Ambitious
Description: Driven to succeed and expects compensation that reflects their potential.

Stat Modifiers:
- Prowess: +1
- Charisma: +1
- Vitality: 0

Contract Modifiers:
- Salary Modifier: +15% (+0.15)
- Loot Cut Modifier: +15% (+0.15)

Mission Modifiers:
- Mission Success: +5% (+0.05)
- Damage Taken: 0%
- Fame Gain: +20% (+0.2)

Negotiation Effects (Automatic):
- Vexp Modifier: 1.2× (expects 20% more value)
- Tension Modifier: 1.0× (normal)
```

#### 4. Loyal
**Easier negotiations with bonded guild**
```
Trait Name: Loyal
Description: Dedicated to the guild and willing to accept fair terms.

Stat Modifiers:
- Prowess: 0
- Charisma: +2
- Vitality: +5

Contract Modifiers:
- Salary Modifier: -10% (-0.1)
- Loot Cut Modifier: -10% (-0.1)

Mission Modifiers:
- Mission Success: +5% (+0.05)
- Damage Taken: -10% (-0.1)
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 0.85× (expects 15% less value)
- Tension Modifier: 1.0× (normal)
```

#### 5. Hotheaded
**Tension builds very quickly**
```
Trait Name: Hotheaded
Description: Quick to anger when insulted by lowball offers.

Stat Modifiers:
- Prowess: +2
- Charisma: -2
- Vitality: 0

Contract Modifiers:
- Salary Modifier: 0%
- Loot Cut Modifier: 0%

Mission Modifiers:
- Mission Success: 0%
- Damage Taken: +10% (+0.1)
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 1.0× (normal expectations)
- Tension Modifier: 1.5× (tension builds 50% faster!)
```

#### 6. Patient
**Tension builds slowly - forgiving of initial lowballs**
```
Trait Name: Patient
Description: Calm and understanding, willing to negotiate fairly.

Stat Modifiers:
- Prowess: 0
- Charisma: +2
- Vitality: +5

Contract Modifiers:
- Salary Modifier: 0%
- Loot Cut Modifier: 0%

Mission Modifiers:
- Mission Success: 0%
- Damage Taken: -5% (-0.05)
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 1.0× (normal expectations)
- Tension Modifier: 0.7× (tension builds 30% slower)
```

#### 7. Stubborn
**Both expects more AND tension builds faster**
```
Trait Name: Stubborn
Description: Set in their ways and unlikely to budge on demands.

Stat Modifiers:
- Prowess: +1
- Charisma: -1
- Vitality: +5

Contract Modifiers:
- Salary Modifier: +10% (+0.1)
- Loot Cut Modifier: +10% (+0.1)

Mission Modifiers:
- Mission Success: 0%
- Damage Taken: +5% (+0.05)
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 1.0× (normal expectations)
- Tension Modifier: 1.3× (tension builds 30% faster)
```

#### 8. Flexible
**Easier negotiations overall**
```
Trait Name: Flexible
Description: Adaptable and open to compromise during negotiations.

Stat Modifiers:
- Prowess: 0
- Charisma: +3
- Vitality: 0

Contract Modifiers:
- Salary Modifier: 0%
- Loot Cut Modifier: 0%

Mission Modifiers:
- Mission Success: +5% (+0.05)
- Damage Taken: 0%
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 1.0× (normal expectations)
- Tension Modifier: 0.8× (tension builds 20% slower)
```

#### 9. Humble
**Very easy negotiations - great for tight budgets**
```
Trait Name: Humble
Description: Doesn't seek recognition or excessive compensation.

Stat Modifiers:
- Prowess: 0
- Charisma: +1
- Vitality: 0

Contract Modifiers:
- Salary Modifier: -25% (-0.25)
- Loot Cut Modifier: -25% (-0.25)

Mission Modifiers:
- Mission Success: 0%
- Damage Taken: 0%
- Fame Gain: -20% (-0.2)

Negotiation Effects (Automatic):
- Vexp Modifier: 0.7× (expects 30% less value)
- Tension Modifier: 1.0× (normal)
```

#### 10. Impatient
**Dangerous for negotiations - walks away quickly**
```
Trait Name: Impatient
Description: Expects fast negotiations and quickly loses patience with haggling.

Stat Modifiers:
- Prowess: +1
- Charisma: -1
- Vitality: 0

Contract Modifiers:
- Salary Modifier: +5% (+0.05)
- Loot Cut Modifier: +5% (+0.05)

Mission Modifiers:
- Mission Success: +5% (+0.05)
- Damage Taken: 0%
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 1.0× (normal expectations)
- Tension Modifier: 1.5× (tension builds 50% faster!)
```

### Combo Traits (Multiple Keywords)

#### 11. Greedy Hothead
**Nightmare tier negotiation - expects more AND easily angered**
```
Trait Name: Greedy Hothead
Description: Expects top-tier compensation and flies into a rage at lowball offers.

Stat Modifiers:
- Prowess: +2
- Charisma: -2
- Vitality: 0

Contract Modifiers:
- Salary Modifier: +40% (+0.4)
- Loot Cut Modifier: +40% (+0.4)

Mission Modifiers:
- Mission Success: 0%
- Damage Taken: +15% (+0.15)
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 1.3× (expects 30% more) [from "Greedy"]
- Tension Modifier: 1.5× (builds 50% faster) [from "Hothead"]
- Combined: Expects way more AND gets angry fast!
```

#### 12. Loyal and Patient
**Dream hero - easy negotiations**
```
Trait Name: Loyal and Patient
Description: A reliable guild member who negotiates in good faith.

Stat Modifiers:
- Prowess: 0
- Charisma: +3
- Vitality: +5

Contract Modifiers:
- Salary Modifier: -15% (-0.15)
- Loot Cut Modifier: -15% (-0.15)

Mission Modifiers:
- Mission Success: +10% (+0.1)
- Damage Taken: -10% (-0.1)
- Fame Gain: 0%

Negotiation Effects (Automatic):
- Vexp Modifier: 0.85× (expects 15% less) [from "Loyal"]
- Tension Modifier: 0.7× (builds 30% slower) [from "Patient"]
- Combined: Expects less AND very forgiving!
```

## Creating Traits in Unity

### Step-by-Step Process

1. **Right-click in Project window**
   - Navigate to: Assets → Resources → Traits (or desired folder)
   - Create → One-Shot Support → Hero Trait

2. **Name the asset**
   - Use descriptive names that match the keywords above
   - Example: "Trait_Greedy", "Trait_Patient", etc.

3. **Configure in Inspector**
   - Fill in Trait Name (this is what the system checks!)
   - Add Description (shown to player)
   - Set Stat Modifiers (if any)
   - Set Contract Modifiers (salary and loot cut)
   - Set Mission Modifiers (optional)

4. **Negotiation effects are automatic**
   - The ContractNegotiationManager reads the Trait Name
   - No need to manually configure Vexp or Tension modifiers
   - Just ensure the trait name contains the right keywords

### Important Notes

⚠️ **Trait Name is Key**: The system uses the `traitName` field (not the asset filename) to detect negotiation effects. Make sure the trait name contains the keywords like "Greedy", "Patient", etc.

⚠️ **Case Insensitive**: "Greedy", "greedy", and "GREEDY" all work the same way.

⚠️ **Multiple Keywords**: A trait named "Greedy Hothead" gets BOTH effects (1.3× Vexp AND 1.5× Tension).

⚠️ **Contract Modifiers**: The salaryModifier and lootCutModifier in the trait affect the EXISTING contract values, but don't directly impact Vexp calculations. They're applied when the contract is finalized.

## Balancing Guidelines

### Vexp Modifiers (Expectations)
- **Easy negotiation**: 0.7× - 0.85×
- **Normal**: 1.0×
- **Hard negotiation**: 1.2× - 1.3×
- **Very hard**: 1.4×+ (use sparingly!)

### Tension Modifiers (Build Rate)
- **Forgiving**: 0.5× - 0.8×
- **Normal**: 1.0×
- **Quick temper**: 1.2× - 1.5×
- **Hair trigger**: 1.6×+ (very dangerous!)

### Contract Modifiers
- Typically match the Vexp modifier for consistency
- Greedy (1.3× Vexp) → +30% salary/loot modifiers
- Frugal (0.7× Vexp) → -30% salary/loot modifiers
- These are applied AFTER negotiation when contract is finalized

## Testing Your Traits

Use the ContractNegotiationTester component:

1. Create a test hero with your new trait
2. Assign to ContractNegotiationTester.testHero
3. Run "Test 2: Expected Value" to see Vexp modifier
4. Run "Test 6: Tension Delta" to see tension modifier
5. Check console logs for "Trait Modifier" values

Example output:
```
[ContractNegotiation] Hero Vexp: TraitModifier=1.30 (Greedy detected)
[ContractNegotiation] Tension Delta: TraitMod=1.50 (Hotheaded detected)
```

## Summary

Negotiation traits add strategic depth to hero recruitment:
- **Greedy/Ambitious/Stubborn**: Harder negotiations, higher costs
- **Frugal/Humble/Loyal**: Easier negotiations, budget-friendly
- **Hotheaded/Impatient**: Fast tension buildup, risky
- **Patient/Calm/Flexible**: Slow tension buildup, forgiving
- **Combo traits**: Stack effects for extreme difficulties

Players must consider not just hero stats, but also their negotiation difficulty when building their roster. A highly skilled but greedy hero might not be worth the squeeze on profit margins!
