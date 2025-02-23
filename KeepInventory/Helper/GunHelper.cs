﻿using System;

using BoneLib;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;

using KeepInventory.Saves.V2;

namespace KeepInventory.Helper
{
    /// <summary>
    /// Provides helper methods for <see cref="Gun"/>
    /// </summary>
    public static class GunHelper
    {
        internal static void FusionLoadMagazine(this Gun gun, int rounds = -1, Action callback = null)
        {
            LabFusion.RPC.NetworkAssetSpawner.Spawn(new LabFusion.RPC.NetworkAssetSpawner.SpawnRequestInfo
            {
                position = Player.Head.position,
                rotation = Player.Head.rotation,
                spawnable = gun.defaultMagazine.spawnable,
                spawnEffect = false,
                spawnCallback = (spawn) =>
                {
                    var gunExt = LabFusion.Entities.GunExtender.Cache.Get(gun);
                    if (gunExt == null)
                        return;

                    var data = new LabFusion.Network.MagazineInsertData
                    {
                        smallId = LabFusion.Player.PlayerIdManager.LocalSmallId,
                        gunId = gunExt.Id,
                        magazineId = spawn.entity.Id
                    };
                    using var writer = LabFusion.Network.FusionWriter.Create();
                    writer.Write(data);
                    using var msg = LabFusion.Network.FusionMessage.Create(LabFusion.Network.NativeMessageTag.MagazineInsert, writer);
                    LabFusion.Network.MessageSender.SendToServer(LabFusion.Network.NetworkChannel.Reliable, msg);

                    var socketExtender = gunExt.GetExtender<LabFusion.Entities.AmmoSocketExtender>();
                    var mag = spawn.spawned.GetComponent<Magazine>();

                    if (socketExtender == null || mag == null)
                        return;

                    // Insert mag into gun
                    if (socketExtender.Component._magazinePlug)
                    {
                        var otherPlug = socketExtender.Component._magazinePlug;

                        if (otherPlug != mag.magazinePlug)
                        {
                            LabFusion.Patching.AmmoSocketPatches.IgnorePatch = true;

                            if (otherPlug)
                            {
                                LabFusion.Extensions.AlignPlugExtensions.ForceEject(otherPlug);
                            }

                            LabFusion.Patching.AmmoSocketPatches.IgnorePatch = false;
                        }
                    }
                    LabFusion.Extensions.InteractableHostExtensions.TryDetach(mag.magazinePlug.host);

                    LabFusion.Patching.AmmoSocketPatches.IgnorePatch = true;
                    mag.magazinePlug.InsertPlug(socketExtender.Component);
                    LabFusion.Patching.AmmoSocketPatches.IgnorePatch = false;

                    gun.MagazineState.SetCartridge(rounds);
                    callback?.Invoke();
                }
            });
        }

        /// <summary>
        /// Loads a magazine into a gun while having full Fusion support
        /// </summary>
        /// <param name="gun">The gun to load the magazine to</param>
        /// <param name="rounds">The amount of rounds in the magazine, -1 will be the default max</param>
        /// <param name="callback">Callback to be called when completed</param>
        public static void LoadMagazine(this Gun gun, int rounds = -1, Action callback = null)
        {
            ArgumentNullException.ThrowIfNull(gun, nameof(gun));
            if (rounds == -1) rounds = gun.defaultMagazine.rounds;
            if (!Utilities.Fusion.IsConnected)
            {
                var task = gun.ammoSocket.ForceLoadAsync(new MagazineData
                {
                    spawnable = gun.defaultMagazine.spawnable,
                    rounds = rounds,
                    platform = gun.defaultMagazine.platform,
                });
                var awaiter = task.GetAwaiter();

                void something()
                {
                    gun.MagazineState.SetCartridge(rounds);
                    callback?.Invoke();
                }

                awaiter.OnCompleted((System.Action)something);
            }
            else
            {
                FusionLoadMagazine(gun, rounds, callback);
            }
        }

