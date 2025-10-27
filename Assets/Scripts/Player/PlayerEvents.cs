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

    public struct ShootEvent : IEvent
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

    public enum PlayerEventsEnum
    {
        Gun,
        Hittable,
        Death,
        Respawn
    }
}