using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.Chervil
{
	// Chervil-specific IChervilResourcePool: MP is delegated to the shared
	// avatar-wide SharedManaPool (assigned externally, same pool every
	// character draws from), Kinetic Energy is owned here since only
	// Chervil's kit uses it.
	//
	// Nothing generates Kinetic Energy yet — the whip-dagger melee combo
	// generator is Phase 3 in the implementation spec. AddKineticEnergy is
	// the hook that generator will call.
	public class ChervilResourcePool : MonoBehaviour, IChervilResourcePool
	{
		[SerializeField] private float maxKineticEnergy = 50f;

		public SharedManaPool manaPool;

		private float _currentKineticEnergy;

		public float CurrentMP => manaPool.CurrentMP;
		public float KineticEnergy => _currentKineticEnergy;

		public bool ConsumeMP(float amount)
		{
			return manaPool.ConsumeMP(amount);
		}

		public bool ConsumeKineticEnergy(float amount)
		{
			bool hadEnough = _currentKineticEnergy >= amount;
			_currentKineticEnergy = Mathf.Max(0f, _currentKineticEnergy - amount);
			return hadEnough;
		}

		public void AddKineticEnergy(float amount)
		{
			if (amount <= 0f)
			{
				return;
			}

			_currentKineticEnergy = Mathf.Min(maxKineticEnergy, _currentKineticEnergy + amount);
		}
	}
}
