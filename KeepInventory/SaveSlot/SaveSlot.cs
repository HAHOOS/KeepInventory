using Il2CppSLZ.Marrow.Warehouse;

namespace KeepInventory.SaveSlot
{
    public class SaveSlot
    {
        public string SlotName { get; set; }
        public string Barcode { get; set; }

        public SpawnableType Type { get; set; }

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
    }
}