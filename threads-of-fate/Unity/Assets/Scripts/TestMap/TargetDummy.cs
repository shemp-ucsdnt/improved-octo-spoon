using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.TestMap
{
	// Stand-in enemy for exercising Thyme spells and Chervil Overclocks: no
	// real combat system exists yet (see Phase 1 in the implementation
	// spec), just enough to prove damage/heal wiring actually reaches a
	// target. Color lerps red(empty)->green(full) with current HP.
	[RequireComponent(typeof(Renderer))]
	public class TargetDummy : MonoBehaviour, IDamageable
	{
		[SerializeField] private float maxHP = 100f;

		public float CurrentHP { get; private set; }
		public float MaxHP => maxHP;
		public bool IsDead => CurrentHP <= 0f;

		private Renderer _renderer;

		private void Awake()
		{
			CurrentHP = maxHP;
			_renderer = GetComponent<Renderer>();
			UpdateColor();
		}

		public void ApplyDamage(float amount, string sourceTag)
		{
			if (amount <= 0f || IsDead)
			{
				return;
			}

			CurrentHP = Mathf.Max(0f, CurrentHP - amount);
			Debug.Log($"[TargetDummy] Took {amount} damage from {sourceTag}. HP {CurrentHP}/{maxHP}.");
			UpdateColor();

			if (IsDead)
			{
				Debug.Log("[TargetDummy] Defeated.");
			}
		}

		public void ApplyHeal(float amount)
		{
			if (amount <= 0f)
			{
				return;
			}

			CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
			Debug.Log($"[TargetDummy] Healed {amount}. HP {CurrentHP}/{maxHP}.");
			UpdateColor();
		}

		private void UpdateColor()
		{
			float t = maxHP > 0f ? CurrentHP / maxHP : 0f;
			_renderer.material.color = Color.Lerp(Color.red, Color.green, t);
		}
	}
}
