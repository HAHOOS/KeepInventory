using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.Notifications;

using KeepInventory.Managers;
using KeepInventory.Menu;

using MelonLoader;

using System;

using UnityEngine;

namespace KeepInventory.Helper
{
    internal static class BLHelper
    {
        #region BoneMenu (Code shared by @camobiwon on Discord)

        private readonly static MelonPreferences_Category prefs = PreferencesManager.PrefsCategory;

        public static IntElement CreateIntPref(this Page page, string name, Color color, ref MelonPreferences_Entry<int> value, NumberProperties<int> properties, MelonPreferences_Category category = null, Action<int> callback = null)
        {
            category ??= prefs;
            MelonPreferences_Entry<int> val = value;
            var element = page.CreateInt(name, color, val.Value, properties.Increment, properties.Min, properties.Max, (x) =>
            {
                val.Value = x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            return element;
        }

        public static FloatElement CreateFloatPref(this Page page, string name, Color color, ref MelonPreferences_Entry<float> value, NumberProperties<float> properties, MelonPreferences_Category category = null, Action<float> callback = null)
        {
            category ??= prefs;
            MelonPreferences_Entry<float> val = value;
            var element = page.CreateFloat(name, color, val.Value, properties.Increment, properties.Min, properties.Max, (x) =>
            {
                val.Value = x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            return element;
        }

        public static BoolElement CreateBoolPref(this Page page, string name, Color color, ref MelonPreferences_Entry<bool> value, MelonPreferences_Category category = null, Action<bool> callback = null)
        {
            category ??= prefs;
            MelonPreferences_Entry<bool> val = value;
            var element = page.CreateBool(name, color, val.Value, (x) =>
            {
                val.Value = x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            return element;
        }

        public static EnumElement CreateEnumPref<T>(this Page page, string name, Color color, ref MelonPreferences_Entry<T> value, MelonPreferences_Category category = null, Action<Enum> callback = null) where T : Enum
        {
            category ??= prefs;
            MelonPreferences_Entry<T> val = value;
            var element = page.CreateEnum(name, color, val.Value, (x) =>
            {
                val.Value = (T)x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            return element;
        }

        public static StringElement CreateStringPref(this Page page, string name, Color color, ref MelonPreferences_Entry<string> value, MelonPreferences_Category category = null, Action<string> callback = null)
        {
            category ??= prefs;
            MelonPreferences_Entry<string> val = value;
            StringElement element = page.CreateString(name, color, val.Value, (x) =>
            {
                val.Value = x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            element.Value = value.Value;
            return element;
        }

        public static ToggleFunctionElement CreateToggleFunction(this Page page, string name, Color offColor, Color onColor, Action<ToggleFunctionElement> callback, bool on = false)
        {
            var element = new ToggleFunctionElement(name, offColor, onColor, callback, on);
            page.Add(element.Element);
            return element;
        }

        public static ToggleFunctionElement CreateToggleFunction(this Page page, string name, Color offColor, Action<ToggleFunctionElement> callback, bool on = false)
        {
            var element = new ToggleFunctionElement(name, offColor, callback, on);
            page.Add(element.Element);
            return element;
        }

        public static BlankElement CreateBlank(this Page page)
        {
            var element = new BlankElement();
            page.Add(element.Element);
            return element;
        }

        public static FunctionElement CreateLabel(this Page page, string text, Color color)
        {
            var element = new FunctionElement(text, color, null);
            element.SetProperty(ElementProperties.NoBorder);
            page.Add(element);
            return element;
        }

        #endregion BoneMenu (Code shared by @camobiwon on Discord)

        #region Notifications

        internal static void SendNotification(string title, string message, bool showTitleOnPopup = false, float popupLength = 2f, NotificationType type = NotificationType.Information, Texture2D customIcon = null)
        {
            if (PreferencesManager.ShowNotifications?.Value != true) return;
            Notifier.Send(new Notification()
            {
                Title = new NotificationText($"KeepInventory | {title}", Color.white, true),
                Message = new NotificationText(message, Color.white, true),
                ShowTitleOnPopup = showTitleOnPopup,
                CustomIcon = customIcon,
                PopupLength = popupLength,
                Type = type
            });
        }

        internal static void SendNotification(NotificationText title, NotificationText message, bool showTitleOnPopup = false, float popupLength = 2f, NotificationType type = NotificationType.Information, Texture2D customIcon = null)
        {
            title.Text = $"KeepInventory | {title.Text}";
            if (PreferencesManager.ShowNotifications?.Value != true) return;
            Notifier.Send(new Notification()
            {
                Title = title,
                Message = message,
                ShowTitleOnPopup = showTitleOnPopup,
                CustomIcon = customIcon,
                PopupLength = popupLength,
                Type = type
            });
        }

        internal static void SendNotification(Notification notification)
        {
            if (PreferencesManager.ShowNotifications?.Value != true) return;
            Notifier.Send(notification);
        }

        #endregion Notifications
    }

    internal struct NumberProperties<T>(T min, T max, T increment)
    {
        public T Min = min;
        public T Max = max;
        public T Increment = increment;
    }
}