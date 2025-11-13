using UnityEngine;
using UnityEngine.Events;

namespace GravityGun
{
    public class PressureButtonTrigger : MonoBehaviour
    {
        public UnityEvent<bool> OnPressure;

        private void OnTriggerEnter(Collider other)
        {
            if (other.name.Equals("Stm_button02"))
            {
                OnPressure.Invoke(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.name.Equals("Stm_button02"))
            {
                OnPressure.Invoke(false);
            }
        }
    }
}