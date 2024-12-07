<div align="center">
<img align="center" src="https://raw.githubusercontent.com/HAHOOS/KeepInventory/refs/heads/master/logo.png"/>
<h1 align="center">Keep Inventory</h1>
<a align="center" href="https://thunderstore.io/c/bonelab/p/HAHOOS/KeepInventory/"><img alt="Dynamic JSON Badge" src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fv1%2Fpackage-metrics%2FHAHOOS%2FKeepInventory%2F&query=%24.downloads&style=for-the-badge&label=TS%20DOWNLOADS"></a>
<a align="center" href="https://github.com/HAHOOS/KeepInventory/blob/master/LICENSE"><img alt="GitHub License" src="https://img.shields.io/github/license/HAHOOS/KeepInventory?style=for-the-badge"></a>
</div>



## What is this?

A BONELAB code mod that lets you keep your inventory when switching between levels that do not save your inventory or when rejoining the game. It features:
- **Full [Fusion](https://thunderstore.io/c/bonelab/p/Lakatrazz/Fusion/) Support!**
- Saving data about **guns** and loading them!
- Customizability, such as disabling inventory loading on level load, or saving/loading ammo
- And much more!

**WARNING!**
This mod might remove campaign data regarding the inventory while playing through campaign levels. You can change this behaviour at any time by going to the BoneMenu > HAHOOS > KeepInventory > Other and disabling `Remove Initial Inventory From Save`.

If you find any bugs, I recommend creating an [issue](https://github.com/HAHOOS/KeepInventory/issues). This will really help the development of the mod

The "Box" asset used in the logo is under the [MIT License](https://github.com/twbs/icons/blob/main/LICENSE)

## Possible bugs

- Saved gun with the slide locked might not load with it locked
- Guns not being put in inventory slots for new players in Fusion (I have asked Lakatrazz and he said it's a problem with Fusion)

## Settings

The mod can be changed to your liking, here is a list of all available settings:

### Saving

#### Save Items `DEFAULT: true / Enabled`
*In MelonPreferences: ItemSaving*<br/>
If true, will save and load items in inventory

#### Save Ammo  `DEFAULT: true / Enabled`
*In MelonPreferences: AmmoSaving*<br/>
If true, will save and load ammo in inventory

#### Save Gun Data  `DEFAULT: true / Enabled`
*In MelonPreferences: SaveGunData*<br/>
If true, will save and load data about guns stored in slots, info such as rounds left etc.

#### Persistent Save  `DEFAULT: true / Enabled`
*In MelonPreferences: PersistentSave*<br/>
If true, will save and load inventory in a KeepInventory_Save.cfg file to be used between sessions

### Events

#### Save on Level Unload`DEFAULT: true / Enabled`
*In MelonPreferences: SaveOnLevelUnload*<br/>
If true, during level unload, the inventory will be automatically saved

#### Load on Level Load  `DEFAULT: true / Enabled`
*In MelonPreferences: LoadOnLevelLoad*<br/>
If true, the saved inventory will be automatically loaded when you get loaded into a level thats not blacklisted

#### Automatically Save To File  `DEFAULT: true / Enabled`
*In MelonPreferences: AutomaticallySaveToFile*<br/>
If true, the inventory will be automatically saved to a save file if 'Persistent Save' is turned on when the game is quitting

### Blacklist

#### Blacklist BONELAB Levels  `DEFAULT: true / Enabled`
*In MelonPreferences: BlacklistBONELABLevels*<br/>
If true, most of the BONELAB levels (except VoidG114 and BONELAB Hub) will be blacklisted from saving/loading inventory

#### BlacklistedLevels  `DEFAULT: [] (empty list)` 
*Only seen in MelonPreferences*<br/>
List of levels that will not save/load inventory

### Other

#### Show Notifications  `DEFAULT: true / Enabled`
*In MelonPreferences: ShowNotifications*<br/>
If true, notifications will be shown in-game regarding errors or other things

#### Fusion Support `DEFAULT: true / Enabled`
*In MelonPreferences: FusionSupport*<br/>
If true, the mod will work with Fusion. If fusion is detected, you are connected to a server and this setting is turned off, the inventory will not be loaded

#### Remove Initial Inventory From Save `DEFAULT: true / Enabled`
*In MelonPreferences: RemoveInitialInventory* <br/>
If true, the mod will remove initial inventory found in save data in a loaded inventory
