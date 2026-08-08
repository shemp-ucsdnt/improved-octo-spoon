using UnityEngine;

namespace ThreadsOfFate.World
{
	// Generic "remove an obstacle" gate: disables its own collider(s) +
	// renderer(s) when opened. Reusable by any trigger source (an
	// ElementalTrigger's OnActivated, a beam receiver, a switch plate, ...)
	// via plain method calls rather than each puzzle piece reinventing
	// "how do I open a gate."
	public class ActivationGate : MonoBehaviour
	{
		public bool IsOpen { get; private set; }

		public void Open()
		{
			if (IsOpen)
			{
				return;
			}

			IsOpen = true;
			SetSolid(false);
			Debug.Log($"[ActivationGate] {name} opened.");
		}

		public void Close()
		{
			if (!IsOpen)
			{
				return;
			}

			IsOpen = false;
			SetSolid(true);
			Debug.Log($"[ActivationGate] {name} closed.");
		}

		private void SetSolid(bool solid)
		{
			foreach (Collider col in GetComponentsInChildren<Collider>())
			{
				col.enabled = solid;
			}
			foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
			{
				renderer.enabled = solid;
			}
		}
	}
}
