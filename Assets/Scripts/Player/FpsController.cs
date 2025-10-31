using System;
using System.Collections;
using System.Collections.Generic;
using Core.EventBus;
using UnityEngine;
using System.Linq;
using Hittables;
using Routines;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming

namespace Player
{
    public class FpsController : MonoBehaviour
    {
        private readonly struct MovementFrameState
        {
            public readonly bool GroundedBeforeMove;
            public readonly float VerticalVelocityBefore;

            public MovementFrameState(bool groundedBeforeMove, float verticalVelocityBefore)
            {
                GroundedBeforeMove = groundedBeforeMove;
                VerticalVelocityBefore = verticalVelocityBefore;
            }
        }

        [Header("Movement")] [SerializeField] private float _speed = 10f;
        [SerializeField] private float _bufferJumpTime = 0.2f;
        [SerializeField] private float _coyoteTime = 0.2f;
        [SerializeField] private float _runSpeedMultiplier = 1.5f;
        [SerializeField] private float _accelTime = 0.08f;
        [SerializeField] private float _accelTimeOnAir = 0.08f;
        [SerializeField] private float _decelTime = 0.12f;
        [SerializeField] private float _decelTimeOnAir = 0.12f;

        [Header("Jump & Gravity")] [SerializeField]
        private float _gravity = -9.81f;

        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _groundedGravity = -2f;

        [Header("Shooting")] 
        [SerializeField] private float shotStartupTime = 0.05f;
        [SerializeField] private LayerMask portalAbleLayer;
        [SerializeField] private float recoilTime = 0.2f;
        [SerializeField] private float shotSpread = 0.01f;
        [SerializeField] private GameObject portalBlue;
        [SerializeField] private GameObject portalOrange;
        private int bulletCount;

        [Header("Camera Look")] [SerializeField, Range(0f, 1f)]
        private float _sensitivity = 1f;

        [SerializeField] private bool _invertPitch = false;
        [SerializeField] private float _runFov = 90f;
        [SerializeField] private float _smoothFovTime = 0.1f;

        [Header("Bob")] [SerializeField] private float _bobYAmount = 0.5f;
        [SerializeField] private float _bobXAmount = 0.1f;
        [SerializeField] private float _bobSmoothTime = 0.1f;
        [SerializeField] private float _bobYFrequency = 10f;
        [SerializeField] private float _bobXFrequency = 15f;

        [Header("Tuning")] [SerializeField] private float _sprintSpeedThresholdFactor = 0.5f;
        [SerializeField] private float _movingSpeedThresholdFactor = 0.5f;
        [SerializeField] private float _sprintBobSpeedMultiplier = 1.25f;

        [Header("Leaning")] [SerializeField] private float _maxLeanRollDegrees = 10f;
        [SerializeField] private float _maxLeanPitchDegrees = 5f;
        [SerializeField] private float _leanSmoothTime = 0.1f;
        [SerializeField] private float _maxMouseLeanRollDegrees = 6f;
        [SerializeField] private float _maxMouseLeanPitchDegrees = 1f;
        [SerializeField] private float _mouseLeanTime = 0.2f;
        [SerializeField] private float _verticalLeanMultiplier = 0.12f;
        [SerializeField] private float _landingLeanMaxDegrees = 4f;
        [SerializeField] private float _landingLeanVelocityScale = 0.12f;
        [SerializeField] private float _landingLeanSmoothTime = 0.12f;
        [SerializeField] private float _jumpLeanImpulseDegrees = 2f;

        [Header("Audio")] [SerializeField] private AudioClip shootClip;
        [SerializeField] private AudioClip footstepsClip;
        private AudioSource audioSource;
        private AudioSource footstepsAudioSource;
        
        [Header("Recoil")]
        [SerializeField] private Vector3 _recoilPositionKickback = new Vector3(0f, 0f, -0.1f);
        [SerializeField] private Vector3 _recoilRotationKickback = new Vector3(-5f, 0f, 0f);
        [SerializeField] private float _recoilVerticalRotation = 3f;
        [SerializeField] private float _recoilSnapTime = 0.05f;
        [SerializeField] private float _recoilReturnTime = 0.2f;

        private float _yaw;
        private float _pitch;
        private PitchController _pitchController;
        private CharacterController _characterController;
        private GameObject _player;
        private float _currentLeanPitch;
        private float _currentLeanRoll;
        private float _leanPitchVelocity;
        private float _leanRollVelocity;
        private float _verticalVelocity;
        private bool _wasJumpPressed;
        private Vector3 _currentWorldVelocity;
        private Vector3 _worldVelocityRef;
        private float _landingLean;
        private float _landingLeanVelocity;
        private Camera _mainCamera;
        private List<Camera> _cameras;
        private float _originalFov;
        private float _smoothFovVelocity;
        private Routine _jumpBufferRoutine;
        private Routine _coyoteTimeRoutine;
        private GameObject _hand;
        private Vector3 _handBaseLocalPosition;
        private Vector3 _bobVelocity;
        private float _bobTime;
        private bool _canShoot = true;
        private bool _isDead = false;

