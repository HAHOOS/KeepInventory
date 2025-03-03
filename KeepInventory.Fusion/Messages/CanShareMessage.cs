using System;
using System.Linq;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace KeepInventory.Fusion.Messages
{
    /// <summary>
    /// Data for the <see cref="CanShareRequestMessage"/> and <see cref="CanShareResponseMessage"/> messages
    /// </summary>
    public class CanShareMessageData : IFusionSerializable
    {
        /// <summary>
        /// The player sending the message
        /// </summary>
        public PlayerId Sender;

        /// <summary>
        /// The player to respond to
        /// </summary>
        public PlayerId Target;

        /// <summary>
        /// ID of the message
        /// </summary>
        public string ID;

        /// <summary>
        /// Deserialize from a <see cref="FusionReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="FusionReader"/></param>
        public void Deserialize(FusionReader reader)
        {
            Sender = PlayerIdManager.GetPlayerId(reader.ReadByte());

            byte? target = reader.ReadByteNullable();
            if (target != null)
                Target = PlayerIdManager.GetPlayerId((byte)target);

            ID = reader.ReadString();
        }

        /// <summary>
        /// Serialize to a <see cref="FusionWriter"/>
        /// </summary>
        /// <param name="writer">The <see cref="FusionWriter"/></param>
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
            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
        }

        /// <summary>
        /// Create a <see cref="CanShareMessageData"/> with local player as sender
        /// </summary>
        /// <param name="ID">The ID of the message</param>
        /// <returns><see cref="CanShareMessageData"/> with local player as sender and the provided ID</returns>
        public static CanShareMessageData Create(string ID)
        {
            return new CanShareMessageData()
            {
                ID = ID,
                Sender = PlayerIdManager.LocalId
            };
        }

        /// <summary>
        /// Create a <see cref="CanShareMessageData"/> with local player as sender
        /// </summary>
        /// <param name="target">The player to respond to</param>
        /// <param name="ID">The ID of the message</param>
        /// <returns><see cref="CanShareMessageData"/> with local player as sender and the provided ID and Target</returns>
        public static CanShareMessageData Create(byte target, string ID)
        {
            return new CanShareMessageData()
            {
                ID = ID,
                Target = PlayerIdManager.GetPlayerId(target),
                Sender = PlayerIdManager.LocalId
            };
        }

        /// <summary>
        /// Create a <see cref="CanShareMessageData"/> with generated ID and local player as sender
        /// </summary>
        /// <returns><see cref="CanShareMessageData"/> with local player as sender and generated ID</returns>
        public static CanShareMessageData Create()
        {
            return new CanShareMessageData()
            {
                ID = $"{PlayerIdManager.LocalSmallId}-{GenerateRandomID(7)}",
                Sender = PlayerIdManager.LocalId
            };
        }
    }

    /// <summary>
    /// Message responsible for checking players that you can share the save with
    /// </summary>
    public class CanShareRequestMessage : ModuleMessageHandler
    {
        /// <inheritdoc cref="HandleMessage(byte[], bool)"/>
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

    /// <summary>
    /// Message sent as a response to <see cref="CanShareRequestMessage"/>
    /// </summary>
    public class CanShareResponseMessage : ModuleMessageHandler
    {
        /// <summary>
        /// The last time a <see cref="CanShareResponseMessage"/> message was sent
        /// </summary>
        public static DateTime LastMessage { get; private set; } = DateTime.Now;

        /// <inheritdoc cref="HandleMessage(byte[], bool)"/>
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