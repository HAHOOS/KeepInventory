using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;
using KeepInventory.Fusion.Messages;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

using MelonLoader.Pastel;

using MelonLoader;

using System;

using UnityEngine;

using KeepInventory.Helper;

using LabFusion.Extensions;

using System.Threading.Tasks;

namespace KeepInventory.Fusion
{
    /// <summary>
    /// Module for Fusion
    /// </summary>
    public class FusionModule : Module
    {
        /// <summary>
        /// Name of the module
        /// </summary>
        public override string Name => "KeepInventory";

        /// <summary>
        /// Author of the module
        /// </summary>
        public override string Author => "HAHOOS";

        /// <summary>
        /// Version of the module
        /// </summary>
        public override Version Version => System.Version.Parse(_Version);

        /// <summary>
        /// Color in the console of the module
        /// </summary>
        public override ConsoleColor Color => ConsoleColor.Yellow;

        /// <summary>
        /// Runs when module is registered
        /// </summary>
        internal static ModuleLogger ModuleLogger { get; set; }

        /// <summary>
        /// Runs when module gets registered
        /// </summary>
        protected override void OnModuleRegistered()
        {
            ModuleLogger = LoggerInstance;
            MsgFusionPrefix("Registering ShareSaveMessage");
            ModuleMessageHandler.RegisterHandler<ShareSaveMessage>();
            MsgFusionPrefix("Registering CanShareRequestMessage");
            ModuleMessageHandler.RegisterHandler<CanShareRequestMessage>();
            MsgFusionPrefix("Registering CanShareResponseMessage");
            ModuleMessageHandler.RegisterHandler<CanShareResponseMessage>();
        }

        /// <summary>
        /// Version of the library, used mainly for AssemblyInfo
        /// </summary>
        public const string _Version = KeepInventory.Core.Version;

        /// <summary>
        /// Event that triggers when RigManager is created
        /// </summary>
        public static event Action OnRigCreated;

        internal static MelonLogger.Instance backupLogger;

        internal static ModuleLogger logger;

        private static void RigCreated(RigManager manager)
        {
            OnRigCreated?.Invoke();
        }

        /// <summary>
        /// Sets up the <see cref="MultiplayerHooking"/> and <see cref="MelonLoader.MelonLogger.Instance"/>
        /// </summary>
        public static void Setup(MelonLogger.Instance _logger)
        {
            backupLogger = _logger;
            logger = FusionModule.ModuleLogger;

            LocalPlayer.OnLocalRigCreated -= RigCreated;

            LocalPlayer.OnLocalRigCreated += RigCreated;
        }

        /// <summary>
        /// Loads the messages
        /// </summary>
        public static void LoadModule()
        {
            MsgFusionPrefix("Loading module");
            ModuleManager.RegisterModule<FusionModule>();
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
        /// Spawns a <see cref="Spawnable"/> from provided <see cref="Barcode"/> to an <see cref="InventorySlot"/>
        /// </summary>
        /// <param name="receiver">The <see cref="InventorySlotReceiver"/> to spawn the <see cref="Spawnable"/> in</param>
        /// <param name="barcode">The <see cref="Barcode"/> to be used to spawn the <see cref="Spawnable"/></param>
        /// <param name="slotColor">Color that will be used in the slot prefix</param>
        /// <param name="slotName">Name of the slot (debugging reasons)</param>
        /// <param name="inBetween">An action that will run between the Spawn Request and Spawn In Slot events</param>
        /// <returns>Entity ID of the <see cref="Spawnable"/></returns>
        public static async Task NetworkSpawnInSlotAsync(InventorySlotReceiver receiver, Barcode barcode, System.Drawing.Color slotColor, string slotName = "N/A", Action<GameObject> inBetween = null)
        {
            if (!NetworkInfo.HasServer)
            {
                Warn($"[{slotName}] The player is not connected to a server!");
                return;
            }

            if (!MarrowGame.assetWarehouse.HasCrate(barcode))
            {
                Warn($"[{slotName}] You do not have the mod installed!");
                return;
            }

            if (barcode == null || string.IsNullOrWhiteSpace(barcode.ID) || receiver == null)
            {
                Error($"[{slotName}] Barcode is either null or empty, or the InventorySlotReceiver was null");
                return;
            }
            MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Attempting to spawn {barcode.ID}");

            bool returned = false;
            NetworkAssetSpawner.SpawnCallbackInfo? result = null;

            Exception exception = null;

            var head = RigData.Refs.RigManager.physicsRig.m_head;

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
    }
}