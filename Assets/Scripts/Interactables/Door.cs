using UnityEngine;

namespace Interactables
{
    public class Door : MonoBehaviour
    {
        private static readonly int OpenDoor = Animator.StringToHash("OpenDoor");
        private static readonly int CloseDoor = Animator.StringToHash("CloseDoor");
        [SerializeField] private Animator animator;

        private void Awake()
        {
            if (!animator)
            {
                animator = GetComponent<Animator>();
            }
        }

        public void Toggle(bool isOpen)
        {
            animator.SetTrigger(isOpen ? OpenDoor : CloseDoor);
        }
    }
}

