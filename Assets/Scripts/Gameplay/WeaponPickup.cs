using Fusion;
using UnityEngine;

namespace ProjectEpsilon {
    public class WeaponPickup : NetworkBehaviour {
		public EWeaponName Type;
		public float Cooldown = 30f;
		public LayerMask LayerMask;
		public GameObject ActiveObject;
		public GameObject InactiveObject;
        public GameObject InfoObject;
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
				Player _playerObject = FindObjectOfType<Player>();
				float distance = Vector3.Distance(transform.position, _playerObject.transform.position);

                Vector3 direction = new Vector3(_playerObject.transform.position.x, _playerObject.transform.position.y + 1.737276f, _playerObject.transform.position.z) - transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0);
                InfoObject.transform.rotation = Quaternion.Slerp(InfoObject.transform.rotation, rotation, Time.deltaTime * 5f);

                if (_playerObject.isSearching) {
                    if (distance <= 15f) {
                        InfoObject.SetActive(true);
                        OutlineScript.enabled = true;
                    } else {
                        InfoObject.SetActive(false);
                        OutlineScript.enabled = false;
                    }
                }
            }
			else {
                OutlineScript.enabled = false;
                InfoObject.SetActive(false);
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
