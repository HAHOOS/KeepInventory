using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using KeepInventory.Fusion.Messages;
using KeepInventory.Saves.V2;

using LabFusion.Network;
using LabFusion.Player;

using MelonLoader;

namespace KeepInventory.Fusion
{
    /// <summary>
    /// Class that lets you easily share and manage the sharing of <see cref="Save"/>s
    /// </summary>
    public static class ShareManager
    {
        /// <summary>
        /// Event that gets called when a save is shared
        /// </summary>
        public static event Action<Save, byte> OnShared;

        /// <summary>
        /// Category for <see cref="MelonPreferences_Category"/>
        /// </summary>
        public static MelonPreferences_Category Category { get; private set; }

        /// <summary>
        /// An entry that decides if you can share with others or others can share with you saves
        /// </summary>
        public static MelonPreferences_Entry<bool> Entry_SharingEnabled { get; private set; }

        /// <summary>
        /// List of long IDs of players that you do not wish for them to share saves with you
        /// </summary>
        public static MelonPreferences_Entry<List<ulong>> Entry_SharingBlacklist { get; private set; }

        private static bool IsSetup;

        internal static List<byte> PlayerResponses = [];

        internal static string AwaitingID = string.Empty;

        /// <summary>
        /// Setup the preferences for sharing
        /// </summary>
        public static void Setup()
        {
            Category = MelonPreferences.CreateCategory("KeepInventory_Sharing");
            Entry_SharingEnabled = Category.CreateEntry<bool>("Enabled", true, "Enabled",
                description: "Can people share saves to you and can you share.");
            Entry_SharingBlacklist = Category.CreateEntry<List<ulong>>("Blacklist", [], "Blacklist",
                description: "List of long IDs of players that you do not want to allow sharing saves from");
            Category.SetFilePath(Path.Combine(Core.KI_PreferencesDirectory, "Sharing.cfg"));
            Category.SaveToFile(false);
            IsSetup = true;
        }

        /// <summary>
        /// Share a <see cref="Save"/> with a specified player
        /// </summary>
        /// <param name="save"><see cref="Save"/> to share</param>
        /// <param name="target">The player to share the save with</param>
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

        /// <summary>
        /// Verify that the data received from <see cref="ShareSaveMessage"/> is valid
        /// </summary>
        /// <param name="data">Data to check</param>
        /// <returns><see langword="true"/> if valid, otherwise <see langword="false"/></returns>
        public static bool VerifyData(string data)
        {
            return !string.IsNullOrWhiteSpace(data) && IsJson(data);
        }

        /// <summary>
        /// Checks if a player is allowed to share a save with you
        /// </summary>
        /// <param name="playerId">The player to check</param>
        /// <returns><see langword="true"/> if allowed, otherwise <see langword="false"/></returns>
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

        private const int Timeout = 200;

        /// <summary>
        /// Get all players that you can share a save with
        /// </summary>
        /// <returns>A list of Small IDs of players</returns>
        public async static Task<List<byte>> GetAllShareablePlayers()
        {
            if (!IsSetup)
                return [];

            if (Entry_SharingEnabled?.Value == false)
                return [];

            if (Entry_SharingBlacklist.Value.TrueForAll(x =>
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

            PlayerResponses.Clear();

            var data = CanShareMessageData.Create();
            AwaitingID = data.ID;

            using var writer = FusionWriter.Create();
            writer.Write(data);
            using var message = FusionMessage.ModuleCreate<CanShareRequestMessage>(writer);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);

            while ((DateTime.Now - CanShareResponseMessage.LastMessage).TotalMilliseconds < Timeout) await Task.Delay(50);
            var resp = new List<byte>(PlayerResponses);
            PlayerResponses.Clear();
            AwaitingID = string.Empty;
            return resp;
        }
    }
}