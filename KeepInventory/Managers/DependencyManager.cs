using System;
using System.IO;
using System.Linq;

namespace KeepInventory.Managers
{
    internal static class DependencyManager
    {
        internal static bool TryLoadDependency(string name)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                if (assembly != null)
                {
                    var assemblyInfo = assembly.GetName();
                    if (assemblyInfo != null)
                    {
                        var _path = $"{assemblyInfo.Name}.Embedded.Dependencies.{name}.dll";
                        var names = assembly.GetManifestResourceNames();
                        if (names == null || names.Length == 0 || !names.Contains(_path))
                        {
                            Core.Logger.Error($"There were no embedded resources or dependency was not found in the list of embedded resources, cannot not load {name}");
                            return false;
                        }
                        else
                        {
                            var stream = assembly.GetManifestResourceStream(_path);
                            if (stream?.Length > 0)
                            {
                                var bytes = StreamToByteArray(stream);
                                System.Reflection.Assembly.Load(bytes);
                                Core.Logger.Msg($"Loaded {name}");
                            }
                            else
                            {
                                Core.Logger.Error($"Could not get stream of {name}, cannot not load it");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Core.Logger.Error($"Assembly Info was not found, cannot not load {name}");
                        return false;
                    }
                }
                else
                {
                    Core.Logger.Error("Executing assembly was somehow not found, cannot not load Fusion Support Library");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"An unexpected error occurred while loading {name}:\n{ex}");
                return false;
            }
            return true;
        }
        public static byte[] StreamToByteArray(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using MemoryStream ms = new();
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
    }
}