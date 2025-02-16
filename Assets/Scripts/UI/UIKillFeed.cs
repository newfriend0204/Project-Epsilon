using UnityEngine;

namespace ProjectEpsilon {
    public class UIKillFeed : MonoBehaviour{
		public UIKillFeedItem KillFeedItemPrefab;
		public float ItemLifetime = 6f;
		public Sprite[] WeaponIcons;

		public void ShowKill(string killer, string victim, EWeaponType weaponType, bool isCriticalKill) {
			var item = Instantiate(KillFeedItemPrefab, transform);

			item.Killer.text = killer;
			item.Victim.text = victim;
			item.WeaponIcon.sprite = WeaponIcons[(int)weaponType];
			item.CriticalKillGroup.SetActive(isCriticalKill);

			Destroy(item.gameObject, ItemLifetime);
		}
	}
}
