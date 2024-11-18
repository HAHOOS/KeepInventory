using System;
using System.Collections.Generic;
using System.Linq;

namespace KeepInventory
{
    /// <summary>
    /// Class that contains all barcodes for things originating from BONELAB
    /// </summary>
    public static class BONELABContentBarcodes
    {
        /// <summary>
        /// This dictionary contains barcodes that were referenced in the CommonBarcodes.cs file in the repository BoneLib which is under GNU General Public License v3.0.<br/>
        /// This code is modified to satisfy the program's needs, which is to quickly get all barcodes originating from BONELAB. All the barcodes that were made as a property in or under the CommonBarcodes class were added to this dictionary and given a BarcodeType value.
        /// <para>See <seealso href="https://github.com/yowchap/BoneLib/blob/main/LICENSE.md">LICENSE</seealso> for the full License.</para>
        /// </summary>
        public static readonly List<BONELABContent> all =
        [

            #region Avatars

            new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Avatar.Heavy", "Heavy", BarcodeType.AVATAR),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Avatar.Fast", "Fast", BarcodeType.AVATAR),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Avatar.CharFurv4GB", "Short", BarcodeType.AVATAR),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Avatar.CharTallv4", "Tall", BarcodeType.AVATAR),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Avatar.Strong", "Strong", BarcodeType.AVATAR),
        new BONELABContent("SLZ.BONELAB.Content.Avatar.Anime", "Light", BarcodeType.AVATAR),
        new BONELABContent("SLZ.BONELAB.Content.Avatar.CharJimmy", "Jimmy", BarcodeType.AVATAR),
        new BONELABContent("SLZ.BONELAB.Content.Avatar.FordBW", "FordBW", BarcodeType.AVATAR),
        new BONELABContent("SLZ.BONELAB.Content.Avatar.CharFord", "FordBL", BarcodeType.AVATAR),
        new BONELABContent("SLZ.BONELAB.Core.Avatar.PeasantFemaleA", "PeasantFemaleA", BarcodeType.AVATAR),
        new BONELABContent("c3534c5a-10bf-48e9-beca-4ca850656173", "PeasantFemaleB", BarcodeType.AVATAR),
        new BONELABContent("c3534c5a-2236-4ce5-9385-34a850656173", "PeasantFemaleC", BarcodeType.AVATAR),
        new BONELABContent("c3534c5a-87a3-48b2-87cd-f0a850656173", "PeasantMaleA", BarcodeType.AVATAR),
        new BONELABContent("c3534c5a-f12c-44ef-b953-b8a850656173", "PeasantMaleB", BarcodeType.AVATAR),
        new BONELABContent("c3534c5a-3763-4ddf-bd86-6ca850656173", "PeasantMaleC", BarcodeType.AVATAR),
        new BONELABContent("SLZ.BONELAB.Content.Avatar.Nullbody", "Nullbody", BarcodeType.AVATAR),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Avatar.Charskeleton", "Skeleton", BarcodeType.AVATAR),
        new BONELABContent("c3534c5a-d388-4945-b4ff-9c7a53656375", "SecurityGuard", BarcodeType.AVATAR),
        new BONELABContent("SLZ.BONELAB.Content.Avatar.DogDuckSeason", "DuckSeasonDog", BarcodeType.AVATAR),
        new BONELABContent("c3534c5a-94b2-40a4-912a-24a8506f6c79", "PolyBlank", BarcodeType.AVATAR),
        new BONELABContent("SLZ.BONELAB.NoBuild.Avatar.PolyDebugger", "PolyDebugger", BarcodeType.AVATAR),

            #endregion Avatars

            #region Levels

