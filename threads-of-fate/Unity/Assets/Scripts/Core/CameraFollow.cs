using System.Collections.Generic;
using UnityEngine;

namespace ThreadsOfFate.Core
{
	// Simple offset follow-cam, no collision/orbit handling. Retargets
	// automatically on CharacterSwitchController.OnActiveCharacterChanged
	// so the camera always trails whichever character you're controlling.
	public class CameraFollow : MonoBehaviour
	{
		[SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
		[SerializeField] private float followLerpSpeed = 6f;

		private Transform _target;

		public void Initialize(CharacterSwitchController switchController, Dictionary<CharacterId, Transform> targetsById)
		{
			switchController.OnActiveCharacterChanged += id => SetTarget(targetsById.TryGetValue(id, out Transform t) ? t : null);
			SetTarget(targetsById.TryGetValue(switchController.ActiveCharacter, out Transform initial) ? initial : null);
		}

		private void SetTarget(Transform target)
		{
			_target = target;
		}

		private void LateUpdate()
		{
			if (_target == null)
			{
				return;
			}

			Vector3 desiredPosition = _target.position + offset;
			transform.position = Vector3.Lerp(transform.position, desiredPosition, followLerpSpeed * Time.deltaTime);
			transform.LookAt(_target.position + Vector3.up * 1.2f);
		}
	}
}
