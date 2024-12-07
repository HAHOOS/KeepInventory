using MelonLoader;

using System.Reflection;
using System.Runtime.InteropServices;

#region MelonLoader

[assembly: MelonInfo(typeof(KeepInventory.Core), "KeepInventory", KeepInventory.Core.Version, "HAHOOS", "https://thunderstore.io/c/bonelab/p/HAHOOS/KeepInventory/")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
[assembly: MelonAuthorColor(0, 255, 165, 0)]
[assembly: MelonColor(0, 255, 255, 0)]
// Make ML shut up about KeepInventory.Fusion not loaded (it gets loaded on initialization which is after the message is thrown)
[assembly: MelonOptionalDependencies("LabFusion", "KeepInventory.Fusion")]

#endregion MelonLoader

#region General

[assembly: AssemblyTitle("Keep your inventory when switching between mod levels, as well as when quitting the game!")]
[assembly: AssemblyDescription("Keep your inventory when switching between mod levels, as well as when quitting the game!")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("HAHOOS")]
[assembly: AssemblyProduct("KeepInventory")]
[assembly: AssemblyCulture("")]

#region Version

[assembly: AssemblyVersion(KeepInventory.Core.Version)]
[assembly: AssemblyFileVersion(KeepInventory.Core.Version)]
[assembly: AssemblyInformationalVersion(KeepInventory.Core.Version)]

#endregion Version

#endregion General

#region Other

[assembly: ComVisible(true)]
[assembly: Guid("b45d7a48-e5c2-49b2-a7a2-331de9c55d26")]

#endregion Other