using UnityEngine;

namespace ThreadsOfFate.World
{
	// Minimal spin animation for the turbine station. Starts disabled;
	// ElementalTrigger enables it once Green/DriveTurbine is applied.
	public class Rotator : MonoBehaviour
	{
		public float degreesPerSecond = 90f;

		private void Update()
		{
			transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.World);
		}
	}
}
