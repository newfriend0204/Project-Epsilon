using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Cinemachine;
using System.Collections;

namespace ProjectEpsilon {
    [DefaultExecutionOrder(-5)]
    public class Player : NetworkBehaviour {
        [Header("Components")]
        public SimpleKCC KCC;
        public Weapons Weapons;
        public Health Health;
        public Animator Animator;
        public HitboxRoot HitboxRoot;

        [Header("Setup")]
        public float MoveSpeed = 6f;
        public float JumpForce = 10f;
        public AudioSource JumpSound;
        public AudioSource SearchSound;
        public AudioClip[] JumpClips;
        public GameObject CameraHandle;
        public GameObject FirstPersonRoot;
        public GameObject ThirdPersonRoot;
        public NetworkObject SprayPrefab;
        public CinemachineVirtualCamera cam;

        [Header("Movement")]
        public float UpGravity = 15f;
        public float DownGravity = 25f;
        public float GroundAcceleration = 55f;
        public float GroundDeceleration = 25f;
        public float AirAcceleration = 25f;
        public float AirDeceleration = 1.3f;

        [Networked]
        private NetworkButtons _previousButtons { get; set; }
        [Networked]
        private int _jumpCount { get; set; }
        [Networked]
        private Vector3 _moveVelocity { get; set; }

        internal bool isAiming = false;
        internal bool isMoving = false;
        internal bool isCrouching = false;
        internal bool isSneaking = false;
        internal bool isRunning = false;
        internal bool isSearching = false;

        [Networked]
        internal int ammo45ACP { get; set; }
        [Networked]
        internal int ammo7_62mm { get; set; }
        [Networked]
        internal int ammo12Gauge { get; set; }

        private int _visibleJumpCount;
        private float _saveSpeed;
        internal Vector3 originalPosition = new Vector3(0f, 1.2f, 0f);
        internal Vector3 crounchPosition = new Vector3(0f, 1.678634f, 0f);
        internal bool isInteracting = false;
        private float _interactionTime = 0f;

        private SceneObjects _sceneObjects;

        public void PlayFireEffect() {
            if (Mathf.Abs(GetAnimationMoveVelocity().x) > 0.2f)
                return;

            Animator.SetTrigger("Fire");
        }

        public override void Spawned() {
            name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";

            SetFirstPersonVisuals(HasInputAuthority);

            if (HasInputAuthority == false) {
                var virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);
                for (int i = 0; i < virtualCameras.Length; i++) {
                    virtualCameras[i].enabled = false;
                }
            }

            _sceneObjects = Runner.GetSingleton<SceneObjects>();
        }

        public override void FixedUpdateNetwork() {
            if (_sceneObjects.Gameplay.State == EGameplayState.Finished) {
                MovePlayer();
                return;
            }

            if (Health.IsAlive == false) {
                MovePlayer();

                KCC.SetColliderLayer(LayerMask.NameToLayer("Ignore Raycast"));
                KCC.SetCollisionLayerMask(LayerMask.GetMask("Default"));

                HitboxRoot.HitboxRootActive = false;

                SetFirstPersonVisuals(false);
                return;
            }

            if (GetInput(out NetworkedInput input)) {
                ProcessInput(input);
            } else {
                MovePlayer();
                RefreshCamera();
            }

            if (GetComponent<Weapons>().currentWeapon == EWeaponName.Search) {
                isSearching = true;
                if (!SearchSound.isPlaying) {
                    SearchSound.Play();
                }
            } else {
                isSearching = false;
                SearchSound.Stop();
            }

            _saveSpeed = MoveSpeed;
            if (isAiming) {
                _saveSpeed -= MoveSpeed / 10 * 1;
            }
            if (isCrouching) {
                _saveSpeed -= MoveSpeed / 10 * 3;
            }
            if (isSneaking) {
                _saveSpeed -= MoveSpeed / 10 * 5;
            }
            if (isRunning) {
                _saveSpeed += MoveSpeed / 10 * 6;
            }
            if (GetComponent<Weapons>().currentWeapon == GetComponent<Weapons>().currentSidearm) {
                _saveSpeed += MoveSpeed / 10 * 0.5f;
            }
            if (GetComponent<Weapons>().currentWeapon == GetComponent<Weapons>().currentPrimary) {
                _saveSpeed -= MoveSpeed / 10 * 1.5f;
            }   
            if (GetComponent<Weapons>().currentWeapon == EWeaponName.Search) {
                _saveSpeed += MoveSpeed / 10 * 0.5f;
            }
        }

