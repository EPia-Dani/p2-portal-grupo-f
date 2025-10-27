using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Routines
{
    public interface ITargetCallback
    {
        void Invoke();
    }

    public readonly struct TargetCallback<T> : ITargetCallback
    {
        private T Target { get; }
        private Action<T> Action { get; }

        public TargetCallback(T target, Action<T> action)
        {
            Target = target;
            Action = action;
        }

        public void Invoke()
        {
            Action(Target);
        }
    }

    public struct Routine
    {
        private static readonly Dictionary<Type, object> _lerpFunctions = new();

        public int Id { get; private set; }

        private Routine(Func<IEnumerator> getRoutine)
        {
            Id = RoutineManager.Instance.StoreRoutine(getRoutine);
        }

        public static Routine Create(IEnumerator routine)
        {
            return new Routine(() => routine);
        }

        public static Routine Create(Action action)
        {
            return new Routine(() => ExecuteAction(action));
        }

        public static Routine Create<T>(T target, Action<T> action)
        {
            return new Routine(() => ExecuteAction(() => action(target)));
        }

        public static Routine Wait(float duration)
        {
            return new Routine(() => WaitRoutine(duration));
        }

        public static Routine Chain(params Routine[] routines)
        {
            return new Routine(() => ChainRoutines(routines));
        }

        public static Routine WaitFrame()
        {
            return new Routine(() => WaitFrameRoutine());
        }

        public static Routine Buffered(float duration, Func<bool> condition, Action action)
        {
            if (condition == null) Debug.LogError("Condition is null");
            if (action == null) Debug.LogError("Action is null");
            return new Routine(() => BufferedRoutine(duration, condition, action));
        }

        public static Routine Buffered<T>(T target, float duration, Func<T, bool> condition, Action<T> action)
        {
            return new Routine(() => BufferedRoutine(duration, () => condition(target), () => action(target)));
        }

        public static Routine Interpolate<T>(T startValue, Func<T> getTarget, Action<T> setValue, float duration, AnimationCurve curve = null)
        {
            return new Routine(() => InterpolateRoutine(startValue, getTarget, setValue, duration, curve ?? AnimationCurve.Linear(0, 0, 1, 1)));
        }

        public static Routine Interpolate<T>(Func<T> getStart, Func<T> getTarget, Action<T> setValue, float duration, AnimationCurve curve = null)
        {
            return new Routine(() => InterpolateRoutine(getStart, getTarget, setValue, duration, curve ?? AnimationCurve.Linear(0, 0, 1, 1)));
        }

        public static Routine Interpolate<TTarget, TValue>(TTarget target, TValue startValue, Func<TTarget, TValue> getTarget, Action<TTarget, TValue> setValue, float duration, AnimationCurve curve = null)
        {
            return new Routine(() => InterpolateRoutine(target, startValue, getTarget, setValue, duration, curve ?? AnimationCurve.Linear(0, 0, 1, 1)));
        }

        public static Routine Interpolate<TTarget, TValue>(TTarget target, Func<TTarget, TValue> getStart, Func<TTarget, TValue> getTarget, Action<TTarget, TValue> setValue, float duration, AnimationCurve curve = null)
        {
            return new Routine(() => InterpolateRoutine(target, getStart, getTarget, setValue, duration, curve ?? AnimationCurve.Linear(0, 0, 1, 1)));
        }

        public Routine Persist()
        {
            RoutineManager.Instance.PersistRoutine(Id);
            return this;
        }

        public Routine OnComplete(Action onComplete)
        {
            RoutineManager.Instance.RegisterOnComplete(Id, onComplete);
            return this;
        }

        public Routine OnComplete<T>(T target, Action<T> onComplete)
        {
            if (onComplete == null) return this;
            if (target is UnityEngine.Object uo && uo == null) return this;
            RoutineManager.Instance.RegisterOnCompleteTarget(Id, new TargetCallback<T>(target, onComplete));
            return this;
        }

        public bool Destroy()
        {
            return RoutineManager.Instance.StopRoutine(Id, true);
        }

        public Routine Run()
        {
            if (Id == 0)
            {
                Debug.LogError("Routine has not been initialized");
                return this;
            }

            if (!RoutineManager.Instance.StartRoutine(Id))
            {
                Debug.LogError($"Routine {Id} could not be started");
            }
            return this;
        }

        public bool Stop()
        {
            return RoutineManager.Instance.StopRoutine(Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRunning() => RoutineManager.Instance.IsRoutineRunning(Id);

        private static IEnumerator ChainRoutines(Routine[] routines)
        {
            foreach (var routine in routines)
            {
                var running = routine.Run();
                while (running.IsRunning())
                {
                    yield return null;
                }
            }
        }

        private static IEnumerator BufferedRoutine(float duration, Func<bool> condition, Action action)
        {
            condition ??= () => false;
            action ??= () => { };

            var timeElapsed = 0f;
            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;
                if (condition())
                {
                    action();
                    yield break;
                }

                yield return null;
            }
        }

        private static IEnumerator ExecuteAction(Action action)
        {
            action?.Invoke();
            yield break;
        }

        private static IEnumerator WaitRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        private static IEnumerator WaitFrameRoutine()
        {
            yield return null;
        }

        private static IEnumerator InterpolateRoutine<T>(T startValue, Func<T> getTarget, Action<T> setValue, float duration, AnimationCurve curve)
        {
            var timeElapsed = 0f;
            var lerpFunc = GetLerpFunction<T>();

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;

                var normalizedTime = Mathf.Clamp01(timeElapsed / duration);
                var curveValue = Mathf.Clamp01(curve.Evaluate(normalizedTime));

                var targetValue = getTarget();

                var currentValue = lerpFunc(startValue, targetValue, curveValue);
                setValue(currentValue);

                yield return null;
            }

            setValue(getTarget());
        }

        private static IEnumerator InterpolateRoutine<T>(Func<T> getStart, Func<T> getTarget, Action<T> setValue, float duration, AnimationCurve curve)
        {
            T start = getStart();
            yield return InterpolateRoutine(start, getTarget, setValue, duration, curve);
        }

        private static IEnumerator InterpolateRoutine<TTarget, TValue>(TTarget target, TValue start, Func<TTarget, TValue> getTarget, Action<TTarget, TValue> setValue, float duration, AnimationCurve curve)
        {
            var timeElapsed = 0f;
            var lerpFunc = GetLerpFunction<TValue>();

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;

                var normalizedTime = Mathf.Clamp01(timeElapsed / duration);
                var curveValue = Mathf.Clamp01(curve.Evaluate(normalizedTime));

                var targetValue = getTarget(target);

                var currentValue = lerpFunc(start, targetValue, curveValue);
                setValue(target, currentValue);

                yield return null;
            }

            setValue(target, getTarget(target));
        }

        private static IEnumerator InterpolateRoutine<TTarget, TValue>(TTarget target, Func<TTarget, TValue> getStart, Func<TTarget, TValue> getTarget, Action<TTarget, TValue> setValue, float duration, AnimationCurve curve)
        {
            var start = getStart(target);
            yield return InterpolateRoutine(target, start, getTarget, setValue, duration, curve);
        }

        private static Func<T, T, float, T> GetLerpFunction<T>()
        {
            var type = typeof(T);
            if (_lerpFunctions.TryGetValue(type, out var cached))
            {
                return (Func<T, T, float, T>)cached;
            }

            Func<T, T, float, T> lerpFunction;

            if (type == typeof(Vector2))
            {
                lerpFunction = (from, to, t) => (T)(object)Vector2.Lerp((Vector2)(object)from, (Vector2)(object)to, t);
            }
            else if (type == typeof(Vector3))
            {
                lerpFunction = (from, to, t) => (T)(object)Vector3.Lerp((Vector3)(object)from, (Vector3)(object)to, t);
            }
            else if (type == typeof(Vector4))
            {
                lerpFunction = (from, to, t) => (T)(object)Vector4.Lerp((Vector4)(object)from, (Vector4)(object)to, t);
            }
            else if (type == typeof(Color))
            {
                lerpFunction = (from, to, t) => (T)(object)Color.Lerp((Color)(object)from, (Color)(object)to, t);
            }
            else if (type == typeof(float))
            {
                lerpFunction = (from, to, t) => (T)(object)Mathf.Lerp((float)(object)from, (float)(object)to, t);
            }
            else if (type == typeof(Quaternion))
            {
                lerpFunction = (from, to, t) => (T)(object)Quaternion.Lerp((Quaternion)(object)from, (Quaternion)(object)to, t);
            }
            else if (type == typeof(int))
            {
                lerpFunction = (from, to, t) => (T)(object)Mathf.RoundToInt(Mathf.Lerp((int)(object)from, (int)(object)to, t));
            }
            else
            {
                var lerpMethod = type.GetMethod("Lerp", new[] { type, type, typeof(float) });
                if (lerpMethod != null)
                {
                    lerpFunction = (from, to, t) => (T)lerpMethod.Invoke(null, new object[] { from, to, t });
                }
                else
                {
                    lerpFunction = (from, to, t) => t >= 1f ? to : from;
                }
            }
            _lerpFunctions[type] = lerpFunction;
            return lerpFunction;
        }
    }
}