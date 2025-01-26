using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectEpsilon {
    public enum EWeaponType {
		None,
		Pistol,
		Rifle,
		Shotgun,
	}

	public class Weapon : NetworkBehaviour {
		public EWeaponType Type;

		[Header("Fire Setup")]
		public bool IsAutomatic = true;
		public float Damage = 10f;
		public int FireRate = 100;
		[Range(1, 20)]
		public int ProjectilesPerShot = 1;
		public float Dispersion = 0f;
		public LayerMask HitMask;
		public float MaxHitDistance = 100f;

		[Header("Ammo")]
		public int MaxClipAmmo = 12;
		public int StartAmmo = 25;
		public float ReloadTime = 2f;

		[Header("Visuals")]
		public Sprite Icon;
		public string Name;
		public Animator Animator;
		[FormerlySerializedAs("WeaponVisual")]
		public GameObject FirstPersonVisual;
		public GameObject ThirdPersonVisual;

		[Header("Fire Effect")]
		[FormerlySerializedAs("MuzzleTransform")]
		public Transform FirstPersonMuzzleTransform;
		public Transform ThirdPersonMuzzleTransform;
		public GameObject MuzzleEffectPrefab;
		public ProjectileVisual ProjectileVisualPrefab;

		[Header("Sounds")]
		public AudioSource FireSound;
		public AudioSource ReloadingSound;
		public AudioSource EmptyClipSound;

        public bool HasAmmo => ClipAmmo > 0 || RemainingAmmo > 0;

		[Networked]
		public NetworkBool IsCollected { get; set; }
		[Networked]
		public NetworkBool IsReloading { get; set; }
		[Networked]
		public int ClipAmmo { get; set; }
		[Networked]
		public int RemainingAmmo { get; set; }

		[Networked]
		private int _fireCount { get; set; }
		[Networked]
		internal TickTimer _fireCooldown { get; set; }
		[Networked, Capacity(32)]
		private NetworkArray<ProjectileData> _projectileData { get; }

		private int _fireTicks;
		private int _visibleFireCount;
		private bool _reloadingVisible;
		private GameObject _muzzleEffectInstance;
		private SceneObjects _sceneObjects;

        public void Fire(Vector3 firePosition, Vector3 fireDirection, bool justPressed) {
			if (IsCollected == false)
				return;
			if (justPressed == false && IsAutomatic == false)
				return;
			if (IsReloading)
				return;
			if (GetComponentInParent<Player>().isRunning)
				return;
			if (_fireCooldown.ExpiredOrNotRunning(Runner) == false)
				return;

            if (ClipAmmo <= 0) {
				PlayEmptyClipSound(justPressed);
				return;
			}

			Random.InitState(Runner.Tick * unchecked((int)Object.Id.Raw));

			for (int i = 0; i < ProjectilesPerShot; i++) {
				var projectileDirection = fireDirection;
				float saveDispersion = Dispersion;
                if (GetComponentInParent<Player>().isAiming) {
                    saveDispersion -= Dispersion / 10 * 4;
                }
                if (GetComponentInParent<Player>().isSneaking) {
                    saveDispersion -= Dispersion / 10 * 1;
                }
                if (GetComponentInParent<Player>().isCrouching) {
                    saveDispersion -= Dispersion / 10 * 3;
                }
                Quaternion dispersion = Quaternion.Euler(Random.insideUnitSphere * saveDispersion);
                if (Dispersion > 0f) {
					var dispersionRotation = dispersion;
                    projectileDirection = dispersionRotation * fireDirection;
				}

				FireProjectile(firePosition, projectileDirection);
			}

			_fireCooldown = TickTimer.CreateFromTicks(Runner, _fireTicks);
			ClipAmmo--;
        }

		public void Reload() {
			if (IsCollected == false)
				return;
			if (ClipAmmo >= MaxClipAmmo)
				return;
			if (RemainingAmmo <= 0)
				return;
			if (IsReloading)
				return;
			if (_fireCooldown.ExpiredOrNotRunning(Runner) == false)
				return;

			if (Type == EWeaponType.Shotgun) {
                Animator.SetTrigger("ReturnReload");
                ReloadingSound.Play();
            }

            IsReloading = true;

			if (GetComponentInParent<Player>().isAiming == true) {
				ExitADS();
			}
            _fireCooldown = TickTimer.CreateFromSeconds(Runner, ReloadTime);
		}

		public void AddAmmo(int amount) {
			RemainingAmmo += amount;
		}

		public void ToggleVisibility(bool isVisible) { 
			FirstPersonVisual.SetActive(isVisible);
			FirstPersonVisual.GetComponent<Animator>().enabled = isVisible;

			if (_muzzleEffectInstance != null) {
				_muzzleEffectInstance.SetActive(false);
			}
		}

		public float GetReloadProgress() {
			if (IsReloading == false)
				return 1f;

			return 1f - _fireCooldown.RemainingTime(Runner).GetValueOrDefault() / ReloadTime;
		}

		public override void Spawned() {
			if (HasStateAuthority) {
				ClipAmmo = Mathf.Clamp(StartAmmo, 0, MaxClipAmmo);
				RemainingAmmo = StartAmmo - ClipAmmo;
			}

			_visibleFireCount = _fireCount;

			float fireTime = 60f / FireRate;
			_fireTicks = Mathf.CeilToInt(fireTime / Runner.DeltaTime);

			_muzzleEffectInstance = Instantiate(MuzzleEffectPrefab, HasInputAuthority ? FirstPersonMuzzleTransform : ThirdPersonMuzzleTransform);
			_muzzleEffectInstance.SetActive(false);

			_sceneObjects = Runner.GetSingleton<SceneObjects>();
        }

		public override void FixedUpdateNetwork() {
			if (IsCollected == false)
				return;

			if (ClipAmmo == 0) {
				Reload();
			}

            if (IsReloading && _fireCooldown.ExpiredOrNotRunning(Runner)) {
				IsReloading = false;

				Animator.ResetTrigger("exitADS");

                int reloadAmmo = MaxClipAmmo - ClipAmmo;
				reloadAmmo = Mathf.Min(reloadAmmo, RemainingAmmo);
				
				if (Type == EWeaponType.Shotgun) {
					reloadAmmo = 1;
				}

				ClipAmmo += reloadAmmo;
				RemainingAmmo -= reloadAmmo;

				if (Type == EWeaponType.Shotgun) {
					if (ClipAmmo != MaxClipAmmo) {
						Animator.SetBool("ReloadEnd", false);
						Reload();
					} else if (ClipAmmo == MaxClipAmmo) {
						Animator.SetBool("ReloadEnd", true);
					}
					if (RemainingAmmo == 0) {
						Animator.SetBool("ReloadEnd", true);
					}
				}

                _fireCooldown = TickTimer.CreateFromSeconds(Runner, 0.5f);
			}
		}

        private void Update() {
            if (Input.GetMouseButtonDown(1)) {
                _fireCooldown = TickTimer.None;
                IsReloading = false;
                ReloadingSound.Stop();
				EnterADS();
            } else if (Input.GetMouseButtonUp(1) && !IsReloading) {
                ExitADS();
            }

			if (GetComponentInParent<Player>().isMoving) {
				Animator.SetBool("IsMoving", true);
			} else {
				Animator.SetBool("IsMoving", false);
			}

			if (Input.GetKey(KeyCode.LeftShift)) {
				GetComponentInParent<Player>().isSneaking = true;
            } else {
				GetComponentInParent<Player>().isSneaking = false;
            }

			if (Input.GetKey(KeyCode.LeftControl) && !GetComponentInParent<Player>().isCrouching && !GetComponentInParent<Player>().isAiming) {
				GetComponentInParent<Player>().isRunning = true;
				if (IsReloading) {
					_fireCooldown = TickTimer.None;
					IsReloading = false;
					ReloadingSound.Stop();
				}
            } else {
				GetComponentInParent<Player>().isRunning = false;
            }

			if (!GetComponentInParent<Player>().isMoving)
				GetComponentInParent<Player>().isRunning = false;
            Animator.SetBool("IsRunning", GetComponentInParent<Player>().isRunning);
        }

        public override void Render() {
			if (_visibleFireCount < _fireCount) {
				PlayFireEffect();
			}

			for (int i = _visibleFireCount; i < _fireCount; i++) {
				var data = _projectileData[i % _projectileData.Length];
				var muzzleTransform = HasInputAuthority ? FirstPersonMuzzleTransform : ThirdPersonMuzzleTransform;

				var projectileVisual = Instantiate(ProjectileVisualPrefab, muzzleTransform.position, muzzleTransform.rotation);
				projectileVisual.SetHit(data.HitPosition, data.HitNormal, data.ShowHitEffect);
			}

			_visibleFireCount = _fireCount;

			if (_reloadingVisible != IsReloading) {
				Animator.SetBool("IsReloading", IsReloading);

				if (IsReloading) {
					ReloadingSound.Play();
				}

				_reloadingVisible = IsReloading;
			}

            if (Type == EWeaponType.Shotgun && ClipAmmo == MaxClipAmmo) {
                Animator.SetBool("ReloadEnd", true);
            }
        }

		private void FireProjectile(Vector3 firePosition, Vector3 fireDirection) {
			var projectileData = new ProjectileData();

			var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;

			if (Runner.LagCompensation.Raycast(firePosition, fireDirection, MaxHitDistance, Object.InputAuthority, out var hit, HitMask, hitOptions)) {
				projectileData.HitPosition = hit.Point;
				projectileData.HitNormal = hit.Normal;

				if (hit.Hitbox != null) {
					ApplyDamage(hit.Hitbox, hit.Point, fireDirection);
				} else {
					projectileData.ShowHitEffect = true;
				}
			}

			_projectileData.Set(_fireCount % _projectileData.Length, projectileData);
			_fireCount++;
		}

		private void PlayFireEffect() {
			if (FireSound != null) {
				FireSound.PlayOneShot(FireSound.clip);
			}

			_muzzleEffectInstance.SetActive(false);
			_muzzleEffectInstance.SetActive(true);

            if (GetComponentInParent<Player>().isAiming) {
                Animator.SetTrigger("FireADS");
            } else {
                Animator.SetTrigger("Fire");
            }

            GetComponentInParent<Player>().PlayFireEffect();
		}

		private void ApplyDamage(Hitbox enemyHitbox, Vector3 position, Vector3 direction) {
			var enemyHealth = enemyHitbox.Root.GetComponent<Health>();
			if (enemyHealth == null || enemyHealth.IsAlive == false)
				return;

			float damageMultiplier = enemyHitbox is BodyHitbox bodyHitbox ? bodyHitbox.DamageMultiplier : 1f;
			bool isCriticalHit = damageMultiplier > 1f;

			float damage = Damage * damageMultiplier;
			if (_sceneObjects.Gameplay.DoubleDamageActive) {
				damage *= 2f;
			}

			if (enemyHealth.ApplyDamage(Object.InputAuthority, damage, position, direction, Type, isCriticalHit) == false)
				return;

			if (HasInputAuthority && Runner.IsForward) {
				_sceneObjects.GameUI.PlayerView.Crosshair.ShowHit(enemyHealth.IsAlive == false, isCriticalHit);
			}
		}

		private void PlayEmptyClipSound(bool fireJustPressed) {
			bool firstEmptyShot = _fireCooldown.TargetTick.GetValueOrDefault() == Runner.Tick - 1;

			if (fireJustPressed == false && firstEmptyShot == false)
				return;

			if (EmptyClipSound == null || EmptyClipSound.isPlaying)
				return;

			if (Runner.IsForward && HasInputAuthority) {
				EmptyClipSound.Play();
			}
		}

        private void EnterADS() {
            Animator.SetTrigger("enterADS");
			GetComponentInParent<Player>().isAiming = true;
			GetComponentInParent<Player>().ZoomIn();
            Animator.ResetTrigger("exitADS");
        }

        private void ExitADS() {
            Animator.SetTrigger("exitADS");
			GetComponentInParent<Player>().isAiming = false;
			GetComponentInParent<Player>().ZoomOut();
        }

        private struct ProjectileData : INetworkStruct {
			public Vector3 HitPosition;
			public Vector3 HitNormal;
			public NetworkBool ShowHitEffect;
		}
	}
}
