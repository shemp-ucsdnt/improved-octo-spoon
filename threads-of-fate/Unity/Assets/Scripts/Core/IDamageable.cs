namespace ThreadsOfFate.Core
{
	public interface IDamageable
	{
		float CurrentHP { get; }
		float MaxHP { get; }
		bool IsDead { get; }

		void ApplyDamage(float amount, string sourceTag);
		void ApplyHeal(float amount);
	}
}
