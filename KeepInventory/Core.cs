using MelonLoader;
using MelonLoader.Utils;
using MelonLoader.Preferences;

using BoneLib;
using BoneLib.BoneMenu;

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

        /// <summary>
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

        // Categories

        internal static MelonPreferences_Category PrefsCategory;

        internal static MelonPreferences_ReflectiveCategory SaveCategory;

        // Settings

        internal static MelonPreferences_Entry<bool> mp_itemsaving;
        internal static MelonPreferences_Entry<bool> mp_ammosaving;

        internal static MelonPreferences_Entry<bool> mp_saveOnLevelUnload;
        internal static MelonPreferences_Entry<bool> mp_loadOnLevelLoad;
        internal static MelonPreferences_Entry<bool> mp_automaticallySaveToFile;

        internal static MelonPreferences_Entry<bool> mp_saveGunData;

        internal static MelonPreferences_Entry<bool> mp_persistentsave;

        internal static MelonPreferences_Entry<List<string>> mp_blacklistedLevels;

        internal static MelonPreferences_Entry<bool> mp_blacklistBONELABlevels;

        internal static MelonPreferences_Entry<bool> mp_showNotifications;

        internal static MelonPreferences_Entry<bool> mp_fusionSupport;

        #endregion MelonPreferences

        /// <summary>
        /// Variable of element in BoneMenu responsible for showing if level is blacklisted or not, and changing it
        /// </summary>
        private FunctionElement statusElement;

        /// <summary>
        /// Current Level Info
        /// </summary>
        private LevelInfo levelInfo;

        /// <summary>
        /// Boolean value indicating if user has Fusion
        /// </summary>
        public static bool HasFusion { get; private set; }

        /// <summary>
        /// Boolean value indicating if user is connected to a server
        /// </summary>
        public static bool IsConnected
        { get { return Internal_IsConnected(); } set { } }

        /// <summary>
        /// Boolean value indicating whether or not was the Fusion Library for KeepInventory loaded/initialized
        /// </summary>
        public static bool IsFusionLibraryInitialized { get; private set; }

        /// <summary>
        /// Boolean value indicating whether or not Fusion Support Library failed to load
        /// </summary>
        public static bool FailedFLLoad { get; private set; }

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

            SetupPreferences();
            SetupMenu();

            HasFusion = HelperMethods.CheckIfAssemblyLoaded("labfusion");
            LoggerInstance.Msg(HasFusion ? "Found LabFusion" : "Could not find LabFusion, the mod will not use any of Fusion's functionality");

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

            if (IsFusionLibraryInitialized) LoadFusionLibrary();
        }

        #endregion MelonLoader

        #region Fusion

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void LoadFusionLibrary()
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

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static bool FusionLibraryIsConnected()
        {
            return Fusion.FusionMethods.LocalNetworkPlayer != null;
            //return LabFusion.Network.NetworkInfo.HasServer;
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static bool FusionIsConnected()
        {
            return LabFusion.Network.NetworkInfo.HasServer;
        }

        [System.Runtime.CompilerServices.MethodImpl(
System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void FusionSpawnInSlot(Barcode barcode, InventorySlotReceiver inventorySlotReceiver, string slotName, System.Drawing.Color slotColor)
        {
            Fusion.FusionMethods.NetworkSpawnInSlotAsync(inventorySlotReceiver, barcode, slotColor, slotName);
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void FusionSpawnInSlot(Barcode barcode, InventorySlotReceiver inventorySlotReceiver, string slotName, System.Drawing.Color slotColor, Action<GameObject> inBetween = null)
        {
            Fusion.FusionMethods.NetworkSpawnInSlotAsync(inventorySlotReceiver, barcode, slotColor, slotName, inBetween);
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static RigManager FusionFindRigManager()
        {
            return Fusion.FusionMethods.RigManager ?? Player.RigManager;
        }

        [System.Runtime.CompilerServices.MethodImpl(
   System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void RemoveRigCreateEvent()
        {
            if (HasFusion && IsConnected && IsFusionLibraryInitialized) Fusion.FusionMethods.OnRigCreated -= SpawnSavedItems;
        }

        [System.Runtime.CompilerServices.MethodImpl(
   System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static AmmoInventory FusionGetAmmoInventory()
        {
            if (HasFusion && IsFusionLibraryInitialized) return Fusion.FusionMethods.FindAmmoInventory() ?? AmmoInventory.Instance;
            else return AmmoInventory.Instance;
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void FusionSpawnSavedItems()
        {
            if (IsConnected)
            {
                LoggerInstance.Msg("Client is connected to a server");
                if (mp_itemsaving.Value)
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

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        internal static bool FusionIsGamemodeRunning()
        {
            return LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeStarted;
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

        internal static void MsgPrefix(string message, string prefix, System.Drawing.Color color)
        {
            Logger._MsgPastel($"[{prefix.Pastel(color)}] {message}");
        }

        private static bool Internal_IsConnected()
        {
            if (HasFusion && IsFusionLibraryInitialized) return FusionLibraryIsConnected();
            else if (HasFusion && !IsFusionLibraryInitialized) return FusionIsConnected();
            else return false;
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

        internal static RigManager FindRigManager()
        {
            if (HasFusion && IsConnected && IsFusionLibraryInitialized) return FusionFindRigManager();
            else return Player.RigManager;
        }

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
                                    if (gunInfo != null && mp_saveGunData.Value)
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

        private void SpawnSavedItems(RigManager _rigManager)
        {
            if (HasFusion && IsConnected && (!IsFusionLibraryInitialized || !mp_fusionSupport.Value))
            {
                LoggerInstance.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. To load inventory in Fusion servers, check if you have the Fusion Library in UserData > KeepInventory (there should be a file called 'KeepInventory.Fusion.dll') or try enabling 'Fusion Support' in settings");
                return;
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

                                if (item.Type == SaveSlot.SpawnableType.Gun && mp_saveGunData.Value)
                                {
                                    // Settings properties for the gun, this is horrible
                                    void action(GameObject obj)
                                    {
                                        if (item.GunInfo != null && obj != null)
                                        {
                                            var gun = obj.GetComponent<Gun>();
                                            MsgPrefix("Attempting to write GunInfo", item.SlotName, SlotColor);
                                            gun.UpdateProperties(item.GunInfo, SlotColor, item, crate.Crate.name, item.Barcode, true, false);
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

        internal static bool LoadAmmoOnAwake = false;

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
            if (HasFusion && IsConnected && FusionIsGamemodeRunning())
            {
                LoggerInstance.Warning("A gamemode is currently running, we cannot load your inventory!");
                return;
            }
            try
            {
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

        private bool InitialLoad = true;

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
                    BLHelper.SendNotification("Failure", "The Fusion Support Library has failed to load, meaning the fusion support for the mod will be disabled. If this will occur again, please report it to the developer (discord @hahoos)");
                    IsFusionLibraryInitialized = false;
                }
                InitialLoad = false;
            }
            levelInfo = obj;
            if (!mp_loadOnLevelLoad.Value) return;
            var list = new List<string>(mp_blacklistedLevels.Value);
            if (mp_blacklistBONELABlevels.Value) list.AddRange(defaultBlacklistedLevels);
            if (!list.Contains(obj.barcode))
            {
                if (HasFusion && IsConnected && (!IsFusionLibraryInitialized || !mp_fusionSupport.Value))
                {
                    LoggerInstance.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly");
                    return;
                }
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
        /// Setup the BoneMenu<br/>
        /// Makes it able to manage settings within the game
        /// </summary>
        private void SetupMenu()
        {
            var mainPage = BoneLib.BoneMenu.Page.Root.CreatePage("HAHOOS", Color.white);
            var modPage = mainPage.CreatePage("KeepInventory", new Color(255, 72, 59));

            var savingPage = modPage.CreatePage("Saving", Color.cyan);
            savingPage.CreateBoolPref("Save Items", Color.white, ref mp_itemsaving, prefDefaultValue: true);
            savingPage.CreateBoolPref("Save Ammo", Color.white, ref mp_ammosaving, prefDefaultValue: true);
            savingPage.CreateBoolPref("Save Gun Data", Color.white, ref mp_saveGunData, prefDefaultValue: true);
            savingPage.CreateBoolPref("Persistent Save", Color.magenta, ref mp_persistentsave, prefDefaultValue: true);

            var eventsPage = modPage.CreatePage("Events", Color.yellow);
            eventsPage.CreateBoolPref("Save on Level Unload", Color.red, ref mp_saveOnLevelUnload, prefDefaultValue: true);
            eventsPage.CreateBoolPref("Load on Level Load", Color.green, ref mp_loadOnLevelLoad, prefDefaultValue: true);
            eventsPage.CreateBoolPref("Automatically Save To File", Color.magenta, ref mp_automaticallySaveToFile, prefDefaultValue: true);
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
                    int item = blacklistList.IndexOf(levelInfo.barcode);
                    if (item != -1)
                    {
                        blacklistList.RemoveAt(item);
                        statusElement.ElementName = "Current Level is not blacklisted";
                        statusElement.ElementColor = Color.green;
                    }
                }
                else
                {
                    blacklistList.Add(levelInfo.barcode);
                    statusElement.ElementName = "Current Level is blacklisted";
                    statusElement.ElementColor = Color.red;
                }
            });

            var otherPage = modPage.CreatePage("Other", Color.white);
            otherPage.CreateBoolPref("Show Notifications", Color.green, ref mp_showNotifications, prefDefaultValue: true);
            otherPage.CreateBoolPref("Fusion Support", Color.cyan, ref mp_fusionSupport, prefDefaultValue: true);
        }

        /// <summary>
        /// Set up Preferences<br/>
        /// They are used to save settings so it can be used every time u play
        /// </summary>
        private void SetupPreferences()
        {
            if (!Directory.Exists(KI_PreferencesDirectory))
            {
                LoggerInstance.Msg("Creating prefs directory");
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

            // Events

            mp_saveOnLevelUnload = PrefsCategory.CreateEntry<bool>("SaveOnLevelUnload", true, "Save On Level Unload",
                description: "If true, during level unload, the inventory will be automatically saved");
            mp_loadOnLevelLoad = PrefsCategory.CreateEntry<bool>("LoadOnLevelLoad", true, "Load On Level Load",
                description: "If true, the saved inventory will be automatically loaded when you get loaded into a level thats not blacklisted");
            mp_automaticallySaveToFile = PrefsCategory.CreateEntry<bool>("AutomaticallySaveToFile", true, "Automatically Save To File",
                description: "If true, the inventory will be automatically saved to a save file if 'Persistent Save' is turned on when the game is quitting");

            // Other

            mp_persistentsave = PrefsCategory.CreateEntry<bool>("PersistentSave", true, "Persistent Save",
                description: "If true, will save and load inventory in a KeepInventory_Save.cfg file to be used between sessions");
            mp_blacklistBONELABlevels = PrefsCategory.CreateEntry<bool>("BlacklistBONELABLevels", true, "Blacklist BONELAB Levels",
                description: "If true, most of the BONELAB levels (except VoidG114 and BONELAB Hub) will be blacklisted from saving/loading inventory");
            mp_blacklistedLevels = PrefsCategory.CreateEntry<List<string>>("BlacklistedLevels", [], "Blacklisted Levels",
                description: "List of levels that will not save/load inventory");
            mp_showNotifications = PrefsCategory.CreateEntry<bool>("ShowNotifications", true, "Show Notifications",
                description: "If true, notifications will be shown in-game regarding errors or other things");
            mp_fusionSupport = PrefsCategory.CreateEntry<bool>("FusionSupport", true, "Fusion Support",
                description: "If true, the mod will work with Fusion. If fusion is detected, you are connected to a server and this setting is turned off, the inventory will not be loaded");

            PrefsCategory.SaveToFile(false);

            SaveCategory = MelonPreferences.CreateCategory<Save>("KeepInventory_Save", "Keep Inventory Save");
            SaveCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "Save.cfg"));
            SaveCategory.SaveToFile(false);
            CurrentSave = mp_persistentsave.Value ? SaveCategory.GetValue<Save>() : new Save();
        }

        #endregion Setup

        #endregion Methods
    }
}