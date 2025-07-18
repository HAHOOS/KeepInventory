using UnityEngine;

namespace KeepInventory.Helper
{
    public static class ColorHelper
    {
        public static string ToHEX(this Color color)
            => $"{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}";

        public static float[] ToArray(this Color color)
            => [color.r * 255, color.g * 255, color.b * 255];
    }
}