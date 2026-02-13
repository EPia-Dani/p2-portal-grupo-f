using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player
{
    public class PlayerService : MonoBehaviour
    {
        [field: SerializeField] public PlayerStats Stats { get; private set; }
        private Dictionary<Type, IModule> _modules;
        public GameObject Player { get; private set; }

        private void Awake()
        {
            var modules = gameObject.GetAllComponentsRecursive<MonoBehaviour>().OfType<IModule>().ToList();
            _modules = modules.ToDictionary(module => module.GetType(), module => module);
            Player = gameObject;
            InitializeModules();
        }

        public void InitializeModules()
        {
            foreach (var module in _modules.Values)
            {
                module.InitializeModule(this);
            }
        }

        public bool TryGetModule<T>(out T module) where T : IModule
        {
            if (_modules.TryGetValue(typeof(T), out var m))
            {
                module = (T)m;
                return true;
            }

            module = default;
            return false;
        }
    }
}