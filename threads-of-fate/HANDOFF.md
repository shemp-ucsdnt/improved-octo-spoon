# Project Handoff — read this first

*Written for a fresh Claude Code session with zero memory of prior conversations. This supersedes the framing in `IMPLEMENTATION_SPEC.md` and `BUILD_STATE_SUMMARY.md` — read those for background, but the decisions below are newer and take precedence where they conflict. This also supersedes the previous version of this same file (the one that opened with "Rename Rue/Mint/Kaelen..." as a to-do item) — that work is now done; see below.*

## What this project is

An open-world, retro-3D (PS1/N64-era), ability-gated Metroidvania. One playable avatar manifests as three switchable models — **Sorrel** (Morph/shapeshifting), **Thyme** (Invoke/elemental casting), **Chervil** (Construct/soul-weaving) — each owning one discipline, all three drawing from a single shared MP pool so they combine within one encounter (HP is *not* shared — see Decision 1 below). Progression is repertoire, not stats: what you can do is what you've earned from the world itself (a defeated enemy's shape, a spell learned at a font shrine, a machine assembled from harvested Threads), never an arbitrary keycard item.

**The hook, in the person's own words: "the novelty of exploration."** This is a movement- and discovery-first game. The retro presentation isn't nostalgia — it's a legibility constraint (Style Guide Section 6) that keeps a large, systemic world readable without HUD clutter.

