using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.Thyme
{
	// Binds Thyme's dual ring-blade combo chain to MP recovery, per spec:
	// "Resource Recovery: Bind MP regeneration directly to melee combo chain
	// hits." Each hit in the chain restores a flat amount; longer combos
	// aren't scaled up (no combo-multiplier value was specified anywhere).
	// Restores into the shared avatar-wide MP pool (assigned externally),
	// not a component on this GameObject — see SharedManaPool.
	[RequireComponent(typeof(MeleeComboComponent))]
	public class ThymeComboMPRegen : MonoBehaviour
	{
		[SerializeField] private float mpPerHit = 4f;

		public SharedManaPool manaPool;

		private void OnEnable()
		{
			GetComponent<MeleeComboComponent>().OnComboHit += HandleComboHit;
		}

		private void OnDisable()
		{
			GetComponent<MeleeComboComponent>().OnComboHit -= HandleComboHit;
		}

		private void HandleComboHit(int comboCount)
		{
			manaPool.RestoreMP(mpPerHit);
		}
	}
}
