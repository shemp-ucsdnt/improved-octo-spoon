namespace ThreadsOfFate.Chervil
{
	// Implemented by whatever a Prism Turret puzzle unlocks (a gate, a
	// switch) when a beam lands on it continuously.
	public interface IPrismReceiver
	{
		void OnBeamHit();
		void OnBeamLost();
	}
}
