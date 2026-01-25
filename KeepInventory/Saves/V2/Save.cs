using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

using KeepInventory.Helper;
using KeepInventory.Managers;

using UnityEngine;

namespace KeepInventory.Saves.V2
{
    public class Save
    {
        [JsonProperty("Version")]
        public readonly int Version = 2;

        [JsonIgnore]
        private string _name;

        [JsonProperty(nameof(Name))]
        public string Name
        {
            get { return _name; }
            set
            {
                var old = _name;
                _name = value;
                OnPropertyChanged?.Invoke(nameof(Name), old, value);
            }
        }

        [JsonIgnore]
        private string _id;

        [JsonProperty(nameof(ID))]
        public string ID
        {
            get { return _id; }
            set
            {
                var old = _id;
                _id = value;
                OnPropertyChanged?.Invoke(nameof(ID), old, value);
            }
        }

        [JsonIgnore]
        public Color DrawingColor { get; set; } = UnityEngine.Color.white;

        [JsonProperty(nameof(Color))]
        public float[] Color
        {
            get => [DrawingColor.r * 255, DrawingColor.g * 255, DrawingColor.b * 255];
            set
            {
                if (value == null || value.Length != 3)
                {
                    DrawingColor = UnityEngine.Color.white;
                }
                else
                {
                    var old = DrawingColor;
                    DrawingColor = new Color(value[0] / 255, value[1] / 255, value[2] / 255);
                    OnPropertyChanged?.Invoke(nameof(Color), old, DrawingColor);
                }
            }
        }

        [JsonIgnore]
        public bool IsFileWatcherEnabled { get; set; } = true;

        [JsonIgnore]
        private string _filePath;

        [JsonIgnore]
        public string FilePath
        {
            get => _filePath;
            internal set
            {
                var old = _filePath;
                _filePath = value;
                OnPropertyChanged?.Invoke(nameof(FilePath), old, value);
            }
        }

        [JsonIgnore]
        private int _lightAmmo = -1;

        [JsonProperty(nameof(LightAmmo))]
        public int LightAmmo
        {
            get { return _lightAmmo; }
            set
            {
                var old = _lightAmmo;
                _lightAmmo = value;
                OnPropertyChanged?.Invoke(nameof(LightAmmo), old, value);
            }
        }

        [JsonIgnore]
        private int _mediumAmmo = -1;

        [JsonProperty(nameof(MediumAmmo))]
        public int MediumAmmo
        {
            get { return _mediumAmmo; }
            set
            {
                var old = _mediumAmmo;
                _mediumAmmo = value;
                OnPropertyChanged?.Invoke(nameof(MediumAmmo), old, value);
            }
        }

        [JsonIgnore]
        private int _heavyAmmo = -1;

        [JsonProperty(nameof(HeavyAmmo))]
        public int HeavyAmmo
        {
            get { return _heavyAmmo; }
            set
            {
                var old = _heavyAmmo;
                _heavyAmmo = value;
                OnPropertyChanged?.Invoke(nameof(HeavyAmmo), old, value);
            }
        }

        [JsonIgnore]
        private List<SaveSlot> _inventorySlots;

        [JsonProperty(nameof(InventorySlots))]
        public List<SaveSlot> InventorySlots
        {
            get { return _inventorySlots; }
            set
            {
                var old = _inventorySlots;
                _inventorySlots = value;
                OnPropertyChanged?.Invoke(nameof(InventorySlots), old, value);
            }
        }

        [JsonConstructor]
        public Save()
        {
        }

        public Save(Save old)
        {
            _name = old.Name;
            _id = old.ID;
            Color = old.Color;
            _lightAmmo = old.LightAmmo;
            _mediumAmmo = old.MediumAmmo;
            _heavyAmmo = old.HeavyAmmo;
            _inventorySlots = [.. old.InventorySlots];
        }

        public Save(string id, string name, Color color, V1.Save v1save)
        {
            _id = id;
            _name = name;
            DrawingColor = color;
            _lightAmmo = v1save.LightAmmo;
            _mediumAmmo = v1save.MediumAmmo;
            _heavyAmmo = v1save.HeavyAmmo;
            List<SaveSlot> _new = [];
            v1save.InventorySlots?.ForEach(x => _new.Add(new SaveSlot(x)));
            _inventorySlots = _new;
        }

