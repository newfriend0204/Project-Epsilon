using UnityEngine;

namespace ProjectEpsilon {
    public class RotateObject : MonoBehaviour
	{
		public float Speed = 90f;

		private void Awake() {
			transform.Rotate(0f, Random.Range(0f, 360f), 0f);
		}

		private void Update() {
			transform.Rotate(0f, Speed * Time.deltaTime, 0f);
		}
	}
}
