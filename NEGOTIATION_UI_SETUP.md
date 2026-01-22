# Negotiation UI Setup Guide

This guide explains how to set up the negotiation UI in Unity after the code integration.

## Overview

The negotiation system uses 3 main components:
1. **TavernScreen** - Shows available heroes with "Negotiate" button
2. **NegotiationPanel** - The contract negotiation interface
3. **ContractNegotiationManager** - Backend logic (already in scene)

## Scene Setup

### 1. Add ContractNegotiationManager to Scene

If not already present:
1. Create empty GameObject: "ContractNegotiationManager"
2. Add Component → Contract Negotiation Manager
3. Configure in Inspector:
   - Lifecycle Multipliers: Rookie 0.8, Prime 1.2, Veteran 1.5
   - Tension Reduction Per Year: 0.10
   - Zero Tension Trust Threshold: 75
   - Max Tension Trust Threshold: 25
   - Max Starting Tension: 25
   - Walk Away Threshold: 100
   - Payment Preference Settings:
     - Min Signing Bonus Percentage: 0.3 (30%)
     - Max Signing Bonus Percentage: 0.2 (20%)
     - Payment Preference Penalty: 10
   - Re Recruitment Lockout Turns: 4

### 2. Create Negotiation Panel UI

Create a new UI Panel as a child of your Canvas:

```
Canvas
└── NegotiationPanel
    ├── Background (Image - contract paper texture)
    ├── HeroDisplay
    │   ├── HeroPortrait (Image)
    │   └── HeroNameText (TextMeshProUGUI)
    ├── InfoDisplay
    │   ├── ExpectedValueText (TextMeshProUGUI) → "Hero Expects: 400g"
    │   ├── CurrentOfferText (TextMeshProUGUI) → "Your Offer: 380g"
    │   └── PaymentPreferenceText (TextMeshProUGUI) → "💰 Prefers Signing Bonus"
    ├── TensionDisplay
    │   ├── TensionBar (Slider - 0 to 100)
    │   └── TensionText (TextMeshProUGUI) → "Tension: 45%"
    ├── ContractControls
    │   ├── SigningBonusSlider (Slider - 0 to 200)
    │   ├── SigningBonusValueText (TextMeshProUGUI) → "50g"
    │   ├── SalarySlider (Slider - 0 to 200)
    │   ├── SalaryValueText (TextMeshProUGUI) → "25g/turn"
    │   └── ContractLengthButtons
    │       ├── 1YearButton (Button)
    │       ├── 2YearButton (Button)
    │       ├── 3YearButton (Button)
    │       ├── 4YearButton (Button)
    │       └── 5YearButton (Button)
    └── ActionButtons
        ├── OfferButton (Button) → "Make Offer"
        └── CancelButton (Button) → "Cancel"
```

### 3. Configure NegotiationPanel Component

1. Select NegotiationPanel GameObject
2. Add Component → Negotiation Panel
3. Drag references from Inspector:

**Hero Display:**
- Hero Portrait → HeroPortrait Image
- Hero Name Text → HeroNameText TMP
- Hero Expected Value Text → ExpectedValueText TMP
- Current Offer Value Text → CurrentOfferText TMP
- Payment Preference Text → PaymentPreferenceText TMP

**Tension Display:**
- Tension Bar → TensionBar Slider
- Tension Text → TensionText TMP

**Contract Sliders:**
- Signing Bonus Slider → SigningBonusSlider Slider
- Signing Bonus Value Text → SigningBonusValueText TMP
- Salary Slider → SalarySlider Slider
- Salary Value Text → SalaryValueText TMP

**Contract Length Buttons:**
- One Year Button → 1YearButton Button
- Two Year Button → 2YearButton Button
- Three Year Button → 3YearButton Button
- Four Year Button → 4YearButton Button
- Five Year Button → 5YearButton Button

**Action Buttons:**
- Offer Button → OfferButton Button
- Offer Button Text → OfferButton's Text child TMP
- Cancel Button → CancelButton Button

**Button Visual States:**
- Selected Year Color → Light Blue (RGB: 77, 179, 255)
- Normal Year Color → White (RGB: 255, 255, 255)

