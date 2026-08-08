namespace ThreadsOfFate.Thyme
{
	// The 7 canonical elements. Only 4 (Ember/Tide/Gale/Umbra) have a
	// world-puzzle interaction (see ThymeElemental.GetWorldInteractionTagForElement)
	// — Lumen/Volt/Aurum are combat-only per the spec's puzzle mapping.
	public enum MagicElement
	{
		Lumen,
		Tide,
		Ember,
		Gale,
		Umbra,
		Volt,
		Aurum
	}

	public static class ThymeElemental
	{
		public static string GetWorldInteractionTagForElement(MagicElement element)
		{
			switch (element)
			{
				case MagicElement.Ember: return "IgniteBrazier";
				case MagicElement.Tide: return "FreezeWater";
				case MagicElement.Gale: return "DriveTurbine";
				case MagicElement.Umbra: return "ShatterBarrier";
				default: return null;
			}
		}
	}
}
