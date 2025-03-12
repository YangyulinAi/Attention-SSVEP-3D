# Allen Experiment 2: Multi-Target Visual Attention Allocation (Number Judgment Version)

This project investigates how participants allocate visual attention in various multi-target flicker (SSVEP) scenarios while performing odd/even judgments on digits. Different conditions involve varying target distances (near/far), target combinations (center only, left-right, three targets), and the presence or absence of central flicker. Below is an introduction to the file structure, key scripts, and workflow.

---

## 1. Project Overview

- **Experiment Name**: Multi-Target Visual Attention Allocation (Number Judgment)
- **Research Objectives**:
  1. Examine how participants allocate visual attention under different flicker conditions (center, left-right, three targets) and at different target distances (near/far).
  2. Combine EEG (SSVEP) and optionally eye-tracking data to explore how users distribute attention across multiple targets.

---

## 2. Requirements

1. **Unity 2020.x or higher** (supporting C# 7.3+ syntax)
2. **SteamVR** (if running in VR)
3. **Vive SRanipal SDK** (if using HTC Vive Pro Eye / Focus 3 for eye tracking)
4. **LSL4Unity** (optional, if sending LSL markers for EEG)
5. **.NET 4.x** and compatible VR/AR SDKs

---

## 3. File Structure
``` 
├── Assets
│   ├── Scripts
│   │   ├── Main.cs                # Main scene logic: init, Update checks, trial transitions
│   │   ├── MarkerController.cs    # Manages marker dictionary, sends LSL/UDP markers
│   │   ├── SpriteController.cs    # Controls sprite switching for experiment/break stages
│   │   ├── NumberController.cs    # Generates & displays random digits, handles odd/even checks 
│   │   ├── ArrowController.cs     # Toggles left/center/right flicker objects & sets near/far distance
│   │   ├── SSVEPController.cs     # Flicker logic (frequency/color switching), attached to each target
│   │   ├── UDPSender.cs           # Sends markers over UDP
│   │   ├── LSLSender.cs           # Sends markers via LSL (optional)
│   │   └── ViveGazeDataRecorder.cs# Records eye-gaze data & fallback mouse simulation
│   └── ...
├── Experiment_Data
│   └── ...                       # Generated log files, gaze CSV, etc.
├── Packages
└── ProjectSettings
```

---

## 4. Key Scripts

1. **Main.cs**  
   - **Core script** controlling the overall experiment flow:
     - `Start()`: Initializes various controllers (Marker, Sprite, Number, Arrow, SSVEP, etc.).
     - `Update()`: Monitors input (pressing space to start, single/double click for odd/even).
     - `ChangeStage()`: A coroutine that alternates between “Task Stage” and “Break Stage”; each trial displays random digits for odd/even judgment.
     - `ShowRandomNumber()` / `HideThenShowNumber()`: Handles the appearance and disappearance of random numbers.
     - `SendMarker(string)`: Wraps `MarkerController.SendMarker(key)` to send markers.

2. **ArrowController.cs**  
   - Controls flicker objects (left/center/right) and their positions (near/far):
     - `UpdateDirection(string currentArrow)`: Uses switch-case based on direction strings like `"Left Close"`, toggling flicker activation and calling `SetDistance()`.
     - `SetBlinking(...)`: Activates/deactivates the relevant GameObjects, thus enabling/disabling flicker.
     - `SetDistance(...)`: Switches X position between ±110 and ±220 to represent near/far.

3. **SSVEPController.cs**  
   - Attached to each flicker target.  
   - Uses `frequency` to determine flicker speed; toggles color in `FixedUpdate()` to generate SSVEP stimuli.

4. **NumberController.cs**  
   - Generates random digits and displays them in the specified location (center, left, or right).  
   - Performs user input checks for odd/even (via `CompareUserInput()`).

5. **MarkerController.cs**  
   - Maintains a dictionary mapping event strings to bytes:
     ```csharp
     {"Left Close", 40}, {"NumberShow", 111}, {"NumberHide", 112}, ...
     ```
   - Sends markers via UDP and optionally via LSL.  
   - Supports event-labeled strings such as `"Start", "End", "NumberShow", "NumberHide", "1", "0", "True", "False"`, etc.

6. **SpriteController.cs**  
   - Switches the sprite displayed in the UI.  
   - `ChangeBreakSprite()` loads the last sprite in the array, usually for a rest indicator (“X”).

7. **ViveGazeDataRecorder.cs**  
   - Records eye-gaze data in a background thread, writing to `GazeDatabyVive.csv`.  
   - If SRanipal isn’t active, it simulates gaze using mouse input.

8. **LSLSender.cs / UDPSender.cs**  
   - Transmits markers to external software (EEG or other systems) over LSL or UDP.

---

## 5. Marker Dictionary & Event Flow

**MarkerController** example dictionary (including the newly added `NumberShow` / `NumberHide`):
```csharp
private Dictionary<string, byte> markerValues = new Dictionary<string, byte>
{
    // Arrow directions
    {"Left Close", 40}, {"Right Close", 60}, {"Up Left Far", 71}, // etc.

    // Number Shown
    {"One", 11}, {"Two", 22}, ... {"Nine", 99},

    // User Response
    {"UserRes", 254},   // start of user input
    {"0", 0}, // double click (even)
    {"1", 1}, // single click (odd)
    {"UserNotRes", 255},// no user response

    // Epoch markers
    {"Start", 101}, {"End", 102},

    // Correctness
    {"True", 201}, {"False", 202},

    // Number show/hide
    {"NumberShow", 111}, {"NumberHide", 112},

    // Bad epoch
    {"Bad", 222}
};
```
A typical trial sequence of markers might be:

SendMarker("Start") → trial start
SendMarker("Left Close") → sets condition/direction
SendMarker("NumberShow") → about to display a digit
SendMarker("One" ~ "Nine") → which digit is actually shown
User input → UserRes + ("0" or "1") + ("True" or "False")
SendMarker("NumberHide") → digit disappears
SendMarker("End") → trial end
、## 6. How to Run

**Scene Setup**:  
- Ensure `SSVEP Left`, `SSVEP Middle`, `SSVEP Right` exist with `SSVEPController`.  
- Text objects `centerNumberText`, `leftNumberText`, `rightNumberText` for number display.  
- Assign references in `Main.cs` (e.g., `sprites[]` array, `maxRunTimes`, `stageInterval`, etc.).  

**Start the Experiment**:  
- Press **Play** in Unity.  
- Press **Space** to begin the block of trials.  

**Task & Break Stages**:  
- `ChangeStage()` alternates between showing flicker/digit tasks and switching to break (X sprite).  
- This continues until all selected directions (in `selectedSprites`) are done.  
- Finally, it loads an “End” scene and stops logging.  

**Eye Tracking / Gaze (optional)**:  
- If SRanipal is active, data is recorded in `Experiment_Data/<date>/GazeDatabyVive.csv`.  
- If not, a fallback uses mouse input to emulate gaze.  

**Data & Logs**:  
- Flicker frequency log in `Experiment_Data/log_yyyy-MM-dd_HH-mm-ss.txt`.  
- Eye tracking in `GazeDatabyVive.csv`.  
- Markers (via UDP or LSL) can be observed in external tools (e.g., LabRecorder).  

---

## 7. Common Issues

**LSL Markers Not Sending?**  
- In `MarkerController`, ensure `lslSender.SendMarker(...)` is not commented out.  
- Install LSL4Unity, confirm external software is listening.

**Flicker Distance Not Changing?**  
- `ArrowController.SetDistance(...)` expects initial `localPosition.x` to be ±110 or ±220. Change the code to match your preferred near/far values (e.g., ±0.08/±0.25).

**Single vs. Double Click Sensitivity**  
- Adjust `doubleClickThreshold` in `Main.cs`. Default is 0.3s.

**Sprite Array & selectedSprites[]**  
- Avoid index mismatches. If the final sprite is for a rest indicator (“X”), confirm you handle its index properly in `SpriteController` and `selectedSprites`.

---

## 8. Contribution & License

- **Author**: Yangyulin Ai  
- **Contributions**: Scripts created by the project team for a multi-target visual attention study.  
- **License**: Intended for academic/research use. For public or commercial applications, please contact the author and cite accordingly.

---

## 9. Future Development

- **Parameterize Distances**: Replace ±110/±220 with configurable float variables.  
- **Optimize Threads/Coroutines**: Ensure stable flicker rates and high-rate eye data capture in VR.  
- **Add Markers**: You can insert more marker events (e.g., partial display, multi-phase digit reveal).  
- **Enhanced UI/UX**: Provide a real-time debug panel to track marker transmissions, EEG synchronization states, etc.
