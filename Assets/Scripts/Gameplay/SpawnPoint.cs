using UnityEngine;

namespace ProjectEpsilon {
    public class SpawnPoint : MonoBehaviour {
		private void OnDrawGizmos() {
			Gizmos.DrawWireSphere(transform.position, 0.1f);
		}
	}
}
