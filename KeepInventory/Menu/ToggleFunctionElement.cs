using System;
using System.Threading;

using BoneLib.BoneMenu;

using UnityEngine;

namespace KeepInventory.Menu
{
    public class ToggleFunctionElement
    {
        public readonly FunctionElement Element;
        public Action<ToggleFunctionElement> Callback;
        public Color OffColor;
        public Color OnColor;
        public bool IsRunning { get; private set; }
        public event Action Started;
        public event Action Cancelled;
        public void Start()
        {
            Core.Logger.Msg("Start");
            if (IsRunning) return;
            Core.Logger.Msg("Start pass");
            IsRunning = true;
            Element.ElementColor = OnColor;
            Started?.Invoke();
            Callback?.Invoke(this);
        }
        public void Cancel()
        {
            Core.Logger.Msg("Cancel");
            if (!IsRunning) return;
            Core.Logger.Msg("Cancel pass");
            IsRunning = false;
            Element.ElementColor = OffColor;
            Cancelled?.Invoke();
        }
        public ToggleFunctionElement(string name, Color offColor, Color onColor, Action<ToggleFunctionElement> callback)
        {
            Callback = callback;
            OffColor = offColor;
            OnColor = onColor;
            Element = new FunctionElement(name, offColor, () =>
            {
                if (this.IsRunning) this.Cancel();
                else this.Start();
            });
        }
        public ToggleFunctionElement(string name, Color offColor, Action<ToggleFunctionElement> callback)
        {
            Callback = callback;
            OffColor = offColor;
            OnColor = Color.red;
            Element = new FunctionElement(name, offColor, () =>
            {
                if (this.IsRunning) this.Cancel();
                else this.Start();
            });
        }
    }
}