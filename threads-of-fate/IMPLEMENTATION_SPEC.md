# Threads of Fate — Implementation Spec

## 1. What's built so far (Phases 1–5 done)

Unity only, going forward — the UE C++ port under `Source/` is frozen as of
the Mint/Kaelen test-map stage and won't receive anything past that.

### Core — shared foundation (`Unity/Assets/Scripts/Core/`)
- `IDamageable`, `CharacterVitals` (per-character HP + MP, independent per
  character — nothing shared), `IStatusAffectable` (minimal status-flag
  set/clear, implemented by `CharacterVitals`).
- `EffectType` (Damage/Heal) + `EffectResolver.Apply()` — the one shared path
  that turns a spell/Overclock's effect data into an actual `IDamageable`
  call. Used by `MintTestInput` and `KaelenTestInput`.
- `MeleeComboComponent` — generic combo-chain tracker shared by Mint's
  ring-blades and Kaelen's whip-dagger (`OnComboHit` event).
- `CharacterId`, `SwapStation` (a location where swapping is permitted),
  `CharacterSwitchController` (location-gated swapping: `SwitchTo()` only
  succeeds while the active character is within range of a registered
  `SwapStation`; never moves anyone).
- `CharacterMovementController` — real WASD+jump movement via
  `CharacterController`, world-space (not camera-relative). Exposes
  `SetPhysicsOverride()`/`ClearPhysicsOverride()` for Rue's per-form physics.
- `CameraFollow` — simple offset follow-cam, retargets on
  `CharacterSwitchController.OnActiveCharacterChanged`.
- `EnvironmentInterfaces.cs` — `IHeavySwitch`, `INarrowPassage`,
  `IHazardFlight`: capability-query interfaces environment objects use to
  gate passage. Only Rue's forms implement them today, but they're not
  Rue-specific by design.

### Kaelen — Soul Constructing Architecture (`Unity/Assets/Scripts/Kaelen/`)
- `ThreadInventoryComponent`, `ConstructDeploymentManager` (deploy/Overclock/
  Deconstruct, tick MP drain, auto-Deconstruct-all at 0 MP) as before, plus:
  - Deploying now actually spawns a visual (`ConstructDefinition.visualPrefab`
    if set, else a placeholder capsule in `placeholderColor`) and assigns it
    to `ActiveConstruct.spawnedActor` — previously declared but never set.
  - `TryGetActiveSpawnedActor()` lets external code (currently the test map)
    attach construct-specific extras to that spawned visual after deploy.
  - `SetMaxSimultaneousConstructs()` / `RegisterConstructs()` for runtime setup.
- `KaelenResourcePool` (MP delegates to a sibling `CharacterVitals`, owns
  Kinetic Energy) + `KaelenComboKineticGenerator` (binds `MeleeComboComponent`
  hits to Kinetic Energy — previously nothing generated it, only Overclock
  consumed it).
