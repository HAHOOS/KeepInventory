using System;
using System.Collections.Generic;
using System.Linq;

using BoneLib.BoneMenu;

using Il2CppSLZ.Marrow.Warehouse;

using KeepInventory.Helper;
using KeepInventory.Saves.V2;
using KeepInventory.Utilities;

using UnityEngine;

namespace KeepInventory.Menu
{
    /// <summary>
    /// Class that holds most of the functionality with <see cref="BoneLib.BoneMenu"/>
    /// </summary>
    public static class BoneMenu
    {
        /// <summary>
        /// Page appearing first, which is the author: "HAHOOS"
        /// </summary>
        public static Page AuthorPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="AuthorPage"/>, which contains all of the settings for this mod
        /// </summary>
        public static Page ModPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="ModPage"/>, which enables the user to manage everything related to saves
        /// </summary>
        public static Page SavesPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="SavesPage"/>, which enables the user to manage saves
        /// </summary>
        public static Page PresetsPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="SavesPage"/>, which enables the player to change how saving and loading works
        /// </summary>
        public static Page SavingConfigPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="ModPage"/>, which enables the player to disable certain events such as load inventory on level load
        /// </summary>
        public static Page EventsPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="ModPage"/>, which enables you to manage the "Sharing" feature for saves
        /// </summary>
        public static Page SharingPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="SharingPage"/>, which enables you to blacklist players in the lobby you are currently in from sharing saves with you
        /// </summary>
        public static Page SharingBlacklistPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="ModPage"/>, which enables the player to disable certain events such as load inventory on level load
        /// </summary>
        public static Page BlacklistPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="BlacklistPage"/>, which is just a list of all blacklisted levels
        /// </summary>
        public static Page BlacklistViewPage { get; private set; }

        /// <summary>
        /// SubPage of <see cref="ModPage"/>, which has more general settings
        /// </summary>
        public static Page OtherPage { get; private set; }

        /// <summary>
        /// Is the BoneMenu setup
        /// </summary>
        public static bool IsSetup { get; private set; }

        /// <summary>
        /// Should the saves be removed on pressed
        /// </summary>
        public static bool RemoveSavesOnPress { get; private set; }

