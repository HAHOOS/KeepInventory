using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.Notifications;

using KeepInventory.Helper;
using KeepInventory.Managers;
using KeepInventory.Menu;
using KeepInventory.Utilities;

using MelonLoader;
using MelonLoader.Pastel;
using MelonLoader.Utils;

using Semver;

using UnityEngine;

namespace KeepInventory
{
    public class Core : MelonMod
    {
        public const string Version = "1.3.0";

        public static Saves.V2.Save CurrentSave
        {
            get
            {
                return SaveManager.Saves.FirstOrDefault(x => x.ID == mp_defaultSave.Value);
            }
            set
            {
                if (value == null)
                    mp_defaultSave.Value = string.Empty;

                mp_defaultSave.Value = value.ID;
                PrefsCategory.SaveToFile(false);
                DefaultSaveChanged?.Invoke();
            }
        }

        public static event Action DefaultSaveChanged;

        public static Core Instance { get; internal set; }
        internal static Assembly MLAssembly;

        public readonly static List<string> bonelabBlacklist = [
                CommonBarcodes.Maps.Home,
                CommonBarcodes.Maps.Ascent,
                CommonBarcodes.Maps.Descent,
                CommonBarcodes.Maps.MineDive,
                CommonBarcodes.Maps.LongRun,
                CommonBarcodes.Maps.BigAnomaly,
                CommonBarcodes.Maps.BigAnomaly2,
                CommonBarcodes.Maps.StreetPuncher,
                CommonBarcodes.Maps.SprintBridge,
                CommonBarcodes.Maps.MagmaGate,
                CommonBarcodes.Maps.Moonbase,
                CommonBarcodes.Maps.MonogonMotorway,
                CommonBarcodes.Maps.PillarClimb,
                CommonBarcodes.Maps.ContainerYard,
                CommonBarcodes.Maps.FantasyArena,
                CommonBarcodes.Maps.TunnelTipper,
                CommonBarcodes.Maps.DropPit,
                CommonBarcodes.Maps.NeonTrial,
                CommonBarcodes.Maps.MainMenu,
        ];

        public readonly static List<string> labworksBlacklist = [
           "volx4.LabWorksBoneworksPort.Level.BoneworksLoadingScreen",
           "volx4.LabWorksBoneworksPort.Level.BoneworksMainMenu",
           "volx4.LabWorksBoneworksPort.Level.Boneworks01Breakroom",
           "volx4.LabWorksBoneworksPort.Level.Boneworks02Museum",
           "volx4.LabWorksBoneworksPort.Level.Boneworks03Streets",
           "volx4.LabWorksBoneworksPort.Level.Boneworks04Runoff",
           "volx4.LabWorksBoneworksPort.Level.Boneworks05Sewers",
           "volx4.LabWorksBoneworksPort.Level.Boneworks06Warehouse",
           "volx4.LabWorksBoneworksPort.Level.Boneworks07CentralStation",
           "volx4.LabWorksBoneworksPort.Level.Boneworks08Tower",
           "volx4.LabWorksBoneworksPort.Level.Boneworks09TimeTower",
           "volx4.LabWorksBoneworksPort.Level.Boneworks10Dungeon",
           "volx4.LabWorksBoneworksPort.Level.Boneworks11Arena",
           "volx4.LabWorksBoneworksPort.Level.Boneworks12ThroneRoom",
           "volx4.LabWorksBoneworksPort.Level.BoneworksCutscene01",
           "volx4.LabWorksBoneworksPort.Level.sceneTheatrigonMovie02"
        ];

        internal static MelonLogger.Instance Logger { get; private set; }

        internal static int _lastAmmoCount_light = 0;
        internal static int _lastAmmoCount_medium = 0;
        internal static int _lastAmmoCount_heavy = 0;
        public readonly static string KI_PreferencesDirectory = Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory");
        internal static MelonPreferences_Category PrefsCategory;
        internal static MelonPreferences_Entry<bool> mp_itemsaving;
        internal static MelonPreferences_Entry<bool> mp_ammosaving;
        internal static MelonPreferences_Entry<bool> mp_saveGunData;
        internal static MelonPreferences_Entry<string> mp_defaultSave;
        internal static MelonPreferences_Entry<bool> mp_saveOnLevelUnload;
        internal static MelonPreferences_Entry<bool> mp_loadOnLevelLoad;
        internal static MelonPreferences_Entry<List<string>> mp_blacklistedLevels;
        internal static MelonPreferences_Entry<bool> mp_blacklistBONELABlevels;
        internal static MelonPreferences_Entry<bool> mp_blacklistLABWORKSlevels;
        internal static MelonPreferences_Entry<bool> mp_showNotifications;
        internal static MelonPreferences_Entry<int> mp_configVersion;
        internal static MelonPreferences_Entry<bool> mp_holsterHeldWeaponsOnDeath;
        internal static FunctionElement statusElement;
        internal static LevelInfo levelInfo;
        public static bool HasFusion => FindMelon("LabFusion", "Lakatrazz") != null;
        public static bool IsFusionLibraryInitialized { get; internal set; } = false;
        public static bool FailedFLLoad { get; internal set; } = false;
        private bool InitialLoad = true;
        internal static Thunderstore ThunderstoreInstance { get; private set; }
        internal static bool IsLatestVersion { get; private set; } = true;
        internal static Package ThunderstorePackage { get; private set; }
        public Thread UnityThread { get; private set; }