- Roster grew from 3 to 5 constructs: Phlogiston Boiler, Tidal Conduit, Mist
  Weaver (as before) plus two previously-unbuilt stealth/utility constructs:
  - `AggroDecoy` — marker component for Umbral Decoy; a real hook a future
    enemy-AI targeting system can query, since no enemy AI exists yet to
    actually redirect.
  - `PrismTurretBeam` + `PrismMirror` + `IPrismReceiver` + `PrismReceiverGate`
    — a real raycast-and-reflect laser puzzle (bounces off `PrismMirror`
    surfaces, triggers `IPrismReceiver` on landing). `PrismReceiverGate`
    adapts a generic `World.ActivationGate` to it.
  - `CleansingFogUtility` — clears `IStatusAffectable` statuses in a radius
    (Mist Weaver's Deconstruct utility blast).
- All 5 constructs now have non-empty, coherent (but still illustrative —
  not designer-balanced) thread recipes.

### Mint — Elemental Spell Synthesis (`Unity/Assets/Scripts/Mint/`)
- `SpellCombinationFactory`/`SpellDefinition` as before, now using the
  shared `Core.EffectType` instead of a Mint-local enum.
- `MintComboMPRegen` — binds `MeleeComboComponent.OnComboHit` to
  `CharacterVitals.RestoreMP()`, per the spec's "MP regen from combo chains."
- Spell matrix expanded from 2 illustrative rows to 8 (every color ×
  Burner/Restore) — still illustrative, not a designer-authored final list.

### World (`Unity/Assets/Scripts/World/`) — new folder
`ElementalTrigger` and `Rotator` were promoted here from `TestMap/` — they're
real, reusable level mechanics, not test-only scaffolding. `ElementalTrigger`
gained an `OnActivated` event so puzzle pieces can react without it knowing
they exist:
- `ActivationGate` — generic "remove an obstacle" gate, reusable by any
  trigger source.
- `FreezeWalkway` — turns "freeze the water" into a real traversal puzzle:
  a solid blocker before freezing, a flush walkable ice platform after.
- `TurbinePlatform` — raises a connected bridge when the turbine engages.
- `HeavySwitchPlate`, `NarrowPassageGate`, `HazardZone` — Rue's three
  environment-interaction world objects, gated by `IHeavySwitch`/
  `INarrowPassage`/`IHazardFlight` on whatever enters them.

### Rue — Monster Shapeshifting System (`Unity/Assets/Scripts/Rue/`) — new, built from scratch
- `MonsterCoinQueue` — strict 5-slot FIFO; a 6th `Collect()` evicts the
  oldest. Holding a coin (not consuming it) is what the implementation
  interprets as "able to transform into that form" — the spec describes the
  queue but not exactly how holding a coin relates to transforming, so this
  is a judgment call, not confirmed design.
- `MonsterFormDefinition` — a ScriptableObject per form: gravity scale, jump
  height, move-speed multiplier, fly flag, `CharacterController`
  height/radius (hitbox swap), body weight/size (for the environment
  interfaces), continuous MP drain rate, and a discrete ability + cost.
- `RueTransformationController` — `TransformInto(formId)` (requires holding
  that form's coin), continuous MP drain while transformed mirroring
  `ConstructDeploymentManager`'s tick-drain pattern, auto-revert to base form
  at 0 MP, `UseFormAbility()` (discrete cost). Implements `IHeavySwitch`/
  `INarrowPassage`/`IHazardFlight` from current form state, and pushes
  physics changes into `CharacterMovementController` on transform.

### Unity Test Map (`Unity/Assets/Scripts/TestMap/`)
`TestMapBuilder` now builds: real WASD+jump movement and a follow camera for
whichever character is active; Mint's 4 elemental stations wired to real
puzzle consequences (gate, walkway, bridge); 5 Kaelen deploy pads; Rue's
three environment pieces (switch, passage, hazard) plus a Prism Turret
mirror/receiver; all three characters with independent `CharacterVitals`,
each with their kit fully wired (Mint: spells + combo MP regen; Kaelen:
constructs + combo Kinetic generation; Rue: pre-seeded with all 4 coins so
transforming is testable immediately, since no monster-defeat/pickup system
exists to earn them legitimately yet). Full control scheme is documented in
the file's header comment.

**Known unverified spot**: the Prism Turret's mirror/receiver placement is a
best-guess at reflection geometry — there's no Unity Editor available in
this environment to test actual beam angles, so it may need repositioning.

**Still not real, deliberately out of scope**: enemy AI (nothing exists to
redirect via `AggroDecoy`, or to fight at all beyond the static
`TargetDummy`), animation, UI/HUD, and everything in Phases 6–7 below.

## 2. Decisions (resolved)

1. **Engine target: Unity, going forward.**
2. **Character control model: location-gated swapping.** Single character
   controlled at a time; switching is only permitted near a `SwapStation`.
   Switching itself doesn't move anyone.
3. **Resource pools: separate per character.** Rue, Mint, and Kaelen each
   own an independent `CharacterVitals`. Kaelen additionally has Kinetic
   Energy since it's the only kit that uses it.

## 3. Phased roadmap

### Phase 1 — Combat & resource foundation — done
`IDamageable`, per-character `CharacterVitals`, `TargetDummy`, and the
three-character/`CharacterSwitchController` foundation the later decisions
required.

### Phase 2 — Mint completion — done
Combo tracking, combo-driven MP regen, real world-interaction consequences,
an 8-cell spell matrix.

### Phase 3 — Kaelen completion — done
Kinetic Energy generation, construct visual spawning, the two previously
unbuilt stealth/utility constructs (Umbral Decoy, Prism Turret) plus the
status-cleansing fog, and real thread recipes.

### Phase 4 — Rue (Monster Shapeshifting System) — done
Built from scratch: FIFO coin queue, per-form data asset, transformation
controller with tick MP drain and physics/hitbox swap, three environment
interfaces with real world objects.

### Phase 5 — Shared character/world systems — done
Real movement (`CharacterMovementController`), a follow camera
(`CameraFollow`), and the damage/heal resolution promoted into a shared
`EffectResolver` — though it's still *called* from the test harness
(`MintTestInput`/`KaelenTestInput`), not from a real player-ability-input
system, since there isn't one yet. That gap is exactly what Phase 6/a real
player controller closes.

### Phase 6 — Content & data authoring
1. Real `ConstructDefinition`/`SpellDefinition`/`MonsterFormDefinition`
   assets authored by design, replacing every "illustrative" value flagged
   above — construct recipes, spell magnitudes, form stats, ability
   pairings, all of it.
2. Purpose-built levels per mechanic — the current test map is a flat
   validation arena, not a real level. In particular the Prism Turret's
   mirror geometry needs real in-Editor tuning.
3. Real `SwapStation` placement (dungeon entrances, NPC hubs) as level design,
   replacing the single catch-all "NPC Hub."
4. A monster-defeat/coin-drop system so Rue's `MonsterCoinQueue` fills
   through actual play instead of being pre-seeded for testing.
5. Animation hookups per form/spell/construct — nothing animates today,
   everything is primitives.

### Phase 7 — Production polish
1. UI/HUD (HP/MP/Kinetic Energy bars, thread inventory, combo counter,
   coin queue) — everything today is Console-log-only.
2. VFX/audio on the existing Observer-pattern hooks (`OnConstructDeployed`,
   `OnConstructOverclocked`, `ElementalTrigger.OnActivated`,
   `RueTransformationController.OnFormChanged`, etc. — all built with
   exactly this in mind).
3. Enemy AI — nothing exists to fight, redirect via `AggroDecoy`, or use
   `IHazardFlight`/`IHeavySwitch` against; this was explicitly deferred
   rather than faked.
4. Save/load.
5. Automated tests: unit tests for `ThreadInventoryComponent`/
   `SpellCombinationFactory`/`MonsterCoinQueue`, Play Mode tests for
   deployment/transformation/movement flows.

## 4. Suggested immediate next step
Phase 6 — every mechanical system named in the original spec now exists in
code, so the highest-leverage next step is replacing illustrative data with
real designed values and building an actual level, rather than more systems
code. The one exception: Phase 7's enemy AI is the biggest remaining gap if
the goal is a playable vertical slice rather than a content pass — worth
deciding which matters more before starting Phase 6.
