using Core.EventBus;
using Player;
using UnityEngine;

namespace GravityGun
{
    public class GravityGun : MonoBehaviour
    {
        public float pickRange = 5f;

        [Header("Ground Collision")]
        public float groundClearance = 0.05f;

        [Header("Smoothing")]
        public float springStrength = 100f;
        public float springDamping = 10f;
        public float maxSpeed = 30f;
        public float rotationSpeed = 10f;

        private Camera playerCamera;
        private Transform playerTransform;
        private CharacterController playerController;
        private (Rigidbody rb, Collider collider) heldObj;
        private float originalDrag;
        private float originalAngularDrag;
        private float holdDistance;

        private void Awake()
        {
            playerCamera = Camera.main;
            playerTransform = transform.root;
            playerController = playerTransform.GetComponent<CharacterController>();
            EventBusVoid<PlayerEventsEnum>.Subscribe(PlayerEventsEnum.Interact, HandleInteract);
            EventBus<DraggableEvent>.Subscribe(HandleDrag);
        }

        private void OnDisable()
        {
            EventBus<DraggableEvent>.Unsubscribe(HandleDrag);
            EventBusVoid<PlayerEventsEnum>.Unsubscribe(PlayerEventsEnum.Interact, HandleInteract);
        }

        private void HandleInteract()
        {
            if (heldObj.rb != null)
                Drop();
        }

        private void HandleDrag(DraggableEvent e)
        {
            if (heldObj.rb == null)
                Pick(e);
        }

        private void Pick(DraggableEvent e)
        {
            var (rb, collider) = e.objectToHold;
            originalDrag = rb.linearDamping;
            originalAngularDrag = rb.angularDamping;
            holdDistance = Vector3.Distance(playerCamera.transform.position, rb.position);

            rb.useGravity = false;
            rb.linearDamping = 2f;
            rb.angularDamping = 5f;

            heldObj = (rb, collider);
        }

        private void Drop()
        {
            if (heldObj.rb == null) return;

            heldObj.rb.useGravity = true;
            heldObj.rb.linearDamping = originalDrag;
            heldObj.rb.angularDamping = originalAngularDrag;

            heldObj.rb = null;
        }

        private void FixedUpdate()
        {
            if (heldObj.rb == null || playerCamera == null) return;

            var targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;

            float bottomOffset = heldObj.collider.bounds.extents.y;
            float checkDistance = holdDistance + bottomOffset + 1f;

            if (Physics.Raycast(targetPos, Vector3.down, out var hit, checkDistance))
            {
                float groundY = hit.point.y;
                float minCenterY = groundY + bottomOffset + groundClearance;

                if (targetPos.y - bottomOffset < groundY + groundClearance)
                {
                    targetPos.y = minCenterY;
                }
            }

            var playerVelocity = playerController != null ? playerController.velocity : Vector3.zero;

            var displacement = targetPos - heldObj.rb.position;
            var springForce = displacement * springStrength;

            var relativeVelocity = heldObj.rb.linearVelocity - playerVelocity;
            var dampingForce = -relativeVelocity * springDamping;

            var totalForce = springForce + dampingForce;
            heldObj.rb.AddForce(totalForce, ForceMode.Acceleration);

            if (heldObj.rb.linearVelocity.magnitude > maxSpeed)
            {
                heldObj.rb.linearVelocity = heldObj.rb.linearVelocity.normalized * maxSpeed;
            }

            var targetRot = Quaternion.LookRotation(playerCamera.transform.forward, playerCamera.transform.up);
            var newRotation = Quaternion.Slerp(heldObj.rb.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
            heldObj.rb.MoveRotation(newRotation);
        }
    }
}