using BoneLib;
using BoneLib.BoneMenu;
using MelonLoader;
using UnityEngine;
using System;
using BoneLib.Notifications;

namespace KeepInventory.Helper
{
    internal static class BLHelper
    {
        #region BoneMenu (Code shared by @camobiwon on Discord)

        //You will need a PrefsCategory someone in your main class to reference here
        //Replace instances of "Main" with your desired class
        //Additionally you would need some method to save your preferences (Main.SavePreferences here)

        private readonly static MelonPreferences_Category prefs = KeepInventory.PrefsCategory;

        internal static IntElement CreateIntPref(this Page page, string name, Color color, ref MelonPreferences_Entry<int> value, int increment, int minValue, int maxValue, Action<int> callback = null, string prefName = null, int prefDefaultValue = default)
        {
            prefName ??= name;

            if (!prefs.HasEntry(prefName))
                value = prefs.CreateEntry(prefName, prefDefaultValue);

            MelonPreferences_Entry<int> val = value;
            return page.CreateInt(name, color, val.Value, increment, minValue, maxValue, (x) =>
            {
                val.Value = x;
                prefs.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
        }

        internal static FloatElement CreateFloatPref(this Page page, string name, Color color, ref MelonPreferences_Entry<float> value, float increment, float minValue, float maxValue, Action<float> callback = null, string prefName = null, float prefDefaultValue = default)
        {
            prefName ??= name;

            if (!prefs.HasEntry(prefName))
                value = prefs.CreateEntry(prefName, prefDefaultValue);

            MelonPreferences_Entry<float> val = value;
            return page.CreateFloat(name, color, val.Value, increment, minValue, maxValue, (x) =>
            {
                val.Value = x;
                prefs.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
        }

        internal static BoolElement CreateBoolPref(this Page page, string name, Color color, ref MelonPreferences_Entry<bool> value, Action<bool> callback = null, string prefName = null, bool prefDefaultValue = default)
        {
            prefName ??= name;

            if (!prefs.HasEntry(prefName))
                value = prefs.CreateEntry(prefName, prefDefaultValue);

            MelonPreferences_Entry<bool> val = value;
            return page.CreateBool(name, color, val.Value, (x) =>
            {
                val.Value = x;
                prefs.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
        }

        internal static EnumElement CreateEnumPref<T>(this Page page, string name, Color color, ref MelonPreferences_Entry<T> value, Action<Enum> callback = null, string prefName = null, Enum prefDefaultValue = default) where T : Enum
        {
            prefName ??= name;

            if (!prefs.HasEntry(prefName))
                value = prefs.CreateEntry(prefName, (T)prefDefaultValue);

            MelonPreferences_Entry<T> val = value;
            return page.CreateEnum(name, color, val.Value, (x) =>
            {
                val.Value = (T)x;
                prefs.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
        }

        internal static StringElement CreateStringPref(this Page page, string name, Color color, ref MelonPreferences_Entry<string> value, Action<string> callback = null, string prefName = null, string prefDefaultValue = default)
        {
            prefName ??= name;

            if (!prefs.HasEntry(prefName))
                value = prefs.CreateEntry(prefName, prefDefaultValue);

            MelonPreferences_Entry<string> val = value;
            StringElement element = page.CreateString(name, color, val.Value, (x) =>
            {
                val.Value = x;
                prefs.SaveToFile(false);
                callback?.InvokeActionSafe(x);
            });
            element.Value = value.Value; //BoneMenu temp hack fix
            return element;
        }

        #endregion BoneMenu (Code shared by @camobiwon on Discord)

        #region Notifications

        public static void SendNotification(string message, string title = null, bool showTitleOnPopup = false, float popupLength = 2f, NotificationType type = NotificationType.Information, Texture2D customIcon = null)
        {
            if ((bool?)KeepInventory.mp_showNotifications?.BoxedValue != true) return;
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

        public static void SendNotification(NotificationText message, NotificationText title, bool showTitleOnPopup = false, float popupLength = 2f, NotificationType type = NotificationType.Information, Texture2D customIcon = null)
        {
            if ((bool?)KeepInventory.mp_showNotifications?.BoxedValue != true) return;
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

        #endregion Notifications
    }
}