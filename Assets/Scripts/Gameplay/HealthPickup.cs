using Fusion;
using UnityEngine;

namespace ProjectEpsilon {
    public class HealthPickup : NetworkBehaviour {
		public float Heal = 50f;
		public float Radius = 1f;
		public float Cooldown = 30f;
		public LayerMask  LayerMask;
		public GameObject ActiveObject;
		public GameObject InactiveObject;

		public bool IsActive => _activationTimer.ExpiredOrNotRunning(Runner);

		[Networked]
		private TickTimer _activationTimer { get; set; }

		private static Collider[] _colliders = new Collider[8];

		public override void Spawned() {
			ActiveObject.SetActive(IsActive);
			InactiveObject.SetActive(IsActive == false);
		}

		public override void FixedUpdateNetwork() {
			if (IsActive == false)
				return;

			int collisions = Runner.GetPhysicsScene().OverlapSphere(transform.position + Vector3.up, Radius, _colliders, LayerMask, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < collisions; i++) {
				var health = _colliders[i].GetComponentInParent<Health>();
				if (health != null && health.AddHealth(Heal)) {
					_activationTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);
					break;
				}
			}
		}

		public override void Render() {
			ActiveObject.SetActive(IsActive);
			InactiveObject.SetActive(IsActive == false);
		}

		private void OnDrawGizmos() {
			Gizmos.DrawWireSphere(transform.position + Vector3.up, Radius);
		}
	}
}
