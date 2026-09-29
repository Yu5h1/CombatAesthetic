# CombatAesthetic Handoff

## Current User Direction

User clarified that CombatAesthetic is about the aesthetics of action/fighting gameplay: game feel, action combat, duel readability, hit response, animation/FX/audio timing, and presentation quality.

Systems that are generally reusable outside this action-combat aesthetic layer should be extracted into Yu5h1Lib packages. CombatAesthetic should become the integration/product layer that composes those packages for action-combat gameplay, not the place where every generic system lives.

## Coordination Notes

- No active `handoff.md` existed before this file.
- Do not implement package moves yet unless the user explicitly asks.
- Existing code has not been moved or refactored in this planning pass.
- A discussion plan exists at `Documentation/.plan/UPM-Package-Split-Plan.md`.
- `plan.md` is the human-facing project plan and should keep the user's language/context.

## Architectural Decision Snapshot

CombatAesthetic should keep:

- Action combat feel and timing.
- Character combat composition.
- Hit/hurt/cancel/reaction aesthetics.
- Animator/FX/audio/UI presentation coordination for combat.
- Samples, presets, and integration behaviours that express the combat aesthetic.

Move or create Yu5h1Lib packages for:

- Generic gameplay data and attributes.
- Generic 2D physics/detection helpers.
- Generic input command adapters.
- Generic AI route/patrol tools.
- Generic UI controls/stat views.
- Generic animation/Timeline/SMB helpers.
- Generic pooling, event, resource, and editor utilities.
- Future network/channel/snapshot abstractions.

## Recommended Package Extraction Direction

Start with broad package boundaries instead of many tiny packages:

1. `GameplayCore`
2. `Gameplay2D`
3. `CombatCore`
4. `Combat2D`
5. `CombatAnimation`

Later split adapters:

- `CombatInput`
- `CombatAI`
- `CombatUI`
- `CombatFX`
- `Network`

## Known Coupling Risks

- `CharacterController2D` currently mixes movement, attribute damage, input host, detector, scanner, caster, teleport, PlayerPrefs tuning, invincibility visuals, and GameManager pause state.
- `AnimatorCharacterController2D` currently mixes Animator root motion, skill selection, FX cast, audio playback, UI/cursor operations, and combat state.
- `AttributeBehaviour` owns UI references, which blocks clean extraction into model/core.
- `HostData2D` is a good command-source concept, but it directly depends on `CharacterController2D`.
- Several 2D event/physics utilities depend on `GameManager` or `SceneController`, which prevents them from being low-level 2D package code.

## Next Safe Steps

- For progression design, resolve [尚未決定](Documentation/plans/Progression-and-Ascension-Design.md#尚未決定) before implementing the [suggested prototype](Documentation/plans/Progression-and-Ascension-Design.md#建議初版與案例). The design is documented; gameplay behavior has not been implemented or tested.

1. Produce a class-to-package mapping table before moving files.
2. Mark generic classes that should be promoted to Yu5h1Lib package candidates.
3. Define command-source interfaces before touching input/AI/network.
4. Define presentation events before splitting UI/FX/audio out of combat behaviours.
5. Only then create package branches and move files in small batches.

## Open decisions

- Progression and ascension: [尚未決定](Documentation/plans/Progression-and-Ascension-Design.md#尚未決定). This is a discussion draft, not an approved implementation specification.
- Question: Should CombatAesthetic's game core adopt physical aesthetics as a base rule?
  - Options: enforce no process-free teleportation and no perfect cloning; allow these only as explicit authored exceptions; leave them unrestricted for convenience.
  - Recommendation: enforce the principle by default, because combat feel and world consistency depend on visible/physical process, source, distance, and material continuity.
- Question: How do players enter or get born into the world?
  - Options: enter from outside the arena; spawn from a field/device; assemble from existing material; be summoned/dropped by another system; defer until the first playable prototype.
  - Recommendation: decide before implementing spawn/respawn/network authority, because it affects rules, animation, synchronization, fairness, and anti-cheat.

## Current Decision Report

- User wants a priority task and decision checklist before implementation.
- Human-facing report added at `Documentation/plans/Priority-Decision-Report.md`.
- TaskProgress report added at `report.json` with scope `combataesthetic`.
- TaskProgress now includes `physical-aesthetic-core` to track no perfect cloning, no process-free teleportation, and player spawn/birth decisions.
- TaskProgress CLI opened the local viewer route: `http://127.0.0.1:8001/?scope=combataesthetic`.
- Next discussion should answer the decision list in that report, starting with the initial Viewer/View success standard and Back/return behavior.
- Do not implement Viewer/View migration until the user validates those decisions.

## Refactor Guidance Added

User asked for clearer refactoring direction, not only package splitting. A guide was added at `Documentation/.plan/Refactor-Direction-Guide.md`.

Current recommended starting point:

1. Clarify `AttributeBehaviour` first because it couples gameplay data, Unity lifecycle, UI refresh, depletion rules, and skill cost validation.
2. Clarify `HostData2D` into a command-source concept because Player, AI, Network, and Replay should all feed the same role.
3. Slim `CharacterController2D` into a command-consuming character executor before trying to make it pure or package-ready.
4. Treat `AnimatorCharacterController2D` as a presentation bridge, not core combat logic.

Architecture stance:

- Do not introduce ECS or a DI container as the first step.
- Use Yu5h1Lib AdapterFactory-style injection only where there is one purpose with multiple implementations.
- Focus first on interfaces, events, and dependency direction.
- ScriptableObject remains useful for authoring data/settings, but runtime state and future rollback snapshots should not live primarily in SO assets.

## Scope Correction

User clarified later that the broader refactor discussion is not CombatAesthetic-specific. CombatAesthetic remains an action-combat aesthetics application/use case. The general data-driven composition/capability-resolution architecture belongs to Yu5h1Lib UnityExtension.

Canonical current plan location:

- `C:\Users\Yu5h1\Dev\VSProjects\Yu5h1Lib\Unity\UnityExtension\plan.md`
- `C:\Users\Yu5h1\Dev\VSProjects\Yu5h1Lib\Unity\UnityExtension\handoff.md`

Do not continue treating CombatAesthetic as the owner of that general architecture. Use it only as a validation case.
