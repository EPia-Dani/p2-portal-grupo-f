using UnityEngine;

namespace Player
{
    public class Follow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;

        private void Update()
        {
            transform.position = target.position + offset;
            transform.rotation = target.rotation;
        }
    }
}