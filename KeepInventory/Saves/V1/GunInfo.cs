using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using static Il2CppSLZ.Marrow.Gun;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeepInventory.Saves.V1
{
    /// <summary>
    /// Class that holds information regarding a <see cref="Gun"/>
    /// </summary>
    /// <remarks>
    /// Create new instance of <see cref="GunInfo"/>, exists for JSON deserializing
    /// </remarks>
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [method: JsonConstructor]
    public class GunInfo()
    {
        /// <summary>
        /// Is a magazine in the gun
        /// </summary>
        [JsonPropertyName("IsMag")]
        public bool IsMag { get; set; }

        /// <summary>
        /// Is there a bullet in the chamber
        /// </summary>
        [JsonPropertyName("IsBulletInChamber")]
        public bool IsBulletInChamber { get; set; }

        /// <summary>
        /// The selected fire mode example: SEMIAUTOMATIC
        /// </summary>
        [JsonPropertyName("FireMode")]
        public FireMode FireMode { get; set; }

        /// <summary>
        /// Rounds left in the magazine
        /// </summary>
        [JsonPropertyName("RoundsLeft")]
        public int RoundsLeft { get; set; }

        /// <summary>
        /// State of the hammer
        /// </summary>
        [JsonPropertyName("HammerState")]
        public HammerStates HammerState { get; set; }

        /// <summary>
        /// State of the slide
        /// </summary>
        [JsonPropertyName("SlideState")]
        public SlideStates SlideState { get; set; }

        /// <summary>
        /// State of the cartridge
        /// </summary>
        [JsonPropertyName("CartridgeState")]
        public CartridgeStates CartridgeState { get; set; }

        /// <summary>
        /// Was the gun fired at least once
        /// </summary>
        [JsonPropertyName("HasFiredOnce")]
        public bool HasFiredOnce { get; set; }

        /// <summary>
        /// Parses <see cref="Gun"/> to <see cref="GunInfo"/>
        /// </summary>
        /// <param name="gun">The gun to parse to <see cref="GunInfo"/></param>
        /// <returns><see cref="GunInfo"/> from the provided <see cref="Gun"/> object</returns>
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

        /// <summary>
        /// Creates <see cref="MagazineData"/> for the provided <see cref="Gun"/>, but setting the rounds to the <see cref="RoundsLeft"/> property
        /// </summary>
        /// <param name="gun">Gun you want to create <see cref="MagazineData"/> for</param>
        /// <returns><see cref="MagazineData"/> for <see cref="Gun"/> with properties from the current <see cref="GunInfo"/></returns>
        public MagazineData GetMagazineData(Gun gun)
        {
            return new MagazineData
            {
                spawnable = gun.defaultMagazine.spawnable,
                rounds = RoundsLeft,
                platform = gun.defaultMagazine.platform,
            };
        }

        /// <summary>
        /// Serialize the <see cref="GunInfo"/> object to JSON, made specifically for saving to files
        /// </summary>
        /// <returns>JSON form of <see cref="GunInfo"/></returns>
        public string Serialize()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = false });
        }

        /// <summary>
        /// Serialize the <see cref="GunInfo"/> object to JSON, made specifically for saving to files
        /// </summary>
        /// <param name="gunInfo">The <see cref="GunInfo"/> to serialize from</param>
        /// <returns>JSON form of <see cref="GunInfo"/></returns>
        public static string Serialize(GunInfo gunInfo)
        {
            return JsonSerializer.Serialize(gunInfo, new JsonSerializerOptions() { WriteIndented = false });
        }

        /// <summary>
        /// Converts JSON to <see cref="GunInfo"/>
        /// </summary>
        /// <param name="json">JSON to convert</param>
        /// <returns><see cref="GunInfo"/> from the provided JSON string</returns>
        public static GunInfo Deserialize(string json)
        {
            return JsonSerializer.Deserialize<GunInfo>(json);
        }
    }
}