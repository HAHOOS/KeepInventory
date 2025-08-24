using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BoneLib;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.VRMK;

using KeepInventory.Helper;
using KeepInventory.Managers;
using KeepInventory.Menu;
using KeepInventory.Saves.V2;

using MelonLoader.Pastel;

using UnityEngine;

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
                KeepInventory.Fusion.Managers.ShareManager.Setup();
            }
            catch (Exception ex)
            {
                Core.FailedFLLoad = true;
                Core.Logger.Error("An unexpected error has occurred while setting up and/or loading the fusion module", ex);
            }
        }

        internal static void Setup()
        {
            KeepInventory.Fusion.Managers.ShareManager.OnShared += (save, sender) =>
            {
                if (LabFusion.Entities.NetworkPlayerManager.TryGetPlayer(sender, out LabFusion.Entities.NetworkPlayer player))
                {
                    if (!SaveManager.Saves.Any(x => x.ID == save.ID) && LabFusion.Network.MetadataHelper.TryGetDisplayName(player.PlayerID, out string name))
                    {
                        LabFusion.UI.Popups.Notifier.Send(new LabFusion.UI.Popups.Notification()
                        {
                            Title = "KeepInventory | Save shared!",
                            SaveToMenu = true,
                            ShowPopup = true,
                            Message = $"{name} has shared a save with you called '<color=#{save.DrawingColor.ToHEX()}>{save.Name}</color>'. Go to the LabFusion notifications menu, press accept to add save, decline will disregard this",
                            PopupLength = 15f,
                            Type = LabFusion.UI.Popups.NotificationType.INFORMATION,
                            OnAccepted = () => SaveManager.RegisterSave(save),
                            OnDeclined = () => Core.Logger.Msg("Save share ignored")
                        });
                    }
                }
            };
            LabFusion.Utilities.MultiplayerHooking.OnStartedServer += OnJoinLeave;
            LabFusion.Utilities.MultiplayerHooking.OnJoinedServer += OnJoinLeave;
            LabFusion.Utilities.MultiplayerHooking.OnDisconnected += OnJoinLeave;
        }

        public static bool SharingEnabled
        {
            get
            {
                if (Core.IsFusionLibraryInitialized && Core.HasFusion)
                    return Internal_SharingEnabled();
                else
                    return false;
            }
        }

        private static bool Internal_SharingEnabled()
            => KeepInventory.Fusion.Managers.ShareManager.Entry_SharingEnabled?.Value ?? false;

        private static void OnJoinLeave()
        {
            BoneMenu.SetupSaves();
        }

        internal static bool Internal_IsConnected()
        {
            return LabFusion.Network.NetworkInfo.HasServer;
        }

        internal static void Internal_ShareSave(byte SmallID, Save save)
        {
            if (LabFusion.Entities.NetworkPlayerManager.TryGetPlayer(SmallID, out LabFusion.Entities.NetworkPlayer plr))
                KeepInventory.Fusion.Managers.ShareManager.Share(save, plr.PlayerID);
            else
                throw new Exception($"Player with small ID {SmallID} could not be found");
        }

        public static void ShareSave(byte SmallID, Save save)
        {
            if (Core.HasFusion && Core.IsFusionLibraryInitialized && IsConnected) Internal_ShareSave(SmallID, save);
        }

        internal static List<FusionPlayer> Internal_GetShareablePlayers()
        {
            var plrs = KeepInventory.Fusion.Managers.ShareManager.GetAllShareablePlayers();
            List<FusionPlayer> players = [];
            foreach (var plr in plrs)
            {
                var player = LabFusion.Player.PlayerIDManager.GetPlayerID(plr);
                if (player == null) continue;
                else if (LabFusion.Network.MetadataHelper.TryGetDisplayName(player, out string name))
                    players.Add(new FusionPlayer(plr, player.PlatformID, name));
            }
            return players;
        }

        public static List<FusionPlayer> GetShareablePlayers()
        {
            if (Core.HasFusion && Core.IsFusionLibraryInitialized && IsConnected)
                return Internal_GetShareablePlayers();
            else
                return [];
        }

        internal static IEnumerable<FusionPlayer> Internal_GetPlayers()
        {
            foreach (var player in LabFusion.Entities.NetworkPlayer.Players)
            {
                yield return new FusionPlayer(player.PlayerID.SmallID, player.PlayerID.PlatformID, player.Username);
            }
        }

        public static List<FusionPlayer> GetPlayers()
        {
            if (Core.HasFusion && IsConnected) return [.. Internal_GetPlayers()];
            else return [];
        }

        internal static byte Internal_GetLocalPlayerSmallID()
            => LabFusion.Player.PlayerIDManager.LocalSmallID;

        public static byte GetLocalPlayerSmallID()
        {
            if (Core.HasFusion && IsConnected) return Internal_GetLocalPlayerSmallID();
            else return 0;
        }

        public static bool IsLocalPlayer(this RigManager rigManager)
        {
            if (!IsConnected) return true;
            else return Internal_IsLocalPlayer(rigManager);
        }

        private static bool Internal_IsLocalPlayer(this RigManager rigManager)
            => LabFusion.Utilities.FusionPlayer.IsLocalPlayer(rigManager);

        internal static bool GamemodeCheck()
        {
            if (!IsConnected) return false;
            else return Internal_GamemodeCheck();
        }

        internal static bool Internal_GamemodeCheck()
            => LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeStarted
            || LabFusion.SDK.Gamemodes.GamemodeManager.StartTimerActive
            || LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeReady;

        public static bool IsGamemodeStarted
        {
            get
            {
                if (!IsConnected) return false;
                else return Internal_IsGamemodeStarted();
            }
        }

        internal static void TargetLevelLoadEvent(Action action)
            => LabFusion.Utilities.MultiplayerHooking.OnTargetLevelLoaded += () => action.Invoke();

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

        internal static void Fusion_LoadMagazine(this Gun gun, int rounds = -1, Action callback = null)
        {
            LabFusion.RPC.NetworkAssetSpawner.Spawn(new LabFusion.RPC.NetworkAssetSpawner.SpawnRequestInfo
            {
                Position = Player.Head.position,
                Rotation = Player.Head.rotation,
                Spawnable = gun.defaultMagazine.spawnable,
                SpawnEffect = false,
                SpawnCallback = (spawn) =>
                {
                    var gunExt = LabFusion.Marrow.Extenders.GunExtender.Cache.Get(gun);
                    if (gunExt == null)
                        return;

                    var socketExtender = gunExt.GetExtender<LabFusion.Entities.AmmoSocketExtender>();
                    var mag = spawn.Spawned.GetComponent<Magazine>();

                    if (socketExtender == null || mag == null)
                        return;

                    var otherPlug = socketExtender?.Component?._magazinePlug;

                    if (otherPlug != null && otherPlug != mag.magazinePlug)
                        LabFusion.Extensions.AlignPlugExtensions.ForceEject(otherPlug);

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
            MsgPrefix("Fusion", message, UnityEngine.Color.cyan);
        }

        internal static void Warn(string message)
        {
            Core.Logger.Warning($"[Fusion] {message}");
        }

        internal static void Error(string message)
        {
            Core.Logger.Warning($"[Fusion] {message}");
        }

        internal static void MsgPrefix(string prefix, string message, UnityEngine.Color color)
        {
            Core.Logger.Msg($"[{prefix.Pastel(color.ToHEX())}] {message}");
        }
    }

    public class FusionPlayer(byte SmallID, ulong PlatformID, string displayName)
    {
        public string DisplayName { get; } = displayName;
        public byte SmallID { get; } = SmallID;
        public ulong PlatformID { get; } = PlatformID;
    }
}