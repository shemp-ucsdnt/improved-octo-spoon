using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.World
{
	// Only depresses (opens its linked gate) once something heavy enough
	// steps on it — Cragclaw's weight, per the spec.
	[RequireComponent(typeof(Collider))]
	public class HeavySwitchPlate : MonoBehaviour
	{
		[SerializeField] private float requiredWeight = 100f;
		[SerializeField] private ActivationGate linkedGate;

		private void Awake()
		{
			GetComponent<Collider>().isTrigger = true;
		}

		public void SetLinkedGate(ActivationGate gate)
		{
			linkedGate = gate;
		}

		private void OnTriggerEnter(Collider other)
		{
			IHeavySwitch heavy = other.GetComponentInParent<IHeavySwitch>();
			if (heavy != null && heavy.BodyWeight >= requiredWeight)
			{
				linkedGate?.Open();
				Debug.Log($"[HeavySwitchPlate] {name} depressed.");
			}
		}

		private void OnTriggerExit(Collider other)
		{
			IHeavySwitch heavy = other.GetComponentInParent<IHeavySwitch>();
			if (heavy != null && heavy.BodyWeight >= requiredWeight)
			{
				linkedGate?.Close();
				Debug.Log($"[HeavySwitchPlate] {name} released.");
			}
		}
	}
}
