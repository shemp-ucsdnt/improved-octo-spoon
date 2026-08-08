using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreadsOfFate.Core
{
	[Serializable]
	public class CharacterSlot
	{
		public CharacterId id;
		public GameObject root;

		// Enabled only while this character is the active one (input
		// handlers, etc.). CharacterVitals and other always-on state should
		// NOT go here — HP/MP keep ticking (e.g. Chervil's construct drain)
		// whether or not that character is currently controlled.
		public List<Behaviour> activeOnlyBehaviours = new List<Behaviour>();
	}

	// Free, instant switching: control can move to another character from
	// anywhere, at any time, at no cost. Switching itself doesn't move
	// anyone; the newly active character is wherever they already were.
	public class CharacterSwitchController : MonoBehaviour
	{
		[SerializeField] private List<CharacterSlot> slots = new List<CharacterSlot>();

		public CharacterId ActiveCharacter { get; private set; }
		public event Action<CharacterId> OnActiveCharacterChanged;

		// Runtime setup, called by whatever builds the character roster
		// (currently the test map; later, real level bootstrap code).
		public void Initialize(List<CharacterSlot> characterSlots, CharacterId startingCharacter)
		{
			slots = characterSlots;
			foreach (CharacterSlot slot in slots)
			{
				SetSlotActive(slot, slot.id == startingCharacter);
			}
			ActiveCharacter = startingCharacter;
		}

		public bool SwitchTo(CharacterId id)
		{
			if (id == ActiveCharacter)
			{
				return false;
			}

			CharacterSlot target = slots.Find(s => s.id == id);
			if (target == null)
			{
				return false;
			}

			CharacterSlot current = slots.Find(s => s.id == ActiveCharacter);
			if (current != null)
			{
				SetSlotActive(current, false);
			}

			SetSlotActive(target, true);
			ActiveCharacter = id;
			OnActiveCharacterChanged?.Invoke(id);
			return true;
		}

		private static void SetSlotActive(CharacterSlot slot, bool active)
		{
			foreach (Behaviour behaviour in slot.activeOnlyBehaviours)
			{
				if (behaviour != null)
				{
					behaviour.enabled = active;
				}
			}
		}
	}
}
