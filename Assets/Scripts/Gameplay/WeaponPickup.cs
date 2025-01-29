using Fusion;
using UnityEngine;

namespace ProjectEpsilon {
    public class WeaponPickup : NetworkBehaviour {
		public EWeaponName Type;
		public float Cooldown = 30f;
		public LayerMask LayerMask;
		public GameObject ActiveObject;
		public GameObject InactiveObject;
		public Outline OutlineScript;

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
        }

		void Update() {
			if (HasStateAuthority && FindObjectOfType<Player>().isSearching) {
				Player _playerobject = FindObjectOfType<Player>();
				float distance = Vector3.Distance(transform.position, _playerobject.transform.position);

				if (_playerobject.isSearching) {
					if (distance <= 15f)
						OutlineScript.enabled = true;
					else
                        OutlineScript.enabled = false;
				}
			}
			else {
                OutlineScript.enabled = false;
            }
		}

		public override void Render() {
			ActiveObject.SetActive(IsActive);
			InactiveObject.SetActive(IsActive == false);
		}

		public void AcquireWeapon(GameObject player) {
            var weapons = player.GetComponentInParent<Weapons>();
            if (weapons != null && weapons.PickupWeapon(Type)) {
                _activationTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);
            }
        }
	}
}
