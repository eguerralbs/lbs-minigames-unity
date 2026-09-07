# Logic-game sequence

This sequence delivers the landscape, touch-first logic activities in a fixed order, ending with **Shortest Route → Lobby**. Use this page as the contributor-level map; implementation references are included only where maintenance requires them.

## Current sequence

| Game | ID | Core player task | Input and current answer rule | Successful transition |
|---|---|---|---|---|
| Shape Analogy | `shape.analogy` | Complete a visual analogy. | Drag the object that completes the analogy onto the target; the current correct token is the outlined heart. | Clothes Selection (`clothes.selection`) |
| Clothes Selection | `clothes.selection` | Choose the item that belongs with the displayed outfit/context. | Tap an answer card; the current implementation marks **gloves** as correct. | Object Selection (`object.selection`) |
| Object Selection | `object.selection` | Find the object that does not belong. | Tap an answer card; the current round contains a tennis shoe among hats, and the tennis shoe is correct. | Make An Emoji Drag (`make.emoji.drag`) |
| Make An Emoji Drag | `make.emoji.drag` | Build a happy emoji. | Drag the three emoji strips in any order; each left-side slot accepts only its matching strip and correct strips remain placed. | Animal Drag (`animal.drag`) |
| Animal Drag | `animal.drag` | Help the animals get to their homes. | Drag the cat to the yellow house (not green) and the pig to the green house, in any order; audio instruction only, no visible text. Hover feedback #FFB740. | Triangles Count (`triangles.count`) |
| Wolfie Flasks | `wolfie.flasks` | Choose Wolfie's correct flask. | Tap the correct flask answer. | Shortest Route (`shortest.route`) |
| Shortest Route | `shortest.route` | Identify the character with the shortest route. | Tap the koala at the end of the blue route; instruction audio only, no visible text. | Completion, then Lobby |

## Shared behavior

- **Landscape and touch:** the games use the shared level chrome and touch-compatible UI; Shape Analogy and Make An Emoji Drag use the shared drag-and-drop kit, while the selection games use answer cards.
- **Instructions:** each scene starts its spoken instruction after any sequence-transition handoff. The Hong control toggles that instruction while the game is ready; voice playback ducks shared music.
- **Completion:** a correct answer plays feedback, presents the shared celebration/final result, records completion, and then moves to the next destination.
- **BGM:** all games in `LevelSequenceRoute.IsLogicSequenceGame`, including `shortest.route`, share the logic-sequence BGM. See `Assets/App/Navigation/LevelSequenceRoute.cs`.

## Maintenance checklist: add a sequenced game

- [ ] Add its definition to `Assets/App/Catalog/Data/MiniGameCatalog.asset`, create its scene under `Assets/App/Games/`, and enable that scene in `ProjectSettings/EditorBuildSettings.asset`.
- [ ] Add the game ID to `Assets/App/Navigation/LevelSequenceRoute.cs`, including logic-sequence BGM membership and the predecessor's success target.
- [ ] Configure the scene's `FinalCelebrationConfiguration` reference (shared default: `Assets/App/Shared/Results/DefaultFinalCelebrationConfiguration.asset`).
- [ ] Update this sequence and table.

## References

- Routing and BGM membership: `Assets/App/Navigation/LevelSequenceRoute.cs`
- Game rules: `Assets/App/Games/ShapeAnalogy/Core/ShapeAnalogyRule.cs`, `Assets/App/Games/ClothesSelection/Core/ClothesSelectionRule.cs`, `Assets/App/Games/ObjectSelection/Core/ObjectSelectionRule.cs`, `Assets/App/Games/MakeAnEmojiDrag/Core/MakeAnEmojiDragRule.cs`, `Assets/App/Games/AnimalDrag/Core/AnimalDragRule.cs`
- Scenes: `Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity`, `Assets/App/Games/ClothesSelection/ClothesSelection.unity`, `Assets/App/Games/ObjectSelection/ObjectSelection.unity`, `Assets/App/Games/MakeAnEmojiDrag/MakeAnEmojiDrag.unity`, `Assets/App/Games/AnimalDrag/AnimalDrag.unity`
