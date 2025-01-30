using System;
using System.Collections.Generic;
using System.Linq;

using BoneLib.BoneMenu;

using KeepInventory.Utilities;
using KeepInventory.Helper;

using UnityEngine;

using static KeepInventory.Utilities.Gradient;

using Gradient = KeepInventory.Utilities.Gradient;

namespace KeepInventory.Menu
{
    /// <summary>
    /// Page that allows to customize a color for a string
    /// </summary>
    /// <remarks>
    /// Create new instance of <see cref="ColorPage"/>
    /// </remarks>
    /// <param name="page">Page for the <see cref="ColorPage"/> to be initialized in</param>
    /// <param name="preview">Preview string</param>
    public class ColorPage(Page page, string preview) : IDisposable
    {
        /// <summary>
        /// Page
        /// </summary>
        public readonly Page Page = page;

        internal event Action Internal_UpdateGradient;

        private Action Internal_UpdatePage;

        /// <summary>
        /// Updates the page with up-to-date settings
        /// </summary>
        public void Update()
        {
            if (Internal_UpdatePage != null && WasSetup) Internal_UpdatePage();
        }

        private bool _addSpaces;

        /// <summary>
        /// Should there be a <see cref="BlankElement"/> between color options in gradients
        /// </summary>
        public bool AddSpaces
        {
            get { return _addSpaces; }
            set
            {
                _addSpaces = value;
                Update();
            }
        }

        /// <summary>
        /// Triggers when the Apply button gets pressed
        /// <para>
        /// The first value is the type of color used (solid color or type of a gradient), the second value will be either <see cref="GradientType"/> (if the type is a type of gradient) or <see cref="Color"/> (if the type is a solid color)
        /// </para>
        /// </summary>
        public event Action<ColorType, object> Applied;

        internal static readonly Dictionary<ColorFormat, List<float>> incrementDecrementValues = new()
        {
            { ColorFormat.RGB, new List<float>() {
               1, 5, 10, 25, 100
            } },
            { ColorFormat.HSL, new List<float>()
            {
               0.01f, 0.05f, 0.1f, 0.25f
            } }
        };

        internal int incrementDecrementIndex = 0;

        private string _preview = preview;

        private ColorType _selectedColorType;

        /// <summary>
        /// The selected <see cref="ColorType"/>, <inheritdoc cref="ColorType"/>
        /// </summary>
        public ColorType SelectedColorType
        {
            get { return _selectedColorType; }
            set
            {
                _selectedColorType = value;
                Update();
            }
        }

        private ColorFormat _selectedColorFormat;

        /// <summary>
        /// The selected <see cref="ColorFormat"/>, <inheritdoc cref="ColorFormat"/>
        /// </summary>
        public ColorFormat SelectedColorFormat
        {
            get { return _selectedColorFormat; }
            set
            {
                _selectedColorFormat = value;
                Update();
            }
        }

        private object _selectedColor;

