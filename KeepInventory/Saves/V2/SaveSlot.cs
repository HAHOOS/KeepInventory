using Il2CppSLZ.Marrow.Warehouse;

using Newtonsoft.Json;

namespace KeepInventory.Saves.V2
{
	public class SaveSlot
	{
		[JsonProperty(nameof(SlotName))]
		public string SlotName { get; set; }

		[JsonProperty(nameof(Barcode))]
		public string Barcode { get; set; }

		[JsonProperty(nameof(Type))]
		public SpawnableType Type { get; set; }

		[JsonProperty(nameof(GunInfo))]
		public GunInfo GunInfo { get; set; }

		public enum SpawnableType
		{
			Gun,
			Other
		}

		[JsonConstructor]
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