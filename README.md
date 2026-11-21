# 🎰 GOLDEN SPIN
### A Retro-Inspired Slot Machine Game | Unity 6.3 LTS · C#

<p align="center">
  <img src="screenshots/main-menu.png" alt="Golden Spin Main Menu" width="85%">
</p>

<p align="center">
  A fully playable 2D slot machine experience featuring animated reels,
  randomized outcomes, a configurable betting system, reward calculations,
  persistent player data, and an interactive user interface.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6.3%20LTS-black?logo=unity" alt="Unity">
  <img src="https://img.shields.io/badge/Language-C%23-purple?logo=csharp" alt="C#">
  <img src="https://img.shields.io/badge/Platform-WebGL%20%7C%20Desktop-blue" alt="Platform">
  <img src="https://img.shields.io/badge/Status-Completed-success" alt="Status">
  <img src="https://img.shields.io/badge/Version-1.0-orange" alt="Version">
</p>

---

##  Project Overview

**Golden Spin** is a 2D slot machine game developed using Unity 6.3 LTS and C#. The project recreates the experience of a traditional three-reel slot machine with a retro pixel-art inspired visual identity.

The player can select a betting amount, initiate a spin, watch the reels animate independently, and receive rewards based on the resulting symbol combinations.

The project goes beyond the basic spinning mechanic by incorporating persistent player balances, multiple winning conditions, animated feedback, audio interactions, menu navigation, confirmation dialogs, and a WebGL deployment.

The primary development focus was to deliver a complete and interactive game while maintaining reliable gameplay logic, smooth visual transitions, and an organized Unity project structure.

### Project Highlights

- Three independently animated reels.
- Randomized symbol selection.
- Multiple betting denominations.
- Combination-based reward calculation.
- Special jackpot condition for three Sevens.
- Persistent player balance using PlayerPrefs.
- Animated lever, reel transitions, and jackpot effects.
- Interactive main menu with New Game and Load Game.
- Paytable and quit confirmation popups.
- Audio feedback for gameplay and UI interactions.
- Browser-compatible WebGL build.

---

##  Gameplay Preview

<p align="center">
  <img src="screenshots/gameplay.png" alt="Golden Spin Gameplay" width="85%">
</p>

The game uses three reels, each displaying one of four available symbols:

| Symbol | Description |
|---|---|
| 7 | Seven |
| 🍒 | Cherries |
| 🔔 | Bell |
| BAR | BAR |

### Gameplay Flow

1. The player launches the game and reaches the main menu.
2. The player selects **New Game** or **Load Game**.
3. The gameplay scene loads with the player's current balance.
4. The player selects a betting amount.
5. The player initiates a spin.
6. All three reels begin their spinning animations.
7. Reels stop sequentially with staggered timing.
8. The final symbol combination is evaluated.
9. The corresponding reward is calculated and added to the balance.
10. The player can continue playing until the balance is insufficient for the selected bet.

---

#  Core Gameplay Implementation

## 1. Reel System and Randomized Outcomes

The slot machine contains three independently controlled reels.

Each reel contains four possible symbols. When a spin begins, a random symbol index is selected for each reel using Unity's random number generation.

The implementation uses:

```csharp
Random.Range(0, reels[i].childCount);
```

This allows the selection logic to work with the number of symbol objects assigned to each reel rather than relying on hardcoded symbol indices.

### Reel Animation

The spinning experience is implemented using Unity Coroutines.

The animation system provides:

- Independent reel animation.
- Sequential reel stopping.
- Staggered stopping intervals.
- Controlled final symbol positioning.
- Audio feedback when reels start and stop.

Instead of stopping all reels simultaneously, the game introduces a delay between individual reel stops. This creates a more recognizable slot machine experience and builds anticipation before the result is displayed.

The result evaluation takes place only after all reels have completed their animations.

## 2. Betting System

Players can choose between three betting denominations:

| Bet Option | Cost |
|---|---:|
| Low Bet | 10 credits |
| Medium Bet | 50 credits |
| High Bet | 100 credits |

The initial player balance is **1,000 credits**.

### Betting Workflow

The betting system follows this sequence:

1. Validate the selected bet.
2. Check whether the player has sufficient credits.
3. Deduct the bet from the player's balance.
4. Start the reel animation.
5. Evaluate the resulting combination.
6. Calculate and distribute any reward.
7. Update the displayed balance.
8. Save the updated balance.

Betting controls are disabled during an active spin to prevent multiple simultaneous spin requests and conflicting betting actions.

## 3. Winning Combinations and Payout Calculation

The game evaluates the final symbols displayed across the three reels.

The reward system supports three levels of winning combinations.

| Winning Combination | Reward Multiplier | Example: 10-Credit Bet |
|---|---:|---:|
| Three Sevens | 10× | 100 credits |
| Any Other Three Matching Symbols | 5× | 50 credits |
| Any Two Matching Symbols | 2× | 20 credits |
| No Matching Combination | 0× | 0 credits |

