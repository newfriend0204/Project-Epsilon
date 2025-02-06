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
        public GameObject InfoObject;
        public Outline OutlineScript;
        public AudioSource SearchFound;

        public bool IsActive => _activationTimer.ExpiredOrNotRunning(Runner);

        [Networked]
        private TickTimer _activationTimer { get; set; }

        private static Collider[] _colliders = new Collider[8];
        private bool _isSearched = false;

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
                        if (_isSearched == false) {
                            _isSearched = true;
                            SearchFound.transform.position = _playerObject.transform.position;
                            SearchFound.Play();
                        }
                        InfoObject.SetActive(true);
                        OutlineScript.enabled = true;
                    } else {
                        _isSearched = false;
                        InfoObject.SetActive(false);
                        OutlineScript.enabled = false;
                    }
                }
            } else {
                _isSearched = false;
                OutlineScript.enabled = false;
                InfoObject.SetActive(false);
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
                    ammos.ammo45ACP += Random.Range(10, 21);
                    break;
                case BulletName.bullet7_62mm:
                    ammos.ammo7_62mm += Random.Range(15, 31);
                    break;
                case BulletName.bullet12Gauge:
                    ammos.ammo12Gauge += Random.Range(5, 11);
                    break;
            }
            _activationTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);
        }
    }
}
