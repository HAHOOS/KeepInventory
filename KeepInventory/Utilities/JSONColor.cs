using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.Json.Serialization;

using static KeepInventory.Utilities.Gradient;

namespace KeepInventory.Utilities
{
    /// <summary>
    /// Color object to be used for JSON
    /// </summary>
    public struct JSONColor
    {
        /// <summary>
        /// HEX format of the color
        /// </summary>
        [JsonPropertyName("HEX")]
        public string HEX
        {
            readonly get
            {
                return ((Color)this).ToHEX(true);
            }
            set
            {
                var color = Gradient.FromHex(value);
                this.Alpha = color.A;
                this.Red = color.R;
                this.Green = color.G;
                this.Blue = color.B;
            }
        }

        [JsonIgnore]
        private int _alpha;

        /// <summary>
        /// Alpha of the color
        /// </summary>
        [JsonIgnore]
        public int Alpha
        {
            readonly get => _alpha;
            set { _alpha = Math.Clamp(value, 0, 255); }
        }

        [JsonIgnore]
        private int _red;

        /// <summary>
        /// Red of the color
        /// </summary>
        [JsonIgnore]
        public int Red
        {
            readonly get => _red;
            set { _red = Math.Clamp(value, 0, 255); }
        }

        [JsonIgnore]
        private int _green;

        /// <summary>
        /// Green of the color
        /// </summary>
        [JsonIgnore]
        public int Green
        {
            readonly get => _green;
            set { _green = Math.Clamp(value, 0, 255); }
        }

        [JsonIgnore]
        private int _blue;

        /// <summary>
        /// Blue of the color
        /// </summary>
        [JsonIgnore]
        public int Blue
        {
            readonly get => _blue;
            set { _blue = Math.Clamp(value, 0, 255); }
        }

        /// <summary>
        /// Creates <see cref="JSONColor"/> from HEX
        /// </summary>
        /// <param name="HEX">Color in the HEX format</param>
        [JsonConstructor]
        public JSONColor(string HEX)
        {
            this.HEX = HEX;
        }

        /// <summary>
        /// Creates <see cref="JSONColor"/> from ARGB
        /// </summary>
        /// <param name="Alpha">Alpha value of ARGB</param>
        /// <param name="Red">Red value of ARGB</param>
        /// <param name="Green">Green value of ARGB</param>
        /// <param name="Blue">Blue value of ARGB</param>
        public JSONColor(int Alpha, int Red, int Green, int Blue)
        {
            this.Alpha = Alpha;
            this.Red = Red;
            this.Green = Green;
            this.Blue = Blue;
        }

        /// <summary>
        /// Creates <see cref="JSONColor"/> from a <see cref="Color"/>
        /// </summary>
        /// <param name="drawingColor"><see cref="Color"/> used to create <see cref="JSONColor"/></param>
        public JSONColor(Color drawingColor)
        {
            this.Alpha = drawingColor.A;
            this.Red = drawingColor.R;
            this.Green = drawingColor.G;
            this.Blue = drawingColor.B;
        }

        /// <summary>
        /// Creates <see cref="JSONColor"/> from a <see cref="UnityEngine.Color"/>
        /// </summary>
        /// <param name="unityColor"><see cref="UnityEngine.Color"/> used to create <see cref="JSONColor"/></param>
        public JSONColor(UnityEngine.Color unityColor)
        {
            this.Alpha = Round(unityColor.a * 255);
            this.Red = Round(unityColor.r * 255);
            this.Green = Round(unityColor.g * 255);
            this.Blue = Round(unityColor.b * 255);
        }

        /// <summary>
        /// Creates <see cref="JSONColor"/> from a <see cref="ColorRGB"/>
        /// </summary>
        /// <param name="color"><see cref="ColorRGB"/> used to create <see cref="JSONColor"/></param>
        public JSONColor(ColorRGB color)
        {
            this.Alpha = 0;
            this.Red = color.R;
            this.Green = color.G;
            this.Blue = color.B;
        }

