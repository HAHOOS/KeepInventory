using BoneLib;

using System;
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

        /// <summary>
        /// Removes all the colors from the blacklist, making all colors usable again instead of having to wait for all colors to be used
        /// </summary>
        public static void ResetSlotColorBlacklist() => SlotColors_Blacklist.Clear();

        /// <summary>
        /// Converts <see cref="Color"/> to HEX
        /// </summary>
        /// <param name="color">The <see cref="Color"/> you want to convert</param>
        /// <returns>HEX format of the provided <see cref="Color"/></returns>
        public static string ToHEX(this Color color)
        {
            return $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Converts HEX to <see cref="Color"/>
        /// </summary>
        /// <param name="hex">The HEX you want to convert</param>
        /// <returns><see cref="Color"/> from HEX</returns>
        public static Color FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex) || hex.Length < 6) return Color.White;
            try
            {
                return ColorTranslator.FromHtml(hex.StartsWith('#') ? hex : $"#{hex}");
            }
            catch (Exception)
            {
                return Color.White;
            }
        }

        /// <summary>
        /// Adds color to text with Unity Rich Text
        /// </summary>
        /// <param name="text">The text you want to add color to</param>
        /// <param name="color">The <see cref="Color"/></param>
        /// <returns>Unity Rich Text with colored text</returns>
        public static string CreateUnityColor(this string text, Color color)
        {
            return $"<color=#{color.ToHEX()}>{text}</color>";
        }
    }
}