using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Core.EventBus
{
    public static class EventBusUtil
    {
        private static IReadOnlyList<Type> EventTypes { get; set; }
        private static IReadOnlyList<Type> EnumTypes { get; set; }
        private static IReadOnlyList<Type> MonoBehaviorTypes { get; set; }
        private static IReadOnlyList<Type> EventBusTypes { get; set; }

#if UNITY_EDITOR

        [InitializeOnLoadMethod]
        private static void InitEvents()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("Clearing all event buses");
                Initialize();
                ClearAllBuses();
            }
        }
#endif
        public static void Initialize()
        {
            EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
            EnumTypes = PredefinedAssemblyUtil.GetTypes(typeof(Enum));
            MonoBehaviorTypes = PredefinedAssemblyUtil.GetTypes(typeof(MonoBehaviour));
            EventBusTypes = InitializeAllBuses();
        }

        private static List<Type> InitializeAllBuses()
        {
            var eventBusTypes = new List<Type>();

            var typedef = typeof(EventBus<>);
            foreach (var eventType in EventTypes)
            {
                var busType = typedef.MakeGenericType(eventType);
                eventBusTypes.Add(busType);
            }

            var typedef2 = typeof(EventBus<,>);
            foreach (var monoBehaviorType in MonoBehaviorTypes)
            {
                foreach (var eventType in EventTypes)
                {
                    var busType = typedef2.MakeGenericType(monoBehaviorType, eventType);
                    eventBusTypes.Add(busType);
                }
            }

            var voidTypeDef = typeof(EventBusVoid<>);
            foreach (var eventEnumType in EnumTypes)
            {
                var busType = voidTypeDef.MakeGenericType(eventEnumType);
                eventBusTypes.Add(busType);
            }

            var voidTypeDef2 = typeof(EventBusVoid<,>);
            foreach (var monoBehaviorType in MonoBehaviorTypes)
            {
                foreach (var eventEnumType in EnumTypes)
                {
                    var busType = voidTypeDef2.MakeGenericType(monoBehaviorType, eventEnumType);
                    eventBusTypes.Add(busType);
                }
            }

            return eventBusTypes;
        }

        public static void ClearAllBuses()
        {
            if (EventBusTypes == null) return;

            foreach (var busType in EventBusTypes)
            {
                try
                {
                    var clearMethod = busType.GetMethod("ClearAll", BindingFlags.Static | BindingFlags.Public);
                    clearMethod?.Invoke(null, null);
                }
                catch { }
            }
        }
    }
}