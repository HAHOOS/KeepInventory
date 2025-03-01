using System;

using BoneLib;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;

using UnityEngine;

using KeepInventory.Saves.V2;
using System.Collections.Generic;
using System.Linq;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.RPC;
using MelonLoader.Pastel;
using System.Threading.Tasks;

namespace KeepInventory.Utilities
{
	/// <summary>
	/// Class that holds most methods for Fusion
	/// </summary>
	public static class Fusion
	{
		/// <summary>
		/// Boolean value indicating if user is connected to a server
		/// </summary>
		public static bool IsConnected
		{
			get
			{
				if (Core.HasFusion) return Internal_IsConnected();
				else return false;
			}
		}

		/// <summary>
		/// Setup the Fusion Support Library
		/// </summary>
		internal static void SetupFusionLibrary()
		{
			Core.Logger.Msg("Setting up the library");
			try
			{
				KeepInventory.Fusion.FusionModule.Setup(Core.Logger);
				KeepInventory.Fusion.FusionModule.LoadModule();
				KeepInventory.Fusion.ShareManager.Setup();
			}
			catch (Exception ex)
			{
				Core.FailedFLLoad = true;
				Core.Logger.Error($"An unexpected error has occurred while setting up and/or loading the fusion module, exception:\n{ex}");
			}
		}

		internal static void Setup()
		{
			KeepInventory.Fusion.ShareManager.OnShared += (save, sender) =>
			{
				if (LabFusion.Entities.NetworkPlayerManager.TryGetPlayer(sender, out LabFusion.Entities.NetworkPlayer player))
				{
					if (!SaveManager.Saves.Any(x => x.ID == save.ID))
					{
						LabFusion.Utilities.FusionNotifier.Send(new LabFusion.Utilities.FusionNotification()
						{
							Title = "KeepInventory | Save shared!",
							SaveToMenu = true,
							ShowPopup = true,
							Message = $"{player.Username} has shared a save with you called '<color=#{save.Color}>{save.Name}</color>'. Go to the LabFusion notifications menu, press accept to add save, decline will disregard this",
							PopupLength = 15f,
							Type = LabFusion.Utilities.NotificationType.INFORMATION,
							OnAccepted = () => SaveManager.RegisterSave(save),
							OnDeclined = () => Core.Logger.Msg("Save share ignored")
						});
					}
				}
			};
		}

		/// <summary>
		/// Check if the player is connected to a Fusion server without the Fusion Support Library
		/// </summary>
		/// <returns>A boolean value indicating whether or not is the player connected to a server</returns>
		internal static bool Internal_IsConnected()
		{
			return LabFusion.Network.NetworkInfo.HasServer;
		}

		internal static void Internal_ShareSave(byte smallId, Save save)
		{
			if (LabFusion.Entities.NetworkPlayerManager.TryGetPlayer(smallId, out LabFusion.Entities.NetworkPlayer plr))
				KeepInventory.Fusion.ShareManager.Share(save, plr.PlayerId);
			else
				throw new Exception($"Player with small ID {smallId} could not be found");
		}

		/// <summary>
		/// Share a save with a target player
		/// </summary>
		/// <param name="smallId">The small ID of the player to share with</param>
		/// <param name="save">The <see cref="Save"/> to share</param>
		public static void ShareSave(byte smallId, Save save)
		{
			if (Core.HasFusion && Core.IsFusionLibraryInitialized && IsConnected) Internal_ShareSave(smallId, save);
		}

		internal static IEnumerable<FusionPlayer> Internal_GetShareablePlayers()
		{
			var task = KeepInventory.Fusion.ShareManager.GetAllShareablePlayers();
			task.Wait();
			foreach (var player in LabFusion.Entities.NetworkPlayer.Players)
			{
				if (player != null && task.Result.Contains(player.PlayerId.SmallId))
					yield return new FusionPlayer(player.PlayerId.SmallId, player.PlayerId.LongId, player.Username);
			}
		}

