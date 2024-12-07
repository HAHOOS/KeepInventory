using Il2CppSLZ.Marrow;

using KeepInventory.Saves;

namespace KeepInventory.Helper
{
    /// <summary>
    /// Provides helper methods for <see cref="Gun"/>
    /// </summary>
    public static class GunHelper
    {
        /// <summary>
        /// Sends a message to the Fusion Server if connected
        /// </summary>
        /// <param name="gun">The gun to update</param>
        /// <param name="info">The data to update with</param>
        /// <param name="slotColor">Color that will be used in the slot prefix</param>
        /// <param name="slot">The save slot (debugging purposes)</param>
        /// <param name="name">Name of the crate (debugging purposes)</param>
        /// <param name="barcode">Barcode of the spawnable (debugging purposes)</param>
        /// <param name="printMessages">If <see langword="true"/>, the method will print debug messages using <see cref="MelonLoader.MelonLogger.Instance"/></param>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void SendFusionMessage(Gun gun, GunInfo info, System.Drawing.Color slotColor, SaveSlot slot = null, string name = "N/A", string barcode = "N/A", bool printMessages = true)
        {
            if (Core.IsConnected)
            {
                Fusion.FusionMethods.SendFusionMessage(gun, info);
            }
            else
            {
                // HACK: Make it seem like its a fusion message to avoid loop
                UpdateProperties(gun, info, slotColor, slot, name, barcode, printMessages, true);
            }
        }

        /// <summary>
        /// Updates the gun with provided <see cref="GunInfo"/>
        /// </summary>
        /// <param name="gun">The gun to update</param>
        /// <param name="info">The data to update with</param>
        /// <param name="slotColor">Color that will be used in the slot prefix</param>
        /// <param name="slot">The save slot (debugging purposes)</param>
        /// <param name="name">Name of the crate (debugging purposes)</param>
        /// <param name="barcode">Barcode of the spawnable (debugging purposes)</param>
        /// <param name="printMessages">If <see langword="true"/>, the method will print debug messages using <see cref="MelonLoader.MelonLogger.Instance"/></param>
        /// <param name="fusionMessage">If <see langword="true"/>, this method will be treated as it was run by a <see cref="LabFusion.Network.FusionMessage"/>, which means it will not send a fusion message</param>
        public static void UpdateProperties(this Gun gun, GunInfo info, System.Drawing.Color slotColor, SaveSlot slot = null, string name = "N/A", string barcode = "N/A", bool printMessages = true, bool fusionMessage = false)
        {
            const bool useFusion = true;

            string slotName = slot == null ? "N/A" : string.IsNullOrWhiteSpace(slot.SlotName) ? "N/A" : slot.SlotName;
            if (gun == null || info == null) return;
            if (Core.HasFusion && Core.IsConnected && !fusionMessage && useFusion)
            {
                if (!Core.IsFusionLibraryInitialized || !Core.mp_fusionSupport.Value)
                {
                    Core.Logger.Warning($"[{slotName}] The Fusion Library is not loaded or the setting 'Fusion Support' is set to Disabled. To update gun properties in Fusion servers, check if you have the Fusion Library in UserData > KeepInventory (there should be a file called 'KeepInventory.Fusion.dll') or try enabling 'Fusion Support' in settings");
                    return;
                }
                if (printMessages) Core.MsgPrefix("Sending request to host to edit gun data for everyone", slotName, slotColor);
                SendFusionMessage(gun, info, slotColor, slot, name, barcode, printMessages);
            }
            else
            {
                if (gun != null)
                {
                    void other()
                    {
                        if (printMessages) Core.MsgPrefix($"Setting fire mode value to {info.FireMode}", slotName, slotColor);
                        gun.fireMode = info.FireMode;
                        if (printMessages) Core.MsgPrefix($"Setting 'HasFiredOnce' value to {info.HasFiredOnce}", slotName, slotColor);
                        gun.hasFiredOnce = info.HasFiredOnce;
                        if (printMessages) Core.MsgPrefix($"Setting hammer state to {info.HammerState}", slotName, slotColor);
                        gun.hammerState = info.HammerState;
                        if (printMessages) Core.MsgPrefix($"Setting slide state to {info.SlideState}", slotName, slotColor);
                        gun.slideState = info.SlideState;

                        if (!gun.isCharged && info.IsBulletInChamber)
                        {
                            if (printMessages) Core.MsgPrefix("Charging gun", slotName, slotColor);
                            gun.Charge();
                            if (gun._hasMagState) gun.MagazineState.AddCartridge(1, gun.defaultCartridge);
                            if (printMessages) Core.MsgPrefix($"Setting cartridge state to {info.CartridgeState}", slotName, slotColor);
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

                        if (printMessages) Core.MsgPrefix($"Spawned to slot: {name} ({barcode})", slotName, slotColor);
                    }

                    if (printMessages) Core.MsgPrefix($"Writing gun info for: {name} ({barcode})", slotName, slotColor);
                    if (info.IsMag && gun.defaultMagazine != null && gun.defaultCartridge != null)
                    {
                        if (printMessages) Core.MsgPrefix("Loading magazine", slotName, slotColor);
                        var mag = info.GetMagazineData(gun);
                        if (mag?.spawnable?.crateRef != null && !string.IsNullOrWhiteSpace(mag.platform))
                        {
                            var task = gun.ammoSocket.ForceLoadAsync(info.GetMagazineData(gun));
                            var awaiter = task.GetAwaiter();

                            void something()
                            {
                                gun.MagazineState.SetCartridge(info.RoundsLeft);
                                other();
                            }

                            awaiter.OnCompleted((Il2CppSystem.Action)something);
                        }
                        else
                        {
                            if (printMessages) Core.Logger.Warning($"[{slotName}] Could not get sufficient information for MagazineData, not loading the magazine and rounds left");
                            other();
                        }
                    }
                    else
                    {
                        other();
                    }
                }
            }
        }
    }
}