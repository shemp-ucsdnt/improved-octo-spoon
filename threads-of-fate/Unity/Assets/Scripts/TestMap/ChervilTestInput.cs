using UnityEngine;
using ThreadsOfFate.Core;
using ThreadsOfFate.Chervil;

namespace ThreadsOfFate.TestMap
{
	// Chervil's slice of the keyboard test harness (WASD/Space are reserved
	// for movement while Chervil is active, so these avoid that block). All
	// 11 constructs from the user-approved roster revision exist as data
	// (see TestMapBuilder.ConfigureConstructs), but only 5 are wired to
	// keys/deploy pads here — the other 6 (Blade-Beast Sentry, Firefly,
	// Aegis Crawler, Gale Spring, Voltaic Coil, Steam Engine Ram) are
	// registered and usable via DeployConstruct(id) but not
	// keyboard-exposed, same "data complete, harness partial" split as
	// Sorrel's 18 forms/4 keys and Thyme's 35 spells/8 key-combos:
	//   E/R/T/Y/U = deploy Sprinkler/Conduit/Weaver/Decoy/Turret
	//   G/H/J/N/M = Overclock Sprinkler/Conduit/Weaver/Decoy/Turret
	//   Z/X/C/V/B = Deconstruct Sprinkler/Conduit/Weaver/Decoy/Turret
	//   Left Mouse = whip-dagger combo hit (generates Kinetic Energy via ChervilComboKineticGenerator)
	//
	// Disabled by default; CharacterSwitchController enables this only while
	// Chervil is the active character. MP costs (deploy + Overclock) are
	// enforced by ConstructDeploymentManager itself via Chervil's own
	// ChervilResourcePool.
	public class ChervilTestInput : MonoBehaviour
	{
		public ConstructDeploymentManager constructManager;
		public MeleeComboComponent combo;
		public IDamageable target;

		public const string SprinklerId = "PhlogistonSprinkler";
		public const string ConduitId = "TidalConduit";
		public const string WeaverId = "MistWeaver";
		public const string DecoyId = "UmbralDecoy";
		public const string TurretId = "PrismTurret";
		public const string BladeBeastSentryId = "BladeBeastSentry";
		public const string FireflyId = "Firefly";
		public const string AegisCrawlerId = "AegisCrawler";
		public const string GaleSpringId = "GaleSpring";
		public const string VoltaicCoilId = "VoltaicCoil";
		public const string SteamEngineRamId = "SteamEngineRam";

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.E)) constructManager.DeployConstruct(SprinklerId);
			if (Input.GetKeyDown(KeyCode.R)) constructManager.DeployConstruct(ConduitId);
			if (Input.GetKeyDown(KeyCode.T)) constructManager.DeployConstruct(WeaverId);
			if (Input.GetKeyDown(KeyCode.Y)) constructManager.DeployConstruct(DecoyId);
			if (Input.GetKeyDown(KeyCode.U)) constructManager.DeployConstruct(TurretId);

			if (Input.GetKeyDown(KeyCode.G)) Overclock(SprinklerId);
			if (Input.GetKeyDown(KeyCode.H)) Overclock(ConduitId);
			if (Input.GetKeyDown(KeyCode.J)) Overclock(WeaverId);
			if (Input.GetKeyDown(KeyCode.N)) Overclock(DecoyId);
			if (Input.GetKeyDown(KeyCode.M)) Overclock(TurretId);

			if (Input.GetKeyDown(KeyCode.Z)) constructManager.Deconstruct(SprinklerId);
			if (Input.GetKeyDown(KeyCode.X)) constructManager.Deconstruct(ConduitId);
			if (Input.GetKeyDown(KeyCode.C)) constructManager.Deconstruct(WeaverId);
			if (Input.GetKeyDown(KeyCode.V)) constructManager.Deconstruct(DecoyId);
			if (Input.GetKeyDown(KeyCode.B)) constructManager.Deconstruct(TurretId);

			if (Input.GetMouseButtonDown(0)) combo.RegisterHit();
		}

		private void Overclock(string constructId)
		{
			if (!constructManager.Overclock(constructId))
			{
				return;
			}

			if (!constructManager.TryGetDefinition(constructId, out ConstructDefinition definition))
			{
				return;
			}

			EffectResolver.Apply(definition.overclockEffectType, definition.overclockEffectMagnitude, definition.overclockAbilityId, target);
		}
	}
}
