using System;

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;

using KeepInventory.Helper;
using KeepInventory.Saves.V2;
using BoneLib;
using System.Collections.Generic;
using System.Linq;
using Il2CppSLZ.Marrow.Utilities;
using KeepInventory.Utilities;
using UnityEngine;

using KeepInventory.Patches;

namespace KeepInventory
{
    /// <summary>
    /// Class that handles most of work with the inventory
    /// </summary>
    public static class InventoryManager
    {
        /// <summary>
        /// A boolean value indicating if the next <see cref="KeepInventory.Patches.AmmoInventoryPatches.Awake"/> should run
        /// </summary>
        internal static bool LoadAmmoOnAwake = false;

        static readonly Dictionary<string, string> SaveNames = new()
        {
            { "Descent", CommonBarcodes.Maps.Descent },
            { "Ascent", CommonBarcodes.Maps.Ascent },
            { "LongRun", CommonBarcodes.Maps.LongRun },
            { "Hub", CommonBarcodes.Maps.BLHub },
            { "KartBowling", CommonBarcodes.Maps.BigBoneBowling },
            { "Tuscany", CommonBarcodes.Maps.Tuscany },
            { "HalfwayPark", CommonBarcodes.Maps.HalfwayPark },
            { "MineDive", CommonBarcodes.Maps.MineDive },
            { "BigAnomaly_A", CommonBarcodes.Maps.BigAnomaly },
            { "StreetPuncher", CommonBarcodes.Maps.StreetPuncher },
            { "SprintBridge", CommonBarcodes.Maps.SprintBridge },
            { "MagmaGate", CommonBarcodes.Maps.MagmaGate },
            { "MoonBase", CommonBarcodes.Maps.Moonbase },
            { "MonogonMotorway", CommonBarcodes.Maps.MonogonMotorway },
            { "Pillar", CommonBarcodes.Maps.PillarClimb },
            { "BigAnomaly_B", CommonBarcodes.Maps.BigAnomaly2 },
            { "DungeonWarrior", CommonBarcodes.Maps.DungeonWarrior },
            { "DistrictParkour", CommonBarcodes.Maps.NeonParkour },
            { "FantasyArena", CommonBarcodes.Maps.FantasyArena },
            { "Baseline", CommonBarcodes.Maps.Baseline },
            { "GunRangeSandbox", CommonBarcodes.Maps.GunRange },
            { "MuseumSandbox", CommonBarcodes.Maps.MuseumBasement },
            { "Mirror", CommonBarcodes.Maps.Mirror },
            { "G114", CommonBarcodes.Maps.VoidG114 },
        };

        /// <summary>
        /// Saves the current inventory, overriding provided <see cref="Save"/>
        /// </summary>
        public static void SaveInventory(Save save, bool notifications = false)
        {
            if (save == null)
            {
                Core.Logger.Error("Cannot save to an empty save!");
                return;
            }
            try
            {
                if (Core.HasFusion && Utilities.Fusion.IsConnected && (!Core.IsFusionLibraryInitialized || !Core.mp_fusionSupport.Value))
                {
                    BLHelper.SendNotification("Failure", "Could not save inventory, because either the 'Fusion Support' setting is set to Disabled or the Fusion Support Library did not load correctly", true, 3.5f, BoneLib.Notifications.NotificationType.Error);
                    Core.Logger.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly");
                    return;
                }
                Core.Logger.Msg("Saving inventory...");
                bool isItemSaved = false;
                bool isAmmoSaved = false;
                if (Core.mp_itemsaving.Value)
                {
                    Core.Logger.Msg("Saving items in inventory slots");

                    bool notFound = false;
                    var rigManager = Core.FindRigManager();
                    if (rigManager == null)
                    {
                        Core.Logger.Warning("RigManager does not exist, cannot save inventory slots");
                        notFound = true;
                    }

                    if (!notFound)
                    {
                        save.InventorySlots?.Clear();
                        foreach (var item in rigManager.GetAllSlots())
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
                                    string name = item.GetSlotName();
                                    var barcode = poolee.SpawnableCrate.Barcode;
                                    Core.Logger.Msg($"Slot: {name} / Barcode: {poolee.SpawnableCrate.name} ({poolee.SpawnableCrate.Barcode.ID})");
                                    if (gunInfo != null && Core.mp_saveGunData.Value && poolee.SpawnableCrate.Barcode != new Barcode(CommonBarcodes.Misc.SpawnGun))
                                    {
                                        save.InventorySlots.Add(new SaveSlot(name, barcode, gunInfo));
                                    }
                                    else
                                    {
                                        save.InventorySlots.Add(new SaveSlot(name, barcode));
                                    }
                                }
                                else
                                {
                                    Core.Logger.Warning($"[{item.transform.parent.name}] Could not find poolee of the spawnable in the inventory slot");
                                }
                            }
                        }
                        if (save.InventorySlots.Count == 0)
                        {
                            Core.Logger.Msg("No spawnables were found in slots");
                        }
                        isItemSaved = true;
                    }
                    else
                    {
                        Core.Logger.Error("Could not save inventory, because some required game objects were not found. Items from the earlier save will be kept");
                        if (notifications) BLHelper.SendNotification("Failure", "Failed to save the inventory, because some required game objects were not found, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
                    }
                }
                if (Core.mp_ammosaving.Value)
                {
                    save.LightAmmo = Core._lastAmmoCount_light;
                    Core.Logger.Msg("Saved Light Ammo: " + save.LightAmmo);
                    save.MediumAmmo = Core._lastAmmoCount_medium;
                    Core.Logger.Msg("Saved Medium Ammo: " + save.MediumAmmo);
                    save.HeavyAmmo = Core._lastAmmoCount_heavy;
                    Core.Logger.Msg("Saved Heavy Ammo: " + save.HeavyAmmo);
                    isAmmoSaved = true;
                }
                save.SaveToFile(true);
                Core.Logger.Msg("Successfully saved inventory");

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
                Core.Logger.Error($"An unexpected error occurred while saving the inventory:\n{ex}");
                if (notifications) BLHelper.SendNotification("Failure", "Failed to save the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
            }
        }

