using UnityEngine;

namespace ThreadsOfFate.World
{
	// Turns "freeze the water" into a real traversal puzzle: before
	// freezing, a solid (invisible) hazard blocker prevents crossing; once
	// Blue is cast on it, the blocker drops and a flush ice platform
	// appears in its place.
	[RequireComponent(typeof(ElementalTrigger))]
	public class FreezeWalkway : MonoBehaviour
	{
		[Tooltip("X/Z should match the water pool's own footprint; Y is the blocker wall's height.")]
		[SerializeField] private Vector3 footprintSize = new Vector3(3f, 2f, 3f);
		[SerializeField] private float iceThickness = 0.05f;

		private GameObject _hazardBlocker;
		private GameObject _icePlatform;

		private void Awake()
		{
			// This object's own collider (from whatever primitive the water
			// pool visual is) would otherwise collide independently of the
			// blocker/platform pair below, which are the sole intended
			// collision volumes for this puzzle.
			Collider ownCollider = GetComponent<Collider>();
			if (ownCollider != null)
			{
				ownCollider.enabled = false;
			}

			Vector3 center = transform.position;

			_hazardBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_hazardBlocker.name = $"{name}_HazardBlocker";
			_hazardBlocker.transform.position = center + Vector3.up * (footprintSize.y * 0.5f);
			_hazardBlocker.transform.localScale = new Vector3(footprintSize.x, footprintSize.y, footprintSize.z);
			_hazardBlocker.GetComponent<Renderer>().enabled = false;

			_icePlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_icePlatform.name = $"{name}_IcePlatform";
			_icePlatform.transform.position = center + Vector3.up * (iceThickness * 0.5f);
			_icePlatform.transform.localScale = new Vector3(footprintSize.x, iceThickness, footprintSize.z);
			_icePlatform.GetComponent<Renderer>().material.color = new Color(0.8f, 0.95f, 1f);
			_icePlatform.SetActive(false);

			GetComponent<ElementalTrigger>().OnActivated += HandleActivated;
		}

		private void HandleActivated(ElementalTrigger trigger)
		{
			if (trigger.kind != ElementalTriggerKind.Freeze)
			{
				return;
			}

			_hazardBlocker.SetActive(false);
			_icePlatform.SetActive(true);
			Debug.Log($"[FreezeWalkway] {name} is now walkable.");
		}
	}
}
