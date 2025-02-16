using BoneLib;

using System.Collections.Generic;
using System.Drawing;

namespace KeepInventory.Utilities
{
    /// <summary>
    /// Class that contains most things regarding coloring (ANSI)
    /// </summary>
    public static class Colors
    {
        /// <summary>
        /// List of colors that can be used in the Slot prefix
        /// </summary>
        public readonly static List<Color> SlotColors =
        [
            Color.BlueViolet,
            Color.LightGreen,
            Color.Red,
            Color.SlateBlue,
            Color.Magenta,
            Color.Gold,
            Color.Azure,
            Color.DarkGreen,
            Color.Aquamarine,
            Color.DarkRed,
            Color.Orange,
            Color.HotPink,
            Color.Purple,
            Color.Lime,
            Color.Goldenrod
        ];

        /// <summary>
        /// List of colors that cannot be used in the Slot prefix anymore until reset
        /// </summary>
        private static readonly List<Color> SlotColors_Blacklist = [];

        /// <summary>
        /// Get a random slot color from the possible ones
        /// </summary>
        /// <returns>A random <see cref="Color"/></returns>
        public static Color GetRandomSlotColor()
        {
            if (SlotColors.Count == SlotColors_Blacklist.Count) ResetSlotColorBlacklist();
            while (true)
            {
                var random = SlotColors.GetRandom();
                if (!SlotColors_Blacklist.Contains(random))
                {
                    SlotColors_Blacklist.Add(random);
                    return random;
                }
            }
        }

        /// <summary>
        /// Removes all the colors from the blacklist, making all colors usable again instead of having to wait for all colors to be used
        /// </summary>
        public static void ResetSlotColorBlacklist() => SlotColors_Blacklist.Clear();
    }
}