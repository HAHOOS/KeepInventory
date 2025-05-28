using System;

using BoneLib;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;

using UnityEngine;

using KeepInventory.Saves.V2;
using System.Collections.Generic;
using System.Linq;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Utilities;
using MelonLoader.Pastel;
using Il2CppSLZ.Marrow.Pool;
using System.Threading.Tasks;
using KeepInventory.Managers;

namespace KeepInventory.Utilities
{
    public static class Fusion
    {
        public static bool IsConnected
        {
            get
            {
                if (Core.HasFusion) return Internal_IsConnected();
                else return false;
            }
        }
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
                            Message = $"{player.Username} has shared a save with you called '<color=#{save.Color}>{save.Name}</color>'. Go to the LabFusion notifications menu, press accept to add save, decline will disregard this",
                            PopupLength = 15f,
                            Type = LabFusion.Utilities.NotificationType.INFORMATION,
                            OnAccepted = () => SaveManager.RegisterSave(save),
                            OnDeclined = () => Core.Logger.Msg("Save share ignored")
                        });
                    }
                }
            };
        }
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
        public static void ShareSave(byte smallId, Save save)
        {
            if (Core.HasFusion && Core.IsFusionLibraryInitialized && IsConnected) Internal_ShareSave(smallId, save);
        }

        internal async static Task<List<FusionPlayer>> Internal_GetShareablePlayers()
        {
            var task = await KeepInventory.Fusion.ShareManager.GetAllShareablePlayers();
            List<FusionPlayer> players = [];
            foreach (var player in LabFusion.Entities.NetworkPlayer.Players)
            {
                if (player != null && task.Contains(player.PlayerId.SmallId))
                    players.Add(new FusionPlayer(player.PlayerId.SmallId, player.PlayerId.LongId, player.Username));
            }
            return players;
        }
        public static async Task<List<FusionPlayer>> GetShareablePlayers()
        {
            if (Core.HasFusion && Core.IsFusionLibraryInitialized && IsConnected)
            {
                var players = await Internal_GetShareablePlayers();
                return players;
            }
            else
            {
                return [];
            }
        }

        internal static IEnumerable<FusionPlayer> Internal_GetPlayers()
        {
            foreach (var player in LabFusion.Entities.NetworkPlayer.Players)
            {
                yield return new FusionPlayer(player.PlayerId.SmallId, player.PlayerId.LongId, player.Username);
            }
        }
        public static List<FusionPlayer> GetPlayers()
        {
            if (Core.HasFusion && IsConnected) return [.. Internal_GetPlayers()];
            else return [];
        }

        internal static byte Internal_GetLocalPlayerSmallId()
        {
            return LabFusion.Player.PlayerIdManager.LocalSmallId;
        }
        public static byte GetLocalPlayerSmallId()
        {
            if (Core.HasFusion && IsConnected) return Internal_GetLocalPlayerSmallId();
            else return 0;
        }
        public static bool IsLocalPlayer(this RigManager rigManager)
        {
            if (!IsConnected) return true;
            else return Internal_IsLocalPlayer(rigManager);
        }

        private static bool Internal_IsLocalPlayer(this RigManager rigManager)
        {
            return LabFusion.Utilities.FusionPlayer.IsLocalPlayer(rigManager);
        }

        private static Action RigCreatedEvent;
        internal static void RemoveRigCreateEvent_FSL()
        {
            if (RigCreatedEvent != null)
            {
                KeepInventory.Fusion.FusionModule.OnRigCreated -= RigCreatedEvent;
                RigCreatedEvent = null;
            }
        }
        internal static void RemoveRigCreateEvent()
        {
            if (Core.HasFusion && Core.IsFusionLibraryInitialized) RemoveRigCreateEvent_FSL();
        }
        internal static void SpawnSavedItems_FSL(Save save)
        {
            if (Player.RigManager == null)
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
        internal static bool GamemodeCheck()
        {
            if (!IsConnected) return false;
            else return Internal_GamemodeCheck();
        }

        internal static bool Internal_GamemodeCheck()
            => LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeStarted || LabFusion.SDK.Gamemodes.GamemodeManager.StartTimerActive || LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeReady;
        public static bool IsGamemodeStarted
        {
            get
            {
                if (!IsConnected) return false;
                else return Internal_IsGamemodeStarted();
            }
        }

        internal static bool Internal_IsGamemodeStarted()
            => LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeStarted;
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
        public static void SpawnInSlot(this InventorySlotReceiver receiver, Barcode barcode, Action<GameObject> callback = null)
        {
            if (receiver._slottedWeapon?.interactableHost != null)
            {
                receiver._weaponHost?.ForceDetach();
                receiver.DropWeapon();
            }
            var task = receiver.SpawnInSlotAsync(barcode);
            var awaiter = task.GetAwaiter();
            awaiter.OnCompleted((Action)(() =>
            {
                if (awaiter.GetResult())
                    callback?.Invoke(receiver._slottedWeapon.GetComponentInParent<Poolee>()?.gameObject);
            }));
        }

        internal static void Fusion_LoadMagazine(this Gun gun, int rounds = -1, Action callback = null)
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

                    var socketExtender = gunExt.GetExtender<LabFusion.Entities.AmmoSocketExtender>();
                    var mag = spawn.spawned.GetComponent<Magazine>();

                    if (socketExtender == null || mag == null)
                        return;

                    if (socketExtender.Component._magazinePlug)
                    {
                        var otherPlug = socketExtender.Component._magazinePlug;

                        if (otherPlug != mag.magazinePlug)
                        {
                            if (otherPlug)
                                LabFusion.Extensions.AlignPlugExtensions.ForceEject(otherPlug);
                        }
                    }
                    LabFusion.Extensions.InteractableHostExtensions.TryDetach(mag.magazinePlug.host);

                    mag.magazinePlug.InsertPlug(socketExtender.Component);

                    gun.MagazineState.SetCartridge(rounds);
                    callback?.Invoke();
                }
            });
        }
        public static void LoadMagazine(this Gun gun, int rounds = -1, Action callback = null)
        {
            if (rounds == -1)
                rounds = gun.defaultMagazine.rounds;
            if (IsConnected)
            {
                Fusion_LoadMagazine(gun, rounds, callback);
            }
            else
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

                awaiter.OnCompleted((Action)something);
            }
        }

        internal static void MsgFusionPrefix(string message)
        {
            MsgPrefix("Fusion", message, System.Drawing.Color.Cyan);
        }

        internal static void Warn(string message)
        {
            Core.Logger.Warning($"[Fusion] {message}");
        }

        internal static void Error(string message)
        {
            Core.Logger.Warning($"[Fusion] {message}");
        }

        internal static void MsgPrefix(string prefix, string message, System.Drawing.Color color)
        {
            Core.Logger.Msg($"[{prefix.Pastel(color)}] {message}");
        }
    }
    public class FusionPlayer(byte smallId, ulong longId, string displayName)
    {
        public string DisplayName = displayName;
        public byte SmallId = smallId;
        public ulong LongId = longId;
    }
}