<div align="center">
<h1 align="center">Keep Inventory</h1>
<h3>A BONELAB code mod</h3>
</div>
<img alt="Dynamic JSON Badge" src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fv1%2Fpackage-metrics%2FHAHOOS%2FKeepInventory%2F&query=%24.downloads&style=for-the-badge&label=THUNDERSTORE%20DOWNLOADS">



**WARNING: This mod was not tested for Fusion and might cause issues**

A BONELAB Mod that lets you keep items when switching between levels. This is my first ever code mod so please expect bugs, if a bug occurs, I recommend [creating an issue](https://github.com/HAHOOS/KeepInventory/issues).

This mod is inspired by [Inventory Persistence](https://thunderstore.io/c/bonelab/p/SilverWare/InventoryPersistence/) which in my case did not work for me so I've decided to remake it myself.
If InventoryPersistence will be updated to Patch 5, this mod will be deprecated

The "Box" asset used in the logo is under the [MIT License](https://github.com/twbs/icons/blob/main/LICENSE)

## Settings

### Save Items
If enabled, saves items when leaving a level and loads saved items when entering a level

### Save Ammo
If enabled, saves ammo when leaving a level and loads saved ammo when entering a level

### Persistent Save
If enabled, saves the inventory to a file so that it can be used when closing and then opening the game

### Blacklist Level
When pressed, if level is not blacklisted, it will blacklist the level which means that your inventory will not save and saved inventory will not load, if level is blacklisted, it will remove the level from the blacklist
The main menu map (MainMenu and VoidG114), campaign, arenas and pit trials are blacklisted by default
