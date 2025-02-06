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
		public Sprite AK47Icon;
        public Sprite RemingtonM870Icon;
        public Sprite SearchIcon;
        public Sprite Ammo45ACPIcon;
        public Sprite Ammo7_62mmIcon;
        public Sprite Ammo12GaugeIcon;
        public Sprite TransparentIcon;

        public TextMeshProUGUI WeaponToggleText;
        public TextMeshProUGUI WeaponExplain;

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
                switch (weaponsScript.currentWeapon) {
                    case EWeaponName.M1911:
                        WeaponExplain.text = "<mark>M1911</mark>\r\n가장 기본적인 권총입니다. 위력, 탄창 모두 평균입니다.\r\n탄약 수:<color=lightblue>12</color>         사용 탄약:<color=lightblue>.45ACP</color>       재장전 속도:<color=lightblue>1초</color>   \r\n연사력:<color=lightblue>450</color>                        탄퍼짐:<color=lightblue>2</color>                   위력:<color=lightblue>20</color>   \r\n\r\n<mark>특성:보조무기</mark>\r\n들고 있을때 요원 속도가 <color=green>소폭 증가</color>합니다.\r\n무기 교체 시간이 <color=green>소폭 감소</color>합니다.\r\n거리별 위력 감소가 <color=red>증가</color>합니다.\r\n거리별 위력 길이가 <color=red>감소</color>합니다.";
                        break;
                    case EWeaponName.SMG11:
                        WeaponExplain.text = "<mark>SMG-11</mark>\r\n연사력은 매우 빠르지만 탄창이 제한적인 기관권총입니다.\r\n탄약 수:<color=lightblue>16</color>         사용 탄약:<color=lightblue>.45ACP</color>    재장전 속도:<color=lightblue>2.2초</color> \r\n연사력:<color=lightblue>1400</color>                        탄퍼짐:<color=lightblue>8</color>                   위력:<color=lightblue>10</color>   \r\n\r\n<mark>특성:보조무기</mark>\r\n들고 있을때 요원 속도가 <color=green>소폭 증가</color>합니다.\r\n무기 교체 시간이 <color=green>소폭 감소</color>합니다.\r\n거리별 위력 감소가 <color=red>증가</color>합니다.\r\n거리별 위력 길이가 <color=red>감소</color>합니다.";
                        break;
                    case EWeaponName.AK47:
                        WeaponExplain.text = "<mark>AK47</mark>\r\n위력은 강력하지만 연사력이 조금 낮은 돌격소총입니다.\r\n탄약 수:<color=lightblue>30</color>      사용 탄약:<color=lightblue>7.62mm</color>     재장전 속도:<color=lightblue>1.8초</color>    \r\n연사력:<color=lightblue>1400</color>                       탄퍼짐:<color=lightblue>4</color>                    위력:<color=lightblue>20</color>   \r\n\r\n<mark>특성:주무기</mark>\r\n들고 있을때 요원 속도가 <color=red>소폭 감소</color>합니다.\r\n무기 교체 시간이 <color=red>소폭 증가</color>합니다.\r\n거리별 위력 감소가 <color=green>감소</color>합니다.\r\n거리별 위력 길이가 <color=green>증가</color>합니다.";
                        break;
                    case EWeaponName.RemingtonM870:
                        WeaponExplain.text = "<mark>Remington M870</mark>\r\n중거리까지 든든히 교전할 수 있는 펌프 액션 산탄총입니다.\r\n탄약 수:<color=lightblue>7</color>      사용 탄약:<color=lightblue>12게이지</color>    재장전 속도:<color=lightblue>발당 0.2초</color>   \r\n연사력:<color=lightblue>75</color>                       탄퍼짐:<color=lightblue>6</color>                   위력:<color=lightblue>발당 10</color>   \r\n\r\n<mark>특성:주무기</mark>\r\n들고 있을때 요원 속도가 <color=red>소폭 감소</color>합니다.\r\n무기 교체 시간이 <color=red>소폭 증가</color>합니다.\r\n거리별 위력 감소가 <color=green>감소</color>합니다.\r\n거리별 위력 길이가 <color=green>증가</color>합니다.";
                        break;
                    case EWeaponName.Search:
                        WeaponExplain.text = "<mark>탐색</mark>\r\n근처 전투장비를 쉽게 찾을 수 있는 특수 고글입니다.\r\n최대 탐색 거리:<color=lightblue>15m</color>\r\n아이템을 찾을시 <color=lightblue>초록색 윤곽선, 혹은 초록으로 보입니다.</color>\r\n또한 전투 장비 위에 <color=lightblue>무슨 장비인지 이름이 뜹니다.</color>\r\n\r\n \r\n\r\n<mark>특성:전투 장비</mark>\r\n들고 있을때 요원 속도가 <color=green>증가</color>합니다.\r\n무기 교체 시간이 <color=green>감소</color>합니다.";
                        break;
                    default:
                        break;
                }
                WeaponToggleText.text = "무기 설명 닫기";
            } else {
                WeaponExplain.text = "";
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