        /// <summary>
        /// Removes the initial inventory from all BONELAB campaign saves.
        /// </summary>
        /// <returns>Did it save successfully</returns>
        public static bool RemoveInitialInventoryFromAllSaves()
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

        /// <summary>
        /// Removes the initial inventory from a BONELAB campaign save with provided barcode.
        /// </summary>
        /// <param name="barcode">The barcode of the level that has the save</param>
        /// <returns>Did it save successfully and was the save found</returns>
        public static bool RemoveInitialInventoryFromSave(string barcode)
        {
            Action staged = null;
            var value = SaveNames.FirstOrDefault(x => x.Value == barcode);
            if (!string.IsNullOrWhiteSpace(value.Key))
            {
                var item = Il2CppSLZ.Bonelab.SaveData.DataManager.ActiveSave.Progression.LevelState[value.Key];
                bool changed = false;
                foreach (var y in item)
                {
                    if (y.Key == "SLZ.Bonelab.initial_inventory" && y.Value != null)
                    {
                        Core.Logger.Warning($"Found initial inventory in save (Level: {value.Key}), removing");
                        staged += () =>
                        {
                            changed = true;
                            item[y.Key] = null;
                        };
                    }
                }
                staged += () =>
                {
                    if (!changed) return;
                    Il2CppSLZ.Bonelab.SaveData.DataManager.ActiveSave.Progression.LevelState[value.Key] = item;
                };

                staged?.Invoke();
                return Il2CppSLZ.Bonelab.SaveData.DataManager.TrySaveActiveSave(Il2CppSLZ.Marrow.SaveData.SaveFlags.Progression);
            }
            return false;
        }

