using BoneLib;

using System.Collections.Generic;
using System.Drawing;

namespace KeepInventory
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
            Color.Silver,
            Color.DarkRed,
            Color.Orange,
            Color.HotPink,
            Color.Purple,
            Color.Lime,
            Color.Goldenrod
        ];

        private static readonly List<Color> SlotColors_Blacklist = [];

        /// <summary>
        /// Get a random slot color from the possible ones
        /// </summary>
        /// <returns>A random <see cref="Color"/></returns>
        public static Color GetRandomSlotColor()
        {
            if (SlotColors.Count == SlotColors_Blacklist.Count) SlotColors_Blacklist.Clear();
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
    }
}