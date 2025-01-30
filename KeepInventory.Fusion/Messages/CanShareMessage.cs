using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace KeepInventory.Fusion.Messages
{
    public class CanShareMessageData : IFusionSerializable
    {
        public PlayerId Sender;

        public PlayerId? Target;

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

        /// <summary>
        /// Generates a string with random characters
        /// </summary>
        /// <param name="length">Length of string</param>
        /// <returns>A random string with specified length</returns>
        public static string GenerateRandomID(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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
                throw new Exception("This message is supposed to be broadcasted, not sent to server");

            using var reader = FusionReader.Create(bytes);
            var msg = reader.ReadFusionSerializable<CanShareMessageData>();
            if (msg == null)
                return;

            if (!msg.Sender.IsValid || msg.Sender.IsMe)
                return;

            if (!ShareManager.IsPlayerAllowed(msg.Sender))
                return;

            var responseData = CanShareMessageData.Create(msg.Sender.SmallId, msg.ID);

            using var writer = FusionWriter.Create();
            writer.Write(responseData);
            using var message = FusionMessage.ModuleCreate<CanShareResponseMessage>(bytes);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
        }
    }

    public class CanShareResponseMessage : ModuleMessageHandler
    {
        public static DateTime LastMessage = DateTime.Now;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (isServerHandled)
                throw new Exception("This message is supposed to be broadcasted, not sent to server");

            using var reader = FusionReader.Create(bytes);
            var msg = reader.ReadFusionSerializable<CanShareMessageData>();
            if (msg == null)
                return;

            if (!msg.Target.IsValid || !msg.Target.IsMe)
                return;

            if (!msg.Sender.IsValid || msg.Sender.IsMe)
                return;

            if (string.IsNullOrWhiteSpace(ShareManager.AwaitingID) || msg.ID != ShareManager.AwaitingID)
                return;

            if (ShareManager.PlayerResponses.Contains(msg.Sender.SmallId))
                return;

            if (!ShareManager.IsPlayerAllowed(msg.Sender))
                return;

            LastMessage = DateTime.Now;

            ShareManager.PlayerResponses.Add(msg.Sender.SmallId);
        }
    }
}