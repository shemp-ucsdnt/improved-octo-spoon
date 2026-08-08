using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.World
{
	// Soft-blocks anything whose current body size exceeds maxBodySize by
	// pushing it back out — Root-Sprite fits through, Fangstrider/Cragclaw
	// don't. A real level would instead just make the passage geometrically
	// too small for big forms to fit; this scripted push-back stands in for
	// that until real level geometry exists.
	[RequireComponent(typeof(Collider))]
	public class NarrowPassageGate : MonoBehaviour
	{
		[SerializeField] private float maxBodySize = 0.5f;
		[SerializeField] private float pushBackDistance = 1f;

		private void Awake()
		{
			GetComponent<Collider>().isTrigger = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			INarrowPassage passage = other.GetComponentInParent<INarrowPassage>();
			if (passage == null || passage.BodySize <= maxBodySize)
			{
				return;
			}

			CharacterController controller = other.GetComponentInParent<CharacterController>();
			if (controller != null)
			{
				controller.enabled = false;
				controller.transform.position -= transform.forward * pushBackDistance;
				controller.enabled = true;
			}

			Debug.Log($"[NarrowPassageGate] {name}: too big to fit (size {passage.BodySize} > {maxBodySize}).");
		}
	}
}