        internal static void Setup()
        {
            AuthorPage = Page.Root.CreatePage("HAHOOS", Color.white);
            ModPage = AuthorPage.CreatePage("KeepInventory", Color.yellow);

            SetupSaves();
            Core.DefaultSaveChanged += UpdatePresetsPage;

            EventsPage = ModPage.CreatePage("Events", Color.yellow);
            EventsPage.CreateBoolPref("Save on Level Unload", Color.red, ref Core.mp_saveOnLevelUnload, prefDefaultValue: true);
            EventsPage.CreateBoolPref("Load on Level Load", Color.green, ref Core.mp_loadOnLevelLoad, prefDefaultValue: true);

            if (Core.HasFusion && Core.IsFusionLibraryInitialized)
                SetupSharing();

            BlacklistPage = ModPage.CreatePage("Blacklist", Color.red);

            BlacklistViewPage = BlacklistPage.CreatePage("View All", Color.magenta);
            SetupBlacklistView();

            BlacklistPage.CreateBoolPref("Blacklist BONELAB Levels", Color.cyan, ref Core.mp_blacklistBONELABlevels, prefDefaultValue: true);
            BlacklistPage.CreateBoolPref("Blacklist LABWORKS Levels", Color.yellow, ref Core.mp_blacklistLABWORKSlevels, prefDefaultValue: true);
            Core.statusElement = BlacklistPage.CreateFunction("Blacklist Level from Saving/Loading", Color.red, () =>
            {
                if (Core.bonelabBlacklist.Contains(Core.levelInfo.barcode) || Core.labworksBlacklist.Contains(Core.levelInfo.barcode))
                    return;

                List<string> blacklistList = Core.mp_blacklistedLevels.Value;
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
            OtherPage.CreateBoolPref("Show Notifications", Color.green, ref Core.mp_showNotifications, prefDefaultValue: true);
            OtherPage.CreateBoolPref("Remove Initial Inventory From Save", Color.red, ref Core.mp_initialInventoryRemove, prefDefaultValue: true);

            var modVersion = ModPage.CreateFunction(Core.IsLatestVersion || Core.ThunderstorePackage == null ? $"Current Version: v{Core.Version}" : $"Current Version: v{Core.Version}<br><color=#00FF00>(Update available!)</color>", Color.white, () => Core.Logger.Msg($"The current version is v{Core.Version}!!!!"));
            modVersion.SetProperty(ElementProperties.NoBorder);
            IsSetup = true;
        }

        private static void SetupBlacklistView()
        {
            if (BlacklistViewPage == null)
                return;

            BlacklistViewPage.RemoveAll();
            BlacklistViewPage.CreateFunction("Refresh", Color.yellow, SetupBlacklistView);
            BlacklistViewPage.CreateBlank();
            foreach (var level in Core.mp_blacklistedLevels.Value)
            {
                var reference = new LevelCrateReference(level);
                if (!reference.TryGetCrate(out LevelCrate crate))
                    continue;

                FunctionElement element = null;
                element = BlacklistViewPage.CreateFunction(crate.Title, Color.white, () =>
                {
                    Core.mp_blacklistedLevels.Value.Remove(level);
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
            SharingPage.CreateBool("Enabled", Color.red, Fusion.ShareManager.Entry_SharingEnabled.Value, (val) =>
            {
                Fusion.ShareManager.Entry_SharingEnabled.Value = val;
                Fusion.ShareManager.Category.SaveToFile(false);
            });
            SharingBlacklistPage ??= SharingPage.CreatePage("Blacklist", Color.red);
            SetupSharingBlacklist();
            LabFusion.Utilities.MultiplayerHooking.OnDisconnect += SetupSharingBlacklist;
            LabFusion.Utilities.MultiplayerHooking.OnJoinServer += SetupSharingBlacklist;
            LabFusion.Utilities.MultiplayerHooking.OnStartServer += SetupSharingBlacklist;
            LabFusion.Utilities.MultiplayerHooking.OnPlayerJoin += (_) => SetupSharingBlacklist();
            LabFusion.Utilities.MultiplayerHooking.OnPlayerLeave += (_) => SetupSharingBlacklist();
        }

        internal static void SetupSharingBlacklist()
        {
            SharingBlacklistPage.RemoveAll();
            SharingBlacklistPage.CreateFunction("Refresh", Color.yellow, SetupSharingBlacklist);
            SharingBlacklistPage.CreateBlank();
            var players = KeepInventory.Utilities.Fusion.GetPlayers();
            players.RemoveAll(x => x.SmallId == KeepInventory.Utilities.Fusion.GetLocalPlayerSmallId());
            if (players.Count == 0)
            {
                SharingBlacklistPage.CreateLabel("Nothing to show here :(", Color.white);
            }
            else
            {
                foreach (var player in players)
                {
                    var element = SharingBlacklistPage.CreateToggleFunction(player.DisplayName, Color.white, null);
                    element.Started += () =>
                    {
                        KeepInventory.Fusion.ShareManager.Entry_SharingBlacklist.Value.Add(player.LongId);
                        KeepInventory.Fusion.ShareManager.Category.SaveToFile(false);
                    };
                    element.Cancelled += () =>
                    {
                        KeepInventory.Fusion.ShareManager.Entry_SharingBlacklist.Value.Remove(player.LongId);
                        KeepInventory.Fusion.ShareManager.Category.SaveToFile(false);
                    };
                }
            }
        }

        internal static void SetupSaves()
        {
            SavesPage ??= ModPage.CreatePage("Saves", Color.cyan);

            SavingConfigPage ??= SavesPage.CreatePage("Config", Color.yellow, 4);
            SavingConfigPage.RemoveAll();
            SavingConfigPage.CreateBoolPref("Save Items", Color.white, ref Core.mp_itemsaving, prefDefaultValue: true);
            SavingConfigPage.CreateBoolPref("Save Ammo", Color.white, ref Core.mp_ammosaving, prefDefaultValue: true);
            SavingConfigPage.CreateBoolPref("Save Gun Data", Color.white, ref Core.mp_saveGunData, prefDefaultValue: true);

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
            func.Started += () =>
            {
                func.Element.ElementName = "Remove [ON]";
                RemoveSavesOnPress = true;
            };
            func.Cancelled += () =>
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

            defaultSaveElements.ForEach(element => PresetsPage.Add(element));

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

                link = PresetsPage.CreateFunction(Core.CurrentSave == save ? $"<color=#{save.Color}>+ {save.Name} +</color>" : $"<color=#{save.Color}>{save.Name}</color>", Color.white, () =>
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
                        }, () => Core.Logger.Msg("Dialog about removing save was denied"));
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