        /// <summary>
        /// The selected color/gradient
        /// </summary>
        public object SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                if (value.GetType() != typeof(Color) && value.GetType() != typeof(GradientObject))
                    throw new ArgumentException("Cannot set value of SelectedColor to something else than a System.Drawing.Color or a GradientObject!");
                _selectedColor = value;
                Update();
            }
        }

        /// <summary>
        /// Preview string for the page
        /// </summary>
        public string Preview
        {
            get
            {
                return _preview;
            }
            set
            {
                _preview = value;
                Internal_UpdateGradient?.Invoke();
            }
        }

        private static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;
            return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
        }

        const string previewColorString = "■■■■■■■■■■■■";

        internal Dictionary<string, Element> CreateColorElements(ColorType type, ColorFormat colorType, System.Drawing.Color? start = null, System.Drawing.Color? end = null, System.Drawing.Color? middle = null)
        {
            if (type == ColorType.RainbowGradient) return null;
            List<string> names = [];
            var elements = new Dictionary<string, Element>();
            if (type != ColorType.SolidColor)
            {
                names = [
                    type != ColorType.MiddleGradient ? "start" : "corners",
                    type == ColorType.ThreeColoredGradient ? "middle" : type == ColorType.MiddleGradient ? "middle" : null,
                    type != ColorType.MiddleGradient ? "end" : null
                ];
            }
            else
            {
                names = [
                    "color"
                ];
            }

            Dictionary<string, Element> rgb = [];
            Dictionary<string, Element> hsl = [];
            Dictionary<string, Element> hex = [];

            float increment = GetIncrementDecrement(colorType);
            foreach (var _name in names)
            {
                rgb = new Dictionary<string, Element>()
                {
                    { "%name%Color_red", new IntElement("%name% | Red", Color.red, 255, (int)increment, 0, 255, (_) => Internal_UpdateGradient?.Invoke())},
                    { "%name%Color_green", new IntElement("%name% | Green", Color.green, 255, (int)increment, 0, 255, (_) => Internal_UpdateGradient?.Invoke())},
                    { "%name%Color_blue", new IntElement("%name% | Blue", Color.blue, 255, (int)increment, 0, 255, (_) => Internal_UpdateGradient?.Invoke())},
                };

                hsl = new Dictionary<string, Element>()
                {
                    { "%name%Color_hue", new FloatElement("%name% | Hue", Color.white, 1, increment, 0, 1, (_) => Internal_UpdateGradient?.Invoke())},
                    { "%name%Color_saturation", new FloatElement("%name% | Saturation", Color.white, 1, increment, 0, 1, (_) => Internal_UpdateGradient?.Invoke())},
                    { "%name%Color_lightness", new FloatElement("%name% | Lightness", Color.white, 1, increment, 0, 1, (_) => Internal_UpdateGradient?.Invoke())},
                };

                hex = new Dictionary<string, Element>()
                {
                    { "%name%HexString", new StringElement("HEX", Color.cyan, "#ffffff",(_) => Internal_UpdateGradient?.Invoke()) }
                };

                var use = new Dictionary<string, Element>();
                if (AddSpaces) use.Add("%name%Blank", new BlankElement().Element);
                var preview = new FunctionElement(previewColorString, Color.white, null)
                {
                    ElementName = previewColorString
                };
                use.Add("%name%Preview", preview);

                var add = colorType == ColorFormat.RGB ? rgb : colorType == ColorFormat.HSL ? hsl : hex;
                add.ForEach(x => use.Add(x.Key, x.Value));

                if (string.IsNullOrWhiteSpace(_name)) continue;
                string name = (string)_name.Clone();
                var previous = name == "start" ? start : name == "middle" ? middle : name == "end" ? end : System.Drawing.Color.White;
                foreach (var item in use)
                {
                    var key = item.Key;
                    key = key.Replace("%name%", name);

                    var value = item.Value;
                    value.ElementName = FirstCharToUpper(value.ElementName.Replace("%name%", name));

                    void convert()
                    {
                        if (previous == null) return;
                        if (colorType == ColorFormat.RGB)
                        {
                            if (item.Key.EndsWith("red")) (value as IntElement).Value = previous.HasValue ? previous.Value.R : 255;
                            else if (item.Key.EndsWith("green")) (value as IntElement).Value = previous.HasValue ? previous.Value.G : 255;
                            else if (item.Key.EndsWith("blue")) (value as IntElement).Value = previous.HasValue ? previous.Value.B : 255;
                        }
                        else if (colorType == ColorFormat.HSL)
                        {
                            ColorRGB.RGB2HSL(new ColorRGB((System.Drawing.Color)previous), out double h, out double s, out double l);
                            if (item.Key.EndsWith("hue")) (value as FloatElement).Value = (float)h;
                            else if (item.Key.EndsWith("saturation")) (value as FloatElement).Value = (float)s;
                            else if (item.Key.EndsWith("lightness")) (value as FloatElement).Value = (float)l;
                        }
                        else if (colorType == ColorFormat.HEX)
                        {
                            string hex = previous?.ToHEX();
                            if (item.Key.EndsWith("HexString")) (value as StringElement).Value = hex;
                        }
                    }
                    convert();

                    elements.Add(key, value);
                }
            }
            return elements;
        }

        internal float GetIncrementDecrement(ColorFormat colorType)
        {
            if (!incrementDecrementValues.ContainsKey(colorType) || incrementDecrementIndex < 0) return -1;
            var value = incrementDecrementValues[colorType];
            if (value == null) return -1;
            if (incrementDecrementIndex > value.Count - 1)
            {
                incrementDecrementIndex = value.Count - 1;
                return value[incrementDecrementIndex];
            }
            else
            {
                return value[incrementDecrementIndex];
            }
        }

        /// <summary>
        /// Was the page already set-up
        /// </summary>
        public bool WasSetup { get; private set; }

        internal void SetupPage()
        {
            ArgumentNullException.ThrowIfNull(Page, nameof(Page));

            Page.RemoveAll();
            incrementDecrementIndex = 0;
            Internal_UpdateGradient = null;

            FunctionElement current = Page.CreateFunction(Preview, Color.white, () => Applied?.Invoke(_selectedColorType, _selectedColor));

            Dictionary<ColorType, Dictionary<string, Element>> typeElements = [];

            Dictionary<string, Element> typeElementsTemporary = [];

            Dictionary<ColorType, (System.Drawing.Color? start, System.Drawing.Color? middle, System.Drawing.Color? end)> previousColors = [];

            System.Drawing.Color previousSolidColor;

            System.Drawing.Color getRGB(string name)
            {
                if (!typeElementsTemporary.ContainsKey($"{name}Preview") || !typeElementsTemporary.ContainsKey($"{name}Color_red") || !typeElementsTemporary.ContainsKey($"{name}Color_green") || !typeElementsTemporary.ContainsKey($"{name}Color_blue")) return System.Drawing.Color.White;
                var preview = typeElementsTemporary[$"{name}Preview"];
                var r = typeElementsTemporary[$"{name}Color_red"] as IntElement;
                var g = typeElementsTemporary[$"{name}Color_green"] as IntElement;
                var b = typeElementsTemporary[$"{name}Color_blue"] as IntElement;
                var color = System.Drawing.Color.FromArgb(0, r.Value, g.Value, b.Value);
                preview.ElementName = previewColorString.CreateUnityColor(color);
                return color;
            }

            System.Drawing.Color getHSL(string name)
            {
                if (!typeElementsTemporary.ContainsKey($"{name}Preview") || !typeElementsTemporary.ContainsKey($"{name}Color_hue") || !typeElementsTemporary.ContainsKey($"{name}Color_saturation") || !typeElementsTemporary.ContainsKey($"{name}Color_lightness")) return System.Drawing.Color.White;
                var preview = typeElementsTemporary[$"{name}Preview"];
                var h = typeElementsTemporary[$"{name}Color_hue"] as FloatElement;
                var s = typeElementsTemporary[$"{name}Color_saturation"] as FloatElement;
                var l = typeElementsTemporary[$"{name}Color_lightness"] as FloatElement;
                System.Drawing.Color color = ColorRGB.HSL2RGB(h.Value, s.Value, l.Value);
                preview.ElementName = previewColorString.CreateUnityColor(color);
                return color;
            }

            System.Drawing.Color getHEX(string name)
            {
                if (!typeElementsTemporary.ContainsKey($"{name}Preview") || !typeElementsTemporary.ContainsKey($"{name}HexString")) return System.Drawing.Color.White;
                var preview = typeElementsTemporary[$"{name}Preview"];
                var _hex = typeElementsTemporary[$"{name}HexString"] as StringElement;
                var color = Gradient.FromHex(_hex.Value);
                preview.ElementName = previewColorString.CreateUnityColor(color);
                return color;
            }

            void updateGradient()
            {
                List<string> names = _selectedColorType == ColorType.StandardGradient ? ["start", "end"] : _selectedColorType == ColorType.MiddleGradient ? ["corners", "middle"] : _selectedColorType == ColorType.ThreeColoredGradient ? ["start", "middle", "end"] : null;
                if (names != null)
                {
                    foreach (string name in names)
                    {
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        System.Drawing.Color color = _selectedColorFormat == ColorFormat.RGB ? getRGB(name) : _selectedColorFormat == ColorFormat.HSL ? getHSL(name) : getHEX(name);

                        if (!previousColors.ContainsKey(_selectedColorType)) previousColors.Add(_selectedColorType, (null, null, null));

                        var newValue = previousColors[_selectedColorType];
                        if (name == "start" || name == "corners") newValue.start = color;
                        else if (name == "middle") newValue.middle = color;
                        else if (name == "end") newValue.end = color;
                        previousColors[_selectedColorType] = newValue;
                    }
                    var start = previousColors[_selectedColorType].start;
                    var middle = previousColors[_selectedColorType].middle;
                    var end = previousColors[_selectedColorType].end;
                    _selectedColor = new GradientObject(ColorTypeToGradientType(_selectedColorType), start, middle, end);
                    if (current == null) return;
                    if (_selectedColorType == ColorType.StandardGradient && start != null && end != null) current.ElementName = Preview.RemoveUnityRichText().CreateStandardGradient((System.Drawing.Color)start, (System.Drawing.Color)end, GradientReturnType.UnityRichText);
                    if (_selectedColorType == ColorType.MiddleGradient && start != null && end != null) current.ElementName = Preview.RemoveUnityRichText().CreateMiddleGradient((System.Drawing.Color)start, (System.Drawing.Color)end, GradientReturnType.UnityRichText);
                    if (_selectedColorType == ColorType.ThreeColoredGradient && start != null && end != null && middle != null) current.ElementName = Preview.RemoveUnityRichText().CreateThreeColoredGradient((System.Drawing.Color)start, (System.Drawing.Color)middle, (System.Drawing.Color)end, GradientReturnType.UnityRichText);
                }
                else
                {
                    if (_selectedColorType == ColorType.RainbowGradient && current != null)
                    {
                        _selectedColor = new GradientObject(ColorTypeToGradientType(_selectedColorType));
                        current.ElementName = current.ElementName.RemoveUnityRichText().CreateRainbowGradient(Gradient.GradientReturnType.UnityRichText);
                    }
                    else if (_selectedColorType == ColorType.SolidColor && current != null)
                    {
                        System.Drawing.Color color = _selectedColorFormat == ColorFormat.RGB ? getRGB("color") : _selectedColorFormat == ColorFormat.HSL ? getHSL("color") : getHEX("color");
                        previousSolidColor = color;
                        _selectedColor = color;
                        current.ElementName = current.ElementName.RemoveUnityRichText().CreateUnityColor(color);
                    }
                }
            }
            EnumElement typeSelect = null;
            EnumElement colorSelect = null;
            FunctionElement incrementDecrement = null;

            bool removedid = false;
            bool removedColorSelect = false;

            void update()
            {
                Internal_UpdateGradient = null;
                typeElementsTemporary.ForEach(x => Page.Remove(x.Value));
                typeElementsTemporary.Clear();
                var elements = CreateColorElements(
                    _selectedColorType,
                    _selectedColorFormat,
                    previousColors.ContainsKey(_selectedColorType) ? previousColors[_selectedColorType].start : null,
                    previousColors.ContainsKey(_selectedColorType) ? previousColors[_selectedColorType].middle : null,
                    previousColors.ContainsKey(_selectedColorType) ? previousColors[_selectedColorType].end : null);
                if (elements != null)
                {
                    foreach (var element in elements)
                    {
                        if (string.IsNullOrWhiteSpace(element.Key) || element.Value == null) continue;
                        typeElementsTemporary.Add(element.Key, element.Value);
                        var _element = typeElementsTemporary[element.Key];
                        Page.Add(_element);
                    }
                }
                else
                {
                    if (_selectedColorType == ColorType.RainbowGradient && current != null)
                    {
                        current.ElementName = current.ElementName.RemoveUnityRichText().CreateRainbowGradient(Gradient.GradientReturnType.UnityRichText);
                    }
                }

                if (_selectedColorFormat != ColorFormat.HEX && _selectedColorType != ColorType.RainbowGradient && removedid)
                {
                    if (Page.Elements.Contains(incrementDecrement)) return;
                    removedid = false;
                    typeElementsTemporary?.ForEach(x => Page.Remove(x.Value));
                    typeElementsTemporary?.Clear();
                    Page.Add(incrementDecrement);
                    update();
                }

                if (_selectedColorType != ColorType.RainbowGradient && removedColorSelect)
                {
                    if (Page.Elements.Contains(colorSelect)) return;
                    removedColorSelect = false;
                    bool contains = Page.Elements.Contains(incrementDecrement);
                    if (contains) Page.Remove(incrementDecrement);

                    typeElementsTemporary?.ForEach(x => Page.Remove(x.Value));
                    typeElementsTemporary?.Clear();
                    Page.Add(colorSelect);
                    if (contains && incrementDecrement != null) Page.Add(incrementDecrement);
                    update();
                }

                if (_selectedColorType == ColorType.RainbowGradient || _selectedColorFormat == ColorFormat.HEX)
                {
                    if (!removedid && Page.Elements.Contains(incrementDecrement)) { Core.Logger.Msg("Removing increment/decrement"); Page.Remove(incrementDecrement); removedid = true; }
                    if (!removedColorSelect && _selectedColorType == ColorType.RainbowGradient && Page.Elements.Contains(colorSelect)) { Core.Logger.Msg("Removing color select"); Page.Remove(colorSelect); removedColorSelect = true; }
                }

                incrementDecrement.ElementName = $"Increment/Decrement by: {GetIncrementDecrement(_selectedColorFormat)}";
                Internal_UpdateGradient = updateGradient;
                updateGradient();
            }

            Internal_UpdatePage = update;

            typeSelect = Page.CreateEnum("Gradient Type", Color.cyan, _selectedColorType, (value) =>
            {
                _selectedColorType = (ColorType)value;
                update();
            });

            colorSelect = Page.CreateEnum("Color Type", Color.yellow, _selectedColorFormat, (value) =>
            {
                _selectedColorFormat = (ColorFormat)value;
                update();
            });

            incrementDecrement = Page.CreateFunction($"Increment/Decrement by: {GetIncrementDecrement(_selectedColorFormat)}", Color.magenta, () =>
            {
                var value = incrementDecrementValues[_selectedColorFormat];
                if (incrementDecrementIndex + 1 > value.Count - 1) incrementDecrementIndex = 0;
                else incrementDecrementIndex++;
                update();
            });

            update();
            WasSetup = true;
        }

        private static GradientType ColorTypeToGradientType(ColorType type)
        {
            return (GradientType)((int)type);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Page.RemoveAll();
            incrementDecrementIndex = -1;
            incrementDecrementValues.Clear();
            _selectedColor = null;
            Applied = null;
            Internal_UpdateGradient = null;
            _preview = null;
            WasSetup = false;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The format used to input the color
        /// </summary>
        public enum ColorFormat
        {
            /// <summary>
            /// User will need to provide Red, Green and Blue values
            /// </summary>
            RGB,

            /// <summary>
            /// User will need to provide Hue, Saturation and Lightness values
            /// </summary>
            HSL,

            /// <summary>
            /// User will need to provide a HEX of RGB
            /// </summary>
            HEX
        }

        /// <summary>
        /// Type of color
        /// </summary>
        public enum ColorType
        {
            /// <summary>
            /// A standard gradient
            /// </summary>
            StandardGradient,

            /// <summary>
            /// A middle gradient
            /// </summary>
            MiddleGradient,

            /// <summary>
            /// A three colored gradient
            /// </summary>
            ThreeColoredGradient,

            /// <summary>
            /// A rainbow gradient
            /// </summary>
            RainbowGradient,

            /// <summary>
            /// A solid color
            /// </summary>
            SolidColor
        }
    }
}