using System;

using Il2CppSLZ.Marrow;

using KeepInventory.Saves.V2;
using KeepInventory.Utilities;

namespace KeepInventory.Helper
{
    public static class GunHelper
    {
        public static void UpdateProperties(this Gun gun, GunInfo info)
        {
            if (gun != null && info != null)
            {
                try
                {
                    if (info.IsMag && gun.defaultMagazine != null && gun.defaultCartridge != null)
                    {
                        var mag = info.GetMagazineData(gun);
                        if (mag?.spawnable?.crateRef != null && !string.IsNullOrWhiteSpace(mag.platform))
                            gun.LoadMagazine(info.RoundsLeft, () => gun.SetProperties(info));
                        else
                            gun.SetProperties(info);
                    }
                    else
                    {
                        gun.SetProperties(info);
                    }
                }
                catch (Exception ex)
                {
                    Core.Logger.Error("An unexpected error has occurred while applying gun info", ex);
                    BLHelper.SendNotification("Error", "Failed to successfully apply gun info to the gun, check the console or logs for more info", true, 3f, BoneLib.Notifications.NotificationType.Error);
                }
            }
        }

        internal static void SetProperties(this Gun gun, GunInfo info)
        {
            gun.fireMode = info.FireMode;
            gun.hasFiredOnce = info.HasFiredOnce;
            gun.hammerState = info.HammerState;
            gun.slideState = info.SlideState;
            if (!gun.isCharged && info.IsBulletInChamber)
            {
                gun.Charge();
                if (gun._hasMagState) gun.MagazineState.AddCartridge(1, gun.defaultCartridge);
                gun.cartridgeState = info.CartridgeState;
            }

            SlideUpdate(gun, info);
        }

        internal static void SlideUpdate(this Gun gun, GunInfo info)
        {
            switch (info.SlideState)
            {
                case Gun.SlideStates.LOCKED:
                    gun.SlideLocked();
                    break;

                case Gun.SlideStates.PULLING:
                    gun.CompleteSlidePull();
                    break;

                case Gun.SlideStates.RETURNING:
                    gun.CompleteSlideReturn();
                    break;
            }
        }
    }
}