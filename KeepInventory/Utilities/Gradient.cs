using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace KeepInventory.Utilities;

/// <summary>
/// Class responsible for making gradients
/// </summary>
// This could definitely be better and has a lot of "borrowed" code
public static class Gradient
{
    /// <summary>
    /// Rounds a <see cref="float"/> number
    /// </summary>
    /// <param name="num">Number to round</param>
    /// <returns>A rounded number</returns>
    public static int Round(float num)
    {
        return (int)MathF.Round(num);
    }

    /// <summary>
    /// The default value for <see cref="GradientReturnType"/>
    /// </summary>
    public static GradientReturnType DefaultReturnType { get; set; } = GradientReturnType.UnityRichText;

    /// <summary>
    /// Get a gradient from a start <see cref="Color"/> and end <see cref="Color"/>
    /// </summary>
    /// <param name="start">The start <see cref="Color"/> of the gradient</param>
    /// <param name="end">The end <see cref="Color"/> of the gradient</param>
    /// <param name="steps">The amount of colors should it make for a gradient</param>
    /// <returns>A list of <see cref="Color"/> that can be used as a gradient</returns>
    // Politely borrowed from https://stackoverflow.com/questions/2011832/generate-color-gradient-in-c-sharp
    public static IEnumerable<Color> GetStandardGradient(Color start, Color end, int steps)
    {
        float stepA = (float)(end.A - start.A) / steps;
        float stepR = (float)(end.R - start.R) / steps;
        float stepG = (float)(end.G - start.G) / steps;
        float stepB = (float)(end.B - start.B) / steps;

        List<Color> result = [];

        (float A, float R, float G, float B) = (start.A, start.R, start.G, start.B);

        for (int i = 0; i < steps; i++)
        {
            result.Add(Color.FromArgb(Round(A), Round(R), Round(G), Round(B)));
            A += stepA;
            R += stepR;
            G += stepG;
            B += stepB;
        }
        return result;
    }

    /// <summary>
    /// Get a gradient from a start <see cref="Color"/> and end <see cref="Color"/>
    /// </summary>
    /// <param name="start">The start <see cref="Color"/> of the gradient</param>
    /// <param name="end">The end <see cref="Color"/> of the gradient</param>
    /// <param name="steps">The amount of colors should it make for a gradient</param>
    /// <returns>A list of <see cref="Color"/> that can be used as a gradient</returns>
    // Politely borrowed and modified from https://stackoverflow.com/questions/2011832/generate-color-gradient-in-c-sharp
    // The logic behind the making of the gradient was used from https://github.com/KanatiMC/Unity-Gradient-Maker/blob/main/js/textcolorizer.js. Credits go to them
    public static IEnumerable<Color> GetMiddleGradient(Color start, Color end, int steps)
    {
        float stepA = (end.A - start.A) / MathF.Floor(steps / 2);
        float stepR = (end.R - start.R) / MathF.Floor(steps / 2);
        float stepG = (end.G - start.G) / MathF.Floor(steps / 2);
        float stepB = (end.B - start.B) / MathF.Floor(steps / 2);

        List<Color> result = [];

        (float A, float R, float G, float B) = (start.A, start.R, start.G, start.B);

        for (int i = 0; i < steps; i++)
        {
            result.Add(Color.FromArgb(Round(A), Round(R), Round(G), Round(B)));
            if (i < MathF.Floor(steps / 2))
            {
                A += stepA;
                R += stepR;
                G += stepG;
                B += stepB;
            }
            else
            {
                A -= stepA;
                R -= stepR;
                G -= stepG;
                B -= stepB;
            }
        }
        return result;
    }

