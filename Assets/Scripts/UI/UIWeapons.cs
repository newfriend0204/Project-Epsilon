using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon {
    public class UIWeapons : MonoBehaviour {
        public GameObject SearchScreen;
	    public Image WeaponIcon;
	    public TextMeshProUGUI WeaponName;
        public TextMeshProUGUI SidearmName;
        public TextMeshProUGUI PrimaryName;
        public TextMeshProUGUI ClipAmmo;
        public TextMeshProUGUI Ammo45ACPRemain;
        public TextMeshProUGUI Ammo7_62mmRemain;
        public TextMeshProUGUI Ammo12GaugeRemain;

        public Image AmmoProgress;
	    public GameObject NoAmmoGroup;
		public Image SidearmIcon;
		public Image PrimaryIcon;
        public GameObject Ammo45ACP;
        public GameObject Ammo7_62mm;
        public GameObject Ammo12Gauge;
        public Image CurrentAmmoIcon;

        public Sprite M1911Icon;
		public Sprite SMGIcon;
		public Sprite AK47Icon;
        public Sprite RemingtonM870Icon;
        public Sprite SearchIcon;
        public Sprite Ammo45ACPIcon;
        public Sprite Ammo7_62mmIcon;
        public Sprite Ammo12GaugeIcon;
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

            Color color;

			if (weaponsScript.currentWeapon == weaponsScript.currentPrimary) {
                color = PrimaryIcon.color;
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
                color = SidearmIcon.color;
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

            SetAmmoTransparency(0.2f, Ammo45ACP);
            SetAmmoTransparency(0.2f, Ammo7_62mm);
            SetAmmoTransparency(0.2f, Ammo12Gauge);

            switch (weaponsScript.currentWeapon) {
                case EWeaponName.M1911:
                case EWeaponName.SMG:
                    SetAmmoTransparency(1f, Ammo45ACP);
                    break;
                case EWeaponName.AK47:
                    SetAmmoTransparency(1f, Ammo7_62mm);
                    break;
                case EWeaponName.RemingtonM870:
                    SetAmmoTransparency(1f, Ammo12Gauge);
                    break;
                default:
                    break;
            }

            void SetIconTransparency(GameObject icon, float alpha) {
                Image[] images = icon.GetComponentsInChildren<Image>();
                foreach (Image img in images) {
                    Color color = img.color;
                    color.a = alpha;
                    img.color = color;
                }
            }

            void SetAmmoTransparency(float alpha, GameObject ammoIcon) {
                Debug.Log(alpha, ammoIcon);
                if (ammoIcon != null) {
                    SetIconTransparency(ammoIcon, alpha);
                }

                if (ammoIcon == Ammo45ACP) {
                    Color remainColor = Ammo45ACPRemain.color;
                    remainColor.a = alpha;
                    Ammo45ACPRemain.color = remainColor;
                }

                if (ammoIcon == Ammo7_62mm) {
                    Color remainColor = Ammo7_62mmRemain.color;
                    remainColor.a = alpha;
                    Ammo7_62mmRemain.color = remainColor;
                }

                if (ammoIcon == Ammo12Gauge) {
                    Color remainColor = Ammo12GaugeRemain.color;
                    remainColor.a = alpha;
                    Ammo12GaugeRemain.color = remainColor;
                }
            }

            Ammo45ACPRemain.text = weapons.ammo45ACP.ToString();
            Ammo7_62mmRemain.text = weapons.ammo7_62mm.ToString();
            Ammo12GaugeRemain.text = weapons.ammo12Gauge.ToString();

            if (_weapon == null)
			    return;

		    UpdateAmmoProgress();

            if (_weapon.ClipAmmo == _lastClipAmmo && _weapon.allAmmo == _lastRemainingAmmo)
                return;

            ClipAmmo.text = _weapon.ClipAmmo.ToString();
            if (weaponsScript.currentWeapon == EWeaponName.Search) {
                ClipAmmo.text = "";
                SearchScreen.SetActive(true);
            } else {
                SearchScreen.SetActive(false);
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
                CurrentAmmoIcon.sprite = Ammo45ACPIcon;
            }
            if (weapon.WeaponName == EWeaponName.SMG) {
                WeaponIcon.sprite = SMGIcon;
                WeaponName.text = "SMG";
                CurrentAmmoIcon.sprite = Ammo45ACPIcon;
            }
            if (weapon.WeaponName == EWeaponName.AK47) {
                WeaponIcon.sprite = AK47Icon;
                WeaponName.text = "AK47";
                CurrentAmmoIcon.sprite = Ammo7_62mmIcon;
            }
            if (weapon.WeaponName == EWeaponName.RemingtonM870) {
                WeaponIcon.sprite = RemingtonM870Icon;
                WeaponName.text = "Remington M870";
                CurrentAmmoIcon.sprite = Ammo12GaugeIcon;
            }
            if (weapon.WeaponName == EWeaponName.Search) {
                WeaponIcon.sprite = SearchIcon;
                WeaponName.text = "Å½»ö";
                CurrentAmmoIcon.sprite = TransparentIcon;
            }
        }

	    private void UpdateAmmoProgress() {
            if (_weapon.IsReloading) {
                if (_weapon.WeaponName == EWeaponName.RemingtonM870) {
                    AmmoProgress.fillAmount = _weapon.ClipAmmo / (float)_weapon.MaxClipAmmo;
                } else {
                    AmmoProgress.fillAmount = _weapon.GetReloadProgress();
                }
            } else {
                AmmoProgress.fillAmount = _weapon.ClipAmmo / (float)_weapon.MaxClipAmmo;
		    }
        }
	}
}