        private bool _isJumpRequested;
        private bool _isCrouchRequested;
        private bool _isSprintRequested;
        private Vector2 _moveInput;
        private Vector2 _lookInput;

        private Action<CrouchEvent> _onCrouch;
        private Action<SprintEvent> _onSprint;
        private Action<MoveEvent> _onMove;
        private Action<LookEvent> _onLook;
        
        private Vector3 _recoilPositionOffset;
        private Vector3 _recoilRotationOffset;
        private Vector3 _recoilPositionVelocity;
        private Vector3 _recoilRotationVelocity;

        private void Awake()
        {
            _player = transform.root.gameObject;

            if (!_player.TryGetComponentRecursive(out _pitchController))
            {
                Debug.LogError("PitchController not found");
            }

            if (!_player.TryGetComponentRecursive(out _characterController))
            {
                Debug.LogError("CharacterController not found");
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                footstepsAudioSource = gameObject.AddComponent<AudioSource>();
                footstepsAudioSource.clip = footstepsClip;
                footstepsAudioSource.loop = true;
                footstepsAudioSource.playOnAwake = false;
            }

            _mainCamera = Camera.main;

            _cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None).ToList();
            _originalFov = _cameras[0].fieldOfView;

            _hand = _player.GetChildRecursive("Hand");
            _handBaseLocalPosition = _hand.transform.localPosition;

            _onCrouch = (e) => _isCrouchRequested = e.value;
            _onSprint = (e) => _isSprintRequested = e.value;
            _onMove = (e) => _moveInput = e.value;
            _onLook = (e) => _lookInput = e.value;

            EventBus<JumpEvent>.Subscribe(OnJump);
            EventBus<CrouchEvent>.Subscribe(_onCrouch);
            EventBus<SprintEvent>.Subscribe(_onSprint);
            EventBus<MoveEvent>.Subscribe(_onMove);
            EventBus<LookEvent>.Subscribe(_onLook);
            EventBus<ShootEvent>.Subscribe(TryShoot);
            EventBusVoid<PlayerEventsEnum>.Subscribe(PlayerEventsEnum.Death, OnPlayerDeath);
            EventBusVoid<PlayerEventsEnum>.Subscribe(PlayerEventsEnum.Respawn, OnPlayerRespawn);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnPlayerDeath()
        {
            _isDead = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            StopAllCoroutines();

            if (footstepsAudioSource != null && footstepsAudioSource.isPlaying)
            {
                footstepsAudioSource.Stop();
            }
        }

        public void OnPlayerRespawn()
        {
            _isDead = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnJump(JumpEvent e)
        {
            if (_isDead) return;

            _isJumpRequested = e.value;
            if (e.value && !_characterController.isGrounded)
            {
                if (_jumpBufferRoutine.IsRunning()) return;

                _jumpBufferRoutine = Routine.Buffered(this, _bufferJumpTime,
                    target => target._characterController.isGrounded, target => { target.PerformJump(); }).Run();
            }
        }

        private void Update()
        {
            if (_isDead) return;
            Look();
            CalculateMovement();
            var frame = CalculateVerticalMovement();
            Move();
            AfterMovement(frame);
            ApplyLean();
            ApplyBob();
        }
        
        private void PerformJump()
        {
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _landingLean = Mathf.Clamp(_landingLean - _jumpLeanImpulseDegrees, -_landingLeanMaxDegrees,
                _landingLeanMaxDegrees);

            _isJumpRequested = false;
        }

        private void Look()
        {
            _pitch += _lookInput.y * _sensitivity;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);

            _yaw += _lookInput.x * _sensitivity;

            _pitchController.SetPitch(_pitch, _invertPitch);
            _player.transform.rotation = Quaternion.Euler(0, _yaw, 0);
        }