    /// <summary>
    /// Get a gradient from a start <see cref="Color"/> and end <see cref="Color"/>
    /// </summary>
    /// <param name="start">The start <see cref="Color"/> of the gradient</param>
    /// <param name="middle">The middle <see cref="Color"/> of the gradient</param>
    /// <param name="end">The end <see cref="Color"/> of the gradient</param>
    /// <param name="steps">The amount of colors should it make for a gradient</param>
    /// <returns>A list of <see cref="Color"/> that can be used as a gradient</returns>
    // Politely borrowed and modified from https://stackoverflow.com/questions/2011832/generate-color-gradient-in-c-sharp
    // The logic behind the making of the gradient was used from https://github.com/KanatiMC/Unity-Gradient-Maker/blob/main/js/textcolorizer.js. Credits go to them
    public static IEnumerable<Color> GetThreeColoredGradient(Color start, Color middle, Color end, int steps)
    {
        float _stepA = (middle.A - start.A) / MathF.Floor(steps / 2);
        float _stepR = (middle.R - start.R) / MathF.Floor(steps / 2);
        float _stepG = (middle.G - start.G) / MathF.Floor(steps / 2);
        float _stepB = (middle.B - start.B) / MathF.Floor(steps / 2);

        float stepA = (end.A - middle.A) / MathF.Floor(steps / 2);
        float stepR = (end.R - middle.R) / MathF.Floor(steps / 2);
        float stepG = (end.G - middle.G) / MathF.Floor(steps / 2);
        float stepB = (end.B - middle.B) / MathF.Floor(steps / 2);

        List<Color> result = [];

        (float A, float R, float G, float B) = (start.A, start.R, start.G, start.B);

        for (int i = 0; i < steps; i++)
        {
            result.Add(Color.FromArgb(Round(A), Round(R), Round(G), Round(B)));
            if (i < MathF.Floor(steps / 2))
            {
                A += _stepA;
                R += _stepR;
                G += _stepG;
                B += _stepB;
            }
            else
            {
                A += stepA;
                R += stepR;
                G += stepG;
                B += stepB;
            }
        }
        return result;
    }

    /// <summary>
    /// Get length of text, removing spaces as well
    /// </summary>
    /// <param name="text">The text to get length of</param>
    /// <returns>Length of text without spaces</returns>
    public static int GetTextLength(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        text = text.Replace(" ", "");
        return text.Length;
    }

    /// <summary>
    /// Converts <see cref="Color"/> to HEX
    /// </summary>
    /// <param name="color">The <see cref="Color"/> you want to convert</param>
    /// <returns>HEX format of the provided <see cref="Color"/></returns>
    public static string ToHEX(this Color color, bool useAlpha = false)
    {
        return $"{color.R:X2}{color.G:X2}{color.B:X2}{(useAlpha ? color.A.ToString("X2") : "FF")}";
    }

    /// <summary>
    /// Converts HEX to <see cref="Color"/>
    /// </summary>
    /// <param name="hex">The HEX you want to convert</param>
    /// <returns><see cref="Color"/> from HEX</returns>
    public static Color FromHex(string hex, bool useAlpha = false)
    {
        if (string.IsNullOrWhiteSpace(hex) || hex.Length < 6) return Color.Empty;
        try
        {
            var color = ColorTranslator.FromHtml(hex.StartsWith('#') ? hex : $"#{hex}");
            if (!useAlpha) color = Color.FromArgb(255, color);
            return color;
        }
        catch (Exception)
        {
            return Color.Empty;
        }
    }

    /// <summary>
    /// Converts <see cref="UnityEngine.Color"/> to HEX
    /// </summary>
    /// <param name="color">The <see cref="UnityEngine.Color"/> you want to convert</param>
    /// <returns>HEX format of the provided <see cref="UnityEngine.Color"/></returns>
    public static string ToHEX(this UnityEngine.Color color, bool useAlpha = false)
    {
        return $"{color.r:X2}{color.g:X2}{color.b:X2}{(useAlpha ? color.a.ToString("X2") : "FF")}";
    }