### 4. Update TavernScreen Reference

1. Select your TavernScreen GameObject
2. Find TavernScreen component
3. Drag NegotiationPanel into the "Negotiation Panel" field

### 5. Update TavernHeroSlot Button Text

For each TavernHeroSlot in your scene:
1. Find the Recruit Button
2. Change button text from "Recruit" to "Negotiate"
3. (Optional) Update button color/styling

## UI Element Details

### Tension Bar

**Setup:**
- Type: Slider
- Min Value: 0
- Max Value: 100
- Whole Numbers: ✓ Checked
- Fill Rect: Create a child Image for the fill
- Background: Optional gray bar

**Visual Feedback (Optional):**
- 0-25%: Green fill
- 25-50%: Yellow fill
- 50-75%: Orange fill
- 75-100%: Red fill

### Sliders (Signing Bonus & Salary)

**Setup:**
- Min Value: 0
- Max Value: 200
- Whole Numbers: ✓ Checked
- Handle: Draggable knob
- Fill Rect: Visual fill indicator

**Interactivity:**
- Real-time value updates as player drags
- Displays current value next to slider
- Updates offer total immediately

### Contract Length Buttons

**Setup:**
- Type: Button
- Text: "1 Year", "2 Years", "3 Years", "4 Years", "5 Years"
- Layout: Horizontal Layout Group recommended

**Visual States:**
- Normal: White background
- Selected: Light blue background (configured in component)
- Only one button selected at a time (toggle group behavior)

### Offer Button

