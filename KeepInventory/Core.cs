using System;
using System.Linq;

using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.Notifications;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.SceneStreaming;

using KeepInventory.Menu;
using KeepInventory.Helper;
using KeepInventory.Managers;
using KeepInventory.Utilities;

using MelonLoader;

using Semver;

using UnityEngine;

namespace KeepInventory
{
    public class Core : MelonMod
    {
        public const string Version = "1.3.1";

        public static Saves.V2.Save CurrentSave
        {
            get
            {
                return SaveManager.Saves.FirstOrDefault(x => x.ID == PreferencesManager.DefaultSave.Value);
            }
            set
            {
                if (value == null)
                    PreferencesManager.DefaultSave.Value = string.Empty;

                PreferencesManager.DefaultSave.Value = value.ID;
                PreferencesManager.Save();
                DefaultSaveChanged?.Invoke();
            }
        }

        public static event Action DefaultSaveChanged;

        public static Core Instance { get; internal set; }

        internal static MelonLogger.Instance Logger { get; private set; }
        public static LevelInfo LevelInfo { get; private set; }
        public static bool HasFusion => FindMelon("LabFusion", "Lakatrazz") != null;
        public static bool IsFusionLibraryInitialized { get; internal set; } = false;
        public static bool FailedFLLoad { get; internal set; } = false;
        private bool InitialLoad = true;
        internal static Thunderstore ThunderstoreInstance { get; private set; }
        internal static bool IsLatestVersion { get; private set; } = true;
        internal static Package ThunderstorePackage { get; private set; }

        internal static bool Deinit { get; private set; } = false;

