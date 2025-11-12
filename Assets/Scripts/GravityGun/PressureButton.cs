using UnityEngine;

namespace GravityGun
{
    public class PressureButton : MonoBehaviour
    {
        
        private Vector3 initialPosition;
        private bool pressed;
        private ConfigurableJoint joint;
        void Start()
        {
            Debug.Log("Button");
            joint = GetComponent<ConfigurableJoint>();
            initialPosition = transform.position;
            pressed = false;
        }

        void Update()
        {
            float limit = Mathf.Abs(joint.linearLimit.limit);
            Vector3 delta = transform.localPosition - initialPosition;
            float proj = Vector3.Dot(delta, Vector3.up.normalized);
            
            Debug.Log(proj);
            Debug.Log(limit);

            if (Mathf.Abs(proj) > limit)
            {
                pressed = true;
                Debug.Log("Pressed");
            }
            else
            {
                pressed = false;
                Debug.Log("Not Pressed");
            }
        }
    }
}
    
