using Il2CppSLZ.Marrow;
using KeepInventory.Fusion.Messages;

using LabFusion.Player;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

using MelonLoader.Pastel;

using MelonLoader;

using System;

namespace KeepInventory.Fusion
{
    /// <summary>
    /// Module for Fusion
    /// </summary>
    public class FusionModule : Module
    {
        /// <summary>
        /// Name of the module
        /// </summary>
        public override string Name => "KeepInventory";

        /// <summary>
        /// Author of the module
        /// </summary>
        public override string Author => "HAHOOS";

        /// <summary>
        /// Version of the module
        /// </summary>
        public override Version Version => System.Version.Parse(_Version);

        /// <summary>
        /// Color in the console of the module
        /// </summary>
        public override ConsoleColor Color => ConsoleColor.Yellow;

        /// <summary>
        /// Runs when module gets registered
        /// </summary>
        protected override void OnModuleRegistered()
        {
            logger = LoggerInstance;
            MsgFusionPrefix("Registering ShareSaveMessage");
            ModuleMessageHandler.RegisterHandler<ShareSaveMessage>();
            MsgFusionPrefix("Registering CanShareRequestMessage");
            ModuleMessageHandler.RegisterHandler<CanShareRequestMessage>();
            MsgFusionPrefix("Registering CanShareResponseMessage");
            ModuleMessageHandler.RegisterHandler<CanShareResponseMessage>();
        }

        /// <summary>
        /// Version of the library, used mainly for AssemblyInfo
        /// </summary>
        public const string _Version = KeepInventory.Core.Version;

        /// <summary>
        /// Event that triggers when RigManager is created
        /// </summary>
        public static event Action OnRigCreated;

        internal static MelonLogger.Instance backupLogger;

        internal static ModuleLogger logger;

        private static void RigCreated(RigManager manager)
        {
            OnRigCreated?.Invoke();
        }

        /// <summary>
        /// Sets up the <see cref="MultiplayerHooking"/> and <see cref="MelonLoader.MelonLogger.Instance"/>
        /// </summary>
        public static void Setup(MelonLogger.Instance _logger)
        {
            backupLogger = _logger;

            LocalPlayer.OnLocalRigCreated -= RigCreated;

            LocalPlayer.OnLocalRigCreated += RigCreated;
        }

        /// <summary>
        /// Loads the messages
        /// </summary>
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