### Reward Evaluation Logic

The result evaluation distinguishes between:

- Three matching Sevens.
- Three matching non-Seven symbols.
- Exactly two matching symbols.
- No winning combination.

The special Seven combination is evaluated separately so that it receives the highest multiplier.

Rewards are calculated using the selected bet amount and the corresponding multiplier.

**Important:** A pair combination is evaluated after the three-symbol matching conditions, ensuring that a three-symbol match receives the correct higher payout.

---

#  Animation, Audio and User Experience

A major part of Golden Spin's development was making the game feel responsive rather than simply displaying randomized results.

## 1. Visual Feedback

The project includes several visual effects:

| Feature | Purpose |
|---|---|
| Reel spinning animation | Communicates an active spin |
| Staggered reel stopping | Builds anticipation |
| Animated machine lever | Provides visual interaction feedback |
| Jackpot animation | Highlights the highest-value combination |
| Reel highlighting | Draws attention to winning symbols |
| Animated payout popup | Makes rewards more noticeable |
| Coin rain effect | Adds visual emphasis to jackpot events |
| Button hover animations | Improves interface responsiveness |
| Button press animations | Provides interaction feedback |
| Fade transitions | Smooths scene navigation |

## 2. Audio Feedback

Audio is integrated into both gameplay and interface interactions.

Implemented audio interactions include:

- Reel starting sound.
- Individual reel stopping sounds.
- Pair-winning sound.
- Three-symbol winning sound.
- Button hover and press sounds.
- Jackpot-related sound effects.
- Coin rain audio.
- Menu interaction audio.

Different audio feedback is used for different gameplay events to make interactions more understandable and satisfying.

## 3. User Interface

The interface follows a retro arcade-inspired visual direction.

Key UI components include:

- Main menu.
- Player balance display.
- Betting controls.
- Spin interaction.
- Paytable popup.
- Payout and result messages.
- Quit confirmation popup.
- Scene transition overlays.

The UI is designed around clear interaction states, with betting controls becoming unavailable while a spin is in progress.

---

#  Save System and Game State

Golden Spin includes a persistent player balance system implemented using Unity's `PlayerPrefs`.

### How It Works

The balance is stored using the key:

```csharp
private const string BALANCE_KEY = "SlotMachine_Balance";
```

The system supports:

- Loading the previously saved balance.
- Saving balance changes after betting.
- Saving balance changes after payouts.
- Saving when the application is paused or closed.
- Resetting the balance when starting a new game.

### New Game vs Load Game

| Action | Behaviour |
|---|---|
| New Game | Resets the player balance to 1,000 credits |
| Load Game | Restores the previously saved balance |
| Exit Game | Saves the current balance |
| Restart Game | Starts with a fresh balance |

The main menu validates the existence and value of saved data before enabling the Load Game button.

A saved balance of zero is considered valid, while an invalid negative balance is rejected.

---

#  Menu Navigation and Scene Management

The game contains two primary scenes:

| Scene | Responsibility |
|---|---|
| `MainMenu` | Entry point, new game, load game, and exit |
| `Slot Machine` | Main gameplay experience |

Scene transitions use Coroutine-based fade animations.

### Main Menu Features

- New Game.
- Load Game.
- Exit Game.
- Load Game availability validation.
- Fade-in transition when the menu opens.
- Fade-out transition before scene changes.
- Configurable delay during transitions.

### Quit Confirmation

A confirmation popup is included to prevent accidental exits.

The player can confirm or cancel the quit action.

The implementation also handles the Unity Editor separately from standalone builds.

**Platform note:** WebGL applications run inside a browser, and browser security restrictions prevent a web application from directly closing the browser tab.

---

#  Technical Architecture and Code Organization

The project uses C# scripts and Unity components to manage gameplay, UI interactions, animations, and scene transitions.

### Key Implementation Areas

| System | Implementation Approach |
|---|---|
| Reel management | Unity GameObjects and Transform hierarchy |
| Randomized outcomes | Unity Random API |
| Reel animations | Coroutines |
| Bet management | Player balance and selected bet state |
| Reward calculation | Symbol combination evaluation |
| Persistent balance | PlayerPrefs |
| Scene transitions | SceneManager and Coroutines |
| UI interaction | Unity UI Buttons and event listeners |
| Text rendering | TextMeshPro |
| Audio | Unity AudioSource |
| Jackpot effects | Animation and Coroutine-based effects |
| Coin rain | Prefab-based 2D coin animation |

### Project Directory Structure

```text
Golden-Spin-Slot-Machine/
│
├── Assets/
│   │
│   ├── Art/
│   │   ├── Background/
│   │   ├── Machine/
│   │   ├── Symbols/
│   │   └── Buttons/
│   │
│   ├── Animations/
│   ├── Audio/
│   ├── Prefabs/
│   ├── References/
│   ├── Scenes/
│   ├── Scripts/
│   └── UI/
│
├── Build/
│   └── WebGL/
│       ├── Build/
│       ├── TemplateData/
│       └── index.html
│
├── Packages/
├── ProjectSettings/
├── Screenshots/
│
├── .gitignore
└── README.md
```

