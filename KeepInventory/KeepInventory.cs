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
using System;
using Il2CppSLZ.Marrow.Utilities;
using KeepInventory.SaveSlot;
using Il2CppSystem;

namespace KeepInventory
{
    public class KeepInventory : MelonMod
    {
        public const string Version = "1.2.0";

        public Save CurrentSave { get; internal set; }

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

        #region MelonPreferences

        // Categories

        internal static MelonPreferences_Category PrefsCategory;

        internal static MelonPreferences_ReflectiveCategory SaveCategory;

        // Settings

        internal static MelonPreferences_Entry<bool> mp_itemsaving;
        internal static MelonPreferences_Entry<bool> mp_ammosaving;

        internal static MelonPreferences_Entry<bool> mp_persistentsave;

        internal static MelonPreferences_Entry<List<string>> mp_blacklistedLevels;

        internal static MelonPreferences_Entry<bool> mp_blacklistBONELABlevels;

        internal static MelonPreferences_Entry<bool> mp_showNotifications;

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
        private bool HasFusion;

        /// <summary>
        /// Calls when MelonLoader loads all Mods/Plugins
        /// </summary>
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Setting up KeepInventory");
            SetupPreferences();
            SetupMenu();

            HasFusion = HelperMethods.CheckIfAssemblyLoaded("labfusion");

            // Well fuck now I have to update for LemonLoader
            Application.quitting += (Il2CppSystem.Action)OnQuit; // Currently does not support Android, will be fixed when LemonLoader is updated
            Hooking.OnLevelLoaded += LevelLoadedEvent;
            Hooking.OnLevelUnloaded += LevelUnloadedEvent;
        }

