using System.Collections.Generic;
using UnityEngine;
using ThreadsOfFate.Core;
using ThreadsOfFate.Chervil;
using ThreadsOfFate.Thyme;
using ThreadsOfFate.Sorrel;
using ThreadsOfFate.World;

namespace ThreadsOfFate.TestMap
{
	// Procedurally builds a validation arena exercising every system built so
	// far (no scene-file authoring): Thyme's elemental stations (now with real
	// puzzle consequences), Chervil's 5-construct roster, Sorrel's shapeshifting,
	// real WASD+jump movement, a follow camera, and free instant character
	// switching (no station gating — all three share one MP pool that keeps
	// draining/regenerating across switches; HP stays independent per model).
	//
	// Drop this on an empty GameObject in an empty scene and press Play.
	//
	// Controls:
	//   WASD + Space          = move / jump (whichever character is active)
	//   F1 / F2 / F3          = switch to Sorrel / Thyme / Chervil (free, instant, anywhere)
	//   Thyme   1/2/3/4        = cast Ember/Tide/Gale/Umbra + Bolt (the 4 world-puzzle elements)
	//   Thyme   Shift+1/2/3/4  = cast Ember/Tide/Gale/Umbra + Arc
	//   Thyme   Left Mouse     = ring-blade combo hit (regenerates MP)
	//   Chervil E/R/T/Y/U      = deploy Sprinkler/Conduit/Weaver/Decoy/Turret
	//   Chervil G/H/J/N/M      = Overclock Sprinkler/Conduit/Weaver/Decoy/Turret
	//   Chervil Z/X/C/V/B      = Deconstruct Boiler/Conduit/Weaver/Decoy/Turret
	//   Chervil Left Mouse     = whip-dagger combo hit (generates Kinetic Energy)
	//   Sorrel    1/2/3/4        = transform into Fangstrider/Drift-Eel/Cragclaw/Root-Sprite
	//   Sorrel    0               = revert to base form
	//   Sorrel    Left Mouse     = use current form's ability
	// Watch the Console for results.
	//
	// Design liberties taken throughout (all illustrative, not confirmed
	// design): construct/spell/form stat values, thread recipes, Overclock/
	// Deconstruct ability-id pairings, and the interpretation that holding a
	// Monster Coin (not consuming it) is what lets Sorrel transform into that
	// form. Prism Turret's mirror/receiver placement is an unverified guess
	// at reflection geometry — no Unity Editor was available to test it.
	public class TestMapBuilder : MonoBehaviour
	{
		private void Start()
		{
			CameraFollow cameraFollow = BuildCameraAndLightingIfMissing();
			BuildGround();
			List<ElementalTrigger> stations = BuildElementalStations();
			BuildDeployPads();
			TargetDummy targetDummy = BuildTargetDummy();
			BuildWorldPuzzlePieces();
			BuildCharacters(stations, targetDummy, cameraFollow);
		}

		private CameraFollow BuildCameraAndLightingIfMissing()
		{
			CameraFollow cameraFollow = null;

			if (Camera.main == null)
			{
				GameObject cameraGo = new GameObject("Main Camera");
				cameraGo.tag = "MainCamera";
				cameraGo.AddComponent<Camera>();
				cameraGo.AddComponent<AudioListener>();
				cameraGo.transform.position = new Vector3(0f, 12f, -14f);
				cameraGo.transform.LookAt(new Vector3(0f, 0f, 2f));
				cameraFollow = cameraGo.AddComponent<CameraFollow>();
			}

			if (FindObjectOfType<Light>() == null)
			{
				GameObject lightGo = new GameObject("Directional Light");
				Light light = lightGo.AddComponent<Light>();
				light.type = LightType.Directional;
				lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
			}

			return cameraFollow;
		}

		private void BuildGround()
		{
			GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
			ground.name = "Ground";
			ground.transform.localScale = new Vector3(2f, 1f, 2f);
		}

		private List<ElementalTrigger> BuildElementalStations()
		{
			var stations = new List<ElementalTrigger>
			{
				BuildStation("Brazier_Ember", ElementalTriggerKind.Ignite, MagicElement.Ember,
					PrimitiveType.Cylinder, new Vector3(-6f, 0.5f, 5f), new Vector3(1f, 0.5f, 1f), new Color(0.3f, 0.3f, 0.3f)),
				BuildStation("WaterPool_Tide", ElementalTriggerKind.Freeze, MagicElement.Tide,
					PrimitiveType.Cylinder, new Vector3(-2f, 0.05f, 5f), new Vector3(3f, 0.05f, 3f), new Color(0.1f, 0.3f, 0.8f)),
				BuildStation("Turbine_Gale", ElementalTriggerKind.DriveTurbine, MagicElement.Gale,
					PrimitiveType.Cylinder, new Vector3(2f, 1f, 5f), new Vector3(1.5f, 1f, 0.3f), Color.gray),
				BuildStation("Barrier_Umbra", ElementalTriggerKind.Shatter, MagicElement.Umbra,
					PrimitiveType.Cube, new Vector3(6f, 1.5f, 5f), new Vector3(2f, 3f, 0.5f), Color.black),
			};

			ElementalTrigger brazier = stations[0];
			ElementalTrigger waterPool = stations[1];
			ElementalTrigger turbine = stations[2];

			turbine.gameObject.AddComponent<Rotator>().enabled = false;

			// Brazier -> opens a gate elsewhere ("powers a connected puzzle
			// element", distinct from the barrier which removes itself).
			ActivationGate brazierGate = BuildActivationGate("BrazierGate", new Vector3(-6f, 1f, 8f), new Vector3(2f, 2f, 0.3f));
			brazier.OnActivated += _ => brazierGate.Open();

			// Water pool -> real blocked/walkable toggle.
			waterPool.gameObject.AddComponent<FreezeWalkway>();

			// Turbine -> raises a connected bridge platform.
			GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
			bridge.name = "TurbineBridge";
			bridge.transform.position = new Vector3(2f, 0.2f, 8f);
			bridge.transform.localScale = new Vector3(2f, 0.4f, 3f);
			bridge.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.55f);
			turbine.gameObject.AddComponent<TurbinePlatform>().SetPlatform(bridge.transform);

			return stations;
		}

		private ElementalTrigger BuildStation(string name, ElementalTriggerKind kind, MagicElement element,
			PrimitiveType primitive, Vector3 position, Vector3 scale, Color initialColor)
		{
			GameObject go = GameObject.CreatePrimitive(primitive);
			go.name = name;
			go.transform.position = position;
			go.transform.localScale = scale;
			go.GetComponent<Renderer>().material.color = initialColor;

			ElementalTrigger trigger = go.AddComponent<ElementalTrigger>();
			trigger.kind = kind;
			trigger.requiredElement = element;
			return trigger;
		}

