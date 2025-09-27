// Ignore Spelling: serializer

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
        private byte _sender;

        // I have no clue how to resolve this warning, tried everything, so I'm just gonna disable it here
#pragma warning disable S2292 // Trivial properties should be auto-implemented

        public byte Sender
        {
            get => _sender;
            set => _sender = value;
        }

        private string _data;

        public string Data
        {
            get => _data;
            set => _data = value;
        }

#pragma warning restore S2292 // Trivial properties should be auto-implemented

        public Save Save => JsonSerializer.Deserialize<Save>(Data, SaveManager.SerializeOptions);

        public void Serialize(INetSerializer serializer)
        {
            serializer.SerializeValue(ref _sender);
            serializer.SerializeValue(ref _data);
        }

        public static ShareSaveMessageData Create(byte sender, string data)
            => new() { Sender = sender, _data = data };
    }

    public class ShareSaveMessage : ModuleMessageHandler
    {
        protected override void OnHandleMessage(ReceivedMessage received)
        {
            var message = received.ReadData<ShareSaveMessageData>();
            if (message == null)
            {
                FusionModule.MsgFusionPrefix("[ShareSave] The received message could not be read or was null");
                return;
            }

            if (!PlayerIDManager.SmallIDLookup.TryGetValue(message.Sender, out PlayerID id) || id == null)
                return;

            if (!ShareManager.IsPlayerAllowed(id))
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

            ShareManager.TriggerEvent(save, message.Sender);
        }
    }
}