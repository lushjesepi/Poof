# Refinements & Changes (running log)

This file is a maintained, high-level log of gameplay/UI/system refinements made during development in Cursor.
It focuses on **scope shifts**, **design decisions**, and **implementation choices** (not exhaustive code diffs).

---

## 2026-03-27

### Player: Cat transformation duration + auto-revert
- **Request**: Cat form should last 45 seconds, configurable in Inspector, then revert to human.
- **Decision**: Implement timer inside `PlayerController` so form switching logic stays centralized.
- **Notes**:
  - Added `catFormDurationSeconds` (Inspector).
  - Auto-revert does **not** consume Fallen Star resources.
  - Exposed timer state for UI (`IsCatForm`, `CatFormTimeRemaining`).

### Player: Sprite facing direction (A/D)
- **Request**: Character sprite should flip to face movement direction.
- **Decision**: Implement in `PlayerMovement` using `Horizontal` axis, because movement input already lives there.
- **Notes**:
  - Added Inspector toggles for flipping + default facing direction.

### Player: Walk squash/stretch (human vs cat)
- **Request**: Human squish up/down while walking; cat squish in/out horizontally.
- **Decision**: Drive a subtle sinusoidal scale animation on the `SpriteRenderer` transform while moving.
- **Notes**:
  - Separate Inspector controls for human/cat squash amount/speed.
  - Smooth return to neutral when idle.

### UI: Cat form countdown (top-right)
- **Request**: Timer UI showing remaining cat time.
- **Decision**: Use `OnGUI` overlay for fastest integration (no Canvas/TMP setup required).
- **Notes**:
  - New script `CatFormTimerUI` renders top-right panel while in cat form.

### Enemies: Falling enemies + wave spawner
- **Request**: Falling enemies spawn in 3 waves (5, 10, 15), 20 seconds apart (configurable), kill player on collision, self-destruct on collision.
- **Decision**:
  - Implement as a prefab-driven `FallingEnemy` + `FallingEnemyWaveSpawner` for Inspector setup and reuse.
  - Keep counts configurable with sensible defaults.
- **Notes**:
  - Player death handled via `PlayerController.Die()` with `IsDead` state.

### Gameplay: Player death flow + death UI
- **Request**: On death, show “You Died” screen with Restart / Main Menu / Quit.
- **Decision**: Use `OnGUI` overlay (`PlayerDeathUI`) consistent with other lightweight UIs in the project.
- **Notes**:
  - On death: disables `PlayerMovement`, zeroes velocity, invokes optional UnityEvent.

### Enemies vs Tilemap: pass-through platforms, die on Death layer
- **Request**: Enemies should pass through Ground platforms, but die when colliding with a Death layer on tilemap.
- **Initial approach**: Physics2D layer collision ignore between enemy layer and Ground layer.
- **Scope correction / bug fix**:
  - **Issue**: Player started falling through ground due to global layer collision settings (shared layers).
  - **Decision**: Switch to per-enemy `Physics2D.IgnoreCollision(...)` against Ground-layer colliders instead of global layer rules.
- **Notes**:
  - Death layer collision remains destructive for enemies.

### Enemies: Waves cycle until ship is fully repaired
- **Request**: After wave 3, restart at wave 1 and continue until ship repair win condition is met.
- **Decision**: Loop wave sequence in spawner and stop only when `GameManager.IsWin` is true.
- **Notes**:
  - Added `secondsBetweenCycles` (optional gap after wave 3).
  - Added guard in `PlayerController.Die()` to ignore death after win triggers.

### Audio: In-game audio manager with Inspector clips
- **Request**: Audio manager with serialized fields for:
  - Background music (plays throughout)
  - Resource pickup SFX
  - Player death SFX
  - Win SFX
- **Decision**:
  - Add `AudioManager` singleton that persists across scenes (`DontDestroyOnLoad`).
  - Use one looping `AudioSource` for music + one one-shot `AudioSource` for SFX.
- **Hook points**:
  - Pickup: `ResourceNode` triggers `AudioManager.PlayResourcePickup()`.
  - Death: `PlayerController.Die()` triggers `AudioManager.PlayPlayerDeath()`.
  - Win: `GameManager.TriggerWin()` triggers `AudioManager.PlayWin()` (once).

### Main Menu: Clickable multi-part tutorial
- **Request**: Tutorial button opens a clickable 3-part tutorial with Next/Done.
- **Decision**: Implement tutorial as OnGUI overlay inside `MainMenuController` to avoid Canvas dependencies.
- **Notes**:
  - Steps are stored as serialized strings (editable in Inspector).
  - “Tutorial” opens part 1; “Next” advances; “Done” closes.