		private ActivationGate BuildActivationGate(string name, Vector3 position, Vector3 scale)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = name;
			go.transform.position = position;
			go.transform.localScale = scale;
			go.GetComponent<Renderer>().material.color = new Color(0.4f, 0.2f, 0.1f);
			return go.AddComponent<ActivationGate>();
		}

		private void BuildDeployPads()
		{
			BuildPad("Pad_PhlogistonSprinkler", new Vector3(-8f, 0.1f, -5f), new Color(0.8f, 0.45f, 0.35f));
			BuildPad("Pad_TidalConduit", new Vector3(-4f, 0.1f, -5f), new Color(0.1f, 0.4f, 0.8f));
			BuildPad("Pad_MistWeaver", new Vector3(0f, 0.1f, -5f), new Color(0.8f, 0.8f, 0.85f));
			BuildPad("Pad_UmbralDecoy", new Vector3(4f, 0.1f, -5f), new Color(0.25f, 0.1f, 0.35f));
			BuildPad("Pad_PrismTurret", new Vector3(8f, 0.1f, -5f), new Color(0.85f, 0.75f, 0.95f));
		}

		private void BuildPad(string name, Vector3 position, Color color)
		{
			GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
			pad.name = name;
			pad.transform.position = position;
			pad.transform.localScale = new Vector3(2f, 0.2f, 2f);
			pad.GetComponent<Renderer>().material.color = color;
		}

		private TargetDummy BuildTargetDummy()
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			go.name = "TargetDummy";
			go.transform.position = new Vector3(0f, 1f, 9f);
			return go.AddComponent<TargetDummy>();
		}

		private void BuildWorldPuzzlePieces()
		{
			// Sorrel: heavy switch (Cragclaw) -> opens a gate.
			ActivationGate switchGate = BuildActivationGate("HeavySwitchGate", new Vector3(-10f, 1f, 6f), new Vector3(2f, 2f, 0.3f));
			GameObject switchPlateGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			switchPlateGo.name = "HeavySwitchPlate";
			switchPlateGo.transform.position = new Vector3(-10f, 0.05f, 3f);
			switchPlateGo.transform.localScale = new Vector3(2f, 0.05f, 2f);
			switchPlateGo.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.2f);
			switchPlateGo.AddComponent<HeavySwitchPlate>().SetLinkedGate(switchGate);

			// Sorrel: narrow passage (Root-Sprite) -> soft-blocks anything too big.
			GameObject passageGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
			passageGo.name = "NarrowPassageGate";
			passageGo.transform.position = new Vector3(10f, 1f, 3f);
			passageGo.transform.localScale = new Vector3(1f, 2f, 0.5f);
			passageGo.GetComponent<Renderer>().material.color = new Color(0.4f, 0.3f, 0.2f);
			passageGo.AddComponent<NarrowPassageGate>();

			// Sorrel: hazard zone — Drift-Eel flies over it, everyone else takes damage.
			GameObject hazardGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
			hazardGo.name = "HazardZone";
			hazardGo.transform.position = new Vector3(0f, 0.05f, -10f);
			hazardGo.transform.localScale = new Vector3(4f, 0.1f, 4f);
			hazardGo.GetComponent<Renderer>().material.color = new Color(0.9f, 0.3f, 0.1f);
			hazardGo.AddComponent<HazardZone>();

			// Chervil: Prism Turret puzzle target — a mirror redirects the beam
			// to a receiver gate. Exact beam geometry is unverified.
			GameObject mirrorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
			mirrorGo.name = "PrismMirror";
			mirrorGo.transform.position = new Vector3(8f, 1f, 6f);
			mirrorGo.transform.localScale = new Vector3(1.5f, 1.5f, 0.1f);
			mirrorGo.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
			mirrorGo.GetComponent<Renderer>().material.color = new Color(0.8f, 0.9f, 1f);
			mirrorGo.AddComponent<PrismMirror>();

			ActivationGate prismGate = BuildActivationGate("PrismReceiverGate", new Vector3(4f, 1f, 9f), new Vector3(2f, 2f, 0.3f));
			prismGate.gameObject.AddComponent<PrismReceiverGate>();
		}

		private void BuildCharacters(List<ElementalTrigger> stations, TargetDummy targetDummy, CameraFollow cameraFollow)
		{
			var slots = new List<CharacterSlot>();
			var transformsById = new Dictionary<CharacterId, Transform>();

			// One MP pool for the whole avatar, shared by all three models —
			// switching characters never touches this object, so whatever
			// drain/regen was already running keeps running. HP stays
			// independent per model (each gets its own CharacterVitals below).
			GameObject manaPoolGo = new GameObject("SharedManaPool");
			SharedManaPool manaPool = manaPoolGo.AddComponent<SharedManaPool>();

			GameObject sorrelRoot = BuildCharacterBody("Sorrel", new Vector3(-6f, 1f, 0f), new Color(0.85f, 0.55f, 0.2f));
			CharacterMovementController sorrelMovement = sorrelRoot.GetComponent<CharacterMovementController>();
			sorrelRoot.AddComponent<CharacterVitals>();
			SorrelTransformationController sorrelTransformation = BuildSorrelKit(sorrelRoot);
			sorrelTransformation.manaPool = manaPool;
			SorrelTestInput sorrelInput = sorrelRoot.AddComponent<SorrelTestInput>();
			sorrelInput.transformation = sorrelTransformation;
			slots.Add(new CharacterSlot { id = CharacterId.Sorrel, root = sorrelRoot, activeOnlyBehaviours = new List<Behaviour> { sorrelMovement, sorrelInput } });
			transformsById[CharacterId.Sorrel] = sorrelRoot.transform;

			GameObject thymeRoot = BuildCharacterBody("Thyme", new Vector3(0f, 1f, 0f), new Color(0.2f, 0.8f, 0.75f));
			CharacterMovementController thymeMovement = thymeRoot.GetComponent<CharacterMovementController>();
			thymeRoot.AddComponent<CharacterVitals>();
			MeleeComboComponent thymeCombo = thymeRoot.AddComponent<MeleeComboComponent>();
			ThymeComboMPRegen thymeMPRegen = thymeRoot.AddComponent<ThymeComboMPRegen>();
			thymeMPRegen.manaPool = manaPool;
			ThymeTestInput thymeInput = thymeRoot.AddComponent<ThymeTestInput>();
			thymeInput.spellFactory = BuildSpellFactory();
			thymeInput.manaPool = manaPool;
			thymeInput.combo = thymeCombo;
			thymeInput.target = targetDummy;
			thymeInput.elementalStations = stations;
			slots.Add(new CharacterSlot { id = CharacterId.Thyme, root = thymeRoot, activeOnlyBehaviours = new List<Behaviour> { thymeMovement, thymeInput } });
			transformsById[CharacterId.Thyme] = thymeRoot.transform;

			GameObject chervilRoot = BuildCharacterBody("Chervil", new Vector3(6f, 1f, 0f), new Color(0.6f, 0.3f, 0.85f));
			CharacterMovementController chervilMovement = chervilRoot.GetComponent<CharacterMovementController>();
			chervilRoot.AddComponent<CharacterVitals>();
			ThreadInventoryComponent threadInventory = chervilRoot.AddComponent<ThreadInventoryComponent>();
			SeedChervilThreads(threadInventory);
			ChervilResourcePool chervilResourcePool = chervilRoot.AddComponent<ChervilResourcePool>();
			chervilResourcePool.manaPool = manaPool;
			MeleeComboComponent chervilCombo = chervilRoot.AddComponent<MeleeComboComponent>();
			chervilRoot.AddComponent<ChervilComboKineticGenerator>();
			ConstructDeploymentManager constructManager = chervilRoot.AddComponent<ConstructDeploymentManager>();
			constructManager.SetMaxSimultaneousConstructs(0);
			ConfigureConstructs(constructManager);
			SubscribeConstructLogging(constructManager);
			SubscribeConstructVisualExtras(constructManager);
			ChervilTestInput chervilInput = chervilRoot.AddComponent<ChervilTestInput>();
			chervilInput.constructManager = constructManager;
			chervilInput.combo = chervilCombo;
			chervilInput.target = targetDummy;
			slots.Add(new CharacterSlot { id = CharacterId.Chervil, root = chervilRoot, activeOnlyBehaviours = new List<Behaviour> { chervilMovement, chervilInput } });
			transformsById[CharacterId.Chervil] = chervilRoot.transform;

			GameObject switchGo = new GameObject("CharacterSwitchController");
			CharacterSwitchController switchController = switchGo.AddComponent<CharacterSwitchController>();
			switchController.Initialize(slots, CharacterId.Chervil);
			switchController.OnActiveCharacterChanged += id => Debug.Log($"[Switch] Active character: {id}");

			CharacterSwitchInput switchInput = switchGo.AddComponent<CharacterSwitchInput>();
			switchInput.controller = switchController;

			cameraFollow?.Initialize(switchController, transformsById);
		}

		private GameObject BuildCharacterBody(string name, Vector3 position, Color color)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			go.name = name;
			go.transform.position = position;
			go.GetComponent<Renderer>().material.color = color;
			// The default CapsuleCollider would fight the CharacterController
			// the movement controller adds below — CharacterController is the
			// sole collision volume for a moving character.
			Object.Destroy(go.GetComponent<Collider>());
			go.AddComponent<CharacterMovementController>();
			return go;
		}

		// All 18 forms from player-mechanics.pdf Section 1, across the 4
		// families. formId/displayName/family and the flavor text
		// (statsSummary/primaryAttack/specialUtility/fieldTacticalUse) are
		// the PDF's exact canonical values. The PDF only gives qualitative
		// tiers (e.g. "High Speed / High Jump / Low Def") rather than
		// numbers, so the numeric movement/collision/resource fields below
		// are a design-liberty translation of those tiers onto this
		// component's existing fields, following one consistent scale:
		// speed/jump Low~0.6-0.8, Medium~1.0, High~1.4-1.7; weight Very
		// Small~5 up to Massive~400 (Colossus); MP drain/ability cost scale
		// roughly with a form's overall power. No "Def"/"HP"/"Crit" field
		// exists yet (no per-form combat stat system built), so those
		// qualifiers live only in statsSummary for now, not a numeric field.
		private SorrelTransformationController BuildSorrelKit(GameObject sorrelRoot)
		{
			var baseForm = ScriptableObject.CreateInstance<MonsterFormDefinition>();
			baseForm.formId = "Base";
			baseForm.displayName = "Sorrel (Base Form)";
			baseForm.gravityScale = 1f;
			baseForm.jumpHeight = 1.2f;
			baseForm.moveSpeedMultiplier = 1f;
			baseForm.controllerHeight = 1.8f;
			baseForm.controllerRadius = 0.4f;
			baseForm.bodyWeight = 55f;
			baseForm.bodySize = 1f;

			// Fang & Wing — fast physical attackers and aerial specialists.
			MonsterFormDefinition fangstrider = MakeForm(SorrelTestInput.FangstriderId, "Fangstrider", "Fang & Wing",
				"High Speed / High Jump / Low Def", "Claw Slash", "Pounce Dash", "Rapid ground traversal across open areas",
				1.6f, 1.5f, 1f, false, 1.6f, 0.5f, 40f, 1.1f, 4f, 15f, "Ability.PounceDash");
			MonsterFormDefinition driftEel = MakeForm(SorrelTestInput.DriftEelId, "Drift-Eel", "Fang & Wing",
				"High Speed / Continuous Hover / Low HP", "Needle Spit", "Venom Prick", "Hovers over water, lava, and chasms",
				1.5f, 1f, 0.3f, true, 0.7f, 0.35f, 8f, 0.5f, 3.5f, 12f, "Ability.VenomPrick");
			MonsterFormDefinition shellclaw = MakeForm("Shellclaw", "Shellclaw", "Fang & Wing",
				"Low Speed / High Def / Med Weight", "Pincer Snap", "Toxin Tail", "Frontal shell auto-blocks light projectiles",
				0.6f, 0.8f, 1.1f, false, 1.3f, 0.6f, 70f, 1f, 3f, 12f, "Ability.ToxinTail");
			MonsterFormDefinition stonegaze = MakeForm("Stonegaze", "Stonegaze", "Fang & Wing",
				"Medium Speed / High Def", "Tail Swipe", "Petrify Beam", "Petrified foes become temporary platforms",
				1f, 1f, 1.1f, false, 1.6f, 0.55f, 90f, 1.1f, 4f, 16f, "Ability.PetrifyBeam");

			// Hollow & Wisp — magical, ranged, or hazard-immune forms.
			MonsterFormDefinition rattlebone = MakeForm("Rattlebone", "Rattlebone", "Hollow & Wisp",
				"Medium Speed / Low Def", "Bone Slash", "Bone Arc", "Reassembles once after non-holy lethal damage (costs MP)",
				1f, 1.1f, 1f, false, 1.7f, 0.4f, 35f, 1f, 4f, 20f, "Ability.BoneArc");
			MonsterFormDefinition wisp = MakeForm("Wisp", "Wisp", "Hollow & Wisp",
				"Floating / Low HP / Trap-immune", "Dark Bolt", "Phase Dash", "Immune to spikes, lava, and acid while floating",
				1.1f, 1f, 0.25f, true, 0.8f, 0.3f, 5f, 0.5f, 3f, 12f, "Ability.PhaseDash");
			MonsterFormDefinition cinderskull = MakeForm("Cinderskull", "Cinderskull", "Hollow & Wisp",
				"Slow / Low HP / High MP Efficiency", "Arcane Orb", "Blink Step", "High-damage ranged spam without burning much MP",
				0.7f, 1f, 1f, false, 1.3f, 0.4f, 20f, 0.7f, 2f, 8f, "Ability.BlinkStep");
			MonsterFormDefinition duskfang = MakeForm("Duskfang", "Duskfang", "Hollow & Wisp",
				"Fast / High Crit Rate", "Shadow Slash", "Somnus Wave", "High crit; AoE sleep disables fast groups",
				1.6f, 1.2f, 1f, false, 1.7f, 0.45f, 45f, 1f, 4f, 18f, "Ability.SomnusWave");
			MonsterFormDefinition hexweaver = MakeForm("Hexweaver", "Hexweaver", "Hollow & Wisp",
				"Slow / Balanced Stats", "Staff Strike", "Blight Mist", "AoE atk/def down — softens targets pre-boss",
				0.8f, 1f, 1f, false, 1.8f, 0.4f, 50f, 1f, 4f, 16f, "Ability.BlightMist");

			// Stone & Iron — high defense and mass, for heavy switches and barriers.
			MonsterFormDefinition cragclaw = MakeForm(SorrelTestInput.CragclawId, "Cragclaw", "Stone & Iron",
				"Very Heavy / Slow / High HP+Def", "Rock Punch", "Boulder Toss", "Weight triggers large pressure plates/switches",
				0.6f, 0.5f, 1.5f, false, 2.2f, 1f, 200f, 2f, 5f, 20f, "Ability.BoulderToss");
			MonsterFormDefinition bruteknuckle = MakeForm("Bruteknuckle", "Bruteknuckle", "Stone & Iron",
				"Heavy / Slow / High Atk", "Club Smash", "Quake Slam", "Interrupts enemy attacks; breaks cracked walls",
				0.7f, 0.6f, 1.3f, false, 2f, 0.85f, 150f, 1.7f, 5f, 20f, "Ability.QuakeSlam");
			MonsterFormDefinition bastion = MakeForm("Bastion", "Bastion", "Stone & Iron",
				"Heavy / Slow / Frontal Invuln", "Lance Thrust", "Aegis Guard", "Walks through frontal projectile traps unharmed",
				0.65f, 0.5f, 1.3f, false, 2f, 0.85f, 160f, 1.7f, 4.5f, 18f, "Ability.AegisGuard");
			MonsterFormDefinition glidewing = MakeForm("Glidewing", "Glidewing", "Stone & Iron",
				"Medium Speed / High Jump / Glide", "Stone Strike", "Glide Jump", "Reaches distant high platforms, crosses wide gaps",
				1f, 1.6f, 0.7f, false, 1.7f, 0.5f, 60f, 1.1f, 3.5f, 14f, "Ability.GlideJump");

			// Root & Thread — niche forms for utility, status effects, or tiny spaces.
			MonsterFormDefinition rootSprite = MakeForm(SorrelTestInput.RootSpriteId, "Root-Sprite", "Root & Thread",
				"Very Small / Low Speed / Very Low HP", "Root Swipe", "Spore Veil", "Tiny hitbox fits small tunnels, pipes, wall gaps",
				0.8f, 0.8f, 1f, false, 0.5f, 0.2f, 5f, 0.25f, 3f, 10f, "Ability.SporeVeil");
			MonsterFormDefinition puffcap = MakeForm("Puffcap", "Puffcap", "Root & Thread",
				"Slow / Medium HP", "Body Tackle", "Slumber Spore", "AoE sleep pauses aggressive mob groups instantly",
				0.7f, 0.8f, 1f, false, 1f, 0.5f, 25f, 0.7f, 3.5f, 16f, "Ability.SlumberSpore");
			MonsterFormDefinition emberwisp = MakeForm("Emberwisp", "Emberwisp", "Root & Thread",
				"Floating / Low HP", "Ember Toss", "Flame Touch", "Lights braziers/torches for puzzle rooms",
				1f, 1f, 0.3f, true, 0.6f, 0.25f, 5f, 0.4f, 3f, 10f, "Ability.FlameTouch");
			MonsterFormDefinition thornpuppet = MakeForm("Thornpuppet", "Thornpuppet", "Root & Thread",
				"Small Size / Fast Attack Speed", "String Blade", "Spin Slash", "Compact hitbox, high combo-hit count",
				1.3f, 1.1f, 1f, false, 0.9f, 0.3f, 15f, 0.4f, 3.5f, 12f, "Ability.SpinSlash");
			// Boss-drop: not found in the world, but fills a coin-queue slot
			// like any other once obtained (HANDOFF.md decision — no
			// monster-defeat system exists yet to grant it that way, so it's
			// registered as data here but not pre-seeded into the queue below).
			MonsterFormDefinition colossus = MakeForm("Colossus", "Colossus", "Root & Thread",
				"Massive Weight / Extreme Atk / Slow", "Hammer Smash", "Quakeburst", "Breaks reinforced environmental barriers",
				0.5f, 0.4f, 1.8f, false, 3f, 1.4f, 400f, 3f, 7f, 30f, "Ability.Quakeburst");

			SorrelTransformationController transformation = sorrelRoot.AddComponent<SorrelTransformationController>();
			transformation.Configure(baseForm, new[]
			{
				fangstrider, driftEel, shellclaw, stonegaze,
				rattlebone, wisp, cinderskull, duskfang, hexweaver,
				cragclaw, bruteknuckle, bastion, glidewing,
				rootSprite, puffcap, emberwisp, thornpuppet, colossus,
			});

			// No monster-defeat/pickup system exists yet, so pre-seed coins
			// for the 4 keyboard-testable forms (fills the 5-slot FIFO queue
			// to 4/5, leaving room to demonstrate a 5th pickup or eviction
			// later). Mirrors how Chervil starts pre-seeded with threads.
			transformation.Coins.Collect(SorrelTestInput.FangstriderId);
			transformation.Coins.Collect(SorrelTestInput.DriftEelId);
			transformation.Coins.Collect(SorrelTestInput.CragclawId);
			transformation.Coins.Collect(SorrelTestInput.RootSpriteId);

			return transformation;
		}

		private static MonsterFormDefinition MakeForm(string formId, string displayName, string family,
			string statsSummary, string primaryAttack, string specialUtility, string fieldTacticalUse,
			float moveSpeedMultiplier, float jumpHeight, float gravityScale, bool canFly,
			float controllerHeight, float controllerRadius, float bodyWeight, float bodySize,
			float transformMPDrainPerSecond, float abilityMPCost, string abilityId)
		{
			var form = ScriptableObject.CreateInstance<MonsterFormDefinition>();
			form.formId = formId;
			form.displayName = displayName;
			form.family = family;
			form.statsSummary = statsSummary;
			form.primaryAttack = primaryAttack;
			form.specialUtility = specialUtility;
			form.fieldTacticalUse = fieldTacticalUse;
			form.moveSpeedMultiplier = moveSpeedMultiplier;
			form.jumpHeight = jumpHeight;
			form.gravityScale = gravityScale;
			form.canFly = canFly;
			form.controllerHeight = controllerHeight;
			form.controllerRadius = controllerRadius;
			form.bodyWeight = bodyWeight;
			form.bodySize = bodySize;
			form.transformMPDrainPerSecond = transformMPDrainPerSecond;
			form.abilityMPCost = abilityMPCost;
			form.abilityId = abilityId;
			return form;
		}

		// All 35 spells from player-mechanics.pdf Section 2. Element/CastShape,
		// spellName, description, and mpCost (or minimumMPToCast for the
		// consumesAllMP Overflow-tier spells) are the PDF's exact canonical
		// values. effectType/effectMagnitude/abilityId are design liberties —
		// the PDF doesn't specify numeric combat magnitudes, only mechanics
		// text, so magnitudes are illustrative and pure-utility spells (no
		// direct HP damage/heal — shields, invulnerability, world-interaction
		// only) are given magnitude 0, which EffectResolver already treats as
		// a no-op.
		private SpellCombinationFactory BuildSpellFactory()
		{
			var spellFactory = new SpellCombinationFactory();
			spellFactory.Initialize(new[]
			{
				// Lumen — light, precision projectiles, unlocked from the start.
				MakeSpell(MagicElement.Lumen, "Bolt", "Glintstream", "Rapid-fire light particles while held", 1f, EffectType.Damage, 3f, "Spell.Glintstream"),
				MakeSpell(MagicElement.Lumen, "Arc", "Starfan", "5-way arc of light particles", 4f, EffectType.Damage, 10f, "Spell.Starfan"),
				MakeSpell(MagicElement.Lumen, "Surge", "Piercer", "Fast, lance-like piercing shot", 4f, EffectType.Damage, 12f, "Spell.Piercer"),
				MakeSpell(MagicElement.Lumen, "Ring", "Twin Wisp", "Two orbiting crystals that shoot nearby foes", 10f, EffectType.Damage, 8f, "Spell.TwinWisp"),

				// Tide — water/ice, crowd control and healing, unlocked from the start.
				MakeSpell(MagicElement.Tide, "Bolt", "Dewshot", "Rapid water bubbles fired forward", 2f, EffectType.Damage, 5f, "Spell.Dewshot"),
				MakeSpell(MagicElement.Tide, "Arc", "Ripfin", "3 water-blade crescents in a wide fan", 4f, EffectType.Damage, 11f, "Spell.Ripfin"),
				MakeSpell(MagicElement.Tide, "Surge", "Undertow", "Expanding water rings with heavy knockback", 4f, EffectType.Damage, 10f, "Spell.Undertow"),
				MakeSpell(MagicElement.Tide, "Ring", "Glacia Ward", "Spiked ice shield directly in front", 6f, EffectType.Damage, 0f, "Spell.GlaciaWard"),
				MakeSpell(MagicElement.Tide, "Cascade", "Icefall", "Falling icicles; hold to drop more", 4f, EffectType.Damage, 9f, "Spell.Icefall"),
				MakeSpell(MagicElement.Tide, "Overflow", "Mercy Tide", "Drains full MP bar; restores full HP", 80f, EffectType.Heal, 9999f, "Spell.MercyTide", consumesAllMP: true),

				// Ember — fire, high offense output.
				MakeSpell(MagicElement.Ember, "Bolt", "Cinderstream", "Flamethrower stream; melts ice, lights braziers", 2f, EffectType.Damage, 6f, "Spell.Cinderstream"),
				MakeSpell(MagicElement.Ember, "Arc", "Emberlob", "Fireballs that linger briefly on the ground", 4f, EffectType.Damage, 11f, "Spell.Emberlob"),
				MakeSpell(MagicElement.Ember, "Surge", "Flareburst", "Growing fireball dealing multiple hits", 6f, EffectType.Damage, 16f, "Spell.Flareburst"),
				MakeSpell(MagicElement.Ember, "Ring", "Triflame", "3 fireballs swirling forward in a drill formation", 6f, EffectType.Damage, 15f, "Spell.Triflame"),
				MakeSpell(MagicElement.Ember, "Cascade", "Firewash", "Arc of ground fire in front of the caster", 6f, EffectType.Damage, 14f, "Spell.Firewash"),
				MakeSpell(MagicElement.Ember, "Overflow", "Cinderheart", "Fiery barrier cloak, damages foes on contact", 80f, EffectType.Damage, 20f, "Spell.Cinderheart", consumesAllMP: true),

				// Gale — wind, fast, low-cost projectiles.
				MakeSpell(MagicElement.Gale, "Bolt", "Windrip", "Fast, consecutive wind-blade projectiles", 3f, EffectType.Damage, 8f, "Spell.Windrip"),
				MakeSpell(MagicElement.Gale, "Arc", "Turbine Gust", "Cone gust; activates windmill mechanisms", 2f, EffectType.Damage, 0f, "Spell.TurbineGust"),
				MakeSpell(MagicElement.Gale, "Surge", "Lowblade", "Triangular wind gusts traveling low along the ground", 3f, EffectType.Damage, 9f, "Spell.Lowblade"),
				MakeSpell(MagicElement.Gale, "Ring", "Updraft Column", "Vertically rising wind column near the caster", 3f, EffectType.Damage, 0f, "Spell.UpdraftColumn"),
				MakeSpell(MagicElement.Gale, "Cascade", "Impulse Wave", "Outward 360-degree shockwave ring", 6f, EffectType.Damage, 17f, "Spell.ImpulseWave"),
				MakeSpell(MagicElement.Gale, "Overflow", "Halcyon Veil", "Complete invulnerability to all damage while active", 80f, EffectType.Damage, 0f, "Spell.HalcyonVeil", consumesAllMP: true),

				// Umbra — gravity/dark, heavy, crushing damage.
				MakeSpell(MagicElement.Umbra, "Bolt", "Gravity Bomb", "Bouncing bomb; shatters small boulders", 5f, EffectType.Damage, 13f, "Spell.GravityBomb"),
				MakeSpell(MagicElement.Umbra, "Arc", "Shroud Arc", "Arc of poisonous smoke", 6f, EffectType.Damage, 12f, "Spell.ShroudArc"),
				MakeSpell(MagicElement.Umbra, "Surge", "Graviton Well", "Heavy, expanding dark orb; breaks giant obstacles", 6f, EffectType.Damage, 18f, "Spell.GravitonWell"),
				MakeSpell(MagicElement.Umbra, "Ring", "Umbral Drill", "Short-range, high-damage drill attack", 6f, EffectType.Damage, 20f, "Spell.UmbralDrill"),
				MakeSpell(MagicElement.Umbra, "Cascade", "Umbral Burst", "Massive localized explosive blast around the caster", 10f, EffectType.Damage, 28f, "Spell.UmbralBurst"),
				MakeSpell(MagicElement.Umbra, "Overflow", "Voidrush", "Forward invulnerable dash that pierces and damages", 50f, EffectType.Damage, 40f, "Spell.Voidrush"),

				// Volt — lightning, homing and wide AoE.
				MakeSpell(MagicElement.Volt, "Bolt", "Spark", "Homing electric bolt that tracks targets", 5f, EffectType.Damage, 12f, "Spell.Spark"),
				MakeSpell(MagicElement.Volt, "Arc", "Twinbolt Row", "Double row of lightning bursts", 6f, EffectType.Damage, 16f, "Spell.TwinboltRow"),
				MakeSpell(MagicElement.Volt, "Surge", "Skypillar", "Lightning pillar striking from above", 10f, EffectType.Damage, 22f, "Spell.Skypillar"),
				MakeSpell(MagicElement.Volt, "Ring", "Voltring", "Ring of lightning orbiting at medium range", 8f, EffectType.Damage, 18f, "Spell.Voltring"),
				MakeSpell(MagicElement.Volt, "Cascade", "Tri-Strike", "Three tracking bolts targeting multiple foes", 8f, EffectType.Damage, 19f, "Spell.TriStrike"),
				MakeSpell(MagicElement.Volt, "Overflow", "Storm's End", "Screen-clearing lightning burst radiating outward", 100f, EffectType.Damage, 45f, "Spell.StormsEnd", consumesAllMP: true),

				// Aurum — ultimate-tier, unlocked near the climax. Only one
				// Cast Shape is defined for this element in the source spec.
				MakeSpell(MagicElement.Aurum, "Zenith", "Cosmic Ward", "Field drains MP instead of HP when hit; release for a multi-hit energy blast", 10f, EffectType.Damage, 20f, "Spell.CosmicWard"),
			});
			return spellFactory;
		}

		private void SeedChervilThreads(ThreadInventoryComponent inventory)
		{
			inventory.AddThreads(ThreadElement.Fire, 3);
			inventory.AddThreads(ThreadElement.Water, 4);
			inventory.AddThreads(ThreadElement.Wind, 2);
			inventory.AddThreads(ThreadElement.Lightning, 2);
			inventory.AddThreads(ThreadElement.Iron, 3);
			inventory.AddThreads(ThreadElement.Phantom, 4);
			inventory.AddThreads(ThreadElement.Crystal, 4);
			// Beast isn't needed by any of the 5 keyboard-exposed constructs,
			// only by Blade-Beast Sentry/Gale Spring (data-registered but not
			// deploy-pad/key wired — see ChervilTestInput) — seeded anyway so
			// DeployConstruct(BladeBeastSentryId/GaleSpringId) works if called directly.
			inventory.AddThreads(ThreadElement.Beast, 2);
		}

		// All 11 constructs from the user-approved roster revision (2026-08-07),
		// which replaces the earlier player-mechanics.pdf-sourced 10-construct
		// roster entirely. constructId/displayName/combatRole/fieldFunction,
		// thread recipes, deployMPCost, maintenanceMPPerSecond, isMobile, and
		// the OC/DC ability names+descriptions are the roster table's exact
		// values.
		//
		// Two real mechanical differences from the prior roster, both
		// implemented (not just relabeled): (1) every construct now has a
		// flat deployMPCost on top of its thread cost — see
		// ConstructDeploymentManager.DeployConstruct; (2) only the 4 Mobile
		// constructs (Blade-Beast Sentry, Firefly, Aegis Crawler, Umbral
		// Decoy) have continuous MP/sec upkeep — the 7 Immobile ones are a
		// flat one-time cost with none. isMobile is otherwise inert (no
		// construct movement/AI system exists to act on it yet).
		//
		// The roster gives every Overclock a single flat MP cost with no
		// Kinetic Energy figure at all, unlike the prior roster — rather than
		// ripping out the Kinetic Energy subsystem (ChervilResourcePool,
		// ChervilComboKineticGenerator), overclockKineticEnergyCost is left
		// at 0 here, which ConsumeKineticEnergy(0) trivially satisfies, so
		// Overclock ends up MP-gated only, matching the table, without
		// deleting a subsystem nothing asked to remove.
		//
		// overclockEffectType/effectMagnitude are still a design liberty
		// (0 for CC/utility Overclocks with no direct HP effect — Kinetic
		// Fortress's shield, Giga Volt Discharge's stun, Flash Freeze —
		// matching EffectResolver's existing no-op-at-0 behavior). Bespoke
		// unique mechanics with no supporting system yet (stealth, phase-dash,
		// invisibility, magnetism, partial single-thread refunds, HP<->MP
		// conversion) are captured in overclockDescription/deconstructDescription
		// only, not partially faked into existing numeric fields — same
		// discipline as the rest of this file's construct/spell/form data.
		private void ConfigureConstructs(ConstructDeploymentManager constructManager)
		{
			ConstructDefinition sentry = MakeConstruct(ChervilTestInput.BladeBeastSentryId, "Blade-Beast Sentry", "Flanker / Mobile Utility",
				"Mobile grapple point for whip traversal; patrols and slashes.", true,
				new[] { new ThreadCost { element = ThreadElement.Beast, count = 1 }, new ThreadCost { element = ThreadElement.Iron, count = 1 } },
				10f, 1f, 25f, EffectType.Damage, 16f, "KineticRazor",
				"Kinetic Razor — spins chassis into a circular blade, dealing continuous damage & shielding against light projectiles; environmentally acts as a rotary saw to cut vines, wooden barricades, or ropes.",
				"ChainReel", "Chain Reel — converts frame into a tether pulling surrounding light enemies inward toward Chervil; environmentally acts as a firm anchor yanking Chervil forward rapidly across gaps.",
				new Color(0.45f, 0.55f, 0.3f));

			ConstructDefinition firefly = MakeConstruct(ChervilTestInput.FireflyId, "Firefly", "Illumination / Debuffer",
				"Thermal Beacon — localized warm light ring counteracting freeze buildup & attracting light-sensitive fauna/insects; ignites braziers, melts ice, & illuminates dark areas.", true,
				new[] { new ThreadCost { element = ThreadElement.Fire, count = 1 }, new ThreadCost { element = ThreadElement.Beast, count = 1 } },
				10f, 1f, 30f, EffectType.Damage, 20f, "IgnitionSwarm",
				"Ignition Swarm — splits into ember drones locking onto up to 5 targets, detonating to apply stacking Burn; environmentally lights multiple distant braziers or fuses with multi-target timing.",
				"FlareFlash", "Flare Flash — screen-wide stun/confusion + permanently reveals hidden illusion walls/secrets.",
				new Color(0.95f, 0.75f, 0.2f));

			ConstructDefinition crawler = MakeConstruct(ChervilTestInput.AegisCrawlerId, "Aegis Crawler", "Mobile Cover / Frontline Tank",
				"Slow-moving mobile hard cover; blocks projectiles and carries a climbing platform across hazard paths.", true,
				new[] { new ThreadCost { element = ThreadElement.Iron, count = 1 }, new ThreadCost { element = ThreadElement.Wind, count = 1 } },
				15f, 1f, 30f, EffectType.Damage, 0f, "KineticFortress",
				"Kinetic Fortress — roots in place, deploys a 180° hard-energy shield absorbing frontal attacks for 5s & converting damage to MP; environmentally acts as a hydraulic jack to lift crushed ruins or trap gates.",
				"FortressBarrier", "Fortress Barrier — leaves an indestructible energy wall for 8s for safe cover; environmentally leaves behind a solid metal stepping block platform.",
				new Color(0.55f, 0.6f, 0.55f));

			ConstructDefinition decoy = MakeConstruct(ChervilTestInput.DecoyId, "Umbral Decoy", "Aggro Redirector / Stealth",
				"Bypasses optical sensors; draws enemy aggro.", true,
				new[] { new ThreadCost { element = ThreadElement.Phantom, count = 1 }, new ThreadCost { element = ThreadElement.Wind, count = 1 } },
				20f, 2f, 25f, EffectType.Damage, 18f, "EchoSwitch",
				"Echo Switch — swaps positions with Chervil, triggering dark explosions that cause heavy stagger at both spots; environmentally bypasses single-way gates or forcefields by swapping places across slits.",
				"ShadowSlip", "Shadow Slip — collapses into a shadow pool granting Chervil 3s invulnerability & phase-dash; environmentally lets Chervil submerge to pass through iron bars, laser grids, or gate gaps.",
				new Color(0.25f, 0.1f, 0.35f));

			ConstructDefinition spring = MakeConstruct(ChervilTestInput.GaleSpringId, "Gale Spring", "Crowd Launcher / Mobility",
				"Directional Vent — can be hit/rotated to angle airflow horizontally or diagonally; acts as a vertical or lateral wind tunnel.", false,
				new[] { new ThreadCost { element = ThreadElement.Beast, count = 1 }, new ThreadCost { element = ThreadElement.Wind, count = 1 } },
				15f, 0f, 30f, EffectType.Damage, 14f, "VacuumCollapse",
				"Vacuum Collapse — reverses airflow for 4s, pulling screen-wide foes into its core before blasting them up; environmentally sucks in distant loose puzzle items, keys, or crates across gaps.",
				"RepulsionBlast", "Repulsion Blast — horizontal shockwave blasting foes into walls/pits; environmentally deflects incoming projectile salvos or blows away hazardous gas clouds.",
				new Color(0.55f, 0.85f, 0.65f));

			ConstructDefinition coil = MakeConstruct(ChervilTestInput.VoltaicCoilId, "Voltaic Coil", "Single-Target Stunner",
				"Persistent Tesla pillar; bridges broken power circuits and chains electricity.", false,
				new[] { new ThreadCost { element = ThreadElement.Iron, count = 1 }, new ThreadCost { element = ThreadElement.Lightning, count = 1 } },
				20f, 0f, 40f, EffectType.Damage, 0f, "GigaVoltDischarge",
				"Giga Volt Discharge — screen-wide stun.",
				"MagnetizePulse", "Magnetize Pulse — magnetic implosion pulling metal-armored foes or airborne drones to the Coil's location; environmentally attracts out-of-reach metal blocks, levers, or items.",
				new Color(0.9f, 0.85f, 0.25f));

			ConstructDefinition turret = MakeConstruct(ChervilTestInput.TurretId, "Prism Turret", "Sniper / Optics",
				"Persistent laser anchor; refracts light/lasers off dungeon mirrors.", false,
				new[] { new ThreadCost { element = ThreadElement.Crystal, count = 1 }, new ThreadCost { element = ThreadElement.Lightning, count = 1 } },
				20f, 0f, 30f, EffectType.Damage, 28f, "FocusBeam",
				"Focus Beam — channels a single high-density laser beam piercing foes & melting heavy armor; environmentally warms/cuts crystalline walls, obsidian, or welds levers.",
				"RefractiveCloak", "Refractive Cloak — shatters into optical crystal dust rendering Chervil invisible to enemy vision for 6s; environmentally lets Chervil act as a portable refraction point for optical puzzles.",
				new Color(0.85f, 0.75f, 0.95f));

			ConstructDefinition conduit = MakeConstruct(ChervilTestInput.ConduitId, "Tidal Conduit", "Fluid Control / Pusher",
				"Persistent water pump; fills dry basins and extinguishes fire traps.", false,
				new[] { new ThreadCost { element = ThreadElement.Water, count = 1 }, new ThreadCost { element = ThreadElement.Iron, count = 1 } },
				20f, 0f, 30f, EffectType.Damage, 16f, "HighPressureJet",
				"High-Pressure Jet — targeted water blast pushing foes back, interrupting casts, and washing away acid/poison; environmentally powers waterwheels, fills high reservoirs, and washes away mud/silt.",
				"AquaticCleansing", "Aquatic Cleansing — purifies status ailments.",
				new Color(0.1f, 0.5f, 0.9f));

			ConstructDefinition weaver = MakeConstruct(ChervilTestInput.WeaverId, "Mist Weaver", "Zone Controller / Support",
				"Persistent mist emitter; slows enemy speed by 40% and cools heat hazards.", false,
				new[] { new ThreadCost { element = ThreadElement.Water, count = 1 }, new ThreadCost { element = ThreadElement.Wind, count = 1 } },
				10f, 0f, 30f, EffectType.Damage, 0f, "FlashFreeze",
				"Flash Freeze — freezes nearby enemies in ice.",
				"DewDrop", "Dew Drop — heals 25% max HP.",
				new Color(0.85f, 0.9f, 0.95f));

			ConstructDefinition ram = MakeConstruct(ChervilTestInput.SteamEngineRamId, "Steam Engine Ram", "Demolition / Heavy Thrust",
				"Persistent heavy piston; shatters reinforced stone barriers, launches push-blocks, and vents hot steam.", false,
				new[] { new ThreadCost { element = ThreadElement.Iron, count = 1 }, new ThreadCost { element = ThreadElement.Fire, count = 1 }, new ThreadCost { element = ThreadElement.Water, count = 1 } },
				30f, 0f, 30f, EffectType.Damage, 26f, "PressureExplosion",
				"Pressure Explosion — triple piston blast + scalding steam cloud.",
				"VentAndRecoil", "Vent & Recoil — directional escape blast + refunds 1 Iron Thread.",
				new Color(0.55f, 0.3f, 0.2f));

			ConstructDefinition sprinkler = MakeConstruct(ChervilTestInput.SprinklerId, "Phlogiston Sprinkler", "Zone Debuffer / Amplifier",
				"Emits flammable mist; applies Fire Vulnerability to foes; coats conduits for chain ignition.", false,
				new[] { new ThreadCost { element = ThreadElement.Fire, count = 1 }, new ThreadCost { element = ThreadElement.Water, count = 1 } },
				15f, 0f, 30f, EffectType.Damage, 24f, "IgnitionFlare",
				"Ignition Flare — room-wide flash fire, detonates mist for heavy Fire damage.",
				"ScaldingFog", "Scalding Fog — pushes foes back + creates obscuring fog cover.",
				new Color(0.8f, 0.45f, 0.35f));

			constructManager.RegisterConstructs(new[]
			{
				sentry, firefly, crawler, decoy, spring, coil, turret, conduit, weaver, ram, sprinkler,
			});
		}

		private static ConstructDefinition MakeConstruct(string constructId, string displayName, string combatRole,
			string fieldFunction, bool isMobile, ThreadCost[] requiredThreads, float deployMPCost, float maintenanceMPPerSecond,
			float overclockMPCost, EffectType overclockEffectType, float overclockEffectMagnitude,
			string overclockAbilityId, string overclockDescription,
			string deconstructUtilityId, string deconstructDescription, Color placeholderColor)
		{
			var construct = ScriptableObject.CreateInstance<ConstructDefinition>();
			construct.constructId = constructId;
			construct.displayName = displayName;
			construct.combatRole = combatRole;
			construct.fieldFunction = fieldFunction;
			construct.isMobile = isMobile;
			construct.requiredThreads = new List<ThreadCost>(requiredThreads);
			construct.deployMPCost = deployMPCost;
			construct.maintenanceMPPerSecond = maintenanceMPPerSecond;
			construct.overclockKineticEnergyCost = 0f;
			construct.overclockMPCost = overclockMPCost;
			construct.overclockEffectType = overclockEffectType;
			construct.overclockEffectMagnitude = overclockEffectMagnitude;
			construct.overclockAbilityId = overclockAbilityId;
			construct.overclockDescription = overclockDescription;
			construct.deconstructUtilityId = deconstructUtilityId;
			construct.deconstructDescription = deconstructDescription;
			construct.placeholderColor = placeholderColor;
			return construct;
		}

		private void SubscribeConstructLogging(ConstructDeploymentManager constructManager)
		{
			constructManager.OnConstructDeployed += id => Debug.Log($"[Chervil] Deployed {id}.");
			constructManager.OnConstructDeconstructed += (id, forced) => Debug.Log($"[Chervil] Deconstructed {id} (forced={forced}).");
			constructManager.OnConstructOverclocked += (id, ability) => Debug.Log($"[Chervil] {id} Overclock -> {ability}.");
		}

		// Construct-specific extras that don't belong in the generic
		// ConstructDeploymentManager: attaches AggroDecoy/PrismTurretBeam to
		// a construct's spawned visual right after it deploys.
		private void SubscribeConstructVisualExtras(ConstructDeploymentManager constructManager)
		{
			constructManager.OnConstructDeployed += id =>
			{
				if (!constructManager.TryGetActiveSpawnedActor(id, out GameObject spawned) || spawned == null)
				{
					return;
				}

				if (id == ChervilTestInput.DecoyId)
				{
					spawned.AddComponent<AggroDecoy>();
				}
				else if (id == ChervilTestInput.TurretId)
				{
					spawned.AddComponent<PrismTurretBeam>();
				}
			};
		}

		// costOrMinimumMP is mpCost normally, or minimumMPToCast when
		// consumesAllMP is set (Overflow-tier spells drain the whole bar).
		private static SpellDefinition MakeSpell(MagicElement element, string castShape, string spellName, string description,
			float costOrMinimumMP, EffectType effectType, float effectMagnitude, string abilityId, bool consumesAllMP = false)
		{
			var spell = ScriptableObject.CreateInstance<SpellDefinition>();
			spell.element = element;
			spell.castShape = castShape;
			spell.spellName = spellName;
			spell.description = description;
			spell.consumesAllMP = consumesAllMP;
			if (consumesAllMP)
			{
				spell.minimumMPToCast = costOrMinimumMP;
			}
			else
			{
				spell.mpCost = costOrMinimumMP;
			}
			spell.effectType = effectType;
			spell.effectMagnitude = effectMagnitude;
			spell.abilityId = abilityId;
			return spell;
		}
	}
}
