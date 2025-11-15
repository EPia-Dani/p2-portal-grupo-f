using UnityEngine;
using UnityEngine.Events;

namespace Interactables
{
    public class LaserButton  : MonoBehaviour
    {
        public UnityEvent<bool> OnActive;

        private int receiving;
        
        public void Update()
        {
            if (receiving > 0)
            {
                receiving--;
            }
        }
        
        public void OnLaserHit()
        {
            if (receiving == 0)
            {
                
                OnActive.Invoke(true);
            }

            receiving = 2;

        }
    }
}