        private void CalculateMovement()
        {
            var localInput = new Vector3(_moveInput.x, 0, _moveInput.y);
            var tempVelocity = _player.transform.TransformDirection(localInput) * _speed;
            var isSprint = _isSprintRequested &&
                           _characterController.velocity.magnitude >
                           _sprintSpeedThresholdFactor * _speed * _runSpeedMultiplier;
            var targetWorldVelocity = tempVelocity * (isSprint ? _runSpeedMultiplier : 1f);

            SetCameraFov(isSprint ? _runFov : _originalFov);

            var inputMagnitude = localInput.magnitude;

            var smoothTime = inputMagnitude switch
            {
                > 0.01f when _characterController.isGrounded => _accelTime,
                < 0.01f when _characterController.isGrounded => _decelTime,
                > 0.01f when !_characterController.isGrounded => _accelTimeOnAir,
                < 0.01f when !_characterController.isGrounded => _decelTimeOnAir,
                _ => 0
            };

            _currentWorldVelocity = Vector3.SmoothDamp(_currentWorldVelocity, targetWorldVelocity,
                ref _worldVelocityRef, smoothTime);
        }

        private MovementFrameState CalculateVerticalMovement()
        {
            var jumpPressed = _isJumpRequested;
            var groundedBeforeMove = _characterController.isGrounded;
            var verticalVelocityBefore = _verticalVelocity;
            if (_characterController.isGrounded)
            {
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = _groundedGravity;
                }

                if (jumpPressed && !_wasJumpPressed)
                {
                    PerformJump();
                }
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }

            _wasJumpPressed = jumpPressed;
            return new MovementFrameState(groundedBeforeMove, verticalVelocityBefore);
        }

        private void Move()
        {
            var total = _currentWorldVelocity + new Vector3(0f, _verticalVelocity, 0f);
            _characterController.Move(total * Time.deltaTime);
        }

