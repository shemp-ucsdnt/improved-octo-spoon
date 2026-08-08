using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreadsOfFate.Core
{
	// Per-character HP pool. Each of Sorrel/Thyme/Chervil gets its own
	// instance — HP is never shared across characters. MP is shared instead
	// (see SharedManaPool) — the two pools are deliberately decoupled.
	public class CharacterVitals : MonoBehaviour, IDamageable, IStatusAffectable
	{
		[Header("Health")]
		[SerializeField] private float maxHP = 100f;

		public float CurrentHP { get; private set; }
		public float MaxHP => maxHP;
		public bool IsDead => CurrentHP <= 0f;

		public event Action<float, float> OnHPChanged;
		public event Action OnDied;

		private void Awake()
		{
			CurrentHP = maxHP;
		}

		public void ApplyDamage(float amount, string sourceTag)
		{
			if (amount <= 0f || IsDead)
			{
				return;
			}

			CurrentHP = Mathf.Max(0f, CurrentHP - amount);
			OnHPChanged?.Invoke(CurrentHP, maxHP);

			if (IsDead)
			{
				OnDied?.Invoke();
			}
		}

		public void ApplyHeal(float amount)
		{
			if (amount <= 0f || IsDead)
			{
				return;
			}

			CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
			OnHPChanged?.Invoke(CurrentHP, maxHP);
		}

		private readonly HashSet<string> _statuses = new HashSet<string>();

		public void ApplyStatus(string statusId) => _statuses.Add(statusId);
		public void ClearStatus(string statusId) => _statuses.Remove(statusId);
		public void ClearAllStatuses() => _statuses.Clear();
		public bool HasStatus(string statusId) => _statuses.Contains(statusId);
	}
}
