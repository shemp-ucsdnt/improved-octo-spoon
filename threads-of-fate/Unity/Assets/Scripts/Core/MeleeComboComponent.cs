using System;
using UnityEngine;

namespace ThreadsOfFate.Core
{
	// Generic combo-chain tracker: Thyme's dual ring-blades and Chervil's
	// whip-dagger both need "hits landed in a chain" to drive resource
	// recovery (Thyme: MP regen, Chervil: Kinetic Energy), so this is shared
	// rather than duplicated per character. RegisterHit() is meant to be
	// called from a weapon/animation hit event; the test harness calls it
	// directly from an attack key press since neither exists yet.
	public class MeleeComboComponent : MonoBehaviour
	{
		[Tooltip("A gap longer than this resets the combo chain back to 0.")]
		[SerializeField] private float comboResetSeconds = 1.5f;

		public int ComboCount { get; private set; }
		public event Action<int> OnComboHit;

		private float _lastHitTime = float.NegativeInfinity;

		public void RegisterHit()
		{
			if (Time.time - _lastHitTime > comboResetSeconds)
			{
				ComboCount = 0;
			}

			ComboCount++;
			_lastHitTime = Time.time;
			OnComboHit?.Invoke(ComboCount);
		}

		private void Update()
		{
			if (ComboCount > 0 && Time.time - _lastHitTime > comboResetSeconds)
			{
				ComboCount = 0;
			}
		}
	}
}
