using MelonLoader;
using BoneLib;
using BoneLib.BoneMenu;
using UnityEngine;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow;
using MelonLoader.Utils;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using MelonLoader.Preferences;
using KeepInventory.Helper;
using Il2CppSLZ.Marrow.Utilities;
using KeepInventory.SaveSlot;
using System;

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

        /// <summary>
        /// Path to the preferences directory of KeepInventory
        /// </summary>
        public readonly static string KI_PreferencesDirectory = Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory");

        #region MelonPreferences

        private bool quitting;

        internal static MelonLogger.Instance Logger { get; private set; }

        // Categories

        internal static MelonPreferences_Category PrefsCategory;

        internal static MelonPreferences_ReflectiveCategory SaveCategory;

        // Settings

        internal static MelonPreferences_Entry<bool> mp_itemsaving;
        internal static MelonPreferences_Entry<bool> mp_ammosaving;

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
        public static bool IsConnected { get; private set; }

        /// <summary>
        /// Calls when MelonLoader loads all Mods/Plugins
        /// </summary>
        public override void OnInitializeMelon()
        {
            Logger = LoggerInstance;
            LoggerInstance.Msg("Setting up KeepInventory");
            SetupPreferences();
            SetupMenu();

            HasFusion = HelperMethods.CheckIfAssemblyLoaded("labfusion");
            LoggerInstance.Msg(HasFusion ? "Found LabFusion" : "Could not find LabFusion, the mod will not use any of Fusion's functionality");

            if (HasFusion && mp_fusionSupport.Value)
            {
                LoggerInstance.Msg("Checking for Fusion Support Library");
                string path = Path.Combine(KI_PreferencesDirectory, "KeepInventory.Fusion.dll");
                if (File.Exists(path))
                {
                    LoggerInstance.Msg("Found library, loading");
                    try
                    {
                        System.Reflection.Assembly.LoadFile(path);
                        LoggerInstance.Msg("Loaded library");
                    }
                    catch (Exception ex)
                    {
                        LoggerInstance.Error($"An unexpected error occurred while loading the library:\n{ex}");
                    }
                }
                else
                {
                    LoggerInstance.Warning("Could not find library, even tho LabFusion is present in the game, KeepInventory will not work with it");
                    LoggerInstance.Warning("Note that without the Fusion Support Library, this mod might cause crashes or bugs when playing in servers, it is recommended that you install it");
                    HasFusion = false;
                    IsConnected = false;
                }
            }

            Hooking.OnLevelLoaded += LevelLoadedEvent;
            Hooking.OnLevelUnloaded += LevelUnloadedEvent;

            if (HasFusion && mp_fusionSupport.Value) LoadFusionLibrary();
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void LoadFusionLibrary()
        {
            Fusion.FusionMethods.LoadModule();
            IsConnected = Fusion.FusionMethods.IsConnected;
        }

        /// <summary>
        /// Runs when application is about to quit
        /// </summary>
        public override void OnApplicationQuit()
        {
            SavePreferences();
        }

        /// <summary>
        /// Triggers when application is being quit <br/>
        /// Used to save inventory if PersistentSave is turned on<br/>
        /// <b>Will not trigger when trying to close MelonLoader, rather than the game</b>
        /// </summary>
        public void SavePreferences()
        {
            quitting = true;
            if (mp_persistentsave.Value)
            {
                LoggerInstance.Msg("Saving Preferences");
                SaveCategory.SaveToFile();
                LoggerInstance.Msg("Saved Preferences successfully");
            }
        }

        /// <summary>
        /// Called when a BONELAB Level is unloaded<br/>
        /// Used now for saving inventory
        /// </summary>
        private void LevelUnloadedEvent()
        {
            if (quitting) return;
            var list = new List<string>(mp_blacklistedLevels.Value);
            if (mp_blacklistBONELABlevels.Value) list.AddRange(defaultBlacklistedLevels);
            if (!list.Contains(levelInfo.barcode))
            {
                CurrentSave.InventorySlots?.Clear();
                LoggerInstance.Msg("Saving inventory...");
                var rigManager = FindRigManager();
                if (rigManager == null)
                {
                    LoggerInstance.Error("RigManager does not exist");
                    return;
                }
                if (rigManager.inventory == null)
                {
                    LoggerInstance.Error("Inventory does not exist");
                    return;
                }
                if (rigManager.inventory.bodySlots == null)
                {
                    LoggerInstance.Error("Body slots do not exist");
                    return;
                }
                if (mp_itemsaving.Value)
                {
                    foreach (var item in rigManager.inventory.bodySlots)
                    {
                        if (item == null || item.inventorySlotReceiver == null || item.inventorySlotReceiver?._weaponHost == null || item.inventorySlotReceiver?._weaponHost.GetTransform() == null) continue;
                        if (item.inventorySlotReceiver?._weaponHost != null)
                        {
                            var gun = item.inventorySlotReceiver._weaponHost.GetTransform().GetComponent<Gun>();
                            GunInfo gunInfo = null;
                            if (gun != null)
                            {
                                gunInfo = GunInfo.Parse(gun);
                            }
                            var poolee = item.inventorySlotReceiver._weaponHost.GetTransform().GetComponent<Poolee>();
                            if (poolee != null)
                            {
                                var barcode = poolee.SpawnableCrate.Barcode;
                                LoggerInstance.Msg($"Slot: {item.name} / Barcode: {poolee.SpawnableCrate.name} ({poolee.SpawnableCrate.Barcode.ID})");
                                if (gunInfo != null && mp_saveGunData.Value)
                                {
                                    CurrentSave.InventorySlots.Add(new SaveSlot.SaveSlot(item.name, barcode, gunInfo));
                                }
                                else
                                {
                                    CurrentSave.InventorySlots.Add(new SaveSlot.SaveSlot(item.name, barcode));
                                }
                            }
                        }
                    }
                    if (CurrentSave.InventorySlots.Count == 0)
                    {
                        LoggerInstance.Msg("No spawnables were found in slots");
                    }
                }
                if (mp_ammosaving.Value)
                {
                    var ammoInventory = AmmoInventory.Instance;
                    CurrentSave.LightAmmo = ammoInventory.GetCartridgeCount("light");
                    LoggerInstance.Msg("Saved Light Ammo: " + CurrentSave.LightAmmo);
                    CurrentSave.MediumAmmo = ammoInventory.GetCartridgeCount("medium");
                    LoggerInstance.Msg("Saved Medium Ammo: " + CurrentSave.MediumAmmo);
                    CurrentSave.HeavyAmmo = ammoInventory.GetCartridgeCount("heavy");
                    LoggerInstance.Msg("Saved Heavy Ammo: " + CurrentSave.LightAmmo);
                }
                LoggerInstance.Msg("Successfully saved inventory");
            }
            else
            {
                LoggerInstance.Msg("Not saving due to the level being blacklisted");
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static GameObject FusionSpawnInSlot(Barcode barcode, InventorySlotReceiver inventorySlotReceiver)
        {
            var result = Fusion.FusionMethods.NetworkSpawnInSlotAsync(inventorySlotReceiver, barcode);
            result.Wait();
            if (result.Result == null)
            {
                return null;
            }
            else
            {
                if (result.Result.HasValue && result.Result.Value.spawned != null)
                {
                    return result.Result.Value.spawned;
                }
                else
                {
                    return null;
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static RigManager FusionFindRigManager()
        {
            return Fusion.FusionMethods.GetRigManager();
        }

        private static RigManager FindRigManager()
        {
            if (HasFusion && IsConnected) return FusionFindRigManager();
            else return Player.RigManager;
        }

        private void SpawnSavedItems()
        {
            try
            {
                if (CurrentSave.InventorySlots.Count >= 1)
                {
                    var rigManager = FindRigManager();
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
                    var list = rigManager.inventory.bodySlots.ToList();
                    foreach (var item in CurrentSave.InventorySlots)
                    {
                        LoggerInstance.Msg($"[{item.SlotName}] Looking for slot");
                        // Check for a slot with the same name and one that is for spawnables, not ammo
                        var slot = list.Find((slot) => slot.name == item.SlotName && slot.inventorySlotReceiver != null);
                        if (slot != null)
                        {
                            LoggerInstance.Msg($"[{item.SlotName}] Found slot");
                            var receiver = slot.inventorySlotReceiver;
                            receiver.DespawnContents();
                            if (MarrowGame.assetWarehouse.HasCrate(new Barcode(item.Barcode)))
                            {
                                LoggerInstance.Msg($"[{item.SlotName}] Mod containing the spawnable is installed");
                                // There was an issue with items not loading models in slots, i cant replicate the issue, but adding this to be sure
                                var crate = new SpawnableCrateReference(item.Barcode);
                                crate?.Crate.PreloadAssets();

                                if (item.Type == SaveSlot.SaveSlot.SpawnableType.Gun && mp_saveGunData.Value)
                                {
                                    LoggerInstance.Msg($"[{item.SlotName}] Attempting to load in slot '{item.SlotName}': {crate.Crate.name} ({item.Barcode})");
                                    LoggerInstance.Msg($"[{item.SlotName}] ^^^ Ammo: {item.GunInfo.RoundsLeft} / Has Mag: {item.GunInfo.IsMag} / Has bullet in chamber: {item.GunInfo.IsBulletInChamber} / Fire Mode: {item.GunInfo.FireMode} / Slide State: {item.GunInfo.SlideState} / Hammer state: {item.GunInfo.HammerState}");

                                    // Settings properties for the gun, this is horrible
                                    void action(GameObject obj)
                                    {
                                        if (item.GunInfo != null && obj != null)
                                        {
                                            var gun = obj.GetComponent<Gun>();
                                            LoggerInstance.Msg($"[{item.SlotName}] Attempting to write GunInfo");
                                            gun.UpdateProperties(item.GunInfo, item, crate.Crate.name, item.Barcode, true, false);
                                        }
                                    }

                                    LoggerInstance.Msg($"[{item.SlotName}] Spawning to slot: {crate.Crate.name} ({item.Barcode})");
                                    if (HasFusion && IsConnected)
                                    {
                                        var result = FusionSpawnInSlot(crate.Crate.Barcode, receiver);
                                        action(result);
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
                                    LoggerInstance.Msg($"[{item.SlotName}] Spawning to slot: {crate.Crate.name} ({item.Barcode})");
                                    if (HasFusion && IsConnected)
                                    {
                                        FusionSpawnInSlot(crate.Crate.Barcode, receiver);
                                        LoggerInstance.Msg($"[{item.SlotName}] Spawned to slot: {crate.Crate.name} ({item.Barcode})");
                                    }
                                    else
                                    {
                                        var task = receiver.SpawnInSlotAsync(crate.Crate.Barcode);
                                        Action complete = () => LoggerInstance.Msg($"[{item.SlotName}] Spawned to slot: {crate.Crate.name} ({item.Barcode})");
                                        task.GetAwaiter().OnCompleted(complete);
                                    }
                                }
                            }
                            else
                            {
                                LoggerInstance.Warning($"[{item.SlotName}] Could not find crate with the following barcode: {item.Barcode}");
                            }
                        }
                        else
                        {
                            LoggerInstance.Warning($"[{item.SlotName}] No slot was found with the provided name. It is possible that an avatar that was used during the saving had more slots than the current one");
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

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void FusionFunction()
        {
            if (Fusion.FusionMethods.IsConnected)
            {
                LoggerInstance.Msg("Client is connected to a server");
                /*
                foreach (var value in CurrentSave.InventorySlots)
                    RequestItemSpawn(value.Barcode);*/
                // Testing if I can just do this, seems to work, although I think there were some errors
                if (mp_itemsaving.Value && mp_fusionSupport.Value)
                {
                    SpawnSavedItems();
                }
            }
            else
            {
                LoggerInstance.Msg("Client is not connected to a server, spawning locally");
                if (mp_itemsaving.Value)
                {
                    SpawnSavedItems();
                }
            }
        }

        private void AddAmmo()
        {
            var ammoInventory = AmmoInventory.Instance;
            ammoInventory.ClearAmmo();
            LoggerInstance.Msg($"Adding light ammo: {CurrentSave.LightAmmo}");
            ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, CurrentSave.LightAmmo);
            LoggerInstance.Msg($"Adding medium ammo: {CurrentSave.MediumAmmo}");
            ammoInventory.AddCartridge(ammoInventory.mediumAmmoGroup, CurrentSave.MediumAmmo);
            LoggerInstance.Msg($"Adding heavy ammo: {CurrentSave.HeavyAmmo}");
            ammoInventory.AddCartridge(ammoInventory.heavyAmmoGroup, CurrentSave.HeavyAmmo);
            if (!mp_itemsaving.Value)
            {
                LoggerInstance.Msg("Loaded inventory");
                BLHelper.SendNotification("Success", "Successfully loaded the inventory", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
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
            var list = new List<string>(mp_blacklistedLevels.Value);
            if (mp_blacklistBONELABlevels.Value) list.AddRange(defaultBlacklistedLevels);
            if (!list.Contains(obj.barcode))
            {
                try
                {
                    statusElement.ElementName = "Current level is not blacklisted";
                    statusElement.ElementColor = Color.green;
                    LoggerInstance.Msg("Loading inventory...");
                    if (mp_ammosaving.Value)
                    {
                        // Adds saved ammo
                        LoggerInstance.Msg("Waiting for Ammo Inventory to be initialized");
                        Task.Run(async () =>
                        {
                            while (AmmoInventory.Instance == null) await Task.Delay(50); // Waits till AmmoInventory is not null
                            AddAmmo();
                        });
                    }

                    if (HasFusion)
                    {
                        // Spawns the saved items by sending messages to the Fusion server
                        LoggerInstance.Msg("Checking if client is connected to a Fusion server");
                        FusionFunction();
                    }
                    else
                    {
                        if (mp_itemsaving.Value)
                        {
                            LoggerInstance.Msg("Spawning in slots saved items");
                            SpawnSavedItems();
                        }
                    }
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

        /// <summary>
        /// Setup the BoneMenu<br/>
        /// Makes it able to manage settings within the game
        /// </summary>
        private void SetupMenu()
        {
            var mainPage = BoneLib.BoneMenu.Page.Root.CreatePage("HAHOOS", Color.white);
            var modPage = mainPage.CreatePage("KeepInventory", new Color(255, 72, 59));
            modPage.CreateBoolPref("Save Items", Color.white, ref mp_itemsaving, prefDefaultValue: true);
            modPage.CreateBoolPref("Save Ammo", Color.white, ref mp_ammosaving, prefDefaultValue: true);
            modPage.CreateBoolPref("Save Gun Data", Color.white, ref mp_saveGunData, prefDefaultValue: true);
            modPage.CreateBoolPref("Persistent Save", Color.magenta, ref mp_persistentsave, prefDefaultValue: true);
            modPage.CreateBoolPref("Blacklist BONELAB Levels", Color.cyan, ref mp_blacklistBONELABlevels, prefDefaultValue: true);
            modPage.CreateBoolPref("Show Notifications", Color.green, ref mp_showNotifications, prefDefaultValue: true);
            modPage.CreateBoolPref("Fusion Support", Color.cyan, ref mp_fusionSupport, prefDefaultValue: true);
            statusElement = modPage.CreateFunction("Blacklist Level from Saving/Loading", Color.red, () =>
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
            PrefsCategory = MelonPreferences.CreateCategory("HAHOOS_KeepInventory_Settings");
            PrefsCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "KeepInventory.cfg"));

            mp_itemsaving = PrefsCategory.CreateEntry<bool>("ItemSaving", true, "Item Saving",
                description: "If true, will save and load items in inventory");
            mp_ammosaving = PrefsCategory.CreateEntry<bool>("AmmoSaving", true, "Ammo Saving",
                description: "If true, will save and load ammo in inventory");
            mp_saveGunData = PrefsCategory.CreateEntry<bool>("SaveGunData", true, "Save Gun Data",
                description: "If true, will save data about guns stored in slots, info such as rounds left etc.");
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
            SaveCategory.SetFilePath(Path.Combine(KI_PreferencesDirectory, "KeepInventory_Save.cfg"));
            SaveCategory.SaveToFile(false);
            CurrentSave = mp_persistentsave.Value ? SaveCategory.GetValue<Save>() : new Save();
        }
    }
}