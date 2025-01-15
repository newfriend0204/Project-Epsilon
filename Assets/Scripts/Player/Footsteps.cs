using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace ProjectEpsilon {
    public class Footsteps : NetworkBehaviour {
		public SimpleKCC   KCC;
		public AudioClip[] FootstepClips;
		public AudioSource FootstepSource;
		public float       FootstepDuration = 0.5f;

		private float _footstepCooldown;
		private bool _wasGrounded;

		public override void Spawned() {
			_wasGrounded = true;

			if (HasInputAuthority) {
				FootstepSource.volume -= 0.1f;
			}
		}

		public override void Render() {
			if (KCC.IsGrounded != _wasGrounded) {
				PlayFootstep();

				_wasGrounded = KCC.IsGrounded;
			}

			if (KCC.IsGrounded == false)
				return;

			_footstepCooldown -= Time.deltaTime;

			if (KCC.RealSpeed < 0.5f)
				return;

			if (_footstepCooldown <= 0f) {
				PlayFootstep();
			}
		}

		private void PlayFootstep() {
			var clip = FootstepClips[Random.Range(0, FootstepClips.Length)];
			FootstepSource.PlayOneShot(clip);

			_footstepCooldown = FootstepDuration;
		}
	}
}
