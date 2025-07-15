using System.Globalization;

using UnityEngine;

namespace KeepInventory.Helper
{
    public static class ColorHelper
    {
        public static string ToHEX(this UnityEngine.Color color)
            => $"{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}";

        public static UnityEngine.Color FromHEX(this string hex)
        {
            if (hex.StartsWith('#'))
                hex = hex.Substring(1);
            if (hex.Length < 6)
            {
                throw new System.FormatException("Needs a string with a length of at least 6");
            }

            var r = hex.Substring(0, 2);
            var g = hex.Substring(2, 2);
            var b = hex.Substring(4, 2);
            string alpha;
            if (hex.Length >= 8)
                alpha = hex.Substring(6, 2);
            else
                alpha = "FF";

            return new Color((int.Parse(r, NumberStyles.HexNumber) / 255f),
                            (int.Parse(g, NumberStyles.HexNumber) / 255f),
                            (int.Parse(b, NumberStyles.HexNumber) / 255f),
                            (int.Parse(alpha, NumberStyles.HexNumber) / 255f));
        }
    }
}