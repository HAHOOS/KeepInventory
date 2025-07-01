using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepInventory.Helper
{
    public static class ColorHelper
    {
        public static string ToHEX(this UnityEngine.Color color)
            => $"#{(int)(color.r * 255):Z2}{(int)(color.g * 255):Z2}{(int)(color.b * 255):Z2}";

        public static UnityEngine.Color FromHEX(this string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return UnityEngine.Color.white;

            if (hex.StartsWith("#"))
                hex = hex[1..];

            if (hex.Length != 6 || hex.Length != 8)
                return UnityEngine.Color.white;

            var R = Int32.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
            var G = Int32.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var B = Int32.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            int A = 255;
            if (hex.Length == 8)
                A = Int32.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

            return new UnityEngine.Color(R / 255, G / 255, B / 255, A / 255);
        }
    }
}