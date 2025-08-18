using System;
using System.Collections.Generic;
using System.Linq;

using BoneLib.BoneMenu;

using Il2CppSLZ.Marrow.Warehouse;

using KeepInventory.Helper;
using KeepInventory.Managers;

using UnityEngine;

namespace KeepInventory.Menu
{
    public static class BoneMenu
    {
        public static Page AuthorPage { get; private set; }
        public static Page ModPage { get; private set; }
        public static Page SavesPage { get; private set; }
        public static Page PresetsPage { get; private set; }
        public static Page SavingConfigPage { get; private set; }
        public static Page EventsPage { get; private set; }
        public static Page SharingPage { get; private set; }
        public static Page SharingBlacklistPage { get; private set; }
        public static Page BlacklistPage { get; private set; }
        public static Page BlacklistViewPage { get; private set; }

        public static Page PredefinedBlacklistPage { get; private set; }

        public static Page OtherPage { get; private set; }
        public static bool IsSetup { get; private set; }
        public static bool RemoveSavesOnPress { get; private set; }

        internal static void Setup()
        {
            AuthorPage = Page.Root.CreatePage("HAHOOS", Color.white);
            ModPage = AuthorPage.CreatePage("KeepInventory", Color.yellow);

            SetupSaves();
            Core.DefaultSaveChanged += UpdatePresetsPage;

            EventsPage = ModPage.CreatePage("Events", Color.yellow);
            EventsPage.CreateBoolPref("Save on Level Unload", Color.red, ref PreferencesManager.SaveOnLevelUnload, prefDefaultValue: true);
            EventsPage.CreateBoolPref("Load on Level Load", Color.green, ref PreferencesManager.LoadOnLevelLoad, prefDefaultValue: true);

            if (Core.HasFusion && Core.IsFusionLibraryInitialized)
                SetupSharing();

            BlacklistPage = ModPage.CreatePage("Blacklist", Color.red);

            PredefinedBlacklistPage = BlacklistPage.CreatePage("Predefined Blacklists", Color.cyan);
            SetupPredefinedBlacklists();

            BlacklistViewPage = BlacklistPage.CreatePage("View All", Color.magenta);
            SetupBlacklistView();

            Core.statusElement = BlacklistPage.CreateFunction("Blacklist Level from Saving/Loading", Color.red, () =>
            {
                if (BlacklistManager.IsCurrentLevelInBlacklist())
                {
                    BLHelper.SendNotification("Failure", "The current level is already in a predefined blacklist. Use the predefined blacklist to blacklist this level instead", true, 4f, BoneLib.Notifications.NotificationType.Error);
                    return;
                }

                List<string> blacklistList = PreferencesManager.BlacklistedLevels.Value;
                if (blacklistList.Contains(Core.levelInfo.barcode))
                {
                    try
                    {
                        int item = blacklistList.IndexOf(Core.levelInfo.barcode);
                        if (item != -1)
                        {
                            blacklistList.RemoveAt(item);
                            Core.statusElement.ElementName = "Current Level is not blacklisted";
                            Core.statusElement.ElementColor = Color.green;
                            BLHelper.SendNotification("Success", $"Successfully unblacklisted current level ({Core.levelInfo.title}) from having the inventory saved and/or loaded!", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Logger.Error($"An unexpected error has occurred while unblacklisting the current level\n{ex}");
                        BLHelper.SendNotification("Failure", "An unexpected error has occurred while unblacklisting the current level, check the console or logs for more details", true, 3f, BoneLib.Notifications.NotificationType.Error);
                    }
                }
                else
                {
                    try
                    {
                        blacklistList.Add(Core.levelInfo.barcode);
                        Core.statusElement.ElementName = "Current Level is blacklisted";
                        Core.statusElement.ElementColor = Color.red;
                        BLHelper.SendNotification("Success", $"Successfully blacklisted current level ({Core.levelInfo.title}) from having the inventory saved and/or loaded!", true, 2.5f, BoneLib.Notifications.NotificationType.Success);
                    }
                    catch (Exception ex)
                    {
                        Core.Logger.Error($"An unexpected error has occurred while removing the current level from blacklist\n{ex}");
                        BLHelper.SendNotification("Failure", "An unexpected error has occurred while blacklisting the current level, check the console or logs for more details", true, 3f, BoneLib.Notifications.NotificationType.Error);
                    }
                }
            });

            OtherPage = ModPage.CreatePage("Other", Color.white);
            OtherPage.CreateBoolPref("Show Notifications", Color.green, ref PreferencesManager.ShowNotifications, prefDefaultValue: true);
            OtherPage.CreateBoolPref("Holster Held Weapons on Death", Color.magenta, ref PreferencesManager.HolsterHeldWeaponsOnDeath);
            OtherPage.CreateFunction("Clear Inventory", Color.yellow, InventoryManager.ClearInventory);

            ModPage.CreateBlank();
            ModPage.CreateFunction("Load inventory from default save", Color.yellow, () =>
            {
                if (Core.CurrentSave != null)
                {
                    InventoryManager.LoadSavedInventory(Core.CurrentSave);
                }
                else
                {
                    BLHelper.SendNotification("Failure", "There is no default save!", true, 2, BoneLib.Notifications.NotificationType.Error);
                }
            });

            var modVersion = ModPage.CreateFunction(Core.IsLatestVersion || Core.ThunderstorePackage == null ? $"Current Version: v{Core.Version}" : $"Current Version: v{Core.Version}<br><color=#00FF00>(Update available!)</color>", Color.white, () => Core.Logger.Msg($"The current version is v{Core.Version}!!!!"));
            modVersion.SetProperty(ElementProperties.NoBorder);
            IsSetup = true;
        }

        private static readonly Dictionary<string, Page> PredefinedBlacklistPages = [];

        internal static void SetupPredefinedBlacklists()
        {
            if (PredefinedBlacklistPage == null)
                return;

            PredefinedBlacklistPage.RemoveAll();
            foreach (var blacklist in BlacklistManager.Blacklist)
            {
                if (!PredefinedBlacklistPages.ContainsKey(blacklist.ID))
                    PredefinedBlacklistPages.Add(blacklist.ID, PredefinedBlacklistPage.CreatePage(blacklist.DisplayName, blacklist.Enabled ? Color.red : new Color(0, 1, 0), createLink: false));

                PredefinedBlacklistPage.CreateFunction($"{blacklist.DisplayName} ({blacklist.Levels.Length})", blacklist.Enabled ? Color.red : new Color(0, 1, 0), () => BoneLib.BoneMenu.Menu.OpenPage(PredefinedBlacklistPages[blacklist.ID]));
                SetupPredefinedBlacklistPage(blacklist.ID);
            }
        }

        internal static void SetupPredefinedBlacklistPage(string id)
        {
            var page = PredefinedBlacklistPages[id];
            if (page == null)
                return;

            var blacklist = BlacklistManager.Blacklist.FirstOrDefault(x => x.ID == id);
            if (blacklist == null)
                return;

            page.Name = blacklist.DisplayName;
            page.Color = blacklist.Enabled ? Color.red : new Color(0, 1, 0);

            page.RemoveAll();
            page.CreateLabel($"ID: {blacklist.ID}", Color.white);
            page.CreateLabel($"DisplayName: {blacklist.DisplayName}", Color.white);
            page.CreateLabel($"Level Count: {blacklist.Levels.Length}", Color.white);
            page.CreateBool("Enabled", Color.green, blacklist.Enabled, (val) => BlacklistManager.SetEnabled(id, val));
            page.CreateBlank();
            foreach (var level in blacklist.Levels)
            {
                var _level = new LevelCrateReference(level.Barcode);
                var title = _level?.Crate?.Title;
                var elem = page.CreateToggleFunction(string.IsNullOrWhiteSpace(title) ? level.Barcode : $"{title} ({level.Barcode})", Color.red, new Color(0, 1, 0), null, !level.IsBlacklisted);
                elem.OnStart += () => BlacklistManager.SetLevelBlacklisted(blacklist.ID, level.Barcode, false);
                elem.OnCancel += () => BlacklistManager.SetLevelBlacklisted(blacklist.ID, level.Barcode, true);
            }
        }

        private static void SetupBlacklistView()
        {
            if (BlacklistViewPage == null)
                return;

            BlacklistViewPage.RemoveAll();
            BlacklistViewPage.CreateFunction("Refresh", Color.yellow, SetupBlacklistView);
            BlacklistViewPage.CreateBlank();
            foreach (var level in PreferencesManager.BlacklistedLevels.Value)
            {
                var reference = new LevelCrateReference(level);
                if (!reference.TryGetCrate(out LevelCrate crate))
                    continue;

                FunctionElement element = null;
                element = BlacklistViewPage.CreateFunction(crate.Title, Color.white, () =>
                {
                    PreferencesManager.BlacklistedLevels.Value.Remove(level);
                    BlacklistViewPage.Remove(element);
                    if (Core.levelInfo.barcode == level)
                    {
                        Core.statusElement.ElementName = "Current level is not blacklisted";
                        Core.statusElement.ElementColor = Color.green;
                    }
                });
            }
        }

        private static readonly List<Element> defaultSaveElements = [];

        internal static void SetupSharing()
        {
            SharingPage ??= ModPage.CreatePage("Sharing", Color.cyan);
            SharingPage.CreateBool("Enabled", Color.red, Fusion.Managers.ShareManager.Entry_SharingEnabled.Value, (val) =>
            {
                Fusion.Managers.ShareManager.Entry_SharingEnabled.Value = val;
                Fusion.Managers.ShareManager.Category.SaveToFile(false);
            });
            SharingBlacklistPage ??= SharingPage.CreatePage("Blacklist", Color.red);
            SetupSharingBlacklist();
            LabFusion.Utilities.MultiplayerHooking.OnDisconnected += SetupSharingBlacklist;
            LabFusion.Utilities.MultiplayerHooking.OnJoinedServer += SetupSharingBlacklist;
            LabFusion.Utilities.MultiplayerHooking.OnStartedServer += SetupSharingBlacklist;
            LabFusion.Utilities.MultiplayerHooking.OnPlayerJoined += (_) => SetupSharingBlacklist();
            LabFusion.Utilities.MultiplayerHooking.OnPlayerLeft += (_) => SetupSharingBlacklist();
        }

        internal static void SetupSharingBlacklist()
        {
            SharingBlacklistPage.RemoveAll();
            SharingBlacklistPage.CreateFunction("Refresh", Color.yellow, SetupSharingBlacklist);
            SharingBlacklistPage.CreateBlank();
            var players = Utilities.Fusion.GetPlayers();
            players.RemoveAll(x => x.SmallID == Utilities.Fusion.GetLocalPlayerSmallID());
            if (players.Count == 0)
            {
                SharingBlacklistPage.CreateLabel("Nothing to show here :(", Color.white);
            }
            else
            {
                foreach (var player in players)
                {
                    var element = SharingBlacklistPage.CreateToggleFunction(player.DisplayName, Color.white, null);
                    element.OnStart += () =>
                    {
                        Fusion.Managers.ShareManager.Entry_SharingBlacklist.Value.Add(player.PlatformID);
                        Fusion.Managers.ShareManager.Category.SaveToFile(false);
                    };
                    element.OnCancel += () =>
                    {
                        Fusion.Managers.ShareManager.Entry_SharingBlacklist.Value.Remove(player.PlatformID);
                        Fusion.Managers.ShareManager.Category.SaveToFile(false);
                    };
                }
            }
        }

        internal static void SetupSaves()
        {
            SavesPage ??= ModPage.CreatePage("Saves", Color.cyan);

            SavingConfigPage ??= SavesPage.CreatePage("Config", Color.yellow, 4);
            SavingConfigPage.RemoveAll();
            SavingConfigPage.CreateBoolPref("Save Items", Color.white, ref PreferencesManager.ItemSaving, prefDefaultValue: true);
            SavingConfigPage.CreateBoolPref("Save Ammo", Color.white, ref PreferencesManager.AmmoSaving, prefDefaultValue: true);
            SavingConfigPage.CreateBoolPref("Save Gun Data", Color.white, ref PreferencesManager.SaveGunData, prefDefaultValue: true);

            PresetsPage ??= SavesPage.CreatePage("<color=#00FF00>Presets</color>", Color.white);
            PresetsPage.RemoveAll();

            var nameElement = PresetsPage.CreateString("Name", Color.white, string.Empty, null);
            var createElement = PresetsPage.CreateFunction("<color=#00FF00>Create</color>", Color.white, () =>
            {
                SaveManager.RegisterSave(new Saves.V2.Save()
                {
                    Name = nameElement.Value,
                    InventorySlots = [],
                    HeavyAmmo = 0,
                    LightAmmo = 0,
                    MediumAmmo = 0,
                    CanBeOverwrittenByPlayer = true,
                    IsHidden = false,
                    ID = $"{nameElement.Value.ToLower()}-{SaveManager.GenerateRandomID(6)}"
                });
            });

            var func = PresetsPage.CreateToggleFunction("Remove [OFF]", Color.cyan, Color.red, null);
            func.OnStart += () =>
            {
                func.Element.ElementName = "Remove [ON]";
                RemoveSavesOnPress = true;
            };
            func.OnCancel += () =>
            {
                func.Element.ElementName = "Remove [OFF]";
                RemoveSavesOnPress = false;
            };

            var refresh = PresetsPage.CreateFunction("Refresh", Color.yellow, () => SetupSaves());

            var blank = PresetsPage.CreateBlank();
            if (defaultSaveElements.Count == 0)
            {
                defaultSaveElements.Add(nameElement);
                defaultSaveElements.Add(createElement);
                defaultSaveElements.Add(refresh);
                defaultSaveElements.Add(func.Element);
                defaultSaveElements.Add(blank.Element);
            }

            UpdatePresetsPage();
        }

        private static readonly List<SavePage> SavePages = [];

        internal static void UpdatePresetsPage()
        {
            if (PresetsPage == null)
                return;

            PresetsPage.RemoveAll();

            defaultSaveElements.ForEach(PresetsPage.Add);

            foreach (var save in SaveManager.Saves)
            {
                if (save == null)
                {
                    Core.Logger.Error("Save is null, cannot generate element");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(save.ID))
                {
                    Core.Logger.Error("ID is null or empty, cannot generate element");
                }
                if (save.IsHidden) continue;
                Page page = null;
                var savePage = SavePages.FirstOrDefault(x => x.CurrentSave == save && x.Page != null);
                if (savePage == null)
                {
                    page = PresetsPage.CreatePage($"<color=#{save.Color}>{save.Name}</color>", Color.white, 0, false);
                    savePage = new SavePage(page, save);
                    SavePages.Add(savePage);
                }
                else
                {
                    page = savePage.Page;
                    savePage.Setup();
                }

                FunctionElement link = null;

                link = PresetsPage.CreateFunction(Core.CurrentSave == save ? $"<color=#{save.DrawingColor.ToHEX() ?? "FFFFFF"}>+ {save.Name} +</color>" : $"<color=#{save.DrawingColor.ToHEX() ?? "FFFFFF"}>{save.Name}</color>", Color.white, () =>
                {
                    if (RemoveSavesOnPress)
                    {
                        BoneLib.BoneMenu.Menu.DisplayDialog("Destructive action", "You are about to delete a save file which after done, cannot be reversed. Are you sure?", Dialog.WarningIcon, () =>
                        {
                            try
                            {
                                SaveManager.UnregisterSave(save.ID, true);
                                PresetsPage.Remove(link);
                            }
                            catch (Exception ex)
                            {
                                BLHelper.SendNotification("Error", "Failed to remove save, check logs or console for more information", true, 2f, BoneLib.Notifications.NotificationType.Error);
                                Core.Logger.Error($"An unexpected error has occurred while attempting to remove save, exception:\n{ex}");
                            }
                        });
                    }
                    else
                    {
                        BoneLib.BoneMenu.Menu.OpenPage(page);
                    }
                });
            }
        }
    }
}