# LBS Mini Games

A catalog-driven Unity application containing **27 playable catalog entries** across logic, mathematics, science, and language-oriented content: 25 games in the fixed logic sequence plus standalone `Number Pull` (`math.number-pull`) and `Memorama` (`juega-aprende.memoria-animales`). The Hub also shows 15 non-playable preview placeholders, for 42 visible cards in the current catalog. `MiniGameCatalog` is the source of truth; do not hand-maintain these snapshot counts after future catalog additions. The application starts at `Bootstrap`, builds the shared services, loads the `Lobby`, and launches each game from its catalog definition.

## Prerequisites

| Need | Version / purpose |
| --- | --- |
| Unity Hub | Install and open the project. |
| Unity Editor | **6000.5.9f1** (the project-pinned version). |
| Git and Git LFS | Clone the repository and retrieve LFS-tracked media. |
| Android Build Support, OpenJDK, Android SDK and NDK | Required only to run on an Android tablet. Install them with Unity 6000.5.9f1 through Unity Hub. |
| iOS Build Support and Xcode | Required only for iOS work. |

The project uses Unity's built-in UGUI package (`com.unity.ugui` 2.5.0), Multiplayer Center (1.0.1), the Unity Test Framework (1.7.0), and Unity modules. Unity resolves these when the project opens.

## Clone and open

```bash
git clone https://github.com/eguerralbs/lbs-minigames-unity.git
cd lbs-minigames-unity
git lfs install
git lfs pull
```

In Unity Hub, select **Add** and choose the cloned project directory. Open it with **Unity 6000.5.9f1** and allow package import and asset refresh to finish.

## Run in the Unity Editor

1. Open `Assets/App/Bootstrap/Bootstrap.unity`.
2. Enter Play mode.
3. The persistent bootstrap creates `AppServices`, configures the catalog and level sequence, and loads `Lobby`.
4. In the Hub, choose a category and launch any playable game card.
5. Complete the game and confirm that the result is recorded before returning to the Hub.

`Bootstrap` is the entry scene. `Lobby` and every launchable game scene are enabled in Build Settings. Start the application from `Bootstrap`; opening a downstream scene directly bypasses bootstrap service configuration.

### Hub and logic sequence

The Hub builds catalog-driven category sections in a vertically scrollable page, with a horizontal card row in each section. Cards are either playable or display `Coming soon`. The difficulty selector is presentation-only: it currently does not filter cards or change the launched difficulty. The header includes Wolfie and decorative background presentation.

The fixed logic route is:

```text
Funny Face Drag → Shape Analogy → Wolfie Flasks → Thinking 3D → Animal Drag
→ Kitchen Math Logic → Triangles Shape Logic → Circle Math → Cube Platform → Chemistry Selection
→ Candies Logic → Clothes Selection → Make An Emoji Drag → Ladybug Place → Stickers Placement
→ Object Selection → Bubble Math → Triangles Count → Thinking Figures → Squares Succession
→ Fraction Succession → Shortest Route → Apple Math → Shape-Put → Age Compare
```

The sequence advances after each successful celebration. **Age Compare** returns to the Lobby after its final result interaction. `Number Pull` and `Memorama` are standalone playable catalog entries and are not part of this fixed route.

## Test on an Android tablet

1. On the tablet, enable Developer options and **USB debugging**. Connect it by USB and authorize the computer if prompted.
2. In Unity, open **File > Build Profiles**. Create or select an **Android** profile and switch to it if Unity asks.
3. Confirm the tablet appears in **Run Device** and select it.
4. Choose **Build And Run**. Unity builds, installs, and launches the app on the selected tablet.
5. Follow the editor flow above and verify both landscape directions.

The application is configured for auto-rotation with **Landscape Left** and **Landscape Right** enabled; both portrait orientations are disabled. UI canvases use a 1920×1080 reference resolution.

### Optional Android batch build

