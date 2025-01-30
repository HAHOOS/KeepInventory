using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib.BoneMenu;

using UnityEngine;

namespace KeepInventory.Menu
{
    /// <summary>
    /// Function element with <see cref="ElementProperties.NoBorder"/> property by default
    /// </summary>
    public class LabelElement
    {
        /// <summary>
        /// The actual element of the label, which is literally a <see cref="FunctionElement"/> with the <see cref="ElementProperties.NoBorder"/> property
        /// </summary>
        public FunctionElement Element { get; set; }

        /// <summary>
        /// Create new instance of <see cref="LabelElement"/>
        /// </summary>
        /// <param name="text">Text that will be displayed</param>
        /// <param name="color">Color of the text</param>
        public LabelElement(string text, Color color)
        {
            Element = new FunctionElement(text, color, null);
            Element.SetProperty(ElementProperties.NoBorder);
        }

        /// <summary>
        /// Create new instance of <see cref="LabelElement"/>
        /// </summary>
        /// <param name="text">Text that will be displayed</param>
        /// <param name="color">Color of the text</param>
        /// <param name="callback">Action that will be ran on pressed</param>
        public LabelElement(string text, Color color, Action callback)
        {
            Element = new FunctionElement(text, color, callback);
            Element.SetProperty(ElementProperties.NoBorder);
        }
    }
}