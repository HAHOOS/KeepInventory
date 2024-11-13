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

namespace KeepInventory
{
    public class KeepInventory : MelonMod
    {
        public const string Version = "1.2.0";

        public Dictionary<string, Barcode> Slots { get; } = [];

        public Save CurrentSave;

        private readonly static List<string> defaultBlacklistedLevels = [
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

        private MelonPreferences_Entry<bool> mp_itemsaving;
        private MelonPreferences_Entry<bool> mp_ammosaving;

        private MelonPreferences_Entry<bool> mp_persistentsave;

        private MelonPreferences_Entry<List<string>> mp_blacklistedLevels;

        private MelonPreferences_Entry<bool> mp_blacklistBONELABlevels;

        #endregion MelonPreferences

        /// <summary>
        /// Variable of element in BoneMenu responsible for showing if level is blacklisted or not
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
                Dictionary<string, string> slots_id = [];
                foreach (KeyValuePair<string, Barcode> item in Slots) slots_id.Add(item.Key, item.Value.ID);
                CurrentSave.InventorySlots = slots_id;
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
            if (!((List<string>)mp_blacklistedLevels.BoxedValue).Contains(levelInfo.barcode))
            {
                Slots.Clear();
                LoggerInstance.Msg("Saving inventory...");
                if (Player.RigManager == null)
                {
                    LoggerInstance.Msg("RigManager does not exist");
                    return;
                }
                if (Player.RigManager.inventory == null)
                {
                    LoggerInstance.Msg("Inventory does not exist");
                    return;
                }
                if ((bool)mp_itemsaving.BoxedValue)
                {
                    foreach (var item in Player.RigManager.inventory.bodySlots)
                    {
                        if (item.inventorySlotReceiver != null)
                        {
                            if (item.inventorySlotReceiver._weaponHost != null)
                            {
                                var poolee = item.inventorySlotReceiver._weaponHost.GetTransform().GetComponent<Poolee>();
                                if (poolee != null)
                                {
                                    var barcode = poolee.SpawnableCrate.Barcode;
                                    Slots.Add(item.name, barcode);
                                }
                            }
                        }
                    }
                    Dictionary<string, string> slots_id = [];
                    foreach (KeyValuePair<string, Barcode> item in Slots) slots_id.Add(item.Key, item.Value.ID);
                    CurrentSave.InventorySlots = slots_id;
                }
                if ((bool)mp_ammosaving.BoxedValue)
                {
                    var ammoInventory = AmmoInventory.Instance;
                    CurrentSave.LightAmmo = ammoInventory.GetCartridgeCount("light");
                    CurrentSave.MediumAmmo = ammoInventory.GetCartridgeCount("medium");
                    CurrentSave.HeavyAmmo = ammoInventory.GetCartridgeCount("heavy");
                }
                LoggerInstance.Msg("Successfully saved inventory");
            }
        }

        private void SpawnSavedItems()
        {
            if (Slots.Count >= 1)
            {
                // Adds saved items to inventory slots
                var list = Player.RigManager.inventory.bodySlots.ToList();
                foreach (KeyValuePair<string, Barcode> item in Slots)
                {
                    var slot = list.Find((slot) => slot.name == item.Key);
                    if (slot != null)
                    {
                        var receiver = slot.inventorySlotReceiver;
                        if (receiver != null)
                        {
                            receiver.DestroyContents();
                            receiver.SpawnInSlotAsync(item.Value);
                        }
                    }
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void RequestItemSpawn(string barcode)
        {
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
                foreach (KeyValuePair<string, Barcode> value in Slots)
                    RequestItemSpawn(value.Value.ID);
            }
            else
            {
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
                        FusionFunction();
                    }
                    else
                    {
                        if ((bool)mp_itemsaving.BoxedValue)
                        {
                            SpawnSavedItems();
                        }
                    }
                    if ((bool)mp_ammosaving.BoxedValue)
                    {
                        // Adds saved ammo
                        while (AmmoInventory.Instance == null) await Task.Delay(50); // Waits till AmmoInventory is not null
                        var ammoInventory = AmmoInventory.Instance;
                        ammoInventory.ClearAmmo();
                        ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, CurrentSave.LightAmmo);
                        ammoInventory.AddCartridge(ammoInventory.mediumAmmoGroup, CurrentSave.MediumAmmo);
                        ammoInventory.AddCartridge(ammoInventory.heavyAmmoGroup, CurrentSave.HeavyAmmo);
                    }
                    LoggerInstance.Msg("Loaded inventory");
                    BoneLib.Notifications.Notifier.Send(new BoneLib.Notifications.Notification()
                    {
                        Title = "Success",
                        Message = "Successfully loaded the inventory",
                        ShowTitleOnPopup = true,
                        PopupLength = 2.5f,
                    });
                }
                catch (Exception ex)
                {
                    LoggerInstance.Error("An error occurred while loading the inventory", ex);
                    BoneLib.Notifications.Notifier.Send(new BoneLib.Notifications.Notification()
                    {
                        Title = "Failure",
                        Message = "Failed to load the inventory, check the logs or console for more details",
                        ShowTitleOnPopup = true,
                        PopupLength = 5f,
                    });
                }
            }
            else
            {
                LoggerInstance.Msg("Not loading inventory because level is blacklisted");
                BoneLib.Notifications.Notifier.Send(new BoneLib.Notifications.Notification()
                {
                    Message = "This level is blacklisted from loading/saving inventory",
                    Title = "Blacklisted",
                    ShowTitleOnPopup = true,
                    PopupLength = 5
                });
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
            var mainPage = Page.Root.CreatePage("HAHOOS", Color.white);
            var modPage = mainPage.CreatePage("KeepInventory", new Color(255, 72, 59));
            modPage.CreateBoolPref("Save Items", Color.white, ref mp_itemsaving, prefDefaultValue: true);
            modPage.CreateBoolPref("Save Ammo", Color.white, ref mp_ammosaving, prefDefaultValue: true);
            modPage.CreateBoolPref("Persistent Save", Color.magenta, ref mp_persistentsave, prefDefaultValue: true);
            modPage.CreateBoolPref("Blacklist BONELAB Levels", Color.cyan, ref mp_blacklistBONELABlevels, prefDefaultValue: true);
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

            PrefsCategory.SaveToFile(false);

            SaveCategory = MelonPreferences.CreateCategory<Save>("KeepInventory_Save", "Keep Inventory Save");
            SaveCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory_Save.cfg"));
            SaveCategory.SaveToFile(false);
            CurrentSave = SaveCategory.GetValue<Save>();

            if ((bool)mp_persistentsave.BoxedValue)
            {
                Slots.Clear();
                if (CurrentSave?.InventorySlots != null)
                {
                    foreach (KeyValuePair<string, string> id in (Dictionary<string, string>)CurrentSave.InventorySlots)
                    {
                        Slots.Add(id.Key, new Barcode(id.Value));
                    }
                }
            }
        }
    }
}