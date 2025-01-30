using UnityEngine;

#if MELONLOADER

using MelonLoader;
using Il2CppInterop.Runtime.Attributes;

#endif

namespace KeepInventory.SDK
{
    /// <summary>
    /// Script that disables loading saved inventories at all cost, can be turned off or on by changing the <see cref="Disabled"/> property
    /// </summary>
#if MELONLOADER

    [RegisterTypeInIl2Cpp]
#else

    [DisallowMultipleComponent]
#endif
    public class Disable : MonoBehaviour
    {
        /// <summary>
        /// Instance of <see cref="Disable"/>
        /// </summary>
#if MELONLOADER

        [HideFromIl2Cpp]
#else

        [HideInInspector]
#endif
        public static Disable Instance { get; private set; }

#if !MELONLOADER
        [SerializeField]
#endif
        private bool m_Disabled = true;

        /// <summary>
        /// Should the inventory be disabled from loading, <see langword="true"/> will prevent, <see langword="false"/> won't
        /// </summary>
#pragma warning disable RCS1085 // Use auto-implemented property

        public bool Disabled
#pragma warning restore RCS1085 // Use auto-implemented property
        {
            get { return m_Disabled; }
            set { m_Disabled = value; }
        }

#if MELONLOADER

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable RCS1213 // Remove unused member declaration

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
}