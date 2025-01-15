using TMPro;
using UnityEngine;

namespace ProjectEpsilon {
    public class UIScoreboardRow : MonoBehaviour {
		public CanvasGroup     CanvasGroup;
		public TextMeshProUGUI Position;
		public TextMeshProUGUI Name;
		public TextMeshProUGUI Kills;
		public TextMeshProUGUI Deaths;
		public GameObject      LocalPlayerGroup;
		public GameObject      DeadGroup;
	}
}
