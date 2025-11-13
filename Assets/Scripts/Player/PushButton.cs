using UnityEngine;

namespace Player
{
    public class PushButton : MonoBehaviour
    {
        [SerializeField] private float force = 10f;
        [SerializeField] private float raycastDistance = 1.1f;
        [SerializeField] private LayerMask layerMask;
        private Transform _player;

        private void Awake()
        {
            _player = transform.root;
        }

        private void Update()
        {
            if (Physics.Raycast(_player.position, Vector3.down, out var hit, raycastDistance, layerMask))
            {
                Push(hit.rigidbody, hit.point);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * raycastDistance);
        }

        public void Push(Rigidbody rb, Vector3 hitPoint)
        {
            rb.AddForceAtPosition(Vector3.down * force, hitPoint, ForceMode.Impulse);
        }
    }
}