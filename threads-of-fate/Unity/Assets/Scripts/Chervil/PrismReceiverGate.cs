using UnityEngine;
using ThreadsOfFate.World;

namespace ThreadsOfFate.Chervil
{
	// Adapts a generic World.ActivationGate to the Prism Turret's beam
	// system, so ActivationGate itself doesn't need to know Chervil exists
	// (World stays a lower-level dependency Chervil builds on, not the
	// reverse).
	[RequireComponent(typeof(ActivationGate))]
	public class PrismReceiverGate : MonoBehaviour, IPrismReceiver
	{
		private ActivationGate _gate;

		private void Awake()
		{
			_gate = GetComponent<ActivationGate>();
		}

		public void OnBeamHit() => _gate.Open();
		public void OnBeamLost() => _gate.Close();
	}
}
