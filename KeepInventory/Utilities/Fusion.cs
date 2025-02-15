using System;

using BoneLib;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;

using UnityEngine;

using KeepInventory.Saves.V2;
using System.Collections.Generic;
using System.Linq;

namespace KeepInventory.Utilities
{
    /// <summary>
    /// Class that holds most methods for Fusion
    /// </summary>
    public static class Fusion
    {
        /// <summary>
        /// Boolean value indicating if user is connected to a server
        /// </summary>
        public static bool IsConnected
        {
            get
            {
                if (Core.HasFusion) return Internal_IsConnected();
                else return false;
            }
        }

        /// <summary>
        /// Setup the Fusion Support Library
        /// </summary>
        internal static void SetupFusionLibrary()
        {
            Core.Logger.Msg("Setting up the library");
            try
            {
                KeepInventory.Fusion.FusionModule.Setup(Core.Logger);
                KeepInventory.Fusion.FusionModule.LoadModule();
                KeepInventory.Fusion.ShareManager.Setup();
            }
            catch (Exception ex)
            {
                Core.FailedFLLoad = true;
                Core.Logger.Error($"An unexpected error has occurred while setting up and/or loading the fusion module, exception:\n{ex}");
            }
        }

        internal static void Setup()
        {
            KeepInventory.Fusion.ShareManager.OnShared += (save, sender) =>
            {
                if (LabFusion.Entities.NetworkPlayerManager.TryGetPlayer(sender, out LabFusion.Entities.NetworkPlayer player))
                {
                    if (!SaveManager.Saves.Any(x => x.ID == save.ID))
                    {
                        LabFusion.Utilities.FusionNotifier.Send(new LabFusion.Utilities.FusionNotification()
                        {
                            Title = "KeepInventory | Save shared!",
                            SaveToMenu = true,
                            ShowPopup = true,
                            Message = $"{player.Username} has shared a save with you called '{save.Name}'. Press accept to add save, decline will disregard this",
                            PopupLength = 15f,
                            Type = LabFusion.Utilities.NotificationType.INFORMATION,
                            OnAccepted = () => SaveManager.RegisterSave(save),
                            OnDeclined = () => Core.Logger.Msg("Save share ignored")
                        });
                    }
                }
            };
        }

        /// <summary>
        /// Check if the player is connected to a Fusion server without the Fusion Support Library
        /// </summary>
        /// <returns>A boolean value indicating whether or not is the player connected to a server</returns>
        internal static bool Internal_IsConnected()
        {
            return LabFusion.Network.NetworkInfo.HasServer;
        }

        internal static void Internal_ShareSave(byte smallId, Save save)
        {
            if (LabFusion.Entities.NetworkPlayerManager.TryGetPlayer(smallId, out LabFusion.Entities.NetworkPlayer plr))
                KeepInventory.Fusion.ShareManager.Share(save, plr.PlayerId);
            else
                throw new Exception($"Player with small ID {smallId} could not be found");
        }

        /// <summary>
        /// Share a save with a target player
        /// </summary>
        /// <param name="smallId">The small ID of the player to share with</param>
        /// <param name="save">The <see cref="Save"/> to share</param>
        public static void ShareSave(byte smallId, Save save)
        {
            if (Core.HasFusion && Core.IsFusionLibraryInitialized && IsConnected) Internal_ShareSave(smallId, save);
        }

        internal static IEnumerable<FusionPlayer> Internal_GetShareablePlayers()
        {
            var task = KeepInventory.Fusion.ShareManager.GetAllShareablePlayers();
            task.Wait();
            foreach (var player in LabFusion.Entities.NetworkPlayer.Players)
            {
                if (player != null && task.Result.Contains(player.PlayerId.SmallId))
                    yield return new FusionPlayer(player.PlayerId.SmallId, player.PlayerId.LongId, player.Username);
            }
        }

        /// <summary>
        /// Gets all players that you can share a save with, if not connected to any or doesn't have Fusion, returns an empty list
        /// </summary>
        /// <returns></returns>
        public static List<FusionPlayer> GetShareablePlayers()
        {
            if (Core.HasFusion && Core.IsFusionLibraryInitialized && IsConnected) return [.. Internal_GetShareablePlayers()];
            else return [];
        }

        internal static IEnumerable<FusionPlayer> Internal_GetPlayers()
        {
            foreach (var player in LabFusion.Entities.NetworkPlayer.Players)
            {
                yield return new FusionPlayer(player.PlayerId.SmallId, player.PlayerId.LongId, player.Username);
            }
        }

        /// <summary>
        /// Get all players in the current lobby, if not connected to any or doesn't have Fusion, returns an empty list
        /// </summary>
        /// <returns></returns>
        public static List<FusionPlayer> GetPlayers()
        {
            if (Core.HasFusion && IsConnected) return [.. Internal_GetPlayers()];
            else return [];
        }

        internal static byte Internal_GetLocalPlayerSmallId()
        {
            return LabFusion.Player.PlayerIdManager.LocalSmallId;
        }

        /// <summary>
        /// Get the small ID of the local player, if not connected to any or doesn't have Fusion, returns 0
        /// </summary>
        /// <returns>The small ID of the local player</returns>
        public static byte GetLocalPlayerSmallId()
        {
            if (Core.HasFusion && IsConnected) return Internal_GetLocalPlayerSmallId();
            else return 0;
        }

