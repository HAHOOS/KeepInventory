using System;
using System.Linq;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace KeepInventory.Fusion.Messages
{
    public class CanShareMessageData : IFusionSerializable
    {
        public PlayerId Sender;
        public PlayerId Target;
        public string ID;

        public void Deserialize(FusionReader reader)
        {
            Sender = PlayerIdManager.GetPlayerId(reader.ReadByte());

            byte? target = reader.ReadByteNullable();
            if (target != null)
                Target = PlayerIdManager.GetPlayerId((byte)target);

            ID = reader.ReadString();
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(Sender.SmallId);
            byte? smallId = Target?.SmallId;
            writer.Write(smallId);
            writer.Write(ID);
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
                Sender = PlayerIdManager.LocalId
            };
        }

        public static CanShareMessageData Create(byte target, string ID)
        {
            return new CanShareMessageData()
            {
                ID = ID,
                Target = PlayerIdManager.GetPlayerId(target),
                Sender = PlayerIdManager.LocalId
            };
        }

        public static CanShareMessageData Create()
        {
            return new CanShareMessageData()
            {
                ID = $"{PlayerIdManager.LocalSmallId}-{GenerateRandomID(7)}",
                Sender = PlayerIdManager.LocalId
            };
        }
    }

    public class CanShareRequestMessage : ModuleMessageHandler
    {
        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (isServerHandled)
            {
                using var _msg = FusionMessage.ModuleCreate<CanShareRequestMessage>(bytes);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, _msg);
            }

            FusionModule.logger.Log("[REQUEST] Request received");

            using var reader = FusionReader.Create(bytes);
            var msg = reader.ReadFusionSerializable<CanShareMessageData>();
            if (msg == null)
                return;

            FusionModule.logger.Log("[REQUEST] Msg is not null");

            if (!msg.Sender.IsValid || msg.Sender.IsMe)
                return;

            FusionModule.logger.Log("[REQUEST] Sender is not me");

            if (!ShareManager.IsPlayerAllowed(msg.Sender))
                return;

            FusionModule.logger.Log("[REQUEST] Sender is allowed");

            var responseData = CanShareMessageData.Create(msg.Sender.SmallId, msg.ID);

            using var writer = FusionWriter.Create();
            writer.Write(responseData);
            using var message = FusionMessage.ModuleCreate<CanShareResponseMessage>(writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }

    public class CanShareResponseMessage : ModuleMessageHandler
    {
        public static DateTime LastMessage { get; internal set; } = DateTime.Now;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (isServerHandled)
            {
                using var _msg = FusionMessage.ModuleCreate<CanShareResponseMessage>(bytes);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, _msg);
            }

            FusionModule.logger.Log("[RESPONSE] Response received");

            using var reader = FusionReader.Create(bytes);
            var msg = reader.ReadFusionSerializable<CanShareMessageData>();
            if (msg == null)
                return;

            FusionModule.logger.Log("[RESPONSE] Msg not null");

            if (!msg.Sender.IsValid || msg.Sender.IsMe)
                return;

            FusionModule.logger.Log("[RESPONSE] Sender is not me");

            if (string.IsNullOrWhiteSpace(ShareManager.AwaitingID) || msg.ID != ShareManager.AwaitingID)
                return;

            FusionModule.logger.Log("[RESPONSE] Awaiting ID is not empty");

            if (ShareManager.PlayerResponses.Contains(msg.Sender.SmallId))
                return;

            FusionModule.logger.Log("[RESPONSE] Player Responses does not contain sender");

            if (!ShareManager.IsPlayerAllowed(msg.Sender))
                return;

            FusionModule.logger.Log("[RESPONSE] Player is allowed");

            LastMessage = DateTime.Now;

            ShareManager.PlayerResponses.Add(msg.Sender.SmallId);
        }
    }
}