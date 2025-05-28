using MelonLoader;
using MelonLoader.Utils;

using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.Notifications;

using UnityEngine;

using Il2CppSLZ.Marrow;

using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Reflection;

using Semver;

using MelonLoader.Pastel;

using KeepInventory.Helper;
using KeepInventory.Utilities;
using KeepInventory.Menu;
using System.Threading;
using KeepInventory.Managers;

namespace KeepInventory
{
    /// <summary>
    /// Main class containing most of the functionality of KeepInventory
    /// </summary>
    public class Core : MelonMod
    {
        /// <summary>
        /// Current version of KeepInventory, used mostly for AssemblyInfo
        /// </summary>
        public const string Version = "1.3.0";

        /// <summary>
        /// The current save for the inventory
        /// </summary>
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

        /// <summary>
        /// Run when <see cref="CurrentSave"/> gets changed
        /// </summary>
        public static event Action DefaultSaveChanged;

        /// <summary>
        /// Instance of the <see cref="Core"/> class
        /// </summary>
        public static Core Instance { get; internal set; }

        /// <summary>
        /// Assembly of MelonLoader
        /// </summary>
        internal static Assembly MLAssembly;

        /// <summary>
        /// List of all blacklisted BONELAB levels
        /// </summary>
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

        /// <summary>
        /// List of all blacklisted LABWORKS levels
        /// </summary>
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

        /// <summary>
        /// Path to the preferences directory of KeepInventory
        /// </summary>
        public readonly static string KI_PreferencesDirectory = Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory");

        /// <summary>
        /// Category containing all of the config
        /// </summary>
        internal static MelonPreferences_Category PrefsCategory;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should items be saved and/or loaded
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_itemsaving;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should ammo be saved and/or loaded
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_ammosaving;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should gun data be saved and/or loaded
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_saveGunData;

        /// <summary>
        /// An entry with a string value indicating what save is default, which is which should be used when automatically loading and saving inventory. The string value should be the ID
        /// </summary>
        internal static MelonPreferences_Entry<string> mp_defaultSave;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should the inventory be saved on level unload
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_saveOnLevelUnload;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should the inventory be loaded on level load
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_loadOnLevelLoad;

        /// <summary>
        /// An entry with a list of all blacklisted levels from loading/saving inventory
        /// </summary>
        internal static MelonPreferences_Entry<List<string>> mp_blacklistedLevels;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should BONELAB levels (except VoidG114 and BL Hub) be blacklisted
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_blacklistBONELABlevels;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should LABWORKS levels (except Sandbox levels, only campaign) be blacklisted
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_blacklistLABWORKSlevels;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should the mod show notifications
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_showNotifications;

        /// <summary>
        /// An entry with an int value that indicates what version of config is it using (will be used for migrating configs in the future updates, if there will be any)
        /// </summary>
        internal static MelonPreferences_Entry<int> mp_configVersion;

        /// <summary>
        /// If true, when you die all of the weapons you were holding get holstered if possible
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_holsterHeldWeaponsOnDeath;

        /// <summary>
        /// Variable of element in BoneMenu responsible for showing if level is blacklisted or not, and changing it
        /// </summary>
        internal static FunctionElement statusElement;

        /// <summary>
        /// <see cref="LevelInfo"/> of the current level
        /// </summary>
        internal static LevelInfo levelInfo;

        /// <summary>
        /// Boolean value indicating if user has Fusion
        /// </summary>
        public static bool HasFusion => FindMelon("LabFusion", "Lakatrazz") != null;

        /// <summary>
        /// Boolean value indicating whether or not was the Fusion Library for KeepInventory loaded/initialized
        /// </summary>
        public static bool IsFusionLibraryInitialized { get; internal set; } = false;

        /// <summary>
        /// Boolean value indicating whether or not Fusion Support Library failed to load
        /// </summary>
        public static bool FailedFLLoad { get; internal set; } = false;

        /// <summary>
        /// A boolean value indicating whether or not will this be the first time <see cref="LevelLoadedEvent(LevelInfo)"/> will be run
        /// </summary>
        private bool InitialLoad = true;

        /// <summary>
        /// Instance of <see cref="KeepInventory.Utilities.Thunderstore"/>
        /// </summary>
        internal static Thunderstore ThunderstoreInstance { get; private set; }

