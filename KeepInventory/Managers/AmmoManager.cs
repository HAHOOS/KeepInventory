using System.Collections.Generic;

using Il2CppSLZ.Marrow;

using MelonLoader;

namespace KeepInventory.Managers
{
    public static class AmmoManager
    {
        private static readonly Dictionary<string, int> Values = [];

        private static readonly List<string> _tracked = [];

        public static IReadOnlyCollection<string> Tracked => _tracked.AsReadOnly();

        public static void Track(string name)
        {
            if (_tracked.Contains(name))
                return;

            _tracked.Add(name);
        }

        public static void StopTracking(string name)
        {
            if (!_tracked.Contains(name))
                return;

            _tracked.Remove(name);
            Values.Remove(name);
        }

        public static void IsBeingTracked(string name)
            => _tracked.Contains(name);

        public static void Init()
        {
            MelonEvents.OnUpdate.Unsubscribe(OnUpdate);
            MelonEvents.OnUpdate.Subscribe(OnUpdate);
        }

        public static void Destroy()
        {
            MelonEvents.OnUpdate.Unsubscribe(OnUpdate);
        }

        private static void OnUpdate()
        {
            var inv = AmmoInventory.Instance;
            if (inv != null)
            {
                foreach (var tracked in _tracked)
                {
                    if (inv._groupCounts.ContainsKey(tracked)
                        && inv._groupCounts[tracked] != 10000000)
                    {
                        Values[tracked] = inv._groupCounts[tracked];
                    }
                }
            }
        }

        public static int GetValue(string name)
            => Values.ContainsKey(name) ? Values[name] : -1;
    }
}