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
        public const string Version = "1.3.0";

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

        internal static FunctionElement statusElement;
        internal static LevelInfo levelInfo;
        public static bool HasFusion => FindMelon("LabFusion", "Lakatrazz") != null;
        public static bool IsFusionLibraryInitialized { get; internal set; } = false;
        public static bool FailedFLLoad { get; internal set; } = false;
        private bool InitialLoad = true;
        internal static Thunderstore ThunderstoreInstance { get; private set; }
        internal static bool IsLatestVersion { get; private set; } = true;
        internal static Package ThunderstorePackage { get; private set; }

        public override void OnInitializeMelon()
        {
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

            if (!HasFusion)
                Hooking.OnLevelLoaded += LevelLoadedEvent;
            else
                Utilities.Fusion.TargetLevelLoadEvent(() => LevelLoadedEvent(new LevelInfo(SceneStreamer.Session.Level)));

            Hooking.OnLevelUnloaded += LevelUnloadedEvent;

            if (IsFusionLibraryInitialized) Utilities.Fusion.SetupFusionLibrary();
            if (HasFusion) Utilities.Fusion.Setup();

            BlacklistManager.Add(new("default_labworks", "LABWORKS", true, [
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
            ]));
            BlacklistManager.Add(new("default_bonelab", "BONELAB", true, [
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
            ]));

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
                            if (SemVersion.Parse(Version) == ThunderstorePackage.Latest.SemVersion)
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
                LoggerInstance.Error($"An unexpected error has occurred while trying to check if KeepInventory is the latest version, exception:\n{e}");
            }
        }

        private void LevelUnloadedEvent()
        {
            if (!PreferencesManager.SaveOnLevelUnload.Value) return;

            if (!IsBlacklisted(levelInfo.levelReference.Barcode))
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

        private void LevelLoadedEvent(LevelInfo obj)
        {
            BoneMenu.SetupPredefinedBlacklists();
            levelInfo = obj;
            if (InitialLoad)
            {
                if (FailedFLLoad)
                {
                    BLHelper.SendNotification("Failure", "The Fusion Support Library has failed to load, meaning the fusion support for the mod will be disabled. If this will occur again, please report it to the developer (discord @hahoos)", true, 10f, BoneLib.Notifications.NotificationType.Error);
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

            if (!IsBlacklisted(obj.levelReference.Barcode))
            {
                if (PreferencesManager.LoadOnLevelLoad.Value)
                {
                    if (HasFusion && Utilities.Fusion.IsConnected && !IsFusionLibraryInitialized)
                    {
                        LoggerInstance.Warning("The Fusion Library is not loaded. Try restarting the game.");
                    }
                    else
                    {
                        try
                        {
                            statusElement.ElementName = "Current level is not blacklisted";
                            statusElement.ElementColor = Color.green;

                            InventoryManager.LoadSavedInventory(CurrentSave);
                        }
                        catch (System.Exception ex)
                        {
                            LoggerInstance.Error("An error occurred while loading the inventory", ex);
                            BLHelper.SendNotification("Failure", "Failed to load the inventory, check the logs or console for more details", true, 5f, BoneLib.Notifications.NotificationType.Error);
                        }
                    }
                }
            }
            else
            {
                LoggerInstance.Warning("Not loading inventory because level is blacklisted");
                BLHelper.SendNotification("This level is blacklisted from loading/saving inventory", "Blacklisted", true, 5f, BoneLib.Notifications.NotificationType.Warning);
                statusElement.ElementName = "Current level is blacklisted";
                statusElement.ElementColor = Color.red;
            }
        }

        private static bool IsBlacklisted(Barcode barcode)
            => BlacklistManager.IsLevelBlacklisted(barcode) || PreferencesManager.BlacklistedLevels.Value.Contains(barcode.ID);
    }
}