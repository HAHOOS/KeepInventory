using System;

using UnityEngine;

namespace KeepInventory.SDK.Objects
{
    /// <summary>
    /// Class that contains data about a specific inventory slot for a save
    /// </summary>
#if !MELONLOADER
    [Serializable]
#endif

    public class SaveSlot
    {
        /// <summary>
        /// Name of the slot that contains the spawnable
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public string SlotName;

        /// <summary>
        /// Barcode of the spawnable
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public string Barcode;

        /// <summary>
        /// Type of the spawnable, currently only: GUN and OTHER<br/>
        /// This is to indicate whether or not should the mod load <see cref="GunInfo"/>
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public SpawnableType Type;

        /// <summary>
        /// Contains information regarding the gun, such as if it has a magazine in or how many rounds does it have left
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public GunInfo GunInfo;

        /// <summary>
        /// Type of the spawnable
        /// </summary>
        public enum SpawnableType
        {
            /// <summary>
            /// Spawnable is a gun, should load <see cref="GunInfo"/> if found
            /// </summary>
            Gun,

            /// <summary>
            /// Spawnable is not a gun
            /// </summary>
            Other
        }

#if MELONLOADER

        /// <summary>
        /// Convert <see cref="SaveSlot"/> to <see cref="Saves.V2.SaveSlot"/>
        /// </summary>
        /// <param name="s">The <see cref="SaveSlot"/> to convert</param>
        public static implicit operator Saves.V2.SaveSlot(SaveSlot s)
        {
            return new Saves.V2.SaveSlot()
            {
                SlotName = s.SlotName,
                Barcode = s.Barcode,
                GunInfo = s.GunInfo?.Parse(),
                Type = (Saves.V2.SaveSlot.SpawnableType)s.Type
            };
        }

#endif
    }
}