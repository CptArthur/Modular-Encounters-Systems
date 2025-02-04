﻿using ModularEncountersSystems.Files;
using ModularEncountersSystems.Helpers;
using ModularEncountersSystems.Logging;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;

namespace ModularEncountersSystems.Spawning.Profiles {

	public class StoreProfile {

		public string ProfileSubtypeId;
		public bool SetupComplete;

		public string FileSource;

		public int MinOfferItems;
		public int MaxOfferItems;

		public int MinOrderItems;
		public int MaxOrderItems;

		public bool ItemsRequireInventory;

		public List<string> Offers;
		public List<string> Orders;

		public List<string> RequiredOffers;
		public List<string> RequiredOrders;

		public List<StoreItem> OfferItems;
		public List<StoreItem> OrderItems;

		public List<StoreItem> RequiredOfferItems;
		public List<StoreItem> RequiredOrderItems;

		public bool AddedItemsCombineQuantity;
		public bool AddedItemsAveragePrice;

		public int MaxOre;
		public int MaxIngots;
		public int MaxComponents;
		public int MaxAmmo;
		public int MaxTools;
		public int MaxConsumables;
		public int MaxGas;

		public bool EqualizeOffersAndOrders;

		private static List<IMyStoreItem> _tempItems;
		private static List<int> _tempOfferIndexes;
		private static List<int> _tempOrderIndexes;
		private static IMyStoreItem _tempFirstItem;
		private static IMyStoreItem _tempSecondItem;
		private static IMyStoreItem _tempNewItem;

		private List<MyDefinitionId> _tempUniqueOffers;
		private List<MyDefinitionId> _tempUniqueOrders;

		public Dictionary<string, Action<string, object>> EditorReference;

		public StoreProfile() {

			ProfileSubtypeId = "";
			SetupComplete = false;

			FileSource = "";

			MinOfferItems = 10;
			MaxOfferItems = 10;

			MinOrderItems = 10;
			MaxOrderItems = 10;

			Offers = new List<string>();
			Orders = new List<string>();

			RequiredOffers = new List<string>();
			RequiredOrders = new List<string>();

			OfferItems = new List<StoreItem>();
			OrderItems = new List<StoreItem>();

			RequiredOfferItems = new List<StoreItem>();
			RequiredOrderItems = new List<StoreItem>();

			AddedItemsCombineQuantity = false;
			AddedItemsAveragePrice = false;

			MaxOre = 100000;
			MaxIngots = 50000;
			MaxComponents = 10000;
			MaxAmmo = 1000;
			MaxTools = 50;
			MaxConsumables = 250;
			MaxGas = 100000;

			EqualizeOffersAndOrders = false;

			_tempItems = new List<IMyStoreItem>();
			_tempOfferIndexes = new List<int>();
			_tempOrderIndexes = new List<int>();
			_tempUniqueOffers = new List<MyDefinitionId>();
			_tempUniqueOrders = new List<MyDefinitionId>();

			EditorReference = new Dictionary<string, Action<string, object>> {

				{"FileSource", (s, o) => TagParse.TagStringCheck(s, ref FileSource) },
				{"MinOfferItems", (s, o) => TagParse.TagIntCheck(s, ref MinOfferItems) },
				{"MaxOfferItems", (s, o) => TagParse.TagIntCheck(s, ref MaxOfferItems) },
				{"MinOrderItems", (s, o) => TagParse.TagIntCheck(s, ref MinOrderItems) },
				{"MaxOrderItems", (s, o) => TagParse.TagIntCheck(s, ref MaxOrderItems) },
				{"Offers", (s, o) => TagParse.TagStringListCheck(s, ref Offers) },
				{"Orders", (s, o) => TagParse.TagStringListCheck(s, ref Orders) },
				{"RequiredOffers", (s, o) => TagParse.TagStringListCheck(s, ref RequiredOffers) },
				{"RequiredOrders", (s, o) => TagParse.TagStringListCheck(s, ref RequiredOrders) },
				{"AddedItemsCombineQuantity", (s, o) => TagParse.TagBoolCheck(s, ref AddedItemsCombineQuantity) },
				{"AddedItemsAveragePrice", (s, o) => TagParse.TagBoolCheck(s, ref AddedItemsAveragePrice) },
				{"MaxOre", (s, o) => TagParse.TagIntCheck(s, ref MaxOre) },
				{"MaxIngots", (s, o) => TagParse.TagIntCheck(s, ref MaxIngots) },
				{"MaxComponents", (s, o) => TagParse.TagIntCheck(s, ref MaxComponents) },
				{"MaxAmmo", (s, o) => TagParse.TagIntCheck(s, ref MaxAmmo) },
				{"MaxTools", (s, o) => TagParse.TagIntCheck(s, ref MaxTools) },
				{"MaxConsumables", (s, o) => TagParse.TagIntCheck(s, ref MaxConsumables) },
				{"MaxGas", (s, o) => TagParse.TagIntCheck(s, ref MaxGas) },
				{"EqualizeOffersAndOrders", (s, o) => TagParse.TagBoolCheck(s, ref EqualizeOffersAndOrders) },

			};

		}

