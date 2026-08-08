using UnityEngine;

namespace ThreadsOfFate.Core
{
	// Generic WASD+jump movement for whichever character is currently
	// active — CharacterSwitchController enables/disables this exactly like
	// the per-character input scripts, so only the controlled character
	// moves. Movement is world-space (not camera- or facing-relative) to
	// keep this simple; the character visually turns to face its move
	// direction. Sorrel's SorrelTransformationController overrides gravity/jump/
	// speed/collider dimensions per active form via SetPhysicsOverride();
	// everyone else just uses the base values.
	[RequireComponent(typeof(CharacterController))]
	public class CharacterMovementController : MonoBehaviour
	{
		[SerializeField] private float baseMoveSpeed = 5f;
		[SerializeField] private float baseJumpHeight = 1.2f;
		[SerializeField] private float baseGravityScale = 1f;
		[SerializeField] private float baseControllerHeight = 1.8f;
		[SerializeField] private float baseControllerRadius = 0.4f;
		[SerializeField] private float gravity = -20f;

		private CharacterController _controller;
		private float _moveSpeed;
		private float _jumpHeight;
		private float _gravityScale;
		private float _verticalVelocity;

		private void Awake()
		{
			_controller = GetComponent<CharacterController>();
			ClearPhysicsOverride();
		}

		public void SetPhysicsOverride(float gravityScale, float jumpHeight, float moveSpeedMultiplier, float controllerHeight, float controllerRadius)
		{
			_gravityScale = gravityScale;
			_jumpHeight = jumpHeight;
			_moveSpeed = baseMoveSpeed * moveSpeedMultiplier;
			ApplyControllerDimensions(controllerHeight, controllerRadius);
		}

		public void ClearPhysicsOverride()
		{
			_gravityScale = baseGravityScale;
			_jumpHeight = baseJumpHeight;
			_moveSpeed = baseMoveSpeed;
			ApplyControllerDimensions(baseControllerHeight, baseControllerRadius);
		}

		private void ApplyControllerDimensions(float height, float radius)
		{
			_controller.height = height;
			_controller.radius = radius;
			_controller.center = new Vector3(0f, height * 0.5f, 0f);
		}

		private void Update()
		{
			Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
			moveInput = Vector3.ClampMagnitude(moveInput, 1f);
			Vector3 move = moveInput * _moveSpeed;

			if (_controller.isGrounded)
			{
				_verticalVelocity = -0.5f;
				if (Input.GetButtonDown("Jump"))
				{
					_verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * gravity * _gravityScale);
				}
			}
			else
			{
				_verticalVelocity += gravity * _gravityScale * Time.deltaTime;
			}

			move.y = _verticalVelocity;
			_controller.Move(move * Time.deltaTime);

			Vector3 flatMove = new Vector3(move.x, 0f, move.z);
			if (flatMove.sqrMagnitude > 0.01f)
			{
				transform.forward = flatMove.normalized;
			}
		}
	}
}
