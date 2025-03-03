using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using KeepInventory.Helper;
using KeepInventory.Saves.V2;
using KeepInventory.Utilities;

using MelonLoader.Utils;

using Tomlet;
using Tomlet.Models;

namespace KeepInventory
{
    /// <summary>
    /// Class that manages the saves
    /// </summary>
    public static class SaveManager
    {
        /// <summary>
        /// Path to the directory that contains all the saves
        /// </summary>
        public static string SavesDirectory { get; private set; } = string.Empty;

        private static readonly List<Save> _Saves = [];

        internal static List<string> IgnoredFilePaths = [];

        /// <summary>
        /// <see cref="JsonSerializerOptions"/> for all operations relate to saves
        /// </summary>
        public static readonly JsonSerializerOptions SerializeOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false
        };

        /// <summary>
        /// List of all saves
        /// </summary>
        public static ReadOnlyCollection<Save> Saves => _Saves.AsReadOnly();

        internal static SynchronousFileSystemWatcher FileSystemWatcher { get; private set; }

        internal static void Setup()
        {
            var directory = Directory.CreateDirectory(Path.Combine(Core.KI_PreferencesDirectory, "Saves"));
            if (directory != null)
            {
                Core.Logger.Msg("Created save directory");
                SavesDirectory = directory.FullName;
                CreateFileWatcher();
                var files = directory.GetFiles();
                if (files?.Length > 0)
                {
                    foreach (var item in files)
                    {
                        try
                        {
                            if (Check(item.FullName)) RegisterSave(item.FullName);
                            else Core.Logger.Error($"Attempted to load {item.Name}, but it failed the check");
                        }
                        catch (Exception ex)
                        {
                            Core.Logger.Error($"An unexpected error has occurred while loading '{item.Name}', exception:\n{ex}");
                        }
                    }
                }
                else
                {
                    Core.Logger.Msg("Found no saves, checking if can migrate old one");
                    TomlDocument oldSave = null;
                    try
                    {
                        var path1 = Path.Combine(Core.KI_PreferencesDirectory, "Save.cfg");
                        var path2 = Path.Combine(MelonEnvironment.UserDataDirectory, "KeepInventory_Save.cfg");

                        // 0 - path1, 1 - path2
                        int correct = -1;

                        if (File.Exists(path1))
                        {
                            oldSave = TomlParser.ParseFile(path1);
                            correct = 0;
                        }
                        else if (File.Exists(path2))
                        {
                            oldSave = TomlParser.ParseFile(path2);
                            correct = 1;
                        }
                        if (oldSave != null && correct != -1)
                        {
                            Core.Logger.Msg("Found old save file, migrating...");
                            try
                            {
                                TomlValue subTable = null;
                                lock (oldSave)
                                {
                                    subTable = oldSave.GetSubTable(correct == 1 ? "HAHOOS_KeepInventory_Save" : "KeepInventory_Save");
                                }
                                if (subTable != null)
                                {
                                    Type type = correct == 0 ? typeof(KeepInventory.Saves.V1.Save) : typeof(KeepInventory.Saves.V0.Save);
                                    var _value = TomletMain.To(type, subTable);

                                    if (correct == 1)
                                    {
                                        if ((KeepInventory.Saves.V0.Save)_value == null)
                                        {
                                            Core.Logger.Error("Could not retrieve the value from old save!");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if ((KeepInventory.Saves.V1.Save)_value == null)
                                        {
                                            Core.Logger.Error("Could not retrieve the value from old save!");
                                            return;
                                        }
                                    }
                                    Save save = null;
                                    if (correct == 1) save = new Save($"migrated-{GenerateRandomID(6)}", "Migrated", Color.Aqua, true, false, (KeepInventory.Saves.V0.Save)_value);
                                    else save = new Save($"migrated-{GenerateRandomID(6)}", "Migrated", Color.Aqua, true, false, (KeepInventory.Saves.V1.Save)_value);
                                    RegisterSave(save, true);
                                    if (File.Exists(path1) && correct == 0) File.Delete(path1);
                                    if (File.Exists(path2) && correct == 1) File.Delete(path2);
                                    Core.Logger.Msg("Successfully migrated old save file");
                                }
                            }
                            catch (Exception ex)
                            {
                                Core.Logger.Error($"An unexpected error occurred while migrating old save file, exception:\n{ex}");
                            }
                        }
                        else
                        {
                            Core.Logger.Warning("Could not find an old save file");
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Logger.Error($"An unexpected error has occurred while attempting to check and migrate an old save file, exception:\n{ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Generates a string with random characters
        /// </summary>
        /// <param name="length">Length of string</param>
        /// <returns>A random string with specified length</returns>
        public static string GenerateRandomID(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
        }

        /// <summary>
        /// Registers a new <see cref="Save"/>
        /// </summary>
        /// <param name="save"><see cref="Save"/> that should be registered</param>
        /// <param name="createFile">Should a file be created with the save if not found</param>
        /// <exception cref="ArgumentException">A save with provided ID already exists</exception>
        public static void RegisterSave(Save save, bool createFile = true)
        {
            ArgumentNullException.ThrowIfNull(save, nameof(save));
            if (string.IsNullOrWhiteSpace(save.ID))
            {
                throw new ArgumentNullException(nameof(save), "ID cannot be empty or null!");
            }
            Core.Logger.Msg($"Registering save with ID '{save.ID}'");
            if (_Saves.Any(x => x.ID == save.ID))
            {
                Core.Logger.Error("A save with that ID already exists!");
                throw new ArgumentException($"A save with the ID '{save.ID}' already exists!");
            }
            _Saves.Add(save);
            if (createFile && string.IsNullOrWhiteSpace(save.FilePath))
            {
                string path = Path.Combine(SavesDirectory, $"{save.ID}.json");
                if (!File.Exists(path))
                {
                    IgnoredFilePaths.Add(path);
                    try
                    {
                        var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        save.FilePath = path;
                        var serialized = JsonSerializer.Serialize(save, SerializeOptions);
                        file.Write(Encoding.UTF8.GetBytes(serialized));
                        file.Flush();
                        file.Position = 0;
                        file.Dispose();
                    }
                    finally
                    {
                        IgnoredFilePaths.Remove(path);
                    }
                }
            }
            Core.Logger.Msg($"Registered save with ID '{save.ID}'");
        }

        /// <summary>
        /// Registers a new <see cref="Save"/>
        /// </summary>
        /// <param name="filePath">Path to the file that contains the <see cref="Save"/></param>
        /// <exception cref="ArgumentException">A save with provided ID already exists</exception>
        public static void RegisterSave(string filePath)
        {
            Core.Logger.Msg($"Attempting to load a save file at '{filePath}'");
            if (File.Exists(filePath))
            {
                string text = ReadAllTextUsedFile(filePath);
                if (string.IsNullOrWhiteSpace(text) || !IsJSON(text))
                {
                    throw new ArgumentException("The contents of the file are not JSON");
                }
                else
                {
                    var save = JsonSerializer.Deserialize<Save>(text, SerializeOptions);
                    if (save != null)
                    {
                        save.FilePath = filePath;
                        if (Saves.Any(x => x.FilePath == filePath))
                            return;

                        RegisterSave(save, false);
                    }
                }
            }
            else
            {
                throw new FileNotFoundException($"Could not find file {filePath}");
            }
        }

        internal static string ReadAllTextUsedFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"Could not read text from '{path}', because it doesn't exist");
            using var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (file != null)
            {
                file.Position = 0;
                using StreamReader reader = new(file);
                return reader.ReadToEnd();
            }
            return null;
        }

        private static bool Check(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!System.IO.File.Exists(path)) throw new FileNotFoundException($"Save file at '{path}' could be found");
            string text;
            try
            {
                text = ReadAllTextUsedFile(path);
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Unable to check the integrity of the file, because an unexpected error has occurred, exception:\n{ex}");
                return false;
            }
            if (string.IsNullOrWhiteSpace(text) || !SaveManager.IsJSON(text) || text == "{}")
            {
                Core.Logger.Error("The content of the file is not correct");
                return false;
            }
            else
            {
                var save = JsonSerializer.Deserialize<Save>(text, SerializeOptions);
                if (save != null)
                {
                    if (!string.IsNullOrWhiteSpace(save.ID))
                    {
                        if (SaveManager.Saves.Any(x => x.ID == save.ID && x.FilePath != path))
                        {
                            Core.Logger.Error("The ID is already used in another save, will not overwrite");
                            return false;
                        }
                    }
                    else
                    {
                        Core.Logger.Error($"The ID '{save.ID}' is null or empty, will not overwrite");
                        return false;
                    }
                }
                else
                {
                    Core.Logger.Error("Deserialized save is null");
                    return false;
                }
            }
            return true;
        }

        private static readonly Dictionary<string, DateTime> LastWrite = [];

        /// <summary>
        /// Checks last write time to prevent the <see cref="System.IO.FileSystemWatcher.Changed"/> from triggering twice
        /// </summary>
        /// <param name="file">The path to check</param>
        /// <returns>Was it a double trigger</returns>
        internal static bool PreventDoubleTrigger(string file)
        {
            if (!File.Exists(file))
            {
                LastWrite.Remove(file);
                return false;
            }

            var write = File.GetLastWriteTime(file);
            if (!LastWrite.ContainsKey(file))
            {
                LastWrite.Add(file, write);
                return false;
            }
            else
            {
                return LastWrite[file] == write;
            }
        }

        internal static void CreateFileWatcher()
        {
            LastWrite.Clear();
            FileSystemWatcher?.Dispose();
            FileSystemWatcher = new SynchronousFileSystemWatcher(SavesDirectory) { EnableRaisingEvents = true };
            FileSystemWatcher.Error += (x, y) => Core.Logger.Error($"An unexpected error was thrown by the file watcher for the saves, exception:\n{y.GetException()}");
            FileSystemWatcher.Deleted += (x, y) =>
            {
                if (IgnoredFilePaths.Contains(y.FullPath)) return;
                LastWrite.Remove(y.FullPath);
                if (y.FullPath.EndsWith(".json"))
                {
                    var saves = Saves.Where(x => x.FilePath == y.FullPath);
                    saves.ForEach(x => UnregisterSave(x.ID));
                }
            };
            FileSystemWatcher.Created += (x, y) =>
            {
                if (IgnoredFilePaths.Contains(y.FullPath)) return;
                if (y.FullPath.EndsWith(".json"))
                {
                    if (Check(y.FullPath)) RegisterSave(y.FullPath);
                    else Core.Logger.Error($"{y.Name} was created, but is not suitable to be a save");
                }
                else
                {
                    Core.Logger.Warning($"A file was created in the Saves directory that has an unsupported file format: '{Path.GetExtension(y.FullPath)}'");
                }
            };
            FileSystemWatcher.Changed += (x, y) =>
            {
                if (IgnoredFilePaths.Contains(y.FullPath)) return;
                if (PreventDoubleTrigger(y.FullPath)) return;
                if (y.FullPath.EndsWith(".json"))
                {
                    if (!Core.Instance.IsCurrentThreadMainThread)
                    {
                        Core.Logger.Error("Changed event is not running on main thread");
                        return;
                    }
                    if (Check(y.FullPath))
                    {
                        Core.Logger.Msg($"{y.Name} has been modified, updating");
                        var saves = Saves.Where(x => x.FilePath == y.FullPath && x.IsFileWatcherEnabled);
                        saves.ForEach(x => x.Update(y.FullPath));
                    }
                    else
                    {
                        Core.Logger.Error($"{y.Name} was updated, but is not suitable to be a save");
                    }
                }
            };
            FileSystemWatcher.Renamed += (x, y) =>
            {
                if (IgnoredFilePaths.Contains(y.FullPath)) return;
                if (LastWrite.ContainsKey(y.OldFullPath))
                {
                    var old = LastWrite[y.OldFullPath];
                    LastWrite.Remove(y.OldFullPath);
                    LastWrite.Add(y.FullPath, old);
                }
                if (y.FullPath.EndsWith(".json"))
                {
                    Core.Logger.Msg($"{y.OldName} has been renamed to {y.Name}, updating information");
                    var saves = Saves.Where(x => x.FilePath == y.OldFullPath);
                    saves.ForEach(x => x.FilePath = y.FullPath);
                }
            };
        }

        internal static bool IsJSON(string text)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(text);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Unregisters a <see cref="Save"/> with provided ID
        /// </summary>
        /// <param name="ID">ID of the <see cref="Save"/> to unregister</param>
        /// <param name="removeFile">Should the file be deleted as well to prevent loading again</param>
        /// <exception cref="KeyNotFoundException">A save with provided ID does not exist</exception>
        public static void UnregisterSave(string ID, bool removeFile = true)
        {
            Core.Logger.Msg($"Unregistering save with ID '{ID}'");
            var save = _Saves.FirstOrDefault(x => x.ID == ID);
            if (save != null)
            {
                _Saves.Remove(save);
                Core.Logger.Msg($"File Path: {save.FilePath}");
                if (removeFile && !string.IsNullOrWhiteSpace(save.FilePath))
                {
                    Core.Logger.Msg($"Removing file at '{save.FilePath}'");
                    if (File.Exists(save.FilePath))
                        File.Delete(save.FilePath);
                }
                Core.Logger.Msg($"Unregistered save with ID '{ID}'");
            }
            else
            {
                Core.Logger.Error($"A save with ID '{ID}' does not exist!");
                throw new KeyNotFoundException($"Save with ID '{ID}' could not be found!");
            }
        }
    }
}