		public void Setup() {

			if (SetupComplete)
				return;

			SetupComplete = true;

			if (string.IsNullOrWhiteSpace(FileSource)) {

				SpawnLogger.Write("StoreProfile with ID [" + ProfileSubtypeId + "] Missing File Source. Cannot Link StoreItems.", SpawnerDebugEnum.Error);
				return;
			
			}

			StoreItemsContainer container = ProfileManager.GetStoreItemContainer(FileSource);

			if (container == null) {

				SpawnLogger.Write("StoreProfile with ID [" + ProfileSubtypeId + "] using FileSource [" + FileSource + "] returned null StoreItemContainer.", SpawnerDebugEnum.Error);
				return;

			}

			SpawnLogger.Write("StoreProfile with ID [" + ProfileSubtypeId + "] Contains Store Item Count: " + container.StoreItems.Length, SpawnerDebugEnum.Dev);

			foreach (var item in container.StoreItems) {

				SpawnLogger.Write(item.StoreItemId, SpawnerDebugEnum.Dev);

				if (item.ItemType != StoreProfileItemTypes.RandomCraftable && item.ItemType != StoreProfileItemTypes.RandomItem && !EconomyHelper.AllItemIds.Contains(item.GetItemId()))
					if(item.ItemType != StoreProfileItemTypes.Hydrogen && item.ItemType != StoreProfileItemTypes.Oxygen && item.ItemType != StoreProfileItemTypes.Prefab)
						continue;

				if (Offers.Contains(item.StoreItemId))
					OfferItems.Add(item);

				if (Orders.Contains(item.StoreItemId))
					OrderItems.Add(item);

				if (RequiredOffers.Contains(item.StoreItemId))
					RequiredOfferItems.Add(item);

				if (RequiredOrders.Contains(item.StoreItemId))
					RequiredOrderItems.Add(item);

			}
		
		}

		public void InitTags(string customData) {

			if (string.IsNullOrWhiteSpace(customData) == false) {

				var descSplit = customData.Split('\n');

				foreach (var tag in descSplit) {

					EditValue(tag);

				}

			}

			Setup();

		}

		public void EditValue(string receivedValue) {

			var processedTag = TagParse.ProcessTag(receivedValue);

			if (processedTag.Length < 2)
				return;

			Action<string, object> referenceMethod = null;

			if (!EditorReference.TryGetValue(processedTag[0], out referenceMethod))
				//TODO: Notes About Value Not Found
				return;

			referenceMethod?.Invoke(receivedValue, null);

		}

