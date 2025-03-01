using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using KeepInventory.Helper;

using Color = System.Drawing.Color;

namespace KeepInventory.Saves.V2
{
    /// <summary>
    /// Class that gets serialized or deserialized, holds all saved info about inventory, ammo etc.
    /// </summary>
    public class Save
    {
        /// <summary>
        /// The version of the save
        /// </summary>
        [JsonPropertyName("Version")]
        public readonly int Version = 2;

        [JsonIgnore]
        private string _name;

        /// <summary>
        /// Name identifying the save
        /// </summary>
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

        /// <summary>
        /// Identifier of the save
        /// </summary>
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

        /// <summary>
        /// Color of the text in the menu
        /// </summary>
        [JsonIgnore]
        public Color DrawingColor = System.Drawing.Color.FromArgb(255, 255, 255);

        /// <summary>
        /// HEX Color of the text in the menu
        /// </summary>
        [JsonPropertyName("Color")]
        public string Color
        {
            get => $"{DrawingColor.R:X2}{DrawingColor.G:X2}{DrawingColor.B:X2}";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    DrawingColor = System.Drawing.Color.White;
                }
                else
                {
                    Color translated;
                    var old = DrawingColor;
                    try
                    {
                        translated = ColorTranslator.FromHtml(value.StartsWith("#") ? value : $"#{value}");
                    }
                    catch (Exception ex)
                    {
                        Core.Logger.Error($"Failed to convert HEX color to Drawing Color, defaulting to white, exception:\n{ex}");
                        translated = System.Drawing.Color.White;
                    }
                    DrawingColor = translated;
                    OnPropertyChanged?.Invoke(nameof(Color), old, DrawingColor);
                }
            }
        }

        /// <summary>
        /// Will the saves properties be automatically updated when changes to the file in <see cref="FilePath"/> are done
        /// </summary>
        [JsonIgnore]
        public bool IsFileWatcherEnabled { get; set; } = true;

        [JsonIgnore]
        private string _filePath;

        /// <summary>
        /// Path to the save file
        /// </summary>
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
        private bool _isHidden;

        /// <summary>
        /// Should the save be hidden from BoneMenu
        /// </summary>
        [JsonPropertyName("IsHidden")]
        public bool IsHidden
        {
            get { return _isHidden; }
            set
            {
                var old = _isHidden;
                _isHidden = value;
                OnPropertyChanged?.Invoke(nameof(IsHidden), old, value);
            }
        }

        [JsonIgnore]
        private bool _canBeOverwrittenByPlayer;

        /// <summary>
        /// Can the save be overwritten by player using the BoneMenu
        /// </summary>
        [JsonPropertyName("CanBeOverwrittenByPlayer")]
        public bool CanBeOverwrittenByPlayer
        {
            get { return _canBeOverwrittenByPlayer; }
            set
            {
                var old = _canBeOverwrittenByPlayer;
                _canBeOverwrittenByPlayer = value;
                OnPropertyChanged?.Invoke(nameof(CanBeOverwrittenByPlayer), old, value);
            }
        }

        [JsonIgnore]
        private int _lightAmmo = -1;

        /// <summary>
        /// The amount of light ammo left
        /// </summary>
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

        /// <summary>
        /// The amount of medium ammo left
        /// </summary>
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

        /// <summary>
        /// The amount of heavy ammo left
        /// </summary>
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

        /// <summary>
        /// List of all slots and the spawnables stored in them
        /// </summary>
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

        private void PostCreate()
        {
            // Currently empty
        }

        /// <summary>
        /// Create new instance of <see cref="Save"/>
        /// </summary>
        public Save()
        {
            PostCreate();
        }

        /// <summary>
        /// Create new instance of <see cref="Save"/> from an old one
        /// </summary>
        public Save(Save old)
        {
            _name = old.Name;
            _id = old.ID;
            Color = old.Color;
            _canBeOverwrittenByPlayer = old.CanBeOverwrittenByPlayer;
            _isHidden = old.IsHidden;
            _lightAmmo = old.LightAmmo;
            _mediumAmmo = old.MediumAmmo;
            _heavyAmmo = old.HeavyAmmo;
            _inventorySlots = [.. old.InventorySlots];
            PostCreate();
        }

        /// <summary>
        /// Create new instance of <see cref="Save"/> from an old one
        /// </summary>
        [JsonConstructor]
        public Save(int Version, string Name, string ID, string Color, bool IsHidden, bool CanBeOverwrittenByPlayer, int LightAmmo, int MediumAmmo, int HeavyAmmo, List<SaveSlot> InventorySlots)
        {
            if (Version != 2)
            {
                throw new ArgumentException($"The V2 save object is not made for saves with version '{Version}'");
            }
            this.Version = Version;
            this._name = Name;
            this._id = ID;
            this.Color = Color;
            this._isHidden = IsHidden;
            this._canBeOverwrittenByPlayer = CanBeOverwrittenByPlayer;
            this._lightAmmo = LightAmmo;
            this._mediumAmmo = MediumAmmo;
            this._heavyAmmo = HeavyAmmo;
            this._inventorySlots = InventorySlots;

            if (string.IsNullOrWhiteSpace(_id))
                throw new Exception("ID cannot be null or empty");

            PostCreate();
        }

        /// <summary>
        /// Create new instance of <see cref="Save"/> from a V1
        /// </summary>
        public Save(string id, string name, Color color, bool canBeOverwritten, bool isHidden, V1.Save v1save)
        {
            _id = id;
            _name = name;
            DrawingColor = color;
            _canBeOverwrittenByPlayer = canBeOverwritten;
            _isHidden = isHidden;
            _lightAmmo = v1save.LightAmmo;
            _mediumAmmo = v1save.MediumAmmo;
            _heavyAmmo = v1save.HeavyAmmo;
            List<SaveSlot> _new = [];
            v1save.InventorySlots?.ForEach(x => _new.Add(new SaveSlot(x)));
            _inventorySlots = _new;
            PostCreate();
        }

        /// <summary>
        /// Create new instance of <see cref="Save"/> from a V0
        /// </summary>
        public Save(string id, string name, Color color, bool canBeOverwritten, bool isHidden, V0.Save v0save)
        {
            _id = id;
            _name = name;
            DrawingColor = color;
            _canBeOverwrittenByPlayer = canBeOverwritten;
            _isHidden = isHidden;
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
            PostCreate();
        }

        /// <summary>
        /// Triggered when a property changes
        /// </summary>
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
            if (this.CanBeOverwrittenByPlayer != save.CanBeOverwrittenByPlayer) this.CanBeOverwrittenByPlayer = save.CanBeOverwrittenByPlayer;
            if (this.IsHidden != save.IsHidden) this.IsHidden = save.IsHidden;
            if (this.Name != save.Name) this.Name = save.Name;
            if (this.Color != save.Color) this.Color = save.Color;
            if (this.InventorySlots != save.InventorySlots) this.InventorySlots = save.InventorySlots;
            if (this.HeavyAmmo != save.HeavyAmmo) this.HeavyAmmo = save.HeavyAmmo;
            if (this.MediumAmmo != save.MediumAmmo) this.MediumAmmo = save.MediumAmmo;
            if (this.LightAmmo != save.LightAmmo) this.LightAmmo = save.LightAmmo;
        }

        internal bool Saving = false;

        /// <summary>
        /// Saves the save to its file
        /// </summary>
        /// <param name="printMessage">Should there be logs in the console for saving</param>
        /// <exception cref="FileNotFoundException">The file was not found or the path was empty</exception>
        public void SaveToFile(bool printMessage = true)
        {
            if (!string.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath))
            {
                Saving = true;
                SaveManager.IgnoredFilePaths.Add(FilePath);
                if (printMessage) Core.Logger.Msg($"Saving '{ID}' to file...");
                try
                {
                    var serialized = JsonSerializer.Serialize<Save>(this, SaveManager.SerializeOptions);
                    File.WriteAllText(FilePath, serialized);
                    if (printMessage) Core.Logger.Msg($"Saved '{ID}' to file successfully!");
                    Saving = false;
                    SaveManager.IgnoredFilePaths.Remove(FilePath);
                }
                catch (Exception ex)
                {
                    if (printMessage) Core.Logger.Error($"Failed to save '{ID}' to file, exception:\n{ex}");
                    Saving = false;
                    SaveManager.IgnoredFilePaths.Remove(FilePath);
                    throw;
                }
            }
            else
            {
                if (printMessage) Core.Logger.Error($"Save '{ID}' does not have a file set or it doesn't exist!");
                throw new FileNotFoundException("Save does not have a file!");
            }
        }

        /// <summary>
        /// Saves the save to its file without throwing exceptions
        /// </summary>
        /// <param name="printMessage">Should there be logs in the console for saving</param>
        /// <returns><see langword="true"/> if saved successfully, otherwise <see langword="false"/></returns>
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
            if (!Saving) SaveManager.IgnoredFilePaths.Remove(FilePath);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
           => this.Name;
    }
}