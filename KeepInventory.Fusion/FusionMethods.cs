using BoneLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Warehouse;
using KeepInventory.Fusion.Messages;
using KeepInventory.Helper;
using KeepInventory.Saves;
using LabFusion.Downloading;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;
using LabFusion.RPC;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using MelonLoader;
using MelonLoader.Pastel;

using System;
using System.Reflection;
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

        internal static MelonLogger.Instance logger;

        private static void RigCreated(RigManager manager)
        {
            RigManager = manager;
            MsgFusionPrefix("Rig was created, running OnRigCreated event");
            OnRigCreated?.Invoke(manager);
        }

        private static void GetPlayer()
        {
            MsgFusionPrefix("Detected join / start server, trying to get local player");
            if (NetworkPlayerManager.TryGetPlayer(PlayerIdManager.LocalId, out NetworkPlayer player))
            {
                MsgFusionPrefix("Found player");
                LocalNetworkPlayer = player;
                RigManager = Player.RigManager;
            }
            else
            {
                logger.Warning("[Fusion] Could not find player");
            }
        }

        private static void OnDisconnect()
        {
            MsgFusionPrefix("Detected disconnect");
            LocalNetworkPlayer = null;
            RigManager = Player.RigManager;
        }

        /// <summary>
        /// Sets up the <see cref="MultiplayerHooking"/> and <see cref="MelonLoader.MelonLogger.Instance"/>, as well as the <see cref="BONELABContentBarcodes"/> variable
        /// </summary>
        public static void Setup(MelonLogger.Instance _logger)
        {
            logger = _logger;

            MultiplayerHooking.OnJoinServer -= GetPlayer;
            MultiplayerHooking.OnStartServer -= GetPlayer;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
            LocalPlayer.OnLocalRigCreated -= RigCreated;

            MultiplayerHooking.OnJoinServer += GetPlayer;
            MultiplayerHooking.OnDisconnect += OnDisconnect;
            MultiplayerHooking.OnStartServer += GetPlayer;
            LocalPlayer.OnLocalRigCreated += RigCreated;
        }

        /// <summary>
        /// Loads the messages
        /// </summary>
        public static void LoadModule()
        {
            MsgFusionPrefix("Loading module");
            ModuleHandler.LoadModule(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Sends a message to the Fusion server to update the provided <see cref="Gun"/> using <see cref="GunMessage"/>
        /// </summary>
        /// <param name="gun"><see cref="Gun"/> to update</param>
        /// <param name="info"><see cref="GunInfo"/> to update with</param>
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

        internal static void MsgFusionPrefix(string message)
        {
            logger._MsgPastel($"[{"Fusion".Pastel(System.Drawing.Color.Cyan)}] {message}");
        }

        internal static void MsgPrefix(string prefix, string message, System.Drawing.Color color)
        {
            logger._MsgPastel($"[{prefix.Pastel(color)}] {message}");
        }

        private static async Task Spawn(InventorySlotReceiver receiver, Barcode barcode, System.Drawing.Color slotColor, string slotName = "N/A", Action<GameObject> inBetween = null)
        {
            if (barcode == null || string.IsNullOrWhiteSpace(barcode.ID) || receiver == null)
            {
                logger.Error($"[Fusion] [{slotName}] Barcode is either null or empty, or the InventorySlotReceiver was null");
                return;
            }
            MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Attempting to spawn");

            bool returned = false;
            NetworkAssetSpawner.SpawnCallbackInfo? result = null;

            Exception exception = null;

            var head = RigManager.physicsRig.m_head;

            var info = new NetworkAssetSpawner.SpawnRequestInfo
            {
                rotation = head.rotation,
                position = (head.position + (head.forward * 1.5f)),
                spawnable = new Spawnable() { crateRef = new SpawnableCrateReference(barcode), policyData = null },
                spawnCallback = (callbackInfo) =>
                {
                    MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Spawned item");
                    try
                    {
                        MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Requesting ownership");
                        NetworkEntityManager.TakeOwnership(callbackInfo.entity);

                        if (!InventorySlotReceiverExtender.Cache.TryGet(receiver, out var slotEntity))
                        {
                            logger.Warning("Could not find the provided receiver in InventorySlotReceiverExtender Cache");
                            returned = true;
                            return;
                        }

                        var slotExtender = slotEntity.GetExtender<InventorySlotReceiverExtender>();

                        byte? index = (byte?)slotExtender.GetIndex(receiver);

                        if (!index.HasValue)
                        {
                            logger.Warning($"[Fusion] [{slotName}] Could not find the extender for the provided receiver");
                            returned = true;
                            return;
                        }

                        MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Running in between function (if found)");
                        inBetween?.Invoke(callbackInfo.spawned);

                        MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Gripping item to avoid Fusion throwing errors");

                        MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Attempting to equip");
                        using var writer = FusionWriter.Create(InventorySlotInsertData.Size);
                        var insertData = InventorySlotInsertData.Create(slotEntity.Id, PlayerIdManager.LocalSmallId, callbackInfo.entity.Id, index.Value);

                        writer.Write(insertData);

                        using var message = FusionMessage.Create(NativeMessageTag.InventorySlotInsert, writer);
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);

                        // Put item in slot locally
                        if (PlayerIdManager.LocalSmallId != PlayerIdManager.HostSmallId) InventorySlotInsertMessage.ReadMessage(message.ToByteArray(), false);

                        result = callbackInfo;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        returned = true;
                    }
                }
            };

            if (KeepInventory.BONELABContentBarcodes.Contains(barcode.ID))
            {
                MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] {barcode.ID} is from BONELAB");
                NetworkAssetSpawner.Spawn(info);
            }
            else
            {
                MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] {barcode.ID} is a mod");

                bool hasFile = false;

                var modInfo = new NetworkModRequester.ModInstallInfo()
                {
                    barcode = barcode.ID,
                    target = PlayerIdManager.HostSmallId,
                    maxBytes = ClientSettings.Downloading.MaxFileSize.Value * 1000,
                    beginDownloadCallback = (x) =>
                    {
                        hasFile = x.hasFile;
                        if (x.hasFile)
                        {
                            MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Host has already the mod installed, spawning");
                            NetworkAssetSpawner.Spawn(info);
                        }
                    },
                    finishDownloadCallback = (x) =>
                    {
                        if (x.result != ModResult.FAILED)
                        {
                            MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Host has successfully downloaded & installed mod: {x.pallet.Title ?? "N/A"} {x.pallet.Version ?? "N/A"} by {x.pallet.Author ?? "N/A"} ({x.pallet.Barcode.ID ?? "N/A"})");
                            NetworkAssetSpawner.Spawn(info);
                        }
                        else
                        {
                            if (!hasFile)
                            {
                                logger.Error($"[Fusion] [{slotName}] Downloaded for the host failed!");
                                returned = true;
                            }
                        }
                    }
                };

                // Requests for the mod to be installed
                NetworkModRequester.RequestAndInstallMod(modInfo);
            }
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
                MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] The player is not connected to a server!");
                return;
            }

            if (!MarrowGame.assetWarehouse.HasCrate(barcode))
            {
                MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] You do not have the mod installed!");
                return;
            }

            await Spawn(receiver, barcode, slotColor, slotName, inBetween);
        }
    }
}