**States:**
- Enabled: "Make Offer" (player has enough gold)
- Disabled: "Insufficient Gold" (player can't afford signing bonus)

**Interactivity:**
- Checks player gold vs signing bonus
- Disabled if signing bonus > player gold
- Clicking triggers offer evaluation and tension update

## Flow Diagram

```
Tavern View
    |
    v
[Click "Negotiate" on Hero]
    |
    v
Negotiation Panel Opens
    |
    +-- Shows Hero Portrait
    +-- Shows "Hero Expects: Xg"
    +-- Shows Payment Preference
    +-- Sliders set to Ideal Offer
    +-- 2 Years selected by default
    +-- Tension bar shows starting tension
    |
    v
Player Adjusts Offer
    |
    +-- Drag Signing Bonus Slider (0-200g)
    +-- Drag Salary Slider (0-200g/turn)
    +-- Click Contract Length (1-5 years)
    +-- "Your Offer" updates in real-time
    |
    v
[Click "Make Offer"]
    |
    +-- Calculate Tension Delta
    +-- Update Tension Bar
    |
    v
   Hero Response
    |
    +-- If Tension < 100%:
    |      ✅ Hero Accepts!
    |      → Deduct signing bonus from gold
    |      → Add hero to barracks
    |      → Close panel, return to tavern
    |
    +-- If Tension >= 100%:
           ⚠️ Hero Walks Away!
           → Mark hero as walked away
           → Close panel, return to tavern
           → Hero shows grayed out in tavern
```

## Testing Checklist

### Basic Functionality
- [ ] Negotiate button appears in tavern
- [ ] Clicking negotiate opens negotiation panel
- [ ] Hero portrait displays correctly
- [ ] Expected value shows correctly
- [ ] Payment preference displays correctly
- [ ] Sliders respond to dragging
- [ ] Slider values update text displays
- [ ] Contract length buttons toggle correctly
- [ ] "Your Offer" updates in real-time
- [ ] Cancel button closes panel

### Offer Mechanics
- [ ] Offer button enables when player has gold
- [ ] Offer button disables when signing bonus > gold
- [ ] Button text changes to "Insufficient Gold"
- [ ] Clicking offer calculates tension
- [ ] Tension bar updates after offer
- [ ] Low offers increase tension
- [ ] Fair offers don't change tension
- [ ] Generous offers decrease tension

### Payment Preferences
- [ ] Greedy heroes show "Prefers Signing Bonus"
- [ ] Cautious heroes show "Prefers Steady Salary"
- [ ] Violating preference adds +10% tension
- [ ] Payment preference shown in panel

### Walk-Away
- [ ] Hero walks away at 100% tension
- [ ] Panel auto-closes on walk-away
- [ ] Hero shows grayed out in tavern
- [ ] Hero shows "Walked Away" message
- [ ] Hero can't be negotiated with again

### Success Path
- [ ] Hero accepts when tension < 100%
- [ ] Signing bonus deducted from gold
- [ ] Hero added to barracks
- [ ] Hero removed from tavern
- [ ] Panel closes and returns to tavern
- [ ] Gold display updates correctly

## Styling Recommendations

### Color Scheme

**Tension Bar:**
- Background: Dark gray (#333333)
- 0-25%: Green (#4CAF50)
- 25-50%: Yellow (#FFEB3B)
- 50-75%: Orange (#FF9800)
- 75-100%: Red (#F44336)

**Contract Paper Background:**
- Parchment texture or beige (#F5E6D3)
- Slight shadow/border for depth

**Buttons:**
- Contract Length (Normal): White (#FFFFFF)
- Contract Length (Selected): Light Blue (#4DB3FF)
- Offer Button (Enabled): Green (#4CAF50)
- Offer Button (Disabled): Gray (#BDBDBD)
- Cancel Button: Red (#F44336)

**Text:**
- Headers: Bold, 18-24pt
- Values: Regular, 14-16pt
- Descriptions: Italic, 12-14pt

### Layout Tips

1. **Panel Size:** ~600x800px (centered on screen)
2. **Spacing:** 10-20px padding around elements
3. **Alignment:** Center-aligned for symmetry
4. **Sliders:** 300-400px wide for easy dragging
5. **Buttons:** ~100-150px wide, 40-50px tall

## GameManager Integration

The TavernScreen now fires different events:

**Old Event:**
```csharp
OnHeroRecruited?.Invoke(HeroData hero)
```

**New Events:**
```csharp
OnHeroRecruited?.Invoke(HeroData hero, ContractOffer offer)
OnHeroWalkedAway?.Invoke(HeroData hero)
```

### Update GameManager

In your GameManager's TavernScreen setup:
```csharp
tavernScreen.OnHeroRecruited += (hero, offer) => {
    // Deduct signing bonus
    goldManager.SpendGold(offer.signingBonus);

    // Add to barracks
    barracksHeroes.Add(hero);

    // Remove from tavern
    tavernHeroes.Remove(hero);

    // Update displays
    uiManager.RefreshTavern();
    uiManager.RefreshBarracks();
};

tavernScreen.OnHeroWalkedAway += (hero) => {
    // Mark walk-away turn
    hero.walkAwayTurn = currentTurn;

    // Refresh tavern to show grayed out
    uiManager.RefreshTavern();
};
```

## Common Issues

### Sliders Not Updating
- Check that "Whole Numbers" is enabled on sliders
- Verify slider min/max values (0 to 200)
- Ensure value text references are assigned

### Buttons Not Toggling
- Check Selected Year Color is different from Normal
- Verify all 5 buttons are assigned
- Make sure color blocks are set correctly

### Offer Button Always Disabled
- Check GoldManager.Instance is in scene
- Verify GoldManager.CurrentGold property works
- Test with high gold amount

### Tension Not Updating
- Ensure ContractNegotiationManager is in scene
- Check that it's a Singleton (Instance not null)
- Verify tension calculations in console logs

### Payment Preference Not Showing
- Check hero has traits assigned
- Verify trait names contain keywords (Greedy, Cautious, etc.)
- Look at console logs for detected preference

## Next Steps

After UI setup:
1. **Create Trait ScriptableObjects** with payment preference keywords
2. **Test with different heroes** (Greedy, Cautious, Neutral)
3. **Tune tension values** based on gameplay feel
4. **Add animations** for panel open/close
5. **Add sound effects** for button clicks and walk-aways
6. **Implement walked-away hero visual** (gray out, locked icon)

## Summary

The negotiation UI is now integrated! Players will:
1. See "Negotiate" button in tavern
2. Open negotiation panel with sliders
3. Adjust signing bonus, salary, and length
4. See real-time offer value updates
5. Click "Make Offer" to submit
6. See tension update based on fairness
7. Hero accepts or walks away
8. Return to tavern view

All backend logic is complete and ready to use!