        public bool IsCurrentThreadMainThread
            => Thread.CurrentThread == UnityThread;

        public override void OnInitializeMelon()
        {
            UnityThread = Thread.CurrentThread;
            MLAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "MelonLoader");
            Logger = LoggerInstance;
            Instance = this;

            LoggerInstance.Msg("Setting up KeepInventory");

            CheckVersion();

            if (!HasFusion)
            {
                LoggerInstance.Warning("Could not find LabFusion, the mod will not use any of Fusion's functionality");
            }

            if (HasFusion)
            {
                LoggerInstance.Msg("Attempting to load the Fusion Support Library");
                if (DependencyManager.TryLoadDependency("KeepInventory.Fusion"))
                    IsFusionLibraryInitialized = true;
                else
                    FailedFLLoad = true;
            }

            Hooking.OnLevelLoaded += LevelLoadedEvent;
            Hooking.OnLevelUnloaded += LevelUnloadedEvent;

            if (IsFusionLibraryInitialized) Utilities.Fusion.SetupFusionLibrary();
            if (HasFusion) Utilities.Fusion.Setup();

            SetupPreferences();
            SaveManager.Setup();
            BoneMenu.Setup();
            AmmoManager.Track("light");
            AmmoManager.Track("medium");
            AmmoManager.Track("heavy");
            AmmoManager.Init();
        }

        private void CheckVersion()
        {
            ThunderstoreInstance = new Thunderstore($"KeepInventory / {Version} A BONELAB MelonLoader Mod");

            try
            {
                ThunderstorePackage = ThunderstoreInstance.GetPackage("HAHOOS", "KeepInventory");
                if (ThunderstorePackage != null)
                {
                    if (ThunderstorePackage.Latest != null && !string.IsNullOrWhiteSpace(ThunderstorePackage.Latest.Version))
                    {
                        IsLatestVersion = ThunderstorePackage.IsLatestVersion(Version);
                        if (!IsLatestVersion)
                        {
                            LoggerInstance.Warning($"A new version of KeepInventory is available: v{ThunderstorePackage.Latest.Version} while the current is v{Version}. It is recommended that you update");
                        }
                        else
                        {
                            if (SemVersion.Parse(Version) == ThunderstorePackage.Latest.SemVersion)
                            {
                                LoggerInstance.Msg($"Latest version of KeepInventory is installed! --> v{Version}");
                            }
                            else
                            {
                                LoggerInstance.Msg($"Beta release of KeepInventory is installed (v{ThunderstorePackage.Latest.Version} is newest, v{Version} is installed)");
                            }
                        }
                    }
                    else
                    {
                        LoggerInstance.Warning("Latest version could not be found or the version is empty");
                    }
                }
                else
                {
                    LoggerInstance.Warning("Could not find thunderstore package for KeepInventory");
                }
            }
            catch (Exception e)
            {
                LoggerInstance.Error($"An unexpected error has occurred while trying to check if KeepInventory is the latest version, exception:\n{e}");
            }
        }

        public override void OnApplicationQuit()
        {
            SavePreferences();
            SaveManager.Saves.ForEach(x =>
            {
                x.IsFileWatcherEnabled = false;
                x.TrySaveToFile(true);
            });
        }

        internal static void MsgPrefix(string message, string prefix, System.Drawing.Color color)
        {
            Logger.Msg($"[{prefix.Pastel(color)}] {message}");
        }

        public void SavePreferences()
        {
            LoggerInstance.Msg("Saving Preferences");
            try
            {
                PrefsCategory?.SaveToFile(false);
                LoggerInstance.Msg("Saved Preferences successfully");
            }
            catch (Exception e)
            {
                LoggerInstance.Error($"An unexpected error has occurred while saving preferences\n{e}");
            }
        }

        private void LevelUnloadedEvent()
        {
            if (!mp_saveOnLevelUnload.Value) return;
            var list = new List<string>(mp_blacklistedLevels.Value);
            if (mp_blacklistBONELABlevels.Value) list.AddRange(bonelabBlacklist);
            if (mp_blacklistLABWORKSlevels.Value) list.AddRange(labworksBlacklist);
            if (!list.Contains(levelInfo.barcode))
            {
                if (CurrentSave != null)
                    InventoryManager.SaveInventory(CurrentSave);
                else
                    LoggerInstance.Warning("No default save is set, cannot save");
            }
            else
            {
                LoggerInstance.Warning("Not saving due to the level being blacklisted");
            }
        }

