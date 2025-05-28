using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using static Il2CppSLZ.Marrow.Gun;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeepInventory.Saves.V2
{
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [method: JsonConstructor]
    public class GunInfo()
    {
        [JsonPropertyName("IsMag")]
        public bool IsMag { get; set; }
        [JsonPropertyName("IsBulletInChamber")]
        public bool IsBulletInChamber { get; set; }
        [JsonPropertyName("FireMode")]
        public FireMode FireMode { get; set; }
        [JsonPropertyName("RoundsLeft")]
        public int RoundsLeft { get; set; }
        [JsonPropertyName("HammerState")]
        public HammerStates HammerState { get; set; }
        [JsonPropertyName("SlideState")]
        public SlideStates SlideState { get; set; }
        [JsonPropertyName("CartridgeState")]
        public CartridgeStates CartridgeState { get; set; }
        [JsonPropertyName("HasFiredOnce")]
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
            return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = false });
        }
        public static string Serialize(GunInfo gunInfo)
        {
            return JsonSerializer.Serialize(gunInfo, new JsonSerializerOptions() { WriteIndented = false });
        }
        public static GunInfo Deserialize(string json)
        {
            return JsonSerializer.Deserialize<GunInfo>(json);
        }
    }
}