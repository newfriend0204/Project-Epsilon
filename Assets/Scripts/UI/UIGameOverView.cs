﻿using TMPro;
using UnityEngine;

namespace ProjectEpsilon {
    public class UIGameOverView : MonoBehaviour
	{
		public TextMeshProUGUI Winner;
		public GameObject VictoryGroup;
		public GameObject DefeatGroup;
		public AudioSource GameOverMusic;

		private GameUI _gameUI;
		private EGameplayState _lastState;

		public void GoToMenu() {
			_gameUI.GoToMenu();
		}

		private void Awake() {
			_gameUI = GetComponentInParent<GameUI>();
		}

		private void Update() {
			if (_gameUI.Runner == null)
				return;

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			if (_gameUI.Gameplay.Object == null || _gameUI.Gameplay.Object.IsValid == false)
				return;

			if (_lastState == _gameUI.Gameplay.State)
				return;

			GameOverMusic.PlayDelayed(1f);

			_lastState = _gameUI.Gameplay.State;

			bool localPlayerIsWinner = false;
			Winner.text = string.Empty;

			foreach (var playerPair in _gameUI.Gameplay.PlayerData) {
				if (playerPair.Value.StatisticPosition != 1)
					continue;

				Winner.text = $"승리자: {playerPair.Value.Nickname}";
				localPlayerIsWinner = playerPair.Key == _gameUI.Runner.LocalPlayer;
			}

			VictoryGroup.SetActive(localPlayerIsWinner);
			DefeatGroup.SetActive(localPlayerIsWinner == false);
		}
	}
}
