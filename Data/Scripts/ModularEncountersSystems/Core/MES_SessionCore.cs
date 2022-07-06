﻿using ModularEncountersSystems.Admin;
using ModularEncountersSystems.API;
using ModularEncountersSystems.Behavior;
using ModularEncountersSystems.BlockLogic;
using ModularEncountersSystems.Configuration;
using ModularEncountersSystems.Entities;
using ModularEncountersSystems.Helpers;
using ModularEncountersSystems.Logging;
using ModularEncountersSystems.Spawning;
using ModularEncountersSystems.Spawning.Manipulation;
using ModularEncountersSystems.Sync;
using ModularEncountersSystems.Tasks;
using ModularEncountersSystems.Watchers;
using ModularEncountersSystems.World;
using ModularEncountersSystems.Zones;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;

namespace ModularEncountersSystems.Core {

	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class MES_SessionCore : MySessionComponentBase {

		public static bool ModEnabled = true;

		public static string ModVersion = "2.1.64";
		public static MES_SessionCore Instance;

		public static bool IsServer;
		public static bool IsDedicated;
		public static DateTime SessionStartTime;

		public static bool AreaTestStart;

		public static Action UnloadActions;

		public override void LoadData() {

			Instance = this;

			IsServer = MyAPIGateway.Multiplayer.IsServer;
			IsDedicated = MyAPIGateway.Utilities.IsDedicated;
			ModEnabled = CheckSyncRules();

			if (!ModEnabled)
				return;

			SpawnLogger.Setup();
			BehaviorLogger.Setup();
			TaskProcessor.Setup();
			SyncManager.Setup(); //Register Network and Chat Handlers
			DefinitionHelper.Setup();
			EconomyHelper.Setup();
			Settings.InitSettings("LoadData"); //Get Existing Settings From XML or Create New
			AddonManager.DetectAddons(); //Check Add-on Mods
			ProfileManager.Setup();
			SpawnGroupManager.CreateSpawnLists();
			BotSpawner.Setup();
			APIs.RegisterAPIs(0); //Register Any Applicable APIs

			if (!IsServer)
				return;

			BlockManager.Setup(); //Build Lists of Special Blocks
			PlayerSpawnWatcher.Setup();
			PrefabSpawner.Setup();

		}

		public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {

			if (!ModEnabled)
				return;

			if (!MyAPIGateway.Multiplayer.IsServer)
				return;

			APIs.TextHud = new HudAPIv2();

		}

		public override void BeforeStart() {

			if (!ModEnabled)
				return;

			Settings.InitSettings("BeforeStart"); //Get Existing Settings From XML or Create New
			BlockLogicManager.Setup();
			EntityWatcher.RegisterWatcher(); //Scan World For Entities and Setup AutoDetect For New Entities
			SetDefaultSettings();
			APIs.RegisterAPIs(2); //Register Any Applicable APIs

			if (!MyAPIGateway.Multiplayer.IsServer)
				return;

			ProgramBlockControls.SpawnProgramBlockForControls();
			LocalApi.SendApiToMods();
			FactionHelper.PopulateNpcFactionLists();
			EventWatcher.Setup();
			NpcManager.Setup();
			CargoShipWatcher.Setup();
			ZoneManager.Setup();
			BehaviorManager.Setup();
			RelationManager.Setup();
			Cleaning.Setup();
			WaveManager.Setup();
			DamageHelper.Setup();
			PrefabManipulation.Setup();

			SessionStartTime = MyAPIGateway.Session.GameDateTime;
			//AttributeApplication

		}

		public override void UpdateBeforeSimulation() {

			if (!ModEnabled) {

				MyAPIGateway.Utilities.InvokeOnGameThread(() => { this.UpdateOrder = MyUpdateOrder.NoUpdate; });
				return;
			
			}

			TaskProcessor.Process();

		}

		protected override void UnloadData() {

			UnloadActions?.Invoke();

		}

		private static bool CheckSyncRules() {

			if (MES_SessionCore.Instance?.ModContext?.ModId != null && MES_SessionCore.Instance.ModContext.ModId.Contains(".sbm")) {

				foreach (var mod in MyAPIGateway.Session.Mods) {

					var context = mod.GetModContext();

					if (context != null && mod.PublishedFileId == 0 && (context.ModName.Contains("Modular Encounters Systems") || context.ModName.Contains("Modular_Encounters_Systems") || context.ModName.Contains("ModularEncountersSystems"))) {

						if (MyAPIGateway.Utilities.FileExistsInModLocation("ModularEncountersSystemsMod.txt", mod)) {

							SpawnLogger.Write("Detected Offline / Local Version of MES loaded with Workshop Version of MES. Disabling Workshop Version", SpawnerDebugEnum.Startup, true);
							ModEnabled = false;
							return false;

						}

					}

				}

			}

			if (!IsDedicated)
				return true;

			if (MyAPIGateway.Session.SessionSettings.EnableSelectivePhysicsUpdates && MyAPIGateway.Session.SessionSettings.SyncDistance < 10000) {

				//TODO: Log SPU Restriction
				SpawnLogger.Write("Mod Disabled: Selective Physics Updates is Enabled with SyncDistance Less Than 10000", SpawnerDebugEnum.Startup, true);
				SpawnLogger.Write("Disable Selective Physics Updates OR Increase SyncDistance To Minimum of 10000", SpawnerDebugEnum.Startup, true);
				return false;

			}

			return true;
		
		}

		private static void SetDefaultSettings() {

			if (MyAPIGateway.Session.SessionSettings.CargoShipsEnabled)
				MyAPIGateway.Session.SessionSettings.CargoShipsEnabled = false;

			if (MyAPIGateway.Session.SessionSettings.EnableEncounters)
				MyAPIGateway.Session.SessionSettings.EnableEncounters = false;

		}

	}

}
