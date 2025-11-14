using Core.EventBus;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class Dispatcher : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }
        private void OnMove(InputValue value)
        {
            EventBus<MoveEvent>.Invoke(new MoveEvent { value = value.Get<Vector2>() });
        }

        private void OnLook(InputValue value)
        {
            EventBus<LookEvent>.Invoke(new LookEvent { value = value.Get<Vector2>() });
        }

        private void OnJump(InputValue value)
        { 
            EventBus<JumpEvent>.Invoke(new JumpEvent { value = value.isPressed });
        }
        
        private void OnSprint(InputValue value)
        {
            EventBus<SprintEvent>.Invoke(new SprintEvent { value = value.isPressed });
        }

        private void OnCrouch(InputValue value)
        {
            EventBus<CrouchEvent>.Invoke(new CrouchEvent { value = value.isPressed });
        }

        private void OnPortalBlue(InputValue value)
        {
            EventBus<ShootBlueEvent>.Invoke(new ShootBlueEvent { value = value.isPressed });
        }

        private void OnPortalOrange(InputValue value)
        {
            EventBus<ShootOrangeEvent>.Invoke(new ShootOrangeEvent { value = value.isPressed });
        }

        private void OnInteract(InputValue value)
        {
            EventBusVoid<PlayerEventsEnum>.Invoke(PlayerEventsEnum.Interact);
        }
        
        private void OnShoot(InputValue value)
        {
            EventBusVoid<PlayerEventsEnum>.Invoke(PlayerEventsEnum.Shoot);
        }
        
        private void OnScroll(InputValue value)
        {
            var delta = value.Get<Vector2>().y;
            
            if (delta > 0)
            {
                EventBusVoid<PlayerEventsEnum>.Invoke(PlayerEventsEnum.ScrollUp);
            }
            else if (delta < 0)
            {
                EventBusVoid<PlayerEventsEnum>.Invoke(PlayerEventsEnum.ScrollDown);
            }
        }
        
    }
}