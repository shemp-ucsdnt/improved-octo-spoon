using System;
using UnityEngine;
using ThreadsOfFate.Thyme;

namespace ThreadsOfFate.World
{
	// World object that reacts to a spell's element-derived world-interaction
	// tag (see ThymeElemental.GetWorldInteractionTagForElement), independent
	// of whichever Cast Shape was used to cast it — matches the spec's
	// "element always maps to the same world state" rule.
	//
	// Promoted out of the test-map folder: this is a real, reusable level
	// mechanic (levels will place these), not test-only scaffolding.
	public enum ElementalTriggerKind
	{
		Ignite,
		Freeze,
		DriveTurbine,
		Shatter
	}

	[RequireComponent(typeof(Renderer))]
	public class ElementalTrigger : MonoBehaviour
	{
		public ElementalTriggerKind kind;
		public MagicElement requiredElement;
		public bool IsActivated { get; private set; }

		// Fired after the built-in visual feedback below, so puzzle pieces
		// (FreezeWalkway, TurbinePlatform, ActivationGate, ...) can react
		// without ElementalTrigger needing to know they exist.
		public event Action<ElementalTrigger> OnActivated;

		private Renderer _renderer;

		private void Awake()
		{
			_renderer = GetComponent<Renderer>();
		}

		// Called by the test harness whenever a spell of castElement is cast.
		// Returns true if this station reacted.
		public bool TryActivate(MagicElement castElement)
		{
			if (castElement != requiredElement)
			{
				return false;
			}

			IsActivated = true;
			string tag = ThymeElemental.GetWorldInteractionTagForElement(castElement);

			switch (kind)
			{
				case ElementalTriggerKind.Ignite:
					_renderer.material.color = new Color(1f, 0.35f, 0f);
					Debug.Log($"[ElementalTrigger] {name} ignited ({tag}).");
					break;
				case ElementalTriggerKind.Freeze:
					_renderer.material.color = new Color(0.6f, 0.85f, 1f);
					Debug.Log($"[ElementalTrigger] {name} frozen solid ({tag}).");
					break;
				case ElementalTriggerKind.DriveTurbine:
					Rotator rotator = GetComponent<Rotator>();
					if (rotator != null)
					{
						rotator.enabled = true;
					}
					_renderer.material.color = Color.green;
					Debug.Log($"[ElementalTrigger] {name} engaged ({tag}).");
					break;
				case ElementalTriggerKind.Shatter:
					Debug.Log($"[ElementalTrigger] {name} shattered ({tag}).");
					gameObject.SetActive(false);
					break;
			}

			OnActivated?.Invoke(this);
			return true;
		}
	}
}
