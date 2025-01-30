using Il2CppSLZ.Marrow.Warehouse;

using System.Text.Json;
using System.Text.Json.Serialization;

using Tomlet.Attributes;

namespace KeepInventory.Saves.V2
{
    /// <summary>
    /// Class used in saving inventory slots
    /// </summary>
    public class SaveSlot
    {
        /// <summary>
        /// Name of the slot that contains the spawnable
        /// </summary>
        [JsonPropertyName("SlotName")]
        public string SlotName { get; set; }

        /// <summary>
        /// Barcode of the spawnable
        /// </summary>
        [JsonPropertyName("Barcode")]
        public string Barcode { get; set; }

        /// <summary>
        /// Type of the spawnable, currently only: GUN and OTHER<br/>
        /// This is to indicate whether or not should the mod load <see cref="GunInfo"/>
        /// </summary>
        [JsonPropertyName("Type")]
        public SpawnableType Type { get; set; }

        /// <summary>
        /// Contains information regarding the gun, such as if it has a magazine in or how many rounds does it have left
        /// </summary>
        [JsonPropertyName("GunInfo")]
        public GunInfo GunInfo { get; set; }

        /// <summary>
        /// Type of the spawnable
        /// </summary>
        public enum SpawnableType
        {
            /// <summary>
            /// Spawnable is a gun, should load <see cref="GunInfo"/> if found
            /// </summary>
            Gun,

            /// <summary>
            /// Spawnable is not a gun
            /// </summary>
            Other
        }

        /// <summary>
        /// Creates a new instance of <see cref="SaveSlot"/>
        /// </summary>
        public SaveSlot()
        { }

        /// <summary>
        /// Creates a new instance of <see cref="SaveSlot"/>
        /// </summary>
        /// <param name="slotName"><inheritdoc cref="SlotName"/></param>
        /// <param name="barcode"><inheritdoc cref="Barcode"/></param>
        public SaveSlot(string slotName, Barcode barcode)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = SpawnableType.Other;
            GunInfo = null;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SaveSlot"/>
        /// </summary>
        /// <param name="slotName"><inheritdoc cref="SlotName"/></param>
        /// <param name="barcode"><inheritdoc cref="Barcode"/></param>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        public SaveSlot(string slotName, Barcode barcode, SpawnableType type)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = type;
            GunInfo = null;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SaveSlot"/>
        /// </summary>
        /// <param name="slotName"><inheritdoc cref="SlotName"/></param>
        /// <param name="barcode"><inheritdoc cref="Barcode"/></param>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        /// <param name="gunInfo"><inheritdoc cref="GunInfo"/></param>
        public SaveSlot(string slotName, Barcode barcode, SpawnableType type, GunInfo gunInfo)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = type;
            GunInfo = gunInfo;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SaveSlot"/>
        /// </summary>
        /// <param name="slotName"><inheritdoc cref="SlotName"/></param>
        /// <param name="barcode"><inheritdoc cref="Barcode"/></param>
        /// <param name="gunInfo"><inheritdoc cref="GunInfo"/></param>
        public SaveSlot(string slotName, Barcode barcode, GunInfo gunInfo)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = SpawnableType.Gun;
            GunInfo = gunInfo;
        }

        /// <summary>
        /// Creates new instance of <see cref="SaveSlot"/> from <see cref="V1.SaveSlot"/>
        /// </summary>
        /// <param name="v1"></param>
        public SaveSlot(V1.SaveSlot v1)
        {
            SlotName = v1.SlotName;
            Barcode = v1.Barcode;
            Type = (SpawnableType)v1.Type;
            GunInfo = GunInfo.Parse(v1.GunInfo);
        }
    }
}