        private void LevelLoadedEvent(LevelInfo obj)
        {
            levelInfo = obj;
            if (InitialLoad)
            {
                if (FailedFLLoad)
                {
                    BLHelper.SendNotification("Failure", "The Fusion Support Library has failed to load, meaning the fusion support for the mod will be disabled. If this will occur again, please report it to the developer (discord @hahoos)", true, 10f, BoneLib.Notifications.NotificationType.Error);
                    IsFusionLibraryInitialized = false;
                }
                if (!IsLatestVersion && ThunderstorePackage != null)
                {
                    BLHelper.SendNotification(
                        "Update!",
                        new NotificationText($"There is a new version of KeepInventory. Go to Thunderstore and download the latest version which is <color=#00FF00>v{ThunderstorePackage.Latest.Version}</color>", Color.white, true),
                        true,
                        5f,
                        NotificationType.Warning);
                }
                InitialLoad = false;
            }
            var list = new List<string>(mp_blacklistedLevels.Value);
            if (mp_blacklistBONELABlevels.Value) list.AddRange(bonelabBlacklist);
            if (mp_blacklistLABWORKSlevels.Value) list.AddRange(labworksBlacklist);
            if (!list.Contains(obj.barcode))
            {
                if (mp_loadOnLevelLoad.Value)
                {
                    if (HasFusion && Utilities.Fusion.IsConnected && !IsFusionLibraryInitialized)
                    {
                        LoggerInstance.Warning("The Fusion Library is not loaded. Try restarting the game.");
                    }
                    else
                    {
                        try
                        {
                            statusElement.ElementName = "Current level is not blacklisted";
                            statusElement.ElementColor = Color.green;

                            InventoryManager.LoadSavedInventory(CurrentSave);
                        }
                        catch (System.Exception ex)
                        {
                            LoggerInstance.Error("An error occurred while loading the inventory", ex);
                            BLHelper.SendNotification("Failure", "Failed to load the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
                        }
                    }
                }
            }
            else
            {
                LoggerInstance.Warning("Not loading inventory because level is blacklisted");
                BLHelper.SendNotification("This level is blacklisted from loading/saving inventory", "Blacklisted", true, 5f, BoneLib.Notifications.NotificationType.Warning);
                statusElement.ElementName = "Current level is blacklisted";
                statusElement.ElementColor = Color.red;
            }
        }

        private void SetupPreferences()
        {
            if (!Directory.Exists(KI_PreferencesDirectory))
            {
                LoggerInstance.Msg("Creating preferences directory");
                Directory.CreateDirectory(KI_PreferencesDirectory);
            }
            PrefsCategory = MelonPreferences.CreateCategory("KeepInventory_Settings");
            PrefsCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "Config.cfg"));

            mp_itemsaving = PrefsCategory.CreateEntry<bool>("ItemSaving", true, "Item Saving",
                description: "If true, will save and load items in inventory");
            mp_ammosaving = PrefsCategory.CreateEntry<bool>("AmmoSaving", true, "Ammo Saving",
                description: "If true, will save and load ammo in inventory");
            mp_saveGunData = PrefsCategory.CreateEntry<bool>("SaveGunData", true, "Save Gun Data",
                description: "If true, will save and load data about guns stored in slots, info such as rounds left etc.");
            mp_defaultSave = PrefsCategory.CreateEntry<string>("DefaultSave", string.Empty, "Default Save",
                description: "ID of the save that will be used for things such as loading inventory on load or saving the inventory on level unload");

            mp_saveOnLevelUnload = PrefsCategory.CreateEntry<bool>("SaveOnLevelUnload", true, "Save On Level Unload",
                description: "If true, during level unload, the inventory will be automatically saved");
            mp_loadOnLevelLoad = PrefsCategory.CreateEntry<bool>("LoadOnLevelLoad", true, "Load On Level Load",
                description: "If true, the saved inventory will be automatically loaded when you get loaded into a level thats not blacklisted");

            mp_blacklistBONELABlevels = PrefsCategory.CreateEntry<bool>("BlacklistBONELABLevels", true, "Blacklist BONELAB Levels",
                description: "If true, most of the BONELAB levels (except VoidG114 and BONELAB Hub) will be blacklisted from saving/loading inventory");
            mp_blacklistLABWORKSlevels = PrefsCategory.CreateEntry<bool>("BlacklistLABWORKSLevels", true, "Blacklist LABWORKS Levels",
                description: "If true, LABWORKS levels from the campaign (not sandbox levels) will be blacklisted from saving/loading inventory");
            mp_blacklistedLevels = PrefsCategory.CreateEntry<List<string>>("BlacklistedLevels", [], "Blacklisted Levels",
                description: "List of levels that will not save/load inventory");

            mp_showNotifications = PrefsCategory.CreateEntry<bool>("ShowNotifications", true, "Show Notifications",
                description: "If true, notifications will be shown in-game regarding errors or other things");
            mp_configVersion = PrefsCategory.CreateEntry<int>("ConfigVersion", 1, "Config Version",
                description: "DO NOT CHANGE THIS AT ALL, THIS WILL BE USED FOR MIGRATING CONFIGS AND SHOULD NOT BE CHANGED AT ALL");
            mp_holsterHeldWeaponsOnDeath = PrefsCategory.CreateEntry<bool>("HolsterHeldWeaponsOnDeath", true, "Holster Held Weapons On Death",
                description: "If true, when you die all of the weapons you were holding get holstered if possible");

            PrefsCategory.SaveToFile(false);
        }
    }
}