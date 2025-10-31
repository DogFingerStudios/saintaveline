# Test Matrix

## Feature 0: Item Interactions

### Test Case 0.1: Hover Text
- **Input:** Hover over item in inventory
- **Expected Result:** Hover test displays (e.g., "Press 'E' to interact")

### Test Case 0.2: Pick Up Item (Menu)
- **Input:** Press 'E' while hovering over item, select "Hold" from menu
- **Expected Result:** Item is equipped in the player's hand, if the player was already holding an item then it is dropped, pressing the primary mouse button will invoked the item's "Attack"

### Test Case 0.3: Pick Up Item (Quick Action)
- **Input:** Press 'Q' while hovering over item
- **Expected Result:** Item is equipped in the player's hand, if the player was already holding an item then it is dropped, pressing the primary mouse button will invoked the item's "Attack"

---

## Feature 1: Friendly NPC Interactions

### Test Case 1.1: Hover Text
- **Input:** Hover over friendly NPC
- **Expected Result:** Hover text displays (e.g., "Press 'E' to talk")

### Test Case 1.2: Talk to NPC
- **Input:** Press 'E' while hovering over friendly NPC
- **Expected Result:** Interaction menu appears with options: Stay, Follow, Go To

### Test Case 1.2.1: Stay Option
- **Input:** Select "Stay" from interaction menu
- **Expected Result:** NPC should switch from previous state (i.e. Follow, Go To, etc) to "Stay" state

### Test Case 1.2.2: Follow Option
- **Input:** Select "Follow" from interaction menu
- **Expected Result:** NPC should follow the player character

### Test Case 1.2.3: Go To Option
- **Input:** Select "Go To" from interaction menu
- **Expected Result:** NPC should move to the specified location
- **Note:** See "Map Labeling feature"

---

## Feature 2: Map Labeling

---

## Feature 3: Enemy NPC Interactions

---

## Feature 4: Missions and Tasks

### Test Case 4.1: Serial Missions
- **Data**: Mission A, Task 1, Task 2

- **Input:** Start Mission A
- **Expected Result:** 
    - Task 1 becomes available
    - Mission A started message shows
    - Task 1 start message shows
    - Task 1 TaskIcon appears
- **Input:** Complete Task 1 of Mission A
- **Expected Result:** 
    - Task 1 complete message shows
    - Task 1 TaskIcon disappears
    - Task 2 becomes available
    - Task 2 start message shows
    - Task 2 TaskIcon appears
- **Input:** Complete Task 2 of Mission A
- **Expected Result:** 
    - Task 2 complete message shows
    - Task 2 TaskIcon disappears
    - Mission A complete message shows

### Test Case 4.2: Parallel Missions
- **Data**: Mission B, Task 1, Task 2
- **Input:** Start Mission B
- **Expected Result:** 
    - Both Task 1 and Task 2 become available
- **Input:** Complete Task 1 of Mission B
- **Expected Result:** 
    - Task 1 complete message shows
    - Task 1 TaskIcon disappears
- **Input:** Complete Task 2 of Mission B
- **Expected Result:** 
    - Task 2 complete message shows
    - Task 2 TaskIcon disappears
    - Mission B complete message shows