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

        public static IntElement CreateIntPref(this Page page, string name, Color color, ref MelonPreferences_Entry<int> value, int increment, int minValue, int maxValue, MelonPreferences_Category category = null, Action<int> callback = null, string prefName = null, int prefDefaultValue = default)
        {
            category ??= prefs;
            if (value == null || !category.HasEntry(value.Identifier))
            {
                prefName ??= name;

                if (!category.HasEntry(prefName))
                    value = category.CreateEntry(prefName, prefDefaultValue);
            }
            MelonPreferences_Entry<int> val = value;
            var element = page.CreateInt(name, color, val.Value, increment, minValue, maxValue, (x) =>
            {
                val.Value = x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            return element;
        }

        public static FloatElement CreateFloatPref(this Page page, string name, Color color, ref MelonPreferences_Entry<float> value, float increment, float minValue, float maxValue, MelonPreferences_Category category = null, Action<float> callback = null, string prefName = null, float prefDefaultValue = default)
        {
            category ??= prefs;
            if (value == null || !category.HasEntry(value.Identifier))
            {
                prefName ??= name;

                if (!category.HasEntry(prefName))
                    value = category.CreateEntry(prefName, prefDefaultValue);
            }
            MelonPreferences_Entry<float> val = value;
            var element = page.CreateFloat(name, color, val.Value, increment, minValue, maxValue, (x) =>
            {
                val.Value = x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            return element;
        }

        public static BoolElement CreateBoolPref(this Page page, string name, Color color, ref MelonPreferences_Entry<bool> value, MelonPreferences_Category category = null, Action<bool> callback = null, string prefName = null, bool prefDefaultValue = default)
        {
            category ??= prefs;
            if (value == null || !category.HasEntry(value.Identifier))
            {
                prefName ??= name;

                if (!category.HasEntry(prefName))
                    value = category.CreateEntry(prefName, prefDefaultValue);
            }
            MelonPreferences_Entry<bool> val = value;
            var element = page.CreateBool(name, color, val.Value, (x) =>
            {
                val.Value = x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            return element;
        }

        public static EnumElement CreateEnumPref<T>(this Page page, string name, Color color, ref MelonPreferences_Entry<T> value, MelonPreferences_Category category = null, Action<Enum> callback = null, string prefName = null, Enum prefDefaultValue = default) where T : Enum
        {
            category ??= prefs;
            if (value == null || !category.HasEntry(value.Identifier))
            {
                prefName ??= name;

                if (!category.HasEntry(prefName))
                    value = category.CreateEntry(prefName, (T)prefDefaultValue);
            }
            MelonPreferences_Entry<T> val = value;
            var element = page.CreateEnum(name, color, val.Value, (x) =>
            {
                val.Value = (T)x;
                category.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            return element;
        }

        public static StringElement CreateStringPref(this Page page, string name, Color color, ref MelonPreferences_Entry<string> value, MelonPreferences_Category category = null, Action<string> callback = null, string prefName = null, string prefDefaultValue = default)
        {
            category ??= prefs;
            if (value == null || !category.HasEntry(value.Identifier))
            {
                prefName ??= name;

                if (!category.HasEntry(prefName))
                    value = category.CreateEntry(prefName, prefDefaultValue);
            }
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
}