using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.World
{
	// Blocks nothing physically, but damages anything without flight over a
	// hazard (lava, chasm, deep water) — Drift-Eel's flight lets it cross
	// freely, everything else takes periodic damage while inside.
	[RequireComponent(typeof(Collider))]
	public class HazardZone : MonoBehaviour
	{
		[SerializeField] private float damagePerSecond = 15f;

		private void Awake()
		{
			GetComponent<Collider>().isTrigger = true;
		}

		private void OnTriggerStay(Collider other)
		{
			IHazardFlight flight = other.GetComponentInParent<IHazardFlight>();
			if (flight != null && flight.CanFlyOverHazard)
			{
				return;
			}

			IDamageable damageable = other.GetComponentInParent<IDamageable>();
			damageable?.ApplyDamage(damagePerSecond * Time.deltaTime, "Hazard");
		}
	}
}