            // Story
            new BONELABContent("c2534c5a-80e1-4a29-93ca-f3254d656e75", "MainMenu", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-4197-4879-8cd3-4a695363656e", "Descent", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-6b79-40ec-8e98-e58c5363656e", "BLHub", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-56a6-40ab-a8ce-23074c657665", "LongRun", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-54df-470b-baaf-741f4c657665", "MineDive", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-7601-4443-bdfe-7f235363656e", "BigAnomaly", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.Content.Level.LevelStreetPunch", "StreetPuncher", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.Content.Level.SprintBridge04", "SprintBridge", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.Content.Level.SceneMagmaGate", "MagmaGate", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.Content.Level.MoonBase", "Moonbase", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.Content.Level.LevelKartRace", "MonogonMotorway", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-c056-4883-ac79-e051426f6964", "PillarClimb", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.Content.Level.LevelBigAnomalyB", "BigAnomaly2", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-db71-49cf-b694-24584c657665", "Ascent", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.Content.Level.LevelOutro", "Home", BarcodeType.LEVEL),
        new BONELABContent("fa534c5a868247138f50c62e424c4144.Level.VoidG114", "VoidG114", BarcodeType.LEVEL),
        // Sandbox
        new BONELABContent("c2534c5a-61b3-4f97-9059-79155363656e", "Baseline", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-2c4c-4b44-b076-203b5363656e", "Tuscany", BarcodeType.LEVEL),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Level.LevelMuseumBasement", "MuseumBasement", BarcodeType.LEVEL),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Level.LevelHalfwayPark", "HalfwayPark", BarcodeType.LEVEL),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Level.LevelGunRange", "GunRange", BarcodeType.LEVEL),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Level.LevelHoloChamber", "Holochamber", BarcodeType.LEVEL),
        // Experimental
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Level.LevelKartBowling", "BigBoneBowling", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.Content.Level.LevelMirror", "Mirror", BarcodeType.LEVEL),
        // Tac Trial
        new BONELABContent("c2534c5a-4f3b-480e-ad2f-69175363656e", "NeonTrial", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-de61-4df9-8f6c-416954726547", "DropPit", BarcodeType.LEVEL),
        // Arena
        new BONELABContent("c2534c5a-c180-40e0-b2b7-325c5363656e", "TunnelTipper", BarcodeType.LEVEL),
        new BONELABContent("fa534c5a868247138f50c62e424c4144.Level.LevelArenaMin", "FantasyArena", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-162f-4661-a04d-975d5363656e", "ContainerYard", BarcodeType.LEVEL),
        // Parkour
        new BONELABContent("c2534c5a-5c2f-4eef-a851-66214c657665", "DungeonWarrior", BarcodeType.LEVEL),
        new BONELABContent("c2534c5a-c6ac-48b4-9c5f-b5cd5363656e", "Rooftops", BarcodeType.LEVEL),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Level.SceneparkourDistrictLogic", "NeonParkour", BarcodeType.LEVEL),
        // Load levels
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Level.DefaultLoad", "LoadDefault", BarcodeType.LEVEL),
        new BONELABContent("SLZ.BONELAB.CORE.Level.LevelModLevelLoad", "LoadMod", BarcodeType.LEVEL),

            #endregion Levels

            #region NPCs

             new BONELABContent("c1534c5a-4583-48b5-ac3f-eb9543726162", "Crablet", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-af28-46cb-84c1-012343726162", "CrabletPlus", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCCultist", "Cultist", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-2ab7-46fe-b0d6-7495466f7264", "EarlyExitZombie", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-3fd8-4d50-9eaf-0695466f7264", "Ford", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-481a-45d8-8bc1-d810466f7264", "FordVRJunkie", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-d82d-4f65-89fd-a4954e756c6c", "Nullbody", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-0e54-4d5b-bdb8-31754e756c6c", "NullbodyAgent", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-2775-4009-9447-22d94e756c6c", "NullbodyCorrupted", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-ef15-44c0-88ae-aebc4e756c6c", "Nullrat", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-7c6d-4f53-b61c-e4024f6d6e69", "OmniProjectorHazmat", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-0df5-495d-8421-75834f6d6e69", "OmniTurret", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCPeasantFemL", "PeasantFemaleA", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCPeasantFemM", "PeasantFemaleB", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCPeasantFemS", "PeasantFemaleC", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCPeasantMaleL", "PeasantMaleA", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCPeasantMaleM", "PeasantMaleB", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCPeasantMaleS", "PeasantMaleC", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCPeasantNull", "PeasantNull", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.NPCSecurityGuard", "SecurityGuard", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-de57-4aa0-9021-5832536b656c", "Skeleton", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-bd53-469d-97f1-165e4e504353", "SkeletonFireMage", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-a750-44ca-9730-b487536b656c", "SkeletonSteel", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-290e-4d56-9b8e-ad95566f6964", "VoidTurret", BarcodeType.SPAWNABLE),

            #endregion NPCs

            #region Guns

            // Pistols
            new BONELABContent("c1534c5a-fcfc-4f43-8fb0-d29531393131", "M1911", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-2a4f-481f-8542-cc9545646572", "Eder22", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.HandgunEder22training", "RedEder22", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.CORE.Spawnable.GunEHG", "eHGBlaster", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-9f55-4c56-ae23-d33b47727562", "Gruber", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-aade-4fa1-8f4b-d4c547756e4d", "M9", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-bcb7-4f02-a4f5-da9550333530", "P350", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-50cf-4500-83d5-c0b447756e50", "PT8Alaris", BarcodeType.SPAWNABLE),
        new BONELABContent("fa534c5a868247138f50c62e424c4144.Spawnable.Stapler", "Stapler", BarcodeType.SPAWNABLE),
        // Rifles
        new BONELABContent("c1534c5a-a6b5-4177-beb8-04d947756e41", "AKM", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.RifleM1Garand", "Garand", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-ea97-495d-b0bf-ac955269666c", "M16ACOG", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-cc53-4aac-b842-46955269666c", "M16Holosight", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-9112-49e5-b022-9c955269666c", "M16IronSights", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-4e5b-4fb7-be33-08955269666c", "M16LaserForegrip", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.RifleMK18HoloForegrip", "MK18HoloForegrip", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-c061-4c5c-a5e2-3d955269666c", "MK18Holosight", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-f3b6-4161-a525-a8955269666c", "MK18IronSights", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-ec8e-418a-a545-cf955269666c", "MK18LaserForegrip", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-5c2b-4cb4-ae31-e7955269666c", "MK18Naked", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-4b3e-4288-849c-ce955269666c", "MK18Sabrelake", BarcodeType.SPAWNABLE),
        // Shotguns
        new BONELABContent("c1534c5a-2774-48db-84fd-778447756e46", "FAB", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-7f05-402f-9320-609647756e35", "M590A1", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-e0b5-4d4b-9df3-567147756e4d", "M4", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-571f-43dc-8bc6-8e9553686f74", "DuckSeasonShotgun", BarcodeType.SPAWNABLE),
        // SMGs
        new BONELABContent("c1534c5a-d00c-4aa8-adfd-3495534d474d", "MP5", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-3e35-4aeb-b1ec-4a95534d474d", "MP5KFlashlight", BarcodeType.SPAWNABLE),
        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Spawnable.MP5KRedDotSight", "MP5KHolosight", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-9f54-4f32-b8b9-f295534d474d", "MP5KIronsights", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-ccfa-4d99-af97-5e95534d474d", "MP5KLaser", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-6670-4ac2-a82a-a595534d474d", "MP5KSabrelake", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-04d7-41a0-b7b8-5a95534d4750", "PDRC", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-40e5-40e0-8139-194347756e55", "UMP", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-8d03-42de-93c7-f595534d4755", "UZI", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-4c47-428d-b5a5-b05747756e56", "Vector", BarcodeType.SPAWNABLE),

        #endregion Guns

            #region Melee

        // Blunt
        new BONELABContent("c1534c5a-e962-46dd-b1ef-f39542617262", "BarbedBat", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-6441-40aa-a070-909542617365", "BaseballBat", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-837c-43ca-b4b5-33d842617365", "Baseball", BarcodeType.SPAWNABLE),
        new BONELABContent("fa534c5a868247138f50c62e424c4144.Spawnable.Baton", "Baton", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-0c8a-4b82-9f8b-7a9543726f77", "Crowbar", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.ElectricGuitar", "ElectricGuitar", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-d0e9-4d53-9218-e76446727969", "FryingPan", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-8597-4ffe-892e-b995476f6c66", "GolfClub", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-11d0-4632-b36e-fa9548616d6d", "Hammer", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-dfa6-466d-9ab7-bf9548616e64", "HandHammer", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-f6f9-4c96-b88e-91d74c656164", "LeadPipe", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-3d5c-4f9f-92fa-c24c4d656c65", "MorningStar", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-5d31-488d-b5b3-aa1c53686f76", "Shovel", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-1f5a-4993-bbc1-03be4d656c65", "Sledgehammer", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-f5a3-4204-a199-a1e14d656c65", "SpikedClub", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-d30c-4c18-9f5f-7cfe54726173", "TrashcanLid", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-6d15-47c7-9ad4-b04156696b69", "VikingShield", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-f6f3-46e2-aa51-67214d656c65", "Warhammer", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-02e7-43cf-bc8d-26955772656e", "Wrench", BarcodeType.SPAWNABLE),
        // Blade
        new BONELABContent("c1534c5a-6d6b-4414-a9f2-af034d656c65", "AxeDouble", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-4774-460f-a814-149541786546", "AxeFirefighter", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-0ba6-4876-be9c-216741786548", "AxeHorror", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-d086-4e27-918d-ee9542617374", "BastardSword", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-8036-440a-8830-b99543686566", "ChefKnife", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-3481-4025-9d28-2e95436c6561", "Cleaver", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-1fb8-477c-afbe-2a95436f6d62", "CombatKnife", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-d3fc-4987-a93d-d79544616767", "Dagger", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-53ae-487e-956f-707148616c66", "HalfSword", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-d605-4f85-870d-f68848617463", "Hatchet", BarcodeType.SPAWNABLE),
        new BONELABContent("SLZ.BONELAB.Content.Spawnable.MeleeIceAxe", "IceAxe", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-282b-4430-b009-58954b617461", "Katana", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-e606-4a82-878c-652f4b617461", "Katar", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-f0d1-40b6-9f9b-c19544616767", "Kunai", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-a767-4a58-b3ef-26064d616368", "Machete", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-e75f-4ded-aa5a-a27b4178655f", "NorseAxe", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-f943-42a8-a994-6e955069636b", "Pickaxe", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-a97f-4bff-b512-e44d53706561", "Spear", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-b59c-4790-9b09-499553776f72", "SwordClaymore", BarcodeType.SPAWNABLE),

        #endregion Melee

            #region Misc

        new BONELABContent("fa534c5a83ee4ec6bd641fec424c4142.Spawnable.VehicleGokart", "GoKart", BarcodeType.SPAWNABLE),
        new BONELABContent("fa534c5a868247138f50c62e424c4144.Spawnable.OmniWay", "OmniWay", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-5747-42a2-bd08-ab3b47616467", "SpawnGun", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-6b38-438a-a324-d7e147616467", "NimbusGun", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-e777-4d15-b0c1-3195426f6172", "BoardGun", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-e963-4a7c-8c7e-1195546f7942", "BalloonGun", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-3813-49d6-a98c-f595436f6e73", "Constrainer", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-c6a8-45d0-aaa2-2c954465764d", "DevManipulator", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-87ce-436d-b00c-ef9547726176", "GravityCup", BarcodeType.SPAWNABLE),
        new BONELABContent("c1534c5a-a1c3-437b-85ac-e09547726176", "GravityPlate", BarcodeType.SPAWNABLE),
        new BONELABContent(" c1534c5a-cebf-42cc-be3a-4595506f7765", "PowerPuncher", BarcodeType.SPAWNABLE),

        #endregion Misc

        ];

        /// <summary>
        /// Check if a barcode is from BONELAB
        /// </summary>
        /// <param name="barcode">The barcode to check</param>
        /// <returns>A boolean value indicating if a barcode is originating from BONELAB</returns>
        public static bool Contains(string barcode)
        {
            return all.Any(x => x.Barcode == barcode);
        }
    }

    /// <summary>
    /// Class that stores information regarding an object originating from BONELAB
    /// </summary>
    public class BONELABContent
    {
        /// <summary>
        /// The barcode
        /// </summary>
        public string Barcode;

        /// <summary>
        /// The name that's used f
        /// </summary>
        public string Title;

        /// <summary>
        /// <inheritdoc cref="BarcodeType"/>
        /// </summary>
        public BarcodeType Type;

        /// <summary>
        /// Create a new <see cref="BONELABContent"/> object
        /// </summary>
        /// <param name="barcode"><inheritdoc cref="Barcode"/></param>
        /// <param name="title"><inheritdoc cref="Title"/></param>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        public BONELABContent(string barcode, string title, BarcodeType type)
        {
            ArgumentNullException.ThrowIfNull(barcode, nameof(barcode));
            ArgumentNullException.ThrowIfNull(title, nameof(title));
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            this.Barcode = barcode;
            this.Title = title;
            this.Type = type;
        }
    }

    /// <summary>
    /// Type of barcode, which is what it references, such as: a spawnable, a level or an avatar
    /// </summary>
    public enum BarcodeType
    {
        /// <summary>
        /// Barcode references a Level
        /// </summary>
        LEVEL,

        /// <summary>
        /// Barcode references a spawnable
        /// </summary>
        SPAWNABLE,

        /// <summary>
        /// Barcode references an avatar
        /// </summary>
        AVATAR,
    }
}