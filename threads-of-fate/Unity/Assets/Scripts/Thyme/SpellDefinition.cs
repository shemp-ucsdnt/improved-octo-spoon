using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.Thyme
{
	// One asset per valid Element+CastShape combination (e.g. Ember+Bolt =
	// Cinderstream). The grid is deliberately irregular — not every
	// Element/CastShape pairing exists (see player-mechanics.pdf).
	// Create via Assets > Create > Threads of Fate > Thyme > Spell Definition.
	[CreateAssetMenu(menuName = "Threads of Fate/Thyme/Spell Definition")]
	public class SpellDefinition : ScriptableObject
	{
		[Header("Recipe")]
		public MagicElement element;
		[Tooltip("Bolt/Arc/Surge/Ring/Cascade/Overflow/Zenith. Open, data-driven set rather than an enum, same as before.")]
		public string castShape;

		[Header("Spell")]
		public string spellName;
		[TextArea]
		public string description;

		[Header("Cost")]
		[Tooltip("Ignored when consumesAllMP is set (Overflow-tier spells: cost is the entire current MP bar).")]
		public float mpCost;
		[Tooltip("Overflow-tier spells drain the whole MP bar instead of a flat cost, and require at least minimumMPToCast banked before they can be cast at all.")]
		public bool consumesAllMP;
		public float minimumMPToCast;

		[Header("Effect")]
		public EffectType effectType = EffectType.Damage;
		public float effectMagnitude;
		[Tooltip("Resolved by the combat system to an ability/effect.")]
		public string abilityId;
	}

	public readonly struct SpellKey : System.IEquatable<SpellKey>
	{
		public readonly MagicElement element;
		public readonly string castShape;

		public SpellKey(MagicElement element, string castShape)
		{
			this.element = element;
			this.castShape = castShape;
		}

		public bool Equals(SpellKey other) => element == other.element && castShape == other.castShape;
		public override bool Equals(object obj) => obj is SpellKey other && Equals(other);
		public override int GetHashCode() => System.HashCode.Combine(element, castShape);
	}
}