        /// <summary>
        /// Creates <see cref="JSONColor"/> from HSL
        /// </summary>
        /// <param name="Hue">Hue value of HSL</param>
        /// <param name="Saturation">Saturation value of HSL</param>
        /// <param name="Lightness">Lightness value of HSL</param>
        public JSONColor(float Hue, float Saturation, float Lightness)
        {
            var color = ColorRGB.HSL2RGB(Hue, Saturation, Lightness);
            this.Alpha = 0;
            this.Red = color.R;
            this.Green = color.G;
            this.Blue = color.B;
        }

        /// <summary>
        /// Returns a string with the ARGB values
        /// </summary>
        /// <returns>String with ARGB values</returns>
        public override readonly string ToString() => ((Color)this).ToHEX(true);

        /// <summary>
        /// Checks if the provided object is equal to this. If it won't be a
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/></returns>
        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj == null)
                return false;
            JSONColor color;
            if (obj.GetType() != typeof(JSONColor))
            {
                // Why does the IDE think the assignment is unnecessary
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                if (obj.GetType() == typeof(Color))
                    color = new JSONColor((Color)obj);
                else if (obj.GetType() == typeof(ColorRGB))
                    color = new JSONColor((ColorRGB)obj);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                return false;
            }
            else
            {
                color = (JSONColor)obj;
            }
            return color.Alpha == Alpha && color.Red == Red && color.Blue == Blue && color.Green == Green;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode() => HashCode.Combine(Alpha, Red, Green, Blue);

        /// <summary>
        /// Convert <see cref="Color"/> to <see cref="JSONColor"/>
        /// </summary>
        /// <param name="color"><see cref="Color"/> to convert</param>
        public static implicit operator JSONColor(Color color)
        {
            return new JSONColor(color);
        }

        /// <summary>
        /// Convert <see cref="UnityEngine.Color"/> to <see cref="JSONColor"/>
        /// </summary>
        /// <param name="color"><see cref="UnityEngine.Color"/> to convert</param>
        public static implicit operator JSONColor(UnityEngine.Color color)
        {
            return new JSONColor(color);
        }

        /// <summary>
        /// Convert <see cref="JSONColor"/> to <see cref="UnityEngine.Color"/>
        /// </summary>
        /// <param name="color"><see cref="JSONColor"/> to convert</param>
        public static implicit operator UnityEngine.Color(JSONColor color)
        {
            return new UnityEngine.Color(color.Red / 255, color.Green / 255, color.Blue / 255, color.Alpha / 255);
        }

        /// <summary>
        /// Convert <see cref="ColorRGB"/> to <see cref="JSONColor"/>
        /// </summary>
        /// <param name="color"><see cref="ColorRGB"/> to convert</param>
        public static implicit operator JSONColor(ColorRGB color)
        {
            return new JSONColor(color);
        }

        /// <summary>
        /// Convert <see cref="JSONColor"/> to <see cref="Color"/>
        /// </summary>
        /// <param name="color"><see cref="JSONColor"/> to convert</param>
        public static implicit operator Color(JSONColor color)
        {
            return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }

        /// <summary>
        /// Convert <see cref="JSONColor"/> to <see cref="ColorRGB"/>
        /// </summary>
        /// <param name="color"><see cref="JSONColor"/> to convert</param>
        public static implicit operator ColorRGB(JSONColor color)
        {
            return new ColorRGB(color);
        }

        /// <summary>
        /// Checks if one <see cref="JSONColor"/> is equal to another
        /// </summary>
        /// <param name="left">The first <see cref="JSONColor"/> to check</param>
        /// <param name="right">The second <see cref="JSONColor"/> to check</param>
        /// <returns><inheritdoc cref="Equals(object)"/></returns>
        public static bool operator ==(JSONColor left, JSONColor right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks if one <see cref="JSONColor"/> is not equal to another
        /// </summary>
        /// <param name="left">The first <see cref="JSONColor"/> to check</param>
        /// <param name="right">The second <see cref="JSONColor"/> to check</param>
        /// <returns><see langword="true"/> if they are not equal, otherwise <see langword="false"/></returns>
        public static bool operator !=(JSONColor left, JSONColor right)
        {
            return !(left == right);
        }
    }
}