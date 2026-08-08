using UnityEngine;

namespace ThreadsOfFate.Sorrel
{
	// One asset per monster form (Fangstrider, Drift-Eel, Cragclaw,
	// Root-Sprite, ...) plus one for Sorrel's own base/human form. Create via
	// Assets > Create > Threads of Fate > Sorrel > Monster Form Definition.
	[CreateAssetMenu(menuName = "Threads of Fate/Sorrel/Monster Form Definition")]
	public class MonsterFormDefinition : ScriptableObject
	{
		[Header("Identity")]
		public string formId;
		public string displayName;
		[Tooltip("Fang & Wing / Hollow & Wisp / Stone & Iron / Root & Thread. Empty for the base form.")]
		public string family;

		[Header("Flavor (from player-mechanics.pdf, not mechanically wired yet)")]
		[Tooltip("The PDF's 'Stats / Movement' column verbatim — covers traits (Def, HP, Crit) this component has no numeric field for.")]
		public string statsSummary;
		public string primaryAttack;
		public string specialUtility;
		[TextArea]
		public string fieldTacticalUse;

		[Header("Movement")]
		public float gravityScale = 1f;
		public float jumpHeight = 1.2f;
		public float moveSpeedMultiplier = 1f;
		public bool canFly;

		[Header("Collision / Hitbox")]
		public float controllerHeight = 1.8f;
		public float controllerRadius = 0.4f;

		[Header("Environment Interaction")]
		[Tooltip("IHeavySwitch: heavy switches require a minimum weight (Cragclaw).")]
		public float bodyWeight = 55f;
		[Tooltip("INarrowPassage: narrow passages require a maximum size (Root-Sprite).")]
		public float bodySize = 1f;

		[Header("Resources")]
		[Tooltip("Continuous MP drain while transformed into this form. Ignored for the base form.")]
		public float transformMPDrainPerSecond = 4f;
		public float abilityMPCost = 15f;
		public string abilityId;
	}
}
