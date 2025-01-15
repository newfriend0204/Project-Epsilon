using UnityEngine;
using Fusion.Menu;

namespace ProjectEpsilon {
    public abstract class MenuConnectionPlugin : MonoBehaviour {
		public abstract IFusionMenuConnection Create(MenuConnectionBehaviour connectionBehaviour);
	}
}