`AutoAndroidBuild.Build` is an optional editor helper that builds all enabled scenes to `build/lbs-minigames-logic.apk`. It checks the entry and representative scenes before building. This helper does not constitute device validation.

## Test and verification

Run the EditMode suite from Unity's Test Runner, or use batch mode:

```bash
Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Library/EditModeTestResults.xml
```

Pure game rules and state transitions have focused EditMode tests. Visual and device behavior still requires running the representative scenes in the Unity Editor or on the target device.

## Project layout and modularity

| Path | Responsibility |
| --- | --- |
| `Assets/App/Bootstrap` | Persistent application bootstrap and service composition. |
| `Assets/App/Lobby` | Catalog-driven Hub scene and controller. |
| `Assets/App/Catalog` | `GameCatalog`, categories, game definitions, validation, and catalog data assets. |
| `Assets/App/Navigation` | Session state, scene loading, level sequencing, and game completion flow. |
| `Assets/App/Shared` | Cross-game contracts, result model, audio, level chrome, and shared UI helpers. |
| `Assets/App/GameKits/DragDrop` | Reusable drag-and-drop mechanic components and state. |
| `Assets/App/Games/<GameName>` | An individual game's scene, rules, art, audio, and tests. |
| `Assets/Tests/Editor` | Shared EditMode tests. |
| `Packages` / `ProjectSettings` | Unity package lockfiles and project-wide settings. |

Keep a mini-game's scene, rules, and presentation inside its own `Assets/App/Games/<GameName>` folder. Put reusable mechanics in `GameKits`, app-wide contracts and helpers in `Shared`, and composition in `ApplicationBootstrap`; do not make the bootstrap or lobby depend on a specific game's implementation.

## Runtime architecture

- `ApplicationBootstrap` owns persistent services and injects them through `AppServices`.
- Each game scene implements `IAppScene` so it can be configured after loading.
- Game-specific mutable state lives in plain C# state objects; catalog and definition assets are authoring-only `ScriptableObject`s.
- Games report completion through `GameLauncher.Complete(new MiniGameResult(...))`.
- `GameDefinition` stores the game ID, display data, category, scene name, difficulty support, and Hub visibility.

## Git and Unity collaboration

- Keep **Asset Serialization** set to **Force Text** and **Version Control** set to **Visible Meta Files**. These are already configured in the project.
- Commit `.meta` files together with every added, moved, or deleted Unity asset. Do not regenerate or omit them.
- Do not commit Unity-generated working directories such as `Library/` or `Temp/`; they are ignored along with build output and editor caches.
- Unity YAML assets (`.unity`, `.prefab`, `.asset`, `.mat`, and `.meta`) are text files with UnityYAML merge configuration. Keep them in normal Git history.
- This repository's LFS policy is binary source media only: `.psd`, `.psb`, `.fbx`, `.blend`, audio (`.wav`, `.aiff`, `.mp3`, `.ogg`), and video (`.mp4`, `.mov`, `.webm`). Do not use LFS for Unity YAML files.

## Add a mini-game

1. Create the game scene, code, art, audio, and tests under `Assets/App/Games/<GameName>`.
2. Implement `IAppScene` and receive injected `AppServices` in `Configure(AppServices)`; keep rules and mutable state in the game's own plain C# types.
3. Build the UI with the existing shared factories and follow the approved `LevelChromeLayout` coordinates.
4. Add the scene to Build Settings, preserving `Bootstrap` as the entry scene.
5. Create a `GameDefinition` asset with a unique ID, display information, category, exact scene name, and supported difficulties. Add it to `MiniGameCatalog` and set `visibleInHub` deliberately.
6. If the game belongs to the progression, add its ID and success target to `LevelSequenceRoute`.
7. Run the focused EditMode tests, batch compilation, and the complete Bootstrap → Lobby → game → Lobby flow in the Editor and on the target device.

## Current scope

The project is an educational mini-game Hub with catalog-driven launching, shared audio and navigation, level sequencing, and session-scoped results. It does not yet provide persistent progression, account or backend integration, or a production content pipeline.