        /// <summary>
        /// Is the mod the latest version
        /// </summary>
        internal static bool IsLatestVersion { get; private set; } = true;

        /// <summary>
        /// <see cref="Package"/> of KeepInventory
        /// </summary>
        internal static Package ThunderstorePackage { get; private set; }

        /// <summary>
        /// The thread that Unity runs on
        /// </summary>
        public Thread UnityThread { get; private set; }

        /// <summary>
        /// Is the current thread the unity thread
        /// </summary>
        public bool IsCurrentThreadMainThread
            => Thread.CurrentThread == UnityThread;

        /// <summary>
        /// Calls when MelonLoader loads all Mods/Plugins
        /// </summary>
        public override void OnInitializeMelon()
        {
            UnityThread = Thread.CurrentThread;
            MLAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "MelonLoader");
            Logger = LoggerInstance;
            Instance = this;

            LoggerInstance.Msg("Setting up KeepInventory");

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

            if (!HasFusion)
            {
                LoggerInstance.Warning("Could not find LabFusion, the mod will not use any of Fusion's functionality");
            }
            else
            {
                LoggerInstance.Msg("Found LabFusion");
            }

            if (HasFusion)
            {
                LoggerInstance.Msg("Attempting to load the Fusion Support Library");
                try
                {
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    if (assembly != null)
                    {
                        var assemblyInfo = assembly.GetName();
                        if (assemblyInfo != null)
                        {
                            var _path = $"{assemblyInfo.Name}.Embedded.Dependencies.KeepInventory.Fusion.dll";
                            var names = assembly.GetManifestResourceNames();
                            if (names == null || names.Length == 0 || !names.Contains(_path))
                            {
                                FailedFLLoad = true;
                                LoggerInstance.Error("There were no embedded resources or dependency was not found in the list of embedded resources, cannot not load Fusion Support Library");
                            }
                            else
                            {
                                var stream = assembly.GetManifestResourceStream(_path);
                                if (stream?.Length > 0)
                                {
                                    var bytes = StreamToByteArray(stream);
                                    System.Reflection.Assembly.Load(bytes);
                                    LoggerInstance.Msg("Loaded Fusion Support Library");
                                    IsFusionLibraryInitialized = true;
                                }
                                else
                                {
                                    FailedFLLoad = true;
                                    LoggerInstance.Error("Could not get stream of Fusion Support Library, cannot not load Fusion Support Library");
                                }
                            }
                        }
                        else
                        {
                            FailedFLLoad = true;
                            LoggerInstance.Error("Assembly Info was not found, cannot not load Fusion Support Library");
                        }
                    }
                    else
                    {
                        FailedFLLoad = true;
                        LoggerInstance.Error("Executing assembly was somehow not found, cannot not load Fusion Support Library");
                    }
                }
                catch (Exception ex)
                {
                    FailedFLLoad = true;
                    LoggerInstance.Error($"An unexpected error occurred while loading the library:\n{ex}");
                }
            }

            Hooking.OnLevelLoaded += LevelLoadedEvent;
            Hooking.OnLevelUnloaded += LevelUnloadedEvent;

            if (IsFusionLibraryInitialized) Utilities.Fusion.SetupFusionLibrary();
            if (HasFusion) KeepInventory.Utilities.Fusion.Setup();

            SetupPreferences();
            SaveManager.Setup();
            BoneMenu.Setup();
        }

        internal static event Action Update;

        /// <summary>
        /// Runs every frame
        /// </summary>
        public override void OnUpdate()
        {
            var inv = AmmoInventory.Instance;
            if (inv != null)
            {
                // HACK: For whatever reason when the level is unloading it sets the ammo to 10000000
                if (inv._groupCounts.ContainsKey("light") && inv._groupCounts["light"] != 10000000) _lastAmmoCount_light = inv._groupCounts["light"];
                if (inv._groupCounts.ContainsKey("medium") && inv._groupCounts["medium"] != 10000000) _lastAmmoCount_medium = inv._groupCounts["medium"];
                if (inv._groupCounts.ContainsKey("heavy") && inv._groupCounts["heavy"] != 10000000) _lastAmmoCount_heavy = inv._groupCounts["heavy"];
            }
            Update?.Invoke();
        }

