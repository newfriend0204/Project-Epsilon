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
                        WeaponExplain.text = "<mark>M1911</mark>\r\n���� �⺻���� �����Դϴ�. ����, źâ ��� ����Դϴ�.\r\nź�� ��:<color=lightblue>12</color>         ��� ź��:<color=lightblue>.45ACP</color>       ������ �ӵ�:<color=lightblue>1��</color>   \r\n�����:<color=lightblue>450</color>                        ź����:<color=lightblue>2</color>                   ����:<color=lightblue>20</color>   \r\n\r\n<mark>Ư��:��������</mark>\r\n��� ������ ��� �ӵ��� <color=green>���� ����</color>�մϴ�.\r\n���� ��ü �ð��� <color=green>���� ����</color>�մϴ�.\r\n�Ÿ��� ���� ���Ұ� <color=red>����</color>�մϴ�.\r\n�Ÿ��� ���� ���̰� <color=red>����</color>�մϴ�.";
                        break;
                    case EWeaponName.SMG11:
                        WeaponExplain.text = "<mark>SMG-11</mark>\r\n������� �ſ� �������� źâ�� �������� ��������Դϴ�.\r\nź�� ��:<color=lightblue>16</color>         ��� ź��:<color=lightblue>.45ACP</color>    ������ �ӵ�:<color=lightblue>2.2��</color> \r\n�����:<color=lightblue>1400</color>                        ź����:<color=lightblue>8</color>                   ����:<color=lightblue>10</color>   \r\n\r\n<mark>Ư��:��������</mark>\r\n��� ������ ��� �ӵ��� <color=green>���� ����</color>�մϴ�.\r\n���� ��ü �ð��� <color=green>���� ����</color>�մϴ�.\r\n�Ÿ��� ���� ���Ұ� <color=red>����</color>�մϴ�.\r\n�Ÿ��� ���� ���̰� <color=red>����</color>�մϴ�.";
                        break;
                    case EWeaponName.AK47:
                        WeaponExplain.text = "<mark>AK47</mark>\r\n������ ���������� ������� ���� ���� ���ݼ����Դϴ�.\r\nź�� ��:<color=lightblue>30</color>      ��� ź��:<color=lightblue>7.62mm</color>     ������ �ӵ�:<color=lightblue>1.8��</color>    \r\n�����:<color=lightblue>1400</color>                       ź����:<color=lightblue>4</color>                    ����:<color=lightblue>20</color>   \r\n\r\n<mark>Ư��:�ֹ���</mark>\r\n��� ������ ��� �ӵ��� <color=red>���� ����</color>�մϴ�.\r\n���� ��ü �ð��� <color=red>���� ����</color>�մϴ�.\r\n�Ÿ��� ���� ���Ұ� <color=green>����</color>�մϴ�.\r\n�Ÿ��� ���� ���̰� <color=green>����</color>�մϴ�.";
                        break;
                    case EWeaponName.RemingtonM870:
                        WeaponExplain.text = "<mark>Remington M870</mark>\r\n�߰Ÿ����� ����� ������ �� �ִ� ���� �׼� ��ź���Դϴ�.\r\nź�� ��:<color=lightblue>7</color>      ��� ź��:<color=lightblue>12������</color>    ������ �ӵ�:<color=lightblue>�ߴ� 0.2��</color>   \r\n�����:<color=lightblue>75</color>                       ź����:<color=lightblue>6</color>                   ����:<color=lightblue>�ߴ� 10</color>   \r\n\r\n<mark>Ư��:�ֹ���</mark>\r\n��� ������ ��� �ӵ��� <color=red>���� ����</color>�մϴ�.\r\n���� ��ü �ð��� <color=red>���� ����</color>�մϴ�.\r\n�Ÿ��� ���� ���Ұ� <color=green>����</color>�մϴ�.\r\n�Ÿ��� ���� ���̰� <color=green>����</color>�մϴ�.";
                        break;
                    case EWeaponName.Search:
                        WeaponExplain.text = "<mark>Ž��</mark>\r\n��ó ������� ���� ã�� �� �ִ� Ư�� ����Դϴ�.\r\n�ִ� Ž�� �Ÿ�:<color=lightblue>15m</color>\r\n�������� ã���� <color=lightblue>�ʷϻ� ������, Ȥ�� �ʷ����� ���Դϴ�.</color>\r\n���� ���� ��� ���� <color=lightblue>���� ������� �̸��� ��ϴ�.</color>\r\n\r\n \r\n\r\n<mark>Ư��:���� ���</mark>\r\n��� ������ ��� �ӵ��� <color=green>����</color>�մϴ�.\r\n���� ��ü �ð��� <color=green>����</color>�մϴ�.";
                        break;
                    default:
                        break;
                }
                WeaponToggleText.text = "���� ���� �ݱ�";
            } else {
                WeaponExplain.text = "";
                WeaponToggleText.text = "���� ���� ����";
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
                    WeaponName.text = "Ž��";
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
