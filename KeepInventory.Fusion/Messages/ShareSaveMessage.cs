using System.Text.Json;

using KeepInventory.Fusion.Managers;
using KeepInventory.Managers;
using KeepInventory.Saves.V2;

using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Modules;

using MelonLoader.Pastel;

namespace KeepInventory.Fusion.Messages
{
    public class ShareSaveMessageData : INetSerializable
    {
        public byte Sender;
        public string Data;
        public Save Save => JsonSerializer.Deserialize<Save>(Data, SaveManager.SerializeOptions);

        public void Serialize(INetSerializer writer)
        {
            writer.SerializeValue(ref Sender);
            writer.SerializeValue(ref Data);
        }
    }

    public class ShareSaveMessage : ModuleMessageHandler
    {
        protected override void OnHandleMessage(ReceivedMessage received)
        {
            var message = received.ReadData<ShareSaveMessageData>();
            if (message == null)
            {
                FusionModule.MsgFusionPrefix($"[{"ShareSave".Pastel(System.Drawing.Color.CornflowerBlue)}] The received message could not be read or was null");
                return;
            }

            if (!PlayerIDManager.SmallIDLookup.TryGetValue(message.Sender, out PlayerID id) || id == null)
                return;

            if (!ShareManager.IsPlayerAllowed(id))
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

            ShareManager.TriggerEvent(save, message.Sender);
        }
    }
}