        /// <summary>
        /// Checks if there is a save for the provided level
        /// </summary>
        /// <param name="barcode">The barcode of the level</param>
        /// <returns><see langword="true"/> if found a save, otherwise <see langword="false"/></returns>
        public static bool DoesSaveForLevelExist(string barcode)
        {
            foreach (var item in Il2CppSLZ.Bonelab.SaveData.DataManager.ActiveSave.Progression.LevelState)
            {
                var value = SaveNames.FirstOrDefault(x => x.Value == barcode);
                if (!string.IsNullOrWhiteSpace(value.Key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Spawn the saved items in provided save to the inventory
        /// </summary>
        /// <param name="save"><see cref="Save"/> to spawn the items from</param>
        internal static void SpawnSavedItems(Save save)
        {
            if (save == null)
            {
                Core.Logger.Error("Cannot spawn items from an empty save!");
                return;
            }
            if (Core.HasFusion && Utilities.Fusion.IsConnected && (!Core.IsFusionLibraryInitialized || !Core.mp_fusionSupport.Value))
            {
                Core.Logger.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly"); return;
            }

            Utilities.Fusion.RemoveRigCreateEvent();

            try
            {
                if (save.InventorySlots?.Count > 0)
                {
                    var rigManager = Core.FindRigManager();
                    if (rigManager == null)
                    {
                        Core.Logger.Error("RigManager does not exist, cannot load saved items!");
                        return;
                    }
                    if (rigManager.inventory == null)
                    {
                        Core.Logger.Error("Inventory does not exist, cannot load saved items!");
                        return;
                    }

                    // Adds saved items to inventory slots
                    var list = rigManager.GetComponentsInChildren<InventorySlotReceiver>().ToList();

                    foreach (var item in save.InventorySlots)
                    {
                        var SlotColor = Colors.GetRandomSlotColor();
                        Core.MsgPrefix("Looking for slot", item.SlotName, SlotColor);

                        void spawn(InventorySlotReceiver receiver)
                        {
                            if (MarrowGame.assetWarehouse.HasCrate(new Barcode(item.Barcode)))
                            {
                                var crate = new SpawnableCrateReference(item.Barcode);

                                if (item.Type == SaveSlot.SpawnableType.Gun && Core.mp_saveGunData.Value && item.Barcode != CommonBarcodes.Misc.SpawnGun)
                                {
                                    // Settings properties for the gun, this is horrible
                                    void action(GameObject obj)
                                    {
                                        if (item.GunInfo != null && obj != null)
                                        {
                                            var guns = obj.GetComponents<Gun>();
                                            Core.MsgPrefix("Attempting to write GunInfo", item.SlotName, SlotColor);
                                            foreach (var gun in guns)
                                                gun.UpdateProperties(item.GunInfo, SlotColor, item, crate.Crate.name, item.Barcode, false, false);
                                        }
                                    }

                                    Core.MsgPrefix($"Spawning to slot: {crate.Crate.name} ({item.Barcode})", item.SlotName, SlotColor);

                                    if (Core.HasFusion && Utilities.Fusion.IsConnected)
                                    {
                                        Utilities.Fusion.SpawnInSlot(crate.Crate.Barcode, receiver, item.SlotName, SlotColor, action);
                                    }
                                    else
                                    {
                                        var task = receiver.SpawnInSlotAsync(crate.Crate.Barcode);
                                        var awaiter = task.GetAwaiter();
                                        Action notGun = () => action(receiver._weaponHost.GetHostGameObject());
                                        awaiter.OnCompleted(notGun);
                                    }
                                }
                                else
                                {
                                    Core.MsgPrefix($"Spawning to slot: {crate.Crate.name} ({item.Barcode})", item.SlotName, SlotColor);
                                    if (Core.HasFusion && Utilities.Fusion.IsConnected)
                                    {
                                        Utilities.Fusion.SpawnInSlot(crate.Crate.Barcode, receiver, item.SlotName, SlotColor);
                                        Core.MsgPrefix($"Spawned to slot: {crate.Crate.name} ({item.Barcode})", item.SlotName, SlotColor);
                                    }
                                    else
                                    {
                                        var task = receiver.SpawnInSlotAsync(crate.Crate.Barcode);
                                        Action complete = () => Core.MsgPrefix($"Spawned to slot: {crate.Crate.name} ({item.Barcode})", item.SlotName, SlotColor);
                                        task.GetAwaiter().OnCompleted(complete);
                                    }
                                }
                            }
                            else
                            {
                                Core.Logger.Warning($"[{item.SlotName}] Could not find crate with the following barcode: {item.Barcode}");
                            }
                        }

                        // Check for a slot with the same name and one that is for spawnables, not ammo
                        InventorySlotReceiver slot = rigManager.FindSlot(item.SlotName);
                        if (slot != null)
                        {
                            var receiver = slot;
                            //if (receiver._weaponHost?.GetHostGameObject() != null) MonoBehaviour.Destroy(receiver._weaponHost.GetHostGameObject());
                            spawn(receiver);
                        }
                        else
                        {
                            Core.Logger.Warning($"[{item.SlotName}] No slot was found with the provided name. It is possible that an avatar that was used during the saving had more slots than the current one");
                        }
                    }
                }
                else
                {
                    Core.Logger.Msg("No saved items found");
                }
                Core.Logger.Msg("Loaded inventory");
                BLHelper.SendNotification("Success", "Successfully loaded the inventory", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
            }
            catch (System.Exception ex)
            {
                Core.Logger.Error("An error occurred while loading the inventory", ex);
                BLHelper.SendNotification("Failure", "Failed to load the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
            }
        }

        private static void AddAmmo(AmmoType type, int count)
        {
            if (count < 0) return;
            if (HelperMethods.CheckIfAssemblyLoaded("infiniteammo"))
            {
                Core.Logger.Warning("Game contains InfiniteAmmo mod, not adding ammo");
                return;
            }
            var ammoInventory = Core.GetAmmoInventory() ?? throw new Exception("Ammo inventory is null");
            //AmmoGroup ammoGroup = type == AmmoType.Light ? ammoInventory.lightAmmoGroup : type == AmmoType.Medium ? ammoInventory.mediumAmmoGroup : ammoInventory.heavyAmmoGroup;
            //ammoInventory.AddCartridge(ammoGroup, count);
            ammoInventory._groupCounts[type.ToString().ToLower()] = count;
            ammoInventory.onAmmoUpdateCount?.Invoke(type.ToString().ToLower(), count);
            ammoInventory.onAmmoUpdate?.Invoke();
        }

        private enum AmmoType
        {
            Light,
            Medium,
            Heavy
        }

        /// <summary>
        /// Sets ammo from provided <see cref="Save"/>
        /// </summary>
        /// <param name="save"><see cref="Save"/> to add the ammo from</param>
        /// <param name="showNotifications">If notifications should be shown</param>
        public static void AddSavedAmmo(Save save, bool showNotifications = true)
        {
            if (save == null)
            {
                Core.Logger.Error("Cannot load ammo from an empty save!");
                return;
            }
            if (!AmmoInventory.CurrentThreadIsMainThread())
            {
                Core.Logger.Warning("Adding ammo not on the main thread, this may cause crashes due to protected memory");
            }
            var ammoInventory = Core.GetAmmoInventory();
            if (ammoInventory == null)
            {
                Core.Logger.Error("Ammo inventory is null");
                return;
            }
            if (!HelperMethods.CheckIfAssemblyLoaded("infiniteammo"))
            {
                ammoInventory.ClearAmmo();
                Core.Logger.Msg($"Adding light ammo: {save.LightAmmo}");
                AddAmmo(AmmoType.Light, save.LightAmmo);
                Core.Logger.Msg($"Adding medium ammo: {save.MediumAmmo}");
                AddAmmo(AmmoType.Medium, save.MediumAmmo);
                Core.Logger.Msg($"Adding heavy ammo: {save.HeavyAmmo}");
                AddAmmo(AmmoType.Heavy, save.HeavyAmmo);
            }
            else
            {
                Core.Logger.Warning("Game contains InfiniteAmmo mod, not adding ammo");
                return;
            }
            if (!Core.mp_itemsaving.Value)
            {
                Core.Logger.Msg("Loaded inventory");
                if (showNotifications) BLHelper.SendNotification("Success", "Successfully loaded the inventory", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
            }
        }

        /// <summary>
        /// Loads the saved inventory from provided <see cref="Save"/>
        /// </summary>
        /// <param name="save"><see cref="Save"/> to load the inventory from</param>
        public static void LoadSavedInventory(Save save)
        {
            if (save == null)
            {
                Core.Logger.Error("Cannot load an empty save!");
                return;
            }

            if (Core.HasFusion && Utilities.Fusion.IsConnected && Utilities.Fusion.GamemodeCheck() && !Utilities.Fusion.DoesGamemodeAllow())
            {
                Core.Logger.Warning("A gamemode is currently running, we cannot load your inventory!");
                return;
            }
            try
            {
                if (Core.HasFusion && Utilities.Fusion.IsConnected && (!Core.IsFusionLibraryInitialized || !Core.mp_fusionSupport.Value))
                {
                    BLHelper.SendNotification("Failure", "Could not load inventory, because either the 'Fusion Support' setting is set to Disabled or the Fusion Support Library did not load correctly", true, 3.5f, BoneLib.Notifications.NotificationType.Error);
                    Core.Logger.Warning("The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. Try enabling 'Fusion Support' in settings or restarting the game if you have Fusion Support option enabled. The Fusion Support library might have not loaded properly");
                    return;
                }
                Core.Logger.Msg("Loading inventory...");
                if (Core.mp_ammosaving.Value)
                {
                    // Adds saved ammo
                    Core.Logger.Msg("Waiting for Ammo Inventory to be initialized");
                    var ammoInventory = Core.GetAmmoInventory();
                    if (ammoInventory != null)
                    {
                        AddSavedAmmo(save, Core.mp_showNotifications.Value);
                    }
                    else
                    {
                        Core.Logger.Warning("Ammo Inventory is empty, awaiting");
                        AmmoInventoryPatches.Save = save;
                    }
                }

                if (Core.HasFusion)
                {
                    // Spawns the saved items by sending messages to the Fusion server
                    Core.Logger.Msg("Checking if client is connected to a Fusion server");
                    Utilities.Fusion.SpawnSavedItems(save);
                }
                else
                {
                    if (Core.mp_itemsaving.Value)
                    {
                        Core.Logger.Msg("Spawning in slots saved items");
                        SpawnSavedItems(save);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Core.Logger.Error("An error occurred while loading the inventory", ex);
                BLHelper.SendNotification("Failure", "Failed to load the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
            }
        }
    }
}