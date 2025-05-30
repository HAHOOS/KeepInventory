using System;

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

        public event Action OnStart;

        public event Action OnCancel;

        public void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            Element.ElementColor = OnColor;
            OnStart?.Invoke();
            Callback?.Invoke(this);
        }

        public void Cancel()
        {
            if (!IsRunning)
                return;
            IsRunning = false;
            Element.ElementColor = OffColor;
            OnCancel?.Invoke();
        }

        public ToggleFunctionElement(string name, Color offColor, Color onColor, Action<ToggleFunctionElement> callback, bool on = false)
        {
            Callback = callback;
            OffColor = offColor;
            OnColor = onColor;
            IsRunning = on;
            Element = new FunctionElement(name, on ? OnColor : OffColor, () =>
            {
                if (this.IsRunning) this.Cancel();
                else this.Start();
            });
        }

        public ToggleFunctionElement(string name, Color offColor, Action<ToggleFunctionElement> callback, bool on = false)
        {
            Callback = callback;
            OffColor = offColor;
            OnColor = Color.red;
            IsRunning = on;
            Element = new FunctionElement(name, on ? OnColor : OffColor, () =>
            {
                if (this.IsRunning) this.Cancel();
                else this.Start();
            });
        }

        public ToggleFunctionElement(string name, Action<ToggleFunctionElement> callback, bool on = false)
        {
            Callback = callback;
            OffColor = Color.white;
            OnColor = Color.red;
            IsRunning = on;
            Element = new FunctionElement(name, on ? OnColor : OffColor, () =>
            {
                if (this.IsRunning) this.Cancel();
                else this.Start();
            });
        }
    }
}