    /// <summary>
    /// Converts HEX to <see cref="Color"/>
    /// </summary>
    /// <param name="hex">The HEX you want to convert</param>
    /// <returns><see cref="Color"/> from HEX</returns>
    public static UnityEngine.Color UnityFromHex(string hex, bool useAlpha = false)
    {
        if (string.IsNullOrWhiteSpace(hex) || hex.Length < 6) return UnityEngine.Color.white;
        try
        {
            var color = ColorTranslator.FromHtml(hex.StartsWith('#') ? hex : $"#{hex}");
            return new UnityEngine.Color(color.R / 255, color.G / 255, color.B / 255, useAlpha ? color.A / 255 : 255);
        }
        catch (Exception)
        {
            return UnityEngine.Color.white;
        }
    }

    /// <summary>
    /// Converts a <see cref="System.Drawing.Color"/> to a <see cref="UnityEngine.Color"/>
    /// </summary>
    /// <param name="color">The <see cref="System.Drawing.Color"/> to convert</param>
    /// <returns>A <see cref="UnityEngine.Color"/></returns>
    public static UnityEngine.Color UnityColorFromDrawingColor(this Color color, bool useAlpha = false)
    {
        return new UnityEngine.Color(useAlpha ? color.R / 255 : 1, color.G / 255, color.B / 255, color.A / 255);
    }

    /// <summary>
    /// Converts a <see cref="System.Drawing.Color"/> to a <see cref="UnityEngine.Color"/>
    /// </summary>
    /// <param name="color">The <see cref="System.Drawing.Color"/> to convert</param>
    /// <returns>A <see cref="UnityEngine.Color"/></returns>
    public static Color DrawingColorFromUnityColor(this UnityEngine.Color color, bool useAlpha = false)
    {
        return Color.FromArgb(useAlpha ? Round(color.a * 255) : 255, Round(color.r * 255), Round(color.g * 255), Round(color.b * 255));
    }

    /// <summary>
    /// Returns a random <see cref="Color"/>
    /// </summary>
    /// <param name="random">Ignore</param>
    /// <returns>A random color</returns>
    public static Color NextColor(this Random random)
    {
        return Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
    }

    /// <summary>
    /// Returns a random <see cref="Color"/> with values ranging from the start <see cref="Color"/> to an end <see cref="Color"/>
    /// </summary>
    /// <param name="random">Ignore</param>
    /// <param name="start">Start <see cref="Color"/> of the range</param>
    /// <param name="end">End <see cref="Color"/> of the range</param>
    /// <returns>A random <see cref="Color"/> within the range</returns>
    public static Color NextColor(this Random random, Color start, Color end)
    {
        return Color.FromArgb(random.Next(start.A, end.A), random.Next(start.R, end.R), random.Next(start.G, end.G), random.Next(start.B, end.B));
    }

    /// <summary>
    /// Returns a random <see cref="Color"/>
    /// </summary>
    /// <returns>A random <see cref="Color"/></returns>
    public static Color RandomColor()
    {
        var random = new Random();
        return Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
    }

    /// <summary>
    /// Returns a random <see cref="Color"/> with values ranging from the start <see cref="Color"/> to an end <see cref="Color"/>
    /// </summary>
    /// <param name="start">Start <see cref="Color"/> of the range</param>
    /// <param name="end">End <see cref="Color"/> of the range</param>
    /// <returns>A random <see cref="Color"/> within the range</returns>
    public static Color RandomColor(Color start, Color end)
    {
        var random = new Random();
        return Color.FromArgb(random.Next(start.A, end.A), random.Next(start.R, end.R), random.Next(start.G, end.G), random.Next(start.B, end.B));
    }