		public void ApplyProfileToBlock(IMyStoreBlock block, bool clearExisting = true) {

			if (block == null) {

				BehaviorLogger.Write(" - Store Block Null", BehaviorDebugEnum.Action);
				return;

			}
				
			_tempItems.Clear();
			_tempUniqueOffers.Clear();
			_tempUniqueOrders.Clear();
			block.GetStoreItems(_tempItems);

			if (clearExisting) {

				BehaviorLogger.Write(" - Clearing Existing Items In Store", BehaviorDebugEnum.Action);

				foreach (var item in _tempItems)
					block.RemoveStoreItem(item);

				_tempItems.Clear();

			}

			PrepareTempIndexList(_tempOfferIndexes, MathTools.RandomBetween(MinOfferItems, MaxOfferItems + 1), OfferItems.Count);
			PrepareTempIndexList(_tempOrderIndexes, MathTools.RandomBetween(MinOrderItems, MaxOrderItems + 1), OrderItems.Count);

			BehaviorLogger.Write(" - Potential Offers: " + OfferItems.Count, BehaviorDebugEnum.Action);
			BehaviorLogger.Write(" - Potential Orders: " + OrderItems.Count, BehaviorDebugEnum.Action);
			BehaviorLogger.Write(" - Required Offers: " + RequiredOffers.Count + " / " + RequiredOfferItems.Count, BehaviorDebugEnum.Action);
			BehaviorLogger.Write(" - Required Orders: " + RequiredOrders.Count + " / " + RequiredOrderItems.Count, BehaviorDebugEnum.Action);
			BehaviorLogger.Write(" - Temp Offer Index Count: " + _tempOfferIndexes.Count, BehaviorDebugEnum.Action);
			BehaviorLogger.Write(" - Temp Order Index Count: " + _tempOrderIndexes.Count, BehaviorDebugEnum.Action);

			//Required Offers
			foreach (var item in RequiredOfferItems)
				AddItemToStore(block, item, true);

			//Required Orders
			foreach (var item in RequiredOrderItems)
				AddItemToStore(block, item, false);

			//Offers
			foreach (var index in _tempOfferIndexes)
				AddItemToStore(block, OfferItems[index], true);

			//Orders
			foreach (var index in _tempOrderIndexes)
				AddItemToStore(block, OrderItems[index], false);

			//Concat Duplicates
			_tempItems.Clear();
			block.GetStoreItems(_tempItems);

			for (int i = _tempItems.Count - 1; i >= 0; i--) {

				_tempFirstItem = _tempItems[i];

				for (int j = _tempItems.Count - 1; j >= 0; j--) {
				
					_tempSecondItem = _tempItems[j];

					if (_tempSecondItem == _tempFirstItem || _tempFirstItem.StoreItemType != _tempSecondItem.StoreItemType)
						continue;

					if (!_tempFirstItem.Item.HasValue || !_tempSecondItem.Item.HasValue)
						continue;

					if (_tempFirstItem.Item.Value.TypeId != _tempSecondItem.Item.Value.TypeId)
						continue;

					if (_tempFirstItem.Item.Value.SubtypeId != _tempSecondItem.Item.Value.SubtypeId)
						continue;

					_tempFirstItem.Amount += _tempSecondItem.Amount;
					_tempSecondItem.Amount = 0;

				}

			}

			if (EqualizeOffersAndOrders)
				EqualizeItems();

			for (int i = _tempItems.Count - 1; i >= 0; i--) {

				_tempFirstItem = _tempItems[i];

				if (_tempFirstItem.Amount == 0)
					block.RemoveStoreItem(_tempFirstItem);

			}

		}

		private void AddItemToStore(IMyStoreBlock block, StoreItem item, bool offer) {

			float additionalAdd = 0;
			float oldPrice = 0;

			if (item.ItemType != StoreProfileItemTypes.Prefab && item.ItemType != StoreProfileItemTypes.RandomCraftable && item.ItemType != StoreProfileItemTypes.RandomItem) {

				var id = item.GetItemId();

				for (int i = _tempItems.Count - 1; i >= 0; i--) {

					if (_tempItems[i].Item.HasValue && _tempItems[i].Item.Value.SubtypeId == id.SubtypeName && _tempItems[i].Item.Value.TypeId == id.TypeId) {

						additionalAdd = AddedItemsCombineQuantity ? _tempItems[i].Amount - _tempItems[i].RemovedAmount : 0;
						oldPrice = _tempItems[i].PricePerUnit;
						_tempItems.RemoveAt(i);
						break;

					}
				
				}
			
			}

			var newItem = CreateStoreItem(block, item, offer ? StoreItemTypes.Offer : StoreItemTypes.Order, (int)additionalAdd);
			
			if (newItem.PricePerUnit <= 0) {

				BehaviorLogger.Write(" - Item Cost Zero", BehaviorDebugEnum.Action);
				return;

			}

			newItem.IsCustomStoreItem = item.IsCustomItem;
			LimitMaximumAmount(newItem);


			if (AddedItemsAveragePrice) {

				newItem.PricePerUnit = (int)MathTools.Average(newItem.PricePerUnit, oldPrice);

			}


			block.InsertStoreItem(newItem);
			BehaviorLogger.Write(" - Item Added", BehaviorDebugEnum.Action);

		}

