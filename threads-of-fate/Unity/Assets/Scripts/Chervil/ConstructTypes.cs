using UnityEngine;

namespace ThreadsOfFate.Chervil
{
	public enum ConstructState
	{
		Inactive,
		Active,
		Deconstructing
	}

	// Per-instance runtime state for a deployed construct. Holds a reference
	// to the shared ConstructDefinition asset (definitions are immutable data,
	// so no copy is needed the way the UE data-table row is copied).
	public class ActiveConstruct
	{
		public ConstructDefinition definition;
		public ConstructState state = ConstructState.Inactive;
		public GameObject spawnedActor;
	}
}
