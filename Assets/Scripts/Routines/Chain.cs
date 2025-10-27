using System;
using System.Collections;
using UnityEngine;

namespace Routines
{
    public struct Chain
    {
        private Routine[] Routines { get; }
        public int Id { get; private set; }
        private bool stopped;
        private Chain(Routine[] routines)
        {
            stopped = false;
            foreach (var routine in routines)
            {
                if (routine.Id == 0)
                {
                    Debug.LogError("Routine has not been initialized");
                }
            }
            Routines = routines;
            Id = RoutineManager.Instance.StoreRoutine(() => ChainRoutines(routines));
        }
        public static Chain Create(params Routine[] routines)
        {
            return new Chain(routines);
        }

        public Chain Run()
        {
            stopped = false;
            RoutineManager.Instance.StartRoutine(Id);
            return this;
        }

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

        public bool Stop()
        {
            stopped = true;
            RoutineManager.Instance.StopRoutine(Id);
            foreach (var routine in Routines)
            {
                // Force stop via manager to avoid struct-copy issues
                RoutineManager.Instance.StopRoutine(routine.Id);
            }
            return true;
        }

        public bool Destroy()
        {
            stopped = true;
            RoutineManager.Instance.StopRoutine(Id, true);
            foreach (var routine in Routines)
            {
                RoutineManager.Instance.StopRoutine(routine.Id, true);
            }
            return true;
        }

        public bool IsRunning()
        {
            if (stopped) return false;
            return RoutineManager.Instance.IsRoutineRunning(Id);
        }
    }
}