        public Save(string id, string name, Color color, V0.Save v0save)
        {
            _id = id;
            _name = name;
            DrawingColor = color;
            _lightAmmo = v0save.AmmoLight;
            _mediumAmmo = v0save.AmmoMedium;
            _heavyAmmo = v0save.AmmoHeavy;
            List<SaveSlot> _new = [];
            v0save.ItemSlots?.ForEach(x => _new.Add(new SaveSlot()
            {
                Barcode = x.Value,
                SlotName = x.Key,
                Type = SaveSlot.SpawnableType.Other,
                GunInfo = null
            }));
            _inventorySlots = _new;
        }

        public event Action<string, object, object> OnPropertyChanged;

        internal bool AutoUpdate(string path) => FilePath == path && IsFileWatcherEnabled;

        internal void Update(Save save)
        {
            ArgumentNullException.ThrowIfNull(save);
            if (!string.IsNullOrWhiteSpace(save.ID))
            {
                if (SaveManager.Saves.Any(x => x.ID == save.ID && x != this))
                {
                    Core.Logger.Error("The new ID is already used in another save, will not overwrite");
                    throw new ArgumentException("The new ID is already used in another save, will not overwrite");
                }
                else
                {
                    this.ID = save.ID;
                }
            }
            else
            {
                Core.Logger.Error("The new ID is null or empty, will not overwrite");
                throw new ArgumentException("The new ID is null or empty, will not overwrite");
            }
            if (this.Name != save.Name) this.Name = save.Name;
            if (this.Color != save.Color) this.Color = save.Color;
            if (this.InventorySlots != save.InventorySlots) this.InventorySlots = save.InventorySlots;
            if (this.HeavyAmmo != save.HeavyAmmo) this.HeavyAmmo = save.HeavyAmmo;
            if (this.MediumAmmo != save.MediumAmmo) this.MediumAmmo = save.MediumAmmo;
            if (this.LightAmmo != save.LightAmmo) this.LightAmmo = save.LightAmmo;
        }

        internal void Update(string path)
        {
            SaveManager.IgnoredFilePaths.Add(FilePath);
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException($"Save file at '{path}' could be found");
            var text = SaveManager.ReadAllTextUsedFile(path);
            if (string.IsNullOrWhiteSpace(text) || !SaveManager.IsJSON(text))
            {
                Core.Logger.Error($"A save file at '{path}' was changed and the content are no longer suitable for loading as a save. This means that the save at runtime will not be overwritten by new content");
                throw new InvalidDataException($"A save file at '{path}' was changed and the content are no longer suitable for loading as a save. This means that the save at runtime will not be overwritten by new content");
            }
            else
            {
                var save = JsonConvert.DeserializeObject<Save>(text);
                if (save != null)
                {
                    Update(save);
                }
                else
                {
                    Core.Logger.Error($"A save file at '{path}' was changed and the content are no longer suitable for loading as a save. This means that the save at runtime will not be overwritten by new content");
                    throw new InvalidDataException($"A save file at '{path}' was changed and the content are no longer suitable for loading as a save. This means that the save at runtime will not be overwritten by new content");
                }
            }
        }

        public void SaveToFile(bool printMessage = true)
        {
            if (!string.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath))
            {
                SaveManager.IgnoredFilePaths.TryAdd(FilePath);
                LoggerMsg($"Saving '{ID}' to file...", printMessage);
                try
                {
                    var serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
                    var file = File.Create(FilePath);
                    using var writer = new StreamWriter(file) { AutoFlush = true };
                    file.Position = 0;
                    writer.Write(serialized);
                    writer.DisposeAsync().AsTask().ContinueWith((task) =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            LoggerMsg($"Saved '{ID}' to file successfully!", printMessage);
                        }
                        else
                        {
                            LoggerError($"Failed to save '{ID}' to file", task.Exception, printMessage);
                        }
                    });
                }
                catch (Exception ex)
                {
                    LoggerError($"Failed to save '{ID}' to file", ex, printMessage);
                    throw;
                }
            }
            else
            {
                LoggerError($"Save '{ID}' does not have a file set or it doesn't exist!", print: printMessage);
                throw new FileNotFoundException("Save does not have a file!");
            }
        }

        private static void LoggerMsg(string message, bool print = false)
        {
            if (print) Core.Logger.Msg(message);
        }

        private static void LoggerError(string message, Exception ex = null, bool print = false)
        {
            if (print)
            {
                if (ex != null)
                    Core.Logger.Error(message, ex);
                else
                    Core.Logger.Error(message);
            }
        }

        public bool TrySaveToFile(bool printMessage = true)
        {
            try
            {
                SaveToFile(printMessage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string ToString()
           => this.Name;
    }
}