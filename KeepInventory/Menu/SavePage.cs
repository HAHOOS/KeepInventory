using System;
using System.Collections.Generic;
using System.IO;

using BoneLib.BoneMenu;

using Il2CppSLZ.Marrow.Warehouse;

using KeepInventory.Helper;
using KeepInventory.Saves.V2;

using UnityEngine;

using static Il2CppSLZ.Marrow.Gun;

namespace KeepInventory.Menu
{
    internal class SavePage
    {
        public readonly Page Page;

        private Save _save;

        public Save CurrentSave
        {
            get
            {
                return _save;
            }
            set
            {
                if (_save != null)
                    _save.OnPropertyChanged -= PropertyChanged;
                _save = value;
                Setup();
            }
        }

        public SavePage(Page page)
        {
            Page = page;
        }

        public SavePage(Page page, Save save)
        {
            Page = page;
            CurrentSave = save;
        }

        public FunctionElement ID { get; set; }

        public StringElement Name { get; set; }

        public FunctionElement FileName { get; set; }

        public Page ColorPage { get; set; }

        public Page DataPage { get; set; }
        public Page AmmoPage { get; set; }

        public Page SlotsPage { get; set; }

        public Page SharePage { get; set; }

        public FunctionElement LightAmmo { get; private set; }
        public FunctionElement MediumAmmo { get; private set; }
        public FunctionElement HeavyAmmo { get; private set; }

        public FunctionElement SetDefaultFunction { get; private set; }
        public FunctionElement SaveInventoryFunction { get; private set; }
        public FunctionElement LoadInventoryFunction { get; private set; }

        public List<byte> SelectedPlayers { get; }

        public void Clear()
        {
            Page?.RemoveAll();
            DataPage?.RemoveAll();
            AmmoPage?.RemoveAll();
            SlotsPage?.RemoveAll();
            SharePage?.RemoveAll();
            ColorPage?.RemoveAll();

            ID = null;
            Name = null;
            FileName = null;

            LightAmmo = null;
            MediumAmmo = null;
            HeavyAmmo = null;

            SetDefaultFunction = null;
            SaveInventoryFunction = null;
            LoadInventoryFunction = null;

            Core.DefaultSaveChanged -= Setup;
            CurrentSave.OnPropertyChanged -= PropertyChanged;
        }

        private void PropertyChanged(string name, object oldVal, object newVal)
            => Setup();

        public void Setup()
        {
            if (Page == null) throw new ArgumentNullException(nameof(Page));
            Page.Name = $"<color=#{CurrentSave.Color}>{CurrentSave.Name}</color>";
            Clear();
            Core.DefaultSaveChanged += Setup;
            CurrentSave.OnPropertyChanged += PropertyChanged;

            ID = Page.CreateLabel($"ID: {CurrentSave.ID}", Color.white);
            Name = Page.CreateString("Name", Color.cyan, CurrentSave.Name, (value) =>
            {
                CurrentSave.Name = value;
                CurrentSave.TrySaveToFile(false);
                BoneMenu.UpdatePresetsPage();
            });
            Name.Value = CurrentSave.Name;

            if (!string.IsNullOrWhiteSpace(CurrentSave.FilePath))
            {
                FileName = Page.CreateLabel($"File Name: {Path.GetFileName(CurrentSave.FilePath)}", Color.white);
            }

            ColorPage ??= Page.CreatePage("Color", Color.magenta, 0, false);
            Page.CreatePageLink(ColorPage);
            SetupColor();

            DataPage ??= Page.CreatePage("Data", Color.yellow, 0, false);
            Page.CreatePageLink(DataPage);
            AmmoPage ??= DataPage.CreatePage("Ammo", Color.red, 0, false);
            DataPage.CreatePageLink(AmmoPage);
            SlotsPage ??= DataPage.CreatePage("<color=#00FF00>Slots</color>", Color.white, 0, false);
            DataPage.CreatePageLink(SlotsPage);

            if (AssetWarehouse.ready)
                SetupSlots();
            else
                AssetWarehouse.OnReady((Action)SetupSlots);

            SetupAmmo();

            if (Core.HasFusion && Core.IsFusionLibraryInitialized)
            {
                SharePage ??= Page.CreatePage("Share", Color.cyan, 0, false);
                Page.CreatePageLink(SharePage);
                SetupFusion();
                SetupShare();
            }

            SetDefaultFunction = Core.CurrentSave != CurrentSave ? Page.CreateFunction("Set as default", Color.green, () =>
            {
                if (Core.CurrentSave != CurrentSave)
                {
                    Core.CurrentSave = CurrentSave;
                    BLHelper.SendNotification("Success", $"Successfully set '<color=#{CurrentSave.Color}>{CurrentSave}</color>' as default!", true, 2f, BoneLib.Notifications.NotificationType.Success);
                    Setup();
                }
                else
                {
                    BLHelper.SendNotification("Warning", $"<color=#{CurrentSave.Color}>{CurrentSave}</color> is already default!", true, 2f, BoneLib.Notifications.NotificationType.Warning);
                }
            }) : Page.CreateLabel("Save is default", Color.white);
            if (CurrentSave.CanBeOverwrittenByPlayer)
            {
                SaveInventoryFunction = Page.CreateFunction("Save inventory to save", Color.cyan, () =>
            {
                InventoryManager.SaveInventory(CurrentSave, true);
                Setup();
            });
            }
            else
            {
                SaveInventoryFunction = Page.CreateFunction("Saving unavailable for this preset", Color.red, null);
                SaveInventoryFunction.SetProperty(ElementProperties.NoBorder);
            }
            LoadInventoryFunction = Page.CreateFunction("Load inventory from save", Color.yellow, () => InventoryManager.LoadSavedInventory(CurrentSave));
        }

