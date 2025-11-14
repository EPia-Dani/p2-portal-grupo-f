using UnityEngine;

namespace Interactables
{
    public class Door : MonoBehaviour
    {
        private static readonly int Open = Animator.StringToHash("Open");
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
            animator.SetBool(Open, isOpen);
        }
    }
}

