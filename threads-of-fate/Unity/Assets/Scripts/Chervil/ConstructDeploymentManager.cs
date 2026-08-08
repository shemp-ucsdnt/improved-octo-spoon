using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThreadsOfFate.Chervil
{
	// Manages Chervil's deployed constructs: a flat MP cost + thread cost to
	// deploy, continuous MP maintenance drain for Mobile constructs (see
	// ConstructDefinition.isMobile), Overclock (flat MP, optionally also
	// Kinetic Energy for a high-impact attack), and Deconstruct (safe
	// unsummon, optionally refunding threads / firing a utility blast).
	// Auto-deconstructs everything if MP hits 0.
	[RequireComponent(typeof(ThreadInventoryComponent))]
	public class ConstructDeploymentManager : MonoBehaviour
	{
		[Tooltip("Every construct this character can deploy. Looked up by constructId.")]
		[SerializeField] private List<ConstructDefinition> availableConstructs = new List<ConstructDefinition>();

		[Tooltip("0 = unlimited simultaneous constructs.")]
		[SerializeField] private int maxSimultaneousConstructs = 1;

		[Tooltip("Where deployed constructs appear. Defaults to this GameObject's own transform if unset.")]
		[SerializeField] private Transform deploySpawnOrigin;
		[SerializeField] private Vector3 deploySpawnLocalOffset = new Vector3(0f, 0f, 1.5f);

		// Fired for UI/VFX/audio hooks (Observer pattern).
		public event Action<string> OnConstructDeployed;
		public event Action<string, bool> OnConstructDeconstructed;
		public event Action<string, string> OnConstructOverclocked;

		private readonly Dictionary<string, ConstructDefinition> _definitionsById = new Dictionary<string, ConstructDefinition>();
		private readonly List<ActiveConstruct> _activeConstructs = new List<ActiveConstruct>();

		private ThreadInventoryComponent _threadInventory;
		private IChervilResourcePool _resourcePool;

		private void Awake()
		{
			_threadInventory = GetComponent<ThreadInventoryComponent>();
			_resourcePool = GetComponent<IChervilResourcePool>();

			if (deploySpawnOrigin == null)
			{
				deploySpawnOrigin = transform;
			}

			RegisterConstructs(availableConstructs);
		}

		// Adds/overwrites deployable construct definitions by constructId.
		// Safe to call at runtime (e.g. from test/tooling code) in addition
		// to populating availableConstructs in the Inspector.
		public void RegisterConstructs(IEnumerable<ConstructDefinition> definitions)
		{
			foreach (ConstructDefinition definition in definitions)
			{
				if (definition != null)
				{
					_definitionsById[definition.constructId] = definition;
				}
			}
		}

		public bool IsConstructActive(string constructId)
		{
			return _activeConstructs.Any(a => a.definition.constructId == constructId);
		}

		public void SetMaxSimultaneousConstructs(int max)
		{
			maxSimultaneousConstructs = max;
		}

		public bool TryGetDefinition(string constructId, out ConstructDefinition definition)
		{
			return _definitionsById.TryGetValue(constructId, out definition);
		}

		// Lets level/test-setup code attach construct-specific extras (e.g.
		// PrismTurretBeam) to a just-deployed construct's spawned visual,
		// without ConstructDeploymentManager needing to know about them.
		public bool TryGetActiveSpawnedActor(string constructId, out GameObject spawnedActor)
		{
			ActiveConstruct active = _activeConstructs.FirstOrDefault(a => a.definition.constructId == constructId);
			spawnedActor = active?.spawnedActor;
			return active != null;
		}

		public bool DeployConstruct(string constructId)
		{
			if (maxSimultaneousConstructs > 0 && _activeConstructs.Count >= maxSimultaneousConstructs)
			{
				return false;
			}

			if (!_definitionsById.TryGetValue(constructId, out ConstructDefinition definition))
			{
				return false;
			}

			// Checked before spending anything so a failed deploy doesn't
			// waste threads (all-or-nothing, same principle as Overclock).
			if (definition.deployMPCost > 0f && (_resourcePool == null || _resourcePool.CurrentMP < definition.deployMPCost))
			{
				return false;
			}

			if (!_threadInventory.ConsumeThreads(definition.requiredThreads))
			{
				return false;
			}

			if (definition.deployMPCost > 0f)
			{
				_resourcePool.ConsumeMP(definition.deployMPCost);
			}

			var active = new ActiveConstruct
			{
				definition = definition,
				state = ConstructState.Active,
				spawnedActor = SpawnConstructVisual(definition)
			};
			_activeConstructs.Add(active);

			OnConstructDeployed?.Invoke(constructId);
			return true;
		}

		private GameObject SpawnConstructVisual(ConstructDefinition definition)
		{
			Vector3 spawnPosition = deploySpawnOrigin.TransformPoint(deploySpawnLocalOffset);

			if (definition.visualPrefab != null)
			{
				return Instantiate(definition.visualPrefab, spawnPosition, Quaternion.identity);
			}

			GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			placeholder.name = $"Construct_{definition.constructId}";
			placeholder.transform.position = spawnPosition;
			placeholder.transform.localScale = Vector3.one * 0.6f;
			placeholder.GetComponent<Renderer>().material.color = definition.placeholderColor;
			return placeholder;
		}

		public bool Overclock(string constructId)
		{
			ActiveConstruct active = _activeConstructs.FirstOrDefault(a => a.definition.constructId == constructId);
			if (active == null || active.state != ConstructState.Active || _resourcePool == null)
			{
				return false;
			}

			float mpCost = active.definition.overclockMPCost;
			float kineticCost = active.definition.overclockKineticEnergyCost;

			if (_resourcePool.CurrentMP < mpCost || _resourcePool.KineticEnergy < kineticCost)
			{
				return false;
			}

			if (!_resourcePool.ConsumeMP(mpCost) || !_resourcePool.ConsumeKineticEnergy(kineticCost))
			{
				return false;
			}

			OnConstructOverclocked?.Invoke(constructId, active.definition.overclockAbilityId);
			return true;
		}

		public void Deconstruct(string constructId, bool forced = false)
		{
			ActiveConstruct active = _activeConstructs.FirstOrDefault(a => a.definition.constructId == constructId);
			if (active == null)
			{
				return;
			}

			active.state = ConstructState.Deconstructing;

			if (active.definition.refundThreadsOnDeconstruct)
			{
				_threadInventory.RefundThreads(active.definition.requiredThreads);
			}

			if (active.spawnedActor != null)
			{
				Destroy(active.spawnedActor);
			}

			_activeConstructs.Remove(active);
			OnConstructDeconstructed?.Invoke(constructId, forced);
		}

		private void DeconstructAll(bool forced)
		{
			List<string> ids = _activeConstructs.Select(a => a.definition.constructId).ToList();
			foreach (string id in ids)
			{
				Deconstruct(id, forced);
			}
		}

		private void Update()
		{
			if (_activeConstructs.Count == 0 || _resourcePool == null)
			{
				return;
			}

			float totalDrain = 0f;
			foreach (ActiveConstruct active in _activeConstructs)
			{
				totalDrain += active.definition.maintenanceMPPerSecond * Time.deltaTime;
			}

			if (totalDrain > 0f)
			{
				_resourcePool.ConsumeMP(totalDrain);
			}

			if (_resourcePool.CurrentMP <= 0f)
			{
				DeconstructAll(forced: true);
			}
		}
	}
}
