using System;
using System.Drawing;

using Il2CppSLZ.Marrow;

using KeepInventory.Saves.V2;
using KeepInventory.Utilities;

namespace KeepInventory.Helper
{
    public static class GunHelper
    {
        private readonly static Color SlotColor = Color.Cyan;

        public static void UpdateProperties(this Gun gun, GunInfo info, SaveSlot slot = null, string name = "N/A", string barcode = "N/A")
        {
            if (string.IsNullOrWhiteSpace(name)) name = "N/A";
            if (string.IsNullOrWhiteSpace(barcode)) barcode = "N/A";
            string slotName = slot == null ? "N/A" : string.IsNullOrWhiteSpace(slot.SlotName) ? "N/A" : slot.SlotName;

            if (gun != null && info != null)
            {
                try
                {
                    void setGunProperties()
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

                    if (info.IsMag && gun.defaultMagazine != null && gun.defaultCartridge != null)
                    {
                        var mag = info.GetMagazineData(gun);
                        if (mag?.spawnable?.crateRef != null && !string.IsNullOrWhiteSpace(mag.platform))
                            gun.LoadMagazine(info.RoundsLeft, setGunProperties);
                        else
                            setGunProperties();
                    }
                    else
                    {
                        setGunProperties();
                    }
                }
                catch (Exception ex)
                {
                    Core.Logger.Error($"An unexpected error has occurred while applying gun info, exception:\n{ex}");
                    BLHelper.SendNotification("Error", "Failed to successfully apply gun info to the gun, check the console or logs for more info", true, 3f, BoneLib.Notifications.NotificationType.Error);
                }
            }
        }
    }
}