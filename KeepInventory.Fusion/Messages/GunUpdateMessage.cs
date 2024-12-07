using Il2CppSLZ.Marrow;

using KeepInventory.Helper;
using KeepInventory.Saves;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Modules;

using MelonLoader.Pastel;

using System.Drawing;

namespace KeepInventory.Fusion.Messages
{
    /// <summary>
    /// Handles the messages containing <see cref="GunMessageData"/>
    /// <para>Changes data of a specified gun to provided <see cref="GunInfo"/> for all clients</para>
    /// </summary>
    public class GunUpdateMessage : ModuleMessageHandler
    {
        /// <summary>
        /// Handles the received message
        /// </summary>
        /// <param name="bytes">Received bytes</param>
        /// <param name="isServerHandled">Honestly, I have no clue, you shouldn't be running this anyway</param>
        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            FusionReader reader = null;
            try
            {
                FusionMethods.MsgFusionPrefix($"[{"GunUpdate".Pastel(Color.Red)}] Received GunUpdateMessage");
                reader = FusionReader.Create(bytes);
                GunMessageData gunMessageData = reader.ReadFusionSerializable<GunMessageData>();
                if (gunMessageData == null || gunMessageData.GunInfo == null)
                {
                    FusionMethods.Warn("[GunUpdate] Some values were null, cannot load");
                    return;
                }
                /*
                if (NetworkInfo.IsServer && isServerHandled)
                {
                    FusionMessage msg = FusionMessage.ModuleCreate<GunUpdateMessage>(bytes);
                    try
                    {
                        FusionMethods.MsgFusionPrefix($"[{"GunUpdate".Pastel(Color.Red)}] Broadcasting the GunUpdateMessage to other players");
                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, msg);
                    }
                    finally
                    {
                        msg?.Dispose();
                    }
                }
                */
                if (NetworkEntityManager.IdManager.RegisteredEntities.IdEntityLookup.TryGetValue(gunMessageData.GunID, out NetworkEntity entity))
                {
                    FusionMethods.MsgFusionPrefix($"[{"GunUpdate".Pastel(Color.Red)}] Found entity in GunUpdateMessage");
                    var extender = entity.GetExtender<GunExtender>();
                    if (extender != null)
                    {
                        FusionMethods.MsgFusionPrefix($"[{"GunUpdate".Pastel(Color.Red)}] Found extender, updating properties");
                        if (extender.Components == null || extender.Components.Length == 0)
                        {
                            FusionMethods.Warn("[GunUpdate] No components were found with the extender");
                            return;
                        }
                        // Just in case going through all of components
                        foreach (var gun in extender.Components)
                        {
                            gun.UpdateProperties(gunMessageData.GunInfo, System.Drawing.Color.White, printMessages: false, fusionMessage: true);
                        }
                    }
                    else
                    {
                        FusionMethods.Warn("[GunUpdate] Extender was not found");
                    }
                }
                else
                {
                    FusionMethods.Warn("[GunUpdate] Entity was not found");
                }
            }
            finally
            {
                reader?.Dispose();
            }
        }
    }

    /// <summary>
    /// Contains all the necessary information for <see cref="GunUpdateMessage"/>
    /// </summary>
    public class GunMessageData : IFusionSerializable
    {
        /// <summary>
        /// Player that is sending the message
        /// </summary>
        public PlayerId PlayerId { get; set; }

        /// <summary>
        /// The gun info to be used to modify the data regarding a specified gun
        /// </summary>
        public GunInfo GunInfo { get; set; }

        /// <summary>
        /// Entity ID of the gun that should be modified
        /// </summary>
        public ushort GunID { get; set; }

        /// <summary>
        /// Deserialize's the message so that <see cref="GunUpdateMessage"/> can read the data correctly
        /// </summary>
        /// <param name="reader">Reader</param>
        public void Deserialize(FusionReader reader)
        {
            PlayerId = PlayerIdManager.GetPlayerId(reader.ReadByte());
            GunID = reader.ReadUInt16();
            GunInfo = GunInfo.Deserialize(reader.ReadString());
        }

        /// <summary>
        /// Serializes the message to be sent to the server
        /// </summary>
        /// <param name="writer">Writer</param>
        public void Serialize(FusionWriter writer)
        {
            writer.Write(PlayerId.SmallId);
            writer.Write(GunID);
            writer.Write(GunInfo.Serialize());
        }

        /// <summary>
        /// Creates new instance of <see cref="GunMessageData"/>
        /// </summary>
        /// <param name="gunInfo"><inheritdoc cref="GunInfo"/></param>
        /// <param name="gunID"><inheritdoc cref="GunID"/></param>
        /// <returns></returns>
        public static GunMessageData Create(GunInfo gunInfo, ushort gunID)
        {
            return new GunMessageData()
            {
                PlayerId = PlayerIdManager.LocalId,
                GunInfo = gunInfo,
                GunID = gunID
            };
        }

        /// <summary>
        /// Creates new instance of <see cref="GunMessageData"/>
        /// </summary>
        /// <param name="gun"><see cref="Gun"/> to generate the GunInfo from</param>
        /// <param name="gunID"><inheritdoc cref="GunID"/></param>
        public static GunMessageData Create(Gun gun, ushort gunID)
        {
            return new GunMessageData()
            {
                PlayerId = PlayerIdManager.LocalId,
                GunInfo = GunInfo.Parse(gun),
                GunID = gunID
            };
        }

        /// <summary>
        /// Creates new instance of <see cref="GunMessageData"/>
        /// </summary>
        /// <param name="gun"><see cref="Gun"/> to generate the GunInfo from. The entity ID will be automatically found</param>
        public static GunMessageData Create(Gun gun)
        {
            var entity = GunExtender.Cache.Get(gun);
            if (entity?.IsOwner != true)
            {
                return null;
            }

            return new GunMessageData()
            {
                PlayerId = PlayerIdManager.LocalId,
                GunInfo = GunInfo.Parse(gun),
                GunID = entity.Id
            };
        }

        /// <summary>
        /// Creates new instance of <see cref="GunMessageData"/>
        /// </summary>
        /// <param name="gun">The entity ID will be automatically found from the provided <see cref="Gun"/></param>
        /// <param name="gunInfo"><inheritdoc cref="GunInfo"/></param>
        public static GunMessageData Create(Gun gun, GunInfo gunInfo)
        {
            var entity = GunExtender.Cache.Get(gun);
            if (entity?.IsOwner != true)
            {
                return null;
            }

            return new GunMessageData()
            {
                PlayerId = PlayerIdManager.LocalId,
                GunInfo = gunInfo,
                GunID = entity.Id
            };
        }
    }
}