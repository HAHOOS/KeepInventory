using System.Text.Json;

using Tomlet.Attributes;

using Il2CppSLZ.Marrow.Warehouse;

namespace KeepInventory.Saves.V1
{
    public class SaveSlot
    {
        public string SlotName { get; set; }
        public string Barcode { get; set; }
        public SpawnableType Type { get; set; }

        [TomlNonSerialized]
        public GunInfo GunInfo { get; set; }

        [TomlProperty("GunInfo")]
        public string GunInfo_JSON
        {
            get
            {
                return JsonSerializer.Serialize(GunInfo, new JsonSerializerOptions() { WriteIndented = false });
            }

            set
            {
                if (!string.IsNullOrWhiteSpace(value) && value != "null" && IsJSON(value))
                {
                    GunInfo = JsonSerializer.Deserialize<GunInfo>(value);
                }
            }
        }

        public enum SpawnableType
        {
            Gun,
            Other
        }

        public SaveSlot()
        { }

        public SaveSlot(string slotName, Barcode barcode)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = SpawnableType.Other;
            GunInfo = null;
        }

        public SaveSlot(string slotName, Barcode barcode, SpawnableType type)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = type;
            GunInfo = null;
        }

        public SaveSlot(string slotName, Barcode barcode, SpawnableType type, GunInfo gunInfo)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = type;
            GunInfo = gunInfo;
        }

        public SaveSlot(string slotName, Barcode barcode, SpawnableType type, string gunInfoJSON)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = type;
            GunInfo_JSON = gunInfoJSON;
        }

        public SaveSlot(string slotName, Barcode barcode, GunInfo gunInfo)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = SpawnableType.Gun;
            GunInfo = gunInfo;
        }

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