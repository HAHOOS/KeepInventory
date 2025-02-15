using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using BoneLib.BoneMenu;

using KeepInventory.Helper;
using KeepInventory.Saves.V2;

using UnityEngine;

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

        public Page DataPage { get; set; }
        public Page AmmoPage { get; set; }

        public Page SharePage { get; set; }

        public FunctionElement LightAmmo { get; private set; }
        public FunctionElement MediumAmmo { get; private set; }
        public FunctionElement HeavyAmmo { get; private set; }

        public FunctionElement SetDefaultFunction { get; private set; }
        public FunctionElement SaveInventoryFunction { get; private set; }
        public FunctionElement LoadInventoryFunction { get; private set; }

        public ObservableCollection<byte> SelectedPlayers { get; }

        public void Clear()
        {
            Page?.RemoveAll();

            ID = null;
            Name = null;

            DataPage = null;

            AmmoPage = null;
            SharePage = null;

            LightAmmo = null;
            MediumAmmo = null;
            HeavyAmmo = null;

            SetDefaultFunction = null;
            SaveInventoryFunction = null;
            LoadInventoryFunction = null;
        }

        public void Setup()
        {
            if (Page == null) throw new ArgumentNullException(nameof(Page));
            Clear();

            ID = Page.CreateLabel($"ID: {CurrentSave.ID}", Color.white);
            Name = Page.CreateString("Name", Color.cyan, CurrentSave.Name, (value) => CurrentSave.Name = value);
            Name.Value = CurrentSave.Name;

            DataPage = Page.CreatePage("Data", Color.yellow, 0, true);
            AmmoPage = DataPage.CreatePage("Ammo", Color.red, 0, true);

            SetupAmmo();

            if (Core.HasFusion && Core.IsFusionLibraryInitialized)
            {
                SharePage = Page.CreatePage("Share", Color.cyan, 0, true);
                SetupFusion();
                SetupShare();
            }

            SetDefaultFunction = Core.CurrentSave != CurrentSave ? Page.CreateFunction("Set as default", Color.green, () =>
            {
                if (Core.CurrentSave != CurrentSave)
                {
                    Core.CurrentSave = CurrentSave;
                    BLHelper.SendNotification("Success", $"Successfully set '{CurrentSave}' as default!", true, 2f, BoneLib.Notifications.NotificationType.Success);
                    Setup();
                }
                else
                {
                    BLHelper.SendNotification("Warning", $"{CurrentSave} is already default!", true, 2f, BoneLib.Notifications.NotificationType.Warning);
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

        internal int ammoIDIndex = 0;

        internal readonly IReadOnlyList<int> ammoIDList =
        [
           1, 5, 25, 100, 1000, 10000, 10000
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
            if (AmmoPage == null) throw new ArgumentNullException(nameof(AmmoPage));
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
                SelectedPlayers.Clear();
                SetupShare();
            };
            LabFusion.Utilities.MultiplayerHooking.OnJoinServer += SetupShare;
            LabFusion.Utilities.MultiplayerHooking.OnStartServer += SetupShare;
            LabFusion.Utilities.MultiplayerHooking.OnPlayerJoin += (_) => SetupShare();
            LabFusion.Utilities.MultiplayerHooking.OnPlayerLeave += (player) =>
            {
                SelectedPlayers.Remove(player.SmallId);
                SetupShare();
            };
        }

        internal void SetupShare()
        {
            SharePage?.RemoveAll();
            SharePage.CreateFunction("Refresh", Color.yellow, SetupShare);
            SharePage.CreateFunction("Share", Color.cyan, () =>
            {
                if (SelectedPlayers.Count == 0) return;
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
            var players = KeepInventory.Utilities.Fusion.GetShareablePlayers();
            players.RemoveAll(x => x.SmallId == KeepInventory.Utilities.Fusion.GetLocalPlayerSmallId());
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