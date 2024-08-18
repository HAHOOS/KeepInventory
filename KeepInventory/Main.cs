using MelonLoader;
using BoneLib;
using BoneLib.BoneMenu;
using UnityEngine;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(global::HAHOOS.KeepInventory.Main), "Keep Inventory", "1.1.1", "HAHOOS", "https://thunderstore.io/c/bonelab/p/HAHOOS/KeepInventory/")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
[assembly: MelonAuthorColor(0, 255, 165, 0)]
[assembly: MelonColor(0, 255, 72, 59)]

namespace HAHOOS.KeepInventory
{
    public class Main : MelonMod
    {
        public Dictionary<string, Barcode> Slots { get; private set; } = new Dictionary<string, Barcode>();

        public int LightAmmo;
        public int MediumAmmo;
        public int HeavyAmmo;

        #region MelonPreferences

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Settings

        private MelonPreferences_Category mp_modCategory;

        private MelonPreferences_Entry mp_itemsaving;
        private MelonPreferences_Entry mp_ammosaving;

        private MelonPreferences_Entry mp_persistentsave;

        private MelonPreferences_Entry mp_blacklistedLevels;

        // Save

        private MelonPreferences_Category mp_saveCategory;

        private MelonPreferences_Entry mp_itemslots;

        private MelonPreferences_Entry mp_ammolight;
        private MelonPreferences_Entry mp_ammomedium;
        private MelonPreferences_Entry mp_ammoheavy;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion MelonPreferences

        /// <summary>
        /// Variable of element in BoneMenu responsible for showing if level is blacklisted or not
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FunctionElement statusElement;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Current Level Info
        /// </summary>
        private LevelInfo levelInfo;

