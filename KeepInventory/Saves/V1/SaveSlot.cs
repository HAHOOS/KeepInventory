using Il2CppSLZ.Marrow.Warehouse;

using System.Text.Json;

using Tomlet.Attributes;

namespace KeepInventory.Saves.V1
{
    /// <summary>
    /// Class used in saving inventory slots
    /// </summary>
    public class SaveSlot
    {
        /// <summary>
        /// Name of the slot that contains the spawnable
        /// </summary>
        public string SlotName { get; set; }

        /// <summary>
        /// Barcode of the spawnable
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// Type of the spawnable, currently only: GUN and OTHER<br/>
        /// This is to indicate whether or not should the mod load <see cref="GunInfo"/>
        /// </summary>
        public SpawnableType Type { get; set; }

        /// <summary>
        /// Contains information regarding the gun, such as if it has a magazine in or how many rounds does it have left
        /// </summary>
        [TomlNonSerialized]
        public GunInfo GunInfo { get; set; }

        /// <summary>
        /// JSON form of <see cref="GunInfo"/>, used in saving in TOML
        /// </summary>
        [TomlProperty("GunInfo")]
        public string GunInfo_JSON
        {
            get
            {
                return JsonSerializer.Serialize(GunInfo, new JsonSerializerOptions() { WriteIndented = false });
            }

            set
            {
                // Check if its actually JSON to avoid errors
                if (!string.IsNullOrWhiteSpace(value) && value != "null" && IsJSON(value))
                {
                    GunInfo = JsonSerializer.Deserialize<GunInfo>(value);
                }
            }
        }

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
        /// <param name="type"><inheritdoc cref="Type"/></param>
        /// <param name="gunInfoJSON">JSON form of <see cref="GunInfo"/></param>
        public SaveSlot(string slotName, Barcode barcode, SpawnableType type, string gunInfoJSON)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = type;
            GunInfo_JSON = gunInfoJSON;
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
        /// Creates a new instance of <see cref="SaveSlot"/>
        /// </summary>
        /// <param name="slotName"><inheritdoc cref="SlotName"/></param>
        /// <param name="barcode"><inheritdoc cref="Barcode"/></param>
        /// <param name="gunInfoJSON">JSON form of <see cref="GunInfo"/></param>
        public SaveSlot(string slotName, Barcode barcode, string gunInfoJSON)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = SpawnableType.Gun;
            GunInfo_JSON = gunInfoJSON;
        }

        private static bool IsJSON(string text)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(text);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}