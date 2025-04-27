
# 🏠 Home: Survival Horror (Prototype 001)

A third-person and first-person survival horror prototype set in **Qusqui-Paris**, a fictional WWII-era city that blends French and Andean cultures. Players must guide their family through a siege inside a towering apartment complex, navigating between stealth, survival, and sacrifice.

---

## 🎮 Game Summary

**Home** is a tense survival experience set in an apartment named named **Torre-Casa de Runa**, under siege by the militant **Sombra-Qhari Guerrillas**. Players take on the role of a father trying to escape the tower with his family or eliminate all enemies before they’re overwhelmed.

### Key Features:
- First-Person ↔ Third-Person toggle
- Permadeath survival mechanics
- Basic stealth, combat, and detection systems
- Randomized NPC spawns and AI pursuit
- Dynamic terrain and procedural building logic
- WWII-inspired aesthetic with Andean mythological overtones

---

## 🧪 Technologies Used

| Tool / Engine | Purpose |
|---------------|---------|
| **Unity 6000.0.43f1** | Game engine |
| **C#** | Core gameplay logic and systems |
| **NavMesh Components** | AI pathfinding and navigation |
| **TextMeshPro** | UI and HUD systems |
| **Unity UI Toolkit** | Modern menus and HUD overlays |
| **Git** | Version control |

---

## 🎯 **Prototype Goals**

- ✅ Establish basic player movement (FPS + 3rd person toggle).
   - 🔹 <s>First Person</s>
   - 🔹 Third Person
   - 🔹 <s>1st/3rd Person Toggle</s>
- ✅ <s>Build a small portion of the tower (Ground Floor + 9th Floor + Roof).</s>
- ✅ Implement basic AI
   - 🔹 Patrol
   - 🔹 Chase
   - 🔹 Attack
- ✅ Create initial stealth and combat mechanics.
- ✅ Set up family NPCs with basic interaction and survival dynamics:
  - 🔹 Friendly NPCs (e.g., SonNPC) use a custom C# state machine
  - 🔹 States for POC: **Idle** (rotates to face player) and **Follow** (tracks player via NavMeshAgent)
- ✅ Establish basic ending conditions (reach the roof or kill all guards).


---

### 🏗️ **1. Foundation & Scene Setup**

1. **Create a New Unity Project**
   - Set up a 3D scene with default lighting
   - Import basic player model, camera, and controller

2. **Create Test Tower Layout**
   - Build 3 floors for testing:
     - 🛡️ **Ground Floor** – Sandbags, entry points, 1–2 guards
     - 🏠 **9th Floor** – Player apartment, family interaction
     - 🚁 **Roof** – Flare signal point
   - Use ProBuilder or modular building assets

3. **Lighting and Atmosphere**
   - Use dim lighting for tension
   - Add ambient war sounds (gunfire, footsteps, wind, etc)

---

### 🎮 **2. Player & Camera Setup**

1. **FPS and 3rd Person Controller**
   - Use `CharacterController`
   - Implement WASD + mouse look
   - Add jump and crouch
   - Toggle between FPS and 3rd-person modes

2. **Smooth Camera Transitions**
   - Use `Lerp` or `SmoothDamp`
   - Adjust FOV per perspective

---

### 👹 **3. AI and Enemy Mechanics**

1. **Basic AI States**
   - 🟢 Patrol – NavMesh-based pathing
   - 🟡 Alert – React to noise or sight
   - 🔴 Chase – Pursue player
   - ☠️ Attack – Trigger animation or damage

2. **Stealth Mechanics**
   - Raycast for enemy vision
   - Line-of-sight logic
   - Detection meter UI element

---

### 🏃 **4. Survival and Combat**

1. **Weapons**
   - Add knife or Molotov
   - Hit detection and damage logic

2. **Noise Distraction**
   - Player can generate sound (e.g. pututu horn)
   - AI reacts and enters alert state

---

### 👨‍👩‍👧‍👦 **5. Family Dynamics**

1. **Family AI**
   - 👦 Pachacuti (son) follows player, uses pututu
   - 👧 Kusi-Rose (daughter) crawls through vents
   - 👴 Túpac (grandfather) helps in combat

2. **Permadeath System**
   - Family death is permanent and impactful

---

### 🏆 **6. Victory and Failure Conditions**

1. **Victory**
   - 🚁 Reach roof and signal helicopter
   - ☠️ Eliminate all enemy guards

2. **Failure**
   - 💀 Player or essential family death
   - 🛑 Caught by enemy AI

---

### 🎯 **7. UI and Feedback**

1. ❤️ Health Bar for player/family
2. 👁️ Detection Meter (stealth feedback)
3. 🎒 Ammo/Inventory UI

---

### 🔥 **8. Playtesting and Refinement**

1. Test:
   - Stealth system (vision, noise)
   - AI behavior (patrol, chase, attack)
   - Combat feedback

2. Adjust difficulty and AI based on feedback


