using MelonLoader;
using System.Reflection;
using System.Runtime.InteropServices;

#region MelonLoader

[assembly: MelonInfo(typeof(KeepInventory.KeepInventory), "Keep Inventory", KeepInventory.KeepInventory.Version, "HAHOOS", "https://github.com/HAHOOS/KeepInventory")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
[assembly: MelonAuthorColor(0, 255, 165, 0)]
[assembly: MelonColor(0, 255, 72, 59)]
[assembly: MelonOptionalDependencies("labfusion")]

#endregion MelonLoader

#region General

[assembly: AssemblyTitle("Keep your inventory when switching between mod levels, as well as when quitting the game!")]
[assembly: AssemblyDescription("Keep your inventory when switching between mod levels, as well as when quitting the game!")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("HAHOOS")]
[assembly: AssemblyProduct("KeepInventory")]
[assembly: AssemblyCulture("")]

#region Version

[assembly: AssemblyVersion(KeepInventory.KeepInventory.Version)]
[assembly: AssemblyFileVersion(KeepInventory.KeepInventory.Version)]
[assembly: AssemblyInformationalVersion(KeepInventory.KeepInventory.Version)]

#endregion Version

#endregion General

#region Other

[assembly: ComVisible(true)]
[assembly: Guid("b45d7a48-e5c2-49b2-a7a2-331de9c55d26")]

#endregion Other