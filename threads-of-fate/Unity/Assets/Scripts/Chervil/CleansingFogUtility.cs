using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.Chervil
{
	// Mist Weaver's Deconstruct utility blast: clears every status on any
	// IStatusAffectable within range. Not wired to an event automatically —
	// call Trigger() from wherever handles ConstructDeploymentManager's
	// OnConstructDeconstructed for the Mist Weaver's deconstructUtilityId.
	public class CleansingFogUtility : MonoBehaviour
	{
		[SerializeField] private float radius = 5f;

		public void Trigger()
		{
			Collider[] hits = Physics.OverlapSphere(transform.position, radius);
			foreach (Collider hit in hits)
			{
				if (hit.GetComponent(typeof(IStatusAffectable)) is IStatusAffectable affectable)
				{
					affectable.ClearAllStatuses();
				}
			}

			Debug.Log($"[CleansingFog] Cleared statuses within {radius}m.");
		}
	}
}
