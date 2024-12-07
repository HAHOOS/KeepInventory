using BoneLib;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Warehouse;

using KeepInventory.Fusion.Messages;
using KeepInventory.Helper;
using KeepInventory.Saves;

using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

using MelonLoader;
using MelonLoader.Pastel;

using System;
using System.Threading.Tasks;

using UnityEngine;

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
        /// RigManager of <see cref="LocalPlayer"/>
        /// </summary>
        public static RigManager RigManager { get; private set; }

        /// <summary>
        /// Ammo Inventory of the local player
        /// </summary>
        public static AmmoInventory AmmoInventory { get; private set; }

        /// <summary>
        /// <see cref="NetworkPlayer"/> of the current client
        /// </summary>
        public static NetworkPlayer LocalNetworkPlayer { get; private set; }

        /// <summary>
        /// Event that triggers when RigManager is created
        /// </summary>
        public static event Action<RigManager> OnRigCreated;

        /// <summary>
        /// Boolean value indicating whether or not the client is connected to a server
        /// </summary>
        public static bool IsConnected
        { get { return LocalNetworkPlayer != null; } set { } }

        internal static MelonLogger.Instance backupLogger;

        internal static ModuleLogger logger;

        private static void RigCreated(RigManager manager)
        {
            RigManager = manager;
            FindAmmoInventory();
            MsgFusionPrefix("Rig was created, running OnRigCreated event");
            OnRigCreated?.Invoke(manager);
        }

        private static void GetPlayer()
        {
            MsgFusionPrefix("Detected join / start server, trying to get local player");
            if (NetworkPlayerManager.TryGetPlayer(PlayerIdManager.LocalId, out NetworkPlayer player))
            {
                MsgFusionPrefix($"Found player / Small ID: {PlayerIdManager.LocalSmallId}");
                LocalNetworkPlayer = player;
                RigManager = Player.RigManager;
            }
            else
            {
                Warn("Could not find player");
            }
        }

        private static void OnDisconnect()
        {
            MsgFusionPrefix("Detected disconnect");
            LocalNetworkPlayer = null;
            RigManager = Player.RigManager;
        }

        /// <summary>
        /// Sets up the <see cref="MultiplayerHooking"/> and <see cref="MelonLoader.MelonLogger.Instance"/>
        /// </summary>
        public static void Setup(MelonLogger.Instance _logger)
        {
            backupLogger = _logger;
            logger = FusionModule.ModuleLogger;

            MultiplayerHooking.OnJoinServer -= GetPlayer;
            MultiplayerHooking.OnStartServer -= GetPlayer;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
            MultiplayerHooking.OnMainSceneInitialized -= () => FindAmmoInventory();
            LocalPlayer.OnLocalRigCreated -= RigCreated;

            MultiplayerHooking.OnJoinServer += GetPlayer;
            MultiplayerHooking.OnDisconnect += OnDisconnect;
            MultiplayerHooking.OnMainSceneInitialized += () => FindAmmoInventory();
            MultiplayerHooking.OnStartServer += GetPlayer;
            LocalPlayer.OnLocalRigCreated += RigCreated;
        }

        /// <summary>
        /// Finds the <see cref="Il2CppSLZ.Marrow.AmmoInventory"/> of local player
        /// </summary>
        /// <returns><see cref="Il2CppSLZ.Marrow.AmmoInventory"/> of local player</returns>
        public static AmmoInventory FindAmmoInventory()
        {
            AmmoInventory = NetworkGunManager.NetworkAmmoInventory ?? Il2CppSLZ.Marrow.AmmoInventory.Instance;
            return AmmoInventory;
        }

        /// <summary>
        /// Loads the messages
        /// </summary>
        public static void LoadModule()
        {
            MsgFusionPrefix("Loading module");
            ModuleManager.RegisterModule<FusionModule>();
        }

        /// <summary>
        /// Sends a message to the Fusion server to update the provided <see cref="Gun"/> using <see cref="GunUpdateMessage"/>
        /// </summary>
        /// <param name="gun"><see cref="Gun"/> to update</param>
        /// <param name="info"><see cref="GunInfo"/> to update with</param>
        public static async void SendFusionMessage(Gun gun, GunInfo info)
        {
            if (IsConnected)
            {
                if (gun == null || info == null)
                {
                    Warn("Some values are null, cannot send GunUpdate message");
                    return;
                }
                MsgFusionPrefix("Attempting to send a GunUpdate message");
                GunMessageData data = null;

                int attempts = 0;
                const int maxAttempts = 5;
                const float interval = 0.35f * 1000;

                while (attempts < maxAttempts)
                {
                    attempts++;
                    var _data = GunMessageData.Create(gun, info);
                    if (_data != null)
                    {
                        data = _data;
                        break;
                    }
                    else
                    {
                        Warn($"Could not successfully create GunMessageData (Attempt {attempts}/{maxAttempts})");
                        await Task.Delay((int)MathF.Round(interval));
                    }
                }

                if (data == null)
                {
                    Warn("Data was null, cannot send message");
                    return;
                }
                FusionWriter writer = FusionWriter.Create();
                try
                {
                    writer.Write(data);
                    FusionMessage msg = FusionMessage.ModuleCreate<GunUpdateMessage>(writer);
                    try
                    {
                        if (PlayerIdManager.LocalSmallId != PlayerIdManager.HostSmallId)
                        {
                            MsgFusionPrefix("Local player is not host, sending to server");
                            MessageSender.SendToServer(NetworkChannel.Reliable, msg);
                        }
                        else
                        {
                            MsgFusionPrefix("Local player is host, reading message");
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, msg);
                        }
                    }
                    finally
                    {
                        msg?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Error($"An unexpected error occurred while sending message to host:\n{ex}");
                }
                finally
                {
                    writer?.Dispose();
                }
            }
        }

        internal static void MsgFusionPrefix(string message)
        {
            if (logger == null && backupLogger == null) return;
            if (logger != null) logger.Log(message);
            else MsgPrefix("Fusion", message, System.Drawing.Color.Cyan);
        }

        internal static void Warn(string message)
        {
            if (logger == null && backupLogger == null) return;
            if (logger != null) logger.Warn(message);
            else backupLogger.Warning($"[Fusion] {message}");
        }

        internal static void Error(string message)
        {
            if (logger == null && backupLogger == null) return;
            if (logger != null) logger.Error(message);
            else backupLogger.Warning($"[Fusion] {message}");
        }

        internal static void MsgPrefix(string prefix, string message, System.Drawing.Color color)
        {
            backupLogger._MsgPastel($"[{prefix.Pastel(color)}] {message}");
        }

        /// <summary>
        /// Invokes <see cref="NetworkEntity.InvokeCatchup(PlayerId)"/> for every connected player
        /// </summary>
        /// <param name="networkEntity"><see cref="NetworkEntity"/> to invoke the catchup on</param>
        public static void InvokeCatchupToAll(this NetworkEntity networkEntity)
        {
            NetworkPlayer.Players.ForEach(x => networkEntity.InvokeCatchup(x.PlayerId));
        }

        private static async Task Spawn(InventorySlotReceiver receiver, Barcode barcode, System.Drawing.Color slotColor, string slotName = "N/A", Action<GameObject> inBetween = null)
        {
            if (barcode == null || string.IsNullOrWhiteSpace(barcode.ID) || receiver == null)
            {
                Error($"[{slotName}] Barcode is either null or empty, or the InventorySlotReceiver was null");
                return;
            }
            MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Attempting to spawn {barcode.ID}");

            bool returned = false;
            NetworkAssetSpawner.SpawnCallbackInfo? result = null;

            Exception exception = null;

            var head = RigManager.physicsRig.m_head;

            var info = new NetworkAssetSpawner.SpawnRequestInfo
            {
                rotation = head.rotation,
                position = (head.position + (head.forward * 1.5f)),
                spawnable = new Spawnable() { crateRef = new SpawnableCrateReference(barcode), policyData = null },
                spawnEffect = false,
                spawnCallback = async (callbackInfo) =>
                {
                    MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Spawned item (Coordinates: {callbackInfo.spawned.transform.position.ToString() ?? "N/A"})");
                    try
                    {
                        int attempts = 0;
                        const int maxAttempts = 5;
                        const float interval = 0.35f * 1000;

                        NetworkEntity slotEntity = null;

                        while (attempts < maxAttempts)
                        {
                            attempts++;
                            if (!InventorySlotReceiverExtender.Cache.TryGet(receiver, out var _slotEntity))
                            {
                                Warn($"Could not find the provided receiver in InventorySlotReceiverExtender Cache (Attempt {attempts}/{maxAttempts})");
                                returned = true;
                                await Task.Delay((int)MathF.Round(interval));
                            }
                            else
                            {
                                slotEntity = _slotEntity;
                                break;
                            }
                        }

                        if (slotEntity == null)
                        {
                            Warn("Network Entity for InventorySlotReceiver was not found, aborting");
                            returned = true;
                            return;
                        }

                        var weaponExtender = callbackInfo.entity.GetExtender<WeaponSlotExtender>();

                        if (weaponExtender == null)
                        {
                            Warn("Weapon Slot Extender was not found, aborting");
                            returned = true;
                            return;
                        }

                        var slotExtender = slotEntity.GetExtender<InventorySlotReceiverExtender>();

                        byte? index = (byte?)slotExtender.GetIndex(receiver);

                        if (!index.HasValue)
                        {
                            Warn($"[{slotName}] Could not find the extender for the provided receiver, aborting");
                            returned = true;
                            return;
                        }

                        MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Running in between function");
                        inBetween?.Invoke(callbackInfo.spawned);

                        MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Attempting to equip");

                        using var writer = FusionWriter.Create(InventorySlotInsertData.Size);
                        var insertData = InventorySlotInsertData.Create(slotEntity.Id, PlayerIdManager.LocalSmallId, callbackInfo.entity.Id, index.Value);

                        writer.Write(insertData);

                        var message = FusionMessage.Create(NativeMessageTag.InventorySlotInsert, writer);

                        try
                        {
                            weaponExtender.Component.interactableHost.TryDetach();

                            var component = slotExtender.GetComponent(index.Value);

                            component.InsertInSlot(weaponExtender.Component.interactableHost);
                        }
                        finally
                        {
                            message?.Dispose();
                        }

                        result = callbackInfo;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        returned = true;
                    }
                }
            };

            NetworkAssetSpawner.Spawn(info);

            // For fuck's sake just now found out it will download by itself if you do a spawn request
            // There used to be code here that I wasted hours on, because it wouldn't work

            while (result == null && !returned && exception == null) await Task.Delay(50);
            if (exception != null) throw exception;
        }

        /// <summary>
        /// Spawns a <see cref="Spawnable"/> from provided <see cref="Barcode"/> to an <see cref="InventorySlot"/>
        /// </summary>
        /// <param name="receiver">The <see cref="InventorySlotReceiver"/> to spawn the <see cref="Spawnable"/> in</param>
        /// <param name="barcode">The <see cref="Barcode"/> to be used to spawn the <see cref="Spawnable"/></param>
        /// <param name="slotColor">Color that will be used in the slot prefix</param>
        /// <param name="slotName">Name of the slot (debugging reasons)</param>
        /// <param name="inBetween">An action that will run between the Spawn Request and Spawn In Slot events</param>
        /// <returns>Entity ID of the <see cref="Spawnable"/></returns>
        public static async Task NetworkSpawnInSlotAsync(this InventorySlotReceiver receiver, Barcode barcode, System.Drawing.Color slotColor, string slotName = "N/A", Action<GameObject> inBetween = null)
        {
            // This used to have a queue system, that's why its a separate method
            if (!IsConnected)
            {
                Warn($"[{slotName}] The player is not connected to a server!");
                return;
            }

            if (!MarrowGame.assetWarehouse.HasCrate(barcode))
            {
                Warn($"[{slotName}] You do not have the mod installed!");
                return;
            }

            await Spawn(receiver, barcode, slotColor, slotName, inBetween);
        }
    }
}