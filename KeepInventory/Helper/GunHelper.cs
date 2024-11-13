using Il2CppSLZ.Marrow;
using KeepInventory.SaveSlot;

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
        /// <param name="slot">The save slot (debugging purposes)</param>
        /// <param name="name">Name of the crate (debugging purposes)</param>
        /// <param name="barcode">Barcode of the spawnable (debugging purposes)</param>
        /// <param name="printMessages">If <see langword="true"/>, the method will print debug messages using <see cref="MelonLoader.MelonLogger.Instance"/></param>
        [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void SendFusionMessage(Gun gun, GunInfo info, SaveSlot.SaveSlot slot = null, string name = "N/A", string barcode = "N/A", bool printMessages = true)
        {
            if (Fusion.FusionMethods.IsConnected)
            {
                Fusion.FusionMethods.SendFusionMessage(gun, info);
            }
            else
            {
                // HACK: Make it seems like its a fusion message to avoid loop
                UpdateProperties(gun, info, slot, name, barcode, printMessages, true);
            }
        }

        /// <summary>
        /// Updates the gun with provided <see cref="GunInfo"/>
        /// </summary>
        /// <param name="gun">The gun to update</param>
        /// <param name="info">The data to update with</param>
        /// <param name="slot">The save slot (debugging purposes)</param>
        /// <param name="name">Name of the crate (debugging purposes)</param>
        /// <param name="barcode">Barcode of the spawnable (debugging purposes)</param>
        /// <param name="printMessages">If <see langword="true"/>, the method will print debug messages using <see cref="MelonLoader.MelonLogger.Instance"/></param>
        /// <param name="fusionMessage">If <see langword="true"/>, this method will be treated as it was run by a <see cref="LabFusion.Network.FusionMessage"/>, which means it will not send a fusion message</param>
        public static void UpdateProperties(this Gun gun, GunInfo info, SaveSlot.SaveSlot slot = null, string name = "N/A", string barcode = "N/A", bool printMessages = true, bool fusionMessage = false)
        {
            // Test

            string slotName = slot == null ? "N/A" : string.IsNullOrWhiteSpace(slot.SlotName) ? "N/A" : slot.SlotName;
            if (gun == null || info == null) return;
            if (Core.HasFusion && !fusionMessage)
            {
                SendFusionMessage(gun, info, slot, name, barcode, printMessages);
            }
            else
            {
                if (gun != null)
                {
                    if (printMessages) Core.Logger.Msg($"[{slotName}] Writing gun info for: {name} ({barcode})");
                    if (info.IsMag && gun.defaultMagazine != null && gun.defaultCartridge != null)
                    {
                        if (printMessages) Core.Logger.Msg($"[{slotName}] Loading magazine");
                        var task = gun.ammoSocket.ForceLoadAsync(info.GetMagazineData(gun));
                        var awaiter = task.GetAwaiter();
                        void action1()
                        {
                            if (printMessages) Core.Logger.Msg($"[{slotName}] Setting rounds left");
                            gun.MagazineState.SetCartridge(info.RoundsLeft);
                            gun.MagazineState.Initialize(gun.MagazineState.cartridgeData, info.RoundsLeft);
                        }
                        awaiter.OnCompleted((Il2CppSystem.Action)action1);
                    }

                    if (printMessages) Core.Logger.Msg($"[{slotName}] Setting fire mode");
                    gun.fireMode = info.FireMode;
                    if (printMessages) Core.Logger.Msg($"[{slotName}] Setting 'HasFiredOnce' value");
                    gun.hasFiredOnce = info.HasFiredOnce;
                    if (printMessages) Core.Logger.Msg($"[{slotName}] Setting hammer state");
                    gun.hammerState = info.HammerState;
                    if (printMessages) Core.Logger.Msg($"[{slotName}] Setting slide state");
                    gun.slideState = info.SlideState;

                    if (gun.hammerState == Gun.HammerStates.COCKED)
                    {
                        if (printMessages) Core.Logger.Msg($"[{slotName}] Charging gun due to hammer state");
                        gun.Charge();
                    }

                    if (info.IsBulletInChamber && gun.defaultCartridge != null)
                    {
                        if (printMessages) Core.Logger.Msg($"[{slotName}] Loading cartridge");
                        gun.SpawnCartridge(gun.defaultCartridge.cartridgeSpawnable);
                    }

                    // Don't ask whats happening here, because I have no idea
                    // Why can't this literally work, I ran the method and what
                    // Nothing happens, it doesn't want to work like I want it to
                    // 😭

                    // I don't even know if this works, I can't get it to work properly so uh

                    switch (info.SlideState)
                    {
                        case Gun.SlideStates.LOCKED:
                            gun.PlayAnimationState(Gun.AnimationStates.SLIDELOCKED);
                            gun.SlideLocked();
                            gun.CompleteSlidePull();

                            gun.SlideLocked();
                            break;

                        case Gun.SlideStates.PULLED:
                            gun.PlayAnimationState(Gun.AnimationStates.SLIDEPULL);
                            gun.CompleteSlidePull();
                            break;

                        case Gun.SlideStates.PULLING:
                            gun.PlayAnimationState(Gun.AnimationStates.SLIDEPULL);
                            gun.CompleteSlidePull();
                            break;

                        case Gun.SlideStates.RETURNED:
                            gun.PlayAnimationState(Gun.AnimationStates.SLIDERETURN);
                            gun.CompleteSlidePull();
                            gun.CompleteSlideReturn();
                            break;

                        case Gun.SlideStates.RETURNING:
                            gun.PlayAnimationState(Gun.AnimationStates.SLIDERETURN);
                            gun.CompleteSlidePull();
                            gun.CompleteSlideReturn();
                            break;
                    }
                }
            }
            if (printMessages) Core.Logger.Msg($"[{slotName}] Spawned to slot: {name} ({barcode})");
        }
    }
}