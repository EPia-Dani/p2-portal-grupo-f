using UnityEngine;
using Player;
using Core.EventBus;

namespace GravityGun
{
    [RequireComponent(typeof(Rigidbody))]
    public class DraggableObject : MonoBehaviour, Interactable
    {
        private Rigidbody rb;
        private Collider col;
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
        }
        public void Interact()
        {
            EventBus<DraggableEvent>.Invoke(new DraggableEvent { objectToHold = (rb, col) });
        }

        private void OnValidate()
        {
            var rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }
}