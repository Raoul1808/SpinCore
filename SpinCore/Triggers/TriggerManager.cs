using System;
using System.Collections.Generic;
using System.Reflection;

namespace SpinCore.Triggers
{
    public static class TriggerManager
    {
        private static readonly Dictionary<string, ModTriggerStore> TriggerStores = new Dictionary<string, ModTriggerStore>();

        public delegate void ChartLoad(TrackData trackData);
        
        /// <summary>
        /// This event is fired whenever the game loads a custom chart.
        /// </summary>
        public static event ChartLoad OnChartLoad;

        /// <summary>
        /// Loads the given triggers into the internal trigger manager.
        /// </summary>
        /// <param name="triggers">The list of triggers</param>
        /// <param name="key">The key to use internally</param>
        /// <exception cref="ArgumentException">Raised if the given array contains nothing</exception>
        public static void LoadTriggers(ITrigger[] triggers, string key = "")
        {
            if (triggers.Length == 0)
                throw new ArgumentException("ITrigger array needs to contain triggers");
            if (string.IsNullOrWhiteSpace(key))
            {
                var trigger = triggers[0];
                key = trigger.GetType().Name;
            }

            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{key}";
            if (!TriggerStores.TryGetValue(fullKey, out var store))
            {
                store = new ModTriggerStore();
                TriggerStores.Add(fullKey, store);
            }

            store.Clear();
            store.AddTriggers(triggers);
        }
        
        public delegate void TriggerUpdate(ITrigger trigger, float trackTime);
        
        /// <summary>
        /// Fires the given method when a trigger is fired/updates
        /// </summary>
        /// <param name="action">A callback method</param>
        /// <typeparam name="T">The affected trigger (required)</typeparam>
        public static void RegisterTriggerEvent<T>(TriggerUpdate action) where T : ITrigger
        {
            RegisterTriggerEvent(typeof(T).Name, action);
        }

        /// <summary>
        /// Fires the given method when a trigger is fired/updates
        /// </summary>
        /// <param name="action">A callback method</param>
        /// <param name="key">The key to use internally</param>
        public static void RegisterTriggerEvent(string key, TriggerUpdate action)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Invalid key");
            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{key}";
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
        /// <typeparam name="T">The affected trigger</typeparam>
        public static void ClearTriggers<T>() where T : ITrigger
        {
            ClearTriggers(typeof(T).Name);
        }

        /// <summary>
        /// Clears triggers for the corresponding trigger store
        /// </summary>
        /// <param name="key">The trigger store key</param>
        public static void ClearTriggers(string key)
        {
            string fullKey = $"{Assembly.GetCallingAssembly().GetName().Name}-{key}";
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

        internal static void InvokeChartLoadEvent(TrackData trackData)
        {
            OnChartLoad?.Invoke(trackData);
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
