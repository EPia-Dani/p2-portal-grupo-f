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
                    Debug.Log("All required triggers are active.");
                    OnAllTriggersActive.Invoke(true);
                }
            }
            else
            {
                activeTriggers--;
                if (activeTriggers < requiredTriggers)
                {
                    Debug.Log("Not all required triggers are active.");
                    OnAllTriggersActive.Invoke(false);
                }
            }
        }
    }
}