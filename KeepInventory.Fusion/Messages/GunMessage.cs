using Il2CppSLZ.Marrow;
using KeepInventory.Helper;
using KeepInventory.SaveSlot;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;

namespace KeepInventory.Fusion.Messages
{
    /// <summary>
    /// Handles the messages containing <see cref="GunMessageData"/>
    /// <para>Changes data of a specified gun to provided <see cref="GunInfo"/> for all clients</para>
    /// </summary>
    public class GunMessage : ModuleMessageHandler
    {
        /// <summary>
        /// Handles the received message
        /// </summary>
        /// <param name="bytes">Received bytes</param>
        /// <param name="isServerHandled">Honestly, I have no clue, you shouldn't be running this anyway</param>
        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            FusionReader reader = FusionReader.Create(bytes);
            try
            {
                GunMessageData gunMessageData = reader.ReadFusionSerializable<GunMessageData>();
                if (NetworkInfo.IsServer && isServerHandled)
                {
                    FusionMessage msg = FusionMessage.ModuleCreate<GunMessage>(bytes);
                    try
                    {
                        MessageSender.BroadcastMessageExcept(gunMessageData.playerId, 0, msg, true);
                    }
                    finally
                    {
                        msg.Dispose();
                    }
                }
                if (!gunMessageData.playerId.IsOwner && NetworkEntityManager.IdManager.RegisteredEntities.IdEntityLookup.TryGetValue(gunMessageData.gunID, out NetworkEntity entity))
                {
                    var extender = entity.GetExtender<GunExtender>();
                    if (extender != null)
                    {
                        // Just in case going through all of components
                        foreach (var gun in extender.Components)
                        {
                            gun.UpdateProperties(gunMessageData.gunInfo, printMessages: false, fusionMessage: true);
                        }
                    }
                }
            }
            finally
            {
                reader.Dispose();
            }
        }
    }

    /// <summary>
    /// Contains all the necessary information for <see cref="GunMessage"/>
    /// </summary>
    public class GunMessageData : IFusionSerializable
    {
        /// <summary>
        /// Player that is sending the message
        /// </summary>
        public PlayerId playerId { get; set; }

        /// <summary>
        /// The gun info to be used to modify the data regarding a specified gun
        /// </summary>
        public GunInfo gunInfo { get; set; }

        /// <summary>
        /// Entity ID of the gun that should be modified
        /// </summary>
        public ushort gunID { get; set; }

        /// <summary>
        /// Deserialize's the message so that <see cref="GunMessage"/> can read the data correctly
        /// </summary>
        /// <param name="reader">Reader</param>
        public void Deserialize(FusionReader reader)
        {
            playerId = PlayerIdManager.GetPlayerId(reader.ReadByte());
            gunID = reader.ReadUInt16();
            gunInfo = GunInfo.Deserialize(reader.ReadString());
        }

        /// <summary>
        /// Serializes the message to be sent to the server
        /// </summary>
        /// <param name="writer">Writer</param>
        public void Serialize(FusionWriter writer)
        {
            writer.Write(playerId.SmallId);
            writer.Write(gunID);
            writer.Write(gunInfo.Serialize());
        }

        /// <summary>
        /// Creates new instance of <see cref="GunMessageData"/>
        /// </summary>
        /// <param name="gunInfo"><inheritdoc cref="gunInfo"/></param>
        /// <param name="gunID"><inheritdoc cref="gunID"/></param>
        /// <returns></returns>
        public static GunMessageData Create(GunInfo gunInfo, ushort gunID)
        {
            return new GunMessageData()
            {
                playerId = PlayerIdManager.LocalId,
                gunInfo = gunInfo,
                gunID = gunID
            };
        }

        /// <summary>
        /// Creates new instance of <see cref="GunMessageData"/>
        /// </summary>
        /// <param name="gun"><see cref="Gun"/> to generate the GunInfo from</param>
        /// <param name="gunID"><inheritdoc cref="gunID"/></param>
        public static GunMessageData Create(Gun gun, ushort gunID)
        {
            return new GunMessageData()
            {
                playerId = PlayerIdManager.LocalId,
                gunInfo = GunInfo.Parse(gun),
                gunID = gunID
            };
        }

        /// <summary>
        /// Creates new instance of <see cref="GunMessageData"/>
        /// </summary>
        /// <param name="gun"><see cref="Gun"/> to generate the GunInfo from. The entity ID will be automatically found</param>
        public static GunMessageData Create(Gun gun)
        {
            var entity = GunExtender.Cache.Get(gun);
            if (entity != null || !entity.IsOwner)
            {
                return null;
            }

            return new GunMessageData()
            {
                playerId = PlayerIdManager.LocalId,
                gunInfo = GunInfo.Parse(gun),
                gunID = entity.Id
            };
        }

        /// <summary>
        /// Creates new instance of <see cref="GunMessageData"/>
        /// </summary>
        /// <param name="gun">The entity ID will be automatically found from the provided <see cref="Gun"/></param>
        /// <param name="gunInfo"><inheritdoc cref="gunInfo"/></param>
        public static GunMessageData Create(Gun gun, GunInfo gunInfo)
        {
            var entity = GunExtender.Cache.Get(gun);
            if (entity != null || !entity.IsOwner)
            {
                return null;
            }

            return new GunMessageData()
            {
                playerId = PlayerIdManager.LocalId,
                gunInfo = gunInfo,
                gunID = entity.Id
            };
        }
    }
}