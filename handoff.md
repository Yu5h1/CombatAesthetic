# Combat Aesthetics Handoff

GitHub repository: [Yu5h1/Combat-Aesthetics](https://github.com/Yu5h1/Combat-Aesthetics). Local folder is now `Unity/Combat-Aesthetics`, renamed by the user; origin uses the corresponding GitHub URL. Unity package identifier and code type names retain their existing names. TaskProgress scope remains `combat-aesthetic` and points to this new local folder.

## Local checkout path migration

The user subsequently completed the local rename to `Unity/Combat-Aesthetics`. The old `Unity/CombatAesthetic` path is absent. TaskProgress registration and the workspace overlay's handoff reference are synchronized to the new path; no compatibility junction was created by this chat.

The user subsequently asked to finish the reference repair. KFC's `Packages/manifest.json` and `Packages/packages-lock.json` now point to `file:C:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/Combat-Aesthetics`; each changed one path only, with all other bytes preserved and package identifier unchanged. KFC files have not been committed by this chat. Yu5h1LibTest was inspected read-only: no CombatAesthetic reference was found in Assets, Packages or asset junction targets; do not add a dependency merely to perform this rename. No Unity instance was launched or package resolution run.

## WhiteRoom sample planning

Current user direction: keep WhiteRoom in this existing package's Sample scope. The canonical implementation plan is [Samples/WhiteRoom/Implementation-Plan.md](Samples/WhiteRoom/Implementation-Plan.md). It supersedes the independent product-host proposal from the UX discussion. This request authorizes planning, commit and push only; no demo implementation, Sample registration, dependency change or asset acquisition.

First delivery is a 3D PvE loop: movement/jump, stationary/directional attack, ordinary/precise defense, hit response/recovery, third-person camera and reset. Follow the plan for stages, acceptance and [美術與角色方向](Samples/WhiteRoom/Implementation-Plan.md#美術與角色方向); do not resume per-reaction interviews before resolving delivery prerequisites. Broader package extraction discussions below are separate work.

Validation for this documentation change is static and scoped; Unity import, gameplay and Windows builds have not been executed.

Preparation work is tracked by this package's Report card `white-room-stage0`, scope `combat-aesthetic` (canonical entry: [WhiteRoom plan](Samples/WhiteRoom/Implementation-Plan.md)). The user dispatches other agents. Follow [可先交辦的準備工作](Samples/WhiteRoom/Implementation-Plan.md#可先交辦的準備工作) for the read-only scope and isolated outputs. Individual instructions and progress belong to Report items. The shared-operation flow is now agreed; preparation agents must not decide the remaining tool/platform/data questions.

The reusable animation system now has its own local repo at `Yu5h1Lib/Character-Animation`. Continue system discovery through its [handoff](../../Character-Animation/handoff.md) and [plan](../../Character-Animation/plan.md); those files own the system goal, current question and confirmed answers. This repo owns Combat Aesthetics integration and WhiteRoom consumer tests. Preserve the consumer requirements in [WhiteRoom plan](Samples/WhiteRoom/Implementation-Plan.md), and follow the new repo's continuation route rather than maintaining a second question list here. No animation-system implementation or source move is authorized.

The user requested Chinese Report display text and scope `combat-aesthetic`. Report/task/item IDs, statuses, priorities, ordering and recorded history are preserved; the stable report_id remains unchanged. Text edits use Report operations. Root scope_id is not supported by report.update, so its rename is the scoped direct-edit exception, under revision lock and followed by Report validation; mention this exception if committing the rename. Use the new scope for links and local registration.

The [existing-capability inventory](Samples/WhiteRoom/Existing-Capabilities.md) still supplies reuse evidence, but its Editor/Clip writer is no longer the required pipeline. Reassess relevant pieces against the new source/runtime boundary rather than discarding all Yu5h1Lib capabilities. A read-only installation check on 2026-10-25 found Unity.exe under both 6000.3.9f1 and 6000.6.0f1; the user selected 6000.3.9f1 as the fixed first host baseline. No Editor was launched or compatibility verified. Host directory remains open.

Ruled out: selecting KayKit or another ready-made animation library as the first source. The user instead wants agents to build an accumulating animation library of their own; the earlier ready-made-animation direction is superseded. This does not authorize a new animation framework or the separate limb-generation system.

Superseded: .anim as the required first delivery format and an Editor/Clip-centered production workflow. The user now wants cross-platform character/motion data and runtime creation. Preserve self-authored source data/scripts and physical causality; do not silently retain mandatory Clip generation behind a new format. LimbSystem will be tested in WhiteRoom later, while preserving its independent ownership; no merger or implementation is authorized.

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


- WhiteRoom Sample: [尚未決定](Samples/WhiteRoom/Implementation-Plan.md#尚未決定) owns this consumer's integration, host directory, precise-defense and import/assembly questions. The separate system's [尚未決定](../../Character-Animation/plan.md#尚未決定) is owned by Character-Animation and routed by its handoff; do not decide or duplicate those answers here. Repo name/location, the split, both test approaches and shared-operation flow are agreed alongside the Unity 6000.3.9f1 URP validation baseline.
- Progression and ascension: [尚未決定](Documentation/plans/Progression-and-Ascension-Design.md#尚未決定). This is a discussion draft, not an approved implementation specification.
- Question: Should CombatAesthetic's game core adopt physical aesthetics as a base rule?
  - Options: enforce no process-free teleportation and no perfect cloning; allow these only as explicit authored exceptions; leave them unrestricted for convenience.
  - Recommendation: enforce the principle by default, because combat feel and world consistency depend on visible/physical process, source, distance, and material continuity.
- Question: How do players enter or get born into the world?
  - Options: enter from outside the arena; spawn from a field/device; assemble from existing material; be summoned/dropped by another system; defer until the first playable prototype.
  - Recommendation: decide before implementing spawn/respawn/network authority, because it affects rules, animation, synchronization, fairness, and anti-cheat.
- Directional input UX: [未決策事項](Documentation/plans/Directional-Input-UX-Design.md#31-觸控螢幕操控-mobile-touch-ux). Options include dynamic floating joystick vs. fixed octagonal D-pad, and double-tap dash vs. dedicated evade button.

## Current Decision Report

- User wants a priority task and decision checklist before implementation.
- Human-facing report added at `Documentation/plans/Priority-Decision-Report.md`.
- TaskProgress report is `report.json`, scope `combat-aesthetic` (includes task `directional-input-ux-design`).
- TaskProgress now includes `physical-aesthetic-core` to track no perfect cloning, no process-free teleportation, and player spawn/birth decisions.
- Local Viewer route: `http://127.0.0.1:8001/?scope=combat-aesthetic`.
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