        private void AfterMovement(MovementFrameState frame)
        {
            var groundedAfterMove = _characterController.isGrounded;
            if (frame.GroundedBeforeMove && !groundedAfterMove && !_wasJumpPressed)
            {
                if (!_coyoteTimeRoutine.IsRunning())
                {
                    _coyoteTimeRoutine = Routine.Buffered(this, _coyoteTime, target => target._isJumpRequested,
                        target => target.PerformJump()).Run();
                }
            }
            else if (groundedAfterMove && _coyoteTimeRoutine.IsRunning())
            {
                _coyoteTimeRoutine.Stop();
            }

            if (!frame.GroundedBeforeMove && groundedAfterMove)
            {
                var impactSpeed = Mathf.Abs(frame.VerticalVelocityBefore);
                var impulse = Mathf.Min(impactSpeed * _landingLeanVelocityScale, _landingLeanMaxDegrees);
                _landingLean = Mathf.Clamp(_landingLean + impulse, -_landingLeanMaxDegrees, _landingLeanMaxDegrees);
            }

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedGravity;
            }
        }

        private void SetCameraFov(float targetFov)
        {
            foreach (var cam in _cameras)
            {
                var smoothFovValue =
                    Mathf.SmoothDamp(cam.fieldOfView, targetFov, ref _smoothFovVelocity, _smoothFovTime);

                if (!Mathf.Approximately(smoothFovValue, cam.fieldOfView))
                {
                    cam.fieldOfView = smoothFovValue;
                }
            }
        }

        private void ApplyLean()
        {
            var rollFromMove = -_moveInput.x * _maxLeanRollDegrees;
            var pitchFromMove = -_moveInput.y * _maxLeanPitchDegrees;

            var lookDeltaX = _lookInput.x;
            var rollFromMouse = Mathf.Clamp(-lookDeltaX * _mouseLeanTime, -_maxMouseLeanRollDegrees,
                _maxMouseLeanRollDegrees);

            var lookDeltaY = _lookInput.y;
            var pitchFromMouse = Mathf.Clamp(lookDeltaY * _mouseLeanTime, -_maxMouseLeanPitchDegrees,
                _maxMouseLeanPitchDegrees);

            var pitchFromVertical = _characterController.isGrounded
                ? 0f
                : Mathf.Clamp(-_verticalVelocity * _verticalLeanMultiplier, -_maxLeanPitchDegrees,
                    _maxLeanPitchDegrees);

            _landingLean = Mathf.SmoothDamp(_landingLean, 0f, ref _landingLeanVelocity, _landingLeanSmoothTime);

            var combinedMaxRoll = _maxLeanRollDegrees + _maxMouseLeanRollDegrees;
            var targetLeanRoll = Mathf.Clamp(rollFromMove + rollFromMouse, -combinedMaxRoll, combinedMaxRoll);

            var combinedMaxPitch = Mathf.Max(_maxLeanPitchDegrees + _maxMouseLeanPitchDegrees, _landingLeanMaxDegrees);
            var targetLeanPitch = Mathf.Clamp(pitchFromMouse + pitchFromMove + pitchFromVertical + _landingLean,
                -combinedMaxPitch, combinedMaxPitch);

            _currentLeanRoll =
                Mathf.SmoothDamp(_currentLeanRoll, targetLeanRoll, ref _leanRollVelocity, _leanSmoothTime);
            _currentLeanPitch =
                Mathf.SmoothDamp(_currentLeanPitch, targetLeanPitch, ref _leanPitchVelocity, _leanSmoothTime);

            _pitchController.SetLean(_currentLeanPitch, _currentLeanRoll);
        }

        private void ApplyBob()
        {
            var targetSpeed = _isSprintRequested ? _speed * _runSpeedMultiplier : _speed;
            var isMoving = _characterController.velocity.magnitude > _movingSpeedThresholdFactor * targetSpeed;

            if (!_characterController.isGrounded || !isMoving)
            {
                _bobTime = 0f;
            }
            else
            {
                _bobTime += Time.deltaTime;
            }

            _recoilPositionOffset = Vector3.SmoothDamp(_recoilPositionOffset, Vector3.zero, 
                ref _recoilPositionVelocity, _recoilReturnTime);
            _recoilRotationOffset = Vector3.SmoothDamp(_recoilRotationOffset, Vector3.zero, 
                ref _recoilRotationVelocity, _recoilReturnTime);

            var bob = _characterController.isGrounded ? 1f : 0f;
            var bobSpeed = _isSprintRequested ? _sprintBobSpeedMultiplier : 1f;
            var velocityMagnitudeNormalized = _characterController.velocity.magnitude / targetSpeed;
            bob *= velocityMagnitudeNormalized;

            var bobY = Mathf.Sin(_bobTime * _bobYFrequency * bobSpeed) * _bobYAmount * bob;
            var bobX = Mathf.Cos(_bobTime * _bobXFrequency * bobSpeed) * _bobXAmount * bob;
            var currentPosition = _hand.transform.localPosition;
            var targetPosition = _handBaseLocalPosition + new Vector3(bobX, bobY, 0f) + _recoilPositionOffset;
            var smoothPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref _bobVelocity, _bobSmoothTime);
            _hand.transform.localPosition = smoothPosition;
            
            _hand.transform.localRotation = Quaternion.Euler(_recoilRotationOffset);
            
            
            if (_characterController.isGrounded && isMoving)
            {
                if (!footstepsAudioSource.isPlaying)
                {
                    footstepsAudioSource.Play();
                }
            }
            else
            {
                if (footstepsAudioSource.isPlaying)
                {
                    footstepsAudioSource.Stop();
                }
            }
        }

        private void TryShoot(ShootEvent e)
        {
            if (e.value)
            {
                if (_canShoot && !_isDead)
                {
                    EventBusVoid<PlayerEventsEnum>.Invoke(PlayerEventsEnum.Gun);
                    StartCoroutine(Shoot());
                }
            }
        }

        private void OnBulletCountChangedEvent(BulletEvent e)
        {
            bulletCount = e.value;
        }

        private IEnumerator Shoot()
        {
            _canShoot = false;
        
            yield return new WaitForSeconds(shotStartupTime);
            
            Debug.Log("Pew Pew");
        
            // audioSource.PlayOneShot(shootClip);
            
            // Apply recoil with vertical rotation
            _recoilPositionOffset = _recoilPositionKickback;
            _recoilRotationOffset = _recoilRotationKickback + new Vector3(_recoilVerticalRotation, 0f, 0f);
        
            var target = _mainCamera.ScreenToWorldPoint(new Vector3(
                _mainCamera.pixelWidth * (0.5f + Random.Range(-shotSpread, shotSpread) / _mainCamera.aspect),
                _mainCamera.pixelHeight * (0.5f + Random.Range(-shotSpread, shotSpread)),
                .3f)) - _mainCamera.transform.position;
        
            Physics.Raycast(_mainCamera.transform.position, target,
                out var hit, 100f, portalAbleLayer);
            
            if (hit.collider)
            {
                
                
                portalBlue.transform.position = hit.point + hit.normal * 0.01f;
                portalBlue.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
        
            yield return new WaitForSeconds(recoilTime);
        
            _canShoot = true;
        }

        private void OnDestroy()
        {
            EventBus<JumpEvent>.Unsubscribe(OnJump);
            EventBus<CrouchEvent>.Unsubscribe(_onCrouch);
            EventBus<SprintEvent>.Unsubscribe(_onSprint);
            EventBus<MoveEvent>.Unsubscribe(_onMove);
            EventBus<LookEvent>.Unsubscribe(_onLook);
            EventBus<ShootEvent>.Unsubscribe(TryShoot);
            EventBusVoid<PlayerEventsEnum>.Unsubscribe(PlayerEventsEnum.Death, OnPlayerDeath);
        }
    }
}