# Logic-game sequence

This sequence delivers twenty-five short logic activities in a fixed order for the landscape, touch-first UI. The opening deliberately prioritizes strong visual variety, then alternates subjects and mechanics before finishing with the more abstract mathematics and sequence activities.

## Current sequence

| # | Game | ID | Core player task | Successful transition |
|---:|---|---|---|---|
| 1 | Funny Face Drag | `funnyface.drag` | Assemble a funny face by dragging its pieces. | Shape Analogy (`shape.analogy`) |
| 2 | Shape Analogy | `shape.analogy` | Complete a visual analogy. | Wolfie Flasks (`wolfie.flasks`) |
| 3 | Wolfie Flasks | `wolfie.flasks` | Select the correct option in the Wolfie science scene. | Thinking 3D (`thinking.3d`) |
| 4 | Thinking 3D | `thinking.3d` | Solve a spatial reasoning task with 3D artwork. | Animal Drag (`animal.drag`) |
| 5 | Animal Drag | `animal.drag` | Help the animals get to their homes. | Kitchen Math Logic (`kitchen.math.logic`) |
| 6 | Kitchen Math Logic | `kitchen.math.logic` | Solve a math activity in a kitchen context. | Triangles Shape Logic (`triangles.shape.logic`) |
| 7 | Triangles Shape Logic | `triangles.shape.logic` | Match and place triangle shapes. | Circle Math (`circle.math`) |
| 8 | Circle Math | `circle.math` | Solve a circle-based math activity. | Cube Platform (`cube.platform`) |
| 9 | Cube Platform | `cube.platform` | Solve a spatial platform and box activity. | Chemistry Selection (`chemistry.selection`) |
| 10 | Chemistry Selection | `chemistry.selection` | Select the correct option in a laboratory context. | Candies Logic (`candies.logic`) |
| 11 | Candies Logic | `candies.logic` | Classify the illustrated sweets and candies. | Clothes Selection (`clothes.selection`) |
| 12 | Clothes Selection | `clothes.selection` | Choose the item that belongs with the displayed outfit or context. | Make An Emoji Drag (`make.emoji.drag`) |
| 13 | Make An Emoji Drag | `make.emoji.drag` | Build a happy emoji by dragging its pieces. | Ladybug Place (`ladybug.place`) |
| 14 | Ladybug Place | `ladybug.place` | Place the ladybug in the correct position. | Stickers Placement (`stickers.placement`) |
| 15 | Stickers Placement | `stickers.placement` | Place the stickers on the board. | Object Selection (`object.selection`) |
| 16 | Object Selection | `object.selection` | Find the object that does not belong. | Bubble Math (`bubble.math`) |
| 17 | Bubble Math | `bubble.math` | Solve a playful math activity with bubbles. | Triangles Count (`triangles.count`) |
| 18 | Triangles Count | `triangles.count` | Count the triangles in the illustration. | Thinking Figures (`thinking.figures`) |
| 19 | Thinking Figures | `thinking.figures` | Solve a visual figure-reasoning task. | Squares Succession (`squares.succession`) |
| 20 | Squares Succession | `squares.succession` | Complete a sequence of squares. | Fraction Succession (`fraction.succession`) |
| 21 | Fraction Succession | `fraction.succession` | Complete a fraction sequence. | Shortest Route (`shortest.route`) |
| 22 | Shortest Route | `shortest.route` | Identify the character with the shortest route. | Apple Math (`apple.math`) |
| 23 | Apple Math | `apple.math` | Discover the value of one apple cube. | Shape-Put (`shape.put`) |
| 24 | Shape-Put | `shape.put` | Identify the third shape in the displayed sequence. | Age Compare (`age.compare`) |
| 25 | Age Compare | `age.compare` | Listen and choose the asked age. | Lobby after the final result interaction |

The Hub may display a catalog `hubSubjectLabel`, such as `Logic` or `Math`, for logic cards. The Game column above remains the authoritative game/scene name.

## Standalone playable games

These catalog entries are playable but are outside the fixed sequence:

| Game | ID | Notes |
|---|---|---|
| Number Pull | `math.number-pull` | Launches from its catalog card. |
| Memorama | `juega-aprende.memoria-animales` | Launches from its catalog card. |

## Shared behavior

- **Landscape and touch:** the games use the shared level chrome and touch-compatible UI. Drag activities use the shared drag-and-drop kit, while selection activities use answer cards or illustrated options.
- **Instructions:** each scene starts its spoken instruction after any sequence-transition handoff. The Hong control toggles that instruction while the game is ready; voice playback ducks shared music.
- **Completion:** a correct answer plays feedback, presents the shared celebration/final result, records completion, and then moves to the next destination.
- **BGM:** all twenty-five IDs in this table are explicit logic-sequence BGM members. See `Assets/App/Navigation/LevelSequenceRoute.cs`.

## Maintenance checklist: add or reorder a sequenced game

- [ ] Add its definition to `Assets/App/Catalog/Data/MiniGameCatalog.asset`, create its scene under `Assets/App/Games/`, and enable that scene in `ProjectSettings/EditorBuildSettings.asset`.
- [ ] Add the game ID to `Assets/App/Navigation/LevelSequenceRoute.cs`, including logic-sequence BGM membership and the predecessor's success target.
- [ ] Ensure the terminal game uses the final-result interaction to return to the Lobby; every non-terminal game must advance to its explicit successor.
- [ ] Configure the scene's `FinalCelebrationConfiguration` reference (shared default: `Assets/App/Shared/Results/DefaultFinalCelebrationConfiguration.asset`).
- [ ] Update this sequence table and its transition descriptions.

## Catalog, build, and verification wiring

- `Assets/App/Catalog/Data/MiniGameCatalog.asset` is the catalog source of truth. Add each playable or preview definition there and set its visibility deliberately.
- `ProjectSettings/EditorBuildSettings.asset` must enable every launchable game scene, alongside `Bootstrap` and `Lobby`.
- `Assets/App/Navigation/LevelSequenceRoute.cs` owns fixed-route order, successors, and logic-sequence BGM membership.
- Standalone games need catalog and Build Settings wiring, but do not need a route, successor, or logic-sequence BGM entry.
- Verify focused EditMode coverage and a representative Bootstrap → Lobby → game → Lobby flow when runtime verification is available.

## References

- Routing and BGM membership: `Assets/App/Navigation/LevelSequenceRoute.cs`
- Scene transition behavior: `Assets/App/Navigation/LevelSequenceController.cs`
- Game definitions: `Assets/App/Catalog/Data/MiniGameCatalog.asset`
- Game rules and scene implementations: the corresponding folder under `Assets/App/Games/`
