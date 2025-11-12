using Core.EventBus;
using Portals;
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
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("WallDisabler"))
            {
                var portal = other.transform.parent;
            
                // Create a plane oriented towards the portal's forward vector
                Plane portalPlane = new Plane(portal.forward, portal.position);
                float distanceToPlane = portalPlane.GetDistanceToPoint(transform.position);
            
                if (distanceToPlane < 0.05f)
                {
                    // Handle portal traversal
                    var destinationPortal = portal.GetComponent<Portal>().linkedPortal;
                    if (destinationPortal)
                    {
                        // Calculate the offset from the portal
                        Vector3 offset = transform.position - portal.position;

                        // Calculate the rotation difference between portals
                        // This accounts for the full 3D rotation difference, not just forward direction
                        Quaternion rotationDifference = destinationPortal.transform.rotation * Quaternion.Inverse(portal.rotation);
                        
                        // Rotate the offset to match the destination portal's orientation
                        Vector3 rotatedOffset = rotationDifference * offset;
                        
                        // Set the new position
                        transform.position = destinationPortal.transform.position + rotatedOffset + destinationPortal.transform.forward * 0.01f;
                        
                        Quaternion newRotation = rotationDifference * transform.rotation;
                        
                        // Apply the rotation difference to the player's current rotation
                        EventBus<SetYawAndPitchEvent>.Invoke(new SetYawAndPitchEvent
                        {
                            yaw = -newRotation.eulerAngles.y,
                            pitch = newRotation.eulerAngles.x
                        });
                        // transform.rotation = rotationDifference * transform.rotation;
                    }
                }
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