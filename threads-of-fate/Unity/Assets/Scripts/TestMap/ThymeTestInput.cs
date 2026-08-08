using System.Collections.Generic;
using UnityEngine;
using ThreadsOfFate.Core;
using ThreadsOfFate.Thyme;
using ThreadsOfFate.World;

namespace ThreadsOfFate.TestMap
{
	// Thyme's slice of the keyboard test harness. Only exercises the 4
	// world-puzzle elements (the other 3 — Lumen/Volt/Aurum — have no
	// station to react to them, so they're not wired to a key here, though
	// their spells are still fully present in the spell factory's data):
	//   1/2/3/4       = cast Ember/Tide/Gale/Umbra + Bolt
	//   Shift+1/2/3/4 = cast Ember/Tide/Gale/Umbra + Arc
	//   Left Mouse    = ring-blade combo hit (regenerates MP via ThymeComboMPRegen)
	// Casting spends MP from the shared avatar-wide pool (same pool Sorrel
	// and Chervil draw from) and, on a valid Element+CastShape combo, applies
	// the resolved damage/heal to the target dummy. The element itself
	// always fires the matching world-interaction station regardless of
	// whether a combat spell exists for that Cast Shape — matches the spec's
	// "element always maps to the same world state" rule.
	//
	// Disabled by default; CharacterSwitchController enables this only while
	// Thyme is the active character.
	public class ThymeTestInput : MonoBehaviour
	{
		public SpellCombinationFactory spellFactory;
		public SharedManaPool manaPool;
		public MeleeComboComponent combo;
		public IDamageable target;
		public List<ElementalTrigger> elementalStations = new List<ElementalTrigger>();

		private void Update()
		{
			bool useArc = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			string castShape = useArc ? "Arc" : "Bolt";

			if (Input.GetKeyDown(KeyCode.Alpha1)) CastSpell(MagicElement.Ember, castShape);
			if (Input.GetKeyDown(KeyCode.Alpha2)) CastSpell(MagicElement.Tide, castShape);
			if (Input.GetKeyDown(KeyCode.Alpha3)) CastSpell(MagicElement.Gale, castShape);
			if (Input.GetKeyDown(KeyCode.Alpha4)) CastSpell(MagicElement.Umbra, castShape);

			if (Input.GetMouseButtonDown(0)) combo.RegisterHit();
		}

		private void CastSpell(MagicElement element, string castShape)
		{
			if (spellFactory.CombineSpell(element, castShape, out SpellDefinition spell))
			{
				bool canAfford = spell.consumesAllMP
					? manaPool.CurrentMP >= spell.minimumMPToCast
					: manaPool.CurrentMP >= spell.mpCost;

				if (!canAfford)
				{
					Debug.Log($"[Thyme] Not enough MP to cast {spell.spellName}.");
				}
				else
				{
					float spent = spell.consumesAllMP ? manaPool.CurrentMP : spell.mpCost;
					manaPool.ConsumeMP(spent);
					Debug.Log($"[Thyme] Cast {element}+{castShape} -> {spell.spellName} ({spent} MP).");
					EffectResolver.Apply(spell.effectType, spell.effectMagnitude, spell.spellName, target);
				}
			}
			else
			{
				Debug.Log($"[Thyme] Cast {element}+{castShape} -> no combat spell defined for this pair.");
			}

			foreach (ElementalTrigger station in elementalStations)
			{
				station.TryActivate(element);
			}
		}
	}
}
