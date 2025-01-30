using System;
using System.Collections.Generic;

using UnityEngine;
using KeepInventory.SDK.Objects;

#if MELONLOADER

using MelonLoader;
using Il2CppInterop.Runtime.Attributes;

#endif

namespace KeepInventory.SDK
{
    /// <summary>
    /// Script that forces the player to load in with a specific save that cannot be changed while in that level
    /// </summary>
#if MELONLOADER

    [RegisterTypeInIl2Cpp]
#else

    [DisallowMultipleComponent]
#endif
    public class ForceSave : MonoBehaviour
    {
        /// <summary>
        /// Instance of <see cref="ForceSave"/>
        /// </summary>
#if MELONLOADER

        [HideFromIl2Cpp]
#else

        [HideInInspector]
#endif
        public static ForceSave Instance { get; private set; }

        /// <summary>
        /// The ID of the save, used to find the save if already created before
        /// </summary>
        public string ID = string.Empty;

        /// <summary>
        /// The name of the save that will be displayed in BoneMenu
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// Should <see cref="Color"/> be used or <see cref="Gradient"/>
        /// </summary>
        public bool UseGradient = false;

        /// <summary>
        /// The color that will be used along with the name in BoneMenu
        /// </summary>
        public UnityEngine.Color Color = UnityEngine.Color.white;

        /// <summary>
        /// The gradient that will be used along with the name in BoneMenu
        /// </summary>
        public Gradient Gradient = null;

        /// <summary>
        /// Should the save be hidden from BoneMenu
        /// </summary>
        public bool IsHidden = true;

        /// <summary>
        /// Should the player be able to modify the save through the BoneMenu
        /// </summary>
        public bool CanBeOverwrittenByPlayer = false;

        /// <summary>
        /// The data for the save, if the save was not found and one needs to be created
        /// </summary>
        public DefaultSave Default = new();

#if MELONLOADER
#pragma warning disable RCS1213 // Remove unused member declaration
#pragma warning disable IDE0051 // Remove unused private members

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

#pragma warning restore RCS1213 // Remove unused member declaration
#pragma warning restore IDE0051 // Remove unused private members
#endif
    }

    /// <summary>
    /// Class that contains data about the ammo and inventory of the player that should be used if a save was not found
    /// </summary>
#if !MELONLOADER
    [Serializable]
#endif

    public class DefaultSave
    {
        /// <summary>
        /// The amount of light ammo
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public int LightAmmo = 0;
        /// <summary>
        /// The amount of medium ammo
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public int MediumAmmo = 0;

        /// <summary>
        /// The amount of heavy ammo
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
        public int HeavyAmmo = 0;

        /// <summary>
        /// The inventory slots
        /// </summary>
#if !MELONLOADER
    [SerializeField]
#endif
#pragma warning disable IDE0028 // Simplify collection initialization
        public List<SaveSlot> SaveSlots = new();
#pragma warning restore IDE0028 // Simplify collection initialization
    }
}