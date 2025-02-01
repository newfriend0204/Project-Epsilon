using Fusion;
using UnityEngine;

namespace ProjectEpsilon {
    public enum BulletName {
        bullet45ACP,
        bullet7_62mm,
        bullet12Gauge,
    };

    public class AmmoPickup : NetworkBehaviour {
        public BulletName Bullet;
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
            } else {
                OutlineScript.enabled = false;
            }
        }

        public override void Render() {
            ActiveObject.SetActive(IsActive);
            InactiveObject.SetActive(IsActive == false);
        }

        public void AcquireAmmo(GameObject player) {
            var ammos = player.GetComponentInParent<Player>();
            switch (Bullet) {
                case BulletName.bullet45ACP:
                    ammos.bullet45ACP += Random.Range(10, 21);
                    break;
                case BulletName.bullet7_62mm:
                    ammos.bullet7_62mm += Random.Range(15, 31);
                    break;
                case BulletName.bullet12Gauge:
                    ammos.bullet12Gauge += Random.Range(5, 11);
                    break;
            }
        }
    }
}
