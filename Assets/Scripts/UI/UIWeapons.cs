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
        public Sprite SMG11Icon;
        public Sprite SuperShortyIcon;
        public Sprite AK47Icon;
        public Sprite RemingtonM870Icon;
        public Sprite MP5Icon;
        public Sprite SearchIcon;
        public Sprite Ammo45ACPIcon;
        public Sprite Ammo7_62mmIcon;
        public Sprite Ammo12GaugeIcon;
        public Sprite TransparentIcon;

        public TextMeshProUGUI WeaponToggleText;
        public GameObject WeaponExplain;
        public TextMeshProUGUI WeaponFirerate;
        public TextMeshProUGUI WeaponDamage;
        public TextMeshProUGUI WeaponDispersion;
        public TextMeshProUGUI WeaponAmmo;
        public TextMeshProUGUI WeaponReloadTime;
        public TextMeshProUGUI WeaponGunExplain;
        public TextMeshProUGUI WeaponExplainText;

        private Weapon _weapon;
	    private int _lastClipAmmo;
	    private int _lastRemainingAmmo;
        private Weapons weaponsScript;
        private bool _isExplaining = false;

        public void UpdateWeapons(Player weapons) {
            weaponsScript = weapons.Weapons;
            SetWeapon(weapons.Weapons.CurrentWeapon);

            if (weaponsScript.currentSidearm == EWeaponName.M1911) {
                SidearmIcon.sprite = M1911Icon;
                SidearmName.text = "M1911";
            } else if (weaponsScript.currentSidearm == EWeaponName.SMG11) {
                SidearmIcon.sprite = SMG11Icon;
                SidearmName.text = "SMG-11";
            } else if (weaponsScript.currentSidearm == EWeaponName.SuperShorty) {
                SidearmIcon.sprite = SuperShortyIcon;
                SidearmName.text = "Super Shorty";
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
            } else if (weaponsScript.currentPrimary == EWeaponName.MP5) {
                PrimaryIcon.sprite = MP5Icon;
                PrimaryName.text = "MP5";
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
            }
            if (weaponsScript.currentWeapon == weaponsScript.currentSidearm) {
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


            if (weapons.ammo45ACP <= 0) {
                Ammo45ACPRemain.GetComponent<Animation>().enabled = true;
            } else {
                Ammo45ACPRemain.GetComponent<Animation>().enabled = false;
                Ammo45ACPRemain.color = Color.white;
            }
            if (weapons.ammo7_62mm <= 0) {
                Ammo7_62mmRemain.GetComponent<Animation>().enabled = true;
            } else {
                Ammo7_62mmRemain.GetComponent<Animation>().enabled = false;
                Ammo7_62mmRemain.color = Color.white;
            }
            if (weapons.ammo12Gauge <= 0) {
                Ammo12GaugeRemain.GetComponent<Animation>().enabled = true;
            } else {
                Ammo12GaugeRemain.GetComponent<Animation>().enabled = false;
                Ammo12GaugeRemain.color = Color.white;
            }


            SetAmmoTransparency(0.2f, Ammo45ACP);
            SetAmmoTransparency(0.2f, Ammo7_62mm);
            SetAmmoTransparency(0.2f, Ammo12Gauge);

            switch (weaponsScript.currentWeapon) {
                case EWeaponName.M1911:
                case EWeaponName.SMG11:
                    SetAmmoTransparency(1f, Ammo45ACP);
                    break;
                case EWeaponName.AK47:
                case EWeaponName.MP5:
                    SetAmmoTransparency(1f, Ammo7_62mm);
                    break;
                case EWeaponName.RemingtonM870:
                case EWeaponName.SuperShorty:
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

            if (_isExplaining) {
                WeaponExplain.SetActive(true);
                switch (weaponsScript.currentWeapon) {
                    case EWeaponName.M1911:
                        WeaponExplainText.text = "가장 기본적인 권총입니다. 위력, 탄창 모두 평균입니다.";
                        WeaponGunExplain.text = "<mark>특성:보조무기</mark>\r\n들고 있을때 요원 속도가 <color=green>소폭 증가</color>합니다.\r\n무기 교체 시간이 <color=green>소폭 감소</color>합니다.\r\n거리별 위력 감소가 <color=red>증가</color>합니다.\r\n거리별 위력 길이가 <color=red>감소</color>합니다.";
                        WeaponAmmo.text = ".45 ACP\r\n총 12발";
                        WeaponDamage.text = "20";
                        WeaponDispersion.text = "2";
                        WeaponFirerate.text = "450";
                        WeaponReloadTime.text = "1초";
                        break;
                    case EWeaponName.SMG11:
                        WeaponExplainText.text = "연사력은 매우 빠르지만 탄창이 제한적인 기관권총입니다.";
                        WeaponGunExplain.text = "<mark>특성:보조무기</mark>\r\n들고 있을때 요원 속도가 <color=green>소폭 증가</color>합니다.\r\n무기 교체 시간이 <color=green>소폭 감소</color>합니다.\r\n거리별 위력 감소가 <color=red>증가</color>합니다.\r\n거리별 위력 길이가 <color=red>감소</color>합니다.";
                        WeaponAmmo.text = ".45 ACP\r\n총 16발";
                        WeaponDamage.text = "10";
                        WeaponDispersion.text = "8";
                        WeaponFirerate.text = "1400";
                        WeaponReloadTime.text = "2.2초";
                        break;
                    case EWeaponName.SuperShorty:
                        WeaponExplainText.text = "근거리에선 매우 강력한 성능을 발휘하지만 중거리부터는\r\n크게 위력이 떨어지는 펌프 액션 산탄총입니다.";
                        WeaponGunExplain.text = "<mark>특성:보조무기</mark>\r\n들고 있을때 요원 속도가 <color=green>소폭 증가</color>합니다.\r\n무기 교체 시간이 <color=green>소폭 감소</color>합니다.\r\n거리별 위력 감소가 <color=red>증가</color>합니다.\r\n거리별 위력 길이가 <color=red>감소</color>합니다.";
                        WeaponAmmo.text = "12게이지\r\n총 3발";
                        WeaponDamage.text = "10";
                        WeaponDispersion.text = "8";
                        WeaponFirerate.text = "75";
                        WeaponReloadTime.text = "발당 0.2초";
                        break;
                    case EWeaponName.AK47:
                        WeaponExplainText.text = "위력은 강력하지만 연사력이 조금 낮은 돌격소총입니다.";
                        WeaponGunExplain.text = "<mark>특성:주무기</mark>\r\n들고 있을때 요원 속도가 <color=red>소폭 감소</color>합니다.\r\n무기 교체 시간이 <color=red>소폭 증가</color>합니다.\r\n거리별 위력 감소가 <color=green>감소</color>합니다.\r\n거리별 위력 길이가 <color=green>증가</color>합니다.";
                        WeaponAmmo.text = "7.62mm\r\n총 20발";
                        WeaponDamage.text = "20";
                        WeaponDispersion.text = "4";
                        WeaponFirerate.text = "600";
                        WeaponReloadTime.text = "1.8초";
                        break;
                    case EWeaponName.RemingtonM870:
                        WeaponExplainText.text = "중거리까지 든든히 교전할 수 있는 펌프 액션 산탄총입니다.";
                        WeaponGunExplain.text = "<mark>특성:주무기</mark>\r\n들고 있을때 요원 속도가 <color=red>소폭 감소</color>합니다.\r\n무기 교체 시간이 <color=red>소폭 증가</color>합니다.\r\n거리별 위력 감소가 <color=green>감소</color>합니다.\r\n거리별 위력 길이가 <color=green>증가</color>합니다.";
                        WeaponAmmo.text = "12게이지\r\n총 7발";
                        WeaponDamage.text = "10";
                        WeaponDispersion.text = "6";
                        WeaponFirerate.text = "75";
                        WeaponReloadTime.text = "발당 0.2초";
                        break;
                    case EWeaponName.MP5:
                        WeaponExplainText.text = "연사력이 빠르지만 위력이 조금 약간 낮은 기관탄총입니다.";
                        WeaponGunExplain.text = "<mark>특성:주무기</mark>\r\n들고 있을때 요원 속도가 <color=red>소폭 감소</color>합니다.\r\n무기 교체 시간이 <color=red>소폭 증가</color>합니다.\r\n거리별 위력 감소가 <color=green>감소</color>합니다.\r\n거리별 위력 길이가 <color=green>증가</color>합니다.";
                        WeaponAmmo.text = "7.62mm\r\n총 30발";
                        WeaponDamage.text = "15";
                        WeaponDispersion.text = "6";
                        WeaponFirerate.text = "900";
                        WeaponReloadTime.text = "2초";
                        break;
                    case EWeaponName.Search:
                        WeaponExplainText.text = "10m 이내의 전투 장비와 20m 밖에 있는 적을 찾을 수 있는\r\n특수 고글입니다. 단, 착용시 소음이 심하다는 단점이 있습니다.";
                        WeaponGunExplain.text = "특성:전투 장비</mark>\r\n들고 있을때 요원 속도가 <color=green>증가</color>합니다.\r\n무기 교체 시간이 <color=green>감소</color>합니다.";
                        WeaponAmmo.text = "-";
                        WeaponDamage.text = "-";
                        WeaponDispersion.text = "-";
                        WeaponFirerate.text = "-";
                        WeaponReloadTime.text = "-";
                        break;
                    default:
                        break;
                }
                WeaponToggleText.text = "무기 설명 닫기";
            } else {
                WeaponExplain.SetActive(false);
                WeaponToggleText.text = "무기 설명 열기";
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

            switch (weapon.WeaponName) {
                case EWeaponName.M1911:
                    WeaponIcon.sprite = M1911Icon;
                    WeaponName.text = "M1911";
                    CurrentAmmoIcon.sprite = Ammo45ACPIcon;
                    break;
                case EWeaponName.SMG11:
                    WeaponIcon.sprite = SMG11Icon;
                    WeaponName.text = "SMG-11";
                    CurrentAmmoIcon.sprite = Ammo45ACPIcon;
                    break;
                case EWeaponName.SuperShorty:
                    WeaponIcon.sprite = SuperShortyIcon;
                    WeaponName.text = "Super Shorty";
                    CurrentAmmoIcon.sprite = Ammo12GaugeIcon;
                    break;
                case EWeaponName.AK47:
                    WeaponIcon.sprite = AK47Icon;
                    WeaponName.text = "AK47";
                    CurrentAmmoIcon.sprite = Ammo7_62mmIcon;
                    break;
                case EWeaponName.RemingtonM870:
                    WeaponIcon.sprite = RemingtonM870Icon;
                    WeaponName.text = "Remington M870";
                    CurrentAmmoIcon.sprite = Ammo12GaugeIcon;
                    break;
                case EWeaponName.MP5:
                    WeaponIcon.sprite = MP5Icon;
                    WeaponName.text = "MP5";
                    CurrentAmmoIcon.sprite = Ammo7_62mmIcon;
                    break;
                case EWeaponName.Search:
                    WeaponIcon.sprite = SearchIcon;
                    WeaponName.text = "탐색";
                    CurrentAmmoIcon.sprite = TransparentIcon;
                    break;
                default:
                    break;
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

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Q)) {
                _isExplaining = !_isExplaining;
            }
        }
    }
}