		/// <summary>
		/// Gets all players that you can share a save with, if not connected to any or doesn't have Fusion, returns an empty list
		/// </summary>
		/// <returns></returns>
		public static List<FusionPlayer> GetShareablePlayers()
		{
			if (Core.HasFusion && Core.IsFusionLibraryInitialized && IsConnected) return [.. Internal_GetShareablePlayers()];
			else return [];
		}

		internal static IEnumerable<FusionPlayer> Internal_GetPlayers()
		{
			foreach (var player in LabFusion.Entities.NetworkPlayer.Players)
			{
				yield return new FusionPlayer(player.PlayerId.SmallId, player.PlayerId.LongId, player.Username);
			}
		}

		/// <summary>
		/// Get all players in the current lobby, if not connected to any or doesn't have Fusion, returns an empty list
		/// </summary>
		/// <returns></returns>
		public static List<FusionPlayer> GetPlayers()
		{
			if (Core.HasFusion && IsConnected) return [.. Internal_GetPlayers()];
			else return [];
		}

		internal static byte Internal_GetLocalPlayerSmallId()
		{
			return LabFusion.Player.PlayerIdManager.LocalSmallId;
		}

		/// <summary>
		/// Get the small ID of the local player, if not connected to any or doesn't have Fusion, returns 0
		/// </summary>
		/// <returns>The small ID of the local player</returns>
		public static byte GetLocalPlayerSmallId()
		{
			if (Core.HasFusion && IsConnected) return Internal_GetLocalPlayerSmallId();
			else return 0;
		}

		/// <summary>
		/// Find the <see cref="RigManager"/> for the local player that is connected to a server
		/// </summary>
		/// <returns>The <see cref="RigManager"/> of the local player</returns>
		internal static RigManager FindRigManager()
		{
			if (!IsConnected)
			{
				return Player.RigManager;
			}
			else
			{
				return LabFusion.Data.RigData.Refs.RigManager ?? Player.RigManager;
			}
		}

		private static Action RigCreatedEvent;

		/// <summary>
		/// Removes the <see cref="InventoryManager.SpawnSavedItems(Save)"/> method from the OnRigCreated event in the Fusion Support Library
		/// </summary>
		internal static void RemoveRigCreateEvent_FSL()
		{
			if (RigCreatedEvent != null)
			{
				KeepInventory.Fusion.FusionModule.OnRigCreated -= RigCreatedEvent;
				RigCreatedEvent = null;
			}
		}

		/// <summary>
		/// Removes the <see cref="InventoryManager.SpawnSavedItems(Save)"/> method from the OnRigCreated event in the Fusion Support Library
		/// </summary>
		internal static void RemoveRigCreateEvent()
		{
			if (Core.HasFusion && Core.IsFusionLibraryInitialized) RemoveRigCreateEvent_FSL();
		}

		/// <summary>
		/// Spawn the saved items, run when Fusion is detected
		/// <para>This is separate to avoid errors if Fusion Support Library is not loaded</para>
		/// </summary>
		internal static void SpawnSavedItems_FSL(Save save)
		{
			if (Core.FindRigManager() == null)
			{
				Core.Logger.Msg("Rig not found, awaiting");

				void _event() => InventoryManager.SpawnSavedItems(save);
				RigCreatedEvent = _event;
				KeepInventory.Fusion.FusionModule.OnRigCreated += _event;
			}
			else
			{
				Core.Logger.Msg("Rig found, spawning");
				InventoryManager.SpawnSavedItems(save);
			}
		}

