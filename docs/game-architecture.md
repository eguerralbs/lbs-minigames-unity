# Game Architecture

How to add a game and when to extract a GameKit. Executable kits/tests are authoritative; this doc is a signpost.

## Register a game
1. Create `Assets/App/Games/<GameName>/` with its scene, code, art, audio, and focused tests.
2. Implement `MonoBehaviour, IAppScene`; `Configure(AppServices)` receives injected `GameLauncher`, `GameSession`, and `IAppAudioService`.
3. Build the UI with `UiFactory` and `LevelChromeFactory`; keep `ScriptableObject`s immutable and mutable state in a plain C# class.
4. Create a `GameDefinition` and add it to `Assets/App/Catalog/Data/MiniGameCatalog.asset`. Set its category, display data, `SceneName`, supported/default difficulty as applicable, and `visibleInHub` deliberately.
5. Enable the scene in `ProjectSettings/EditorBuildSettings.asset`, preserving `Bootstrap` as the entry scene.
6. For a fixed-sequence game only, add route order, predecessor/successor wiring, and logic-sequence BGM membership in `Assets/App/Navigation/LevelSequenceRoute.cs`. Standalone games need none of these.
7. Wire completion through `GameLauncher.Complete(new MiniGameResult(...))`, including `DifficultyId` when needed.

### Catalog availability

A catalog entry with a non-empty `SceneName` is playable. An entry without a `SceneName` is a visible preview card and shows `Coming soon` in the Hub. Current snapshot: 25 fixed-sequence games, two standalone playable entries, and 15 visible preview entries. Catalog data remains authoritative as content changes.

## Extract a GameKit
Extract when **second** unrelated game needs the same mechanic family.

- `Assets/App/GameKits/<Mechanic>/Core/` — pure `IDragDropRule`, `DragDropLevelState`, `DragDropLevelDefinition`.
- `Assets/App/GameKits/<Mechanic>/Runtime/` — reusable `MonoBehaviour` components (`DragDropCard`, `ProximityHighlighter`, `CardAnimator`).
- Games keep per-game layout, rule derivation, instruction/voice, celebration. Do not make a giant `BaseGame`.

Current kit: `DragDrop` (used by `ShapeAnalogy`; `Classification` stays on `Games/Common` shim). No empty speculative kit folders.

## Reuse rules
| Need | Use |
|------|-----|
| App-wide music/voice/SFX, level chrome, navigation | `Assets/App/Shared/*` |
| Reusable mechanic family | `Assets/App/GameKits/<Mechanic>` |
| Concrete game | `Assets/App/Games/<GameName>` |

No singletons, no `GameManager`, no cross-game imports. Composition at `ApplicationBootstrap`; scenes configure via `IAppScene`.

## Verification
- Run focused and shared EditMode coverage for pure rules and catalog/navigation wiring.
- Verify a representative Bootstrap → Lobby → game → Lobby flow in the Editor when runtime verification is available.
- Batch compile: `Unity -batchmode -quit -projectPath . -executeMethod UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFiles` (or `-runTests -testPlatform EditMode`).