The project separates visual assets, gameplay scripts, scenes, audio, animations, and prefabs to make the development environment easier to navigate and maintain.

---

# Technology Stack

| Technology | Usage |
|---|---|
| Unity 6.3 LTS | Game engine |
| C# | Gameplay and interaction logic |
| Unity UI | Buttons, menus, and popups |
| TextMeshPro | UI text rendering |
| Unity Coroutines | Reel animations and transitions |
| PlayerPrefs | Persistent player balance |
| Unity AudioSource | Gameplay and interface audio |
| Unity WebGL | Browser-compatible deployment |
| Git | Version control |
| GitHub | Source code hosting |

### Development Environment

- **Unity Version:** 6000.3.22f1
- **Primary Language:** C#
- **Game Type:** 2D
- **Rendering Template:** Universal 2D
- **Target Platforms:** Desktop and WebGL

---

# Running the Project

## Requirements

- Unity Hub.
- Unity 6.3 LTS (`6000.3.22f1`).
- Git.
- A compatible desktop browser for WebGL.

## Option 1: Run the Unity Project

### Step 1 — Clone the repository

```bash
git clone https://github.com/hari-shankar-k/Golden-Spin-Slot-Machine.git
```

### Step 2 — Open Unity Hub

Select **Add Project from Disk**.

Choose the cloned `Golden-Spin-Slot-Machine` directory.

### Step 3 — Select the Unity version

Use Unity `6000.3.22f1`.

### Step 4 — Open the project

Allow Unity to import the project assets and required packages.

### Step 5 — Open the Main Menu scene

Navigate to:

```text
Assets/Scenes/MainMenu
```

### Step 6 — Run

Press the Play button in the Unity Editor.

## Option 2: Run the WebGL Build

A prebuilt WebGL version is included in:

```text
Build/WebGL/
```

For local testing, open a terminal inside the WebGL directory.

Start a local HTTP server:

```bash
python -m http.server 8000
```

Open the following address in your browser:

```text
http://localhost:8000
```

The game should load in the browser.

> Do not open `index.html` directly using the `file://` protocol. WebGL builds should be served through an HTTP server.

---

#  Screenshots

## Main Menu

<p align="center">
  <img src="screenshots/main-menu.png" alt="Golden Spin Main Menu" width="85%">
</p>

## Gameplay Interface

<p align="center">
  <img src="screenshots/gameplay.png" alt="Golden Spin Gameplay Interface" width="85%">
</p>

## Paytable

<p align="center">
  <img src="screenshots/paytable.png" alt="Golden Spin Paytable" width="85%">
</p>

## Quit Confirmation

<p align="center">
  <img src="screenshots/quit-popup.png" alt="Golden Spin Quit Confirmation" width="85%">
</p>

---

#  Evaluation Criteria Alignment

The following table maps the implementation to the project's evaluation criteria.

| Evaluation Criteria | Implementation in Golden Spin |
|---|---|
| **Core Functionality Implementation** | Three-reel system, randomized symbols, betting logic, reward evaluation, balance management, and persistent save data. |
| **Code Cleanliness & Structure** | C# scripts, Unity component-based implementation, organized asset directories, prefabs, scenes, and separation of gameplay and UI responsibilities. |
| **Reel Animation & Game Feel** | Coroutine-based reel animations, staggered stopping, lever animation, reel highlights, audio feedback, and jackpot effects. |
| **Git Commit History & Repository Quality** | Git-based version control, public GitHub repository, organized project files, `.gitignore`, README documentation, and included WebGL build. |
| **Bonus Features / Creative Additions** | Persistent balance, New Game/Load Game functionality, jackpot coin rain, animated payout popup, confirmation dialog, and audio interactions. |
| **UI/UX Clarity** | Retro-inspired interface, selectable bets, clear balance display, paytable, interaction feedback, and scene transitions. |

---

#  Future Improvements

Although the current version provides a complete playable experience, several improvements could extend the game further:

- Additional symbols and expanded payout combinations.
- Free-spin and bonus-round mechanics.
- Player statistics and session history.
- Leaderboards and high-score tracking.
- More advanced reel animation and particle effects.
- Mobile-responsive controls and layout.
- Additional visual themes and machine customization.

---

#  Author

**Hari Shankar K**

B.Tech — Computer Science and Engineering (AI Focus)  
SRM University–AP

**GitHub:** [hari-shankar-k](https://github.com/hari-shankar-k)

**Project Repository:** [Golden Spin — Slot Machine](https://github.com/hari-shankar-k/Golden-Spin-Slot-Machine)

---

<p align="center">
  <strong>GOLDEN SPIN</strong>
  <br>
  Designed, developed, and built with Unity and C#.
  <br><br>
   Thanks for checking out the project!
</p>
