using Newtonsoft.Json;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;

using static Il2CppSLZ.Marrow.Gun;

namespace KeepInventory.Saves.V2
{
	[method: JsonConstructor]
	public class GunInfo()
	{
		[JsonProperty(nameof(IsMag))]
		public bool IsMag { get; set; }

		[JsonProperty(nameof(IsBulletInChamber))]
		public bool IsBulletInChamber { get; set; }

		[JsonProperty(nameof(FireMode))]
		public FireMode FireMode { get; set; }

		[JsonProperty(nameof(RoundsLeft))]
		public int RoundsLeft { get; set; }

		[JsonProperty(nameof(HammerState))]
		public HammerStates HammerState { get; set; }

		[JsonProperty(nameof(SlideState))]
		public SlideStates SlideState { get; set; }

		[JsonProperty(nameof(CartridgeState))]
		public CartridgeStates CartridgeState { get; set; }

		[JsonProperty(nameof(HasFiredOnce))]
		public bool HasFiredOnce { get; set; }

		public static GunInfo Parse(Gun gun)
		{
			var _new = new GunInfo
			{
				IsMag = gun.HasMagazine(),
				IsBulletInChamber = gun.chamberedCartridge != null,
				FireMode = gun.fireMode,
				HammerState = gun.hammerState,
				SlideState = gun.slideState,
				HasFiredOnce = gun.hasFiredOnce,
				CartridgeState = gun.cartridgeState,
			};

			if (gun.MagazineState != null)
			{
				_new.RoundsLeft = gun.MagazineState.AmmoCount;
			}

			return _new;
		}

		public static GunInfo Parse(V1.GunInfo gunInfoV1)
		{
			if (gunInfoV1 == null) return null;
			return new GunInfo()
			{
				IsMag = gunInfoV1.IsMag,
				IsBulletInChamber = gunInfoV1.IsBulletInChamber,
				FireMode = gunInfoV1.FireMode,
				HammerState = gunInfoV1.HammerState,
				SlideState = gunInfoV1.SlideState,
				HasFiredOnce = gunInfoV1.HasFiredOnce,
				CartridgeState = gunInfoV1.CartridgeState,
				RoundsLeft = gunInfoV1.RoundsLeft,
			};
		}

		public MagazineData GetMagazineData(Gun gun)
		{
			return new MagazineData
			{
				spawnable = gun.defaultMagazine.spawnable,
				rounds = RoundsLeft,
				platform = gun.defaultMagazine.platform,
			};
		}

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		public static string Serialize(GunInfo gunInfo)
		{
			return JsonConvert.SerializeObject(gunInfo, Formatting.None);
		}

		public static GunInfo Deserialize(string json)
		{
			return JsonConvert.DeserializeObject<GunInfo>(json);
		}
	}
}