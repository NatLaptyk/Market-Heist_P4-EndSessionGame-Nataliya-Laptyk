# Market Heist
## Scripting II · Assignment 4 · End Session Game

**Name:** Nataliya Laptyk
**Student ID:** 2530192

## Game Concept

Market Heist is a 3D stealth-action prototype. The player controls a stray cat trying to obtain a fish from a market stall. 
Two patrol cats guard adjacent stalls, and a chef cat blocks access to the prize fish.

The player can win by:
1. **Persuading** the chef through dialogue ("My family is starving")
2. **Bribing** the chef with 5 coins collected from around the market
3. **Defeating** the chef in combat after threatening them

## Systems Implemented

### System 1 — AI Opponent
NavMesh-based AI with an enum state machine: Patrol → Chase → Attack. Patrol cats roam between waypoints, chase the player on sight, and attack when in melee range. 
Cat stats are stored in CatData ScriptableObjects (three instances: two patrol cats and the chef boss).

### System 2 — Dialogue System
Branching dialogue done entirely in ScriptableObjects. Each DialogueNode is its own .asset file, holding speaker text, body text, and an array of choices. 
Choices reference the next node, an outcome enum (PersuadeChef / BribeChef / StartCombat / EndConversation), and an optional coin requirement. 
The bribe option is dynamically hidden if the player doesn't have enough coins, integrating with the HUD's item count.

## Event Approach

C# events used throughout (not UnityEvents). Reasons:
- Type-safe at compile time
- Decoupled from the Inspector: no broken visual links when scenes change
- Faster than UnityEvents at runtime

UnityEvents would suit designer-facing hooks (e.g., wiring an audio clip to a door open event in the Inspector). 
This prototype has no such hooks, every subscriber is a script.

## Save / Load System

### Architecture

The project uses **two separate persistence systems** that follow the assignment's required separation of concerns:

| Storage | Used For | Why |
|---|---|---|
| **JsonUtility + Application.persistentDataPath** | Game state (position, health, items, world flags) | Handles complex types (Vector3, lists, structs), human-readable JSON files, easy to inspect and reset. |
| **PlayerPrefs** | Settings (master volume) | Simple key-value storage for user preferences. Persists separately from save files so settings survive game state resets. |

### Game state — JsonUtility

The game state is serialized to a single JSON file at `Application.persistentDataPath/savegame.json`. 
This path is OS-appropriate (AppData on Windows, ~/Library on macOS, sandboxed on mobile) and persists across game sessions.

**State persisted:**
- Player position (via `SerializableVector3`)
- Player health
- Coin count (item count)
- Chef cat persuaded flag
- Fish stolen flag (win condition)
- List of collected pickup IDs (so picked-up coins stay gone after reload)

All file I/O is wrapped in try/catch. 
Missing file = warning. 
Corrupt JSON = error logged, no crash.

### Settings — PlayerPrefs

`SaveManager.SaveVolumeSetting(float)` and `SaveManager.LoadVolumeSetting()` demonstrate correct PlayerPrefs usage for a master volume setting. 
The setting is clamped to 0–1 and persists in the OS-specific PlayerPrefs store (Windows registry, macOS plist, etc.).

There is no in-game volume slider in this prototype; the methods are created to demonstrate the architectural distinction between game state and settings storage. 
They are fully functional and testable via the editor (see below).

## How to Test the Save System

### Test in-game

| Key | Action |
|---|---|
| **K** | Save game state to disk |
| **L** | Load game state from disk |

**Suggested test sequence:**
1. Hit Play. Move the cat. Pick up some coins. Take damage from a patrol cat.
2. Press **K** → Console: `SaveManager: Game saved to <path>`
3. Move the cat somewhere else. Pick up more coins. Persuade the chef.
4. Press **L** → cat teleports back to saved position, health restores, coin count restores, chef state reverts.

### Test via Editor ContextMenu

The `SaveManager` GameObject in the scene exposes these test options. 
**Right-click the SaveManager component in the Inspector** during Play mode (or in Edit mode for some tests) to access them:

**Game state tests:**
- `Test: Save Game` — same as pressing K
- `Test: Load Game` — same as pressing L
- `Test: Delete Save File` — removes `savegame.json` from disk (used to test the "missing file" edge case)

**Settings tests (PlayerPrefs):**
- `Test: Save Volume 0.5` — writes a volume value of 0.5 to PlayerPrefs
- `Test: Load Volume` — reads and logs the current volume value
- `Test: Clear Volume Setting` — deletes the PlayerPrefs volume key

To verify PlayerPrefs persistence across sessions:
1. Hit Play → right-click SaveManager → `Test: Save Volume 0.5` → Console confirms save.
2. Stop Play.
3. Hit Play again → right-click SaveManager → `Test: Load Volume` → Console shows `0.5` (the value persisted across the session restart).

### Edge cases handled

The assignment requires the save system to handle missing or corrupt save files without crashing. 
Both are tested via:

**Missing file:**
1. Right-click SaveManager → `Test: Delete Save File`.
2. Press L (or right-click → `Test: Load Game`).
3. Console shows: `SaveManager: No save file found at <path>. Starting fresh.`
4. No crash. Game continues running.

**Corrupt file:**
1. Press K to create a valid save.
2. Stop Play. Open `savegame.json` in any text editor.
3. Break the JSON (delete a brace, scramble values).
4. Save the file. Hit Play. Press L.
5. Console shows: `SaveManager: Load failed — <exception message>. Save file may be corrupt.`
6. No crash. Game continues running.

## Known Issues

- Cat jump animation has a double-bounce visual from the imported asset's clip: physics is correct (single jump), animation is what plays the bounce, which is why the Jump animation was disabled.
- After loading a save where coins were collected, the original coin GameObjects are destroyed at runtime. If you save very early and load very late, you'll see the coins reappear if their IDs weren't in the saved list.
- Dialogue choices that should be hidden by coin requirement do not show a disabled state; they're simply hidden, which is intentional.
- EnemyAI.cs is disabled at start for Chef Cat, enabled by DialogueTrigger.MakeChefHostile().

## Project Structure
Assets/Scripts/
├── EnemyAI/               EnemyAI.cs
├── Core/                  Health.cs
├── Data/                  CatData, PickupData, DialogueNode (ScriptableObjects)
├── Dialogue/              DialogueManager, DialogueTrigger
├── Interactables/         Pickup, FishWinTrigger, InteractableHighlight
├── Managers/              GameManager
├── Player/                PlayerInputHandler, PlayerController, PlayerCombat, PlayerInteractor
├── SaveSystem/            SaveData, SaveManager
└── UI/                    HUDController, DialoguePanel