		/// <summary>
		/// Spawn the saved items, run when Fusion is detected
		/// </summary>
		internal static void SpawnSavedItems(Save save)
		{
			if (IsConnected)
			{
				Core.Logger.Msg("Client is connected to a server");
				if (Core.mp_itemsaving.Value)
				{
					if (Core.IsFusionLibraryInitialized) SpawnSavedItems_FSL(save);
					else InventoryManager.SpawnSavedItems(save);
				}
			}
			else
			{
				Core.Logger.Msg("Client is not connected to a server, spawning locally");
				if (Core.mp_itemsaving.Value)
				{
					InventoryManager.SpawnSavedItems(save);
				}
			}
		}

		/// <summary>
		/// Check if a gamemode is currently running in the server
		/// </summary>
		/// <returns>A boolean value indicating whether or not is a gamemode running</returns>
		internal static bool GamemodeCheck()
		{
			if (!IsConnected) return false;
			else return LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeStarted || LabFusion.SDK.Gamemodes.GamemodeManager.StartTimerActive || LabFusion.SDK.Gamemodes.GamemodeManager.IsGamemodeReady;
		}

		/// <summary>
		/// Check if the current gamemode allows the use of KeepInventory
		/// </summary>
		/// <returns>A boolean value indicating whether or not does the gamemode allow the use of KeepInventory</returns>
		internal static bool DoesGamemodeAllow()
		{
			if (!IsConnected)
			{
				return false;
			}
			else
			{
				var active = LabFusion.SDK.Gamemodes.GamemodeManager.ActiveGamemode;
				if (active == null) return true;
				if (!GamemodeCheck()) return true;
				else return active.Metadata != null && active.Metadata.TryGetMetadata("AllowKeepInventory", out string val) && val != null && bool.TryParse(val, out bool res) && res;
			}
		}

