using System;
using System.IO;
using System.Collections.Generic;

using KeepInventory.Menu;
using KeepInventory.Helper;

using MelonLoader;
using MelonLoader.Utils;

namespace KeepInventory.Managers
{
    internal static class PreferencesManager
    {
        public readonly static string KI_PreferencesDirectory = Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory");

        internal static MelonPreferences_Category PrefsCategory;
        internal static MelonPreferences_Category BlacklistCategory;

        internal static MelonPreferences_Entry<bool> ItemSaving;
        internal static MelonPreferences_Entry<bool> AmmoSaving;
        internal static MelonPreferences_Entry<bool> SaveGunData;
        internal static MelonPreferences_Entry<bool> SaveOnLevelUnload;
        internal static MelonPreferences_Entry<bool> LoadOnLevelLoad;
        internal static MelonPreferences_Entry<bool> ShowNotifications;
        internal static MelonPreferences_Entry<bool> HolsterHeldWeaponsOnDeath;

        internal static MelonPreferences_Entry<List<string>> BlacklistedLevels;
        internal static MelonPreferences_Entry<List<BlacklistItem>> PredefinedBlacklist;

        internal static MelonPreferences_Entry<string> DefaultSave;

        internal static void Setup()
        {
            if (!Directory.Exists(KI_PreferencesDirectory))
            {
                Core.Logger.Msg("Creating preferences directory");
                Directory.CreateDirectory(KI_PreferencesDirectory);
            }
            PrefsCategory = MelonPreferences.CreateCategory("KeepInventory_Settings");
            PrefsCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "Config.cfg"));

            ItemSaving = PrefsCategory.CreateEntry<bool>("ItemSaving", true, "Item Saving",
                description: "If true, will save and load items in inventory");
            AmmoSaving = PrefsCategory.CreateEntry<bool>("AmmoSaving", true, "Ammo Saving",
                description: "If true, will save and load ammo in inventory");
            SaveGunData = PrefsCategory.CreateEntry<bool>("SaveGunData", true, "Save Gun Data",
                description: "If true, will save and load data about guns stored in slots, info such as rounds left etc.");
            DefaultSave = PrefsCategory.CreateEntry<string>("DefaultSave", string.Empty, "Default Save",
                description: "ID of the save that will be used for things such as loading inventory on load or saving the inventory on level unload");
            DefaultSave.OnEntryValueChanged.Subscribe((_, _) =>
            {
                if (!Core.Deinit)
                    BoneMenu.SetupSaves();
            });
            SaveOnLevelUnload = PrefsCategory.CreateEntry<bool>("SaveOnLevelUnload", true, "Save On Level Unload",
                description: "If true, during level unload, the inventory will be automatically saved");
            LoadOnLevelLoad = PrefsCategory.CreateEntry<bool>("LoadOnLevelLoad", true, "Load On Level Load",
                description: "If true, the saved inventory will be automatically loaded when you get loaded into a level thats not blacklisted");

            ShowNotifications = PrefsCategory.CreateEntry<bool>("ShowNotifications", true, "Show Notifications",
                description: "If true, notifications will be shown in-game regarding errors or other things");
            HolsterHeldWeaponsOnDeath = PrefsCategory.CreateEntry<bool>("HolsterHeldWeaponsOnDeath", true, "Holster Held Weapons On Death",
                description: "If true, when you die all of the weapons you were holding get holstered if possible");

            PrefsCategory.SaveToFile(false);

            BlacklistCategory = MelonPreferences.CreateCategory("KeepInventory_Blacklist", "Blacklist");
            PredefinedBlacklist = BlacklistCategory.CreateEntry<List<BlacklistItem>>("PredefinedBlacklist", [new("default_labworks"), new("default_bonelab")], "Predefined Blacklist",
                description: "List of predefined blacklists whether they are enabled or not etc.");
            BlacklistedLevels = BlacklistCategory.CreateEntry<List<string>>("BlacklistedLevels", [], "Blacklisted Levels",
                description: "List of levels that will not save/load inventory");

            BlacklistCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "Blacklist.cfg"));

            LoadBlacklist();
        }

        public static void Save()
        {
            Core.Logger.Msg("Saving Preferences");
            try
            {
                PrefsCategory?.SaveToFile(false);
                Core.Logger.Msg("Saved Preferences successfully");
            }
            catch (Exception e)
            {
                Core.Logger.Error($"An unexpected error has occurred while saving preferences", e);
            }
        }

        internal static void LoadBlacklist()
        {
            if (BlacklistCategory == null)
                return;

            List<string> ids = [];
            BlacklistManager.Blacklist.ForEach(x => ids.Add(x.ID));
            PredefinedBlacklist.Value.ForEach(x =>
            {
                if (BlacklistManager.HasItem(x.ID))
                {
                    BlacklistManager.SetEnabled(x.ID, x.Enabled);
                    x.AllowedLevels.ForEach(y => BlacklistManager.AllowLevel(x.ID, y));
                    ids.Remove(x.ID);
                }
            });
            ids.ForEach(x =>
            {
                if (BlacklistManager.HasItem(x))
                    BlacklistManager.Disable(x);
            });
            BoneMenu.SetupPredefinedBlacklists();
        }

        internal static void SaveBlacklist()
        {
            if (BlacklistCategory == null)
                return;

            List<BlacklistItem> ids = [];
            BlacklistManager.Blacklist.ForEach(x =>
            {
                var item = new BlacklistItem(x.ID, x.Enabled);
                x.Levels.ForEach(x =>
                {
                    if (!x.IsBlacklisted && !item.AllowedLevels.Contains(x.Barcode))
                    {
                        item.AllowedLevels.Add(x.Barcode);
                    }
                });
                ids.Add(item);
            });
            PredefinedBlacklist.Value = ids;
            BlacklistCategory.SaveToFile(false);
            BoneMenu.SetupPredefinedBlacklists();
        }
    }

    internal class BlacklistItem
    {
        public string ID { get; set; }

        public bool Enabled { get; set; } = true;

        public List<string> AllowedLevels { get; set; } = [];

        public BlacklistItem()
        {
        }

        public BlacklistItem(string id)
        {
            ID = id;
        }

        public BlacklistItem(string id, bool enabled)
        {
            ID = id;
            Enabled = enabled;
        }

        public BlacklistItem(string id, bool enabled, List<string> allowedLevels)
        {
            ID = id;
            Enabled = enabled;
            AllowedLevels = allowedLevels;
        }

        public BlacklistItem(string id, List<string> allowedLevels)
        {
            ID = id;
            AllowedLevels = allowedLevels;
        }
    }
}