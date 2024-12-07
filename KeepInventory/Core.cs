using MelonLoader;
using MelonLoader.Utils;
using MelonLoader.Preferences;

using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.Notifications;

using UnityEngine;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow;

using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Reflection;

using MelonLoader.Pastel;

using KeepInventory.Helper;
using KeepInventory.Saves;
using KeepInventory.Utilities;

namespace KeepInventory
{
    /// <summary>
    /// Main class containing most of the functionality of KeepInventory
    /// </summary>
    public class Core : MelonMod
    {
        #region Variables

        /// <summary>
        /// Current version of KeepInventory, used mostly for AssemblyInfo
        /// </summary>
        public const string Version = "1.2.0";

        /// <summary>s
        /// The current save for the inventory
        /// </summary>
        public static Save CurrentSave { get; internal set; }

        /// <summary>
        /// Instance of the <see cref="Core"/> class
        /// </summary>
        public static Core Instance { get; internal set; }

        /// <summary>
        /// Assembly of MelonLoader
        /// </summary>
        internal static Assembly MLAssembly;

        /// <summary>
        /// List of all default blacklisted BONELAB levels
        /// </summary>
        public readonly static List<string> defaultBlacklistedLevels = [
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

        internal static MelonLogger.Instance Logger { get; private set; }

        #region MelonPreferences

        /// <summary>
        /// Path to the preferences directory of KeepInventory
        /// </summary>
        public readonly static string KI_PreferencesDirectory = Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory");

        #region Categories

        /// <summary>
        /// Category containing all of the config
        /// </summary>
        internal static MelonPreferences_Category PrefsCategory;

        /// <summary>
        /// Category containing the current save, like ammo, items etc.
        /// </summary>
        internal static MelonPreferences_ReflectiveCategory SaveCategory;

        #endregion Categories

        #region Entries

        #region Saving

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
        /// An entry with a boolean value indicating whether or not should the inventory be saved to <see cref="SaveCategory"/> to be used between game sessions
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_persistentsave;

        #endregion Saving

        #region Events

        /// <summary>
        /// An entry with a boolean value indicating whether or not should the inventory be saved on level unload
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_saveOnLevelUnload;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should the inventory be loaded on level load
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_loadOnLevelLoad;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should the inventory be saved to file on application quit
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_automaticallySaveToFile;

        #endregion Events

        #region Blacklist

        /// <summary>
        /// An entry with a list of all blacklisted levels from loading/saving inventory
        /// </summary>
        internal static MelonPreferences_Entry<List<string>> mp_blacklistedLevels;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should BONELAB levels (except VoidG114 and BL Hub) be blacklisted
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_blacklistBONELABlevels;

        #endregion Blacklist

        #region Other

        /// <summary>
        /// An entry with a boolean value indicating whether or not should the mod show notifications
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_showNotifications;

        /// <summary>
        /// An entry with a boolean value indicating whether or not should the mod work with Fusion (use this in case something breaks with Fusion)
        /// </summary>
        internal static MelonPreferences_Entry<bool> mp_fusionSupport;

        /// <summary>
        /// An entry with an int value that indicates what version of config is it using (will be used for migrating configs in the future updates, if there will be any)
        /// </summary>
        internal static MelonPreferences_Entry<int> mp_configVersion;

        internal static MelonPreferences_Entry<bool> mp_initialInventoryRemove;

        #endregion Other

        #endregion Entries

        #endregion MelonPreferences

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
        public static bool HasFusion { get; private set; }

        /// <summary>
        /// Boolean value indicating if user is connected to a server
        /// </summary>
        public static bool IsConnected
        {
            get
            {
                if (HasFusion && IsFusionLibraryInitialized) return FusionLibraryIsConnected();
                else if (HasFusion && !IsFusionLibraryInitialized) return FusionIsConnected();
                else return false;
            }
        }

        /// <summary>
        /// Boolean value indicating whether or not was the Fusion Library for KeepInventory loaded/initialized
        /// </summary>
        public static bool IsFusionLibraryInitialized { get; private set; } = false;

        /// <summary>
        /// Boolean value indicating whether or not Fusion Support Library failed to load
        /// </summary>
        public static bool FailedFLLoad { get; private set; } = false;

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
        /// A boolean value indicating if the next <see cref="KeepInventory.Patches.AmmoInventoryPatches.Awake"/> should run
        /// </summary>
        internal static bool LoadAmmoOnAwake = false;

        #endregion Variables

        #region Methods

        #region MelonLoader

        /// <summary>
        /// Calls when MelonLoader loads all Mods/Plugins
        /// </summary>
        public override void OnInitializeMelon()
        {
            MLAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "MelonLoader");
            Logger = LoggerInstance;
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
                        if (!IsLatestVersion) LoggerInstance.Warning($"A new version of KeepInventory is available: v{ThunderstorePackage.Latest.Version}. It is recommended that you update");
                        else LoggerInstance.Msg("Latest version of KeepInventory is installed!");
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

            SetupPreferences();
            SetupMenu();

            HasFusion = HelperMethods.CheckIfAssemblyLoaded("labfusion");
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

            if (IsFusionLibraryInitialized) SetupFusionLibrary();
        }

