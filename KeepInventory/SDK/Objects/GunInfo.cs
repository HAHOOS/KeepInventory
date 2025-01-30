#if MELONLOADER

using Il2CppSLZ.Marrow;

using static Il2CppSLZ.Marrow.Gun;

#else

using System;

using SLZ.Marrow;

using UnityEngine;

using static SLZ.Marrow.Gun;

#endif

namespace KeepInventory.SDK.Objects
{
    /// <summary>
    /// Class that holds information regarding a <see cref="Gun"/>
    /// </summary>
#if !MELONLOADER
    [Serializable]
#endif

    public class GunInfo
    {
        /// <summary>
        /// Is a magazine in the gun
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public bool IsMag;

        /// <summary>
        /// Is there a bullet in the chamber
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public bool IsBulletInChamber;

        /// <summary>
        /// The selected fire mode example: SEMIAUTOMATIC
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public FireMode FireMode;

        /// <summary>
        /// Rounds left in the magazine
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public int RoundsLeft;

        /// <summary>
        /// State of the hammer
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public HammerStates HammerState;

        /// <summary>
        /// State of the slide
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public SlideStates SlideState;

        /// <summary>
        /// State of the cartridge
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public CartridgeStates CartridgeState;

        /// <summary>
        /// Was the gun fired at least once
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public bool HasFiredOnce;

#if MELONLOADER

        /// <summary>
        /// Convert to <see cref="Saves.V2.GunInfo"/>
        /// </summary>
        /// <returns>V2 GunInfo</returns>
        public Saves.V2.GunInfo Parse()
        {
            return new Saves.V2.GunInfo()
            {
                CartridgeState = CartridgeState,
                HammerState = HammerState,
                IsBulletInChamber = IsBulletInChamber,
                RoundsLeft = RoundsLeft,
                FireMode = FireMode,
                HasFiredOnce = HasFiredOnce,
                IsMag = IsMag,
                SlideState = SlideState,
            };
        }

#endif
    }
}