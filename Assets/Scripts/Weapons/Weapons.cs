using Fusion;
using UnityEngine;

namespace ProjectEpsilon {
    public class Weapons : NetworkBehaviour {
		public Animator Animator;
	    public Transform FireTransform;
	    public float WeaponSwitchTime = 1f;
	    public AudioSource SwitchSound;

	    public bool IsSwitching => _switchTimer.ExpiredOrNotRunning(Runner) == false;

	    [Networked, HideInInspector]
	    public Weapon CurrentWeapon { get; set; }
	    [HideInInspector]
	    public Weapon[] AllWeapons;

	    [Networked]
	    private TickTimer _switchTimer { get; set; }
	    [Networked]
	    private Weapon _pendingWeapon { get; set; }

	    private Weapon _visibleWeapon;
		private bool _isCollectedPrimary = false;
		private bool _isCollectedSidearm = false;
		private EWeaponName _currentPrimary;
		private EWeaponName _currentSidearm;
		private EWeaponName _currentWeapon;
		private EWeaponName _previousSearchWeapon;

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
			var newWeapon = GetWeapon(_currentWeapon);
            if (slot == 0) {
                newWeapon = GetWeapon(EWeaponName.Search);
				if (_previousSearchWeapon != EWeaponName.Search && CurrentWeapon != GetWeapon(EWeaponName.Search))
					_previousSearchWeapon = CurrentWeapon.WeaponName;
            } else if (slot == 1) {
				newWeapon = GetWeapon(_currentSidearm);
				if (!_isCollectedSidearm)
					return;
			} else if (slot == 2) {
				newWeapon = GetWeapon(_currentPrimary);
				if (!_isCollectedPrimary)
					return;
			}
			if (newWeapon == null) {
				return;
			}
			if (newWeapon == CurrentWeapon && _pendingWeapon == null) {
                if (CurrentWeapon == GetWeapon(EWeaponName.Search) && _previousSearchWeapon != EWeaponName.Search) {
					newWeapon = GetWeapon(_previousSearchWeapon);
				} else {
					return;
				}
			}
			if (newWeapon == _pendingWeapon) {
                return;
			}
			if (CurrentWeapon != null) {
				if (CurrentWeapon.IsReloading) {
					CurrentWeapon._fireCooldown = TickTimer.None;
					CurrentWeapon.IsReloading = false;
					CurrentWeapon.ReloadingSound.Stop();
				}
			}
			if (GetComponentInParent<Player>().isAiming) {
				CurrentWeapon.ExitADS();
			}

			_pendingWeapon = newWeapon;
			_switchTimer = TickTimer.CreateFromSeconds(Runner, WeaponSwitchTime);

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
                _currentWeapon = EWeaponName.M1911;
                _isCollectedSidearm = true;
                _currentSidearm = EWeaponName.M1911;
                SwitchWeapon(1);
            }
            if (weaponType == EWeaponName.SMG) {
                _currentWeapon = EWeaponName.SMG;
                _isCollectedSidearm = true;
                _currentSidearm = EWeaponName.SMG;
                SwitchWeapon(1);
            }
            if (weaponType == EWeaponName.AK47) {
                _currentWeapon = EWeaponName.AK47;
                _isCollectedPrimary = true;
				_currentPrimary = EWeaponName.AK47;
                SwitchWeapon(2);
            }
            if (weaponType == EWeaponName.RemingtonM870) {
                _currentWeapon = EWeaponName.RemingtonM870;
                _isCollectedPrimary = true;
				_currentPrimary = EWeaponName.RemingtonM870;
                SwitchWeapon(2);
            }

			if (weapon.IsCollected) {
                weapon.ClipAmmo = weapon.StartAmmo;
            } else {
				weapon.IsCollected = true;
			}
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
            if (HasStateAuthority) {
                CurrentWeapon = AllWeapons[0];
                CurrentWeapon.IsCollected = true;
				_isCollectedSidearm = true;
            }
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

			if (_switchTimer.RemainingTime(Runner) > WeaponSwitchTime * 0.5f)
				return;

		    CurrentWeapon = _pendingWeapon;
		    _pendingWeapon = null;

		    CurrentWeapon.gameObject.SetActive(true);

		    if (HasInputAuthority && Runner.IsForward) {
			    CurrentWeapon.Animator.SetTrigger("Show");
		    }
	    }
	}
}