		/// <summary>
		/// Spawns a <see cref="Spawnable"/> from provided <see cref="Barcode"/> to an <see cref="InventorySlot"/>
		/// </summary>
		/// <param name="receiver">The <see cref="InventorySlotReceiver"/> to spawn the <see cref="Spawnable"/> in</param>
		/// <param name="barcode">The <see cref="Barcode"/> to be used to spawn the <see cref="Spawnable"/></param>
		/// <param name="slotColor">Color that will be used in the slot prefix</param>
		/// <param name="slotName">Name of the slot (debugging reasons)</param>
		/// <param name="inBetween">An action that will run between the Spawn Request and Spawn In Slot events</param>
		/// <returns>Entity ID of the <see cref="Spawnable"/></returns>
		public static async Task NetworkSpawnInSlotAsync(InventorySlotReceiver receiver, Barcode barcode, System.Drawing.Color slotColor, string slotName = "N/A", Action<GameObject> inBetween = null)
		{
			if (!NetworkInfo.HasServer)
			{
				Warn($"[{slotName}] The player is not connected to a server!");
				return;
			}

			if (!MarrowGame.assetWarehouse.HasCrate(barcode))
			{
				Warn($"[{slotName}] You do not have the mod installed!");
				return;
			}

			if (barcode == null || string.IsNullOrWhiteSpace(barcode.ID) || receiver == null)
			{
				Error($"[{slotName}] Barcode is either null or empty, or the InventorySlotReceiver was null");
				return;
			}
			MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Attempting to spawn {barcode.ID}");

			bool returned = false;
			NetworkAssetSpawner.SpawnCallbackInfo? result = null;

			Exception exception = null;

			var head = RigData.Refs.RigManager.physicsRig.m_head;

			var info = new NetworkAssetSpawner.SpawnRequestInfo
			{
				rotation = head.rotation,
				position = (head.position + (head.forward * 1.5f)),
				spawnable = new Spawnable() { crateRef = new SpawnableCrateReference(barcode), policyData = null },
				spawnEffect = false,
				spawnCallback = async (callbackInfo) =>
				{
					MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Spawned item (Coordinates: {callbackInfo.spawned.transform.position.ToString() ?? "N/A"})");
					try
					{
						int attempts = 0;
						const int maxAttempts = 5;
						const float interval = 0.35f * 1000;

						LabFusion.Entities.NetworkEntity slotEntity = null;

						while (attempts < maxAttempts)
						{
							attempts++;
							if (!LabFusion.Entities.InventorySlotReceiverExtender.Cache.TryGet(receiver, out var _slotEntity))
							{
								Warn($"Could not find the provided receiver in InventorySlotReceiverExtender Cache (Attempt {attempts}/{maxAttempts})");
								returned = true;
								await Task.Delay((int)MathF.Round(interval));
							}
							else
							{
								slotEntity = _slotEntity;
								break;
							}
						}

						if (slotEntity == null)
						{
							Warn("Network Entity for InventorySlotReceiver was not found, aborting");
							returned = true;
							return;
						}

						var weaponExtender = callbackInfo.entity.GetExtender<LabFusion.Entities.WeaponSlotExtender>();

						if (weaponExtender == null)
						{
							Warn("Weapon Slot Extender was not found, aborting");
							returned = true;
							return;
						}

						var slotExtender = slotEntity.GetExtender<LabFusion.Entities.InventorySlotReceiverExtender>();

						byte? index = (byte?)slotExtender.GetIndex(receiver);

						if (!index.HasValue)
						{
							Warn($"[{slotName}] Could not find the extender for the provided receiver, aborting");
							returned = true;
							return;
						}

						MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Running in between function");
						inBetween?.Invoke(callbackInfo.spawned);

						MsgFusionPrefix($"[{slotName.Pastel(slotColor)}] Attempting to equip");

						using var writer = FusionWriter.Create(InventorySlotInsertData.Size);
						var insertData = InventorySlotInsertData.Create(slotEntity.Id, LabFusion.Player.PlayerIdManager.LocalSmallId, callbackInfo.entity.Id, index.Value);

						writer.Write(insertData);

						var message = FusionMessage.Create(NativeMessageTag.InventorySlotInsert, writer);

						try
						{
							LabFusion.Extensions.InteractableHostExtensions.TryDetach(weaponExtender.Component.interactableHost);

							var component = slotExtender.GetComponent(index.Value);

							component.InsertInSlot(weaponExtender.Component.interactableHost);
						}
						finally
						{
							message?.Dispose();
						}

						result = callbackInfo;
					}
					catch (Exception ex)
					{
						exception = ex;
						returned = true;
					}
				}
			};

			NetworkAssetSpawner.Spawn(info);

			// For fuck's sake just now found out it will download by itself if you do a spawn request
			// There used to be code here that I wasted hours on, because it wouldn't work

			while (result == null && !returned && exception == null) await Task.Delay(50);
			if (exception != null) throw exception;
		}

		internal static void MsgFusionPrefix(string message)
		{
			MsgPrefix("Fusion", message, System.Drawing.Color.Cyan);
		}

		internal static void Warn(string message)
		{
			Core.Logger.Warning($"[Fusion] {message}");
		}

		internal static void Error(string message)
		{
			Core.Logger.Warning($"[Fusion] {message}");
		}

		internal static void MsgPrefix(string prefix, string message, System.Drawing.Color color)
		{
			Core.Logger.Msg($"[{prefix.Pastel(color)}] {message}");
		}
	}

	/// <summary>
	/// Class that holds really small amounts of information about a player in a Fusion lobby
	/// </summary>
	/// <param name="smallId"><inheritdoc cref="SmallId"/></param>
	/// <param name="longId"><inheritdoc cref="LongId"/></param>
	/// <param name="displayName"><inheritdoc cref="DisplayName"/></param>
	public class FusionPlayer(byte smallId, ulong longId, string displayName)
	{
		/// <summary>
		/// Display name of the player
		/// </summary>
		public string DisplayName = displayName;

		/// <summary>
		/// Small id of the player
		/// </summary>
		public byte SmallId = smallId;

		/// <summary>
		/// Long id of the player
		/// </summary>
		public ulong LongId = longId;
	}
}