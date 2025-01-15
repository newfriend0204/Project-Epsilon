using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace ProjectEpsilon {
    public static class PlayerManager {
		private static List<PlayerRef> _tempSpawnPlayers   = new List<PlayerRef>();
		private static List<Player>    _tempSpawnedPlayers = new List<Player>();

		public static void UpdatePlayerConnections(NetworkRunner runner, Action<PlayerRef> spawnPlayer, Action<PlayerRef, Player> despawnPlayer) {
			_tempSpawnPlayers.Clear();
			_tempSpawnedPlayers.Clear();

			_tempSpawnPlayers.AddRange(runner.ActivePlayers);

			runner.GetAllBehaviours(_tempSpawnedPlayers);

			for (int i = 0; i < _tempSpawnedPlayers.Count; ++i) {
				Player player = _tempSpawnedPlayers[i];
				PlayerRef playerRef = player.Object.InputAuthority;

				_tempSpawnPlayers.Remove(playerRef);

				if (runner.IsPlayerValid(playerRef) == false) {
					try {
						despawnPlayer(playerRef, player);
					} catch (Exception exception) {
						Debug.LogException(exception);
					}
				}
			}

			for (int i = 0; i < _tempSpawnPlayers.Count; ++i) {
				try {
					spawnPlayer(_tempSpawnPlayers[i]);
				} catch (Exception exception) {
					Debug.LogException(exception);
				}
			}

			_tempSpawnPlayers.Clear();
			_tempSpawnedPlayers.Clear();
		}
	}
}
