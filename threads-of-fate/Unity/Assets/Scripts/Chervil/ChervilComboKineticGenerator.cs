using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.Chervil
{
	// Binds Chervil's whip-dagger combo chain to Kinetic Energy generation —
	// the spec names Kinetic Energy as "generated via whip-dagger melee
	// combos" but never built a generator for it; Overclock has only ever
	// consumed it until now.
	[RequireComponent(typeof(MeleeComboComponent))]
	[RequireComponent(typeof(ChervilResourcePool))]
	public class ChervilComboKineticGenerator : MonoBehaviour
	{
		[SerializeField] private float kineticEnergyPerHit = 5f;

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
			GetComponent<ChervilResourcePool>().AddKineticEnergy(kineticEnergyPerHit);
		}
	}
}
