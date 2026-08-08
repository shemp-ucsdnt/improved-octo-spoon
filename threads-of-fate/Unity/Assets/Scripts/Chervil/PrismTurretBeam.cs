using System.Collections.Generic;
using UnityEngine;

namespace ThreadsOfFate.Chervil
{
	// Continuous raycast "laser" for the Prism Turret puzzle: bounces off
	// PrismMirror surfaces (mirror reflection) until it hits an
	// IPrismReceiver (solves the puzzle), runs out of bounces, or misses.
	// Not attached by ConstructDeploymentManager itself (that stays generic
	// across every construct type) — level/test-setup code adds this to the
	// Prism Turret's spawned visual after deployment.
	[RequireComponent(typeof(LineRenderer))]
	public class PrismTurretBeam : MonoBehaviour
	{
		[SerializeField] private int maxBounces = 4;
		[SerializeField] private float maxSegmentDistance = 50f;

		private LineRenderer _lineRenderer;
		private IPrismReceiver _currentReceiver;

		private void Awake()
		{
			_lineRenderer = GetComponent<LineRenderer>();
			_lineRenderer.startWidth = 0.05f;
			_lineRenderer.endWidth = 0.05f;
		}

		private void Update()
		{
			TraceBeam();
		}

		private void TraceBeam()
		{
			var points = new List<Vector3> { transform.position };
			Vector3 origin = transform.position;
			Vector3 direction = transform.forward;
			IPrismReceiver hitReceiver = null;

			for (int bounce = 0; bounce < maxBounces; bounce++)
			{
				if (!Physics.Raycast(origin, direction, out RaycastHit hit, maxSegmentDistance))
				{
					points.Add(origin + direction * maxSegmentDistance);
					break;
				}

				points.Add(hit.point);

				PrismMirror mirror = hit.collider.GetComponent<PrismMirror>();
				if (mirror != null)
				{
					direction = Vector3.Reflect(direction, hit.normal);
					origin = hit.point + direction * 0.01f;
					continue;
				}

				hitReceiver = hit.collider.GetComponent<IPrismReceiver>();
				break;
			}

			_lineRenderer.positionCount = points.Count;
			_lineRenderer.SetPositions(points.ToArray());
			UpdateReceiver(hitReceiver);
		}

		private void UpdateReceiver(IPrismReceiver hitReceiver)
		{
			if (hitReceiver == _currentReceiver)
			{
				return;
			}

			_currentReceiver?.OnBeamLost();
			_currentReceiver = hitReceiver;
			_currentReceiver?.OnBeamHit();
		}

		private void OnDestroy()
		{
			_currentReceiver?.OnBeamLost();
		}
	}
}
