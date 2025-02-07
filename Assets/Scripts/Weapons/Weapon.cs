using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
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
		SMG11,
		AK47,
		RemingtonM870,
		Search,
        SuperShorty,
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
		public string Name;
		public Animator Animator;
		[FormerlySerializedAs("WeaponVisual")]
		public GameObject FirstPersonVisual;
        public GameObject ThirdPersonVisual;
        public GameObject PickupVisual;

        [Header("Fire Effect")]
		[FormerlySerializedAs("MuzzleTransform")]
		public Transform FirstPersonMuzzleTransform;
		public Transform ThirdPersonMuzzleTransform;
		public GameObject MuzzleEffectPrefab;
		public ProjectileVisual ProjectileVisualPrefab;
        public GameObject CartridgePosition;
        public GameObject CartridgePrefab;

        [Header("Sounds")]
		public AudioSource FireSound;
		public AudioSource ReloadingSound;
		public AudioSource EmptyClipSound;

		[Header("Switch")]
		public float TakeOutTime = 1f;
		public float TakeInTime = 1f;

		public bool HasAmmo => ClipAmmo > 0 || allAmmo > 0;

		[Networked]
		public NetworkBool IsCollected { get; set; }
		[Networked]
		public NetworkBool IsReloading { get; set; }
		[Networked]
		public int ClipAmmo { get; set; }
		internal int allAmmo;

		[Networked]
		private int _fireCount { get; set; }
		[Networked]
		internal TickTimer _fireCooldown { get; set; }
		[Networked, Capacity(32)]
		private NetworkArray<ProjectileData> _projectileData { get; }

		private int _fireTicks;
		private int _visibleFireCount;
		private bool _reloadingVisible;
		private float _saveDispersion;
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
			if (!GetComponentInParent<Weapons>().weaponTimer.ExpiredOrNotRunning(Runner)) {
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
                Quaternion dispersion = Quaternion.Euler(Random.insideUnitSphere * _saveDispersion);
                if (Dispersion > 0f) {
					var dispersionRotation = dispersion;
                    projectileDirection = dispersionRotation * fireDirection;
				}

				FireProjectile(firePosition, projectileDirection);
			}

            GameObject _cartridge = Instantiate(CartridgePrefab, CartridgePosition.transform.position, CartridgePosition.transform.rotation, FirstPersonVisual.transform);
            ParticleSystem particleSystem = _cartridge.GetComponentInChildren<ParticleSystem>();
			float _particleDelay = 0;
            IEnumerator PlayParticleWithDelay() {
                yield return new WaitForSeconds(_particleDelay);
                particleSystem.Play();
            }
            if (Type == EWeaponType.Shotgun) {
				_particleDelay = 0.5f;
            }
			if (PlayParticleWithDelay() != null)
				StartCoroutine(PlayParticleWithDelay());
            Destroy(_cartridge, 2f);

            _fireCooldown = TickTimer.CreateFromTicks(Runner, _fireTicks);
			ClipAmmo--;
        }

        public void Reload() {
			if (IsCollected == false)
				return;
			if (ClipAmmo >= MaxClipAmmo)
				return;
			if (allAmmo <= 0)
				return;
			if (IsReloading)
				return;
			if (_fireCooldown.ExpiredOrNotRunning(Runner) == false)
				return;
            if (Type == EWeaponType.Search)
                return;

            if (Type == EWeaponType.Shotgun && ClipAmmo < MaxClipAmmo - 1) {
                Animator.SetTrigger("ReturnReload");
                ReloadingSound.Play();
            }

            IsReloading = true;

			if (GetComponentInParent<Player>().isAiming == true) {
				ExitADS();
			}
            _fireCooldown = TickTimer.CreateFromSeconds(Runner, ReloadTime);
		}

		public void ToggleVisibility(bool isVisible) { 
			FirstPersonVisual.SetActive(isVisible);
			FirstPersonVisual.GetComponent<Animator>().enabled = isVisible;
			ThirdPersonVisual.SetActive(isVisible);

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
                        _remainingAmmo = GetComponentInParent<Player>().ammo45ACP;
                        break;
                    case EWeaponName.SMG11:
                        _remainingAmmo = GetComponentInParent<Player>().ammo45ACP;
                        break;
                    case EWeaponName.SuperShorty:
                        _remainingAmmo = GetComponentInParent<Player>().ammo12Gauge;
                        break;
                    case EWeaponName.AK47:
                        _remainingAmmo = GetComponentInParent<Player>().ammo7_62mm;
                        break;
                    case EWeaponName.RemingtonM870:
                        _remainingAmmo = GetComponentInParent<Player>().ammo12Gauge;
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

            if (ClipAmmo == 0) {
				if (GetComponentInParent<Player>().isAiming)
					ExitADS();
                Reload();
            }

            if (IsReloading && _fireCooldown.ExpiredOrNotRunning(Runner)) {
                Animator.ResetTrigger("exitADS");
                IsReloading = false;

                int reloadAmmo = MaxClipAmmo - ClipAmmo;
				int _remainingAmmo = 0;
                switch (WeaponName) {
                    case EWeaponName.M1911:
						_remainingAmmo = GetComponentInParent<Player>().ammo45ACP;
                        break;
                    case EWeaponName.SMG11:
                        _remainingAmmo = GetComponentInParent<Player>().ammo45ACP;
                        break;
                    case EWeaponName.SuperShorty:
                        _remainingAmmo = GetComponentInParent<Player>().ammo12Gauge;
                        break;
                    case EWeaponName.AK47:
                        _remainingAmmo = GetComponentInParent<Player>().ammo7_62mm;
                        break;
                    case EWeaponName.RemingtonM870:
                        _remainingAmmo = GetComponentInParent<Player>().ammo12Gauge;
                        break;
                }
                reloadAmmo = Mathf.Min(reloadAmmo, _remainingAmmo);
				
				if (Type == EWeaponType.Shotgun) {
					if (ClipAmmo != MaxClipAmmo) {
						Animator.SetBool("ReloadEnd", false);
						Reload();
					} else if (ClipAmmo == MaxClipAmmo) {
						Animator.SetBool("ReloadEnd", true);
					}
					if (GetComponentInParent<Player>().ammo12Gauge == 0) {
						Animator.SetBool("ReloadEnd", true);
					}
				}

				if (Type == EWeaponType.Shotgun) {
					reloadAmmo = 1;
				}

				if (ClipAmmo < MaxClipAmmo && allAmmo > 0) {
					ClipAmmo += reloadAmmo;
					switch (WeaponName) {
						case EWeaponName.M1911:
							GetComponentInParent<Player>().ammo45ACP -= reloadAmmo;
							break;
                        case EWeaponName.SMG11:
                            GetComponentInParent<Player>().ammo45ACP -= reloadAmmo;
                            break;
                        case EWeaponName.SuperShorty:
                            GetComponentInParent<Player>().ammo12Gauge -= reloadAmmo;
                            break;
                        case EWeaponName.AK47:
							GetComponentInParent<Player>().ammo7_62mm -= reloadAmmo;
							break;
						case EWeaponName.RemingtonM870:
							GetComponentInParent<Player>().ammo12Gauge -= reloadAmmo;
							break;
					}
				}

                _fireCooldown = TickTimer.CreateFromSeconds(Runner, 0.5f);
			}
		}

        private void Update() {
            if (Input.GetMouseButtonDown(1) && Type != EWeaponType.Search && ClipAmmo > 0) {
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
					allAmmo = GetComponentInParent<Player>().ammo45ACP;
					break;
                case EWeaponName.SMG11:
                    allAmmo = GetComponentInParent<Player>().ammo45ACP;
                    break;
                case EWeaponName.SuperShorty:
                    allAmmo = GetComponentInParent<Player>().ammo12Gauge;
                    break;
                case EWeaponName.AK47:
                    allAmmo = GetComponentInParent<Player>().ammo7_62mm;
                    break;
                case EWeaponName.RemingtonM870:
                    allAmmo = GetComponentInParent<Player>().ammo12Gauge;
                    break;
            }

            if (!GetComponentInParent<Weapons>().weaponTimer.ExpiredOrNotRunning(Runner)) {
                _fireCooldown = TickTimer.None;
                IsReloading = false;
                ReloadingSound.Stop();
            }

			if (GetComponentInParent<Player>().isInteracting) {
				FirstPersonVisual.SetActive(false);
				PickupVisual.SetActive(true);
			} else {
                FirstPersonVisual.SetActive(true);
                PickupVisual.SetActive(false);
            }

            _saveDispersion = Dispersion;
            if (GetComponentInParent<Player>().isAiming) {
                _saveDispersion -= Dispersion / 10 * 4;
            }
            if (GetComponentInParent<Player>().isSneaking) {
                _saveDispersion -= Dispersion / 10 * 1;
            }
            if (GetComponentInParent<Player>().isCrouching) {
                _saveDispersion -= Dispersion / 10 * 3;
            }

            _sceneObjects.GameUI.PlayerView.Crosshair.TopCrossHair.transform.localPosition = new Vector3(0, _saveDispersion * 10);
            _sceneObjects.GameUI.PlayerView.Crosshair.BottomCrossHair.transform.localPosition = new Vector3(0, -_saveDispersion * 10);
            _sceneObjects.GameUI.PlayerView.Crosshair.LeftCrossHair.transform.localPosition = new Vector3(_saveDispersion * 10, 0);
            _sceneObjects.GameUI.PlayerView.Crosshair.RightCrossHair.transform.localPosition = new Vector3(-_saveDispersion * 10, 0);
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
                    ApplyDamage(hit.Hitbox, hit.Point, hit.Distance, fireDirection);
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

		private void ApplyDamage(Hitbox enemyHitbox, Vector3 position, float distance, Vector3 direction) {
			var enemyHealth = enemyHitbox.Root.GetComponent<Health>();
			if (enemyHealth == null || enemyHealth.IsAlive == false)
				return;

			float damageMultiplier = enemyHitbox is BodyHitbox bodyHitbox ? bodyHitbox.DamageMultiplier : 1f;
			bool isCriticalHit = damageMultiplier > 1f;

			float damage = Damage * damageMultiplier;
			if (_sceneObjects.Gameplay.DoubleDamageActive) {
				damage *= 2f;
			}

			switch (GetComponent<Weapons>().currentWeapon) {
				case EWeaponName.M1911:
					if (distance < 10f)
						break;
					else if (distance < 20f)
						damage *= 0.85f;
					else if (distance < 30f)
						damage *= 0.7f;
					else if (distance < 40f)
						damage *= 0.6f;
					else
						damage *= 0.5f;
					break;
				case EWeaponName.SMG11:
                    if (distance < 10f)
                        break;
                    else if (distance < 20f)
                        damage *= 0.85f;
                    else if (distance < 30f)
                        damage *= 0.7f;
                    else if (distance < 40f)
                        damage *= 0.6f;
                    else
                        damage *= 0.5f;
                    break;
                case EWeaponName.SuperShorty:
                    if (distance < 2f)
                        break;
                    else if (distance < 5f)
                        damage *= 0.8f;
                    else if (distance < 12f)
                        damage *= 0.6f;
                    else if (distance < 16f)
                        damage *= 0.4f;
                    else
                        damage *= 0.1f;
                    break;
                case EWeaponName.AK47:
                    if (distance < 20f)
                        break;
                    else if (distance < 30f)
                        damage *= 0.8f;
                    else if (distance < 40f)
                        damage *= 0.75f;
                    else
                        damage *= 0.7f;
                    break;
				case EWeaponName.RemingtonM870:
                    if (distance < 5f)
                        break;
                    else if (distance < 10f)
                        damage *= 0.6f;
                    else if (distance < 15f)
                        damage *= 0.4f;
                    else if (distance < 20f)
                        damage *= 0.3f;
                    else
                        damage *= 0.2f;
                    break;
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
