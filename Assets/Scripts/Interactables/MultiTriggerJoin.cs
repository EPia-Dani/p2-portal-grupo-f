using UnityEngine;
using UnityEngine.Events;

namespace Interactables
{
    public class MultiTriggerJoin : MonoBehaviour
    {
        
        [SerializeField] private int requiredTriggers = 2;
        private int activeTriggers = 0;
        
        public UnityEvent<bool> OnAllTriggersActive;

        public void Toggle(bool active)
        {
            if (active)
            {
                activeTriggers++;
                if (activeTriggers >= requiredTriggers)
                {
                    OnAllTriggersActive.Invoke(true);
                }
            }
            else
            {
                activeTriggers--;
                if (activeTriggers < requiredTriggers)
                {
                    OnAllTriggersActive.Invoke(false);
                }
            }
        }
    }
}