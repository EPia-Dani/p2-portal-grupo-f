using System;
using Core.EventBus;
using UnityEngine;

namespace Player
{
    public class LookModule : MonoBehaviour, IModule
    {
        private PlayerStats _stats;
        private GameObject _player;
        private PitchController _pitchController;
        private float _yaw;
        private float _pitch;
        private Vector2 _lookInput;
        private Action<LookEvent> _onLook;

        public void InitializeModule(PlayerService playerService)
        {
            _stats = playerService.Stats;
            _player = playerService.Player;

            if (!playerService.TryGetModule(out _pitchController))
            {
                Debug.LogError("PitchController not found");
            }

            // Initialize yaw from current player rotation to avoid snapping on start
            _yaw = _player.transform.eulerAngles.y;
            _pitch = 0f;

            _onLook = (e) => _lookInput = e.value;
            EventBus<LookEvent>.Subscribe(_onLook);
        }


        private void FixedUpdate()
        {
            // Accumulate pitch (vertical) and clamp
            _pitch += _lookInput.y * _stats.sensitivity;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);

            // Accumulate yaw (horizontal)
            _yaw += _lookInput.x * _stats.sensitivity;

            // Apply rotations
            _pitchController.SetPitch(_pitch, _stats.invertPitch);
            _player.transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }
    }
}