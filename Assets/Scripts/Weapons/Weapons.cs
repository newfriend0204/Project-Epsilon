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

	    public void SwitchWeapon(EWeaponType weaponType) {
			var newWeapon = GetWeapon(weaponType);

		    if (newWeapon == null || newWeapon.IsCollected == false)
				return;
		    if (newWeapon == CurrentWeapon && _pendingWeapon == null)
			    return;
		    if (newWeapon == _pendingWeapon)
			    return;

		    if (CurrentWeapon.IsReloading)
				return;

		    _pendingWeapon = newWeapon;
		    _switchTimer = TickTimer.CreateFromSeconds(Runner, WeaponSwitchTime);

		    if (HasInputAuthority && Runner.IsForward)
		    {
			    CurrentWeapon.Animator.SetTrigger("Hide");
			    SwitchSound.Play();
		    }
	    }

	    public bool PickupWeapon(EWeaponType weaponType) {
		    if (CurrentWeapon.IsReloading)
				return false;

			var weapon = GetWeapon(weaponType);
			if (weapon == null)
				return false;

			if (weapon.IsCollected) {
				weapon.AddAmmo(weapon.StartAmmo - weapon.RemainingAmmo);
			}
			else {
				weapon.IsCollected = true;
			}

			SwitchWeapon(weaponType);

			return true;
	    }

	    public Weapon GetWeapon(EWeaponType weaponType) {
			for (int i = 0; i < AllWeapons.Length; ++i) {
				if (AllWeapons[i].Type == weaponType)
					return AllWeapons[i];
			}

			return default;
	    }

	    public override void Spawned() {
		    if (HasStateAuthority) {
			    CurrentWeapon = AllWeapons[0];
			    CurrentWeapon.IsCollected = true;
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
			    } else {
					weapon.ToggleVisibility(false);
				}
		    }

		    _visibleWeapon = CurrentWeapon;

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
