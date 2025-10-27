using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.EventBus
{
    public static class EventBus<TOwner, TData> where TOwner : MonoBehaviour where TData : IEvent
    {
        private static readonly Dictionary<TOwner, Action<TData>> events = new();

        public static void Subscribe(TOwner owner, Action<TData> callback)
        {
            if (!events.ContainsKey(owner))
                events[owner] = null;
            events[owner] += callback;
        }

        public static void Unsubscribe(TOwner owner, Action<TData> callback)
        {
            if (events.ContainsKey(owner))
                events[owner] -= callback;
        }

        public static void Invoke(TOwner owner, TData eventData)
        {
            if (events.ContainsKey(owner))
                events[owner]?.Invoke(eventData);
        }

        public static void Clear(TOwner owner)
        {
            if (events.ContainsKey(owner))
                events[owner] = null;
        }

        public static void ClearAll()
        {
            events.Clear();
        }
    }

    public static class EventBus<TData> where TData : IEvent
    {
        private static Action<TData> @event;


        public static void Subscribe(Action<TData> callback)
        {
            @event += callback;
        }

        public static void Unsubscribe(Action<TData> callback)
        {
            @event -= callback;
        }

        public static void Invoke(TData data)
        {
            @event?.Invoke(data);
        }

        public static void Clear()
        {
            @event = null;
        }

        public static void ClearAll()
        {
            Clear();
        }
    }

    public static class EventBusVoid<TOwner, TEnum> where TOwner : MonoBehaviour where TEnum : Enum
    {
        private static readonly Dictionary<TOwner, Dictionary<TEnum, Action>> voidEvents = new();


        public static void Subscribe(TOwner owner, TEnum voidEvent, Action callback)
        {
            if (!voidEvents.ContainsKey(owner))
                voidEvents[owner] = new Dictionary<TEnum, Action>();

            if (!voidEvents[owner].ContainsKey(voidEvent))
                voidEvents[owner][voidEvent] = null;

            voidEvents[owner][voidEvent] += callback;
        }

        public static void Unsubscribe(TOwner owner, TEnum voidEvent, Action callback)
        {
            if (voidEvents.ContainsKey(owner) && voidEvents[owner].ContainsKey(voidEvent))
                voidEvents[owner][voidEvent] -= callback;
        }

        public static void Invoke(TOwner owner, TEnum voidEvent)
        {
            if (!voidEvents.ContainsKey(owner) || !voidEvents[owner].ContainsKey(voidEvent))
                return;

            voidEvents[owner][voidEvent]?.Invoke();
        }

        public static void Clear(TOwner owner)
        {
            if (voidEvents.ContainsKey(owner))
                voidEvents[owner].Clear();
        }

        public static void ClearAll()
        {
            voidEvents.Clear();
        }
    }

    public static class EventBusVoid<TEnum> where TEnum : Enum
    {
        private static readonly Dictionary<TEnum, Action> events = new();


        public static void Subscribe(TEnum voidEvent, Action callback)
        {
            if (!events.ContainsKey(voidEvent))
                events[voidEvent] = null;
            events[voidEvent] += callback;
        }

        public static void Unsubscribe(TEnum voidEvent, Action callback)
        {
            if (events.ContainsKey(voidEvent))
                events[voidEvent] -= callback;
        }

        public static void Invoke(TEnum voidEvent)
        {
            if (events.ContainsKey(voidEvent))
                events[voidEvent]?.Invoke();
        }

        public static void Clear(TEnum voidEvent)
        {
            if (events.ContainsKey(voidEvent))
                events[voidEvent] = null;
        }

        public static void ClearAll()
        {
            events.Clear();
        }
    }
}