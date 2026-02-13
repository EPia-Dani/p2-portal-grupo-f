using System;
using System.Collections.Generic;
using System.Linq;
using Core.EventBus;
using Routines;
using UnityEngine;

namespace Player
{
    public class MoveModule : MonoBehaviour, IModule
    {
        private GameObject _player;
        private PitchController _pitchController;
        private Rigidbody _rigidbody;

        private bool _isCrouchRequested;
        private bool _isSprintRequested;
        private Vector2 _moveInput;

        private Action<CrouchEvent> _onCrouch;
        private Action<SprintEvent> _onSprint;
        private Action<MoveEvent> _onMove;
        private Action<JumpEvent> _onJump;
        private Action<SetYawAndPitchEvent> _onSetYawAndPitch;


        private List<Camera> _cameras;
        private float _originalFov;
        private Routine _jumpBufferRoutine;
        private Routine _coyoteTimeRoutine;
        private GameObject _hand;
        private Vector3 _handBaseLocalPosition;
        private bool _isJumpRequested;
        private PlayerStats _stats;

        private float _verticalVelocity;
        private float _landingLean;
        private float _jumpLeanImpulseDegrees;
        private float _landingLeanMaxDegrees;
        private float _gravity;
        private float _jumpHeight;
        private CapsuleCollider _capsuleCollider;

        public void InitializeModule(PlayerService playerService)
        {
            _stats = playerService.Stats;

            _player = playerService.Player;

            if (!_player.TryGetComponentRecursive(out _pitchController))
            {
                Debug.LogError("PitchController not found");
            }

            if (!_player.TryGetComponentRecursive(out _capsuleCollider))
            {
                Debug.LogError("CapsuleCollider not found");
            }

            if (!_player.TryGetComponentRecursive(out _rigidbody))
            {
                Debug.LogError("CharacterController not found");
            }

            _cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None).ToList();
            _originalFov = _cameras[0].fieldOfView;

            _hand = _player.GetChildRecursive("Hand");
            _handBaseLocalPosition = _hand.transform.localPosition;

            _onCrouch = (e) => _isCrouchRequested = e.value;
            _onSprint = (e) => _isSprintRequested = e.value;
            _onMove = (e) => _moveInput = e.value;

            EventBus<JumpEvent>.Subscribe(OnJump);
            EventBus<CrouchEvent>.Subscribe(_onCrouch);
            EventBus<SprintEvent>.Subscribe(_onSprint);
            EventBus<MoveEvent>.Subscribe(_onMove);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void FixedUpdate()
        {
            var move = new Vector3(_moveInput.x, 0, _moveInput.y);
            var tempVelocity = _player.transform.TransformDirection(move) * _stats.speed;
            var isSprint = _isSprintRequested && _rigidbody.linearVelocity.magnitude > _stats.runSpeedMultiplier * _stats.speed;
            var finalVelocity = tempVelocity * (isSprint ? _stats.runSpeedMultiplier : 1f);

            _rigidbody.linearVelocity = finalVelocity;
        }

        private void OnJump(JumpEvent e)
        {
            _isJumpRequested = e.value;
            if (e.value && !IsGrounded())
            {
                if (_jumpBufferRoutine.IsRunning()) return;

                _jumpBufferRoutine = Routine.Buffered(this, _stats.bufferJumpTime,
                    target => target.IsGrounded(), target => { target.PerformJump(); }).Run();
            }
        }

        private bool IsGrounded()
        {
            var origin = _capsuleCollider.bounds.center;
            var distance = _capsuleCollider.bounds.extents.y + _stats.groundCheckExtraDistance;
            return Physics.SphereCast(origin, _stats.groundCheckRadius, Vector3.down, out _, distance, ~0, QueryTriggerInteraction.Ignore);
        }

        private void PerformJump()
        {
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _landingLean = Mathf.Clamp(_landingLean - _jumpLeanImpulseDegrees, -_landingLeanMaxDegrees,
                _landingLeanMaxDegrees);

            _isJumpRequested = false;
        }

        public void InitializeModule()
        {
            throw new NotImplementedException();
        }
    }
}