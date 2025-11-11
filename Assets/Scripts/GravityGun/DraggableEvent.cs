using Core.EventBus;
using UnityEngine;

namespace GravityGun
{
    public class DraggableEvent : IEvent
    {
        public (Rigidbody rb, Collider collider) objectToHold;
    }
}