        /// <summary>
        /// Converts a <see cref="Stream"/> to a byte array
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to convert</param>
        /// <returns>A<see cref="byte"/> array of the provided <see cref="Stream"/></returns>
        public static byte[] StreamToByteArray(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using MemoryStream ms = new();
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Runs when the application is about to quit
        /// </summary>
        public override void OnApplicationQuit()
        {
            SavePreferences();
            SaveManager.Saves.ForEach(x =>
            {
                x.IsFileWatcherEnabled = false;
                x.TrySaveToFile(true);
            });
        }

        /// <summary>
        /// Send a message to the console with a prefix
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="prefix">The prefix of the message</param>
        /// <param name="color">The color of the prefix</param>
        internal static void MsgPrefix(string message, string prefix, System.Drawing.Color color)
        {
            Logger.Msg($"[{prefix.Pastel(color)}] {message}");
        }

        /// <summary>
        /// Triggers when application is being quit <br/>
        /// Used to save inventory if PersistentSave is turned on<br/>
        /// <b>Will not trigger when trying to close MelonLoader, rather than the game</b>
        /// </summary>
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

        /// <summary>
        /// Called when a BONELAB Level is unloaded<br/>
        /// Used now for saving inventory
        /// </summary>
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
                    Core.Logger.Warning("No default save is set, cannot save");
            }
            else
            {
                LoggerInstance.Warning("Not saving due to the level being blacklisted");
            }
        }

        /// <summary>
        /// Called when a BONELAB Level is loaded<br/>
        /// Mostly used to load the inventory as of now
        /// </summary>
        /// <param name="obj">Contains Level Information</param>
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
                        LoggerInstance.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly");
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

        /// <summary>
        /// Set up Preferences
        /// </summary>
        private void SetupPreferences()
        {
            if (!Directory.Exists(KI_PreferencesDirectory))
            {
                LoggerInstance.Msg("Creating preferences directory");
                Directory.CreateDirectory(KI_PreferencesDirectory);
            }
            PrefsCategory = MelonPreferences.CreateCategory("KeepInventory_Settings");
            PrefsCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "Config.cfg"));

            // Saving

            mp_itemsaving = PrefsCategory.CreateEntry<bool>("ItemSaving", true, "Item Saving",
                description: "If true, will save and load items in inventory");
            mp_ammosaving = PrefsCategory.CreateEntry<bool>("AmmoSaving", true, "Ammo Saving",
                description: "If true, will save and load ammo in inventory");
            mp_saveGunData = PrefsCategory.CreateEntry<bool>("SaveGunData", true, "Save Gun Data",
                description: "If true, will save and load data about guns stored in slots, info such as rounds left etc.");
            mp_defaultSave = PrefsCategory.CreateEntry<string>("DefaultSave", string.Empty, "Default Save",
                description: "ID of the save that will be used for things such as loading inventory on load or saving the inventory on level unload");

            // Events

            mp_saveOnLevelUnload = PrefsCategory.CreateEntry<bool>("SaveOnLevelUnload", true, "Save On Level Unload",
                description: "If true, during level unload, the inventory will be automatically saved");
            mp_loadOnLevelLoad = PrefsCategory.CreateEntry<bool>("LoadOnLevelLoad", true, "Load On Level Load",
                description: "If true, the saved inventory will be automatically loaded when you get loaded into a level thats not blacklisted");

            // Blacklist

            mp_blacklistBONELABlevels = PrefsCategory.CreateEntry<bool>("BlacklistBONELABLevels", true, "Blacklist BONELAB Levels",
                description: "If true, most of the BONELAB levels (except VoidG114 and BONELAB Hub) will be blacklisted from saving/loading inventory");
            mp_blacklistLABWORKSlevels = PrefsCategory.CreateEntry<bool>("BlacklistLABWORKSLevels", true, "Blacklist LABWORKS Levels",
                description: "If true, LABWORKS levels from the campaign (not sandbox levels) will be blacklisted from saving/loading inventory");
            mp_blacklistedLevels = PrefsCategory.CreateEntry<List<string>>("BlacklistedLevels", [], "Blacklisted Levels",
                description: "List of levels that will not save/load inventory");

            // Other

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