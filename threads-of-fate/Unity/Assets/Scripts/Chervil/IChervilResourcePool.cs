namespace ThreadsOfFate.Chervil
{
	// Decouples ConstructDeploymentManager from a specific character class:
	// anything that owns MP and Kinetic Energy (generated via whip-dagger
	// melee combos) can implement this to be drained/spent by constructs.
	public interface IChervilResourcePool
	{
		float CurrentMP { get; }

		// Consumes up to amount, clamped at 0 (so continuous drain can run the
		// pool dry rather than being rejected outright). Returns false if the
		// pool held less than amount. Callers needing all-or-nothing semantics
		// (e.g. Overclock's flat cost) should check CurrentMP first.
		bool ConsumeMP(float amount);

		float KineticEnergy { get; }
		bool ConsumeKineticEnergy(float amount);
	}
}
