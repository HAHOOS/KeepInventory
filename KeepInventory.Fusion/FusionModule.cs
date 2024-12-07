using KeepInventory.Fusion.Messages;

using LabFusion.SDK.Modules;

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
        public override Version Version => System.Version.Parse(FusionMethods.Version);

        /// <summary>
        /// Color in the console of the module
        /// </summary>
        public override ConsoleColor Color => ConsoleColor.Yellow;

        /// <summary>
        /// Runs when module is registered
        /// </summary>
        internal static ModuleLogger ModuleLogger { get; set; }

        /// <summary>
        /// Runs when module gets registered
        /// </summary>
        protected override void OnModuleRegistered()
        {
            ModuleLogger = LoggerInstance;
            FusionMethods.MsgFusionPrefix("Registering GunUpdateMessage");
            ModuleMessageHandler.RegisterHandler<GunUpdateMessage>();
        }
    }
}