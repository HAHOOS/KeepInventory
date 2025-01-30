using System;
using System.Text.Json;

using KeepInventory.Saves.V2;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace KeepInventory.Fusion.Messages
{
    public class ShareSaveMessageData : IFusionSerializable
    {
        public PlayerId Sender;
        public PlayerId Target;

        public string Data;

        public Save Save => JsonSerializer.Deserialize<Save>(Data, SaveManager.SerializeOptions);

        public void Deserialize(FusionReader reader)
        {
            Sender = PlayerIdManager.GetPlayerId(reader.ReadByte());
            Target = PlayerIdManager.GetPlayerId(reader.ReadByte());
            Data = reader.ReadString();
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(Sender.SmallId);
            writer.Write(Target.SmallId);
            writer.Write(Data);
        }
    }

    public class ShareSaveMessage : ModuleMessageHandler
    {
        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (isServerHandled)
                return;

            using var reader = FusionReader.Create(bytes);
            var message = reader.ReadFusionSerializable<ShareSaveMessageData>();
            if (message == null)
            {
                FusionModule.MsgFusionPrefix("[ShareSave] The received message could not be read or was null");
                return;
            }

            if (message.Target.SmallId != PlayerIdManager.LocalSmallId)
            {
                FusionModule.MsgFusionPrefix("[ShareSave] The target player is not the local player, ignoring");
                return;
            }

            if (ShareManager.Entry_SharingBlacklist?.Value.Contains(message.Target.LongId) == true)
            {
                FusionModule.MsgFusionPrefix("[ShareSave] A blacklisted player tried to share a save with you");
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