        #endregion MelonLoader

        #region Fusion

        /// <summary>
        /// Setup the Fusion Support Library
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void SetupFusionLibrary()
        {
            Logger.Msg("Setting up the library");
            try
            {
                Fusion.FusionMethods.Setup(Logger);
                Fusion.FusionMethods.LoadModule();
            }
            catch (Exception ex)
            {
                FailedFLLoad = true;
                Logger.Error($"An unexpected error has occurred while setting up and/or loading the fusion module, exception:\n{ex}");
            }
        }

        /// <summary>
        /// Check if the player is connected to a Fusion server with the Fusion Support Library
        /// </summary>
        /// <returns>A boolean value indicating whether or not is the player connected to a server</returns>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static bool FusionLibraryIsConnected()
        {
            return Fusion.FusionMethods.LocalNetworkPlayer != null;
            //return LabFusion.Network.NetworkInfo.HasServer;
        }

        /// <summary>
        /// Check if the player is connected to a Fusion server without the Fusion Support Library
        /// </summary>
        /// <returns>A boolean value indicating whether or not is the player connected to a server</returns>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static bool FusionIsConnected()
        {
            return LabFusion.Player.LocalPlayer.GetNetworkPlayer() != null;
        }

        /// <summary>
        /// Spawns an item in the provided <see cref="InventorySlotReceiver"/>
        /// </summary>
        /// <param name="barcode"><see cref="Barcode"/> of the spawnable you want to spawn into the provided <see cref="InventorySlotReceiver"/></param>
        /// <param name="inventorySlotReceiver"><see cref="InventorySlotReceiver"/> in which should the spawnable be spawned</param>
        /// <param name="slotName">Name of the slot (debug purposes)</param>
        /// <param name="slotColor">Color of the slot name (debug purposes)</param>
        /// <param name="inBetween">The <see cref="Action{GameObject}"/> that will be run between spawning and putting the spawnable into the <see cref="InventorySlotReceiver"/></param>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void FusionSpawnInSlot(Barcode barcode, InventorySlotReceiver inventorySlotReceiver, string slotName, System.Drawing.Color slotColor, Action<GameObject> inBetween = null)
        {
            Fusion.FusionMethods.NetworkSpawnInSlotAsync(inventorySlotReceiver, barcode, slotColor, slotName, inBetween);
        }

        /// <summary>
        /// Find the <see cref="RigManager"/> for the local player that is connected to a server with the Fusion Support Library
        /// </summary>
        /// <returns>The <see cref="RigManager"/> of the local player</returns>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static RigManager FusionFindRigManager_FSL()
        {
            return Fusion.FusionMethods.RigManager ?? Player.RigManager;
        }

        /// <summary>
        /// Find the <see cref="RigManager"/> for the local player that is connected to a server
        /// </summary>
        /// <returns>The <see cref="RigManager"/> of the local player</returns>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static RigManager FusionFindRigManager()
        {
            if (!IsConnected)
            {
                return Player.RigManager;
            }
            else
            {
                return IsFusionLibraryInitialized ? FusionFindRigManager_FSL() : (LabFusion.Player.LocalPlayer.GetNetworkPlayer().RigRefs.RigManager ?? Player.RigManager);
            }
        }

