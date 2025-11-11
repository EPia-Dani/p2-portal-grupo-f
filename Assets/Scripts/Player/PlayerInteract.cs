using Core.EventBus;
using UnityEngine;

namespace Player
{
    public class PlayerInteract : MonoBehaviour
    {
        private Camera playerCamera;
        private void Start()
        {
            playerCamera = Camera.main;
            EventBusVoid<PlayerEventsEnum>.Subscribe(PlayerEventsEnum.Interact, OnInteract);
        }
        
        private void OnDisable()
        {
            EventBusVoid<PlayerEventsEnum>.Unsubscribe(PlayerEventsEnum.Interact, OnInteract);
        }
        
        private void OnInteract()
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.TryGetComponent<Interactable>(out var interactable))
                {
                    interactable.Interact();
                }
            }
        }
    }
}