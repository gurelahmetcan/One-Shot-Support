# Pentagon Stat Display - Setup & Testing Guide

## Quick Setup (5 minutes)

### 1. Create Test Scene
1. Create a new empty GameObject in your scene: `PentagonTest`
2. Add a **Canvas** component if you don't have one already
3. Under the Canvas, create an **empty UI GameObject**: Right-click Canvas → UI → Panel (or Empty Object with RectTransform)
4. Name it `PentagonDisplay`

### 2. Add Pentagon Component
1. Select the `PentagonDisplay` GameObject
2. **Add Component** → `PentagonStatDisplay`
3. **Add Component** → `PentagonStatDisplayTester`

### 3. Configure Pentagon Display
In the Inspector for `PentagonDisplay`:

**PentagonStatDisplay Settings:**
- Radius: `150` (size of pentagon)
- Line Thickness: `3`
- Stats: Set all to `30` (Might, Charm, Wit, Agility, Fortitude)
- Fill Pentagon: ✓ (checked)
- Fill Color: Blue with some transparency (R:0.2, G:0.6, B:1, A:0.3)
- Outline Color: Bright blue (R:0.2, G:0.6, B:1, A:1)

**PentagonStatDisplayTester Settings:**
- Pentagon Display: Drag the same GameObject here (self-reference)
- Auto Cycle Scenarios: ✓ (to see automatic demos)
- Cycle Interval: `3` seconds

### 4. Run and Test!
1. Press **Play**
2. The pentagon should appear!
3. Try the test buttons in the Inspector:
   - Right-click on `PentagonStatDisplayTester` → choose any "Test: ..." option
   - Or adjust the sliders in real-time to see changes

---

## Testing Scenarios

### Test 1: Combat Mission
```
Right-click PentagonStatDisplayTester → Test: Combat Mission
```
Shows a high Might/Fortitude pentagon (warrior mission)

### Test 2: Stealth Mission
```
Right-click PentagonStatDisplayTester → Test: Stealth Mission
```
Shows high Agility/Wit pentagon (rogue mission)

### Test 3: Hero vs Mission Overlay
```
Right-click PentagonStatDisplayTester → Test: Hero vs Mission Overlay
```
Shows TWO pentagons:
- **Blue (base)**: Mission requirements
- **Orange (overlay)**: Hero stats
- You can see where hero exceeds or falls short!

### Test 4: Auto-Cycling
Enable "Auto Cycle Scenarios" in the tester to see different hero archetypes automatically rotate every 3 seconds:
- Balanced Hero
- Warrior
- Rogue
- Diplomat
- Tank
- All-Rounder

---

## Customization Options

### Colors
- **Fill Color**: The interior fill of the pentagon
- **Outline Color**: The border lines
- **Overlay Fill/Outline**: Colors for the second pentagon (hero stats)

Recommended color schemes:
- **Mission Requirements**: Blue (cool, analytical)
- **Hero Stats**: Orange/Green (warm, active)
- **Success Match**: Green overlay
- **Failure Match**: Red overlay

### Size
- **Radius**: 50-300 (how big the pentagon is)
- **Line Thickness**: 1-10 (outline boldness)

---

## Advanced: Adding Stat Labels

Want to show stat names around the pentagon?

### 1. Create 5 TextMeshPro Labels
1. Under `PentagonDisplay`, create 5 TextMeshPro objects:
   - Right-click → UI → TextMeshPro - Text
   - Name them: `MightLabel`, `CharmLabel`, `WitLabel`, `AgilityLabel`, `FortitudeLabel`

### 2. Position Labels Around Pentagon
Position the labels around the pentagon corners:
- **MightLabel**: Top (X:0, Y:170)
- **WitLabel**: Top-Right (X:160, Y:50)
- **AgilityLabel**: Bottom-Right (X:100, Y:-140)
- **FortitudeLabel**: Bottom-Left (X:-100, Y:-140)
- **CharmLabel**: Top-Left (X:-160, Y:50)

### 3. Link Labels to Tester
Drag each label into the corresponding slot in `PentagonStatDisplayTester`:
- Might Label → `mightLabel`
- Charm Label → `charmLabel`
- etc.

Now the labels will update automatically when you test different scenarios!

---

## Understanding the Visualization

### Pentagon Layout (Clockwise from Top):
```
         [Might]
            △
           ╱ ╲
    [Charm]   [Wit]
         ╱     ╲
        ╱       ╲
  [Fortitude]—[Agility]
```

### What Each Stat Means:
- **Might** (Top): Physical combat strength
- **Wit** (Top-Right): Intelligence, tactics
- **Agility** (Bottom-Right): Speed, reflexes
- **Fortitude** (Bottom-Left): Endurance, HP
- **Charm** (Top-Left): Social skills, persuasion

### Reading the Pentagon:
- **Larger area** = Higher overall stats
- **Lopsided shape** = Specialized character (e.g., warrior is wide at top for Might)
- **Balanced pentagon** = Generalist character
- **Overlay comparison** = Where orange > blue = hero exceeds requirements

---

## Next Steps: Integration

Once you're happy with the pentagon visualization:

1. **Integrate into Mission Board**: Show mission requirements
2. **Integrate into Hero Selection**: Show hero stats overlaid on mission requirements
3. **Add color coding**: Green for "meets requirement", Red for "below requirement"
4. **Add animations**: Pentagon grows/shrinks when stats change

---

## Troubleshooting

**Pentagon doesn't appear?**
- Make sure GameObject has a `RectTransform` (UI element)
- Check Canvas is in "Screen Space - Overlay" mode
- Pentagon might be behind another UI element (adjust Z position or layer order)

**Colors don't work?**
- Check alpha values (A) - if A=0, it's invisible!
- Material should be "UI/Default" or similar

**Tester doesn't update?**
- Make sure Pentagon Display reference is set in the tester
- Try pressing "Update Pentagon" button manually
- Check if Play mode is active

---

## Example Values for Testing

### Rookie Hero (Low Stats):
- Might: 15, Charm: 15, Wit: 20, Agility: 10, Fortitude: 25

### Elite Hero (High Stats):
- Might: 50, Charm: 45, Wit: 55, Agility: 50, Fortitude: 60

### 3-Star Combat Mission Requirements:
- Might: 55, Charm: 10, Wit: 15, Agility: 20, Fortitude: 40

### Perfect Match Overlay:
1. Set mission requirements (base pentagon)
2. Enable overlay
3. Set hero stats equal or higher than all requirements
4. Pentagon should be fully enclosed by overlay!

---

Happy testing! 🎮