    /// <summary>
    /// Creates a rainbow gradient (list of colors) for a given text
    /// </summary>
    /// <param name="text">The text you want to create the rainbow gradient for</param>
    /// <returns>Rainbow gradient as a <see cref="IEnumerable{Color}"/> that can be used in other methods</returns>
    // Modified version of https://stackoverflow.com/questions/2288498/how-do-i-get-a-rainbow-color-gradient-in-c
    public static IEnumerable<Color> GetRainbowGradient(string text)
    {
        int steps = text.GetTextLength();
        List<Color> result = [];
        double add = 1.0 / steps;
        for (double i = 0; i < 1; i += add)
        {
            ColorRGB c = ColorRGB.HSL2RGB(i, 0.5, 0.5);
            result.Add(c);
        }
        return result;
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

    /// <summary>
    /// Adds color to text with ANSI
    /// <para>This will not properly handle existing ANSI colors in text</para>
    /// </summary>
    /// <param name="text">The text you want to add color to</param>
    /// <param name="color">The <see cref="Color"/></param>
    /// <returns>ANSI with colored text</returns>
    public static string CreateANSIColor(this string text, Color color)
    {
        return $"\u001b[38;2;{color.R};{color.G};{color.B}m{text}\u001b[0m";
    }

    private static string ReplacePlaceholders(string text, char @char, Color color)
    {
        ColorRGB.RGB2HSL(new ColorRGB(color), out double hue, out double saturation, out double lightness);

        Dictionary<string, string> placeholders = new()
        {
            { "%#hex%", $"#{color.ToHEX()}" },
            { "%hex%", color.ToHEX() },
            { "%char%", @char.ToString() },
            // In case someone decides to not use %char%
            { "%text%", @char.ToString() },
            { "%color_a%", color.A.ToString() },
            { "%color_r%", color.R.ToString() },
            { "%color_g%", color.G.ToString() },
            { "%color_b%", color.B.ToString() },
            { "%color_h%", hue.ToString() },
            { "%color_s%", saturation.ToString() },
            { "%color_l%", lightness.ToString() },
        };

        foreach (var placeholder in placeholders)
        {
            text = text.Replace(placeholder.Key, placeholder.Value, StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }

    /// <summary>
    /// Create a horizontal gradient
    /// </summary>
    /// <param name="text">The text you want to add the gradient to</param>
    /// <param name="start">Start color of the horizontal gradient</param>
    /// <param name="end">End color of the horizontal gradient</param>
    /// <param name="returnType">The format to return the gradient in</param>
    /// <returns>A gradient for provided text</returns>
    public static string CreateStandardGradient(this string text, Color start, Color end, GradientReturnType? returnType = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        return CreateGradient(text, GetStandardGradient(start, end, text.GetTextLength()), returnType != null ? (GradientReturnType)returnType : DefaultReturnType);
    }

    /// <summary>
    /// Create a middle gradient
    /// </summary>
    /// <param name="text">The text you want to add the gradient to</param>
    /// <param name="start">Start color of the middle gradient</param>
    /// <param name="end">End color of the middle gradient</param>
    /// <param name="returnType">The format to return the gradient in</param>
    /// <returns>A gradient for provided text</returns>
    public static string CreateMiddleGradient(this string text, Color start, Color end, GradientReturnType? returnType = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        return CreateGradient(text, GetMiddleGradient(start, end, text.GetTextLength()), returnType != null ? (GradientReturnType)returnType : DefaultReturnType);
    }

    /// <summary>
    /// Create a three colored gradient
    /// </summary>
    /// <param name="text">The text you want to add the gradient to</param>
    /// <param name="start">Start color of the three colored gradient</param>
    /// <param name="middle">Middle color of the three colored gradient</param>
    /// <param name="end">End color of the three colored gradient</param>
    /// <param name="returnType">The format to return the gradient in</param>
    /// <returns>A gradient for provided text</returns>
    public static string CreateThreeColoredGradient(this string text, Color start, Color middle, Color end, GradientReturnType? returnType = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        return CreateGradient(text, GetThreeColoredGradient(start, middle, end, text.GetTextLength()), returnType != null ? (GradientReturnType)returnType : DefaultReturnType);
    }

    /// <summary>
    /// Creates a rainbow gradient
    /// </summary>
    /// <param name="text">The text you want to add the gradient to</param>
    /// <param name="returnType">The format to return the gradient in</param>
    /// <returns>A gradient for provided text</returns>
    public static string CreateRainbowGradient(this string text, GradientReturnType? returnType = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        return CreateGradient(text, GetRainbowGradient(text), returnType != null ? (GradientReturnType)returnType : DefaultReturnType);
    }

    /// <summary>
    /// Creates a gradient from the provided list of colors
    /// </summary>
    /// <param name="text">The text you want to add the gradient to</param>
    /// <param name="colors">An <see cref="IEnumerable{T}"/> of <see cref="Color"/>s for the gradient</param>
    /// <param name="returnType">The format to return the gradient in</param>
    /// <returns>A gradient for provided text</returns>
    public static string CreateGradient(this string text, IEnumerable<Color> colors, GradientReturnType? returnType = null)
    {
        returnType ??= DefaultReturnType;
        if (string.IsNullOrWhiteSpace(text)) return text;
        // Placeholders:
        // %#hex% - HEX Format of the color with #
        // %hex% - HEX Format of the color without #
        // %text% / %char% - The character that will be colored
        // %color_a% - Alpha value of the color
        // %color_r% - Red value of the color
        // %color_g% - Green value of the color
        // %color_b% - Blue value of the color
        // %color_h% - Hue value of the color
        // %color_s% - Saturation value of the color
        // %color_l% - Lightness value of the color

        const string unityRichText_format = "<color=%#hex%>%char%</color>";
        const string ansi_format = "\u001b[38;2;%color_r%;%color_g%;%color_b%m%char%\u001b[0m";
        const string html_format = "<span style='color:%#hex%;'>%char%</span>";
        const string bbcode_format = "[color=%#hex%]%char%[/color]";

        var gradient = colors.GetEnumerator();
        string @return = string.Empty;
        gradient.MoveNext();
        string format = returnType == GradientReturnType.HTML ? html_format : returnType == GradientReturnType.ANSI ? ansi_format : returnType == GradientReturnType.UnityRichText ? unityRichText_format : bbcode_format;
        foreach (var @char in text)
        {
            if (@char == ' ')
            {
                @return += @char;
                continue;
            }
            string replace = ReplacePlaceholders(format, @char, gradient.Current);

            @return += replace;
            bool moveNext = gradient.MoveNext();
            if (!moveNext)
            {
                gradient = colors.GetEnumerator();
                gradient.MoveNext();
            }
        }
        return @return;
    }

    /// <summary>
    /// Converts the unity rich text gradient to ANSI
    /// </summary>
    /// <param name="text">The text containing Unity Rich Text</param>
    /// <returns>ANSI converted from Unity Rich Text</returns>
    public static string ConvertUnityRichTextToANSI(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        Regex regex = new(@"<color=#(.*?)>(.*?)<\/color>");

        static string _match(Match match)
        {
            if (match.Success)
            {
                const int _colorIndex = 1;
                const int _textIndex = 2;

                if (match.Groups.Count >= 3)
                {
                    var color = FromHex(match.Groups[_colorIndex].Value);
                    string @char = match.Groups[_textIndex].Value;

                    string ansi = @char.CreateANSIColor(color);

                    return ansi;
                }
            }
            return string.Empty;
        }
        var replaced = regex.Replace(text, new MatchEvaluator(_match));
        return Regex.Replace(replaced, @"<\/?(?!\/?color)(.*?)>", string.Empty);
    }

    /// <summary>
    /// Removes unity rich text from provided text
    /// </summary>
    /// <param name="text">The text containing Unity Rich Text</param>
    /// <returns>Text without Unity Rich Text</returns>
    public static string RemoveUnityRichText(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        Regex regex = new(@"<color=#(.*?)>(.*?)<\/color>");

        static string _match(Match match)
        {
            if (match.Success)
            {
                const int _textIndex = 2;

                string @char = match.Groups[_textIndex].Value;
                return @char;
            }
            return string.Empty;
        }
        var replaced = regex.Replace(text, new MatchEvaluator(_match));
        return Regex.Replace(replaced, @"<\/?(?!\/?color)(.*?)>", string.Empty);
    }

    /// <summary>
    /// Create a gradient for provided text from a <see cref="UnityEngine.Gradient"/>
    /// </summary>
    /// <param name="text">The text you want to add the gradient to</param>
    /// <param name="gradient">The <see cref="UnityEngine.Gradient"/> to apply</param>
    /// <param name="returnType">The format to return the gradient in</param>
    /// <returns>A gradient for provided text</returns>
    public static string GenerateUnityGradient(this string text, UnityEngine.Gradient gradient, GradientReturnType? returnType = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        returnType ??= DefaultReturnType;

        var length = text.GetTextLength();
        if (length == 0) return text;

        float by = 1f / (float)length;

        List<Color> colors = [];

        for (int i = 1; i <= length; i++)
        {
            colors.Add(gradient.Evaluate(by * i).DrawingColorFromUnityColor());
        }

        return CreateGradient(text, colors.AsEnumerable(), returnType);
    }

    /// <summary>
    /// The format in which should the gradient be returned in
    /// </summary>
    public enum GradientReturnType
    {
        /// <summary>
        /// The returned value is made to work with HTML/Markdown
        /// </summary>
        HTML,

        /// <summary>
        /// The returned value is made to work with ANSI (used in Console)
        /// </summary>
        ANSI,

        /// <summary>
        /// The returned value is made to work with Unity Rich Text
        /// </summary>
        UnityRichText,

        /// <summary>
        /// The returned value is made to work with BBCode (Used on forums, not sure why you would need that, but added it, because why not)
        /// </summary>
        // I don't know why you would want to use this, but added this, because why not
        BBCode
    }

    /// <summary>
    /// Type of a gradient
    /// </summary>
    public enum GradientType
    {
        /// <summary>
        /// Standard gradient, starts from one color and ends with another
        /// </summary>
        Standard,

        /// <summary>
        /// Middle gradient, starts with one color, ends in middle with another and then goes back to the first color
        /// </summary>
        Middle,

        /// <summary>
        /// Three colored, starts with one color, ends in middle with a second color and ends with a third
        /// </summary>
        ThreeColored,

        /// <summary>
        /// Rainbow gradient, self explanatory
        /// </summary>
        Rainbow
    }

    /// <summary>
    /// Class that contains information about a gradient
    /// </summary>
    public struct GradientObject
    {
        /// <summary>
        /// Type of the gradient
        /// </summary>
        [JsonPropertyName("Type")]
        public GradientType Type { get; set; }

        /// <summary>
        /// The start color of the gradient, corner color if the <see cref="Type"/> is <see cref="GradientType.Middle"/>
        /// </summary>
        [JsonPropertyName("Start")]
        public JSONColor? Start { get; set; } = null;

        /// <summary>
        /// The middle color of the gradient, only used in <see cref="GradientType.ThreeColored"/>
        /// </summary>
        [JsonPropertyName("Middle")]
        public JSONColor? Middle { get; set; } = null;

        /// <summary>
        /// The end color of the gradient
        /// </summary>
        [JsonPropertyName("End")]
        public JSONColor? End { get; set; } = null;

        /// <summary>
        /// The unity gradient if specified
        /// </summary>
        [JsonPropertyName("UnityGradient")]
        public JSONUnityGradient? UnityGradient { get; set; } = null;

        /// <summary>
        /// Create new instance of <see cref="GradientObject"/>
        /// </summary>
        public GradientObject()
        {
        }

        /// <summary>
        /// Create new instance of <see cref="GradientObject"/>
        /// </summary>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        /// <param name="start"><inheritdoc cref="Start"/></param>
        /// <param name="middle"><inheritdoc cref="Middle"/></param>
        /// <param name="end"><inheritdoc cref="End"/></param>
        [JsonConstructor]
        public GradientObject(GradientType type, JSONColor? start = null, JSONColor? middle = null, JSONColor? end = null)
        {
            Type = type;
            Start = start;
            Middle = middle;
            End = end;
        }

        /// <summary>
        /// Create new instance of <see cref="GradientObject"/>
        /// </summary>
        /// <param name="unityGradient"><inheritdoc cref="UnityGradient"/></param>
        public GradientObject(UnityEngine.Gradient unityGradient)
        {
            UnityGradient = unityGradient;
        }

        /// <summary>
        /// Generate a gradient for provided text from current gradient object
        /// </summary>
        /// <param name="text">Text to generate the gradient for</param>
        /// <param name="returnType">In what format should the gradient be returned in</param>
        /// <returns>A text with gradient in provided format</returns>
        public readonly string GenerateGradient(string text, GradientReturnType? returnType = null)
        {
            returnType ??= DefaultReturnType;
            if (string.IsNullOrWhiteSpace(text)) return text;

            if (UnityGradient == null)
            {
                var start = Start.GetValueOrDefault();
                var middle = Start.GetValueOrDefault();
                var end = Start.GetValueOrDefault();

                if (Type != GradientType.Rainbow)
                {
                    if (!Start.HasValue || Start == null) throw new NullReferenceException("Start color cannot be null!");
                    if (!End.HasValue || End == null) throw new NullReferenceException("End color cannot be null!");
                    if (Type == GradientType.ThreeColored)
                    {
                        if (!Middle.HasValue || Middle == null) throw new NullReferenceException("Middle color cannot be null!");
                    }
                }
                return Type == GradientType.Standard ? text.CreateStandardGradient(start, end, returnType) : Type == GradientType.Middle ? text.CreateMiddleGradient(start, end, returnType) : Type == GradientType.ThreeColored ? text.CreateThreeColoredGradient(start, middle, end, returnType) : text.CreateRainbowGradient(returnType);
            }
            else
            {
                Core.Logger.Msg("unity gradient");
                return text.GenerateUnityGradient(UnityGradient, returnType);
            }
        }

        /// <summary>
        /// Generate a gradient for provided text from current gradient object without throwing exceptions
        /// </summary>
        /// <param name="text">Text to generate the gradient for</param>
        /// <param name="returned">Text with gradient in provided format</param>
        /// <param name="returnType">In what format should the gradient be returned in</param>
        /// <returns><see langword="true"/> if done successfully, otherwise <see langword="false"/></returns>
        public readonly bool TryGenerateGradient(string text, out string returned, GradientReturnType? returnType = null)
        {
            try
            {
                returned = GenerateGradient(text, returnType);
                return true;
            }
            catch (ArgumentNullException ex)
            {
                Core.Logger.Error($"Parameter '{ex.ParamName}' cannot be null, failed to generate gradient");
                returned = null;
                return false;
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"An unexpected error has occurred while generating gradient, exception:\n{ex}");
                returned = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(GradientObject)) return false;
            var cast = (GradientObject)obj;
            return cast.Type == Type && cast.End == End && cast.Start == Start && cast.Middle == Middle && cast.UnityGradient == UnityGradient;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return UnityGradient == null ? (int)Type + (Start?.GetHashCode() ?? 0) + (End?.GetHashCode() ?? 0) + (Middle?.GetHashCode() ?? 0) : UnityGradient.Value.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(GradientObject left, GradientObject right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(GradientObject left, GradientObject right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Holds information about the Red, Green and Blue values
    /// </summary>
    /// <remarks>
    /// Creates new instance of <see cref="ColorRGB"/>
    /// </remarks>
    /// <param name="value"><see cref="Color"/> to create from</param>
    // Politely borrowed from https://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm
    public struct ColorRGB(Color value)
    {
        /// <summary>
        /// Red
        /// </summary>
        public byte R = value.R;

        /// <summary>
        /// Green
        /// </summary>
        public byte G = value.G;

        /// <summary>
        /// Blue
        /// </summary>
        public byte B = value.B;

        /// <summary>
        /// Convert <see cref="ColorRGB"/> to <see cref="Color"/>
        /// </summary>
        /// <param name="rgb">The <see cref="ColorRGB"/> to create <see cref="Color"/> from</param>
        public static implicit operator Color(ColorRGB rgb)

        {
            Color c = Color.FromArgb(rgb.R, rgb.G, rgb.B);

            return c;
        }

        /// <summary>
        /// Convert <see cref="Color"/> to <see cref="ColorRGB"/>
        /// </summary>
        /// <param name="c">The <see cref="Color"/> to create <see cref="ColorRGB"/> from</param>
        public static explicit operator ColorRGB(Color c)

        {
            return new ColorRGB(c);
        }

        /// <summary>
        /// Converts HSL to RGB
        /// </summary>
        /// <param name="h">Hue</param>
        /// <param name="sl">Saturation</param>
        /// <param name="l">Lightness</param>
        /// <returns><see cref="ColorRGB"/> from provided HSL</returns>
        public static ColorRGB HSL2RGB(double h, double sl, double l)

        {
            double v;

            double r, g, b;

            r = l;   // default to gray

            g = l;

            b = l;

            v = l <= 0.5 ? l * (1.0 + sl) : l + sl - (l * sl);

            if (v > 0)

            {
                double m;

                double sv;

                int sextant;

                double fract, vsf, mid1, mid2;

                m = l + l - v;

                sv = (v - m) / v;

                h *= 6.0;

                sextant = (int)h;

                fract = h - sextant;

                vsf = v * sv * fract;

                mid1 = m + vsf;

                mid2 = v - vsf;

                switch (sextant)

                {
                    case 0:

                        r = v;

                        g = mid1;

                        b = m;

                        break;

                    case 1:

                        r = mid2;

                        g = v;

                        b = m;

                        break;

                    case 2:

                        r = m;

                        g = v;

                        b = mid1;

                        break;

                    case 3:

                        r = m;

                        g = mid2;

                        b = v;

                        break;

                    case 4:

                        r = mid1;

                        g = m;

                        b = v;

                        break;

                    case 5:

                        r = v;

                        g = m;

                        b = mid2;

                        break;
                }
            }

            ColorRGB rgb;

            rgb.R = Convert.ToByte(r * 255.0f);

            rgb.G = Convert.ToByte(g * 255.0f);

            rgb.B = Convert.ToByte(b * 255.0f);

            return rgb;
        }

        /// <summary>
        /// Converts RGB to HSL
        /// </summary>
        /// <param name="rgb">The RGB to convert</param>
        /// <param name="h">Converted hue</param>
        /// <param name="s">Converted saturation</param>
        /// <param name="l">Converted lightness</param>
        public static void RGB2HSL(ColorRGB rgb, out double h, out double s, out double l)

        {
            double r = rgb.R / 255.0;

            double g = rgb.G / 255.0;

            double b = rgb.B / 255.0;

            double v;

            double m;

            double vm;

            double r2, g2, b2;

            h = 0; // default to black

            s = 0;

#pragma warning disable IDE0059 // Unnecessary assignment of a value
            l = 0;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            v = Math.Max(r, g);

            v = Math.Max(v, b);

            m = Math.Min(r, g);

            m = Math.Min(m, b);

            l = (m + v) / 2.0;

            if (l <= 0.0)

            {
                return;
            }

            vm = v - m;

            s = vm;

            if (s > 0.0)

            {
                s /= l <= 0.5 ? v + m : 2.0 - v - m;
            }
            else
            {
                return;
            }

            r2 = (v - r) / vm;

            g2 = (v - g) / vm;

            b2 = (v - b) / vm;

            if (r == v)

            {
                h = g == m ? 5.0 + b2 : 1.0 - g2;
            }
            else if (g == v)

            {
                h = b == m ? 1.0 + r2 : 3.0 - b2;
            }
            else
            {
                h = r == m ? 3.0 + g2 : 5.0 - r2;
            }

            h /= 6.0;
        }
    }
}