		private IMyStoreItem CreateStoreItem(IMyStoreBlock block, StoreItem item, StoreItemTypes types, int extra) {

			var offer = types == StoreItemTypes.Offer;

			if (item.ItemType == StoreProfileItemTypes.Prefab) {

				return block.CreateStoreItem(item.ItemSubtypeId, 1, (int)item.GetPrice(offer: offer), item.PCU);
			
			}

			if (item.ItemType == StoreProfileItemTypes.Oxygen) {
			
				return block.CreateStoreItem(item.GetAmount(types == StoreItemTypes.Offer) + extra, (int)item.GetPrice(offer: offer), StoreItemTypes.Offer, ItemTypes.Oxygen);

			}

			if (item.ItemType == StoreProfileItemTypes.Hydrogen) {

				return block.CreateStoreItem(item.GetAmount(types == StoreItemTypes.Offer) + extra, (int)item.GetPrice(offer: offer), StoreItemTypes.Offer, ItemTypes.Hydrogen);

			}

			if (item.ItemType == StoreProfileItemTypes.RandomCraftable) {

				var unique = types == StoreItemTypes.Offer ? _tempUniqueOffers : _tempUniqueOrders;
				var id = RandomCraftableItemId(item, unique);

				if (id.HasValue) {

					
					unique.Add(id.Value);
					return block.CreateStoreItem(id.Value, item.GetAmount(types == StoreItemTypes.Offer) + extra, (int)item.GetPrice(offer: offer, overrideId: id), types);

				}
					

			}

			if (item.ItemType == StoreProfileItemTypes.RandomItem) {

				var unique = types == StoreItemTypes.Offer ? _tempUniqueOffers : _tempUniqueOrders;
				var id = RandomItemId(item, unique);

				BehaviorLogger.Write("ID Random: " + id.ToString(), BehaviorDebugEnum.Action);

				if (id.HasValue) {

					unique.Add(id.Value);
					return block.CreateStoreItem(id.Value, item.GetAmount(types == StoreItemTypes.Offer) + extra, (int)item.GetPrice(offer: offer, overrideId: id), types);

				}
					

			}

			if (item.ItemType == StoreProfileItemTypes.None) {

				return null;
			
			}

			return block.CreateStoreItem(item.GetItemId(), item.GetAmount(types == StoreItemTypes.Offer) + extra, (int)item.GetPrice(offer: offer), types);

		}

		private void EqualizeItems() {

			for (int i = _tempItems.Count - 1; i >= 0; i--) {

				_tempFirstItem = _tempItems[i];

				for (int j = _tempItems.Count - 1; j >= 0; j--) {

					_tempSecondItem = _tempItems[j];

					if (_tempSecondItem == _tempFirstItem || _tempFirstItem.StoreItemType == _tempSecondItem.StoreItemType)
						continue;

					if (!_tempFirstItem.Item.HasValue || !_tempSecondItem.Item.HasValue)
						continue;

					if (_tempFirstItem.Item.Value.TypeId != _tempSecondItem.Item.Value.TypeId)
						continue;

					if (_tempFirstItem.Item.Value.SubtypeId != _tempSecondItem.Item.Value.SubtypeId)
						continue;

					if (_tempFirstItem.Amount > _tempSecondItem.Amount) {

						_tempFirstItem.Amount -= _tempSecondItem.Amount;
						_tempSecondItem.Amount = 0;

					} else if (_tempFirstItem.Amount < _tempSecondItem.Amount) {

						_tempSecondItem.Amount -= _tempFirstItem.Amount;
						_tempFirstItem.Amount = 0;

					} else {

						_tempFirstItem.Amount = 0;
						_tempSecondItem.Amount = 0;

					}

				}

			}

		}

