using UnityEngine;

using BoneLib.BoneMenu;

namespace KeepInventory.Menu
{
    public class BlankElement
    {
        public readonly FunctionElement Element;

        public BlankElement()
        {
            Element = new FunctionElement(string.Empty, Color.white, null);
            Element.SetProperty(ElementProperties.NoBorder);
        }
    }
}