using Fusion;
using Fusion.Menu;

namespace ProjectEpsilon {
    public class MenuUIController : FusionMenuUIController<FusionMenuConnectArgs> {
		public FusionMenuConfig Config => _config;

		public GameMode SelectedGameMode { get; protected set; } = GameMode.AutoHostOrClient;

		public virtual void OnGameStarted() {
		}
		public virtual void OnGameStopped() {
		}
	}
}
