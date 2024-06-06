using System;
using System.Collections.Generic;
using System.Reflection;

namespace SpinCore.Triggers
{
    public static class TriggerManager
    {
        private static readonly Dictionary<string, ModTriggerStore> TriggerStores = new Dictionary<string, ModTriggerStore>();

        /// <summary>
        /// Loads the given triggers into the internal trigger manager.
        /// </summary>
        /// <param name="triggers">The list of triggers</param>
        /// <typeparam name="T">The affected trigger, used as a lookup key</typeparam>
        /// <exception cref="ArgumentException">Raised if the given array contains nothing</exception>
        public static void LoadTriggers<T>(T[] triggers) where T : ITrigger
        {
            if (triggers.Length == 0)
                throw new ArgumentException("ITrigger array needs to contain triggers");
            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{typeof(T).Name}";
            InternalLoadTriggers(fullKey, Array.ConvertAll(triggers, t => (ITrigger)t));
        }

        /// <summary>
        /// Loads the given triggers into the internal trigger manager.
        /// </summary>
        /// <param name="key">The internal lookup key</param>
        /// <param name="triggers">The list of triggers</param>
        /// <exception cref="ArgumentException">Raised if the given array contains nothing</exception>
        public static void LoadTriggers(string key, ITrigger[] triggers)
        {
            if (triggers.Length == 0)
                throw new ArgumentException("ITrigger array needs to contain triggers");
            if (string.IsNullOrWhiteSpace(key))
            {
                var trigger = triggers[0];
                key = trigger.GetType().Name;
            }

            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{key}";
            InternalLoadTriggers(fullKey, triggers);
        }

        private static void InternalLoadTriggers(string fullKey, ITrigger[] triggers)
        {
            if (!TriggerStores.TryGetValue(fullKey, out var store))
            {
                store = new ModTriggerStore();
                TriggerStores.Add(fullKey, store);
            }

            store.Clear();
            store.AddTriggers(triggers);
        }
        
        public delegate void TriggerUpdate(ITrigger trigger, float trackTime);
        public delegate void TriggerUpdate<in T>(T trigger, float trackTime) where T : ITrigger;
        
        /// <summary>
        /// Fires the given method when a trigger is fired/updates
        /// </summary>
        /// <param name="action">A callback method</param>
        /// <typeparam name="T">The affected trigger, used both as a lookup key and to automatically cast fired triggers into the target trigger type</typeparam>
        public static void RegisterTriggerEvent<T>(TriggerUpdate<T> action) where T : ITrigger
        {
            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{typeof(T).Name}";
            InternalRegisterTriggerEvent(fullKey, (trigger, trackTime) =>
            {
                var castTrigger = (T)trigger;
                action.Invoke(castTrigger, trackTime);
            });
        }

        /// <summary>
        /// Fires the given method when a trigger is fired/updates
        /// </summary>
        /// <param name="key">The internal lookup key</param>
        /// <param name="action">A callback method</param>
        /// <typeparam name="T">The affected trigger, used to automatically cast fired triggers into the target trigger type</typeparam>
        public static void RegisterTriggerEvent<T>(string key, TriggerUpdate<T> action) where T : ITrigger
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Invalid key");
            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{key}";
            InternalRegisterTriggerEvent(fullKey, (trigger, trackTime) =>
            {
                var castTrigger = (T)trigger;
                action.Invoke(castTrigger, trackTime);
            });
        }

        /// <summary>
        /// Fires the given method when a trigger is fired/updates
        /// </summary>
        /// <param name="key">The internal lookup key</param>
        /// <param name="action">A callback method</param>
        public static void RegisterTriggerEvent(string key, TriggerUpdate action)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Invalid key");
            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{key}";
            InternalRegisterTriggerEvent(fullKey, action);
        }

        private static void InternalRegisterTriggerEvent(string fullKey, TriggerUpdate action)
        {
            if (!TriggerStores.TryGetValue(fullKey, out var store))
            {
                store = new ModTriggerStore();
                TriggerStores.Add(fullKey, store);
            }

            store.OnTriggerUpdate += action;
        }

        /// <summary>
        /// Clears triggers for the corresponding trigger store
        /// </summary>
        /// <typeparam name="T">The affected trigger, used as a lookup key</typeparam>
        public static void ClearTriggers<T>() where T : ITrigger
        {
            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{typeof(T).Name}";
            InternalClearTriggers(fullKey);
        }

        /// <summary>
        /// Clears triggers for the corresponding trigger store
        /// </summary>
        /// <param name="key">The trigger store key</param>
        public static void ClearTriggers(string key)
        {
            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{key}";
            InternalClearTriggers(fullKey);
        }

        private static void InternalClearTriggers(string fullKey)
        {
            if (!TriggerStores.TryGetValue(fullKey, out var store))
            {
                store = new ModTriggerStore();
                TriggerStores.Add(fullKey, store);
            }
            
            store.Clear();
        }

        internal static void ClearAllTriggers()
        {
            foreach (var store in TriggerStores.Values)
            {
                store.Clear();
            }
        }

        internal static void Update(float trackTime)
        {
            foreach (var store in TriggerStores.Values)
            {
                store.Update(trackTime);
            }
        }

        internal static void ResetTriggerStores()
        {
            foreach (var store in TriggerStores.Values)
            {
                store.Reset();
            }
        }
    }
}
