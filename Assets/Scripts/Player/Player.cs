using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Cinemachine;
using System.Collections;
using TMPro;

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
        public float JumpForce = 10f;
        [Networked]
        public float MoveSpeed { get; set; }
        public AudioSource JumpSound;
        public AudioSource SearchSound;
        public AudioClip[] JumpClips;
        public AudioSource VoiceSound;
        public AudioClip[] ReloadClips;
        public AudioClip[] SearchClips;
        public AudioClip[] HurtClips;
        public AudioClip[] KillClips;
        public AudioClip[] RespawnClips;
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

        public bool IsAiming { get; set; }
        [Networked]
        public bool IsMoving { get; set; }
        [Networked]
        public bool IsCrouching { get; set; }
        [Networked]
        public bool IsSneaking { get; set; }
        [Networked]
        public bool IsRunning { get; set; }
        [Networked]
        public bool IsSearching { get; set; }
        [Networked]
        internal bool IsDebuging { get; set; }

        private bool isPressedSneak = false;
        private bool isPressedRun = false;

        [Networked]
        internal int ammo45ACP { get; set; }
        [Networked]
        internal int ammo7_62mm { get; set; }
        [Networked]
        internal int ammo12Gauge { get; set; }

        private int _visibleJumpCount;
        private float _saveOriginalSpeed;
        internal Vector3 originalPosition = new Vector3(0f, 1.2f, 0f);
        internal Vector3 crounchPosition = new Vector3(0f, 1.678634f, 0f);
        internal bool isInteracting = false;
        private float _interactionTime = 0f;
        private SceneObjects _sceneObjects;

        public TextMeshProUGUI DebugText;

        public void PlayFireEffect() {
            if (Mathf.Abs(GetAnimationMoveVelocity().x) > 0.2f)
                return;

            Animator.SetTrigger("Fire");
        }

        public override void Spawned() {
            name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";

            SetFirstPersonVisuals(HasInputAuthority);
            ammo7_62mm = 100;
            ammo45ACP = 100;
            ammo12Gauge = 100;

            if (HasInputAuthority == false) {
                var virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);
                for (int i = 0; i < virtualCameras.Length; i++) {
                    virtualCameras[i].enabled = false;
                }
            }

            _sceneObjects = Runner.GetSingleton<SceneObjects>();

            GameObject gameUI = GameObject.Find("GameUI");
            if (gameUI != null) {
                Transform debugScreen = gameUI.transform.Find("PlayerView");
                if (debugScreen != null) {
                    Transform debugTextTransform = debugScreen.Find("DebugText");
                    if (debugTextTransform != null) {
                        DebugText = debugTextTransform.GetComponent<TextMeshProUGUI>();
                    }
                }
            }

            MoveSpeed = 5f;
            _saveOriginalSpeed = MoveSpeed;
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
            Animator.SetBool("IsCrouching", IsCrouching);
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

            if (GetComponent<Weapons>().currentWeapon == EWeaponName.Search) {
                IsSearching = true;
                if (!SearchSound.isPlaying) {
                    SearchSound.Play();
                }
            } else {
                IsSearching = false;
                SearchSound.Stop();
            }

            if (IsCrouching)
                KCC.SetHeight(1.2f);
            else
                KCC.SetHeight(1.737276f);

            if (!IsMoving) {
                IsRunning = false;
            }
        }

        private void LateUpdate() {
            if (HasInputAuthority == false)
                return;

            RefreshCamera();

            if (!IsDebuging)
                DebugText.text = "";
            else {
                DebugText.text =
                    "-----Player Status-----\r\n" +
                    "ObjectName: " + gameObject.name + "\r\n" +
                    "HasInputAuthority: " + HasInputAuthority + "\r\n" +
                    "HasStateAuthority: " + HasStateAuthority + "\r\n" +
                    "IsMoving: " + IsMoving + "\r\n" +
                    "IsCrouching: " + IsCrouching + "\r\n" +
                    "IsRunning: " + IsRunning + "\r\n" +
                    "IsSneaking: " + IsSneaking + "\r\n" +
                    "IsAiming: " + IsAiming + "\r\n" +
                    "IsSearching: " + IsSearching + "\r\n" +
                    "Primary: " + GetComponent<Weapons>().currentPrimary + "\r\n" +
                    "Sidearm: " + GetComponent<Weapons>().currentSidearm + "\r\n" +
                    "currentWeapon: " + GetComponent<Weapons>().currentWeapon + "\r\n" +
                    "_saveOriginalSpeed: " + _saveOriginalSpeed + "\r\n" +
                    "MoveSpeed: " + MoveSpeed + "\r\n" +
                    "ammo45ACP: " + ammo45ACP + "\r\n" +
                    "ammo7_62mm: " + ammo7_62mm + "\r\n" +
                    "ammo12Gauge: " + ammo12Gauge + "\r\n";
            }
        }

        private void ProcessInput(NetworkedInput input) {
            KCC.AddLookRotation(input.LookRotationDelta, -89, 89);

            KCC.SetGravity(KCC.RealVelocity.y >= 0f ? -UpGravity : -DownGravity);

            var inputDirection = KCC.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            var jumpImpulse = 0f;

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Jump) && KCC.IsGrounded) {
                jumpImpulse = JumpForce;
            }

            MoveSpeed = _saveOriginalSpeed;
            if (IsAiming) {
                MoveSpeed -= _saveOriginalSpeed / 10 * 1f;
            }
            if (IsCrouching) {
                MoveSpeed -= _saveOriginalSpeed / 10 * 3f;
            }
            if (IsSneaking) {
                MoveSpeed -= _saveOriginalSpeed / 10 * 4f;
            }
            if (IsRunning) {
                MoveSpeed += _saveOriginalSpeed / 10 * 5f;
            }
            if (GetComponent<Weapons>().currentWeapon == GetComponent<Weapons>().currentSidearm) {
                MoveSpeed += _saveOriginalSpeed / 10 * 0.5f;
            } else if (GetComponent<Weapons>().currentWeapon == GetComponent<Weapons>().currentPrimary) {
                MoveSpeed -= _saveOriginalSpeed / 10 * 0.5f;
            }
            if (GetComponent<Weapons>().currentWeapon == EWeaponName.Search) {
                MoveSpeed += _saveOriginalSpeed / 10 * 0.75f;
            }
            MovePlayer(inputDirection * MoveSpeed, jumpImpulse);
            RefreshCamera();

            if (KCC.HasJumped) {
                _jumpCount++;
            }

            if (input.Buttons.IsSet(EInputButton.Sneak)) {
                if (!isPressedSneak) {
                    isPressedSneak = true;
                    IsSneaking = true;
                }
            } else {
                if (isPressedSneak) {
                    isPressedSneak = false;
                    IsSneaking = false;
                }
            }

            if (input.Buttons.IsSet(EInputButton.Run) && !IsAiming) {
                if (IsCrouching) {
                    IsCrouching = false;
                    StartCoroutine(MoveCamera(crounchPosition));
                }
                if (!isPressedRun) {
                    isPressedRun = true;
                    IsRunning = true;
                }
                if (GetComponentInChildren<Weapon>().IsReloading) {
                    //GetComponentInChildren<Weapon>()._fireCooldown = TickTimer.None;
                    GetComponentInChildren<Weapon>().IsReloading = false;
                    GetComponentInChildren<Weapon>().ReloadingSound.Stop();
                }
            } else {
                if (isPressedRun) {
                    isPressedRun = false;
                    IsRunning = false;
                }
            }

            if (input.Buttons.IsSet(EInputButton.Fire) && !GetComponent<Weapons>().IsSwitching) {
                bool justPressed = input.Buttons.WasPressed(_previousButtons, EInputButton.Fire);
                Weapons.Fire(justPressed);
                Health.StopImmortality();
            } else if (input.Buttons.IsSet(EInputButton.Reload)) {
                Weapons.Reload();
            }

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Crouch)) {
                if (!IsCrouching) {
                    IsCrouching = true;
                    StartCoroutine(MoveCamera(originalPosition));
                } else {
                    IsCrouching = false;
                    StartCoroutine(MoveCamera(crounchPosition));
                }
            }

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Debug)) {
                if (!IsDebuging) {
                    IsDebuging = true;
                } else {
                    IsDebuging = false;
                }
            }

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Search)) {
                Weapons.SwitchWeapon(0);
            } else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Sidearm)) {
                Weapons.SwitchWeapon(1);
            } else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Primary)) {
                Weapons.SwitchWeapon(2);
            }

            if (Runner.GetPhysicsScene().Raycast(CameraHandle.transform.position, KCC.LookDirection, out var hit, 2.5f, LayerMask.GetMask("Item"), QueryTriggerInteraction.Ignore)) {
                if (input.Buttons.WasPressed(_previousButtons, EInputButton.Interact)) {
                    if (GetComponent<Weapons>().CurrentWeapon != null) {
                        if (GetComponent<Weapons>().CurrentWeapon.IsReloading) {
                            //GetComponent<Weapons>().CurrentWeapon._fireCooldown = TickTimer.None;
                            GetComponent<Weapons>().CurrentWeapon.IsReloading = false;
                            GetComponent<Weapons>().CurrentWeapon.ReloadingSound.Stop();
                        }
                    }
                    _interactionTime = 0f;
                    isInteracting = true;
                }
                if (isInteracting) {
                    _interactionTime += Time.fixedDeltaTime;

                    if (_interactionTime >= 0.6f) {
                        switch (hit.collider.gameObject.name) {
                            case "M1911Collider":
                            case "SMG11Collider":
                            case "AK47Collider":
                            case "MP5Collider":
                            case "RemingtonM870Collider":
                            case "SuperShortyCollider":
                                hit.collider.gameObject.GetComponentInParent<Pickup>().AcquireWeapon(gameObject, 1);
                                break;
                            case "Ammo45ACPCollider":
                            case "Ammo7_62mmCollider":
                            case "Ammo12GaugeCollider":
                                hit.collider.gameObject.GetComponentInParent<Pickup>().AcquireWeapon(gameObject, 2);
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
                IsMoving = false;
            } else {
                acceleration = KCC.IsGrounded == true ? GroundAcceleration : AirAcceleration;
                if (isPressedRun)
                    IsRunning = true;
                IsMoving = true;
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