        /// <summary>
        /// Triggers when application is being quit <br/>
        /// Used to save inventory if PersistentSave is turned on<br/>
        /// <b>Will not trigger when trying to close MelonLoader, rather than the game</b>
        /// </summary>
        public void OnQuit()
        {
            if ((bool)mp_persistentsave.BoxedValue)
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
            var list = new List<string>(mp_blacklistedLevels.BoxedValue as List<string>);
            if ((bool)mp_blacklistBONELABlevels.BoxedValue) list.AddRange(defaultBlacklistedLevels);
            if (!list.Contains(levelInfo.barcode))
            {
                CurrentSave.InventorySlots.Clear();
                LoggerInstance.Msg("Saving inventory...");
                if (Player.RigManager == null)
                {
                    LoggerInstance.Error("RigManager does not exist");
                    return;
                }
                if (Player.RigManager.inventory == null)
                {
                    LoggerInstance.Error("Inventory does not exist");
                    return;
                }
                if (Player.RigManager.inventory.bodySlots == null)
                {
                    LoggerInstance.Error("Body slots do not exist");
                    return;
                }
                if ((bool)mp_itemsaving.BoxedValue)
                {
                    foreach (var item in Player.RigManager.inventory.bodySlots)
                    {
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
                                LoggerInstance.Msg($"Slot: {item.name} / Barcode: {poolee.SpawnableCrate.name} {poolee.SpawnableCrate.Barcode.ID}");
                                if (gunInfo != null)
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
                if ((bool)mp_ammosaving.BoxedValue)
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

        private void SpawnSavedItems()
        {
            if (CurrentSave.InventorySlots.Count >= 1)
            {
                // Adds saved items to inventory slots
                var list = Player.RigManager.inventory.bodySlots.ToList();
                foreach (var item in CurrentSave.InventorySlots)
                {
                    // Check for a slot with the same name and one that is for spawnables, not ammo
                    var slot = list.Find((slot) => slot.name == item.SlotName && slot.inventorySlotReceiver != null);
                    if (slot != null)
                    {
                        var receiver = slot.inventorySlotReceiver;
                        receiver.DespawnContents();
                        if (MarrowGame.assetWarehouse.HasCrate(new Barcode(item.Barcode)))
                        {
                            // There was an issue with items not loading models in slots, i cant replicate the issue, but adding this to be sure
                            var crate = new SpawnableCrateReference(item.Barcode);
                            crate?.Crate.PreloadAssets();

                            if (item.Type == SaveSlot.SaveSlot.SpawnableType.Gun)
                            {
                                LoggerInstance.Msg($"Attempting to load in slot '{item.SlotName}': {crate.Crate.name} ({item.Barcode})");
                                // Settings properties for the gun, this is horrible
                                System.Action action = () =>
                                {
                                    if (item.GunInfo != null)
                                    {
                                        var gun = receiver._weaponHost.GetTransform().GetComponent<Gun>();
                                        if (gun != null)
                                        {
                                            LoggerInstance.Msg("BIC");
                                            if (item.GunInfo.IsBulletInChamber) gun.SpawnCartridge(gun.defaultCartridge.cartridgeSpawnable);
                                            LoggerInstance.Msg("IM");
                                            if (item.GunInfo.IsMag && item.GunInfo.MagazineState != null)
                                            {
                                                // For some reason it does not take rounds into consideration
                                                gun.ammoSocket.ForceLoadAsync(item.GunInfo.GetMagazineData(gun));

                                                gun.MagazineState.magazineData.rounds = item.GunInfo.MagazineState.Count;
                                                while (gun.MagazineState.magazineData.rounds > item.GunInfo.MagazineState.Count) gun.EjectCartridge();
                                            }
                                            LoggerInstance.Msg("FM");
                                            gun.fireMode = item.GunInfo.FireMode;
                                            LoggerInstance.Msg("SS");
                                            gun.slideState = item.GunInfo.SlideState;
                                            LoggerInstance.Msg("HS");
                                            gun.hammerState = item.GunInfo.HammerState;
                                        }
                                    }
                                };

                                LoggerInstance.Msg($"Spawning to slot '{item.SlotName}': {crate.Crate.name} ({item.Barcode})");
                                var task = receiver.SpawnInSlotAsync(crate.Barcode);
                                var awaiter = task.GetAwaiter();
                                awaiter.OnCompleted(action);
                            }
                            else
                            {
                                LoggerInstance.Msg($"Spawning to slot '{item.SlotName}': {crate.Crate.name} ({item.Barcode})");
                                receiver.SpawnInSlotAsync(crate.Barcode);
                            }
                        }
                        else
                        {
                            LoggerInstance.Warning($"Could not find crate with the following barcode: {item.Barcode}");
                        }
                    }
                    else
                    {
                        LoggerInstance.Warning($"No slot with the name '{item.SlotName}' was found. It is possible that an avatar that was used during the saving had more slots than the current one");
                    }
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void RequestItemSpawn(string barcode)
        {
            var crate = new SpawnableCrateReference(barcode);
            LoggerInstance.Msg($"Sending request to spawn item in front of player: {crate.Crate.name} ({barcode})");
            LabFusion.Utilities.PooleeUtilities.RequestSpawn(
                                    barcode, new LabFusion.Data.SerializedTransform
                                    (Player.Head.position + (Player.Head.forward * 1.5f),
                                     Quaternion.identity), LabFusion.Player.PlayerIdManager.LocalSmallId);
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void FusionFunction()
        {
            if (LabFusion.Network.NetworkInfo.HasServer)
            {
                LoggerInstance.Msg("Client is connected to a server");
                foreach (var value in CurrentSave.InventorySlots)
                    RequestItemSpawn(value.Barcode);
            }
            else
            {
                LoggerInstance.Msg("Client is not connected to a server, spawning locally");
                if ((bool)mp_itemsaving.BoxedValue)
                {
                    SpawnSavedItems();
                }
            }
        }

        /// <summary>
        /// Called when a BONELAB Level is loaded<br/>
        /// Mostly used to load the inventory as of now
        /// </summary>
        /// <param name="obj">Contains Level Information</param>
        private async void LevelLoadedEvent(LevelInfo obj)
        {
            levelInfo = obj;
            var list = new List<string>(mp_blacklistedLevels.BoxedValue as List<string>);
            if ((bool)mp_blacklistBONELABlevels.BoxedValue) list.AddRange(defaultBlacklistedLevels);
            if (!list.Contains(obj.barcode))
            {
                try
                {
                    statusElement.ElementName = "Current level is not blacklisted";
                    statusElement.ElementColor = Color.green;
                    LoggerInstance.Msg("Loading inventory...");
                    if (HasFusion)
                    {
                        // Spawns the saved items by sending messages to the Fusion server
                        LoggerInstance.Msg("Checking if client is connected to a Fusion server");
                        FusionFunction();
                    }
                    else
                    {
                        if ((bool)mp_itemsaving.BoxedValue)
                        {
                            LoggerInstance.Msg("Spawning in slots saved items");
                            SpawnSavedItems();
                        }
                    }
                    if ((bool)mp_ammosaving.BoxedValue)
                    {
                        // Adds saved ammo
                        LoggerInstance.Msg("Waiting for Ammo Inventory to be initialized");
                        while (AmmoInventory.Instance == null) await Task.Delay(50); // Waits till AmmoInventory is not null
                        var ammoInventory = AmmoInventory.Instance;
                        ammoInventory.ClearAmmo();
                        LoggerInstance.Msg($"Adding light ammo: {CurrentSave.LightAmmo}");
                        ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, CurrentSave.LightAmmo);
                        LoggerInstance.Msg($"Adding medium ammo: {CurrentSave.MediumAmmo}");
                        ammoInventory.AddCartridge(ammoInventory.mediumAmmoGroup, CurrentSave.MediumAmmo);
                        LoggerInstance.Msg($"Adding heavy ammo: {CurrentSave.HeavyAmmo}");
                        ammoInventory.AddCartridge(ammoInventory.heavyAmmoGroup, CurrentSave.HeavyAmmo);
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
            modPage.CreateBoolPref("Persistent Save", Color.magenta, ref mp_persistentsave, prefDefaultValue: true);
            modPage.CreateBoolPref("Blacklist BONELAB Levels", Color.cyan, ref mp_blacklistBONELABlevels, prefDefaultValue: true);
            modPage.CreateBoolPref("Show Notifications", Color.green, ref mp_showNotifications, prefDefaultValue: true);
            statusElement = modPage.CreateFunction("Blacklist Level from Saving/Loading", Color.red, () =>
            {
                if (defaultBlacklistedLevels.Contains(levelInfo.barcode)) return;
                List<string> blacklistList = (List<string>)mp_blacklistedLevels.BoxedValue;
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
            PrefsCategory = MelonPreferences.CreateCategory("HAHOOS_KeepInventory_Settings");
            PrefsCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory.cfg"));

            mp_itemsaving = PrefsCategory.CreateEntry<bool>("ItemSaving", true, "Item Saving",
                description: "If true, will save and load items in inventory");
            mp_ammosaving = PrefsCategory.CreateEntry<bool>("AmmoSaving", true, "Ammo Saving",
                description: "If true, will save and load ammo in inventory");
            mp_persistentsave = PrefsCategory.CreateEntry<bool>("PersistentSave", true, "Persistent Save",
                description: "If true, will save and load inventory in a KeepInventory_Save.cfg file to be used between sessions");
            mp_blacklistBONELABlevels = PrefsCategory.CreateEntry<bool>("BlacklistBONELABLevels", true, "Blacklist BONELAB Levels",
                description: "If true, most of the BONELAB levels (except VoidG114 and BONELAB Hub) will be blacklisted from saving/loading inventory");
            mp_blacklistedLevels = PrefsCategory.CreateEntry<List<string>>("BlacklistedLevels", [], "Blacklisted Levels",
                description: "List of levels that will not save/load inventory");
            mp_showNotifications = PrefsCategory.CreateEntry<bool>("ShowNotifications", true, "Show Notifications",
                description: "If true, notifications will be shown in-game regarding errors or other things");

            PrefsCategory.SaveToFile(false);

            SaveCategory = MelonPreferences.CreateCategory<Save>("KeepInventory_Save", "Keep Inventory Save");
            SaveCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory_Save.cfg"));
            SaveCategory.SaveToFile(false);
            CurrentSave = (bool)mp_persistentsave.BoxedValue ? SaveCategory.GetValue<Save>() : new Save();
        }
    }
}