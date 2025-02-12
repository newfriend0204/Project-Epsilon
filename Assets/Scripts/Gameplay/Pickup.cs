using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectEpsilon {
    public enum BulletName {
        ammo45ACP,
        ammo7_62mm,
        ammo12Gauge,
    };

    public class Pickup : NetworkBehaviour {
        public EWeaponName Type;
        public BulletName Bullet;
        public float Cooldown = 5f;
        public LayerMask LayerMask;
        public GameObject ActiveObject;
        public GameObject Ammo45ACP;
        public GameObject Ammo7_62mm;
        public GameObject Ammo12Gauge;
        public GameObject M1911;
        public GameObject SMG11;
        public GameObject SuperShorty;
        public GameObject AK47;
        public GameObject RemingtonM870;
        public GameObject MP5;
        public GameObject InactiveObject;
        public GameObject InfoObject;
        public Outline OutlineScript;
        public AudioSource SearchFound;

        public bool IsActive => _activationTimer.ExpiredOrNotRunning(Runner);

        [Networked]
        private TickTimer _activationTimer { get; set; }

        private static Collider[] _colliders = new Collider[8];
        private bool _isSearched = false;
        private int _currentMode = 0;
        private int _spawnMode = 0;
        private bool _isGaming = false;

        public override void Spawned() {
            ActiveObject.SetActive(IsActive);
            InactiveObject.SetActive(IsActive == false);
            _spawnMode = Random.Range(1, 4);
            _currentMode = Random.Range(1, 10);
            transform.rotation = Quaternion.Euler(0, Random.Range(1, 360), 0);
            Ammo45ACP.SetActive(true);
            Ammo7_62mm.SetActive(true);
            Ammo12Gauge.SetActive(true);
            M1911.SetActive(true);
            SMG11.SetActive(true);
            SuperShorty.SetActive(true);
            AK47.SetActive(true);
            RemingtonM870.SetActive(true);
            MP5.SetActive(true);
        }

        public override void FixedUpdateNetwork() {
            if (IsActive == false)
                return;
        }

        private void OnDrawGizmos() {
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }

        void Update() {
            Ammo45ACP.SetActive(true);
            Ammo7_62mm.SetActive(true);
            Ammo12Gauge.SetActive(true);
            M1911.SetActive(true);
            SMG11.SetActive(true);
            SuperShorty.SetActive(true);
            AK47.SetActive(true);
            RemingtonM870.SetActive(true);
            MP5.SetActive(true);

            switch (_currentMode) {
                case 1:
                    Ammo7_62mm.SetActive(false);
                    Ammo12Gauge.SetActive(false);
                    M1911.SetActive(false);
                    SMG11.SetActive(false);
                    SuperShorty.SetActive(false);
                    AK47.SetActive(false);
                    RemingtonM870.SetActive(false);
                    MP5.SetActive(false);
                    Bullet = BulletName.ammo45ACP;
                    OutlineScript = Ammo45ACP.transform.Find("Ammo45ACP")?.GetComponent<Outline>();
                    InfoObject = Ammo45ACP.transform.Find("Text")?.gameObject;
                    break;
                case 2:
                    Ammo45ACP.SetActive(false);
                    Ammo12Gauge.SetActive(false);
                    M1911.SetActive(false);
                    SMG11.SetActive(false);
                    SuperShorty.SetActive(false);
                    AK47.SetActive(false);
                    RemingtonM870.SetActive(false);
                    MP5.SetActive(false);
                    Bullet = BulletName.ammo7_62mm;
                    OutlineScript = Ammo7_62mm.transform.Find("Ammo7_62mm")?.GetComponent<Outline>();
                    InfoObject = Ammo7_62mm.transform.Find("Text")?.gameObject;
                    break;
                case 3:
                    Ammo45ACP.SetActive(false);
                    Ammo7_62mm.SetActive(false);
                    M1911.SetActive(false);
                    SMG11.SetActive(false);
                    SuperShorty.SetActive(false);
                    AK47.SetActive(false);
                    RemingtonM870.SetActive(false);
                    MP5.SetActive(false);
                    Bullet = BulletName.ammo12Gauge;
                    OutlineScript = Ammo12Gauge.transform.Find("Ammo12Gauge")?.GetComponent<Outline>();
                    InfoObject = Ammo12Gauge.transform.Find("Text")?.gameObject;
                    break;
                case 4:
                    Ammo45ACP.SetActive(false);
                    Ammo7_62mm.SetActive(false);
                    Ammo12Gauge.SetActive(false);
                    SMG11.SetActive(false);
                    SuperShorty.SetActive(false);
                    AK47.SetActive(false);
                    RemingtonM870.SetActive(false);
                    MP5.SetActive(false);
                    Type = EWeaponName.M1911;
                    OutlineScript = M1911.transform.Find("M1911")?.GetComponent<Outline>();
                    InfoObject = M1911.transform.Find("Text")?.gameObject;
                    break;
                case 5:
                    Ammo45ACP.SetActive(false);
                    Ammo7_62mm.SetActive(false);
                    Ammo12Gauge.SetActive(false);
                    M1911.SetActive(false);
                    SuperShorty.SetActive(false);
                    AK47.SetActive(false);
                    RemingtonM870.SetActive(false);
                    MP5.SetActive(false);
                    Type = EWeaponName.SMG11;
                    OutlineScript = SMG11.transform.Find("SMG11")?.GetComponent<Outline>();
                    InfoObject = SMG11.transform.Find("Text")?.gameObject;
                    break;
                case 6:
                    Ammo45ACP.SetActive(false);
                    Ammo7_62mm.SetActive(false);
                    Ammo12Gauge.SetActive(false);
                    M1911.SetActive(false);
                    SMG11.SetActive(false);
                    AK47.SetActive(false);
                    RemingtonM870.SetActive(false);
                    MP5.SetActive(false);
                    Type = EWeaponName.SuperShorty;
                    OutlineScript = SuperShorty.transform.Find("SuperShorty")?.GetComponent<Outline>();
                    InfoObject = SuperShorty.transform.Find("Text")?.gameObject;
                    break;
                case 7:
                    Ammo45ACP.SetActive(false);
                    Ammo7_62mm.SetActive(false);
                    Ammo12Gauge.SetActive(false);
                    M1911.SetActive(false);
                    SMG11.SetActive(false);
                    SuperShorty.SetActive(false);
                    RemingtonM870.SetActive(false);
                    MP5.SetActive(false);
                    Type = EWeaponName.AK47;
                    OutlineScript = AK47.transform.Find("AK47")?.GetComponent<Outline>();
                    InfoObject = AK47.transform.Find("Text")?.gameObject;
                    break;
                case 8:
                    Ammo45ACP.SetActive(false);
                    Ammo7_62mm.SetActive(false);
                    Ammo12Gauge.SetActive(false);
                    M1911.SetActive(false);
                    SMG11.SetActive(false);
                    SuperShorty.SetActive(false);
                    AK47.SetActive(false);
                    MP5.SetActive(false);
                    Type = EWeaponName.RemingtonM870;
                    OutlineScript = RemingtonM870.transform.Find("RemingtonM870")?.GetComponent<Outline>();
                    InfoObject = RemingtonM870.transform.Find("Text")?.gameObject;
                    break;
                case 9:
                    Ammo45ACP.SetActive(false);
                    Ammo7_62mm.SetActive(false);
                    Ammo12Gauge.SetActive(false);
                    M1911.SetActive(false);
                    SMG11.SetActive(false);
                    SuperShorty.SetActive(false);
                    AK47.SetActive(false);
                    RemingtonM870.SetActive(false);
                    Type = EWeaponName.MP5;
                    OutlineScript = MP5.transform.Find("MP5")?.GetComponent<Outline>();
                    InfoObject = MP5.transform.Find("Text")?.gameObject;
                    break;
            }

            if (HasStateAuthority && FindObjectOfType<Player>().isSearching) {
                Player _playerObject = FindObjectOfType<Player>();
                float distance = Vector3.Distance(transform.position, _playerObject.transform.position);

                Vector3 direction = new Vector3(_playerObject.transform.position.x, _playerObject.transform.position.y + 1.737276f, _playerObject.transform.position.z) - transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0);
                InfoObject.transform.rotation = Quaternion.Slerp(InfoObject.transform.rotation, rotation, Time.deltaTime * 5f);

                if (_playerObject.isSearching) {
                    if (distance <= 10) {
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
                OutlineScript.enabled = false;
                InfoObject.SetActive(false);
            }
        }

        public override void Render() {
            ActiveObject.SetActive(IsActive);
            InactiveObject.SetActive(IsActive == false);
        }

        public void AcquireWeapon(GameObject player, int mode) {
            if (mode == 1) {
                var weapons = player.GetComponentInParent<Weapons>();
                if (weapons != null && weapons.PickupWeapon(Type)) {
                    Cooldown = Random.Range(3, 5);
                    _activationTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);
                }
            } else if (mode == 2) {
                var ammos = player.GetComponentInParent<Player>();
                switch (Bullet) {
                    case BulletName.ammo45ACP:
                        ammos.ammo45ACP += Random.Range(5, 11);
                        break;
                    case BulletName.ammo7_62mm:
                        ammos.ammo7_62mm += Random.Range(10, 16);
                        break;
                    case BulletName.ammo12Gauge:
                        ammos.ammo12Gauge += Random.Range(2, 6);
                        break;
                }
                _activationTimer = TickTimer.CreateFromSeconds(Runner, Cooldown);
            }
            _currentMode = Random.Range(1, 10);
            transform.rotation = Quaternion.Euler(0, Random.Range(1, 360), 0);
        }

        void ChangeMode() {
            int[] modes;
            switch (_spawnMode) {
                case 1:
                    modes = new int[] { 1, 1, 2, 2, 3, 3, 4, 5, 6, 7, 8, 9 };
                    _currentMode = modes[Random.Range(0, modes.Length)];
                    break;
                case 2:
                    modes = new int[] { 1, 2, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9 };
                    _currentMode = modes[Random.Range(0, modes.Length)];
                    break;
                case 3:
                    modes = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    _currentMode = modes[Random.Range(0, modes.Length)];
                    break;
            }
        }
    }
}