        /// <summary>
        /// Spawns an item in the provided <see cref="InventorySlotReceiver"/>
        /// </summary>
        /// <param name="barcode"><see cref="Barcode"/> of the spawnable you want to spawn into the provided <see cref="InventorySlotReceiver"/></param>
        /// <param name="inventorySlotReceiver"><see cref="InventorySlotReceiver"/> in which should the spawnable be spawned</param>
        /// <param name="slotName">Name of the slot (debug purposes)</param>
        /// <param name="slotColor">Color of the slot name (debug purposes)</param>
        /// <param name="inBetween">The <see cref="Action{GameObject}"/> that will be run between spawning and putting the spawnable into the <see cref="InventorySlotReceiver"/></param>
        internal static void SpawnInSlot(Barcode barcode, InventorySlotReceiver inventorySlotReceiver, string slotName, System.Drawing.Color slotColor, Action<GameObject> inBetween = null)
        {
            KeepInventory.Fusion.FusionModule.NetworkSpawnInSlotAsync(inventorySlotReceiver, barcode, slotColor, slotName, inBetween);
        }

        /// <summary>
        /// Find the <see cref="RigManager"/> for the local player that is connected to a server
        /// </summary>
        /// <returns>The <see cref="RigManager"/> of the local player</returns>
        internal static RigManager FindRigManager()
        {
            if (!IsConnected)
            {
                return Player.RigManager;
            }
            else
            {
                return LabFusion.Data.RigData.Refs.RigManager ?? Player.RigManager;
            }
        }

        private static Action RigCreatedEvent;

        /// <summary>
        /// Removes the <see cref="InventoryManager.SpawnSavedItems(Save)"/> method from the OnRigCreated event in the Fusion Support Library
        /// </summary>
        internal static void RemoveRigCreateEvent_FSL()
        {
            if (RigCreatedEvent != null)
            {
                KeepInventory.Fusion.FusionModule.OnRigCreated -= RigCreatedEvent;
                RigCreatedEvent = null;
            }
        }

        /// <summary>
        /// Removes the <see cref="InventoryManager.SpawnSavedItems(Save)"/> method from the OnRigCreated event in the Fusion Support Library
        /// </summary>
        internal static void RemoveRigCreateEvent()
        {
            if (Core.HasFusion && Core.IsFusionLibraryInitialized) RemoveRigCreateEvent_FSL();
        }

        /// <summary>
        /// Find the <see cref="AmmoInventory"/> of the local player
        /// </summary>
        /// <returns></returns>
        internal static AmmoInventory GetAmmoInventory()
        {
            if (Core.HasFusion) return IsConnected ? (LabFusion.Marrow.NetworkGunManager.NetworkAmmoInventory ?? AmmoInventory.Instance) : AmmoInventory.Instance;
            else return AmmoInventory.Instance;
        }

        /// <summary>
        /// Spawn the saved items, run when Fusion is detected
        /// <para>This is separate to avoid errors if Fusion Support Library is not loaded</para>
        /// </summary>
        internal static void SpawnSavedItems_FSL(Save save)
        {
            if (Core.FindRigManager() == null)
            {
                Core.Logger.Msg("Rig not found, awaiting");

                void _event() => InventoryManager.SpawnSavedItems(save);
                RigCreatedEvent = _event;
                KeepInventory.Fusion.FusionModule.OnRigCreated += _event;
            }
            else
            {
                Core.Logger.Msg("Rig found, spawning");
                InventoryManager.SpawnSavedItems(save);
            }
        }

        /// <summary>
        /// Spawn the saved items, run when Fusion is detected
        /// </summary>
        internal static void SpawnSavedItems(Save save)
        {
            if (IsConnected)
            {
                Core.Logger.Msg("Client is connected to a server");
                if (Core.mp_itemsaving.Value)
                {
                    if (Core.IsFusionLibraryInitialized) SpawnSavedItems_FSL(save);
                    else InventoryManager.SpawnSavedItems(save);
                }
            }
            else
            {
                Core.Logger.Msg("Client is not connected to a server, spawning locally");
                if (Core.mp_itemsaving.Value)
                {
                    InventoryManager.SpawnSavedItems(save);
                }
            }
        }

        /// <summary>
        /// Check if a gamemode is currently running in the server
        /// </summary>
        /// <returns>A boolean value indicating whether or not is a gamemode running</returns>
        internal static bool GamemodeCheck()
        {
            if (!IsConnected) return false;
            else return LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeStarted || LabFusion.SDK.Gamemodes.GamemodeManager.StartTimerActive || LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeReady;
        }

        /// <summary>
        /// Check if the current gamemode allows the use of KeepInventory
        /// </summary>
        /// <returns>A boolean value indicating whether or not does the gamemode allow the use of KeepInventory</returns>
        internal static bool DoesGamemodeAllow()
        {
            if (!IsConnected)
            {
                return false;
            }
            else
            {
                var active = LabFusion.SDK.Gamemodes.GamemodeManager.ActiveGamemode;
                if (active == null) return true;
                if (!GamemodeCheck()) return true;
                else return active.Metadata != null && active.Metadata.TryGetMetadata("AllowKeepInventory", out string val) && val != null && bool.TryParse(val, out bool res) && res;
            }
        }
    }

    /// <summary>
    /// Class that holds really small amounts of information about a player in a Fusion lobby
    /// </summary>
    /// <param name="smallId"><inheritdoc cref="SmallId"/></param>
    /// <param name="longId"><inheritdoc cref="LongId"/></param>
    /// <param name="displayName"><inheritdoc cref="DisplayName"/></param>
    public class FusionPlayer(byte smallId, ulong longId, string displayName)
    {
        /// <summary>
        /// Display name of the player
        /// </summary>
        public string DisplayName = displayName;

        /// <summary>
        /// Small id of the player
        /// </summary>
        public byte SmallId = smallId;

        /// <summary>
        /// Long id of the player
        /// </summary>
        public ulong LongId = longId;
    }
}