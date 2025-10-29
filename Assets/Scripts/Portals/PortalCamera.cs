using UnityEngine;
using UnityEngine.Serialization;

namespace Portals
{
    public class PortalCamera : MonoBehaviour
    {
        private Transform _cameraTransform;
        private Transform _playerTransform;
        
        [SerializeField]
        private GameObject portal;
        [SerializeField]
        private GameObject otherPortal;
        
        void Awake()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            if (Camera.main) _cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            var relativePlayerPosition = portal.transform.InverseTransformPoint(_cameraTransform.position);
            var unProjectedCameraPosition = otherPortal.transform.TransformPoint(-relativePlayerPosition);
            transform.position = unProjectedCameraPosition;
            
            var relativePlayerRotation = Quaternion.Inverse(portal.transform.rotation) * _cameraTransform.rotation;
            var unProjectedCameraRotation = otherPortal.transform.rotation * relativePlayerRotation;
            transform.rotation = unProjectedCameraRotation * Quaternion.Euler(0, 180, 0);
            
            // var relativePlayerRotation = portal.transform.InverseTransformDirection(_cameraTransform.rotation.eulerAngles);
            // var unProjectedCameraRotation = otherPortal.transform.TransformDirection(relativePlayerRotation);
            // transform.rotation = Quaternion.Euler(unProjectedCameraRotation);
            
            
        }
    }
}
