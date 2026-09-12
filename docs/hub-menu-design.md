# LBS+ Games Hub/Menu visual-design specification

**Status:** Team-ready visual direction for the first screen.  
**Governing identity:** [`visual-identity.md`](visual-identity.md).  
**Working name:** `LBS+ Games` is provisional text, not a final logo lockup.

The Hub is the first design priority: a direct, shared-screen gallery that lets players choose a game with one large touch. It is for always-landscape Android tablets and 98-inch Android touch TVs; it must remain clear for several simultaneous users and readable at a distance.

## Quick visual path

1. Read the provisional `LBS+ Games` label in the top bar.
2. Scan the gallery for a large illustrated game card.
3. Touch any part of a card to open that game directly; no profile, account, or setup step is placed before selection.

## Visual decisions

| Area | Direction |
| --- | --- |
| Screen shell | Calm `#F7F5FA` canvas **(proposed neutral)** with a purple `#9448F4` header band and white `#FFFFFF` **(proposed neutral)** rounded surfaces. |
| Identity | Use purple `#9448F4` for primary identity and selected emphasis; use orange `#FFB740` for supporting emphasis and functional feedback. Never create a new product logo or lockup. |
| Typography | Volte Regular only. Establish hierarchy through size, spacing, and color—not unavailable font weights. The provisional `LBS+ Games` header title is the sole approved exception and may use Unity synthetic bold. |
| Icons and art | Simple, consistent line icons. Use abstract subject-color shapes or line illustrations until game art exists; a card must still be understandable without art. |
| Contrast | Use dark ink `#241A35` **(proposed)** for text and icons on orange controls. Do not use small white text on orange. |
| Feedback color | Success `#167A4A` and error `#B3261E` are **proposed semantic colors**, not approved brand colors; reserve them for clear status feedback rather than decoration. |

## 16:9 layout

Use a responsive 16:9 stage with safe outer padding, a fixed-purpose header, and one dominant gallery. Preserve generous negative space rather than filling the screen with utility controls.

```text
┌──────────────────────────────────────────────────────────────────────────┐
│  LBS+ Games (provisional text)                         optional utility  │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   [ game card ]   [ game card ]   [ game card ]   [ game card ]         │
│                                                                          │
│   [ game card ]   [ game card ]   [ game card ]   [ game card ]         │
│                                                                          │
├──────────────────────────────────────────────────────────────────────────┤
│  Gallery position / paging indicator, only when additional cards exist  │
└──────────────────────────────────────────────────────────────────────────┘
```

- **Header:** one short textual product label, with optional non-critical utility affordance only when a real need is defined. It must not become an account, profile, or teacher-setup entry point.
- **Gallery:** center the cards when there are few. When there are many, retain a consistent card size and use touch-friendly paging or a clearly bounded horizontal/vertical gallery; do not shrink cards until they become hard to read or select.
- **Density:** use one or two rows according to available landscape space. The layout may expose different card counts on tablet and TV, but it must keep the same card anatomy, order, and direct-touch behavior.
- **Footer:** absent when it adds no information. When the gallery extends beyond one view, it contains only a large, visible position indicator and/or large paging controls.

### MascotArea layout

The Hub main content is built at runtime with this stable containment structure:

```text
MainArea
├─ GamesArea
│  └─ GameGrid
└─ MascotArea
├─ WolfieImage
├─ WolfieSpeechBubble
│  └─ Message
└─ WolfieSpeechTail
```

- **GamesArea:** owns the direct-touch gallery and contains every game card. It reserves the mascot region rather than allowing decoration to overlap cards.
- **MascotArea:** reserves 25% of the main-content width by default; it is adjustable only within 20–30% for landscape tablets and 98-inch touch TVs.
- **WolfieImage:** uses `Assets/App/Theme/Brand/WolfieHub.png` as a single UGUI sprite, preserves its source aspect ratio, and sits in the lower-right within a tunable bottom-right inset.
- **Approved static welcome bubble:** `WolfieSpeechBubble` is the sole approved bubble. Position it in the unused upper-left space of `MascotArea`, to Wolfie’s left and slightly above the raised hand, with a small pointer toward Wolfie. It is a rounded, responsive, non-interactive UGUI surface fully contained within `MascotArea`; it says exactly: `¡Hola! Elige un juego y aprendamos juntos.` The bubble, pointer, and text have raycasts disabled; Wolfie renders above them so the face, glasses, and silhouette remain unobscured.
- **Non-interaction rule:** Wolfie is a decorative `Image` with raycasts disabled. The mascot and approved static welcome bubble have no Button, EventTrigger, collider, dialogue interaction, animation, effects, or mascot behavior.
- **Future extensibility:** `MascotArea` is intentionally a sibling of `GamesArea`, so later approved speech, effects, or interaction can be added without restructuring the game gallery. Interactive dialogue, effects, and animation remain deferred.

