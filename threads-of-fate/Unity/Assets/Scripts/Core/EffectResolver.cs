namespace ThreadsOfFate.Core
{
	// The one place that turns "an EffectType + a magnitude" into an actual
	// IDamageable call. Previously this if/else lived duplicated in
	// ThymeTestInput and ChervilTestInput; promoted here so the real player
	// controller (and anything else) has one reusable path instead of
	// reinventing it.
	public static class EffectResolver
	{
		public static void Apply(EffectType type, float magnitude, string sourceTag, IDamageable target)
		{
			if (target == null || magnitude <= 0f)
			{
				return;
			}

			if (type == EffectType.Damage)
			{
				target.ApplyDamage(magnitude, sourceTag);
			}
			else
			{
				target.ApplyHeal(magnitude);
			}
		}
	}
}