        private static List<int> IncrementValues = [1, 5, 10, 25, 100];
        private int IncrementIndex = 0;

        public int NextIncrement()
        {
            IncrementIndex++;
            IncrementIndex %= IncrementValues.Count;

            return IncrementValues[IncrementIndex];
        }

        private void PopulateGunInfoPage(Page page, SaveSlot slot)
        {
            var info = slot.GunInfo;
            if (info == null)
                return;

            page?.RemoveAll();

            if (CurrentSave.CanBeOverwrittenByPlayer)
            {
                page.CreateBool("Is Mag", Color.white, info.IsMag, (val) => info.IsMag = val);
                page.CreateBool("Is Bullet In Chamber", Color.white, info.IsBulletInChamber, (val) => info.IsBulletInChamber = val);
                page.CreateEnum("Fire Mode", Color.yellow, info.FireMode, (val) => info.FireMode = (FireMode)val);
                page.CreateInt("Rounds Left", Color.cyan, info.RoundsLeft, IncrementValues[IncrementIndex], 0, int.MaxValue, (val) => info.RoundsLeft = val);
                page.CreateFunction($"Increment: {IncrementValues[IncrementIndex]}", Color.magenta, () =>
                {
                    NextIncrement();
                    PopulateGunInfoPage(page, slot);
                });
                page.CreateEnum("Hammer State", Color.yellow, info.HammerState, (val) => info.HammerState = (HammerStates)val);
                page.CreateEnum("Slide State", Color.cyan, info.SlideState, (val) => info.SlideState = (SlideStates)val);
                page.CreateEnum("Cartridge State", Color.magenta, info.CartridgeState, (val) => info.CartridgeState = (CartridgeStates)val);
                page.CreateBool("Has Fired Once", Color.red, info.HasFiredOnce, (val) => info.HasFiredOnce = val);
                page.CreateFunction("<color=#00FF00>Save updated gun info</color>", Color.white, () =>
                {
                    int index = CurrentSave.InventorySlots.FindIndex(x => x.SlotName == slot.SlotName && x.Barcode == slot.Barcode);
                    if (index == -1)
                    {
                        BLHelper.SendNotification("Failure", "Cannot save updated gun info, the original slot cannot be found!", true, 3f, BoneLib.Notifications.NotificationType.Error);
                    }
                    else
                    {
                        var copy = CurrentSave.InventorySlots[index];
                        copy.GunInfo = info;
                        CurrentSave.InventorySlots[index] = copy;
                        CurrentSave.TrySaveToFile(false);
                        SetupSlots();
                    }
                });
            }
            else
            {
                page.CreateLabel($"Is Mag: {info.IsMag}", Color.white);
                page.CreateLabel($"Is Bullet In Chamber: {info.IsBulletInChamber}", Color.white);
                page.CreateLabel($"Fire Mode: {Enum.GetName(info.FireMode)}", Color.yellow);
                page.CreateLabel($"Rounds Left: {info.RoundsLeft}", Color.cyan);
                page.CreateLabel($"Hammer State: {Enum.GetName(info.HammerState)}", Color.yellow);
                page.CreateLabel($"Slide State: {Enum.GetName(info.SlideState)}", Color.cyan);
                page.CreateLabel($"Cartridge State: {Enum.GetName(info.CartridgeState)}", Color.magenta);
                page.CreateLabel($"Has Fired Once: {info.HasFiredOnce}", Color.red);
            }
        }

