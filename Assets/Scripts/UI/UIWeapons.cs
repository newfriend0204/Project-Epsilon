using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon {
    public class UIWeapons : MonoBehaviour {
	    public Image WeaponIcon;
	    public TextMeshProUGUI WeaponName;
        public TextMeshProUGUI SidearmName;
        public TextMeshProUGUI PrimaryName;
        public TextMeshProUGUI ClipAmmo;
        public Image AmmoProgress;
	    public GameObject NoAmmoGroup;
		public Image SidearmIcon;
		public Image PrimaryIcon;
        public Sprite M1911Icon;
		public Sprite SMGIcon;
		public Sprite AK47Icon;
        public Sprite RemingtonM870Icon;
        public Sprite SearchIcon;
        public Sprite TransparentIcon;

        private Weapon _weapon;
	    private int _lastClipAmmo;
	    private int _lastRemainingAmmo;
        private Weapons weaponsScript;

        public void UpdateWeapons(Player weapons) {
            weaponsScript = weapons.Weapons;
		    SetWeapon(weapons.Weapons.CurrentWeapon);

            if (weaponsScript.currentSidearm == EWeaponName.M1911) {
                SidearmIcon.sprite = M1911Icon;
                SidearmName.text = "M1911";
            } else if (weaponsScript.currentSidearm == EWeaponName.SMG) {
                SidearmIcon.sprite = SMGIcon;
                SidearmName.text = "SMG";
            } else {
                SidearmIcon.sprite = TransparentIcon;
                SidearmName.text = "";
            }
            if (weaponsScript.currentPrimary == EWeaponName.AK47) {
                PrimaryIcon.sprite = AK47Icon;
                PrimaryName.text = "AK47";
            } else if (weaponsScript.currentPrimary == EWeaponName.RemingtonM870) {
                PrimaryIcon.sprite = RemingtonM870Icon;
                PrimaryName.text = "Remington M870";
            } else {
                PrimaryIcon.sprite = TransparentIcon;
                PrimaryName.text = "";
            }

			if (weaponsScript.currentWeapon == weaponsScript.currentPrimary) {
                Color color = PrimaryIcon.color;
                color.a = 1f;
                PrimaryIcon.color = color;
                color = PrimaryName.color;
                color.a = 1f;
                PrimaryName.color = color;

                color = SidearmIcon.color;
                color.a = 0.2f;
                SidearmIcon.color = color;
                color = SidearmName.color;
                color.a = 0.2f;
                SidearmName.color = color;
            } else if (weaponsScript.currentWeapon == weaponsScript.currentSidearm) {
                Color color = SidearmIcon.color;
                color.a = 1f;
                SidearmIcon.color = color;
                color = SidearmName.color;
                color.a = 1f;
                SidearmName.color = color;

                color = PrimaryIcon.color;
                color.a = 0.2f;
                PrimaryIcon.color = color;
                color = PrimaryName.color;
                color.a = 0.2f;
                PrimaryName.color = color;
            }

            if (_weapon == null)
			    return;

		    UpdateAmmoProgress();

            if (_weapon.ClipAmmo == _lastClipAmmo && _weapon.allAmmo == _lastRemainingAmmo)
                return;

            ClipAmmo.text = _weapon.ClipAmmo.ToString();
            if (weaponsScript.currentWeapon == EWeaponName.Search) {
                ClipAmmo.text = "";
            }

            NoAmmoGroup.SetActive(_weapon.ClipAmmo == 0 && _weapon.allAmmo == 0);

            _lastClipAmmo = _weapon.ClipAmmo;
            _lastRemainingAmmo = _weapon.allAmmo;
        }

	    private void SetWeapon(Weapon weapon) {
		    _weapon = weapon;

            if (weapon.WeaponName == EWeaponName.M1911) {
                WeaponIcon.sprite = M1911Icon;
                WeaponName.text = "M1911";
            }
            if (weapon.WeaponName == EWeaponName.SMG) {
                WeaponIcon.sprite = SMGIcon;
                WeaponName.text = "SMG";
            }
            if (weapon.WeaponName == EWeaponName.AK47) {
                WeaponIcon.sprite = AK47Icon;
                WeaponName.text = "AK47";
            }
            if (weapon.WeaponName == EWeaponName.RemingtonM870) {
                WeaponIcon.sprite = RemingtonM870Icon;
                WeaponName.text = "Remington M870";
            }
            if (weapon.WeaponName == EWeaponName.Search) {
                WeaponIcon.sprite = SearchIcon;
                WeaponName.text = "Å½»ö";
            }
        }

	    private void UpdateAmmoProgress() {
		    if (_weapon.IsReloading) {
			    AmmoProgress.fillAmount = _weapon.GetReloadProgress();
		    } else {
			    AmmoProgress.fillAmount = _weapon.ClipAmmo / (float)_weapon.MaxClipAmmo;
		    }
	    }
	}
}
