using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using KeepInventory.Helper;

using UnityEngine;
using UnityEngine.Playables;

using static KeepInventory.Utilities.Gradient;

namespace KeepInventory.Utilities
{
    /// <summary>
    /// JSON struct for <see cref="UnityEngine.Gradient"/>
    /// </summary>
    /// <param name="mode"><inheritdoc cref="Mode"/></param>
    /// <param name="colorKeys"><inheritdoc cref="ColorKeys"/></param>
    /// <param name="alphaKeys"><inheritdoc cref="AlphaKeys"/></param>
    [method: JsonConstructor]
    public struct JSONUnityGradient(GradientMode mode, List<JSONUnityGradientColorKey> colorKeys, List<JSONUnityGradientAlphaKey> alphaKeys)
    {
        /// <summary>
        /// The mode of the gradient
        /// </summary>
        [JsonPropertyName("Mode")]
        public GradientMode Mode = mode;

        /// <summary>
        /// The color keys of the gradient
        /// </summary>
        [JsonPropertyName("ColorKeys")]
        public List<JSONUnityGradientColorKey> ColorKeys = colorKeys;

        /// <summary>
        /// The alpha keys of the gradient
        /// </summary>
        [JsonPropertyName("AlphaKeys")]
        public List<JSONUnityGradientAlphaKey> AlphaKeys = alphaKeys;

        /// <summary>
        /// Converts <see cref="UnityEngine.Gradient"/> to <see cref="JSONUnityGradient"/>
        /// </summary>
        /// <param name="gradient"><see cref="UnityEngine.Gradient"/> to convert</param>
        public static implicit operator JSONUnityGradient(UnityEngine.Gradient gradient)
        {
            var json = new JSONUnityGradient
            {
                Mode = gradient.mode
            };

            var alpha = new List<JSONUnityGradientAlphaKey>();
            gradient.alphaKeys.ForEach(x => alpha.Add(x));
            json.AlphaKeys = alpha;

            var color = new List<JSONUnityGradientColorKey>();
            gradient.colorKeys.ForEach(x => color.Add(x));
            json.ColorKeys = color;

            return json;
        }

        /// <summary>
        /// Converts <see cref="JSONUnityGradient"/> to <see cref="UnityEngine.Gradient"/>
        /// </summary>
        /// <param name="gradient"><see cref="JSONUnityGradient"/> to convert</param>
        public static implicit operator UnityEngine.Gradient(JSONUnityGradient gradient)
        {
            var unity = new UnityEngine.Gradient
            {
                mode = gradient.Mode
            };

            var alpha = new List<GradientAlphaKey>();
            gradient.AlphaKeys.ForEach(x => alpha.Add(x));
            unity.alphaKeys = alpha.ToArray();

            var color = new List<GradientColorKey>();
            gradient.ColorKeys.ForEach(x => color.Add(x));
            unity.colorKeys = color.ToArray();

            return unity;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            int hash = 0;
            hash += (int)Mode;
            ColorKeys?.ForEach(x => hash += x.GetHashCode());
            AlphaKeys?.ForEach(x => hash += x.GetHashCode());
            return hash;
        }

        /// <inheritdoc/>
        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(JSONUnityGradient)) return false;
            var cast = (JSONUnityGradient)obj;
            return cast.Mode == Mode && cast.AlphaKeys == AlphaKeys && cast.ColorKeys == ColorKeys;
        }

        /// <summary>
        /// Checks if one <see cref="JSONUnityGradient"/> is equal to another
        /// </summary>
        /// <param name="left">The first <see cref="JSONUnityGradient"/> to check</param>
        /// <param name="right">The second <see cref="JSONUnityGradient"/> to check</param>
        /// <returns><inheritdoc cref="Equals(object)"/></returns>
        public static bool operator ==(JSONUnityGradient left, JSONUnityGradient right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks if one <see cref="JSONUnityGradient"/> is not equal to another
        /// </summary>
        /// <param name="left">The first <see cref="JSONUnityGradient"/> to check</param>
        /// <param name="right">The second <see cref="JSONUnityGradient"/> to check</param>
        /// <returns><see langword="true"/> if they are not equal, otherwise <see langword="false"/></returns>
        public static bool operator !=(JSONUnityGradient left, JSONUnityGradient right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// JSON Struct for <see cref="GradientAlphaKey"/>
    /// </summary>
    /// <remarks>
    /// Creates new instance of <see cref="JSONUnityGradientAlphaKey"/>
    /// </remarks>
    /// <param name="alpha"><inheritdoc cref="Alpha"/></param>
    /// <param name="time"><inheritdoc cref="Time"/></param>
    [method: JsonConstructor]
    public struct JSONUnityGradientAlphaKey(float alpha, float time)
    {
        /// <summary>
        /// Alpha value of the key
        /// </summary>
        [JsonPropertyName("Alpha")]
        public float Alpha = alpha;

        /// <summary>
        /// The position of the key
        /// </summary>
        [JsonPropertyName("Time")]
        public float Time = time;

        /// <summary>
        /// Converts <see cref="GradientAlphaKey"/> to <see cref="JSONUnityGradientAlphaKey"/>
        /// </summary>
        /// <param name="key"><see cref="GradientAlphaKey"/> to convert</param>
        public static implicit operator JSONUnityGradientAlphaKey(GradientAlphaKey key)
        {
            return new JSONUnityGradientAlphaKey(key.alpha, key.time);
        }

        /// <summary>
        /// Converts <see cref="JSONUnityGradientAlphaKey"/> to <see cref="GradientAlphaKey"/>
        /// </summary>
        /// <param name="key"><see cref="JSONUnityGradientAlphaKey"/> to convert</param>
        public static implicit operator GradientAlphaKey(JSONUnityGradientAlphaKey key)
        {
            return new GradientAlphaKey(key.Alpha, key.Time);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return Alpha.GetHashCode() + Time.GetHashCode();
        }
    }

    /// <summary>
    /// JSON Struct for <see cref="GradientColorKey"/>
    /// </summary>
    /// <remarks>
    /// Creates new instance of <see cref="JSONUnityGradientColorKey"/>
    /// </remarks>
    /// <param name="color"><inheritdoc cref="Color"/></param>
    /// <param name="time"><inheritdoc cref="Time"/></param>
    [method: JsonConstructor]
    public struct JSONUnityGradientColorKey(JSONColor color, float time)
    {
        /// <summary>
        /// Color of the key
        /// </summary>
        [JsonPropertyName("Color")]
        public JSONColor Color = color;

        /// <summary>
        /// The position of the key
        /// </summary>
        [JsonPropertyName("Time")]
        public float Time = time;

        /// <summary>
        /// Converts <see cref="GradientColorKey"/> to <see cref="JSONUnityGradientColorKey"/>
        /// </summary>
        /// <param name="key"><see cref="GradientColorKey"/> to convert</param>
        public static implicit operator JSONUnityGradientColorKey(GradientColorKey key)
        {
            return new JSONUnityGradientColorKey(key.color, key.time);
        }

        /// <summary>
        /// Converts <see cref="JSONUnityGradientColorKey"/> to <see cref="GradientColorKey"/>
        /// </summary>
        /// <param name="key"><see cref="JSONUnityGradientColorKey"/> to convert</param>
        public static implicit operator GradientColorKey(JSONUnityGradientColorKey key)
        {
            return new GradientColorKey(key.Color, key.Time);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return Color.GetHashCode() + Time.GetHashCode();
        }
    }
}