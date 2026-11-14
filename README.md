# Outpost Balance 3D — Unity Hackathon Project

An interactive 3D space engineering trade-off game in **Unity 6** built for students **ages 7–12+** (and families/classrooms). Players manage a **Moon (Artemis Shackleton Ridge)** or **Mars (Jezero Crater)** 3D outpost for 7 days/Sols, balancing **Power**, **Life Support**, **Radiation Shielding**, and **Science** using real NASA open datasets.

## Quick Start for Teammates (Unity 6)
1. Clone this repository.
2. Open **Unity Hub** → **Add → Add project from disk** → select this folder (`outpost-balance-unity`).
3. Open `Assets/Scenes/MainScene.unity` and press **Play**!
   - *(If you ever want to regenerate the default 3D scene & UI from code: top menu **Outpost Balance → Build Complete 3D Scene & UI**).*

## Features & Mechanics
- **Interactive 3D Outpost Diorama:** Orbit/zoom the camera around the Moon or Mars base; click 3D modules (`O2 Tower`, `Greenhouse Dome`, `Shield Tower`, `Science Lab`) directly or use the HUD `+ / -` power allocator.
- **No Free Choices (Engineering Trade-Offs):** Every decision has a real cost. Skipping the Greenhouse wilts crops (`OSDR OSD-120`) and cuts tomorrow's recovery; overspending solar output drains emergency battery ahead of Mars dust storms.
- **Live + Archived NASA Open Data:**
  - **NASA DONKI (`api.nasa.gov/DONKI/FLR`):** Live solar flare fetch via `UnityWebRequest` with transparent fallback to `Assets/Resources/Data/donki_archive.json` (`LIVE` vs `ARCHIVED` banner).
  - **NASA OSDR:** `osdr_findings.json` (`OSD-120`, `OSD-436`, `OSD-253`).
  - **Light-Speed Comm Delay:** Moon `1.3 seconds` vs Mars `~12 minutes`.
- **Bilingual EN / UZ + Kid-Friendly Glossary:** Instant language toggle and synthesized audio feedback.

## Project Structure
- `Assets/Scripts/Core/` — `GameManager`, `MissionConfig`, `ResourceSystem`
- `Assets/Scripts/Data/` — `NasaDonkiClient`, `NasaDataModels`, `LocalizationMgr`
- `Assets/Scripts/Gameplay/` — `PowerAllocator`, `DilemmaEngine`, `CommDelayMgr`
- `Assets/Scripts/World3D/` — `OutpostVisuals`, `CameraOrbit`, `ModuleClickable`, `RoverPatrol`
- `Assets/Scripts/UI/` — `HudController`, `PowerPanelUI`, `DilemmaPanelUI`, `DataGlossaryUI`, `DebriefUI`, `SfxPlayer`
- `Assets/Editor/OutpostSceneBuilder.cs` — One-click scene & UI builder

## Recommended WebGL Build (For Judging)
In Unity: **File → Build Profiles → Web (WebGL) → Build** so judges can play in any browser without downloading an installer.