        private void SetupSlots()
        {
            SlotsPage?.RemoveAll();

            var slots = CurrentSave.InventorySlots;
            if (slots == null || slots.Count == 0)
            {
                SlotsPage.CreateLabel("No slots saved :(", Color.white);
            }
            else
            {
                foreach (var slot in slots)
                {
                    var reference = new SpawnableCrateReference(slot.Barcode);
                    bool found = reference.TryGetCrate(out SpawnableCrate crate);
                    var slotPage = SlotsPage.CreatePage($"{slot.SlotName}: {(found ? crate.Title : slot.Barcode)}", found ? Color.white : Color.red);

                    slotPage.CreateLabel(slot.Barcode, Color.white);
                    slotPage.CreateLabel("Slot: " + slot.SlotName, Color.white);
                    if (slot.GunInfo != null)
                    {
                        var gunInfoPage = slotPage.CreatePage("Gun Info", Color.yellow);
                        PopulateGunInfoPage(gunInfoPage, slot);
                    }
                    slotPage.CreateBlank();
                    slotPage.CreateFunction("Remove", Color.red, () =>
                    {
                        BoneLib.BoneMenu.Menu.DisplayDialog("Destructive action", "You are about to delete an inventory slot which after done, cannot be reversed. Are you sure?", Dialog.WarningIcon, () =>
                        {
                            var index = CurrentSave.InventorySlots.FindIndex(x => x.SlotName == slot.SlotName);
                            if (index == -1)
                            {
                                BLHelper.SendNotification("Failure", "Cannot remove inventory slot, because it was not found!", true, 3f, BoneLib.Notifications.NotificationType.Error);
                            }
                            else
                            {
                                CurrentSave.InventorySlots.RemoveAt(index);
                                CurrentSave.TrySaveToFile(false);
                                SetupSlots();
                            }
                        });
                    });
                }
            }
        }

        private void SetupColor()
        {
            var preview = ColorPage.CreateFunction($"Preview: <color=#{CurrentSave.Color}>{CurrentSave.Name}</color>", Color.white, null);

            const float increment = 0.05f;

            Color.RGBToHSV(new Color(CurrentSave.DrawingColor.R / 255, CurrentSave.DrawingColor.G / 255, CurrentSave.DrawingColor.B / 255), out float H, out float S, out float V);

            void apply()
            {
                var unity = Color.HSVToRGB(H, S, V);
                var color = System.Drawing.Color.FromArgb(0,
                    (int)Math.Round(unity.r * 255, 0),
                    (int)Math.Round(unity.g * 255, 0),
                    (int)Math.Round(unity.b * 255, 0));
                CurrentSave.DrawingColor = color;
                CurrentSave.TrySaveToFile(false);
                BoneMenu.UpdatePresetsPage();
                preview.ElementName = $"Preview: <color=#{CurrentSave.Color}>{CurrentSave.Name}</color>";
            }

            ColorPage.CreateFloat("Hue", Color.red, H, increment, 0, 1, (val) =>
            {
                H = val;
                apply();
            });
            ColorPage.CreateFloat("Saturation", Color.green, S, increment, 0, 1, (val) =>
            {
                S = val;
                apply();
            });
            ColorPage.CreateFloat("Value", Color.blue, V, increment, 0, 1, (val) =>
            {
                V = val;
                apply();
            });
        }

        internal int ammoIDIndex = 0;

        internal readonly IReadOnlyList<int> ammoIDList =
        [
           1, 5, 25, 100, 1000, 10000, 100000, 1000000
        ];

