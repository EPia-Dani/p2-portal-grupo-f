using UnityEngine;

namespace GravityGun
{
    public class PressureButton : MonoBehaviour
    {
        
        private Vector3 initialPosition;
        private bool _pressed;
        private ConfigurableJoint joint;
        void Start()
        {
            _pressed = false;
        }

        public void SetPressed(bool p)
        {
            string log = p ? "pressed" : "unpressed";
            Debug.Log($"Button {log}");
            _pressed = p;
        }
    }
}
    