        public override void OnInitializeMelon()
        {
            Deinit = false;
            Logger = LoggerInstance;
            Instance = this;

            LoggerInstance.Msg("Setting up KeepInventory");

            CheckVersion();

            if (!HasFusion)
            {
                LoggerInstance.Warning("Could not find LabFusion, the mod will not use any of Fusion's functionality");
            }

            if (HasFusion)
            {
                LoggerInstance.Msg("Attempting to load the Fusion Support Library");
                if (DependencyManager.TryLoadDependency("KeepInventory.Fusion"))
                    IsFusionLibraryInitialized = true;
                else
                    FailedFLLoad = true;
            }

            Hooking.OnLevelLoaded += (lvl) => LevelLoadedEvent(lvl);
            if (HasFusion)
                Utilities.Fusion.TargetLevelLoadEvent(() => LevelLoadedEvent(new LevelInfo(SceneStreamer.Session.Level), true));

            Hooking.OnLevelUnloaded += LevelUnloadedEvent;

            if (IsFusionLibraryInitialized) Utilities.Fusion.SetupFusionLibrary();
            if (HasFusion) Utilities.Fusion.Setup();

            System.Collections.Generic.List<Level> labworks = [
                new("volx4.LabWorksBoneworksPort.Level.BoneworksLoadingScreen"),
                new("volx4.LabWorksBoneworksPort.Level.BoneworksMainMenu"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks01Breakroom"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks02Museum"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks03Streets"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks04Runoff"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks05Sewers"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks06Warehouse"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks07CentralStation"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks08Tower"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks09TimeTower"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks10Dungeon"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks11Arena"),
                new("volx4.LabWorksBoneworksPort.Level.Boneworks12ThroneRoom"),
                new("volx4.LabWorksBoneworksPort.Level.BoneworksCutscene01"),
                new("volx4.LabWorksBoneworksPort.Level.sceneTheatrigonMovie02")
            ];

            System.Collections.Generic.List<Level> bonelab = [
                new(CommonBarcodes.Maps.Home),
                new(CommonBarcodes.Maps.Ascent),
                new(CommonBarcodes.Maps.Descent),
                new(CommonBarcodes.Maps.MineDive),
                new(CommonBarcodes.Maps.LongRun),
                new(CommonBarcodes.Maps.BigAnomaly),
                new(CommonBarcodes.Maps.BigAnomaly2),
                new(CommonBarcodes.Maps.StreetPuncher),
                new(CommonBarcodes.Maps.SprintBridge),
                new(CommonBarcodes.Maps.MagmaGate),
                new(CommonBarcodes.Maps.Moonbase),
                new(CommonBarcodes.Maps.MonogonMotorway),
                new(CommonBarcodes.Maps.PillarClimb),
                new(CommonBarcodes.Maps.ContainerYard),
                new(CommonBarcodes.Maps.FantasyArena),
                new(CommonBarcodes.Maps.TunnelTipper),
                new(CommonBarcodes.Maps.DropPit),
                new(CommonBarcodes.Maps.NeonTrial),
                new(CommonBarcodes.Maps.MainMenu),
            ];

            BlacklistManager.Add(new("default_labworks", "LABWORKS", true, labworks));
            BlacklistManager.Add(new("default_bonelab", "BONELAB", true, bonelab));

            PreferencesManager.Setup();
            SaveManager.Setup();

            BoneMenu.Setup();

            AmmoManager.Track("light");
            AmmoManager.Track("medium");
            AmmoManager.Track("heavy");
            AmmoManager.Init();
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnDeinitializeMelon()
        {
            Deinit = true;
            LoggerInstance.Msg("Deinitialize requested, cleaning up.");
            AmmoManager.Destroy();
            PreferencesManager.Save();
            SaveManager.Saves.ForEach(x =>
            {
                x.IsFileWatcherEnabled = false;
                x.TrySaveToFile(true);
            });
        }

        private void CheckVersion()
        {
            ThunderstoreInstance = new Thunderstore($"KeepInventory / {Version} A BONELAB MelonLoader Mod");

            try
            {
                ThunderstorePackage = ThunderstoreInstance.GetPackage("HAHOOS", "KeepInventory");
                if (ThunderstorePackage != null)
                {
                    if (ThunderstorePackage.Latest != null && !string.IsNullOrWhiteSpace(ThunderstorePackage.Latest.Version))
                    {
                        IsLatestVersion = ThunderstorePackage.IsLatestVersion(Version);
                        if (!IsLatestVersion)
                        {
                            LoggerInstance.Warning($"A new version of KeepInventory is available: v{ThunderstorePackage.Latest.Version} while the current is v{Version}. It is recommended that you update");
                        }
                        else
                        {
                            if (SemVersion.Parse(Version) == ThunderstorePackage.Latest.SemanticVersion)
                            {
                                LoggerInstance.Msg($"Latest version of KeepInventory is installed! --> v{Version}");
                            }
                            else
                            {
                                LoggerInstance.Msg($"Beta release of KeepInventory is installed (v{ThunderstorePackage.Latest.Version} is newest, v{Version} is installed)");
                            }
                        }
                    }
                    else
                    {
                        LoggerInstance.Warning("Latest version could not be found or the version is empty");
                    }
                }
                else
                {
                    LoggerInstance.Warning("Could not find thunderstore package for KeepInventory");
                }
            }
            catch (Exception e)
            {
                LoggerInstance.Error("An unexpected error has occurred while trying to check if KeepInventory is the latest version", e);
            }
        }

        private void LevelUnloadedEvent()
        {
            if (!PreferencesManager.SaveOnLevelUnload.Value) return;

            if (!IsBlacklisted(LevelInfo.levelReference.Barcode))
            {
                if (CurrentSave != null)
                    InventoryManager.SaveInventory(CurrentSave);
                else
                    LoggerInstance.Warning("No default save is set, cannot save");
            }
            else
            {
                LoggerInstance.Warning("Not saving due to the level being blacklisted");
            }
        }

        private void LevelLoadedEvent(LevelInfo obj, bool triggeredByFusion = false)
        {
            if (triggeredByFusion && !Utilities.Fusion.IsConnected)
                return;

            BoneMenu.SetupPredefinedBlacklists();
            LevelInfo = obj;
            if (InitialLoad)
            {
                if (FailedFLLoad)
                {
                    BLHelper.SendNotification("Failure", "The Fusion Library has failed to load, which will cause the Sharing feature to not work. If this occurs again, create an issue on Github or DM the developer (@hahoos)", true, 10f, BoneLib.Notifications.NotificationType.Error);
                    IsFusionLibraryInitialized = false;
                }
                if (!IsLatestVersion && ThunderstorePackage != null)
                {
                    BLHelper.SendNotification(
                        "Update!",
                        new NotificationText($"There is a new version of KeepInventory. Go to Thunderstore and download the latest version which is <color=#00FF00>v{ThunderstorePackage.Latest.Version}</color>", Color.white, true),
                        true,
                        5f,
                        NotificationType.Warning);
                }
                InitialLoad = false;
            }
            if (CurrentSave != null)
            {
                if (!IsBlacklisted(obj.levelReference.Barcode))
                {
                    if (PreferencesManager.LoadOnLevelLoad.Value)
                    {
                        try
                        {
                            BoneMenu.StatusElement.ElementName = "Current level is not blacklisted";
                            BoneMenu.StatusElement.ElementColor = Color.green;

                            InventoryManager.LoadSavedInventory(CurrentSave);
                        }
                        catch (Exception ex)
                        {
                            LoggerInstance.Error("An error occurred while loading the inventory", ex);
                            BLHelper.SendNotification("Failure", "Failed to load the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
                        }
                    }
                }
                else
                {
                    LoggerInstance.Warning("Not loading inventory because level is blacklisted");
                    BLHelper.SendNotification("This level is blacklisted from loading/saving inventory", "Blacklisted", true, 5f, BoneLib.Notifications.NotificationType.Warning);
                    BoneMenu.StatusElement.ElementName = "Current level is blacklisted";
                    BoneMenu.StatusElement.ElementColor = Color.red;
                }
            }
        }

        private static bool IsBlacklisted(Barcode barcode)
            => barcode.IsLevelBlacklisted() || PreferencesManager.BlacklistedLevels.Value.Contains(barcode.ID);
    }
}