**The one pillar that survives any scope cut** (confirmed, don't relitigate this): *"One connected world, no loading walls — true open-world, ability-gated exploration; backtracking is a core verb, not a chore."* Every other pillar (retro legibility, transformation-as-map-key, systemic-not-scripted) is real and should still guide decisions, but if something has to give under scope pressure, protect this one first.

## Audience (confirmed, unchanged)

- **Who it's for**: Pseudoregalia players — people who want tight, movement-driven 3D exploration with minimal hand-holding, low-poly presentation as an aesthetic choice rather than a budget compromise.
- **Who it's explicitly not for**: players who expect waypoint markers, quest-arrow guidance, or fast-travel-as-menu-teleport. Losing that audience is an accepted, intended trade.
- **Quality-of-life philosophy**: comfort-of-life additions should be *in-world traversal enhancements* (mounts, movement tech) rather than *world-skipping* ones (fast travel, teleport menus). (Open question: whether a mount is a Sorrel Morph or a separate system — still not decided.)

## Team & scope reality (confirmed, unchanged)

- **Solo developer.** No team. Sequencing and scope discipline matter more than they would for a funded studio.
- **This spec is the full north-star**, not a demo scope.
- **Sequencing**: long-term target is depth-first (fully realize one biome as a proof before scaling breadth), but near-term work stays breadth-first (data-driven architecture across all three disciplines) because the shader/render pipeline is blocked until Unity runs reliably somewhere with a working Editor. See "Render pipeline / cloud VM status" below — this is still blocked, and the blocker is now well-understood rather than vague.
- **Co-op is explicitly out of scope.** Forked elsewhere. Don't design around it.

## Naming migration — DONE

`Rue/`→`Sorrel/`, `Mint/`→`Thyme/`, `Kaelen/`→`Chervil/` is complete: folders, namespaces, class/file names, the `CharacterId` enum, and every construct/spell/form ID now use the canonical names throughout `Unity/Assets/Scripts/` (48 C# files). Verified zero leftover placeholder references. The project itself still has no locked title (`[Working Title]` throughout the style guide) — don't invent one. The `threads-of-fate/` folder name is stale but unrenamed; still just a housekeeping item, not a priority.

## Key decisions resolved

1. **MP is shared across all three models; HP is not.** One `SharedManaPool` component (in `Core/`) is referenced by all three characters' behaviours — switching models never touches it, so drain/regen continues uninterrupted across a switch, satisfying that requirement for free. HP stays on each character's own `CharacterVitals` instance, independent per model — **this was an open question in the prior handoff, now explicitly confirmed by the person** (not inferred). `CharacterVitals` no longer has any MP fields at all; that split is intentional, not a partial refactor.
2. **Model switching is free, instant, anywhere.** `SwapStation` and its proximity gate are deleted entirely. `CharacterSwitchController.SwitchTo()` is unconditional.
3. **Chervil's construct roster was fully replaced** by an 11-construct table the person supplied directly (not from `player-mechanics.pdf`) after reviewing a PDF export of it. **This roster is now canonical and replaces the PDF's Section 3 entirely** — Sorrel's forms and Thyme's spells still come from `player-mechanics.pdf` and are unaffected. The full source table is now in the repo at `CHERVIL_ROSTER.md`; see "Chervil roster" below for the condensed version.
4. **Co-op: not this project.**
5. **Render/shader pipeline is explicitly parked** at the person's request ("forget the render pipeline for now, I have some decisions on my end to make"). Don't resume shader/visual-identity work until told to. A *narrower* ask — get a plain C# **compile check** running on a cloud VM — was attempted this session and is now blocked; see below. That compile-check effort is separate from the shader/render-pipeline hold and can be resumed independently if the person wants.

## Current implementation state

`Unity/Assets/Scripts/` — 48 C# files, **still never compiled or run** (no working Unity install anywhere this project has touched yet — see "Render pipeline / cloud VM status"). Treat all of it as carefully reasoned, not verified.

- **Sorrel**: all 18 canonical forms implemented across the 4 families (Fang & Wing, Hollow & Wisp, Stone & Iron, Root & Thread), sourced from `player-mechanics.pdf`. `MonsterFormDefinition` gained descriptive fields (`family`, `statsSummary`, `primaryAttack`, `specialUtility`, `fieldTacticalUse`) to preserve the PDF's qualitative info (no numeric Def/Crit/HP-per-form system exists, so those stay text-only). Only 4 forms are wired to the keyboard test harness (`SorrelTestInput`: Fangstrider/Drift-Eel/Cragclaw/Root-Sprite, chosen to match the existing heavy-switch/narrow-passage/hazard-flight world puzzle pieces) — all 18 exist as data via `TestMapBuilder.BuildSorrelKit`. Colossus (boss-drop) is registered but not coin-seeded, since no monster-defeat/grant system exists yet.
- **Thyme**: all 35 canonical spells across the 7 elements (Lumen/Tide/Ember/Gale/Umbra/Volt/Aurum), sourced from `player-mechanics.pdf`. `MagicColor` (4 placeholder colors) was replaced by the real `MagicElement` enum. Added support for Overflow-tier "drain the whole MP bar" spells (`consumesAllMP`/`minimumMPToCast` on `SpellDefinition`) rather than forcing them into a flat-cost model. Keyboard harness only exercises the 4 world-puzzle elements (Ember/Tide/Gale/Umbra) × 2 cast shapes (Bolt/Arc) — all 35 exist as data via `TestMapBuilder.BuildSpellFactory`.
- **Chervil**: **11 constructs, entirely from the person's own table, not the PDF** (see below). `ConstructDefinition` gained `deployMPCost` (flat MP charged once at deploy, on top of thread cost — a real new mechanic, wired into `ConstructDeploymentManager.DeployConstruct`), `isMobile` (data-only flag, no construct movement/AI system exists to act on it), and `combatRole`/`overclockDescription`/`deconstructDescription` text fields. Kinetic Energy costs are all zeroed (the new roster's Overclocks are flat-MP-only; rather than deleting the Kinetic Energy subsystem, `overclockKineticEnergyCost = 0` everywhere, which `ConsumeKineticEnergy(0)` trivially satisfies) — the subsystem (`ChervilResourcePool.KineticEnergy`, `ChervilComboKineticGenerator`) is still there, just unused by this construct set. 5 of 11 are wired to the keyboard harness (`ChervilTestInput`: Phlogiston Sprinkler/Tidal Conduit/Mist Weaver/Umbral Decoy/Prism Turret on E/R/T/Y/U, all now with real Overclocks on G/H/J/N/M — the prior illustrative data had Decoy/Turret as Overclock-free, which was wrong even before the roster replacement).

### Chervil roster (canonical, replaces the PDF)

| Construct | Recipe | Mobility | Deploy Cost | Upkeep |
|---|---|---|---|---|
| Blade-Beast Sentry | Beast+Iron | Mobile | 10 MP | 1 MP/s |
| Firefly | Fire+Beast | Mobile | 10 MP | 1 MP/s |
| Aegis Crawler | Iron+Wind | Mobile | 15 MP | 1 MP/s |
| Umbral Decoy | Phantom+Wind | Mobile | 20 MP | 2 MP/s |
| Gale Spring | Beast+Wind | Immobile | 15 MP | — |
| Voltaic Coil | Iron+Lightning | Immobile | 20 MP | — |
| Prism Turret | Crystal+Lightning | Immobile | 20 MP | — |
| Tidal Conduit | Water+Iron | Immobile | 20 MP | — |
| Mist Weaver | Water+Wind | Immobile | 10 MP | — |
| Steam Engine Ram | Iron+Fire+Water | Immobile | 30 MP | — |
| Phlogiston Sprinkler | Fire+Water | Immobile | 15 MP | — |

Full ability names/descriptions (Overclock + Deconstruct) are in `TestMapBuilder.ConfigureConstructs` — bespoke unique mechanics with no supporting system yet (stealth, phase-dash, invisibility, magnetism, partial single-thread refunds, HP↔MP conversion) are captured in description text only, not partially faked into numeric fields, same discipline as everything else in this file.

## Render pipeline / cloud VM status — blocked, well-understood now

Shader/render-pipeline work itself stays parked per the person's explicit instruction. Separately, a **plain compile-check on a cloud VM** was attempted this session and hit real, specific blockers — don't re-attempt naively, read this first:

1. **No Unity install works locally.** Unchanged from before — Unity Hub doesn't run on this Mac (macOS 12.6.3).
2. **`.claude/skills/oci-unity-render`'s bootstrap script only installs Unity Hub, not an Editor version.** `render.sh`'s final step calls a `unity-editor` binary that nothing actually creates — an explicit `unityhub --headless install --version <X>` step needs adding before this pipeline can work at all, on any shape. **Install exactly `2022.3.62f3`** (changeset `96770f904ca7`) to match `ProjectVersion.txt` below — don't let Hub install whatever's latest/default.
3. **`ProjectSettings/ProjectVersion.txt` now exists** (`2022.3.62f3`, changeset `96770f904ca7`, verified against Unity's public release API, not guessed) — this blocker is resolved. Note this is *only* `ProjectVersion.txt`; the rest of `ProjectSettings/` (`ProjectSettings.asset`, `TagManager.asset`, `EditorBuildSettings.asset`, etc.) still doesn't exist. Unity is expected to auto-generate those with defaults on first real open (normal for a project that's never been opened in an Editor) — this is expected, not re-verified, since no Editor has actually opened this project yet. If batch mode instead balks or prompts for something interactively on first open, that's the next thing to debug.
4. **Unity Editor license activation cannot be automated.** Even Personal-tier requires a logged-in Unity account; nothing in this pipeline (nor Claude generally) can enter credentials on the person's behalf. The realistic flow is: get the VM to "Editor installed, project synced," then hand the person SSH access to activate interactively themselves, then resume.
5. **Unity Editor has no Linux ARM64 build at all** — `VM.Standard.A1.Flex` (Ampere/ARM) cannot run it, full stop, not just "less proven." The person was told this explicitly and chose to try A1.Flex anyway for a different, narrower reason (see below) — for an actual compile-check attempt, use an x86_64 shape (e.g. `VM.Standard.E3.Flex`, paid but cheap for this workload, not GPU — GPU is only relevant to real rendering, which is still on hold).
6. **`VM.Standard.A1.Flex` in `us-ashburn-1` is currently out of capacity** across all 3 ADs (a well-known Always-Free-tier scarcity issue, not specific to this account) — a launch attempt via the new `oci-vm-provisioner` skill failed cleanly on this with no resources created/billed. This was a deliberate test of that new skill's mechanics (dry-run validation → confirm → launch), not a real attempt at the Unity goal — the person explicitly chose the free ARM shape for that test knowing it can't run Unity, separately from the E3.Flex recommendation above for when the actual compile-check is resumed.

**What already exists and is safe to reuse, not billed:**
- A VCN + public subnet in the person's OCI tenancy (`us-ashburn-1`, compartment = tenancy root — no sub-compartments exist). Ashburn image OCIDs for Ubuntu 22.04 (both x86_64 and aarch64) were already resolved this session; re-resolve if stale.
- An SSH keypair at `~/.ssh/oci_compile_check` (+ `.pub`).
- `Unity/Assets/Scripts/Editor/CompileCheck.cs` — the batch-mode entry point (`-executeMethod ThreadsOfFate.Tooling.CompileCheck.Run`), harmless to leave in the project regardless of when the VM attempt resumes. Writes a small report to `render-output/` and exits 0 if every script compiled.
- `Unity/ProjectSettings/ProjectVersion.txt` — pins the project to `2022.3.62f3`. See blocker 3 above for what's still missing beyond this one file.
- `oci` CLI is installed (via Homebrew) and authenticated; `~/.oci/config` has working tenancy/user/region credentials.
- Two OCI provisioning paths now exist: the original `.claude/skills/oci-unity-render` (full render pipeline, GPU-oriented, still needs the Editor-install fix above) and the newer `.claude/skills/oci-vm-provisioner` (general-purpose VM provisioning with automatic Always-Free capacity retry across ADs — cleaner for a plain compile-check VM, doesn't install anything Unity-specific itself).

## Open questions — not resolved, don't assume answers

- Is a "mount" system a Sorrel Morph, a separate fourth system, or something else? Still only mentioned once, as a QoL philosophy note.
- MP pool's growth curve across the campaign (unset).
- Unlock ordering — which form families / elements / constructs come online in which world regions (unset).
- Biome list and world map topology (unset).
- Fixed internal render resolution — 320×240 vs 480×270 vs something else (unset; affects UI scale, texture budgets, streaming chunk size — resolve before content production scales, and before the shader pipeline resumes).
- The project's actual title.

## Recommended next steps

1. **If resuming the compile-check VM effort**: fix the bootstrap script's missing Editor-install step, generate a local `ProjectVersion.txt`, use an x86_64 shape (not A1.Flex), and plan for the person to interactively activate the Unity license over SSH partway through. Don't relaunch blind — all four gaps above need addressing, not just the capacity issue.
2. ~~Copy the Chervil roster source table into the repo~~ — done, see `CHERVIL_ROSTER.md`.
3. Continue expanding world-layout/level content once the person is ready — architecture supports it; content is the remaining gap for Sorrel/Thyme/Chervil's *un*-keyboard-exposed data slices too, if broader in-harness testing is ever wanted.
4. Leave shader/render-pipeline work parked until explicitly resumed.
