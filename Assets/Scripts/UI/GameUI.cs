using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectEpsilon {
    public class GameUI : MonoBehaviour {
		public Gameplay Gameplay;
		[HideInInspector]
		public NetworkRunner Runner;

		public UIPlayerView PlayerView;
		public UIGameplayView GameplayView;
		public UIGameOverView GameOverView;
		public GameObject ScoreboardView;
		public GameObject MenuView;
		public UISettingsView SettingsView;
		public GameObject DisconnectedView;

		public void OnRunnerShutdown(NetworkRunner runner, ShutdownReason reason) {
			if (GameOverView.gameObject.activeSelf)
				return;

			ScoreboardView.SetActive(false);
			SettingsView.gameObject.SetActive(false);
			MenuView.gameObject.SetActive(false);

			DisconnectedView.SetActive(true);
		}

		public void GoToMenu() {
			if (Runner != null) {
				Runner.Shutdown();
			}

			SceneManager.LoadScene("Startup");
		}

		private void Awake() {
			PlayerView.gameObject.SetActive(false);
			MenuView.SetActive(false);
			SettingsView.gameObject.SetActive(false);
			DisconnectedView.SetActive(false);

			SettingsView.LoadSettings();

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void Update() {
			if (Application.isBatchMode == true)
				return;

			if (Gameplay.Object == null || Gameplay.Object.IsValid == false)
				return;

			Runner = Gameplay.Runner;

			var keyboard = Keyboard.current;
			bool gameplayActive = Gameplay.State < EGameplayState.Finished;

			ScoreboardView.SetActive(gameplayActive && keyboard != null && keyboard.tabKey.isPressed);

			if (gameplayActive && keyboard != null && keyboard.escapeKey.wasPressedThisFrame) {
				MenuView.SetActive(!MenuView.activeSelf);
			}

			GameplayView.gameObject.SetActive(gameplayActive);
			GameOverView.gameObject.SetActive(gameplayActive == false);

			var playerObject = Runner.GetPlayerObject(Runner.LocalPlayer);
			if (playerObject != null) {
				var player = playerObject.GetComponent<Player>();
				var playerData = Gameplay.PlayerData.Get(Runner.LocalPlayer);

				PlayerView.UpdatePlayer(player, playerData);
				PlayerView.gameObject.SetActive(gameplayActive);
			} else {
				PlayerView.gameObject.SetActive(false);
			}
		}
	}
}