        /// <summary>
        /// Updates the gun with provided <see cref="GunInfo"/>
        /// </summary>
        /// <param name="gun">The gun to update</param>
        /// <param name="info">The data to update with</param>
        /// <param name="slotColor">Color that will be used in the slot prefix</param>
        /// <param name="slot">The save slot (debugging purposes)</param>
        /// <param name="name">Name of the crate (debugging purposes)</param>
        /// <param name="barcode">Barcode of the spawnable (debugging purposes)</param>
        /// <param name="printMessages">If <see langword="true"/>, the method will print debug messages using <see cref="MelonLoader.MelonLogger.Instance"/></param>
        public static void UpdateProperties(this Gun gun, GunInfo info, System.Drawing.Color slotColor, SaveSlot slot = null, string name = "N/A", string barcode = "N/A", bool printMessages = true)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "N/A";
            if (string.IsNullOrWhiteSpace(barcode)) barcode = "N/A";
            string slotName = slot == null ? "N/A" : string.IsNullOrWhiteSpace(slot.SlotName) ? "N/A" : slot.SlotName;

            if (gun != null && info != null)
            {
                try
                {
                    void other()
                    {
                        if (printMessages) Core.MsgPrefix($"Setting fire mode value to {info.FireMode}", slotName, slotColor);
                        gun.fireMode = info.FireMode;
                        if (printMessages) Core.MsgPrefix($"Setting 'HasFiredOnce' value to {info.HasFiredOnce}", slotName, slotColor);
                        gun.hasFiredOnce = info.HasFiredOnce;
                        if (printMessages) Core.MsgPrefix($"Setting hammer state to {info.HammerState}", slotName, slotColor);
                        gun.hammerState = info.HammerState;
                        if (printMessages) Core.MsgPrefix($"Setting slide state to {info.SlideState}", slotName, slotColor);
                        gun.slideState = info.SlideState;

                        if (!gun.isCharged && info.IsBulletInChamber)
                        {
                            if (printMessages) Core.MsgPrefix("Charging gun", slotName, slotColor);
                            gun.Charge();
                            if (gun._hasMagState) gun.MagazineState.AddCartridge(1, gun.defaultCartridge);
                            if (printMessages) Core.MsgPrefix($"Setting cartridge state to {info.CartridgeState}", slotName, slotColor);
                            gun.cartridgeState = info.CartridgeState;
                        }

                        switch (info.SlideState)
                        {
                            case Gun.SlideStates.LOCKED:
                                gun.SlideLocked();
                                break;

                            case Gun.SlideStates.PULLING:
                                gun.CompleteSlidePull();
                                break;

                            case Gun.SlideStates.RETURNING:
                                gun.CompleteSlideReturn();
                                break;
                        }

                        if (printMessages) Core.MsgPrefix($"Spawned to slot: {name} ({barcode})", slotName, slotColor);
                    }

                    if (printMessages) Core.MsgPrefix($"Writing gun info for: {name} ({barcode})", slotName, slotColor);
                    if (info.IsMag && gun.defaultMagazine != null && gun.defaultCartridge != null)
                    {
                        if (printMessages) Core.MsgPrefix("Loading magazine", slotName, slotColor);
                        var mag = info.GetMagazineData(gun);
                        if (mag?.spawnable?.crateRef != null && !string.IsNullOrWhiteSpace(mag.platform))
                        {
                            LoadMagazine(gun, info.RoundsLeft, other);
                        }
                        else
                        {
                            if (printMessages) Core.Logger.Warning($"[{slotName}] Could not get sufficient information for MagazineData, not loading the magazine and rounds left");
                            other();
                        }
                    }
                    else
                    {
                        other();
                    }
                }
                catch (Exception ex)
                {
                    Core.Logger.Error($"An unexpected error has occurred while applying gun info, exception:\n{ex}");
                    BLHelper.SendNotification("Error", "Failed to successfully apply gun info to the gun, check the console or logs for more info", true, 3f, BoneLib.Notifications.NotificationType.Error);
                }
            }
        }
    }
}