        /// <summary>
        /// Calls when MelonLoader loads all Mods/Plugins
        /// </summary>
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Setting up KeepInventory");
            SetupPreferences();
            SetupMenu();
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
                Dictionary<string, string> slots_id = new();
                foreach (KeyValuePair<string, Barcode> item in Slots) slots_id.Add(item.Key, item.Value.ID);
                mp_itemslots.BoxedValue = slots_id;
                mp_ammolight.BoxedValue = LightAmmo;
                mp_ammomedium.BoxedValue = MediumAmmo;
                mp_ammoheavy.BoxedValue = HeavyAmmo;
                mp_saveCategory.SaveToFile();
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
                HeavyAmmo = 0;
                MediumAmmo = 0;
                LightAmmo = 0;
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
                    Dictionary<string, string> slots_id = new();
                    foreach (KeyValuePair<string, Barcode> item in Slots) slots_id.Add(item.Key, item.Value.ID);
                    mp_itemslots.BoxedValue = slots_id;
                }
                if ((bool)mp_ammosaving.BoxedValue)
                {
                    var ammoInventory = AmmoInventory.Instance;
                    LightAmmo = ammoInventory.GetCartridgeCount("light");
                    MediumAmmo = ammoInventory.GetCartridgeCount("medium");
                    HeavyAmmo = ammoInventory.GetCartridgeCount("heavy");

                    mp_ammolight.BoxedValue = LightAmmo;
                    mp_ammomedium.BoxedValue = MediumAmmo;
                    mp_ammoheavy.BoxedValue = HeavyAmmo;
                }
                LoggerInstance.Msg("Successfully saved inventory");
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
            if (!((List<string>)mp_blacklistedLevels.BoxedValue).Contains(obj.barcode))
            {
                statusElement.ElementName = "Current level is not blacklisted";
                statusElement.ElementColor = Color.green;
                LoggerInstance.Msg("Loading inventory...");
                if ((bool)mp_itemsaving.BoxedValue)
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
                if ((bool)mp_ammosaving.BoxedValue)
                {
                    // Adds saved ammo
                    while (AmmoInventory.Instance == null) await Task.Delay(50); // Waits till AmmoInventory is not null
                    var ammoInventory = AmmoInventory.Instance;
                    ammoInventory.ClearAmmo();
                    ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, LightAmmo);
                    ammoInventory.AddCartridge(ammoInventory.mediumAmmoGroup, MediumAmmo);
                    ammoInventory.AddCartridge(ammoInventory.heavyAmmoGroup, HeavyAmmo);
                }
                LoggerInstance.Msg("Loaded inventory");
            }
            else
            {
                LoggerInstance.Msg("Not loading inventory because level is blacklisted");
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
            modPage.CreateBool("Save Items", Color.white, (bool)mp_itemsaving.BoxedValue, (value) =>
            {
                mp_itemsaving.BoxedValue = value;
                mp_itemsaving.Save();
            });
            modPage.CreateBool("Save Ammo", Color.white, (bool)mp_ammosaving.BoxedValue, (value) =>
            {
                mp_ammosaving.BoxedValue = value;
                mp_ammosaving.Save();
            });
            modPage.CreateBool("Persistent Save", Color.magenta, (bool)mp_persistentsave.BoxedValue, (value) =>
            {
                mp_persistentsave.BoxedValue = value;
                mp_persistentsave.Save();
            });
            modPage.CreateFunction("Blacklist Level from Saving/Loading", Color.red, () =>
            {
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
            statusElement = modPage.CreateFunction("Current Level is not blacklisted", Color.green, () => { });
        }

        /// <summary>
        /// Set up Preferences<br/>
        /// They are used to save settings so it can be used every time u play
        /// </summary>
        private void SetupPreferences()
        {
            List<string> defaultBlacklistedLevels = new(){
                CommonBarcodes.Maps.VoidG114,
                CommonBarcodes.Maps.Home,
                CommonBarcodes.Maps.Ascent,
                CommonBarcodes.Maps.Descent,
                CommonBarcodes.Maps.BLHub,
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
            };
            mp_modCategory = MelonPreferences.CreateCategory("HAHOOS_KeepInventory_Settings");
            mp_itemsaving = mp_modCategory.CreateEntry<bool>("ItemSaving", true, "Item Saving",
                description: "If true, will save and load items in inventory");
            mp_ammosaving = mp_modCategory.CreateEntry<bool>("AmmoSaving", true, "Ammo Saving",
                description: "If true, will save and load ammo in inventory");
            mp_persistentsave = mp_modCategory.CreateEntry<bool>("PersistentSave", true, "Persistent Save",
                description: "If true, will save and load inventory in a KeepInventory_Save.cfg file to be used between sessions");
            mp_blacklistedLevels = mp_modCategory.CreateEntry<List<string>>("BlacklistedLevels", defaultBlacklistedLevels, "Blacklisted Levels",
                description: "List of levels that will not save/load inventory");

            mp_saveCategory = MelonPreferences.CreateCategory("HAHOOS_KeepInventory_Save");
            mp_itemslots = mp_saveCategory.CreateEntry<Dictionary<string, string>>("ItemSlots",
                new Dictionary<string, string>(), "Item Slots", description: "Saved items in the inventory");
            mp_ammolight = mp_saveCategory.CreateEntry<int>("AmmoLight", 0, "Ammo Light",
                description: "Saved ammo of type Light");
            mp_ammomedium = mp_saveCategory.CreateEntry<int>("AmmoMedium", 0, "Ammo Medium",
                description: "Saved ammo of type Medium");
            mp_ammoheavy = mp_saveCategory.CreateEntry<int>("AmmoHeavy", 0, "Ammo Heavy",
                description: "Saved ammo of type Heavy");

            mp_saveCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory_Save.cfg"));

            if ((bool)mp_persistentsave.BoxedValue)
            {
                Slots.Clear();
                foreach (KeyValuePair<string, string> id in (Dictionary<string, string>)mp_itemslots.BoxedValue)
                {
                    Slots.Add(id.Key, new Barcode(id.Value.ToString()));
                }
                try
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    LightAmmo = int.Parse(mp_ammolight.BoxedValue.ToString());
                    MediumAmmo = int.Parse(mp_ammomedium.BoxedValue.ToString());
                    HeavyAmmo = int.Parse(mp_ammoheavy.BoxedValue.ToString());
#pragma warning restore CS8604 // Possible null reference argument.
                }
                catch (Exception)
                {
                    LoggerInstance.Error("Could not parse saved data from String to Int, don't tell me you tried to modify the file yourself..");
                }
            }
        }
    }
}