using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using KeepInventory.Fusion.Messages;
using KeepInventory.Managers;
using KeepInventory.Saves.V2;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

using MelonLoader;
using MelonLoader.Utils;

namespace KeepInventory.Fusion.Managers
{
    public static class ShareManager
    {
        public static event Action<Save, byte> OnShared;

        public static MelonPreferences_Category Category { get; private set; }
        public static MelonPreferences_Entry<bool> Entry_SharingEnabled { get; private set; }
        public static MelonPreferences_Entry<List<ulong>> Entry_SharingBlacklist { get; private set; }

        private static bool IsSetup;

        internal static List<byte> PlayerResponses = [];

        internal static string AwaitingID = string.Empty;

        public static void Setup()
        {
            Category = MelonPreferences.CreateCategory("KeepInventory_Sharing");
            Entry_SharingEnabled = Category.CreateEntry("Enabled", true, "Enabled",
                description: "Can people share saves to you and can you share.");
            Entry_SharingBlacklist = Category.CreateEntry<List<ulong>>("Blacklist", [], "Blacklist",
                description: "List of long IDs of players that you do not want to allow sharing saves from");
            Category.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory", "Sharing.cfg"));
            Category.SaveToFile(false);
            IsSetup = true;

            MultiplayerHooking.OnStartServer += ()
                => LocalPlayer.Metadata.TrySetMetadata("HasKeepInventory", bool.TrueString);
        }

        public static void Share(Save save, PlayerId target)
        {
            if (!IsSetup)
                return;

            if (!Entry_SharingEnabled.Value)
                return;

            ShareSaveMessageData messageData = new()
            {
                Sender = PlayerIdManager.LocalId,
                Target = target,
                Data = JsonSerializer.Serialize(save, SaveManager.SerializeOptions)
            };
            using var writer = FusionWriter.Create();
            writer.Write(messageData);
            using FusionMessage msg = FusionMessage.ModuleCreate<ShareSaveMessage>(writer);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, msg);
        }

        private static bool IsJson(string data)
        {
            try
            {
                JsonSerializer.Deserialize<Save>(data, SaveManager.SerializeOptions);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool VerifyData(string data)
        {
            return !string.IsNullOrWhiteSpace(data) && IsJson(data);
        }

        public static bool IsPlayerAllowed(PlayerId playerId)
        {
            if (!IsSetup)
                return true;

            if (!Entry_SharingEnabled.Value)
                return false;

            if (Entry_SharingBlacklist.Value.Contains(playerId.LongId))
                return false;

            return true;
        }

        internal static void TriggerEvent(Save save, byte sender)
        {
            if (IsSetup && Entry_SharingEnabled?.Value == true)
                OnShared?.Invoke(save, sender);
        }

        private const int Timeout = 500;

        public async static Task<List<byte>> GetAllShareablePlayers()
        {
            if (!IsSetup)
                return [];

            FusionModule.logger.Log("Is set up");

            if (Entry_SharingEnabled?.Value == false)
                return [];

            FusionModule.logger.Log("Sharing enabled");

            if (Entry_SharingBlacklist.Value.Count > 0 && Entry_SharingBlacklist.Value.TrueForAll(x =>
            {
                var id = PlayerIdManager.GetPlayerId(x);
                if (id != null)
                {
                    if (Entry_SharingBlacklist.Value.Contains(id.LongId))
                        return true;
                }
                return false;
            }))
            {
                return [];
            }

            FusionModule.logger.Log("There are people not on the blacklist");

            PlayerResponses.Clear();

            var data = CanShareMessageData.Create();
            AwaitingID = data.ID;

            using var writer = FusionWriter.Create();
            writer.Write(data);
            using var message = FusionMessage.ModuleCreate<CanShareRequestMessage>(writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
            var last = DateTime.Now;
            CanShareResponseMessage.LastMessage = DateTime.Now;
            while ((DateTime.Now - CanShareResponseMessage.LastMessage).TotalMilliseconds < Timeout) await Task.Delay(50);
            var resp = new List<byte>(PlayerResponses);
            PlayerResponses.Clear();
            AwaitingID = string.Empty;
            return resp;
        }
    }
}