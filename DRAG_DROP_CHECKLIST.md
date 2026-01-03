# Drag & Drop Debugging Checklist

If drag & drop isn't working properly, check these items in order:

## 1. Canvas Setup ✓
- [ ] Canvas has **GraphicRaycaster** component
- [ ] Canvas Render Mode is set correctly (usually "Screen Space - Overlay" or "Screen Space - Camera")
- [ ] If using "Screen Space - Camera", the camera is assigned

## 2. EventSystem ✓
- [ ] Scene has an **EventSystem** GameObject
- [ ] EventSystem is active and enabled
- [ ] Only ONE EventSystem exists in the scene

## 3. DraggableItem Setup ✓
For each draggable item prefab/object:
- [ ] Has **RectTransform** component
- [ ] Has **CanvasGroup** component (auto-added by script, but check)
- [ ] Has **Image** component with "Raycast Target" ENABLED
- [ ] DraggableItem script is attached and enabled
- [ ] ItemIcon Image reference is assigned in inspector

## 4. ItemSlot Setup ✓
For each item slot (inventory + equipment):
- [ ] Has **RectTransform** component
- [ ] Has ItemSlot script attached and enabled
- [ ] ItemContainer RectTransform is assigned (or will use self)
- [ ] For equipment slots: `isEquipmentSlot` should be TRUE
- [ ] For inventory slots: `isEquipmentSlot` should be FALSE

## 5. Hierarchy & Layering ✓
- [ ] All draggable items are children of the Canvas (or children of objects within Canvas)
- [ ] No UI element is blocking raycasts unintentionally
- [ ] Check sorting order - dragged item should be on top
- [ ] Tooltip has `blocksRaycasts = false` (to prevent interference)

## 6. Prefab Assignment ✓
- [ ] ConsultationScreen has `draggableItemPrefab` assigned
- [ ] Prefab has ALL required components (see #3)
- [ ] Prefab's Image has a sprite assigned (or placeholder)

## 7. Common Issues & Solutions

### Issue: Items don't drag at all
**Solution:**
- Check if Image has "Raycast Target" enabled
- Verify CanvasGroup exists and `blocksRaycasts = true` (when NOT dragging)
- Ensure EventSystem exists

### Issue: Items drag but don't drop into slots
**Solution:**
- Check ItemSlot has the script attached
- Verify CanvasGroup on draggable has `blocksRaycasts = false` DURING drag
- Check OnDrop is being called (add Debug.Log in ItemSlot.OnDrop)

### Issue: Items snap back even when dropped on valid slots
**Solution:**
- Check CanAcceptItem() logic in ItemSlot
- Verify slot type (equipment vs inventory)
- Check if slot is already occupied

### Issue: Dragging is janky/laggy
**Solution:**
- Reduce canvas scaleFactor if using "Scale with Screen Size"
- Check Canvas is using correct render mode
- Verify no heavy operations in OnDrag()

### Issue: Tooltip blinks/flickers
**Solution:**
- ✓ FIXED: ItemTooltip now has `blocksRaycasts = false`
- Tooltip should NOT block mouse input

## 8. Debug Testing Steps

### Test 1: Basic Drag
1. Click and hold on an item in inventory
2. Item should become semi-transparent (alpha 0.6)
3. Item should follow mouse cursor
4. Release - item should return to original position

**If this fails:** Issue is with DraggableItem setup (#3)

### Test 2: Drop on Equipment Slot
1. Drag an item from inventory
2. Hover over an empty equipment slot
3. Release mouse
4. Item should appear in the slot

**If this fails:** Issue is with ItemSlot setup (#4) or OnDrop detection

### Test 3: Drop on Occupied Slot
1. Drag an item to an occupied equipment slot
2. Items should swap positions

**If this fails:** Check SwapItems() logic in ItemSlot

### Test 4: Tooltip Display
1. Hover over an item WITHOUT dragging
2. Tooltip should appear
3. Tooltip should follow mouse
4. Tooltip should NOT flicker

**If this fails:** Check ItemTooltip setup and blocksRaycasts setting

## 9. Console Debug Checks

Add these debug lines temporarily to track issues:

```csharp
// In DraggableItem.OnBeginDrag()
Debug.Log($"Begin drag: {itemData.itemName}");

// In DraggableItem.OnDrag()
Debug.Log($"Dragging at position: {rectTransform.anchoredPosition}");

// In DraggableItem.OnEndDrag()
Debug.Log($"End drag. Pointer over: {eventData.pointerEnter?.name}");

// In ItemSlot.OnDrop()
Debug.Log($"Drop detected in slot: {gameObject.name}");

// In ItemSlot.CanAcceptItem()
Debug.Log($"Can accept item? {result}");
```

## 10. Inspector Verification

### For DraggableItemPrefab:
```
GameObject: DraggableItemPrefab
├── RectTransform ✓
├── Image ✓ (Raycast Target: ON)
├── CanvasGroup ✓ (auto-added)
└── DraggableItem ✓
    └── Item Icon: [Image reference]
```

### For ItemSlot (Equipment):
```
GameObject: EquipmentSlot_1
├── RectTransform ✓
├── Image (optional, for slot background)
└── ItemSlot ✓
    ├── isEquipmentSlot: TRUE
    └── itemContainer: [RectTransform reference or empty]
```

### For ItemSlot (Inventory):
```
GameObject: InventorySlot_1
├── RectTransform ✓
├── Image (optional)
└── ItemSlot ✓
    ├── isEquipmentSlot: FALSE
    └── itemContainer: [RectTransform reference or empty]
```

---

## Quick Fix: Reset Everything

If nothing works, try this reset:

1. Delete all DraggableItem instances in the scene
2. Check ConsultationScreen inspector:
   - MainViewPanel assigned
   - InventoryViewPanel assigned
   - draggableItemPrefab assigned
   - equipmentSlots array filled
   - inventoryGrid assigned
3. Enter Play Mode
4. Items should auto-generate in inventory grid
5. Try dragging again

---

## Still Not Working?

Check these advanced issues:
- Canvas sorting order conflicts
- Parent CanvasGroup blocking input
- Camera culling (if using Camera render mode)
- Physics2D Raycaster instead of GraphicRaycaster
- Multiple canvases with conflicting settings
