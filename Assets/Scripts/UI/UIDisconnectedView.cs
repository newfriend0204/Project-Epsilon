using UnityEngine;

namespace ProjectEpsilon {
    public class UIDisconnectedView : MonoBehaviour {
		public void GoToMenu() {
			var gameUI = GetComponentInParent<GameUI>(true);
			gameUI.GoToMenu();
		}

		private void Update() {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
