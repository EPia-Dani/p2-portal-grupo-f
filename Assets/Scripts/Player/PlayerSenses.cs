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
            
                if (distanceToPlane < 0.1f)
                {
                    // Handle portal traversal
                    var destinationPortal = portal.GetComponent<Portal>().linkedPortal;
                    if (destinationPortal)
                    {
                        // Calculate the offset from the portal
                        Vector3 offset = transform.position - portal.position;

                        // Rotate the offset to match the destination portal's orientation
                        Quaternion rotationDifference =
                            Quaternion.FromToRotation(portal.forward, destinationPortal.transform.forward);
                        Vector3 rotatedOffset = rotationDifference * offset;

                        // Set the new position and rotation
                        transform.position = destinationPortal.transform.position + rotatedOffset + destinationPortal.transform.forward*.1f;
                        Vector3 newForward = rotationDifference * transform.forward;
                        transform.rotation = Quaternion.LookRotation(newForward, Vector3.up);
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