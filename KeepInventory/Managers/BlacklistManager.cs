using System;
using System.Linq;
using System.Collections.Generic;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.SceneStreaming;

namespace KeepInventory.Managers
{
    internal static class BlacklistManager
    {
        private static readonly List<Blacklist> _blacklist = [];

        public static IReadOnlyCollection<Blacklist> Blacklist => _blacklist.AsReadOnly();

        public static void Add(Blacklist blacklist)
        {
            if (blacklist == null)
                throw new ArgumentException("Blacklist param is null", nameof(blacklist));

            if (string.IsNullOrWhiteSpace(blacklist.ID) || string.IsNullOrWhiteSpace(blacklist.DisplayName))
                throw new ArgumentException("Blacklist ID and/or Display Name cannot be null / empty!", nameof(blacklist));

            if (HasItem(blacklist))
                throw new ArgumentException("Blacklist with such ID already exists", nameof(blacklist));

            _blacklist.Add(blacklist);
            PreferencesManager.LoadBlacklist();
        }

        public static void Remove(string id)
        {
            if (!HasItem(id))
                throw new ArgumentException("Blacklist with such ID doesn't exist");

            _blacklist.RemoveAll(x => x.ID == id);
        }

        public static void Remove(Blacklist blacklist)
            => Remove(blacklist.ID);

        public static bool HasItem(Blacklist blacklist)
            => HasItem(blacklist.ID);

        public static bool HasItem(string id)
            => _blacklist.Any(x => x.ID == id);

        public static bool IsLevelBlacklisted(Barcode barcode)
            => _blacklist.Any(x => x.Enabled && x.Levels.Any(x => x.IsBlacklisted && x.Barcode == barcode.ID));

        public static bool IsLevelInBlacklist(Barcode barcode)
            => _blacklist.Any(x => x.Levels.Any(x => x.IsBlacklisted && x.Barcode == barcode.ID));

        public static bool IsLevelBlacklisted(this LevelCrate levelCrate)
            => IsLevelBlacklisted(levelCrate.Barcode);

        public static bool IsLevelBlacklisted(this LevelCrateReference levelCrate)
           => IsLevelBlacklisted(levelCrate.Barcode);

        public static bool HasLevel(string id, string barcode)
        {
            if (!HasItem(id))
                throw new ArgumentException("Blacklist with provided ID does not exist", nameof(id));

            return _blacklist.FirstOrDefault(x => x.ID == id).Levels.Any(x => x.Barcode == barcode);
        }

        public static void SetLevelBlacklisted(string id, string barcode, bool blacklisted)
        {
            if (!HasItem(id))
                throw new ArgumentException("Blacklist with provided ID does not exist", nameof(id));

            if (!HasLevel(id, barcode))
                throw new ArgumentException("Level with the provided barcode does not exist in the blacklist", nameof(barcode));

            _blacklist.FirstOrDefault(x => x.ID == id).Levels.FirstOrDefault(x => x.Barcode == barcode).IsBlacklisted = blacklisted;
            PreferencesManager.SaveBlacklist();
        }

        public static void AllowLevel(string id, string barcode)
            => SetLevelBlacklisted(id, barcode, false);

        public static void AllowLevel(string id, Barcode barcode)
            => AllowLevel(id, barcode?.ID);

        public static void BlacklistLevel(string id, string barcode)
            => SetLevelBlacklisted(id, barcode, true);

        public static void BlacklistLevel(string id, Barcode barcode)
            => BlacklistLevel(id, barcode?.ID);

        public static bool IsCurrentLevelInBlacklist()
            => IsLevelInBlacklist(SceneStreamer.Session.Level.Barcode);

        public static void SetEnabled(string id, bool enabled)
        {
            _blacklist.FirstOrDefault(x => x.ID == id)?.SetEnabled(enabled);
            PreferencesManager.SaveBlacklist();
        }

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
        public Blacklist(string id, string displayName, bool enabled = true, params List<Level> levels)
        {
            ID = id;
            DisplayName = displayName;
            Enabled = enabled;
            Levels = [.. levels];
        }

        public string ID { get; set; }

        public string DisplayName { get; set; }

        public bool Enabled { get; set; }

        public Level[] Levels { get; set; }

        public void SetEnabled(bool value)
            => Enabled = value;
    }

    public class Level
    {
        public string Barcode { get; set; }

        public bool IsBlacklisted { get; set; } = true;

        public Level(string barcode, bool isBlacklisted)
        {
            Barcode = barcode;
            IsBlacklisted = isBlacklisted;
        }

        public Level(Barcode barcode, bool isBlacklisted)
        {
            Barcode = barcode?.ID;
            IsBlacklisted = isBlacklisted;
        }

        public Level(string barcode)
        {
            Barcode = barcode;
        }

        public Level(Barcode barcode)
        {
            Barcode = barcode?.ID;
        }
    }
}