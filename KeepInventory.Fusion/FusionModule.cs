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
            logger = LoggerInstance;
            MsgFusionPrefix("Registering ShareSaveMessage");
            ModuleMessageManager.RegisterHandler<ShareSaveMessage>();
            MsgFusionPrefix("Registering CanShareRequestMessage");
            ModuleMessageManager.RegisterHandler<CanShareRequestMessage>();
            MsgFusionPrefix("Registering CanShareResponseMessage");
            ModuleMessageManager.RegisterHandler<CanShareResponseMessage>();
        }

        public const string _Version = KeepInventory.Core.Version;

        public static event Action OnRigCreated;

        internal static MelonLogger.Instance backupLogger;

        internal static ModuleLogger logger;

        private static void RigCreated(RigManager manager)
        {
            OnRigCreated?.Invoke();
        }

        public static void Setup(MelonLogger.Instance _logger)
        {
            backupLogger = _logger;

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
            if (logger == null && backupLogger == null) return;
            if (logger != null) logger.Log(message);
            else MsgPrefix("Fusion", message, System.Drawing.Color.Cyan);
        }

        internal static void Warn(string message)
        {
            if (logger == null && backupLogger == null) return;
            if (logger != null) logger.Warn(message);
            else backupLogger.Warning($"[Fusion] {message}");
        }

        internal static void Error(string message)
        {
            if (logger == null && backupLogger == null) return;
            if (logger != null) logger.Error(message);
            else backupLogger.Warning($"[Fusion] {message}");
        }

        internal static void MsgPrefix(string prefix, string message, System.Drawing.Color color)
        {
            backupLogger.Msg($"[{prefix.Pastel(color)}] {message}");
        }
    }
}