using System;

using BoneLib.BoneMenu;

using UnityEngine;

namespace KeepInventory.Menu
{
    public class LabelElement
    {
        public FunctionElement Element { get; set; }
        public LabelElement(string text, Color color)
        {
            Element = new FunctionElement(text, color, null);
            Element.SetProperty(ElementProperties.NoBorder);
        }
        public LabelElement(string text, Color color, Action callback)
        {
            Element = new FunctionElement(text, color, callback);
            Element.SetProperty(ElementProperties.NoBorder);
        }
    }
}