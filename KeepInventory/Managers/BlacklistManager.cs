using System.Collections.Generic;
using System.Linq;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

namespace KeepInventory.Managers
{
    internal static class BlacklistManager
    {
        private static readonly List<Blacklist> _blacklist = [];

        public static IReadOnlyCollection<Blacklist> Blacklist => _blacklist.AsReadOnly();

        public static void Add(Blacklist blacklist)
        {
            if (HasItem(blacklist))
                return;

            _blacklist.Add(blacklist);
            PreferencesManager.SetupBlacklist();
        }

        public static void Remove(string id)
        {
            if (!HasItem(id))
                return;

            _blacklist.RemoveAll(x => x.ID == id);
        }

        public static void Remove(Blacklist blacklist)
            => Remove(blacklist.ID);

        public static bool HasItem(Blacklist blacklist)
            => HasItem(blacklist.ID);

        public static bool HasItem(string id)
            => _blacklist.Any(x => x.ID == id);

        public static bool IsLevelBlacklisted(Barcode barcode)
            => _blacklist.Any(x => x.Enabled && x.Levels.Any(x => x.ID == barcode.ID));

        public static bool IsLevelBlacklisted(this LevelCrate levelCrate)
            => IsLevelBlacklisted(levelCrate.Barcode);

        public static bool IsLevelBlacklisted(this LevelCrateReference levelCrate)
           => IsLevelBlacklisted(levelCrate.Barcode);

        public static bool IsCurrentLevelBlacklisted()
            => IsLevelBlacklisted(SceneStreamer.Session.Level.Barcode);

        private static void SetEnabled(string id, bool enabled)
            => _blacklist.FirstOrDefault(x => x.ID == id)?.SetEnabled(enabled);

        public static void Enable(string id)
            => SetEnabled(id, true);

        public static void Enable(Blacklist blacklist)
            => Enable(blacklist.ID);

        public static void Disable(string id)
            => SetEnabled(id, false);

        public static void Disable(Blacklist blacklist)
            => Disable(blacklist.ID);

        public static void Toggle(string id)
            => SetEnabled(id, !IsEnabled(id));

        public static void Toggle(Blacklist blacklist)
            => Toggle(blacklist.ID);

        public static bool IsEnabled(string id)
            => _blacklist.FirstOrDefault(x => x.ID == id)?.Enabled ?? false;

        public static bool IsEnabled(Blacklist blacklist)
            => IsEnabled(blacklist.ID);
    }

    public class Blacklist
    {
        public Blacklist(string id, string displayName, bool enabled = true, params List<Barcode> levels)
        {
            ID = id;
            DisplayName = displayName;
            Enabled = enabled;
            Levels = [.. levels];
        }

        public Blacklist(string id, string displayName, bool enabled = true, params List<string> levels)
        {
            ID = id;
            DisplayName = displayName;
            Enabled = enabled;
            List<Barcode> barcodes = [];
            levels.ForEach(x => barcodes.Add(new(x)));
            Levels = [.. barcodes];
        }

        public string ID { get; set; }

        public string DisplayName { get; set; }

        public bool Enabled { get; set; }

        public Barcode[] Levels { get; set; }

        public void SetEnabled(bool value)
            => Enabled = value;
    }
}