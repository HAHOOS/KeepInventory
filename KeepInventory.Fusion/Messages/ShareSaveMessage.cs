using System.Text.Json;

using KeepInventory.Saves.V2;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Modules;

using MelonLoader.Pastel;

namespace KeepInventory.Fusion.Messages
{
    /// <summary>
    /// Data for <see cref="ShareSaveMessage"/>
    /// </summary>
    public class ShareSaveMessageData : IFusionSerializable
    {
        /// <summary>
        /// The sender of the message
        /// </summary>
        public PlayerId Sender;

        /// <summary>
        /// The target of the message
        /// </summary>
        public PlayerId Target;

        /// <summary>
        /// Serialized <see cref="Save"/>
        /// </summary>
        public string Data;

        /// <summary>
        /// <see cref="Saves.V2.Save"/> that should be shared
        /// </summary>
        public Save Save => JsonSerializer.Deserialize<Save>(Data, SaveManager.SerializeOptions);

        /// <summary>
        /// Deserialize from a <see cref="FusionReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="FusionReader"/></param>
        public void Deserialize(FusionReader reader)
        {
            Sender = PlayerIdManager.GetPlayerId(reader.ReadByte());
            Target = PlayerIdManager.GetPlayerId(reader.ReadByte());
            Data = reader.ReadString();
        }

        /// <summary>
        /// Serialize to a <see cref="FusionWriter"/>
        /// </summary>
        /// <param name="writer">The <see cref="FusionWriter"/></param>
        public void Serialize(FusionWriter writer)
        {
            writer.Write(Sender.SmallId);
            writer.Write(Target.SmallId);
            writer.Write(Data);
        }
    }

    /// <summary>
    /// Message responsible for sharing saves
    /// </summary>
    public class ShareSaveMessage : ModuleMessageHandler
    {
        /// <inheritdoc cref="HandleMessage(byte[], bool)"/>
        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (isServerHandled)
                return;

            using var reader = FusionReader.Create(bytes);
            var message = reader.ReadFusionSerializable<ShareSaveMessageData>();
            if (message == null)
            {
                FusionModule.MsgFusionPrefix($"[{"ShareSave".Pastel(System.Drawing.Color.CornflowerBlue)}] The received message could not be read or was null");
                return;
            }

            if (message.Target.SmallId != PlayerIdManager.LocalSmallId)
            {
                FusionModule.MsgFusionPrefix($"[{"ShareSave".Pastel(System.Drawing.Color.CornflowerBlue)}] The target player is not the local player, ignoring");
                return;
            }

            if (!ShareManager.IsPlayerAllowed(message.Sender))
            {
                FusionModule.MsgFusionPrefix($"[{"ShareSave".Pastel(System.Drawing.Color.CornflowerBlue)}] A blacklisted player tried to share a save with you");
                return;
            }

            if (!ShareManager.VerifyData(message.Data))
            {
                FusionModule.Warn("[ShareSave] The received data is not a valid save, ignoring");
                return;
            }

            var save = JsonSerializer.Deserialize<Save>(message.Data);

            ShareManager.TriggerEvent(save, message.Sender.SmallId);
        }
    }
}