        public override void Render() {
            if (_sceneObjects.Gameplay.State == EGameplayState.Finished)
                return;

            var moveVelocity = GetAnimationMoveVelocity();

            Animator.SetFloat("LocomotionTime", Time.time * 2f);
            Animator.SetBool("IsAlive", Health.IsAlive);
            Animator.SetBool("IsGrounded", KCC.IsGrounded);
            if (Weapons.CurrentWeapon != null)
                Animator.SetBool("IsReloading", Weapons.CurrentWeapon.IsReloading);
            Animator.SetBool("IsCrouching", isCrouching);
            Animator.SetFloat("MoveX", moveVelocity.x, 0.05f, Time.deltaTime);
            Animator.SetFloat("MoveZ", moveVelocity.z, 0.05f, Time.deltaTime);
            Animator.SetFloat("MoveSpeed", moveVelocity.magnitude);
            Animator.SetFloat("Look", -KCC.GetLookRotation(true, false).x / 90f);

            if (Health.IsAlive == false) {

                int upperBodyLayerIndex = Animator.GetLayerIndex("UpperBody");
                Animator.SetLayerWeight(upperBodyLayerIndex, Mathf.Max(0f, Animator.GetLayerWeight(upperBodyLayerIndex) - Time.deltaTime));

                int lowerBodyLayerIndex = Animator.GetLayerIndex("LowerBody");
                Animator.SetLayerWeight(lowerBodyLayerIndex, Mathf.Max(0f, Animator.GetLayerWeight(lowerBodyLayerIndex) - Time.deltaTime));

                int lookLayerIndex = Animator.GetLayerIndex("Look");
                Animator.SetLayerWeight(lookLayerIndex, Mathf.Max(0f, Animator.GetLayerWeight(lookLayerIndex) - Time.deltaTime));
            }

            if (_visibleJumpCount < _jumpCount) {
                Animator.SetTrigger("Jump");

                JumpSound.clip = JumpClips[Random.Range(0, JumpClips.Length)];
                JumpSound.Play();
            }

            _visibleJumpCount = _jumpCount;
        }

        private void LateUpdate() {
            if (HasInputAuthority == false)
                return;

            RefreshCamera();
        }

