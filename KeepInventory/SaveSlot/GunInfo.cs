using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Tomlet.Attributes;
using static Il2CppSLZ.Marrow.Gun;

namespace KeepInventory.SaveSlot
{
    public class GunInfo
    {
        [TomlPrecedingComment("Is a mag inserted")]
        public bool IsMag { get; set; }

        [TomlPrecedingComment("Is there a bullet in the chamber")]
        public bool IsBulletInChamber { get; set; }

        [TomlPrecedingComment("The selected fire mode")]
        public FireMode FireMode { get; set; }

        [TomlPrecedingComment("State of the magazine")]
        public Save_MagazineState MagazineState { get; set; }

        [TomlPrecedingComment("State of the hammer")]
        public HammerStates HammerState { get; set; }

        [TomlPrecedingComment("State of the slide")]
        public SlideStates SlideState { get; set; }

        public GunInfo()
        { }

        public static GunInfo Parse(Gun gun)
        {
            var _new = new GunInfo
            {
                IsMag = gun.HasMagazine(),
                IsBulletInChamber = gun.chamberedCartridge != null,
                FireMode = gun.fireMode,
                HammerState = gun.hammerState,
                SlideState = gun.slideState,
            };
            if (gun.MagazineState != null)
            {
                _new.MagazineState = new Save_MagazineState
                {
                    Count = gun.MagazineState.AmmoCount
                };
            }

            return _new;
        }

        public MagazineData GetMagazineData(Gun gun)
        {
            var data = new MagazineData
            {
                spawnable = gun.defaultMagazine.spawnable,
                rounds = MagazineState.Count,
                platform = gun.defaultMagazine.platform,
            };
            return data;
        }
    }
}