        internal int GetIncrementDecrement()
        {
            if (ammoIDIndex < 0) return -1;
            var value = ammoIDList;
            if (value == null) return -1;
            if (ammoIDIndex > value.Count - 1)
            {
                ammoIDIndex = value.Count - 1;
                return value[ammoIDIndex];
            }
            else
            {
                return value[ammoIDIndex];
            }
        }

        internal void SetupAmmo()
        {
            if (AmmoPage == null)
                throw new ArgumentNullException(nameof(AmmoPage));

            AmmoPage.RemoveAll();
            int id = GetIncrementDecrement();

            int light = CurrentSave.LightAmmo;
            int medium = CurrentSave.MediumAmmo;
            int heavy = CurrentSave.HeavyAmmo;

            List<Element> elements = [
                CurrentSave.CanBeOverwrittenByPlayer ? new FunctionElement($"Increment/Decrement by {id}", Color.cyan, () => {
                    ammoIDIndex++;
                    SetupAmmo();
                }) : null,
                CurrentSave.CanBeOverwrittenByPlayer ? new IntElement("Light Ammo", Color.green, light, id, 0, int.MaxValue, (value)=> CurrentSave.LightAmmo = value) : new LabelElement($"Light Ammo: {light}", Color.green).Element,
                CurrentSave.CanBeOverwrittenByPlayer ? new IntElement("Medium Ammo", Color.yellow, medium, id, 0, int.MaxValue, (value)=> CurrentSave.MediumAmmo = value) : new LabelElement($"Medium Ammo: {light}", Color.yellow).Element,
                CurrentSave.CanBeOverwrittenByPlayer ? new IntElement("Heavy Ammo", Color.red, light, id, 0, int.MaxValue, (value)=> CurrentSave.HeavyAmmo = value) : new LabelElement($"Heavy Ammo: {light}", Color.red).Element,
            ];

            elements.ForEach(element => { if (element != null) AmmoPage.Add(element); });
        }

        internal void SetupFusion()
        {
            LabFusion.Utilities.MultiplayerHooking.OnDisconnect += () =>
            {
                SelectedPlayers?.Clear();
                SetupShare();
            };
            LabFusion.Utilities.MultiplayerHooking.OnJoinServer += SetupShare;
            LabFusion.Utilities.MultiplayerHooking.OnStartServer += SetupShare;
            LabFusion.Utilities.MultiplayerHooking.OnPlayerJoin += (_) => SetupShare();
            LabFusion.Utilities.MultiplayerHooking.OnPlayerLeave += (player) =>
            {
                SelectedPlayers?.Remove(player.SmallId);
                SetupShare();
            };
        }

        internal void SetupShare()
        {
            if (SharePage == null)
                return;

            SharePage?.RemoveAll();
            SharePage.CreateFunction("Refresh", Color.yellow, SetupShare);
            SharePage.CreateFunction("Share", Color.cyan, () =>
            {
                if (SelectedPlayers == null || SelectedPlayers.Count == 0) return;
                try
                {
                    SelectedPlayers?.ForEach(player => KeepInventory.Utilities.Fusion.ShareSave(player, _save));
                }
                catch (Exception ex)
                {
                    BLHelper.SendNotification("Failure", "Failed to share save, check console or logs for more information", true, 2, BoneLib.Notifications.NotificationType.Error);
                    Core.Logger.Error($"An unexpected error has occurred while trying to share save, exception:\n{ex}");
                }
            });
            SharePage.CreateBlank();
            var players = Utilities.Fusion.GetShareablePlayers();
            players.RemoveAll(x => x.SmallId == Utilities.Fusion.GetLocalPlayerSmallId());
            if (players.Count == 0)
            {
                SharePage.CreateLabel("You can't share the save /w anyone :(", Color.white);
            }
            else
            {
                foreach (var player in players)
                {
                    var element = SharePage.CreateToggleFunction(player.DisplayName, Color.white, null);
                    element.Started += () => SelectedPlayers.Add(player.SmallId);
                    element.Cancelled += () => SelectedPlayers.Remove(player.SmallId);
                    if (SelectedPlayers.Contains(player.SmallId))
                        element.Start();
                }
            }
        }
    }
}