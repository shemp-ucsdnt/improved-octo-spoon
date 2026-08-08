using System;
using UnityEngine;

namespace ThreadsOfFate.Core
{
	// The one shared MP pool the active model draws from, regardless of
	// which of Sorrel/Thyme/Chervil is currently controlled (HP stays
	// independent per character — see CharacterVitals). Switching models
	// doesn't touch this component at all, so whatever drain/regen was
	// already running when the switch happened keeps running uninterrupted.
	//
	// No passive regen by default: Thyme's spec ties regen to melee combo
	// hits rather than time (see ThymeComboMPRegen), and nothing else has
	// specified passive regen. A regen source can call RestoreMP() directly.
	public class SharedManaPool : MonoBehaviour
	{
		[SerializeField] private float maxMP = 100f;

		public float CurrentMP { get; private set; }
		public float MaxMP => maxMP;

		public event Action<float, float> OnMPChanged;

		private void Awake()
		{
			CurrentMP = maxMP;
		}

		// Consumes up to amount, clamped at 0. Returns false if the pool held
		// less than amount (mirrors IChervilResourcePool.ConsumeMP semantics).
		public bool ConsumeMP(float amount)
		{
			bool hadEnough = CurrentMP >= amount;
			CurrentMP = Mathf.Max(0f, CurrentMP - amount);
			OnMPChanged?.Invoke(CurrentMP, maxMP);
			return hadEnough;
		}

		public void RestoreMP(float amount)
		{
			if (amount <= 0f)
			{
				return;
			}

			CurrentMP = Mathf.Min(maxMP, CurrentMP + amount);
			OnMPChanged?.Invoke(CurrentMP, maxMP);
		}
	}
}
