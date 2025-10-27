using System;
using System.Collections;
using System.Collections.Generic;
using Routines;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class RoutineManager : MonoBehaviour
{
    private struct RoutineInstance : IEquatable<RoutineInstance>
    {
        public int Id;
        public Coroutine Coroutine;
        public IEnumerator Routine;
        public Func<IEnumerator> GetRoutine;
        public Action OnComplete;
        public ITargetCallback OnCompleteTarget;
        public bool Persistent;
        public override bool Equals(object obj)
        {
            if (obj is RoutineInstance instance)
            {
                return Id == instance.Id;
            }

            return base.Equals(obj);
        }

        public bool Equals(RoutineInstance other)
        {
            return Id == other.Id && Equals(Coroutine, other.Coroutine) && Equals(Routine, other.Routine) &&
                   Equals(OnComplete, other.OnComplete) && Equals(OnCompleteTarget, other.OnCompleteTarget);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Coroutine, Routine, OnComplete, OnCompleteTarget);
        }

        public static bool operator ==(RoutineInstance left, RoutineInstance right) => left.Equals(right);
        public static bool operator !=(RoutineInstance left, RoutineInstance right) => !left.Equals(right);
    }

    private static RoutineManager _instance;
    private static bool _isQuitting;
    private const int _maxAttempts = 10000;

    public static RoutineManager Instance
    {
        get
        {
            if (!_instance)
            {
                Initialize();
            }

            return _instance;
        }
    }

    private readonly Dictionary<int, RoutineInstance> _routines = new();
    private static int _nextRoutineId = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_instance) return;
        if (_isQuitting) return; // don't create during app quit

        var routineManagerObject = new GameObject("[RoutineManager]");
        _instance = routineManagerObject.AddComponent<RoutineManager>();
        DontDestroyOnLoad(routineManagerObject);
    }

    private RoutineInstance GetRoutineInstance(int routineId)
    {
        if (_routines.TryGetValue(routineId, out var instance))
        {
            return instance;
        }

        return default;
    }

    public bool StartRoutine(int routineId)
    {
        if (_isQuitting) return false;

        if (!_routines.TryGetValue(routineId, out var instance)) return false;

        // If already running, stop and restart the stored enumerator
        if (instance.Coroutine != null)
        {
            StopCoroutine(instance.Coroutine);
            instance.Coroutine = null;
        }

        // Recreate enumerator from getRoutine if available; fallback to stored enumerator
        var enumerator = instance.GetRoutine != null ? instance.GetRoutine() : instance.Routine;
        instance.Routine = enumerator;
        var coroutine = StartCoroutine(WrapRoutine(routineId, enumerator, instance.OnComplete));
        instance.Coroutine = coroutine;
        _routines[routineId] = instance;

        return true;
    }

    public int StoreRoutine(Func<IEnumerator> getRoutine)
    {
        if (_isQuitting) return -1;

        var routineId = _nextRoutineId;
        var attempts = 0;
        while (_routines.ContainsKey(routineId) && attempts < _maxAttempts)
        {
            routineId++;
            if (routineId >= 10000) routineId = 1;
            attempts++;
        }

        if (attempts >= _maxAttempts)
        {
            Debug.LogError("Failed to generate unique routine ID - too many active routines!");
            return -1;
        }

        _nextRoutineId = routineId + 1;
        if (_nextRoutineId >= 10000) _nextRoutineId = 1;

        _routines[routineId] = new RoutineInstance
        {
            Id = routineId,
            Coroutine = null,
            Routine = null,
            GetRoutine = getRoutine,
            OnComplete = null,
            OnCompleteTarget = null,
            Persistent = false
        };

        return routineId;
    }

    public void PersistRoutine(int routineId)
    {
        if (_isQuitting) return;
        if (_routines.TryGetValue(routineId, out var instance))
        {
            instance.Persistent = true;
            _routines[routineId] = instance;
        }
    }

    public void RegisterOnComplete(int routineId, Action callback)
    {
        if (routineId == -1) return;
        if (_routines.TryGetValue(routineId, out var inst))
        {
            inst.OnComplete = callback;
            _routines[routineId] = inst;
        }
    }

    public void RegisterOnCompleteTarget(int routineId, ITargetCallback callback)
    {
        if (routineId == -1) return;
        if (_routines.TryGetValue(routineId, out var inst))
        {
            inst.OnCompleteTarget = callback;
            _routines[routineId] = inst;
        }
    }

    public bool StopRoutine(int routineId, bool destroy = false)
    {
        if (routineId == -1) return false;

        if (!_routines.TryGetValue(routineId, out var instance))
            return false;

        if (destroy)
        {
            instance.OnComplete = null;
        }

        if (instance.Coroutine != null)
        {
            StopCoroutine(instance.Coroutine);
            instance.Coroutine = null;
        }

        if (!instance.Persistent)
        {
            _routines.Remove(routineId);
        }
        else
        {
            _routines[routineId] = instance;
        }
        return true;
    }

    public bool IsRoutineRunning(int routineId)
    {
        if (_isQuitting || routineId == -1) return false;
        if (!_routines.TryGetValue(routineId, out var instance)) return false;
        return instance.Coroutine != null;
    }

    private void StopAllRoutines()
    {
        foreach (var instance in _routines.Values)
        {
            if (instance.Coroutine != null)
            {
                StopCoroutine(instance.Coroutine);
            }
        }
        _routines.Clear();
    }

    private IEnumerator WrapRoutine(int routineId, IEnumerator routine, Action onComplete)
    {
        yield return routine;

        if (_routines.TryGetValue(routineId, out var instance))
        {
            // capture callbacks then remove if not persistent
            var targetCb = instance.OnCompleteTarget;
            var cb = instance.OnComplete;

            if (!instance.Persistent)
            {
                _routines.Remove(routineId);
            }
            else
            {
                // Reset wrapper coroutine; enumerator will be recreated from GetRoutine on next Start
                instance.Coroutine = null;
                _routines[routineId] = instance;
            }

            if (targetCb != null)
            {
                targetCb.Invoke();
            }
            else if (cb != null)
            {
                cb.Invoke();
            }
            else
            {
                onComplete?.Invoke();
            }
        }
        else
        {
            // routine was stopped/removed
            onComplete?.Invoke();
        }
    }

    private void OnDestroy()
    {
        _isQuitting = true;
        StopAllRoutines();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
        StopAllRoutines();
    }
}