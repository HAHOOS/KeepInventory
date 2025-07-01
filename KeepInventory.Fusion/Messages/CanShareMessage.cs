using System;
using System.Linq;

using KeepInventory.Fusion.Managers;

using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace KeepInventory.Fusion.Messages
{
    public class CanShareMessageData : INetSerializable
    {
        public byte Sender;
        public string ID;

        public void Serialize(INetSerializer writer)
        {
            writer.SerializeValue(ref Sender);
            writer.SerializeValue(ref ID);
        }

        public static string GenerateRandomID(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
        }

        public static CanShareMessageData Create(string ID)
        {
            return new CanShareMessageData()
            {
                ID = ID,
                Sender = PlayerIDManager.LocalSmallID,
            };
        }

        public static CanShareMessageData Create()
        {
            return new CanShareMessageData()
            {
                ID = $"{PlayerIDManager.LocalSmallID}-{GenerateRandomID(7)}",
                Sender = PlayerIDManager.LocalID
            };
        }
    }

    public class CanShareRequestMessage : ModuleMessageHandler
    {
        protected override void OnHandleMessage(ReceivedMessage message)
        {
            var msg = message.ReadData<CanShareMessageData>();
            if (msg == null)
                return;

            FusionModule.logger.Log("[REQUEST] Msg is not null");

            if (!PlayerIDManager.SmallIDLookup.TryGetValue(msg.Sender, out PlayerID id) || id == null)
                return;

            FusionModule.logger.Log("[REQUEST] Sender is not me");

            if (!ShareManager.IsPlayerAllowed(id))
                return;

            FusionModule.logger.Log("[REQUEST] Sender is allowed");

            var responseData = CanShareMessageData.Create(msg.ID);

            using var writer = NetWriter.Create();
            writer.SerializeValue(ref responseData);
            using var _message = NetMessage.ModuleCreate<CanShareResponseMessage>(writer, new(msg.Sender, NetworkChannel.Reliable));
            MessageSender.SendToServer(NetworkChannel.Reliable, _message);
        }
    }

    public class CanShareResponseMessage : ModuleMessageHandler
    {
        public static DateTime LastMessage { get; internal set; } = DateTime.Now;

        protected override void OnHandleMessage(ReceivedMessage message)
        {
            var msg = message.ReadData<CanShareMessageData>();
            if (msg == null)
                return;

            FusionModule.logger.Log("[RESPONSE] Msg not null");

            if (!PlayerIDManager.SmallIDLookup.TryGetValue(msg.Sender, out PlayerID id) || id == null)
                return;

            FusionModule.logger.Log("[RESPONSE] Sender is not me");

            if (string.IsNullOrWhiteSpace(ShareManager.AwaitingID) || msg.ID != ShareManager.AwaitingID)
                return;

            FusionModule.logger.Log("[RESPONSE] Awaiting ID is not empty");

            if (ShareManager.PlayerResponses.Contains(msg.Sender))
                return;

            FusionModule.logger.Log("[RESPONSE] Player Responses does not contain sender");

            if (!ShareManager.IsPlayerAllowed(id))
                return;

            FusionModule.logger.Log("[RESPONSE] Player is allowed");

            LastMessage = DateTime.Now;

            ShareManager.PlayerResponses.Add(msg.Sender);
        }
    }
}