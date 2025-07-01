using System;

using BoneLib;

using Il2CppSLZ.Marrow;

using KeepInventory.Saves.V2;
using System.Collections.Generic;
using System.Linq;
using Il2CppSLZ.Marrow.Data;
using MelonLoader.Pastel;
using System.Threading.Tasks;
using KeepInventory.Managers;
using KeepInventory.Helper;

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
                Core.Logger.Error($"An unexpected error has occurred while setting up and/or loading the fusion module, exception:\n{ex}");
            }
        }

        internal static void Setup()
        {
            KeepInventory.Fusion.Managers.ShareManager.OnShared += (save, sender) =>
            {
                if (LabFusion.Entities.NetworkPlayerManager.TryGetPlayer(sender, out LabFusion.Entities.NetworkPlayer player))
                {
                    if (!SaveManager.Saves.Any(x => x.ID == save.ID))
                    {
                        LabFusion.UI.Popups.Notifier.Send(new LabFusion.UI.Popups.Notification()
                        {
                            Title = "KeepInventory | Save shared!",
                            SaveToMenu = true,
                            ShowPopup = true,
                            Message = $"{player.Username} has shared a save with you called '<color=#{save.Color}>{save.Name}</color>'. Go to the LabFusion notifications menu, press accept to add save, decline will disregard this",
                            PopupLength = 15f,
                            Type = LabFusion.UI.Popups.NotificationType.INFORMATION,
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

        internal async static Task<List<FusionPlayer>> Internal_GetShareablePlayers()
        {
            var task = await KeepInventory.Fusion.Managers.ShareManager.GetAllShareablePlayers();
            List<FusionPlayer> players = [];
            foreach (var player in LabFusion.Entities.NetworkPlayer.Players)
            {
                if (player != null && task.Contains(player.PlayerID.SmallID))
                    players.Add(new FusionPlayer(player.PlayerID.SmallID, player.PlayerID.PlatformID, player.Username));
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
                if (PreferencesManager.ItemSaving.Value)
                {
                    if (Core.IsFusionLibraryInitialized) SpawnSavedItems_FSL(save);
                    else InventoryManager.SpawnSavedItems(save);
                }
            }
            else
            {
                Core.Logger.Msg("Client is not connected to a server, spawning locally");
                if (PreferencesManager.ItemSaving.Value)
                    InventoryManager.SpawnSavedItems(save);
            }
        }

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