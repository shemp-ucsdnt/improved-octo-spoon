namespace ThreadsOfFate.Core
{
	// Capability-query interfaces environment objects use to gate passage.
	// Only Sorrel's forms implement these today (Cragclaw -> IHeavySwitch,
	// Root-Sprite -> INarrowPassage, Drift-Eel -> IHazardFlight), but they
	// aren't Sorrel-specific by design — any future character/vehicle/ability
	// could implement one.
	public interface IHeavySwitch
	{
		float BodyWeight { get; }
	}

	public interface INarrowPassage
	{
		float BodySize { get; }
	}

	public interface IHazardFlight
	{
		bool CanFlyOverHazard { get; }
	}
}
