using Fusion;
using UnityEngine;

namespace ProjectEpsilon {
    public class Weapons : NetworkBehaviour {
		public Animator Animator;
	    public Transform FireTransform;
	    public AudioSource SwitchSound;
		public AudioSource SearchSound;
		public AudioClip SearchStartSound;
		public AudioClip SearchEndSound;

        public bool IsSwitching => _switchTimer.ExpiredOrNotRunning(Runner) == false;

	    [Networked, HideInInspector]
	    public Weapon CurrentWeapon { get; set; }
	    [HideInInspector]
	    public Weapon[] AllWeapons;

        [Networked]
        private TickTimer _switchTimer { get; set; }
        [Networked]
        internal TickTimer weaponTimer { get; set; }
        [Networked]
	    internal Weapon _pendingWeapon { get; set; }

	    private Weapon _visibleWeapon;
        private bool _isCollectedPrimary = false;
		private bool _isCollectedSidearm = false;
		private float _saveTakeInTime;
		private float _saveTakeOutTime;
        [Networked]
        internal EWeaponName previousSearchWeapon { get; set; }
        [Networked]
        internal EWeaponName currentPrimary { get; set; }
        [Networked]
        internal EWeaponName currentSidearm { get; set; }
        [Networked]
        internal EWeaponName currentWeapon { get; set; }

        public void Fire(bool justPressed) {
			if (CurrentWeapon == null || IsSwitching)
				return;

			CurrentWeapon.Fire(FireTransform.position, FireTransform.forward, justPressed);
		}

	    public void Reload() {
		    if (CurrentWeapon == null || IsSwitching)
				return;

		    CurrentWeapon.Reload();
	    }

	    public void SwitchWeapon(int slot) {
			var newWeapon = GetWeapon(currentWeapon);
            if (slot == 0) {
                newWeapon = GetWeapon(EWeaponName.Search);
                if (previousSearchWeapon != EWeaponName.Search && CurrentWeapon != GetWeapon(EWeaponName.Search))
					previousSearchWeapon = CurrentWeapon.WeaponName;
            } else if (slot == 1) {
				newWeapon = GetWeapon(currentSidearm);
				if (!_isCollectedSidearm)
					return;
			} else if (slot == 2) {
				newWeapon = GetWeapon(currentPrimary);
				if (!_isCollectedPrimary)
					return;
			}
			if (newWeapon == null) {
				return;
			}
			if (newWeapon == CurrentWeapon && _pendingWeapon == null) {
                if (CurrentWeapon == GetWeapon(EWeaponName.Search) && previousSearchWeapon != EWeaponName.Search) {
					newWeapon = GetWeapon(previousSearchWeapon);
				} else {
					return;
				}
			}
			if (newWeapon == _pendingWeapon) {
                return;
			}
			if (CurrentWeapon != null) {
				if (CurrentWeapon.IsReloading) {
					CurrentWeapon._fireCooldown = TickTimer.CreateFromTicks(Runner, 1);
                    CurrentWeapon.IsReloading = false;
					CurrentWeapon.ReloadingSound.Stop();
				}
			}
			if (GetComponentInParent<Player>().IsAiming) {
				CurrentWeapon.ExitADS();
			}

            if (CurrentWeapon.WeaponName == EWeaponName.Search) {
                SearchSound.clip = SearchEndSound;
                SearchSound.Play();
            }
            if (newWeapon.WeaponName == EWeaponName.Search) {
                SearchSound.clip = SearchStartSound;
                SearchSound.Play();
                GetComponentInParent<Player>().VoiceSound.clip = GetComponentInParent<Player>().SearchClips[Random.Range(0, GetComponentInParent<Player>().SearchClips.Length)];
                GetComponentInParent<Player>().VoiceSound.Play();
            }

            _pendingWeapon = newWeapon;
			_saveTakeInTime = CurrentWeapon.TakeInTime;
			_saveTakeOutTime = newWeapon.TakeOutTime;
            _switchTimer = TickTimer.CreateFromSeconds(Runner, _saveTakeInTime + _saveTakeOutTime);
            weaponTimer = TickTimer.CreateFromSeconds(Runner, _saveTakeInTime + _saveTakeOutTime + 0.1f);

            if (HasInputAuthority && Runner.IsForward) {
				CurrentWeapon.Animator.SetTrigger("Hide");
				SwitchSound.Play();
			}
	    }

