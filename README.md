**Mobile AR Darts Game**

# Game Overview

An AR darts game that spawns a virtual dartboard in world space and lets two players take turns tapping the screen to throw darts. The controller listens for board-placement events, spawns dart prefabs from camera space, animates their flight to the hit point, scores hits, and manages turn/round flow and end-of-game UI.

# Key Technical Features

* **AR board placement events:** Subscribes to `PlaceGameBoard` events to get board position and orientation.
* **Progress-based throw animation:** `AnimateDartThrow()` lerps position and applies a spin while in `MainMotion` mode.
* **Simple object tracking / queue:** Thrown darts are enqueued in a `Queue<GameObject>` so they can be processed/cleared at turn end.
* **Scoring rules in code:** `decodeScore()` computes points from distance offsets relative to `boardPosition`.
* **Coroutines for UI & cleanup:** `FadeInOut()` animates status text; `HideDartAfterAnimation()` scales and deactivates darts.
* **UI & flow controls:** Uses TextMeshPro for scores/status, dart icon images to show remaining throws, and a Game Over panel with Reset/Continue buttons.

# Game Flow & State Management

* **Modes:** `Mode.Main` (idle/waiting), `Mode.MainMotion` (dart in flight), `Mode.Dart` (placeholder / not used).
* **Turn system:** Two players alternate; `P1Turn` boolean toggles to switch players. Each player throws up to 3 darts (`ThrowNumber.tmax`).
* **Round end:** After 3 throws, a short delay (`TURN_END_WAIT`) triggers playback of each dart’s "Drop" animation, hides them, then either switches to Player 2 or shows Game Over after both players have played.
* **Game over:** `ShowGameOver()` shows panel and winner text; `ResetGame()` and `ContinueGame()` restore state.

# Dart Throwing Mechanism

* **Input detection:** `Input.GetMouseButtonDown(0)` (touch/click) + `Physics.Raycast` from camera to detect board hit.
* **Spawn & aim:** Instantiates `dartPre` at a position slightly below the camera, sets `currentDart`, enqueues it, and records `hitPosition`.
* **Animation:** `dartAnimationProgress` drives a LERP from `dartStartPosition` to `hitPosition` and applies a fast forward spin (`Quaternion.AngleAxis`) while in `Mode.MainMotion`. When animation finishes the controller returns to `Mode.Main`.
* **Queue management:** All thrown darts are enqueued; at turn end they are dequeued, play the "Drop" animation and are hidden after a short coroutine.

# Scoring System

`decodeScore(Vector3 pos)` computes score by offset magnitude from `boardPosition`:

* offset < 0.1 ⇒ **50 (Bullseye)**
* offset < 0.2 ⇒ **25 (Second ring)**
* 0.25 < offset < 0.35 ⇒ **10 (Triple ring)**
* offset > 0.45 and < 0.5 ⇒ **15 (Double ring)**
* offset between ~0.2–0.5 (else) ⇒ **1 (Main area)**
* offset ≥ 0.5 ⇒ **0 (Miss)**

(Note: angle checks are present but scoring is currently based on magnitude.)

# Visual Feedback & UI

* Updates `player1ScoreText` / `player2ScoreText` with cumulative scores.
* `StatusTxt` shows the last throw’s `currScore` and uses `MoveStatusTxtTo()` + `FadeInOut()` coroutine to appear above the hit point.
* Three dart icon `Image`s mirror remaining throws; `HideNextDart()` and `ShowAllDarts()` toggle them.
* Game Over UI displays winner and offers Reset / Continue actions wired to button listeners.

# Implementation Notes / Quick Suggestions

* Scoring currently uses full 3D magnitude from `boardPosition`; for more accurate board coordinates switch to board-plane local 2D (project hit onto board plane and compute distance in that plane).
* `ThrowNumber` is used atypically as an enum; an `int` for throw count could simplify comparisons and icon logic.
* `Mode.Dart` is unused—could be used to pin/embedding darts into the board on animation end.

If you want, I can convert that into a one-page README or a short in-editor comment block to paste into the script.

