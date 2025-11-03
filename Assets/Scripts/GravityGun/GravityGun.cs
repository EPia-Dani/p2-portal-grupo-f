using System;
using Core.EventBus;
using Player;
using UnityEngine;
    
    public class GravityGun : MonoBehaviour
    {
        public float pickRange = 5f;
        public float holdDistance = 2f;
        public float holdSmooth = 10f;
        public float hoverAmplitude = 0.15f;
        public float hoverSpeed = 2f;
    
        private Camera playerCamera;
        private Rigidbody heldRb;
        private float originalDrag;
        private float hoverOffset;
      
    
        void Start()
        {
            playerCamera = Camera.main;
            EventBusVoid<PlayerEventsEnum>.Subscribe(PlayerEventsEnum.Interact, HandleInteract);
        }
        
        void OnDisable()
        {
            EventBusVoid<PlayerEventsEnum>.Unsubscribe(PlayerEventsEnum.Interact, HandleInteract);
        }
    
        void HandleInteract()
        {
            Debug.Log("TryPick");
            if (heldRb == null)
                TryPick();
            else
                Drop();
        }
    
        void TryPick()
        {
            if (playerCamera == null) return;
    
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
            if (Physics.Raycast(ray, out RaycastHit hit, pickRange))
            {
                var pickable = hit.collider.GetComponent<PickableCube>();
                if (pickable != null)
                {
                    var rb = pickable.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        heldRb = rb;
                        originalDrag = heldRb.linearDamping;
                        heldRb.useGravity = false;
                        heldRb.linearDamping = 10f;
                        heldRb.angularDamping = 10f;
                    }
                }
            }
        }
    
        void Drop()
        {
            if (heldRb == null) return;
            heldRb.useGravity = true;
            heldRb.linearDamping = originalDrag;
            heldRb.angularDamping = 0.05f;
            heldRb = null;
        }
    
        void FixedUpdate()
        {
            if (heldRb == null || playerCamera == null) return;
    
            Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
            hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            targetPos += playerCamera.transform.up * hoverOffset;
    
            Vector3 newPos = Vector3.Lerp(heldRb.position, targetPos, Time.fixedDeltaTime * holdSmooth);
            heldRb.MovePosition(newPos);
    
            // Optional: make the object face the player front
            Quaternion targetRot = Quaternion.LookRotation(playerCamera.transform.forward, playerCamera.transform.up);
            heldRb.MoveRotation(Quaternion.Slerp(heldRb.rotation, targetRot, Time.fixedDeltaTime * holdSmooth));
        }
    }