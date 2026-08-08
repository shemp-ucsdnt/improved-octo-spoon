using System;
using System.Collections.Generic;
using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.Sorrel
{
	// Sorrel's shapeshifting system. Transforming requires currently holding a
	// coin for that form (collected via the 5-slot FIFO MonsterCoinQueue —
	// this doesn't consume the coin, so Sorrel can transform in and out of a
	// held form freely until a 6th distinct pickup evicts it; this is an
	// interpretation of the spec, which describes the queue but not exactly
	// how holding a coin relates to transforming). MP drains continuously
	// while transformed (mirrors ConstructDeploymentManager's tick-drain
	// pattern) and auto-reverts to base form at 0 MP, alongside a discrete
	// per-form ability cost. Physics/hitbox swap via
	// CharacterMovementController.SetPhysicsOverride(). MP is drawn from the
	// shared avatar-wide pool (assigned externally), not a component on this
	// GameObject — see SharedManaPool.
	public class SorrelTransformationController : MonoBehaviour, IHeavySwitch, INarrowPassage, IHazardFlight
	{
		[SerializeField] private List<MonsterFormDefinition> availableForms = new List<MonsterFormDefinition>();
		[SerializeField] private MonsterFormDefinition baseForm;

		public SharedManaPool manaPool;

		public MonsterCoinQueue Coins { get; } = new MonsterCoinQueue();
		public MonsterFormDefinition CurrentForm { get; private set; }
		public event Action<MonsterFormDefinition> OnFormChanged;

		private readonly Dictionary<string, MonsterFormDefinition> _formsById = new Dictionary<string, MonsterFormDefinition>();
		private CharacterMovementController _movement;

		private void Awake()
		{
			_movement = GetComponent<CharacterMovementController>();
			RebuildFormLookup();
			CurrentForm = baseForm;
		}

		// Runtime setup, mirroring ConstructDeploymentManager.RegisterConstructs
		// — lets procedurally-built rigs (currently the test map) supply forms
		// without needing Inspector-authored assets.
		public void Configure(MonsterFormDefinition newBaseForm, IEnumerable<MonsterFormDefinition> forms)
		{
			baseForm = newBaseForm;
			availableForms = new List<MonsterFormDefinition>(forms);
			RebuildFormLookup();
			CurrentForm = baseForm;
		}

		private void RebuildFormLookup()
		{
			_formsById.Clear();
			foreach (MonsterFormDefinition form in availableForms)
			{
				if (form != null)
				{
					_formsById[form.formId] = form;
				}
			}
		}

		public bool TransformInto(string formId)
		{
			if (!_formsById.TryGetValue(formId, out MonsterFormDefinition form))
			{
				return false;
			}

			if (!Coins.Contains(formId))
			{
				Debug.Log($"[Sorrel] No coin held for {formId}.");
				return false;
			}

			CurrentForm = form;
			_movement?.SetPhysicsOverride(form.gravityScale, form.jumpHeight, form.moveSpeedMultiplier, form.controllerHeight, form.controllerRadius);
			OnFormChanged?.Invoke(form);
			Debug.Log($"[Sorrel] Transformed into {form.displayName}.");
			return true;
		}

		public void RevertToBaseForm()
		{
			if (CurrentForm == baseForm)
			{
				return;
			}

			CurrentForm = baseForm;
			_movement?.ClearPhysicsOverride();
			OnFormChanged?.Invoke(baseForm);
			Debug.Log("[Sorrel] Reverted to base form.");
		}

		public bool UseFormAbility()
		{
			if (CurrentForm == null || CurrentForm == baseForm || string.IsNullOrEmpty(CurrentForm.abilityId))
			{
				return false;
			}

			if (!manaPool.ConsumeMP(CurrentForm.abilityMPCost))
			{
				Debug.Log("[Sorrel] Not enough MP for form ability.");
				return false;
			}

			Debug.Log($"[Sorrel] Used ability {CurrentForm.abilityId}.");
			return true;
		}

		private void Update()
		{
			if (CurrentForm == null || CurrentForm == baseForm)
			{
				return;
			}

			manaPool.ConsumeMP(CurrentForm.transformMPDrainPerSecond * Time.deltaTime);

			if (manaPool.CurrentMP <= 0f)
			{
				RevertToBaseForm();
			}
		}

		public float BodyWeight => CurrentForm != null ? CurrentForm.bodyWeight : 0f;
		public float BodySize => CurrentForm != null ? CurrentForm.bodySize : 1f;
		public bool CanFlyOverHazard => CurrentForm != null && CurrentForm.canFly;
	}
}
