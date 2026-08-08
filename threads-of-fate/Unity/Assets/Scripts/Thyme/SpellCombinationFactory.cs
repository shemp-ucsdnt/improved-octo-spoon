using System.Collections.Generic;

namespace ThreadsOfFate.Thyme
{
	// Strategy/Factory for Thyme's Element x CastShape magic matrix: each
	// SpellDefinition asset is a strategy, keyed by (element, castShape) for
	// O(1) lookup at cast time. Plain C# class (not a MonoBehaviour) so it can
	// be owned by whatever composes Thyme's spellcasting component.
	public class SpellCombinationFactory
	{
		private readonly Dictionary<SpellKey, SpellDefinition> _lookup = new Dictionary<SpellKey, SpellDefinition>();

		public void Initialize(IEnumerable<SpellDefinition> spellDefinitions)
		{
			_lookup.Clear();
			foreach (SpellDefinition spell in spellDefinitions)
			{
				if (spell != null)
				{
					_lookup[new SpellKey(spell.element, spell.castShape)] = spell;
				}
			}
		}

		public bool CombineSpell(MagicElement element, string castShape, out SpellDefinition spell)
		{
			return _lookup.TryGetValue(new SpellKey(element, castShape), out spell);
		}

		public bool IsValidCombination(MagicElement element, string castShape)
		{
			return _lookup.ContainsKey(new SpellKey(element, castShape));
		}
	}
}
