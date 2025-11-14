using Core.EventBus;
using UnityEngine;

namespace Player
{
    public struct JumpEvent : IEvent
    {
        public bool value;
    }

    public struct CrouchEvent : IEvent
    {
        public bool value;
    }

    public struct SprintEvent : IEvent
    {
        public bool value;
    }

    public struct LookEvent : IEvent
    {
        public Vector2 value;
    }

    public struct MoveEvent : IEvent
    {
        public Vector2 value;
    }

    public struct ShootBlueEvent : IEvent
    {
        public bool value;
    }
    
    public struct ShootOrangeEvent : IEvent
    {
        public bool value;
    }

    public struct BulletEvent : IEvent
    {
        public int value;
    }
    

    public struct HealEvent : IEvent
    {
        public float value;
    }

    public struct ShieldEvent : IEvent
    {
        public float value;
    }

    public struct AmmoEvent : IEvent
    {
        public int value;
    }

    public struct DamageEvent : IEvent
    {
        public float value;
    }
    
    public struct ShowE : IEvent
    {
        public bool value;
    }
    
    public struct PortalEventBlue : IEvent
    {
        public Vector3 destPosition;
        public Quaternion destRotation;
        public Vector3 destScale;
        public GameObject destObject;
    }
    
    public struct PortalEventOrange : IEvent
    {
        public Vector3 destPosition;
        public Quaternion destRotation;
        public Vector3 destScale;
        public GameObject destObject;
    }
    
    public struct SetYawAndPitchEvent : IEvent
    {
        public float yaw;
        public float pitch;
        public Quaternion rotationDelta;
    }

    public enum PlayerEventsEnum
    {
        Interact,
        Hittable,
        Death,
        Respawn,
        Gun,
        Shoot,
        ScrollUp,
        ScrollDown
    }
}