The current `LobbyController` Inspector serializes `mascotSprite`, `backgroundDecorations`, `backgroundDecorOpacity`, `backgroundDecorBaseSpeed`, and `cardTitleFont`. The design-target fields `mascotAreaWidthFraction` (0.20–0.30) and `mascotBottomRightInset` (reference-pixel bottom-right inset) are not currently serialized. Those target fields would tune only mascot presentation; they would not change gallery, card activation, or navigation behavior.

### Runtime implementation note

The current branch visibly implements a Wolfie avatar/profile-style capsule, a presentation-only difficulty selector, vertical category sections with horizontal card rows, rotating background decorations, and `Coming soon`/`Opening...` cues. These are implementation facts for review, not an automatic amendment of this approved design. The product/design decision remains whether to promote them into the approved direction or align the implementation in a follow-up.

## Game-card anatomy

Each complete rounded card is one target; do not require a separate small “Play” button.

1. **Art zone:** a rounded 16:9 (width / height) thumbnail frame with an edge-to-edge replaceable abstract illustration or simple line icon, intentionally cropped or fitted to identify the card at a glance.
2. **Title zone:** in the `logica` section, show the explicit subject label assigned in the catalog instead of the game name; all other sections show the short game name. Any example such as `Game title` is a placeholder, not proposed game content or final naming.
3. **Optional metadata:** one concise, non-essential label only when it genuinely helps recognition. Omit it rather than creating dense card copy.
4. **State cue:** border, tonal surface change, or visible badge for availability and feedback—never text alone.

### Card states

| State | Visual treatment | Interaction expectation |
| --- | --- | --- |
| Default | White proposed-neutral surface, rounded corners, clear title and art, subtle purple structural emphasis. | Entire card is touchable. |
| Pressed | Immediate scale or tonal response plus a clear purple/orange emphasis that remains visible while touched. | Supports simultaneous independent touches; no hover is required. |
| Selected / opening | Stable highlighted outline or overlay with a short non-blocking transition cue. | Prevent duplicate activation of the same card without blocking other users. |
| Unavailable | Reduced emphasis plus an explicit icon and short label; do not rely on opacity alone. | Not selectable. Use only for a real, defined availability condition. |
| Feedback | Success/error only for a meaningful system result, using the proposed semantic colors until approved. | Feedback must not obscure neighboring cards or prevent their use. |

## Touch and readability principles

- Design every interactive element as a large, forgiving touch region with clear separation from neighboring targets; the card is the primary target.
- Size title text, icons, indicators, and state changes for long-distance viewing first, then validate that they remain clear on tablets. Avoid dense labels, fine outlines, and low-contrast secondary text.
- Support multiple concurrent touches: actions must be spatially independent, touch feedback must be local to the touched card, and no gesture should require precision, drag dexterity, keyboard, or mouse.
- Use visible labels alongside icons whenever an icon’s meaning could be ambiguous. Icon-only utility controls must have a universally recognizable line icon and a large target.
- Do not depend on color alone to communicate selection, availability, success, or error; pair it with shape, label, icon, or tonal change.

## Decoration rules

- Optional space motifs—stars, orbit lines, small planets, or abstract motion marks—may frame empty space or sit behind content at low visual priority.
- Future LBS+ character art may occupy a decorative edge area, but it is replaceable, non-interactive, and cannot block titles, cards, states, or touch paths.
- The Hub must work with no supplied character or space asset. Prefer simple vector-like geometry and line motifs over asset-dependent compositions.
- Decoration must never mimic or copy MathBug branding, UI, illustrations, assets, layouts, or specific visual expression.

## Deferred / out of scope

- UI implementation, interaction code, final breakpoint measurements, and asset production.
- Final product name, final logo lockup, and any new brand mark.
- Profiles, player setup, teacher setup, accounts, authentication, and permissions.
- Detailed game content, final game titles, game-specific worlds, and individual game screen design.
- Secondary/prep yellow/cyan theme and final character assets.
- Interactive mascot dialogue, visual effects, and animation beyond the approved static welcome bubble.
- MathBug-derived branding, UI, assets, or visual expression.

## Review checklist

- [ ] The first actionable content is a direct game-selection gallery.
- [ ] Purple and orange are used in their approved roles, while proposed neutrals, ink, success, and error remain explicitly provisional.
- [ ] Volte Regular, rounded surfaces, simple line icons, large multitouch targets, and long-distance readability are visible in the design.
- [ ] The gallery scales without inventing game content or requiring unavailable decorative assets.
- [ ] The screen remains a standalone LBS+ Games experience and does not reproduce MathBug’s visual expression.