        private void ProcessInput(NetworkedInput input) {
            KCC.AddLookRotation(input.LookRotationDelta, -89, 89);

            KCC.SetGravity(KCC.RealVelocity.y >= 0f ? -UpGravity : -DownGravity);

            var inputDirection = KCC.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            var jumpImpulse = 0f;

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Jump) && KCC.IsGrounded) {
                jumpImpulse = JumpForce;
            }

            MovePlayer(inputDirection * _saveSpeed, jumpImpulse);
            RefreshCamera();

            if (KCC.HasJumped) {
                _jumpCount++;
            }

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Crouch)) {
                isCrouching = !isCrouching;
                if (isCrouching) {
                    StartCoroutine(MoveCamera(originalPosition));
                    KCC.SetHeight(1.2f);
                } else {
                    StartCoroutine(MoveCamera(crounchPosition));
                    KCC.SetHeight(1.8f);
                }
            }

            if (input.Buttons.IsSet(EInputButton.Fire) && !GetComponent<Weapons>().IsSwitching) {
                bool justPressed = input.Buttons.WasPressed(_previousButtons, EInputButton.Fire);
                Weapons.Fire(justPressed);
                Health.StopImmortality();
            } else if (input.Buttons.IsSet(EInputButton.Reload)) {
                Weapons.Reload();
            }

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Search)) {
                Weapons.SwitchWeapon(0);
            } else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Sidearm)) {
                Weapons.SwitchWeapon(1);
            } else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Primary)) {
                Weapons.SwitchWeapon(2);
            }

            if (Runner.GetPhysicsScene().Raycast(CameraHandle.transform.position, KCC.LookDirection, out var hit, 2.5f, LayerMask.GetMask("Item"), QueryTriggerInteraction.Ignore)) {
                if (input.Buttons.WasPressed(_previousButtons, EInputButton.Interact) && HasStateAuthority) {
                    if (GetComponent<Weapons>().CurrentWeapon != null) {
                        if (GetComponent<Weapons>().CurrentWeapon.IsReloading) {
                            GetComponent<Weapons>().CurrentWeapon._fireCooldown = TickTimer.None;
                            GetComponent<Weapons>().CurrentWeapon.IsReloading = false;
                            GetComponent<Weapons>().CurrentWeapon.ReloadingSound.Stop();
                        }
                    }
                    _interactionTime = 0f;
                    isInteracting = true;
                }
                if (isInteracting) {
                    _interactionTime += Time.deltaTime;

                    if (_interactionTime >= 0.5f) {
                        switch (hit.collider.gameObject.name) {
                            case "M1911Collider":
                            case "SMG11Collider":
                            case "AK47Collider":
                            case "MP5Collider":
                            case "RemingtonM870Collider":
                            case "SuperShortyCollider":
                                hit.collider.gameObject.GetComponentInParent<WeaponPickup>().AcquireWeapon(gameObject);
                                break;
                            case "Ammo45ACPCollider":
                            case "Ammo7_62mmCollider":
                            case "Ammo12GaugeCollider":
                                hit.collider.gameObject.GetComponentInParent<AmmoPickup>().AcquireAmmo(gameObject);
                                break;
                        }
                        isInteracting = false;
                    }
                }
            } else {
                isInteracting = false;
                _interactionTime = 0f;
            }

            if (input.Buttons.WasReleased(_previousButtons, EInputButton.Interact)) {
                isInteracting = false;
                _interactionTime = 0f;
            }

            _previousButtons = input.Buttons;
        }

        private void RefreshCamera() {
            Vector2 lookRotation = KCC.GetLookRotation(true, false);
            CameraHandle.transform.localRotation = Quaternion.Euler(lookRotation.x, 0f, 0f);
        }

        private void MovePlayer(Vector3 desiredMoveVelocity = default, float jumpImpulse = default) {
            float acceleration = 1f;

            if (desiredMoveVelocity == Vector3.zero) {
                acceleration = KCC.IsGrounded == true ? GroundDeceleration : AirDeceleration;
                isMoving = false;
            } else {
                acceleration = KCC.IsGrounded == true ? GroundAcceleration : AirAcceleration;
                isMoving = true;
            }

            _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);
            KCC.Move(_moveVelocity, jumpImpulse);
        }

        private void SetFirstPersonVisuals(bool firstPerson) {
            FirstPersonRoot.SetActive(firstPerson);
            ThirdPersonRoot.SetActive(firstPerson == false);
        }

        private Vector3 GetAnimationMoveVelocity() {
            if (KCC.RealSpeed < 0.01f)
                return default;

            var velocity = KCC.RealVelocity;

            velocity.y = 0f;

            if (velocity.sqrMagnitude > 1f) {
                velocity.Normalize();
            }

            return transform.InverseTransformVector(velocity);
        }

        public void ZoomIn() {
            StartCoroutine(ChangeFOV(45, 0.07f));
        }

        public void ZoomOut() {
            StartCoroutine(ChangeFOV(60, 0.07f));
        }

        IEnumerator ChangeFOV(float endFOV, float duration) {
            float startFOV = cam.m_Lens.FieldOfView;
            float time = 0;
            while (time < duration) {
                cam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, time / duration);
                yield return null;
                time += Time.deltaTime;
            }
        }

        public IEnumerator MoveCamera(Vector3 targetPosition) {
            Vector3 startPosition = CameraHandle.transform.localPosition;
            float duration = 0.1f;
            float elapsedTime = 0f;

            while (elapsedTime < duration) {
                CameraHandle.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            CameraHandle.transform.localPosition = targetPosition;
        }
    }
}