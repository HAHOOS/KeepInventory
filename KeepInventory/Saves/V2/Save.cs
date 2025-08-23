using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using KeepInventory.Helper;
using KeepInventory.Managers;

using UnityEngine;

namespace KeepInventory.Saves.V2
{
    public class Save
    {
        [JsonPropertyName("Version")]
        public readonly int Version = 2;

        [JsonIgnore]
        private string _name;

        [JsonPropertyName("Name")]
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

        [JsonPropertyName("ID")]
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
        public UnityEngine.Color DrawingColor = UnityEngine.Color.white;

        [JsonPropertyName("Color")]
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

        [JsonPropertyName("LightAmmo")]
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

        [JsonPropertyName("MediumAmmo")]
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

        [JsonPropertyName("HeavyAmmo")]
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

        private List<SaveSlot> _inventorySlots;

        [JsonPropertyName("InventorySlots")]
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

        [JsonConstructor]
        public Save(int Version, string Name, string ID, float[] Color, int LightAmmo, int MediumAmmo, int HeavyAmmo, List<SaveSlot> InventorySlots)
        {
            if (Version != 2)
            {
                throw new ArgumentException($"The V2 save object is not made for saves with version '{Version}'");
            }
            this.Version = Version;
            this._name = Name;
            this._id = ID;
            this.Color = Color;
            this._lightAmmo = LightAmmo;
            this._mediumAmmo = MediumAmmo;
            this._heavyAmmo = HeavyAmmo;
            this._inventorySlots = InventorySlots;

            if (string.IsNullOrWhiteSpace(_id))
                throw new Exception("ID cannot be null or empty");
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

        internal bool Saving = false;

        public void SaveToFile(bool printMessage = true)
        {
            if (!string.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath))
            {
                Saving = true;
                if (printMessage) Core.Logger.Msg($"Saving '{ID}' to file...");
                try
                {
                    var serialized = JsonSerializer.Serialize<Save>(this, SaveManager.SerializeOptions);
                    var file = File.Create(FilePath);
                    using var writer = new StreamWriter(file) { AutoFlush = true };
                    file.Position = 0;
                    writer.Write(serialized);
                    writer.DisposeAsync().AsTask().ContinueWith((task) =>
                     {
                         if (task.IsCompletedSuccessfully)
                         {
                             if (printMessage) Core.Logger.Msg($"Saved '{ID}' to file successfully!");
                         }
                         else
                         {
                             if (printMessage) Core.Logger.Error($"Failed to save '{ID}' to file", task.Exception);
                         }
                         Saving = false;
                     });
                }
                catch (Exception ex)
                {
                    if (printMessage) Core.Logger.Error($"Failed to save '{ID}' to file", ex);
                    Saving = false;
                    throw;
                }
            }
            else
            {
                if (printMessage) Core.Logger.Error($"Save '{ID}' does not have a file set or it doesn't exist!");
                throw new FileNotFoundException("Save does not have a file!");
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

        internal void Update(string path)
        {
            SaveManager.IgnoredFilePaths.Add(FilePath);
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException($"Save file at '{path}' could be found");
            var text = SaveManager.ReadAllTextUsedFile(path);
            if (string.IsNullOrWhiteSpace(text) || !SaveManager.IsJSON(text))
            {
                Core.Logger.Error($"A save file at '{path}' was changed and the content are no longer suitable for loading as a save. This means that the save at runtime will not be overwritten by new content");
                throw new Exception($"A save file at '{path}' was changed and the content are no longer suitable for loading as a save. This means that the save at runtime will not be overwritten by new content");
            }
            else
            {
                var save = JsonSerializer.Deserialize<Save>(text, SaveManager.SerializeOptions);
                if (save != null)
                {
                    Update(save);
                }
                else
                {
                    Core.Logger.Error($"A save file at '{path}' was changed and the content are no longer suitable for loading as a save. This means that the save at runtime will not be overwritten by new content");
                    throw new Exception($"A save file at '{path}' was changed and the content are no longer suitable for loading as a save. This means that the save at runtime will not be overwritten by new content");
                }
            }
        }

        public override string ToString()
           => this.Name;
    }
}