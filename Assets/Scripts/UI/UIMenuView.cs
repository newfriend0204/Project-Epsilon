using UnityEngine;

namespace ProjectEpsilon {
    public class UIMenuView : MonoBehaviour {
		private GameUI _gameUI;

		private CursorLockMode _previousLockState;
		private bool _previousCursorVisibility;

		public void ResumeGame() {
			gameObject.SetActive(false);
		}

		public void OpenSettings() {
			_gameUI.SettingsView.gameObject.SetActive(true);
		}

		public void LeaveGame() {
			_previousLockState = CursorLockMode.None;
			_previousCursorVisibility = true;

			_gameUI.GoToMenu();
		}

		private void Awake() {
			_gameUI = GetComponentInParent<GameUI>();
		}

		private void OnEnable() {
			_previousLockState = Cursor.lockState;
			_previousCursorVisibility = Cursor.visible;

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void OnDisable() {
			Cursor.lockState = _previousLockState;
			Cursor.visible = _previousCursorVisibility;
		}
	}
}
