using MelonLoader;
using BoneLib;
using BoneLib.BoneMenu;
using UnityEngine;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;

[assembly: MelonInfo(typeof(global::HAHOOS.KeepInventory.Main), "Keep Inventory", "1.0.0", "HAHOOS")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
[assembly: MelonAuthorColor(0,255,165,0)]
[assembly: MelonColor(0, 255, 72, 59)]

namespace HAHOOS.KeepInventory
{
    public class Main : MelonMod
    {

        public Dictionary<string, Barcode> slots = new Dictionary<string, Barcode>();
        private bool IsDebug = true;

        public override void OnInitializeMelon()
        {
           LoggerInstance.Msg("Setting up KeepInventory");
           SetupMenu();

           Hooking.OnLevelLoaded += LevelLoadedEvent;
           Hooking.OnLevelUnloaded += LevelUnloadedEvent;

        }

        private void LevelUnloadedEvent()
        {
            if (Settings.Enabled)
            {
                slots.Clear();
                LoggerInstance.Msg("Saving inventory...");
                if (Player.RigManager == null)
                {
                     LoggerInstance.Msg("RigManager does not exist");
                    return;
                }
                if (Player.RigManager.inventory == null)
                {
                    LoggerInstance.Msg("Inventory does not exist");
                    return;
                }
                foreach (var item in Player.RigManager.inventory.bodySlots)
                {
                    if (item.inventorySlotReceiver != null)
                    {
                        if (item.inventorySlotReceiver._weaponHost != null)
                        {
                            var poolee = item.inventorySlotReceiver._weaponHost.GetTransform().GetComponent<Poolee>();
                            if (poolee != null)
                            {
                                var barcode = poolee.SpawnableCrate.Barcode;
                                slots.Add(item.name, barcode);
                            }
                        }

                    }
                }
                if (IsDebug) LoggerInstance.Msg("Successfully saved inventory");
            }
        }

        private void LevelLoadedEvent(LevelInfo obj)
        {
            if (Settings.Enabled)
            {
                 LoggerInstance.Msg("Loading inventory...");
                if (slots.Count >= 1)
                {
                    var list = Player.RigManager.inventory.bodySlots.ToList();
                    foreach (var item in slots)
                    {
                        var slot = list.Find((slot) => slot.name == item.Key);
                        if (slot != null){
                            if(slot.inventorySlotReceiver != null)
                            {
                                slot.inventorySlotReceiver.SpawnInSlotAsync(item.Value);
                            }
                        }
                    }
                    LoggerInstance.Msg(" Loaded inventory");
                }
                else
                {
                    LoggerInstance.Msg("Saved Inventory not found");
                }
            }
            else
            {
                if (IsDebug) LoggerInstance.Msg("[DEBUG] Saving/Loading Inventory has been disabled, check the mod settings to enable again");
            }
        }

        public void SetupMenu()
        {
            var mainPage = Page.Root.CreatePage("HAHOOS", Color.white);
            var modPage = mainPage.CreatePage("KeepInventory", new Color(255, 72, 59));
            modPage.CreateBool("Enabled", Color.green, Settings.Enabled, (value)=>Settings.Enabled=value);
        }
    }
}
