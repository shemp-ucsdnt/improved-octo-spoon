using System.Collections;
using UnityEngine;

namespace ThreadsOfFate.World
{
	// Turns "drive the turbine" into a real mechanism: raises a connected
	// bridge/platform once Green engages it.
	[RequireComponent(typeof(ElementalTrigger))]
	public class TurbinePlatform : MonoBehaviour
	{
		[SerializeField] private Transform platform;
		[SerializeField] private float raisedHeight = 2f;
		[SerializeField] private float raiseSpeed = 1.5f;

		private float _loweredY;
		private Coroutine _moveRoutine;

		private void Awake()
		{
			if (platform != null)
			{
				_loweredY = platform.position.y;
			}

			GetComponent<ElementalTrigger>().OnActivated += HandleActivated;
		}

		public void SetPlatform(Transform platformTransform)
		{
			platform = platformTransform;
			_loweredY = platformTransform.position.y;
		}

		private void HandleActivated(ElementalTrigger trigger)
		{
			if (trigger.kind != ElementalTriggerKind.DriveTurbine || platform == null)
			{
				return;
			}

			if (_moveRoutine != null)
			{
				StopCoroutine(_moveRoutine);
			}

			_moveRoutine = StartCoroutine(MoveTo(_loweredY + raisedHeight));
		}

		private IEnumerator MoveTo(float targetY)
		{
			while (Mathf.Abs(platform.position.y - targetY) > 0.01f)
			{
				Vector3 pos = platform.position;
				pos.y = Mathf.MoveTowards(pos.y, targetY, raiseSpeed * Time.deltaTime);
				platform.position = pos;
				yield return null;
			}

			Debug.Log($"[TurbinePlatform] {name} platform raised.");
		}
	}
}
