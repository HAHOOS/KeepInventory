using BoneLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Warehouse;
using KeepInventory.Fusion.Messages;
using KeepInventory.SaveSlot;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Modules;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace KeepInventory.Fusion
{
    /// <summary>
    /// Provides methods for LabFusion support
    /// </summary>
    public static class FusionMethods
    {
        /// <summary>
        /// Version of the library, used mainly for AssemblyInfo
        /// </summary>
        public const string Version = "1.0.0";

        /// <summary>
        /// Boolean value indicating whether or not the client is connected to a server
        /// </summary>
        public readonly static bool IsConnected = NetworkInfo.HasServer;

        /// <summary>
        /// Loads the messages
        /// </summary>
        public static void LoadModule()
        {
            ModuleHandler.LoadModule(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Loads the messages
        /// </summary>
        public static void LoadModule(Assembly assembly)
        {
            ModuleHandler.LoadModule(assembly);
        }

        /// <summary>
        /// Sends a message to the Fusion server to update the gun
        /// </summary>
        /// <param name="gun">Gun to update</param>
        /// <param name="info">Data to update with</param>
        public static void SendFusionMessage(Gun gun, GunInfo info)
        {
            if (IsConnected)
            {
                GunMessageData data = GunMessageData.Create(gun, info);
                if (data == null) return;
                FusionWriter writer = FusionWriter.Create();
                try
                {
                    writer.Write(data);
                    FusionMessage msg = FusionMessage.ModuleCreate<GunMessage>(writer);
                    try
                    {
                        MessageSender.SendToServer(0, msg);
                    }
                    finally
                    {
                        msg.Dispose();
                    }
                }
                finally
                {
                    writer.Dispose();
                }
            }
        }

        /// <summary>
        /// Spawns a <see cref="Spawnable"/> from provided <see cref="Barcode"/> to an <see cref="InventorySlot"/>
        /// </summary>
        /// <param name="receiver">The <see cref="InventorySlotReceiver"/> to spawn the <see cref="Spawnable"/> in</param>
        /// <param name="barcode">The <see cref="Barcode"/> to be used to spawn the <see cref="Spawnable"/></param>
        /// <returns>Entity ID of the <see cref="Spawnable"/></returns>
        public static async Task<NetworkAssetSpawner.SpawnCallbackInfo?> NetworkSpawnInSlotAsync(this InventorySlotReceiver receiver, Barcode barcode)
        {
            if (!IsConnected) return null;

            if (!MarrowGame.assetWarehouse.HasCrate<SpawnableCrate>(barcode)) return null;

            bool returned = false;
            NetworkAssetSpawner.SpawnCallbackInfo? result = null;

            Exception exception = null;

            var info = new NetworkAssetSpawner.SpawnRequestInfo
            {
                rotation = Player.Head.rotation,
                position = (Player.Head.position + (Player.Head.forward * 1.5f)),
                spawnable = new Spawnable() { crateRef = new SpawnableCrateReference(barcode), policyData = null },
                spawnCallback = (callbackInfo) =>
                {
                    try
                    {
                        if (!InventorySlotReceiverExtender.Cache.TryGet(receiver, out var slotEntity))
                        {
                            returned = true;
                            return;
                        }

                        var slotExtender = slotEntity.GetExtender<InventorySlotReceiverExtender>();

                        byte? index = (byte?)slotExtender.GetIndex(receiver);

                        if (!index.HasValue)
                        {
                            returned = true;
                            return;
                        }

                        // HACK: Avoid errors caused by inserting items in slot without gripping

                        var gun = callbackInfo.spawned.GetComponent<Gun>();
                        bool gripped = false;
                        if (gun != null)
                        {
                            if (gun.triggerGrip != null)
                            {
                                var grab = new SerializedEntityGrab();
                                grab.RequestGrab(LocalPlayer.GetNetworkPlayer(), Il2CppSLZ.Marrow.Interaction.Handedness.RIGHT, gun.triggerGrip);
                                gripped = true;
                            }
                        }

                        if (!gripped)
                        {
                            var grip = callbackInfo.spawned.GetComponent<Grip>();
                            if (grip != null)
                            {
                                var grab = new SerializedEntityGrab();
                                grab.RequestGrab(LocalPlayer.GetNetworkPlayer(), Il2CppSLZ.Marrow.Interaction.Handedness.RIGHT, grip);
                                gripped = true;
                            }
                            else
                            {
                                grip = callbackInfo.spawned.GetComponentInChildren<Grip>();
                                if (grip != null)
                                {
                                    var grab = new SerializedEntityGrab();
                                    grab.RequestGrab(LocalPlayer.GetNetworkPlayer(), Il2CppSLZ.Marrow.Interaction.Handedness.RIGHT, grip);
                                    gripped = true;
                                }
                            }
                        }

                        using var writer = FusionWriter.Create(InventorySlotInsertData.Size);
                        var insertData = InventorySlotInsertData.Create(slotEntity.Id, PlayerIdManager.LocalSmallId, callbackInfo.entity.Id, index.Value);

                        writer.Write(insertData);

                        using var message = FusionMessage.Create(NativeMessageTag.InventorySlotInsert, writer);
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        result = callbackInfo;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        returned = true;
                    }
                }
            };
            while (result == null && !returned && exception == null) await Task.Delay(50);
            if (exception != null) throw exception;
            return result;
        }
    }
}