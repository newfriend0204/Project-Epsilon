using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace ProjectEpsilon {
    public class Footsteps : NetworkBehaviour {
		public SimpleKCC KCC;
		public AudioClip[] FootstepClips;
		public AudioSource FootstepSource;
		public float FootstepDuration = 0.5f;

		private float _footstepCooldown;
		private bool _wasGrounded;
		private float _currentDuration;

		public override void Spawned() {
			_wasGrounded = true;
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

            float _saveVolume = HasInputAuthority ? 0.15f : 0.6f;
			float _saveMax = _saveVolume;
            float _saveDuration = FootstepDuration;
            if (GetComponentInParent<Player>().isCrouching) {
                _saveVolume -= _saveMax / 10 * 2.5f;
                _saveDuration += 0.05f;
			}
            if (GetComponentInParent<Player>().isSneaking) {
                _saveVolume -= _saveMax / 10 * 3.5f;
                _saveDuration += 0.1f;
            }
            if (GetComponentInParent<Player>().isAiming) {
                _saveVolume -= _saveMax / 10 * 1;
                _saveDuration += 0.01f;
            }
            if (GetComponentInParent<Player>().isRunning) {
                _saveVolume += _saveMax / 10 * 5;
                _saveDuration -= 0.05f;
            }
            FootstepSource.volume = _saveVolume;
			_currentDuration = _saveDuration;

            if (_footstepCooldown <= 0f) {
				PlayFootstep();
			}
		}

		private void PlayFootstep() {
			var clip = FootstepClips[Random.Range(0, FootstepClips.Length)];
			FootstepSource.PlayOneShot(clip);

			_footstepCooldown = _currentDuration;
		}
	}
}
