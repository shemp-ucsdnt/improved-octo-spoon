using System.Collections.Generic;
using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.Chervil
{
	// One asset per deployable construct (Blade-Beast Sentry, Tidal Conduit,
	// Mist Weaver, Umbral Decoy, Prism Turret, ...). Create via
	// Assets > Create > Threads of Fate > Chervil > Construct Definition.
	[CreateAssetMenu(menuName = "Threads of Fate/Chervil/Construct Definition")]
	public class ConstructDefinition : ScriptableObject
	{
		[Header("Identity")]
		public string constructId;
		public string displayName;
		[Tooltip("Combat-role label from the roster table (e.g. 'Flanker / Mobile Utility'), not mechanically wired to anything yet — no combat/targeting/AI system exists.")]
		public string combatRole;
		[Tooltip("The roster table's 'Primary Field Utility' column verbatim.")]
		public string fieldFunction;

		[Header("Mobility")]
		[Tooltip("Whether this construct moves (patrols/follows/hovers) once deployed. Data only — no construct movement/AI system exists yet; a real implementation needs one before this means anything at runtime.")]
		public bool isMobile;

		[Header("Recipe")]
		[Tooltip("Threads consumed from the Thread Inventory on deployment.")]
		public List<ThreadCost> requiredThreads = new List<ThreadCost>();

		[Header("Deploy Cost")]
		[Tooltip("Flat MP charged once at deployment, on top of the thread cost. Distinct from maintenanceMPPerSecond below.")]
		public float deployMPCost;

		[Header("Maintenance")]
		[Tooltip("Continuous MP/sec drain while deployed. In this roster, only the 4 Mobile constructs have upkeep — Immobile ones are a flat deploy cost with no ongoing drain.")]
		public float maintenanceMPPerSecond;

		[Header("Overclock")]
		[Tooltip("Flat MP cost for the high-impact attack. This roster's Overclocks are flat-MP-only (no separate Kinetic Energy figure was given), so overclockKineticEnergyCost is left at 0 — ConsumeKineticEnergy(0) is trivially satisfied, so Overclock ends up MP-gated only without deleting the Kinetic Energy subsystem itself.")]
		public float overclockMPCost;
		public float overclockKineticEnergyCost;
		public EffectType overclockEffectType = EffectType.Damage;
		public float overclockEffectMagnitude;
		[Tooltip("Resolved by the combat system to an ability/effect.")]
		public string overclockAbilityId;
		[TextArea]
		public string overclockDescription;

		[Header("Deconstruct")]
		public bool refundThreadsOnDeconstruct = true;
		[Tooltip("Utility blast on manual deconstruct (e.g. Steam Screen, Aquatic Cleansing, Dew Drop). Leave blank if none.")]
		public string deconstructUtilityId;
		[TextArea]
		public string deconstructDescription;

		[Header("Visual")]
		[Tooltip("Spawned at deploy time and assigned to ActiveConstruct.spawnedActor. If null, ConstructDeploymentManager spawns a placeholder capsule in placeholderColor instead.")]
		public GameObject visualPrefab;
		public Color placeholderColor = Color.white;
	}
}