	    public bool PickupWeapon(EWeaponName weaponType) {
			if (CurrentWeapon != null) {
				if (CurrentWeapon.IsReloading)
					return false;
			}

			var weapon = GetWeapon(weaponType);
			if (weapon == null)
				return false;

            if (weaponType == EWeaponName.M1911) {
                currentWeapon = EWeaponName.M1911;
                _isCollectedSidearm = true;
                currentSidearm = EWeaponName.M1911;
                SwitchWeapon(1);
            }
            if (weaponType == EWeaponName.SMG11) {
                currentWeapon = EWeaponName.SMG11;
                _isCollectedSidearm = true;
                currentSidearm = EWeaponName.SMG11;
                SwitchWeapon(1);
            }
            if (weaponType == EWeaponName.SuperShorty) {
                currentWeapon = EWeaponName.SuperShorty;
                _isCollectedSidearm = true;
                currentSidearm = EWeaponName.SuperShorty;
                SwitchWeapon(1);
            }
            if (weaponType == EWeaponName.AK47) {
                currentWeapon = EWeaponName.AK47;
                _isCollectedPrimary = true;
				currentPrimary = EWeaponName.AK47;
                SwitchWeapon(2);
            }
            if (weaponType == EWeaponName.RemingtonM870) {
                currentWeapon = EWeaponName.RemingtonM870;
                _isCollectedPrimary = true;
                currentPrimary = EWeaponName.RemingtonM870;
                SwitchWeapon(2);
            }
            if (weaponType == EWeaponName.MP5) {
                currentWeapon = EWeaponName.MP5;
                _isCollectedPrimary = true;
                currentPrimary = EWeaponName.MP5;
                SwitchWeapon(2);
            }
            weapon.ClipAmmo = Random.Range(weapon.StartAmmo / 3 * 2, weapon.StartAmmo + 1);
			return true;
        }

	    public Weapon GetWeapon(EWeaponName weaponType) {
			for (int i = 0; i < AllWeapons.Length; ++i) {
				if (AllWeapons[i].WeaponName == weaponType)
					return AllWeapons[i];
			}

			return default;
	    }

	    public override void Spawned() {
            CurrentWeapon = AllWeapons[0];
			_isCollectedSidearm = true;
			currentWeapon = EWeaponName.M1911;
			currentSidearm = EWeaponName.M1911;
        }

	    public override void FixedUpdateNetwork() {
		    TryActivatePendingWeapon();
        }

	    public override void Render() {
		    if (_visibleWeapon == CurrentWeapon)
			    return;

			int currentWeaponID = -1;

		    for (int i = 0; i < AllWeapons.Length; i++) {
			    var weapon = AllWeapons[i];
			    if (weapon == CurrentWeapon) {
					currentWeaponID = i;
					weapon.ToggleVisibility(true);
					weapon.GetComponent<Weapon>().enabled = true;
			    } else {
					weapon.ToggleVisibility(false);
                    weapon.GetComponent<Weapon>().enabled = false;
                }
		    }

		    _visibleWeapon = CurrentWeapon;

			if (CurrentWeapon.Type == EWeaponType.Search)
				currentWeaponID = -1;
			Animator.SetFloat("WeaponID", currentWeaponID);
	    }

	    private void Awake() {
		    AllWeapons = GetComponentsInChildren<Weapon>();
	    }

		private void TryActivatePendingWeapon() {
			if (IsSwitching == false || _pendingWeapon == null)
				return;

			if (_switchTimer.RemainingTime(Runner) > _saveTakeOutTime)
				return;

		    CurrentWeapon = _pendingWeapon;
			currentWeapon = CurrentWeapon.WeaponName;
		    _pendingWeapon = null;

		    CurrentWeapon.gameObject.SetActive(true);

		    if (HasInputAuthority && Runner.IsForward) {
			    CurrentWeapon.Animator.SetTrigger("Show");
		    }
	    }
	}
}
