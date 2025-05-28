using Il2CppSLZ.Marrow.Warehouse;

using System.Text.Json;
using System.Text.Json.Serialization;

using Tomlet.Attributes;

namespace KeepInventory.Saves.V2
{
    public class SaveSlot
    {
        [JsonPropertyName("SlotName")]
        public string SlotName { get; set; }
        [JsonPropertyName("Barcode")]
        public string Barcode { get; set; }
        [JsonPropertyName("Type")]
        public SpawnableType Type { get; set; }
        [JsonPropertyName("GunInfo")]
        public GunInfo GunInfo { get; set; }
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
        public SaveSlot(string slotName, Barcode barcode, GunInfo gunInfo)
        {
            SlotName = slotName;
            Barcode = barcode.ID;
            Type = SpawnableType.Gun;
            GunInfo = gunInfo;
        }
        public SaveSlot(V1.SaveSlot v1)
        {
            SlotName = v1.SlotName;
            Barcode = v1.Barcode;
            Type = (SpawnableType)v1.Type;
            GunInfo = GunInfo.Parse(v1.GunInfo);
        }
    }
}