using UnityEngine;

namespace ProjectEpsilon {
    public class UIKillFeed : MonoBehaviour{
		public UIKillFeedItem KillFeedItemPrefab;
		public float ItemLifetime = 6f;
		public Sprite[] WeaponIcons;

		public void ShowKill(string killer, string victim, EWeaponName weaponName, bool isCriticalKill) {
			var item = Instantiate(KillFeedItemPrefab, transform);

			item.Killer.text = killer;
			item.Victim.text = victim;
            item.WeaponIcon.sprite = WeaponIcons[0];
            switch (weaponName) {
                case EWeaponName.M1911:
                    item.WeaponIcon.sprite = WeaponIcons[1];
                    break;
                case EWeaponName.SMG11:
                    item.WeaponIcon.sprite = WeaponIcons[2];
                    break;
                case EWeaponName.SuperShorty:
                    item.WeaponIcon.sprite = WeaponIcons[3];
                    break;
                case EWeaponName.AK47:
                    item.WeaponIcon.sprite = WeaponIcons[4];
                    break;
                case EWeaponName.RemingtonM870:
                    item.WeaponIcon.sprite = WeaponIcons[5];
                    break;
                case EWeaponName.MP5:
                    item.WeaponIcon.sprite = WeaponIcons[6];
                    break;
            }
			item.CriticalKillGroup.SetActive(isCriticalKill);

			Destroy(item.gameObject, ItemLifetime);
		}
	}
}
