using UnityEngine;

namespace Player
{
    public class PlayerSenses : MonoBehaviour
    {
        private Collider playerCollider;
        private CharacterController characterController;

        private void Awake()
        {  
            playerCollider = GetComponent<Collider>();
            characterController = GetComponent<CharacterController>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Handle trigger enter logic
            if (other.CompareTag("WallDisabler"))
            {
                Debug.Log("Wall Disabler");
                playerCollider.excludeLayers |= LayerMask.GetMask("PortalAble");
                characterController.excludeLayers |= LayerMask.GetMask("PortalAble");
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            // Handle trigger exit logic
            if (other.CompareTag("WallDisabler"))
            {
                Debug.Log("Wall Enabler");
                playerCollider.excludeLayers &= ~LayerMask.GetMask("PortalAble");
                characterController.excludeLayers &= ~LayerMask.GetMask("PortalAble");
            }
        }
    }
}