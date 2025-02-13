using UnityEngine;

namespace ProjectEpsilon {
    public class PickupPoint : MonoBehaviour {
        void Start() {
            float randomYRotation = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0, randomYRotation, 0);
        }

        private void OnDrawGizmos() {
			Gizmos.DrawWireSphere(transform.position, 0.1f);
		}
	}
}
