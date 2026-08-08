using UnityEngine;

namespace ThreadsOfFate.Chervil
{
	// Marker for Umbral Decoy (Phantom x2): a real hook a future enemy-AI
	// targeting system can query ("prefer the nearest/highest-priority
	// AggroDecoy over the player"), without this project building a full AI
	// system yet — no enemy AI exists anywhere in the codebase to redirect.
	public class AggroDecoy : MonoBehaviour
	{
		[Tooltip("Higher = more attractive to an aggro search than a lower-priority decoy or the player's own baseline.")]
		public float priority = 10f;
	}
}