		private void LimitMaximumAmount(IMyStoreItem item) {

			int amountMax = -1;

			if (amountMax < 0 && item.Item.HasValue && item.Item.Value.TypeId == typeof(MyObjectBuilder_Ore))
				amountMax = MaxOre;

			if (amountMax < 0 && item.Item.HasValue && item.Item.Value.TypeId == typeof(MyObjectBuilder_Ingot))
				amountMax = MaxIngots;

			if (amountMax < 0 && item.Item.HasValue && item.Item.Value.TypeId == typeof(MyObjectBuilder_Component))
				amountMax = MaxComponents;

			if (amountMax < 0 && item.Item.HasValue && item.Item.Value.TypeId == typeof(MyObjectBuilder_AmmoMagazine))
				amountMax = MaxAmmo;

			if (amountMax < 0 && item.Item.HasValue && item.Item.Value.TypeId == typeof(MyObjectBuilder_PhysicalGunObject))
				amountMax = MaxTools;

			if (amountMax < 0 && item.Item.HasValue && item.Item.Value.TypeId == typeof(MyObjectBuilder_ConsumableItem))
				amountMax = MaxConsumables;

			if (amountMax < 0 && item.Item.HasValue && item.Item.Value.TypeId == typeof(MyObjectBuilder_GasProperties))
				amountMax = MaxGas;

			if (amountMax < 0)
				return;

			if (item.Amount > amountMax)
				item.Amount = amountMax;
		
		}

		private MyDefinitionId? RandomCraftableItemId(StoreItem item, List<MyDefinitionId> exclusionList) {

			if(item.ItemSubtypeId == "Component" && EconomyHelper.CraftableComponents.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.CraftableComponents, exclusionList);

			if (item.ItemSubtypeId == "Ammo" && EconomyHelper.CraftableAmmo.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.CraftableAmmo, exclusionList);

			if (item.ItemSubtypeId == "Tool" && EconomyHelper.CraftableTools.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.CraftableTools, exclusionList);

			if (EconomyHelper.AssemblerCraftableItems.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.AssemblerCraftableItems, exclusionList);

			return null;

			
			
			
			

		}

		private MyDefinitionId? RandomItemId(StoreItem item, List<MyDefinitionId> exclusionList) {

			if (item.ItemSubtypeId == "Ingot" && EconomyHelper.PublicIngots.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.PublicIngots, exclusionList);

			if (item.ItemSubtypeId == "Consumable" && EconomyHelper.PublicConsumables.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.PublicConsumables, exclusionList);

			if (item.ItemSubtypeId == "Component" && EconomyHelper.PublicComponents.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.PublicComponents, exclusionList);

			if (item.ItemSubtypeId == "Ammo" && EconomyHelper.PublicAmmos.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.PublicAmmos, exclusionList);

			if (item.ItemSubtypeId == "Tool" && EconomyHelper.PublicTools.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.PublicTools, exclusionList);

			if (EconomyHelper.PublicItems.Count > 0)
				return CollectionHelper.GetRandomIdFromList(EconomyHelper.AssemblerCraftableItems, exclusionList);

			return null;

			
			
			
			
			
			

		}

		private void PrepareTempIndexList(List<int> list, int randomAmount, int itemCount) {

			list.Clear();

			for (int i = 0; i < itemCount; i++) 
				list.Add(i);

			int itemCountTracker = itemCount;
			int randomAdded = 0;

			while (itemCountTracker > randomAmount && itemCountTracker > 0) {

				list.RemoveAt(MathTools.RandomBetween(0, list.Count));
				itemCountTracker--;
			
			}

		}

	}

}