        /// <summary>
        /// Removes the <see cref="SpawnSavedItems(RigManager)"/> method from the OnRigCreated event in the Fusion Support Library
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(
   System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void RemoveRigCreateEvent_FSL()
        {
            Fusion.FusionMethods.OnRigCreated -= SpawnSavedItems;
        }

        /// <summary>
        /// Removes the <see cref="SpawnSavedItems(RigManager)"/> method from the OnRigCreated event in the Fusion Support Library
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(
   System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void RemoveRigCreateEvent()
        {
            if (HasFusion && IsConnected && IsFusionLibraryInitialized) RemoveRigCreateEvent_FSL();
        }

        [System.Runtime.CompilerServices.MethodImpl(
   System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static AmmoInventory FusionGetAmmoInventory_FSL()
        {
            return Fusion.FusionMethods.FindAmmoInventory();
        }

        /// <summary>
        /// Find the <see cref="AmmoInventory"/> of the local player
        /// </summary>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(
   System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static AmmoInventory FusionGetAmmoInventory()
        {
            if (HasFusion) return IsFusionLibraryInitialized ? (FusionGetAmmoInventory_FSL() ?? AmmoInventory.Instance) : (LabFusion.Marrow.NetworkGunManager.NetworkAmmoInventory ?? AmmoInventory.Instance);
            else return AmmoInventory.Instance;
        }

        /// <summary>
        /// Spawn the saved items, run when Fusion is detected
        /// <para>This is separate to avoid errors if Fusion Support Library is not loaded</para>
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void FusionSpawnSavedItems_FSL()
        {
            if (Fusion.FusionMethods.RigManager == null)
            {
                Logger.Msg("Rig not found, awaiting");
                Fusion.FusionMethods.OnRigCreated += SpawnSavedItems;
            }
            else
            {
                Logger.Msg("Rig found, spawning");
                SpawnSavedItems(Fusion.FusionMethods.RigManager);
            }
        }

        /// <summary>
        /// Spawn the saved items, run when Fusion is detected
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void FusionSpawnSavedItems()
        {
            if (IsConnected)
            {
                LoggerInstance.Msg("Client is connected to a server");
                if (mp_itemsaving.Value)
                {
                    if (IsFusionLibraryInitialized) FusionSpawnSavedItems_FSL();
                    else SpawnSavedItems(null);
                }
            }
            else
            {
                LoggerInstance.Msg("Client is not connected to a server, spawning locally");
                if (mp_itemsaving.Value)
                {
                    SpawnSavedItems(null);
                }
            }
        }

        /// <summary>
        /// Check if a gamemode is currently running in the server
        /// </summary>
        /// <returns>A boolean value indicating whether or not is a gamemode running</returns>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        internal static bool FusionGamemodeCheck()
        {
            if (!IsConnected) return false;
            else return LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeStarted || LabFusion.SDK.Gamemodes.GamemodeManager.StartTimerActive || LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeReady;
        }

        #endregion Fusion

        #region Other

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
        /// Send a message to the console with a prefix
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="prefix">The prefix of the message</param>
        /// <param name="color">The color of the prefix</param>
        internal static void MsgPrefix(string message, string prefix, System.Drawing.Color color)
        {
            Logger._MsgPastel($"[{prefix.Pastel(color)}] {message}");
        }

        /// <summary>
        /// Triggers when application is being quit <br/>
        /// Used to save inventory if PersistentSave is turned on<br/>
        /// <b>Will not trigger when trying to close MelonLoader, rather than the game</b>
        /// </summary>
        public void SavePreferences()
        {
            if (mp_persistentsave.Value && mp_automaticallySaveToFile.Value)
            {
                LoggerInstance.Msg("Saving Preferences");
                try
                {
                    SaveCategory?.SaveToFile(false);
                    PrefsCategory?.SaveToFile(false);
                    LoggerInstance.Msg("Saved Preferences successfully");
                }
                catch (Exception e)
                {
                    LoggerInstance.Error($"An unexpected error has occurred while saving prefs\n{e}");
                }
            }
        }

        /// <summary>
        /// Find the <see cref="RigManager"/> of the player
        /// </summary>
        /// <returns>The <see cref="RigManager"/> of the player</returns>
        internal static RigManager FindRigManager()
        {
            if (HasFusion && IsConnected && IsFusionLibraryInitialized) return FusionFindRigManager();
            else return Player.RigManager;
        }

        /// <summary>
        /// Find the <see cref="AmmoInventory"/> of the player
        /// </summary>
        /// <returns>The <see cref="AmmoInventory"/> of the player</returns>
        internal static AmmoInventory GetAmmoInventory()
        {
            if (HasFusion && IsFusionLibraryInitialized) return FusionGetAmmoInventory();
            else return AmmoInventory.Instance;
        }

        #endregion Other

        #region Saving & Loading

        /// <summary>
        /// Saves the current inventory, overriding <see cref="CurrentSave"/>
        /// </summary>
        public void SaveInventory(bool notifications = false)
        {
            try
            {
                if (HasFusion && IsConnected && (!IsFusionLibraryInitialized || !mp_fusionSupport.Value))
                {
                    BLHelper.SendNotification("Failure", "Could not save inventory, because either the 'Fusion Support' setting is set to Disabled or the Fusion Support Library did not load correctly", true, 3.5f, BoneLib.Notifications.NotificationType.Error);
                    LoggerInstance.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly");
                    return;
                }
                LoggerInstance.Msg("Saving inventory...");
                bool isItemSaved = false;
                bool isAmmoSaved = false;
                if (mp_itemsaving.Value)
                {
                    LoggerInstance.Msg("Saving items in inventory slots");

                    bool notFound = false;
                    var rigManager = FindRigManager();
                    if (rigManager == null)
                    {
                        LoggerInstance.Warning("RigManager does not exist");
                        notFound = true;
                    }

                    if (!notFound)
                    {
                        CurrentSave.InventorySlots?.Clear();
                        var components = rigManager.GetComponentsInChildren<InventorySlotReceiver>();
                        foreach (var item in components)
                        {
                            if (item == null || item?._weaponHost == null || item?._weaponHost.GetTransform() == null) continue;
                            if (item?._weaponHost != null)
                            {
                                var gun = item._weaponHost.GetTransform().GetComponent<Gun>();
                                GunInfo gunInfo = null;
                                if (gun != null)
                                {
                                    gunInfo = GunInfo.Parse(gun);
                                }
                                var poolee = item._weaponHost.GetTransform().GetComponent<Poolee>();
                                if (poolee != null)
                                {
                                    var name = item.transform.parent.name;
                                    if (name.StartsWith("prop")) name = item.transform.parent.parent.name;
                                    var barcode = poolee.SpawnableCrate.Barcode;
                                    LoggerInstance.Msg($"Slot: {name} / Barcode: {poolee.SpawnableCrate.name} ({poolee.SpawnableCrate.Barcode.ID})");
                                    if (gunInfo != null && mp_saveGunData.Value && poolee.SpawnableCrate.Barcode != new Barcode(CommonBarcodes.Misc.SpawnGun))
                                    {
                                        CurrentSave.InventorySlots.Add(new SaveSlot(name, barcode, gunInfo));
                                    }
                                    else
                                    {
                                        CurrentSave.InventorySlots.Add(new SaveSlot(name, barcode));
                                    }
                                }
                                else
                                {
                                    Logger.Warning($"[{item.transform.parent.name}] Could not find poolee of the spawnable in the inventory slot");
                                }
                            }
                        }
                        if (CurrentSave.InventorySlots.Count == 0)
                        {
                            LoggerInstance.Msg("No spawnables were found in slots");
                        }
                        isItemSaved = true;
                    }
                    else
                    {
                        LoggerInstance.Error("Could not save inventory, because some required game objects were not found. Items from the earlier save will be kept");
                        if (notifications) BLHelper.SendNotification("Failure", "Failed to save the inventory, because some required game objects were not found, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
                    }
                }
                if (mp_ammosaving.Value)
                {
                    var ammoInventory = GetAmmoInventory();
                    CurrentSave.LightAmmo = ammoInventory.GetCartridgeCount("light");
                    LoggerInstance.Msg("Saved Light Ammo: " + CurrentSave.LightAmmo);
                    CurrentSave.MediumAmmo = ammoInventory.GetCartridgeCount("medium");
                    LoggerInstance.Msg("Saved Medium Ammo: " + CurrentSave.MediumAmmo);
                    CurrentSave.HeavyAmmo = ammoInventory.GetCartridgeCount("heavy");
                    LoggerInstance.Msg("Saved Heavy Ammo: " + CurrentSave.LightAmmo);
                    isAmmoSaved = true;
                }
                SavePreferences();
                LoggerInstance.Msg("Successfully saved inventory");

                string formatString()
                {
                    string list = "";
                    if (isItemSaved)
                    {
                        if (!string.IsNullOrWhiteSpace(list)) list = $"{list}, Items";
                        else list = "Items";
                    }
                    if (isAmmoSaved)
                    {
                        if (!string.IsNullOrWhiteSpace(list)) list = $"{list}, Ammo";
                        else list = "Ammo";
                    }
                    if (string.IsNullOrWhiteSpace(list)) list = "N/A";
                    return list;
                }

                if (notifications) BLHelper.SendNotification("Success", $"Successfully saved the inventory, the following was saved: {formatString()}", true, 5f, BoneLib.Notifications.NotificationType.Success);
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"An unexpected error occurred while saving the inventory:\n{ex}");
                if (notifications) BLHelper.SendNotification("Failure", "Failed to save the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
            }
        }

        internal static bool RemoveInitialInventoryFromSave()
        {
            Action staged = null;
            foreach (var item in Il2CppSLZ.Bonelab.SaveData.DataManager.ActiveSave.Progression.LevelState)
            {
                bool changed = false;
                foreach (var y in item.Value)
                {
                    if (y.Key == "SLZ.Bonelab.initial_inventory" && y.Value != null)
                    {
                        Core.Logger.Warning($"Found initial inventory in save (Level: {item.Key}), removing");
                        staged += () =>
                        {
                            changed = true;
                            item.Value[y.Key] = null;
                        };
                    }
                }
                staged += () =>
                {
                    if (!changed) return;
                    Il2CppSLZ.Bonelab.SaveData.DataManager.ActiveSave.Progression.LevelState[item.key] = item.value;
                };
            }
            staged?.Invoke();
            return Il2CppSLZ.Bonelab.SaveData.DataManager.TrySaveActiveSave(Il2CppSLZ.Marrow.SaveData.SaveFlags.Progression);
        }

        internal static bool RemoveInitialInventoryFromSave(string name)
        {
            Action staged = null;
            foreach (var item in Il2CppSLZ.Bonelab.SaveData.DataManager.ActiveSave.Progression.LevelState)
            {
                if (item.key != name) continue;
                bool changed = false;
                foreach (var y in item.Value)
                {
                    if (y.Key == "SLZ.Bonelab.initial_inventory" && y.Value != null)
                    {
                        Core.Logger.Warning($"Found initial inventory in save (Level: {item.Key}), removing");
                        staged += () =>
                        {
                            changed = true;
                            item.Value[y.Key] = null;
                        };
                    }
                }
                staged += () =>
                {
                    if (!changed) return;
                    Il2CppSLZ.Bonelab.SaveData.DataManager.ActiveSave.Progression.LevelState[item.key] = item.value;
                };
            }
            staged?.Invoke();
            return Il2CppSLZ.Bonelab.SaveData.DataManager.TrySaveActiveSave(Il2CppSLZ.Marrow.SaveData.SaveFlags.Progression);
        }

        internal static bool DoesSaveForLevelExist(string name)
        {
            foreach (var item in Il2CppSLZ.Bonelab.SaveData.DataManager.ActiveSave.Progression.LevelState)
            {
                if (string.Equals(item.key, name, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Msg($"Found save for level: {name}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Spawn the saved items in <see cref="CurrentSave"/> to the inventory
        /// </summary>
        /// <param name="_rigManager"><see cref="RigManager"/> of the player if known, otherwise will search for it</param>
        private void SpawnSavedItems(RigManager _rigManager = null)
        {
            if (HasFusion && IsConnected && (!IsFusionLibraryInitialized || !mp_fusionSupport.Value))
            {
                LoggerInstance.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly"); return;
            }
            else if (HasFusion && IsConnected && IsFusionLibraryInitialized)
            {
                RemoveRigCreateEvent();
            }

            try
            {
                if (CurrentSave.InventorySlots?.Count >= 1)
                {
                    var rigManager = _rigManager ?? FindRigManager();
                    if (rigManager == null)
                    {
                        LoggerInstance.Error("RigManager does not exist, cannot load saved items!");
                        return;
                    }
                    if (rigManager.inventory == null)
                    {
                        LoggerInstance.Error("Inventory does not exist, cannot load saved items!");
                        return;
                    }
                    if (rigManager.inventory.bodySlots == null)
                    {
                        LoggerInstance.Error("Body slots do not exist, cannot load saved items!");
                        return;
                    }

                    // Adds saved items to inventory slots
                    var list2 = rigManager.GetComponentsInChildren<InventorySlotReceiver>().ToList();
                    var list = rigManager.inventory.bodySlots.ToList();

                    foreach (var item in CurrentSave.InventorySlots)
                    {
                        var SlotColor = Colors.GetRandomSlotColor();
                        MsgPrefix("Looking for slot", item.SlotName, SlotColor);

                        void spawn(InventorySlotReceiver receiver)
                        {
                            if (MarrowGame.assetWarehouse.HasCrate(new Barcode(item.Barcode)))
                            {
                                // There was an issue with items not loading models in slots, i cant replicate the issue, but adding this to be sure
                                var crate = new SpawnableCrateReference(item.Barcode);
                                crate?.Crate.PreloadAssets();

                                if (item.Type == SaveSlot.SpawnableType.Gun && mp_saveGunData.Value && item.Barcode != CommonBarcodes.Misc.SpawnGun)
                                {
                                    // Settings properties for the gun, this is horrible
                                    void action(GameObject obj)
                                    {
                                        if (item.GunInfo != null && obj != null)
                                        {
                                            var guns = obj.GetComponents<Gun>();
                                            MsgPrefix("Attempting to write GunInfo", item.SlotName, SlotColor);
                                            foreach (var gun in guns) gun.UpdateProperties(item.GunInfo, SlotColor, item, crate.Crate.name, item.Barcode, true, false);
                                        }
                                    }

                                    MsgPrefix($"Spawning to slot: {crate.Crate.name} ({item.Barcode})", item.SlotName, SlotColor);

                                    if (HasFusion && IsConnected)
                                    {
                                        FusionSpawnInSlot(crate.Crate.Barcode, receiver, item.SlotName, SlotColor, action);
                                    }
                                    else
                                    {
                                        var task = receiver.SpawnInSlotAsync(crate.Crate.Barcode);
                                        var awaiter = task.GetAwaiter();
                                        void notGun()
                                        {
                                            action(receiver._weaponHost.GetHostGameObject());
                                        }
                                        awaiter.OnCompleted((Il2CppSystem.Action)notGun);
                                    }
                                }
                                else
                                {
                                    MsgPrefix($"Spawning to slot: {crate.Crate.name} ({item.Barcode})", item.SlotName, SlotColor);
                                    if (HasFusion && IsConnected)
                                    {
                                        FusionSpawnInSlot(crate.Crate.Barcode, receiver, item.SlotName, SlotColor);
                                        MsgPrefix($"Spawned to slot: {crate.Crate.name} ({item.Barcode})", item.SlotName, SlotColor);
                                    }
                                    else
                                    {
                                        var task = receiver.SpawnInSlotAsync(crate.Crate.Barcode);
                                        Action complete = () => MsgPrefix($"Spawned to slot: {crate.Crate.name} ({item.Barcode})", item.SlotName, SlotColor);
                                        task.GetAwaiter().OnCompleted(complete);
                                    }
                                }
                            }
                            else
                            {
                                LoggerInstance.Warning($"[{item.SlotName}] Could not find crate with the following barcode: {item.Barcode}");
                            }
                        }

                        // Check for a slot with the same name and one that is for spawnables, not ammo
                        InventorySlotReceiver slot = list.Find((slot) => slot.name == item.SlotName && slot.inventorySlotReceiver != null)?.inventorySlotReceiver;
                        if (slot != null)
                        {
                            var receiver = slot;
                            //if (receiver._weaponHost?.GetHostGameObject() != null) MonoBehaviour.Destroy(receiver._weaponHost.GetHostGameObject());
                            spawn(receiver);
                        }
                        else
                        {
                            slot = list2.Find((slot) => (slot.transform.parent.name.StartsWith("prop") ? slot.transform.parent.parent.name : slot.transform.parent.name) == item.SlotName);
                            if (slot != null)
                            {
                                spawn(slot);
                            }
                            else
                            {
                                LoggerInstance.Warning($"[{item.SlotName}] No slot was found with the provided name. It is possible that an avatar that was used during the saving had more slots than the current one");
                            }
                        }
                    }
                }
                else
                {
                    LoggerInstance.Msg("No saved items found");
                }
                LoggerInstance.Msg("Loaded inventory");
                BLHelper.SendNotification("Success", "Successfully loaded the inventory", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Error("An error occurred while loading the inventory", ex);
                BLHelper.SendNotification("Failure", "Failed to load the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
            }
        }

        /// <summary>
        /// Sets ammo from <see cref="CurrentSave"/>
        /// </summary>
        /// <param name="showNotifications">If notifications should be shown</param>
        public static void AddSavedAmmo(bool showNotifications = true)
        {
            if (!AmmoInventory.CurrentThreadIsMainThread())
            {
                Logger.Warning("Adding ammo not on the main thread, this may cause crashes due to protected memory");
            }
            var ammoInventory = GetAmmoInventory();
            if (ammoInventory == null)
            {
                Logger.Error("Ammo inventory is null");
                return;
            }
            ammoInventory.ClearAmmo();
            Logger.Msg($"Adding light ammo: {CurrentSave.LightAmmo}");
            ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, CurrentSave.LightAmmo);
            Logger.Msg($"Adding medium ammo: {CurrentSave.MediumAmmo}");
            ammoInventory.AddCartridge(ammoInventory.mediumAmmoGroup, CurrentSave.MediumAmmo);
            Logger.Msg($"Adding heavy ammo: {CurrentSave.HeavyAmmo}");
            ammoInventory.AddCartridge(ammoInventory.heavyAmmoGroup, CurrentSave.HeavyAmmo);
            if (!mp_itemsaving.Value)
            {
                Logger.Msg("Loaded inventory");
                if (showNotifications) BLHelper.SendNotification("Success", "Successfully loaded the inventory", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
            }
        }

        /// <summary>
        /// Loads the saved inventory from <see cref="CurrentSave"/>
        /// </summary>
        public void LoadSavedInventory()
        {
            if (HasFusion && IsConnected && FusionGamemodeCheck())
            {
                LoggerInstance.Warning("A gamemode is currently running, we cannot load your inventory!");
                return;
            }
            try
            {
                if (HasFusion && IsConnected && (!IsFusionLibraryInitialized || !mp_fusionSupport.Value))
                {
                    BLHelper.SendNotification("Failure", "Could not load inventory, because either the 'Fusion Support' setting is set to Disabled or the Fusion Support Library did not load correctly", true, 3.5f, BoneLib.Notifications.NotificationType.Error);
                    LoggerInstance.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly");
                    return;
                }
                LoggerInstance.Msg("Loading inventory...");
                if (mp_ammosaving.Value)
                {
                    // Adds saved ammo
                    LoggerInstance.Msg("Waiting for Ammo Inventory to be initialized");
                    var ammoInventory = GetAmmoInventory();
                    if (ammoInventory != null)
                    {
                        AddSavedAmmo(mp_showNotifications.Value);
                    }
                    else
                    {
                        Logger.Warning("Ammo Inventory is empty, awaiting");
                        LoadAmmoOnAwake = true;
                    }
                }

                if (HasFusion)
                {
                    // Spawns the saved items by sending messages to the Fusion server
                    LoggerInstance.Msg("Checking if client is connected to a Fusion server");
                    FusionSpawnSavedItems();
                }
                else
                {
                    if (mp_itemsaving.Value)
                    {
                        LoggerInstance.Msg("Spawning in slots saved items");
                        SpawnSavedItems(null);
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Error("An error occurred while loading the inventory", ex);
                BLHelper.SendNotification("Failure", "Failed to load the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
            }
        }

        #endregion Saving & Loading

        #region BoneLib Events

        /// <summary>
        /// Called when a BONELAB Level is unloaded<br/>
        /// Used now for saving inventory
        /// </summary>
        private void LevelUnloadedEvent()
        {
            if (!mp_saveOnLevelUnload.Value) return;
            var list = new List<string>(mp_blacklistedLevels.Value);
            if (mp_blacklistBONELABlevels.Value) list.AddRange(defaultBlacklistedLevels);
            if (!list.Contains(levelInfo.barcode))
            {
                SaveInventory();
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
                        new NotificationText($"There is a new version of KeepInventory. Go to Thunderstore and download the latest version which is {$"v{ThunderstorePackage.Latest.Version}".CreateUnityColor(System.Drawing.Color.LimeGreen)}", Color.white, true),
                        true,
                        5f,
                        NotificationType.Warning);
                }
                InitialLoad = false;
            }
            levelInfo = obj;
            var list = new List<string>(mp_blacklistedLevels.Value);
            if (mp_blacklistBONELABlevels.Value) list.AddRange(defaultBlacklistedLevels);
            if (!list.Contains(obj.barcode))
            {
                if (mp_loadOnLevelLoad.Value)
                {
                    if (CommonBarcodes.Maps.All.Contains(levelInfo.barcode) && DoesSaveForLevelExist(levelInfo.title))
                    {
                        if (mp_initialInventoryRemove.Value) RemoveInitialInventoryFromSave(levelInfo.title);
                    }
                    if (HasFusion && IsConnected && (!IsFusionLibraryInitialized || !mp_fusionSupport.Value))
                    {
                        LoggerInstance.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly");
                    }
                    else
                    {
                        try
                        {
                            statusElement.ElementName = "Current level is not blacklisted";
                            statusElement.ElementColor = Color.green;
                            LoadSavedInventory();
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

        #endregion BoneLib Events

        #region Setup

        /// <summary>
        /// Setup the BoneMenu
        /// </summary>
        private void SetupMenu()
        {
            var mainPage = BoneLib.BoneMenu.Page.Root.CreatePage("HAHOOS", Color.white);
            var modPage = mainPage.CreatePage("KeepInventory", Color.yellow);

            var savingPage = modPage.CreatePage("Saving", Color.cyan);
            savingPage.CreateBoolPref("Save Items", Color.white, ref mp_itemsaving, prefDefaultValue: true);
            savingPage.CreateBoolPref("Save Ammo", Color.white, ref mp_ammosaving, prefDefaultValue: true);
            savingPage.CreateBoolPref("Save Gun Data", Color.white, ref mp_saveGunData, prefDefaultValue: true);
            savingPage.CreateBoolPref("Persistent Save", Color.magenta, ref mp_persistentsave, (value) =>
            {
                if (value)
                {
                    SaveCategory = MelonPreferences.CreateCategory<Save>("KeepInventory_Save", "Keep Inventory Save");
                    SaveCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "Save.cfg"));
                    var _old = SaveCategory.GetValue<Save>();
                    _old = CurrentSave;
                    CurrentSave = _old;
                }
                else
                {
                    CurrentSave = new Save(CurrentSave);
                }
            }, prefDefaultValue: true);

            var eventsPage = modPage.CreatePage("Events", Color.yellow);
            eventsPage.CreateBoolPref("Save on Level Unload", Color.red, ref mp_saveOnLevelUnload, prefDefaultValue: true);
            eventsPage.CreateBoolPref("Load on Level Load", Color.green, ref mp_loadOnLevelLoad, prefDefaultValue: true);
            eventsPage.CreateBoolPref("Automatically Save To File", Color.magenta, ref mp_automaticallySaveToFile, (value) =>
            {
                if (value)
                {
                    SaveCategory = MelonPreferences.CreateCategory<Save>("KeepInventory_Save", "Keep Inventory Save");
                    SaveCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "Save.cfg"));
                    var _old = SaveCategory.GetValue<Save>();
                    _old = CurrentSave;
                    CurrentSave = _old;
                }
                else
                {
                    CurrentSave = new Save(CurrentSave);
                }
            }, prefDefaultValue: true);
            eventsPage.CreateFunction("Save Current Inventory", Color.white, () => SaveInventory(true));
            eventsPage.CreateFunction("Load Saved Inventory", Color.white, LoadSavedInventory);
            eventsPage.CreateFunction("Save The Current Inventory & Save To File", Color.white, () =>
            {
                try
                {
                    SaveInventory(true);
                    SavePreferences();
                    BLHelper.SendNotification("Success", "Successfully saved the current inventory to file!", true, 2f, BoneLib.Notifications.NotificationType.Success);
                }
                catch (Exception ex)
                {
                    LoggerInstance.Error($"An unexpected error has occurred while saving the current inventory to file:\n{ex}");
                    BLHelper.SendNotification("Failure", "Failed to save the current inventory to file, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
                }
            });
            eventsPage.CreateFunction("Save To File", Color.white, () =>
            {
                try
                {
                    //SaveInventory(true);
                    SavePreferences();
                    BLHelper.SendNotification("Success", "Successfully saved the current save to file!", true, 2f, BoneLib.Notifications.NotificationType.Success);
                }
                catch (Exception ex)
                {
                    LoggerInstance.Error($"An unexpected error has occurred while saving the current save to file:\n{ex}");
                    BLHelper.SendNotification("Failure", "Failed to save the current save to file, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
                }
            });

            var blacklistPage = modPage.CreatePage("Blacklist", Color.red);
            blacklistPage.CreateBoolPref("Blacklist BONELAB Levels", Color.cyan, ref mp_blacklistBONELABlevels, prefDefaultValue: true);
            statusElement = blacklistPage.CreateFunction("Blacklist Level from Saving/Loading", Color.red, () =>
            {
                if (defaultBlacklistedLevels.Contains(levelInfo.barcode)) return;
                List<string> blacklistList = mp_blacklistedLevels.Value;
                if (blacklistList.Contains(levelInfo.barcode))
                {
                    try
                    {
                        int item = blacklistList.IndexOf(levelInfo.barcode);
                        if (item != -1)
                        {
                            blacklistList.RemoveAt(item);
                            statusElement.ElementName = "Current Level is not blacklisted";
                            statusElement.ElementColor = Color.green;
                            BLHelper.SendNotification("Success", $"Successfully unblacklisted current level ({levelInfo.title}) from having the inventory saved and/or loaded!", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerInstance.Error($"An unexpected error has occurred while unblacklisting the current level\n{ex}");
                        BLHelper.SendNotification("Failure", "An unexpected error has occurred while unblacklisting the current level, check the console or logs for more details", true, 3f, BoneLib.Notifications.NotificationType.Error);
                    }
                }
                else
                {
                    try
                    {
                        blacklistList.Add(levelInfo.barcode);
                        statusElement.ElementName = "Current Level is blacklisted";
                        statusElement.ElementColor = Color.red;
                        BLHelper.SendNotification("Success", $"Successfully blacklisted current level ({levelInfo.title}) from having the inventory saved and/or loaded!", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
                    }
                    catch (Exception ex)
                    {
                        LoggerInstance.Error($"An unexpected error has occurred while removing the current level from blacklist\n{ex}");
                        BLHelper.SendNotification("Failure", "An unexpected error has occurred while blacklisting the current level, check the console or logs for more details", true, 3f, BoneLib.Notifications.NotificationType.Error);
                    }
                }
            });

            var otherPage = modPage.CreatePage("Other", Color.white);
            otherPage.CreateBoolPref("Show Notifications", Color.green, ref mp_showNotifications, prefDefaultValue: true);
            otherPage.CreateBoolPref("Fusion Support", Color.cyan, ref mp_fusionSupport, prefDefaultValue: true);
            otherPage.CreateBoolPref("Remove initial inventory from save", Color.red, ref mp_initialInventoryRemove, prefDefaultValue: true);

            var modVersion = modPage.CreateFunction(IsLatestVersion || ThunderstorePackage == null ? $"Current Version: v{Version}" : $"Current Version: v{Version}<br>{"(Update available!)".CreateUnityColor(System.Drawing.Color.LimeGreen)}", Color.white, () => LoggerInstance.Msg($"The current version is v{Version}!!!!"));
            modVersion.SetProperty(ElementProperties.NoBorder);
            modVersion.SetTooltip("Version of KeepInventory");
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
                description: "If true, will save data about guns stored in slots, info such as rounds left etc.");
            mp_persistentsave = PrefsCategory.CreateEntry<bool>("PersistentSave", true, "Persistent Save",
                description: "If true, will save and load inventory in a KeepInventory_Save.cfg file to be used between sessions");

            // Events

            mp_saveOnLevelUnload = PrefsCategory.CreateEntry<bool>("SaveOnLevelUnload", true, "Save On Level Unload",
                description: "If true, during level unload, the inventory will be automatically saved");
            mp_loadOnLevelLoad = PrefsCategory.CreateEntry<bool>("LoadOnLevelLoad", true, "Load On Level Load",
                description: "If true, the saved inventory will be automatically loaded when you get loaded into a level thats not blacklisted");
            mp_automaticallySaveToFile = PrefsCategory.CreateEntry<bool>("AutomaticallySaveToFile", true, "Automatically Save To File",
                description: "If true, the inventory will be automatically saved to a save file if 'Persistent Save' is turned on when the game is quitting");

            // Blacklist

            mp_blacklistBONELABlevels = PrefsCategory.CreateEntry<bool>("BlacklistBONELABLevels", true, "Blacklist BONELAB Levels",
                description: "If true, most of the BONELAB levels (except VoidG114 and BONELAB Hub) will be blacklisted from saving/loading inventory");
            mp_blacklistedLevels = PrefsCategory.CreateEntry<List<string>>("BlacklistedLevels", [], "Blacklisted Levels",
                description: "List of levels that will not save/load inventory");

            // Other

            mp_showNotifications = PrefsCategory.CreateEntry<bool>("ShowNotifications", true, "Show Notifications",
                description: "If true, notifications will be shown in-game regarding errors or other things");
            mp_fusionSupport = PrefsCategory.CreateEntry<bool>("FusionSupport", true, "Fusion Support",
                description: "If true, the mod will work with Fusion. If fusion is detected, you are connected to a server and this setting is turned off, the inventory will not be loaded");
            mp_configVersion = PrefsCategory.CreateEntry<int>("ConfigVersion", 1, "Config Version",
                description: "DO NOT CHANGE THIS AT ALL, THIS WILL BE USED FOR MIGRATING CONFIGS AND SHOULD NOT BE CHANGED AT ALL");
            mp_initialInventoryRemove = PrefsCategory.CreateEntry<bool>("RemoveInitialInventory", true, "Remove Initial Inventory",
                description: "If true, the mod will remove initial inventory found in save data in a loaded inventory");

            PrefsCategory.SaveToFile(false);

            if (mp_persistentsave.Value)
            {
                SaveCategory = MelonPreferences.CreateCategory<Save>("KeepInventory_Save", "Keep Inventory Save");
                SaveCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "Save.cfg"));
                SaveCategory.SaveToFile(false);
                CurrentSave = SaveCategory.GetValue<Save>();
            }
            else
            {
                CurrentSave = new Save();
            }
        }

        #endregion Setup

        #endregion Methods
    }
}