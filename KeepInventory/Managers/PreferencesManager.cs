using System;
using System.Collections.Generic;
using System.IO;

using KeepInventory.Helper;
using KeepInventory.Menu;

using MelonLoader;
using MelonLoader.Utils;

namespace KeepInventory.Managers
{
    internal static class PreferencesManager
    {
        public readonly static string KI_PreferencesDirectory = Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory");

        internal static MelonPreferences_Category PrefsCategory;
        internal static MelonPreferences_Entry<bool> ItemSaving;
        internal static MelonPreferences_Entry<bool> AmmoSaving;
        internal static MelonPreferences_Entry<bool> SaveGunData;
        internal static MelonPreferences_Entry<string> DefaultSave;
        internal static MelonPreferences_Entry<bool> SaveOnLevelUnload;
        internal static MelonPreferences_Entry<bool> LoadOnLevelLoad;
        internal static MelonPreferences_Entry<List<string>> BlacklistedLevels;
        internal static MelonPreferences_Entry<List<string>> EnabledBlacklist;
        internal static MelonPreferences_Entry<bool> ShowNotifications;
        internal static MelonPreferences_Entry<int> ConfigVersion;
        internal static MelonPreferences_Entry<bool> HolsterHeldWeaponsOnDeath;

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

            SaveOnLevelUnload = PrefsCategory.CreateEntry<bool>("SaveOnLevelUnload", true, "Save On Level Unload",
                description: "If true, during level unload, the inventory will be automatically saved");
            LoadOnLevelLoad = PrefsCategory.CreateEntry<bool>("LoadOnLevelLoad", true, "Load On Level Load",
                description: "If true, the saved inventory will be automatically loaded when you get loaded into a level thats not blacklisted");

            BlacklistedLevels = PrefsCategory.CreateEntry<List<string>>("BlacklistedLevels", [], "Blacklisted Levels",
                description: "List of levels that will not save/load inventory");
            EnabledBlacklist = PrefsCategory.CreateEntry<List<string>>("EnabledBlacklist", ["default_labworks", "default_bonelab"], "Enabled Blacklist",
                description: "List of blacklist IDs that should be used");
            LoadBlacklist();

            ShowNotifications = PrefsCategory.CreateEntry<bool>("ShowNotifications", true, "Show Notifications",
                description: "If true, notifications will be shown in-game regarding errors or other things");
            ConfigVersion = PrefsCategory.CreateEntry<int>("ConfigVersion", 1, "Config Version",
                description: "DO NOT CHANGE THIS AT ALL, THIS WILL BE USED FOR MIGRATING CONFIGS AND SHOULD NOT BE CHANGED AT ALL");
            HolsterHeldWeaponsOnDeath = PrefsCategory.CreateEntry<bool>("HolsterHeldWeaponsOnDeath", true, "Holster Held Weapons On Death",
                description: "If true, when you die all of the weapons you were holding get holstered if possible");

            PrefsCategory.SaveToFile(false);
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
                Core.Logger.Error($"An unexpected error has occurred while saving preferences\n{e}");
            }
        }

        internal static void LoadBlacklist()
        {
            if (EnabledBlacklist == null)
                return;

            List<string> ids = [];
            BlacklistManager.Blacklist.ForEach(x => ids.Add(x.ID));
            EnabledBlacklist.Value.ForEach(x =>
            {
                if (BlacklistManager.HasItem(x))
                {
                    BlacklistManager.Enable(x);
                    ids.Remove(x);
                }
            });
            ids.ForEach(x =>
            {
                if (BlacklistManager.HasItem(x))
                    BlacklistManager.Disable(x);
            });
            BoneMenu.SetupPredefinedBlacklist();
        }

        internal static void SaveBlacklist()
        {
            if (EnabledBlacklist == null)
                return;

            List<string> ids = [];
            BlacklistManager.Blacklist.ForEach(x => { if (x.Enabled) ids.Add(x.ID); });
            EnabledBlacklist.Value = ids;
            PrefsCategory.SaveToFile(true);
        }
    }
}