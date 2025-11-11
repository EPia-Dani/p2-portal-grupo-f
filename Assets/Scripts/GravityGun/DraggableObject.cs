using UnityEngine;

namespace GravityGun
{
    [RequireComponent(typeof(Rigidbody))]
    public class DraggableObject : MonoBehaviour
    {
        private void OnValidate()
        {
            var rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }
}