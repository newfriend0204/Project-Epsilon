using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectEpsilon {
    public enum EWeaponType {
		Pistol,
		Rifle,
		Shotgun,
		Search,
	}

	public enum EWeaponName {
		M1911,
		SMG,
		AK47,
		RemingtonM870,
		Search,
	}

	public class Weapon : NetworkBehaviour {
		public EWeaponType Type;
		public EWeaponName WeaponName;

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

		public bool HasAmmo => ClipAmmo > 0 || _allAmmo > 0;//|| RemainingAmmo > 0;

		[Networked]
		public NetworkBool IsCollected { get; set; }
		[Networked]
		public NetworkBool IsReloading { get; set; }
		[Networked]
		public int ClipAmmo { get; set; }
		private int _allAmmo;
		//[Networked]
		//public int RemainingAmmo { get; set; }

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
			if (_fireCooldown.ExpiredOrNotRunning(Runner) == false && !IsReloading)
				return;
			if (Type == EWeaponType.Search)
				return;
            if (GetComponentInParent<Player>().isRunning) {
				return;
            }

            if (IsReloading && ClipAmmo > 0) {
                _fireCooldown = TickTimer.None;
                IsReloading = false;
                ReloadingSound.Stop();
            }

            if (ClipAmmo <= 0) {
				PlayEmptyClipSound(justPressed);
				return;
			}

			Random.InitState(Runner.Tick * unchecked((int)Object.Id.Raw));

			for (int i = 0; i < ProjectilesPerShot; i++) {
				var projectileDirection = fireDirection;
				float _saveDispersion = Dispersion;
                if (GetComponentInParent<Player>().isAiming) {
                    _saveDispersion -= Dispersion / 10 * 4;
                }
                if (GetComponentInParent<Player>().isSneaking) {
                    _saveDispersion -= Dispersion / 10 * 1;
                }
                if (GetComponentInParent<Player>().isCrouching) {
                    _saveDispersion -= Dispersion / 10 * 3;
                }
                Quaternion dispersion = Quaternion.Euler(Random.insideUnitSphere * _saveDispersion);
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
			//if (RemainingAmmo <= 0)
			//	return;
			if (IsReloading)
				return;
			if (_fireCooldown.ExpiredOrNotRunning(Runner) == false)
				return;
            if (Type == EWeaponType.Search)
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

		//public void AddAmmo(int amount) {
  //          switch (WeaponName) {
  //              case EWeaponName.M1911:
  //                  GetComponentInParent<Player>().bullet9mm += amount;
  //                  break;
  //              case EWeaponName.SMG:
  //                  GetComponentInParent<Player>().bullet9mm += amount;
  //                  break;
  //              case EWeaponName.AK47:
  //                  GetComponentInParent<Player>().bullet5_56mm += amount;
  //                  break;
  //              case EWeaponName.RemingtonM870:
  //                  GetComponentInParent<Player>().bulletShell += amount;
  //                  break;
  //          }
		//}

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
                int _remainingAmmo = 0;
                switch (WeaponName) {
                    case EWeaponName.M1911:
                        _remainingAmmo = GetComponentInParent<Player>().bullet9mm;
                        break;
                    case EWeaponName.SMG:
                        _remainingAmmo = GetComponentInParent<Player>().bullet9mm;
                        break;
                    case EWeaponName.AK47:
                        _remainingAmmo = GetComponentInParent<Player>().bullet5_56mm;
                        break;
                    case EWeaponName.RemingtonM870:
                        _remainingAmmo = GetComponentInParent<Player>().bulletShell;
                        break;
                }
                _remainingAmmo = StartAmmo - ClipAmmo;
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

            if (IsReloading && _fireCooldown.ExpiredOrNotRunning(Runner)) {
				IsReloading = false;

				Animator.ResetTrigger("exitADS");

                int reloadAmmo = MaxClipAmmo - ClipAmmo;
				int _remainingAmmo = 0;
                switch (WeaponName) {
                    case EWeaponName.M1911:
						_remainingAmmo = GetComponentInParent<Player>().bullet9mm;
                        break;
                    case EWeaponName.SMG:
                        _remainingAmmo = GetComponentInParent<Player>().bullet9mm;
                        break;
                    case EWeaponName.AK47:
                        _remainingAmmo = GetComponentInParent<Player>().bullet5_56mm;
                        break;
                    case EWeaponName.RemingtonM870:
                        _remainingAmmo = GetComponentInParent<Player>().bulletShell;
                        break;
                }
                reloadAmmo = Mathf.Min(reloadAmmo, _remainingAmmo);
				
				if (Type == EWeaponType.Shotgun) {
					reloadAmmo = 1;
				}

				ClipAmmo += reloadAmmo;
                switch (WeaponName) {
                    case EWeaponName.M1911:
                        GetComponentInParent<Player>().bullet9mm -= reloadAmmo;
                        break;
                    case EWeaponName.SMG:
                        GetComponentInParent<Player>().bullet9mm -= reloadAmmo;
                        break;
                    case EWeaponName.AK47:
                        GetComponentInParent<Player>().bullet5_56mm -= reloadAmmo;
                        break;
                    case EWeaponName.RemingtonM870:
                        GetComponentInParent<Player>().bulletShell -= reloadAmmo;
                        break;
                }

				if (Type == EWeaponType.Shotgun) {
					if (ClipAmmo != MaxClipAmmo) {
						Animator.SetBool("ReloadEnd", false);
						Reload();
					} else if (ClipAmmo == MaxClipAmmo) {
						Animator.SetBool("ReloadEnd", true);
					}
					if (GetComponentInParent<Player>().bulletShell == 0) {
						Animator.SetBool("ReloadEnd", true);
					}
				}

                _fireCooldown = TickTimer.CreateFromSeconds(Runner, 0.5f);
			}
		}

        private void Update() {
            if (Input.GetMouseButtonDown(1) && Type != EWeaponType.Search) {
                _fireCooldown = TickTimer.None;
                IsReloading = false;
                ReloadingSound.Stop();
				EnterADS();
            } else if (Input.GetMouseButtonUp(1) && !IsReloading && Type != EWeaponType.Search) {
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

			if (Input.GetKey(KeyCode.LeftControl) && !GetComponentInParent<Player>().isAiming) {
				if (GetComponentInParent<Player>().isCrouching) {
                    GetComponentInParent<Player>().isCrouching = !GetComponentInParent<Player>().isCrouching;
                    if (GetComponentInParent<Player>().isCrouching) {
                        StartCoroutine(GetComponentInParent<Player>().MoveCamera(GetComponentInParent<Player>().originalPosition));
                        GetComponentInParent<Player>().KCC.SetHeight(1.2f);
                    } else {
                        StartCoroutine(GetComponentInParent<Player>().MoveCamera(GetComponentInParent<Player>().crounchPosition));
                        GetComponentInParent<Player>().KCC.SetHeight(1.8f);
                    }
                }
				GetComponentInParent<Player>().isRunning = true;
				if (IsReloading) {
					_fireCooldown = TickTimer.None;
					IsReloading = false;
					ReloadingSound.Stop();
				}
            } else {
				GetComponentInParent<Player>().isRunning = false;
            }

			if (!GetComponentInParent<Player>().isMoving) {
				GetComponentInParent<Player>().isRunning = false;
			}
            Animator.SetBool("IsRunning", GetComponentInParent<Player>().isRunning);

			if (_muzzleEffectInstance != null) {
				if (!_muzzleEffectInstance.GetComponent<ParticleSystem>().isPlaying) {
					_muzzleEffectInstance.SetActive(false);
				}
			}

			if (Type == EWeaponType.Search) {
				GetComponentInParent<Player>().isSearching = true;
            } else {
				GetComponentInParent<Player>().isSearching = false;

            }

            float _saveSpeed = 1;
			if (GetComponentInParent<Player>().isSneaking) {
				_saveSpeed -= 0.5f;
			}
            if (GetComponentInParent<Player>().isCrouching) {
                _saveSpeed -= 0.1f;
            }
			Animator.SetFloat("speed", _saveSpeed);

			switch (WeaponName) {
				case EWeaponName.M1911:
					_allAmmo = GetComponentInParent<Player>().bullet9mm;
					break;
				case EWeaponName.SMG:
                    _allAmmo = GetComponentInParent<Player>().bullet9mm;
					break;
                case EWeaponName.AK47:
                    _allAmmo = GetComponentInParent<Player>().bullet5_56mm;
                    break;
                case EWeaponName.RemingtonM870:
                    _allAmmo = GetComponentInParent<Player>().bulletShell;
                    break;
            }
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

			_muzzleEffectInstance.SetActive(true);
			_muzzleEffectInstance.GetComponent<ParticleSystem>().Play();

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

        public void EnterADS() {
            Animator.SetTrigger("enterADS");
			GetComponentInParent<Player>().isAiming = true;
			GetComponentInParent<Player>().ZoomIn();
            Animator.ResetTrigger("exitADS");
        }

        public void ExitADS() {
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
