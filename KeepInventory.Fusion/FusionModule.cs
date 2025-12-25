using System;

using Il2CppSLZ.Marrow;

using KeepInventory.Fusion.Messages;

using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Modules;

using MelonLoader;
using MelonLoader.Pastel;

namespace KeepInventory.Fusion
{
    public class FusionModule : Module
    {
        public override string Name => "KeepInventory";
        public override string Author => "HAHOOS";
        public override Version Version => System.Version.Parse(_Version);
        public override ConsoleColor Color => ConsoleColor.Yellow;

        protected override void OnModuleRegistered()
        {
            Logger = LoggerInstance;
            MsgFusionPrefix("Registering ShareSaveMessage");
            ModuleMessageManager.RegisterHandler<ShareSaveMessage>();
        }

        public const string _Version = KeepInventory.Core.Version;

        public static event Action OnRigCreated;

        internal static MelonLogger.Instance BackupLogger { get; private set; }

        internal static ModuleLogger Logger { get; private set; }

        private static void RigCreated(RigManager manager)
        {
            OnRigCreated?.Invoke();
        }

        public static void Setup(MelonLogger.Instance _logger)
        {
            BackupLogger = _logger;

            LocalPlayer.OnLocalRigCreated -= RigCreated;

            LocalPlayer.OnLocalRigCreated += RigCreated;
        }

        public static void LoadModule()
        {
            MsgFusionPrefix("Loading module");
            ModuleManager.RegisterModule<FusionModule>();
        }

        internal static void MsgFusionPrefix(string message)
        {
            if (Logger == null && BackupLogger == null) return;
            if (Logger != null) Logger.Log(message);
            else MsgPrefix("Fusion", message, "#00FFFF");
        }

        internal static void Warn(string message)
        {
            if (Logger == null && BackupLogger == null) return;
            if (Logger != null) Logger.Warn(message);
            else BackupLogger.Warning($"[Fusion] {message}");
        }

        internal static void Error(string message)
        {
            if (Logger == null && BackupLogger == null) return;
            if (Logger != null) Logger.Error(message);
            else BackupLogger.Warning($"[Fusion] {message}");
        }

        internal static void MsgPrefix(string prefix, string message, string hex)
        {
            BackupLogger.MsgPastel($"[{prefix.Pastel(hex)}] {message}");
        }
    }
}