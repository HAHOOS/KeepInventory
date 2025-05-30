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

namespace KeepInventory.Managers
{
    public static class InventoryManager
    {
        internal static bool LoadAmmoOnAwake = false;

        public static void SaveInventory(Save save, bool notifications = false)
        {
            if (save == null)
            {
                Core.Logger.Error("Cannot save to an empty save!");
                return;
            }
            try
            {
                Core.Logger.Msg("Saving inventory...");
                if (PreferencesManager.ItemSaving.Value)
                {
                    Core.Logger.Msg("Saving items in inventory slots");

                    bool notFound = false;
                    var rigManager = Player.RigManager;
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
                                    if (gunInfo != null && PreferencesManager.SaveGunData.Value && poolee.SpawnableCrate.Barcode != new Barcode(CommonBarcodes.Misc.SpawnGun))
                                        save.InventorySlots.Add(new SaveSlot(name, barcode, gunInfo));
                                    else
                                        save.InventorySlots.Add(new SaveSlot(name, barcode));
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
                    }
                    else
                    {
                        Core.Logger.Error("Could not save inventory, because some required game objects were not found. Items from the earlier save will be kept");
                        if (notifications) BLHelper.SendNotification("Failure", "Failed to save the inventory, because some required game objects were not found, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
                    }
                }
                if (PreferencesManager.AmmoSaving.Value)
                {
                    save.LightAmmo = AmmoManager.GetValue("light");
                    Core.Logger.Msg("Saved Light Ammo: " + save.LightAmmo);
                    save.MediumAmmo = AmmoManager.GetValue("medium");
                    Core.Logger.Msg("Saved Medium Ammo: " + save.MediumAmmo);
                    save.HeavyAmmo = AmmoManager.GetValue("heavy");
                    Core.Logger.Msg("Saved Heavy Ammo: " + save.HeavyAmmo);
                }
                save.SaveToFile(true);
                Core.Logger.Msg("Successfully saved inventory");

                if (notifications) BLHelper.SendNotification("Success", "Successfully saved the inventory!", true, 5f, BoneLib.Notifications.NotificationType.Success);
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"An unexpected error occurred while saving the inventory:\n{ex}");
                if (notifications) BLHelper.SendNotification("Failure", "Failed to save the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
            }
        }

        internal static void SpawnSavedItems(Save save)
        {
            if (save == null)
            {
                Core.Logger.Error("Cannot spawn items from an empty save!");
                return;
            }

            Utilities.Fusion.RemoveRigCreateEvent();

            try
            {
                if (save.InventorySlots?.Count > 0)
                {
                    var rigManager = Player.RigManager;
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
                    var list = rigManager.GetComponentsInChildren<InventorySlotReceiver>().ToList();

                    foreach (var item in save.InventorySlots)
                    {
                        void spawn(InventorySlotReceiver receiver)
                        {
                            if (MarrowGame.assetWarehouse.HasCrate(new Barcode(item.Barcode)))
                            {
                                var crate = new SpawnableCrateReference(item.Barcode);

                                if (item.Type == SaveSlot.SpawnableType.Gun && PreferencesManager.SaveGunData.Value && item.Barcode != CommonBarcodes.Misc.SpawnGun)
                                {
                                    void action(GameObject obj)
                                    {
                                        if (item.GunInfo != null && obj != null)
                                        {
                                            foreach (var gun in obj.GetComponents<Gun>())
                                                gun.UpdateProperties(item.GunInfo, item, crate.Crate.name, item.Barcode);
                                        }
                                    }

                                    if (Core.HasFusion && Utilities.Fusion.IsConnected)
                                    {
                                        receiver.SpawnInSlot(crate.Crate.Barcode, action);
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
                                    if (Core.HasFusion && Utilities.Fusion.IsConnected)
                                    {
                                        receiver.SpawnInSlot(crate.Crate.Barcode);
                                    }
                                    else
                                    {
                                        var task = receiver.SpawnInSlotAsync(crate.Crate.Barcode);
                                    }
                                }
                            }
                            else
                            {
                                Core.Logger.Warning($"[{item.SlotName}] Could not find crate with the following barcode: {item.Barcode}");
                            }
                        }
                        InventorySlotReceiver slot = rigManager.FindSlot(item.SlotName);
                        if (slot != null)
                            spawn(slot);
                        else
                            Core.Logger.Warning($"[{item.SlotName}] No slot was found with the provided name. It is possible that an avatar that was used during the saving had more slots than the current one");
                    }
                }
                else
                {
                    Core.Logger.Msg("No saved items found");
                }
                Core.Logger.Msg("Loaded inventory");
                BLHelper.SendNotification("Success", "Successfully loaded the inventory", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
            }
            catch (Exception ex)
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
            var ammoInventory = AmmoInventory.Instance ?? throw new Exception("Ammo inventory is null");
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

        public static void AddSavedAmmo(Save save)
        {
            if (save == null)
            {
                Core.Logger.Error("Cannot load ammo from an empty save!");
                return;
            }
            if (!UnityEngine.Object.CurrentThreadIsMainThread())
            {
                Core.Logger.Warning("Adding ammo not on the main thread, this may cause crashes due to protected memory");
            }
            var ammoInventory = AmmoInventory.Instance;
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
            if (!PreferencesManager.ItemSaving.Value)
            {
                Core.Logger.Msg("Loaded inventory");
                if (PreferencesManager.ShowNotifications.Value) BLHelper.SendNotification("Success", "Successfully loaded the inventory", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
            }
        }

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
                Core.Logger.Msg("Loading inventory...");
                if (PreferencesManager.AmmoSaving.Value)
                {
                    Core.Logger.Msg("Waiting for Ammo Inventory to be initialized");
                    var ammoInventory = AmmoInventory.Instance;
                    if (ammoInventory != null)
                    {
                        AddSavedAmmo(save);
                    }
                    else
                    {
                        Core.Logger.Warning("Ammo Inventory is empty, awaiting");
                        AmmoInventoryPatches.Save = save;
                    }
                }

                if (Core.HasFusion)
                {
                    Core.Logger.Msg("Checking if client is connected to a Fusion server");
                    Utilities.Fusion.SpawnSavedItems(save);
                }
                else
                {
                    if (PreferencesManager.ItemSaving.Value)
                    {
                        Core.Logger.Msg("Spawning in slots saved items");
                        SpawnSavedItems(save);
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Logger.Error("An error occurred while loading the inventory", ex);
                BLHelper.SendNotification("Failure", "Failed to load the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
            }
        }

        public static void ClearInventory()
        {
            var slots = Player.RigManager.GetAllSlots();
            slots.ForEach(x =>
            {
                var host = x._weaponHost;
                if (host != null)
                {
                    var poolee = host.GetTransform()?.GetComponent<Poolee>();
                    poolee?.Despawn();
                }
            });
        }
    }
}