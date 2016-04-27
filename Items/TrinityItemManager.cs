﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Buddy.Coroutines;
using Trinity.Cache;
using Trinity.Coroutines.Town;
using TrinityCoroutines.Resources;
using Trinity.DbProvider;
using Trinity.Framework.Actors;
using Trinity.Framework.Objects.Enums;
using Trinity.Helpers;
using Trinity.ItemRules;
using Trinity.Reference;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Items;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Items
{
    public class TrinityItemManager //: ItemManager
    {
        //private RuleTypePriority _priority;

        static TrinityItemManager()
        {
            ItemEvents.OnItemStashed += TrinityOnItemStashed;
            ItemEvents.OnItemSalvaged += TrinityOnItemSalvaged;
            ItemEvents.OnItemSold += TrinityOnItemSold;
        }

        //public override RuleTypePriority Priority
        //{
        //    get
        //    {               
        //        if (_priority == null)
        //        {
        //            _priority = new RuleTypePriority
        //            {
        //                Priority1 = ItemEvaluationType.Keep,
        //                Priority2 = ItemEvaluationType.Salvage,
        //                Priority3 = ItemEvaluationType.Sell
        //            };
        //        }
        //        return _priority;
        //    }
        //}

        //public override bool EvaluateItem(ACDItem item, ItemEvaluationType evaluationType)
        //{
        //    try
        //    {
        //        //Logger.LogVerbose(LogCategory.ItemValuation, "EvaluateItem Called Item={0} ({1}) IsUnidentified={2} EvaluationType={3} IsValid={4} IsDisposed={5} ActorType={6} IsInTown={7} IsLoadingWorld={8} FreeBackpackSlots={9} UnidentifiedInBackpack={10}", 
        //        //    item.InternalName, item.ActorSnoId, item.IsUnidentified, evaluationType, item.IsValid, item.IsDisposed, item.ActorType, ZetaDia.IsInTown, 
        //        //    ZetaDia.IsLoadingWorld, ZetaDia.Me.Inventory.NumFreeBackpackSlots, ZetaDia.Me.Inventory.Backpack.Count(i => i.IsUnidentified));

        //        // Salvage/Sell/Stashing is disabled during greater rifts
        //        if (Trinity.Player.IsInventoryLockedForGreaterRift || !Trinity.Settings.Loot.TownRun.KeepLegendaryUnid && Trinity.Player.ParticipatingInTieredLootRun)
        //            return false;

        //        if (!item.IsValid || item.IsDisposed || item.ActorType != ActorType.Item)
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, "ItemManager.EvaluateItem: Bad Item Data Detected: IsValid={0} IsDisposed={1}",
        //                item.IsValid, item.IsDisposed);

        //            return evaluationType == ItemEvaluationType.Keep;
        //        }

        //        if (item.IsUnidentified)
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, "Item has not been identified yet. {0}", item.Name);
        //            return evaluationType == ItemEvaluationType.Keep;
        //        }

        //        if (Trinity.Settings.Loot.ItemFilterMode != ItemFilterMode.DemonBuddy)
        //        {
        //            Current.EvaluateItem(item, evaluationType);
        //        }
        //        else
        //        {
        //            switch (evaluationType)
        //            {
        //                case ItemEvaluationType.Keep:
        //                    return ShouldStashItem(item);
        //                case ItemEvaluationType.Salvage:
        //                    return ShouldSalvageItem(item);
        //                case ItemEvaluationType.Sell:
        //                    return ShouldSellItem(item);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError("Exception in TrinityItemManager.EvaluateItem {0}", ex);
        //    }
        //    return false;
        //}

        //public override bool ShouldSalvageItem(ACDItem item)
        //{
        //    try
        //    {
        //        //Logger.Log(LogCategory.ItemValuation, "ShouldSalvageItem Called Item={0} ({1}) IsUnidentified={2} IsValid={3} IsDisposed={4} ActorType={5} IsInTown={6} IsLoadingWorld={7} FreeBackpackSlots={8} UnidentifiedBackpackItems={9}",
        //        //    item.InternalName, item.ActorSnoId, item.IsUnidentified, item.IsValid, item.IsDisposed, item.ActorType, ZetaDia.IsInTown, 
        //        //    ZetaDia.IsLoadingWorld, ZetaDia.Me.Inventory.NumFreeBackpackSlots, ZetaDia.Me.Inventory.Backpack.Count(i => i.IsUnidentified));


        //        if (Trinity.Player.IsInventoryLockedForGreaterRift || !Trinity.Settings.Loot.TownRun.KeepLegendaryUnid && Trinity.Player.ParticipatingInTieredLootRun)
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Salvage=False (Greater Rift / Inventory Locked) Item={item.Name} ({item.ActorSnoId})");
        //            return false;
        //        }

        //        if (item.IsUnidentified)
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Salvage=False (Unidentified) Item={item.Name} ({item.ActorSnoId})");
        //            return true;
        //        }

        //        bool action;
        //        action = ShouldSalvageItem(item, ItemEvaluationType.Salvage);
        //        if (action)
        //            ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item), "Salvage");

        //        Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Salvage={action} Item={item.Name} ({item.ActorSnoId})");
        //        return action;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError("Exception in TrinityItemManager.ShouldSalvageItem {0}", ex);
        //    }
        //    return false;
        //}

        //public bool ShouldSalvageItem(ACDItem item, ItemEvaluationType evaluationType)
        //{
        //    //ItemEvents.ResetTownRun();

        //    if (Current.ItemIsProtected(item))
        //        return false;

        //    // Keep any high gems placed in backpack while levelling, so we can socket items with them.
        //    if (item.IsGem && item.GemQuality >= GemQuality.Marquise && ZetaDia.Me.Level < 70)
        //    {
        //        return false;
        //    }

        //    if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
        //    {
        //        return Current.ShouldSalvageItem(item);
        //    }
        //    if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
        //    {
        //        return ItemRulesSalvageSell(item, evaluationType);
        //    }
        //    return TrinitySalvage(new CachedItem(item));
        //}

        //public override bool ShouldSellItem(ACDItem item)
        //{
        //    try
        //    {
        //        //Logger.Log(LogCategory.ItemValuation, "ShouldSellItem Called Item={0} ({1}) IsUnidentified={2} IsValid={3} IsDisposed={4} ActorType={5} IsInTown={6} IsLoadingWorld={7} FreeBackpackSlots={8} UnidentifiedBackpackItems={9}",
        //        //    item.InternalName, item.ActorSnoId, item.IsUnidentified, item.IsValid, item.IsDisposed, item.ActorType, ZetaDia.IsInTown,
        //        //    ZetaDia.IsLoadingWorld, ZetaDia.Me.Inventory.NumFreeBackpackSlots, ZetaDia.Me.Inventory.Backpack.Count(i => i.IsUnidentified));

        //        if (Trinity.Player.IsInventoryLockedForGreaterRift || !Trinity.Settings.Loot.TownRun.KeepLegendaryUnid && Trinity.Player.ParticipatingInTieredLootRun)
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Sell=False (Greater Rift / Inventory Locked) Item={item.Name} ({item.ActorSnoId})");
        //            return false;
        //        }

        //        if (item.IsUnidentified)
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Sell=False (Unidentified) Item={item.Name} ({item.ActorSnoId})");
        //            return true;
        //        }

        //        bool action = ShouldSellItem(item, ItemEvaluationType.Sell);

        //        if (item.IsVendorBought && (action || ShouldSalvageItem(item)))
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Sell=True (Vendor Purchased / Can't be Salvaged) Item={item.Name} ({item.ActorSnoId})");
        //            action = true;
        //        }

        //        if (action)
        //            ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item), "Sell");

        //        Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Sell={action} Item={item.Name} ({item.ActorSnoId})");
        //        return action;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError("Exception in TrinityItemManager.ShouldSellItem {0}", ex);
        //    }
        //    return false;
        //}

        //public bool ShouldSellItem(ACDItem item, ItemEvaluationType evaluationType)
        //{
        //    //ItemEvents.ResetTownRun();

        //    if (Current.ItemIsProtected(item))
        //        return false;

        //    // Keep any high gems placed in backpack while levelling, so we can socket items with them.
        //    if (item.IsGem && item.GemQuality >= GemQuality.Marquise && ZetaDia.Me.Level < 70)
        //    {
        //        return false;
        //    }

        //    if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
        //    {
        //        return Current.ShouldSellItem(item);
        //    }
        //    if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
        //    {
        //        return ItemRulesSalvageSell(item, evaluationType);
        //    }

        //    var cItem = new CachedItem(item);

        //    // Dont sell if we should salvage
        //    if (TrinitySalvage(cItem))
        //        return false;

        //    return TrinitySell(cItem);
        //}

        //public override bool ShouldStashItem(ACDItem item)
        //{
        //    try
        //    {
        //        if (Trinity.Player.IsInventoryLockedForGreaterRift || !Trinity.Settings.Loot.TownRun.KeepLegendaryUnid && Trinity.Player.ParticipatingInTieredLootRun)
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Stash=False (Greater Rift / Inventory Locked) Item={item.Name} ({item.ActorSnoId})");
        //            return false;
        //        }

        //        if (item.IsUnidentified)
        //        {
        //            Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Stash=False (Unidentified) Item={item.Name} ({item.ActorSnoId})");
        //            return true;
        //        }

        //        if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
        //        {
        //            return Current.ShouldStashItem(item);
        //        }

        //        bool action = TrinityStash(new CachedItem(item));

        //        if (action)
        //            ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item), "Stash");
           
        //        //if (item.ItemQualityLevel >= ItemQuality.Legendary && Trinity.Settings.Loot.TownRun.DropInTownOption == DropInTownOption.All)
        //        //    ItemDropper.Drop(item);

        //        //if (action && item.ItemQualityLevel >= ItemQuality.Legendary && Trinity.Settings.Loot.TownRun.DropInTownOption == DropInTownOption.Keep)
        //        //    ItemDropper.Drop(item);

        //        //if (!action && item.ItemQualityLevel >= ItemQuality.Legendary && Trinity.Settings.Loot.TownRun.DropInTownOption == DropInTownOption.Vendor)
        //        //    ItemDropper.Drop(item);

        //        Logger.LogVerbose(LogCategory.ItemValuation, $"[ItemManager] Stash={action} Item={item.Name} ({item.ActorSnoId})");
        //        return action;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError("Exception in TrinityItemManager.ShouldStashItem {0}", ex);
        //    }
        //    return false;
        //}

        public static bool TrinityDrop(CachedItem item, ItemEvaluationType scheduledAction)
        {
            if (item.IsProtected() || item.IsAccountBound)
                return false;
            
            if (item.IsGem || item.IsCraftingReagent || item.TrinityItemType == TrinityItemType.CraftingPlan)
                return false;

            if (!item.IsUnidentified && (item.IsPotion || item.RawItemType == RawItemType.RamaladnisGift || item.IsMiscItem))
                return false;

            if (item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                if (Trinity.Settings.Loot.TownRun.DropInTownOption == DropInTownOption.All)
                {
                    Logger.LogVerbose($"Should Drop {item.Name} - Setting='{Trinity.Settings.Loot.TownRun.DropInTownOption}'");
                    return true;
                }

                switch (scheduledAction)
                {
                    case ItemEvaluationType.Keep:

                        if (Trinity.Settings.Loot.TownRun.DropInTownOption == DropInTownOption.Keep)
                        {
                            Logger.LogVerbose($"Should Drop {item.Name} - Setting='{Trinity.Settings.Loot.TownRun.DropInTownOption}' and item is scheduled for {scheduledAction}");
                            return true;
                        }

                        break;

                    case ItemEvaluationType.Salvage:
                    case ItemEvaluationType.Sell:

                        if (Trinity.Settings.Loot.TownRun.DropInTownOption == DropInTownOption.Vendor)
                        {
                            Logger.LogVerbose($"Should Drop {item.Name} - Setting='{Trinity.Settings.Loot.TownRun.DropInTownOption}' and item is scheduled for {scheduledAction}");
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// Trinity internal stashing checks
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="evaluationType">Type of the evaluation.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool TrinityStash(CachedItem item)
        {
            //ItemEvents.ResetTownRun();

            if (item.IsProtected())
                return false;

            if (Trinity.Player.IsInventoryLockedForGreaterRift || !Trinity.Settings.Loot.TownRun.KeepLegendaryUnid && Trinity.Player.ParticipatingInTieredLootRun)
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return Trinity.Settings.Loot.TownRun.StashVanityItems;

            //CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            // Now look for Misc items we might want to keep
            TrinityItemType tItemType = item.TrinityItemType; // DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            TrinityItemBaseType tBaseType = item.TrinityItemBaseType; // DetermineBaseType(trinityItemType);

            // ItemRules - Always stash ancients setting
            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules && Trinity.Settings.Loot.ItemRules.AlwaysStashAncients && item.IsAncient)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (STASHING: ItemRules Option - Always Stash Ancients)", item.Name, item.InternalName);
                return true;
            }

            // Keep any high gems placed in backpack while levelling, so we can socket items with them.
            if (item.IsGem && item.GemQuality >= GemQuality.Marquise && ZetaDia.Me.Level < 70)
            {
                return false;
            }

            // ItemList - Always stash ancients setting
            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.ItemList && Trinity.Settings.Loot.ItemList.AlwaysStashAncients && item.IsAncient)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (STASHING: ItemList Option - Always Stash Ancients)", item.Name, item.InternalName);
                return true;
            }
                
            // ItemList - Always sell/salvage non-ancients setting
            var isArmorWeaponOrJewellery = item.ItemBaseType == ItemBaseType.Armor || item.ItemBaseType == ItemBaseType.Jewelry || item.ItemBaseType == ItemBaseType.Weapon;

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.ItemList && Trinity.Settings.Loot.ItemList.AlwaysTrashNonAncients && 
                !item.IsAncient && item.ItemQualityLevel >= ItemQuality.Legendary && isArmorWeaponOrJewellery)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (TRASHING: ItemList Option - Always Sell/Salvage Non-Ancients) IsAncient={2} IsUnidentified={3}", item.Name, item.InternalName, item.IsAncient, item.IsUnidentified);
                return false;
            }

            bool isEquipment = (tBaseType == TrinityItemBaseType.Armor ||
                tBaseType == TrinityItemBaseType.Jewelry ||
                tBaseType == TrinityItemBaseType.Offhand ||
                tBaseType == TrinityItemBaseType.WeaponOneHand ||
                tBaseType == TrinityItemBaseType.WeaponRange ||
                tBaseType == TrinityItemBaseType.WeaponTwoHand);

            //if (Trinity.Settings.Loot.TownRun.ApplyPickupValidationToStashing)
            //{
            //    // Check pickup (in case we accidentally picked it up)
            //    var pItem = new PickupItem(item, tBaseType, tItemType);
            //    var pickupCheck = PickupItemValidation(pItem);
            //    if (!pickupCheck)
            //    {
            //        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (TRASHING: Pickup check failed)", item.Name, item.InternalName);
            //        return false;
            //    }
            //    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (Pickup check passed)", item.Name, item.InternalName);
            //}

            if (item.ItemType == ItemType.KeystoneFragment)
            {
                //if ((Trinity.Settings.Loot.TownRun.KeepTieredLootRunKeysInBackpack && item.TieredLootRunKeyLevel >= 1) ||
                //(Trinity.Settings.Loot.TownRun.KeepTrialLootRunKeysInBackpack && item.TieredLootRunKeyLevel == 0) ||
                //(Trinity.Settings.Loot.TownRun.KeepRiftKeysInBackpack && item.TieredLootRunKeyLevel <= -1))
                //    return false;
                return true;
            }

            if (item.TrinityItemType == TrinityItemType.HoradricCache && Trinity.Settings.Loot.TownRun.OpenHoradricCaches)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (ignoring Horadric Cache)", item.Name, item.InternalName);
                return false;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (item.IsUnidentified)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (autokeep unidentified items)", item.Name, item.InternalName);
                return true;
            }
            if (tItemType == TrinityItemType.StaffOfHerding)
            {

                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", item.Name, item.InternalName, tItemType);
                return true;
            }
            if (tItemType == TrinityItemType.CraftingMaterial || item.IsCraftingReagent)
            {
                //var craftMaterialType = GetCraftingMaterialType(item);
                //if (craftMaterialType != InventoryItemType.None)
                //{
                //    // This is really slow, turning it off.
                //    //var stackCount = GetItemStackCount(item, InventorySlot.SharedStash);

                //    //if (craftMaterialType == InventoryItemType.ArcaneDust && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackArcaneDust)
                //    //{
                //    //    Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackArcaneDust);
                //    //    return false;
                //    //}

                //    //if (craftMaterialType == InventoryItemType.DeathsBreath && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackDeathsBreath)
                //    //{
                //    //    Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackDeathsBreath);
                //    //    return false;
                //    //}

                //    //if (craftMaterialType == InventoryItemType.ForgottenSoul && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackForgottenSoul)
                //    //{
                //    //    Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackForgottenSoul);
                //    //    return false;
                //    //}

                //    //if (craftMaterialType == InventoryItemType.ReusableParts && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackReusableParts)
                //    //{
                //    //    Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackReusableParts);
                //    //    return false;
                //    //}

                //    //if (craftMaterialType == InventoryItemType.VeiledCrystal && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackVeiledCrystal)
                //    //{
                //    //    Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackVeiledCrystal);
                //    //    return false;
                //    //}
                //}

                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.Emerald || tItemType == TrinityItemType.Amethyst || tItemType == TrinityItemType.Topaz || tItemType == TrinityItemType.Ruby || tItemType == TrinityItemType.Diamond)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", item.Name, item.InternalName, tItemType);
                return true;
            }
            if (tItemType == TrinityItemType.CraftTome)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", item.Name, item.InternalName, tItemType);
                return true;
            }
            if (tItemType == TrinityItemType.InfernalKey)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep infernal key)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.HealthPotion && item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                var shouldStash = Trinity.Settings.Loot.TownRun.StashLegendaryPotions;
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = ({3} legendary potions)", item.Name, item.InternalName, tItemType,
                        shouldStash ? "stashing" : "ignoring");
                return shouldStash;
            }

            if (tItemType == TrinityItemType.HealthPotion && item.ItemQualityLevel < ItemQuality.Legendary)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", item.Name, item.InternalName, tItemType);
                return false;
            }

            if (tItemType == TrinityItemType.CraftingPlan && item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendary plans)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.ConsumableAddSockets)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep Ramaladni's Gift)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.PortalDevice)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep Machines)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.UberReagent)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep Uber Reagents)", item.Name, item.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.TieredLootrunKey)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (ignoring Tiered Rift Keys)", item.Name, item.InternalName, tItemType);
                return false;
            }

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                Interpreter.InterpreterAction action = Trinity.StashRule.checkItem(item.GetAcdItem(), ItemEvaluationType.Keep);

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "IR2 {0} [{1}] [{2}] = (" + action + ")", item.Name, item.InternalName, item.ItemType);
                switch (action)
                {
                    case Interpreter.InterpreterAction.KEEP:
                        return true;
                    case Interpreter.InterpreterAction.TRASH:
                        return false;
                    case Interpreter.InterpreterAction.SCORE:
                        break;
                }
            }

            if (tItemType == TrinityItemType.CraftingPlan)
            {

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep plans)", item.Name, item.InternalName, tItemType);
                return true;
            }

            // Stashing Whites, auto-keep
            if (Trinity.Settings.Loot.TownRun.StashWhites && isEquipment && item.ItemQualityLevel <= ItemQuality.Superior)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (stashing whites)", item.Name, item.InternalName, tItemType);
                return true;
            }
            // Else auto-trash
            if (item.ItemQualityLevel <= ItemQuality.Superior && (isEquipment || item.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trash whites)", item.Name, item.InternalName, tItemType);
                return false;
            }

            // Stashing blues, auto-keep
            if (Trinity.Settings.Loot.TownRun.StashBlues && isEquipment && item.ItemQualityLevel >= ItemQuality.Magic1 && item.ItemQualityLevel <= ItemQuality.Magic3)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (stashing blues)", item.Name, item.InternalName, tItemType);
                return true;
            }
            // Else auto trash
            if (item.ItemQualityLevel >= ItemQuality.Magic1 && item.ItemQualityLevel <= ItemQuality.Magic3 && (isEquipment || item.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trashing blues)", item.Name, item.InternalName, tItemType);
                return false;
            }

            // Force salvage Rares
            if (Trinity.Settings.Loot.TownRun.ForceSalvageRares && item.ItemQualityLevel >= ItemQuality.Rare4 && item.ItemQualityLevel <= ItemQuality.Rare6 && (isEquipment || item.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (force salvage rare)", item.Name, item.InternalName, tItemType);
                return false;
            }

            // Item List
            if (item.ItemQualityLevel >= ItemQuality.Legendary && Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.ItemList && (item.IsEquipment || item.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
            {
                var result = ItemList.ShouldStashItem(item);
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = {3}", item.Name, item.InternalName, tItemType, "ItemListCheck=" + (result ? "KEEP" : "TRASH"));

                return result;
            }

            if (item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendaries)", item.Name, item.InternalName, tItemType);
                return true;
            }

            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = ItemValuation.ScoreNeeded(item.ItemBaseType);
            double iMyScore = ItemValuation.ValueThisItem(CachedACDItem.GetCachedItem(item.GetAcdItem()), tItemType);

            Logger.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "{0} [{1}] [{2}] = {3}", item.Name, item.InternalName, tItemType, iMyScore);


            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        private static Interpreter.InterpreterAction ItemRulesSalvageSell(ACDItem item, ItemEvaluationType evaluationType)
        {
            //ItemEvents.ResetTownRun();

            if (!item.IsPotion || item.ItemType != ItemType.Potion)
                Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation,
                    "Incoming {0} Request: {1}, {2}, {3}, {4}, {5}",
                    evaluationType, item.ItemQualityLevel, item.Level, item.ItemBaseType,
                    item.ItemType, item.IsOneHand ? "1H" : item.IsTwoHand ? "2H" : "NH");

            Interpreter.InterpreterAction action = Trinity.StashRule.checkItem(item, ItemEvaluationType.Salvage);

            switch (action)
            {
                case Interpreter.InterpreterAction.SALVAGE:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0}: {1}", evaluationType, (evaluationType == ItemEvaluationType.Salvage));
                    return action;
                case Interpreter.InterpreterAction.SELL:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0}: {1}", evaluationType, (evaluationType == ItemEvaluationType.Sell));
                    return action;
                default:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ScriptRule, "Trinity, item is unhandled by ItemRules (SalvageSell)!");
                    return Interpreter.InterpreterAction.NULL;
            }
        }

        /// <summary>
        /// Determines if we should salvage an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool TrinitySalvage(CachedItem item)
        {
            if (item.IsProtected())
                return false;

            if (item.IsUnidentified)
                return false;

            if (Trinity.Player.IsInventoryLockedForGreaterRift || !Trinity.Settings.Loot.TownRun.KeepLegendaryUnid && Trinity.Player.ParticipatingInTieredLootRun)
                return false;

            if (!item.IsSalvageable)
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            if (item.ItemType == ItemType.KeystoneFragment)
                return false;

            // Stashing Whites
            if (Trinity.Settings.Loot.TownRun.StashWhites && item.ItemQualityLevel < ItemQuality.Magic1)
                return false;

            // Stashing Blues
            if (Trinity.Settings.Loot.TownRun.StashBlues && item.ItemQualityLevel > ItemQuality.Superior && item.ItemQualityLevel < ItemQuality.Rare4)
                return false;

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                var result = ItemRulesSalvageSell(item.GetAcdItem(), ItemEvaluationType.Salvage);
                switch (result)
                {
                    case Interpreter.InterpreterAction.SALVAGE:
                        return true;
                    case Interpreter.InterpreterAction.SELL:
                        return false;  
                }
            }

            // Take Salvage Option corresponding to ItemLevel
            SalvageOption salvageOption = GetSalvageOption(item.ItemQualityLevel);

            if (salvageOption == SalvageOption.Salvage)
                return true;

            switch (item.TrinityItemBaseType)
            {
                case TrinityItemBaseType.WeaponRange:
                case TrinityItemBaseType.WeaponOneHand:
                case TrinityItemBaseType.WeaponTwoHand:
                case TrinityItemBaseType.Armor:
                case TrinityItemBaseType.Offhand:
                    return salvageOption == SalvageOption.Salvage;
                case TrinityItemBaseType.Jewelry:
                    return salvageOption == SalvageOption.Salvage;
                case TrinityItemBaseType.FollowerItem:
                    return salvageOption == SalvageOption.Salvage;
                case TrinityItemBaseType.Gem:
                case TrinityItemBaseType.Misc:
                case TrinityItemBaseType.Unknown:
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if we should Sell an Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool TrinitySell(CachedItem item)
        {
            if (item.IsProtected())
                return false;

            //ActorId: 367009, Type: Item, Name: Griswold's Scribblings
            if (item.ActorSnoId == 367009)
                return true;

            if (item.IsUnidentified)
                return false;

            if (Trinity.Player.IsInventoryLockedForGreaterRift || !Trinity.Settings.Loot.TownRun.KeepLegendaryUnid && Trinity.Player.ParticipatingInTieredLootRun)
                return false;

            if (item.IsVendorBought)
                return false;

            if (item.IsGem && item.GemQuality >= GemQuality.Marquise && ZetaDia.Me.Level < 70)
            {
                return false;
            }

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            if (item.ItemType == ItemType.KeystoneFragment)
                return false;

            if (item.ItemType == ItemType.HoradricCache)
                return false;

            //if (Trinity.Settings.Loot.TownRun.ApplyPickupValidationToStashing)
            //{
            //    var pItem = new PickupItem(item.AcdItem, item.TrinityItemBaseType, item.TrinityItemType);
            //    var pickupCheck = PickupItemValidation(pItem);
            //    if (!pickupCheck)
            //        return true;
            //}

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                var result = ItemRulesSalvageSell(item.GetAcdItem(), ItemEvaluationType.Sell);
                switch (result)
                {
                    case Interpreter.InterpreterAction.SALVAGE:
                        return false;
                    case Interpreter.InterpreterAction.SELL:
                        return true;
                }
            }

            switch (item.TrinityItemBaseType)
            {
                case TrinityItemBaseType.WeaponRange:
                case TrinityItemBaseType.WeaponOneHand:
                case TrinityItemBaseType.WeaponTwoHand:
                case TrinityItemBaseType.Armor:
                case TrinityItemBaseType.Offhand:
                case TrinityItemBaseType.Jewelry:
                case TrinityItemBaseType.FollowerItem:
                    return true;
                case TrinityItemBaseType.Gem:
                case TrinityItemBaseType.Misc:
                    if (item.TrinityItemType == TrinityItemType.CraftingPlan)
                        return true;
                    if (item.TrinityItemType == TrinityItemType.CraftingMaterial)
                        return true;
                    return false;
                case TrinityItemBaseType.Unknown:
                    return false;
            }

            return false;
        }
        private static SalvageOption GetSalvageOption(ItemQuality quality)
        {
            if (quality >= ItemQuality.Inferior && quality <= ItemQuality.Superior)
            {
                return Trinity.Settings.Loot.TownRun.SalvageWhiteItemOption;
            }
            if (quality >= ItemQuality.Magic1 && quality <= ItemQuality.Magic3)
            {
                return Trinity.Settings.Loot.TownRun.SalvageBlueItemOption;
            }
            if (quality >= ItemQuality.Rare4 && quality <= ItemQuality.Rare6)
            {
                return Trinity.Settings.Loot.TownRun.SalvageYellowItemOption;
            }
            if (quality >= ItemQuality.Legendary)
            {
                return Trinity.Settings.Loot.TownRun.SalvageLegendaryItemOption;
            }
            return SalvageOption.Sell;
        }

        public enum DumpItemLocation
        {
            All,
            Equipped,
            Backpack,
            Ground,
            Stash,
            Merchant,
        }

        public static void DumpQuickItems()
        {
            List<ACDItem> itemList;
            try
            {
                itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).OrderBy(i => i.InventorySlot).ThenBy(i => i.Name).ToList();
            }
            catch
            {
                Logger.LogError("QuickDump: Item Errors Detected!");
                itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).ToList();
            }
            StringBuilder sbTopList = new StringBuilder();
            foreach (var item in itemList)
            {
                try
                {
                    sbTopList.AppendFormat("\nName={0} InternalName={1} ActorSnoId={2} DynamicID={3} InventorySlot={4}",
                        item.Name, item.InternalName, item.ActorSnoId, item.AnnId, item.InventorySlot);
                }
                catch (Exception)
                {
                    sbTopList.AppendFormat("Exception reading data from ACDItem ACDId={0}", item.ACDId);
                }
            }
            Logger.Log(sbTopList.ToString());
        }

#pragma warning disable 1718
        public static void DumpItems(DumpItemLocation location)
        {
            //ZetaDia.Actors.Update();
            using (ZetaDia.Memory.SaveCacheState())
            {
                ZetaDia.Memory.TemporaryCacheState(false);

                List<ItemWrapper> itemList = new List<ItemWrapper>();

                switch (location)
                {
                    case DumpItemLocation.All:
                        itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).Select(i => new ItemWrapper(i)).OrderBy(i => i.InventorySlot).ThenBy(i => i.Name).ToList();
                        break;
                    case DumpItemLocation.Backpack:
                        itemList = ZetaDia.Me.Inventory.Backpack.Select(i => new ItemWrapper(i)).ToList();
                        break;
                    case DumpItemLocation.Merchant:
                        itemList = ZetaDia.Me.Inventory.MerchantItems.Select(i => new ItemWrapper(i)).ToList();
                        break;
                    case DumpItemLocation.Ground:
                        itemList = ZetaDia.Actors.GetActorsOfType<DiaItem>(true).Select(i => new ItemWrapper(i.CommonData)).ToList();
                        break;
                    case DumpItemLocation.Equipped:
                        itemList = ZetaDia.Me.Inventory.Equipped.Select(i => new ItemWrapper(i)).ToList();
                        break;
                    case DumpItemLocation.Stash:
                        if (UIElements.StashWindow.IsVisible)
                        {
                            itemList = ZetaDia.Me.Inventory.StashItems.Select(i => new ItemWrapper(i)).ToList();
                        }
                        else
                        {
                            Logger.Log("Stash window not open!");
                        }
                        break;
                }

                itemList.RemoveAll(i => i == null);
                //itemList.RemoveAll(i => !i.IsValid);

                foreach (var item in itemList.OrderBy(i => i.InventorySlot).ThenBy(i => i.Name))
                {
                    try
                    {
                        string itemName = String.Format("\nName={0} InternalName={1} ActorSnoId={2} DynamicID={3} InventorySlot={4}",
                        item.Name, item.InternalName, item.ActorSNO, item.DynamicId, item.InventorySlot);

                        Logger.Log(itemName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Basic Item Info\n{0}", ex.ToString());
                    }
                    try
                    {
                        foreach (object val in Enum.GetValues(typeof(ActorAttributeType)))
                        {
                            int iVal = item.Item.GetAttribute<int>((ActorAttributeType)val);
                            float fVal = item.Item.GetAttribute<float>((ActorAttributeType)val);

                            if (iVal > 0 || fVal > 0)
                                Logger.Log("Attribute: {0}, iVal: {1}, fVal: {2}", val, iVal, (fVal != fVal) ? "" : fVal.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading attributes for {0}\n{1}", item.Name, ex.ToString());
                    }

                    try
                    {
                        foreach (var stat in Enum.GetValues(typeof(ItemStats.Stat)).Cast<ItemStats.Stat>())
                        {
                            float fStatVal = item.Stats.GetStat<float>(stat);
                            int iStatVal = item.Stats.GetStat<int>(stat);
                            if (fStatVal > 0 || iStatVal > 0)
                                Logger.Log("Stat {0}={1}f ({2})", stat, fStatVal, iStatVal);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Stats\n{0}", ex.ToString());
                    }

                    try
                    {
                        Logger.Log("Link Color ItemQuality=" + item.Item.GetItemQuality());
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Link\n{0}", ex.ToString());
                    }

                    try
                    {
                        PrintObjectProperties(item);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Properties\n{0}", ex.ToString());
                    }

                }
            }

        }

        private static void PrintObjectProperties<T>(T item)
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties.ToList().OrderBy(p => p.Name))
            {
                try
                {
                    object val = property.GetValue(item, null);
                    if (val != null)
                    {
                        Logger.Log(typeof(T).Name + "." + property.Name + "=" + val);

                        // Special cases!
                        if (property.Name == "ValidInventorySlots")
                        {
                            foreach (var slot in ((InventorySlot[])val))
                            {
                                Logger.Log(slot.ToString());
                            }
                        }
                    }
                }
                catch
                {
                    Logger.Log("Exception reading {0} from object", property.Name);
                }
            }
        }

        private static int _lastBackPackCount;
        private static int _lastProtectedSlotsCount;
        private static Vector2 _lastBackPackLocation = new Vector2(-2, -2);

        public static readonly Vector2 NoFreeSlot = new Vector2(-1, -1);

        internal static void ResetBackPackCheck()
        {
            _lastBackPackCount = -1;
            _lastProtectedSlotsCount = -1;
            _lastBackPackLocation = new Vector2(-2, -2);
            TownRun.LastCheckBackpackDurability = DateTime.MinValue;
        }

        /// <summary>
        /// Check for space ignoring user settings on townrun bag space.
        /// </summary>
        public static bool IsAnyTwoSlotBackpackLocation
        {
            get
            {
                var validLocation = FindValidBackpackLocation(true, true);
                return validLocation.X >= 0 && validLocation.Y >= 0;
            }
        }

        public static bool IsValidTwoSlotBackpackLocation
        {
            get
            {
                var validLocation = FindValidBackpackLocation(true);
                return validLocation.X >= 0 && validLocation.Y >= 0;               
            }
        }

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        /// <param name="isOriginalTwoSlot"></param>
        /// <returns></returns>
        internal static Vector2 FindValidBackpackLocation(bool isOriginalTwoSlot, bool forceRefresh = false)
        {
            using (new PerformanceLogger("FindValidBackpackLocation"))
            {
                try
                {
                    if (!forceRefresh && _lastBackPackLocation != new Vector2(-2, -2) && _lastBackPackLocation != new Vector2(-1, -1) &&
                        _lastBackPackCount == CacheData.Inventory.Backpack.Count &&
                        _lastProtectedSlotsCount == CharacterSettings.Instance.ProtectedBagSlots.Count)
                    {
                        return _lastBackPackLocation;
                    }

                    bool[,] backpackSlotBlocked = new bool[10, 6];

                    int freeBagSlots = 60;

                    if (!forceRefresh)
                    {
                        _lastProtectedSlotsCount = CharacterSettings.Instance.ProtectedBagSlots.Count;
                        _lastBackPackCount = CacheData.Inventory.Backpack.Count;
                    }

                    // Block off the entire of any "protected bag slots"
                    foreach (InventorySquare square in CharacterSettings.Instance.ProtectedBagSlots)
                    {
                        backpackSlotBlocked[square.Column, square.Row] = true;
                        freeBagSlots--;
                    }

                    // Map out all the items already in the backpack
                    foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
                    {
                        if (!item.IsValid)
                        {
                            Logger.LogError("Invalid backpack item detetected! marking down two slots!");
                            freeBagSlots -= 2;
                            continue;
                        }
                        int row = item.InventoryRow;
                        int col = item.InventoryColumn;

                        if (row < 0 || row > 5)
                        {
                            Logger.LogError("Item {0} ({1}) is reporting invalid backpack row of {2}!",
                                item.Name, item.InternalName, item.InventoryRow);
                            continue;
                        }

                        if (col < 0 || col > 9)
                        {
                            Logger.LogError("Item {0} ({1}) is reporting invalid backpack column of {2}!",
                                item.Name, item.InternalName, item.InventoryColumn);
                            continue;
                        }

                        // Slot is already protected, don't double count
                        if (!backpackSlotBlocked[col, row])
                        {
                            backpackSlotBlocked[col, row] = true;
                            freeBagSlots--;
                        }

                        if (!item.IsTwoSquareItem)
                            continue;

                        try
                        {
                            // Slot is already protected, don't double count
                            if (backpackSlotBlocked[col, row + 1])
                                continue;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (item.IsValid && !item.IsDisposed)
                            {
                                Logger.LogDebug("Error checking for next slot on item {0}, row={1} col={2} IsTwoSquare={3} ItemType={4}",
                                    item.Name, item.InventoryRow, item.InventoryColumn, item.IsTwoSquareItem, item.ItemType);
                            }
                            else
                            {
                                Logger.LogDebug("Error checking for next slot on item is no longer valid");
                            }                            
                            continue;
                        }

                        freeBagSlots--;
                        backpackSlotBlocked[col, row + 1] = true;
                    }

                    bool noFreeSlots = freeBagSlots < 1;
                    int unprotectedSlots = 60 - _lastProtectedSlotsCount;

                    // Use count of Unprotected slots if FreeBagSlots is higher than unprotected slots
                    int minFreeSlots = Trinity.Player.IsInTown ?
                        Math.Min(Trinity.Settings.Loot.TownRun.FreeBagSlotsInTownA, unprotectedSlots) :
                        Math.Min(Trinity.Settings.Loot.TownRun.FreeBagSlotsA, unprotectedSlots);

                    // free bag slots is less than required
                    if (noFreeSlots || freeBagSlots < minFreeSlots && !forceRefresh)
                    {
                        Logger.LogDebug("Free Bag Slots is less than required. FreeSlots={0}, FreeBagSlots={1} FreeBagSlotsInTown={2} IsInTown={3} Protected={4} BackpackCount={5}",
                            freeBagSlots, Trinity.Settings.Loot.TownRun.FreeBagSlotsA, Trinity.Settings.Loot.TownRun.FreeBagSlotsInTownA, Trinity.Player.IsInTown,
                            _lastProtectedSlotsCount, _lastBackPackCount);
                        _lastBackPackLocation = NoFreeSlot;
                        return _lastBackPackLocation;
                    }

                    // 10 columns
                    Vector2 pos;
                    for (int col = 0; col <= 9; col++)
                    {
                        // 6 rows
                        for (int row = 0; row <= 5; row++)
                        {
                            // Slot is blocked, skip
                            if (backpackSlotBlocked[col, row])
                                continue;

                            // Not a two slotitem, slot not blocked, use it!
                            if (!isOriginalTwoSlot)
                            {
                                pos = new Vector2(col, row);
                                if (!forceRefresh)
                                {
                                    _lastBackPackLocation = pos;
                                }
                                return pos;
                            }

                            // Is a Two Slot, Can't check for 2 slot items on last row
                            if (row == 5)
                                continue;

                            // Is a Two Slot, check row below
                            if (backpackSlotBlocked[col, row + 1])
                                continue;

                            pos = new Vector2(col, row);
                            if (!forceRefresh)
                            {
                                _lastBackPackLocation = pos;
                            }
                            return pos;
                        }
                    }

                    // no free slot
                    Logger.LogDebug("No Free slots!");

                    pos = NoFreeSlot;
                    if (!forceRefresh)
                    {
                        _lastBackPackLocation = pos;
                    }

                    return pos;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogCategory.UserInformation, "Error in finding backpack slot");
                    Logger.Log(LogCategory.UserInformation, "{0}", ex.ToString());
                    return NoFreeSlot;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool ItemRulesPickupValidation(PickupItem item)
        {
            if (Trinity.StashRule == null)
                Trinity.StashRule = new Interpreter();

            Interpreter.InterpreterAction action = Trinity.StashRule.checkPickUpItem(item, ItemEvaluationType.PickUp);

            switch (action)
            {
                case Interpreter.InterpreterAction.PICKUP:
                    return true;

                case Interpreter.InterpreterAction.IGNORE:
                    return false;
            }
            return PickupItemValidation(item);
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool PickupItemValidation(PickupItem item)
        {
            // Calculate item types and base types etc.
            TrinityItemType itemType = DetermineItemType(item.InternalName, item.DBItemType, item.ItemFollowerType);
            TrinityItemBaseType baseType = DetermineBaseType(itemType);

            // Pickup Ramaladni's Gift
            if (itemType == TrinityItemType.ConsumableAddSockets)
            {
                return Trinity.Settings.Loot.Pickup.RamadalinisGift;
            }

            // Pickup Deaths Breath
            // Crafting Materials have the same SNO when on the ground
            // GameBalanceId still works though.
            // 2087837753 DB
            // -605947593 veil
            if (item.BalanceID == 2087837753)
            {               
                return Trinity.Settings.Loot.Pickup.PickupDeathsBreath;
            }
            
            // Tiered Rift Keys
            if (itemType == TrinityItemType.TieredLootrunKey)
            {
                return Trinity.Settings.Loot.Pickup.LootRunKey;
            }

            // Pickup Legendary potions
            if (itemType == TrinityItemType.HealthPotion && item.Quality >= ItemQuality.Legendary)
            {
                return Trinity.Settings.Loot.Pickup.LegendaryPotions;
            }

            if (itemType == TrinityItemType.InfernalKey || itemType == TrinityItemType.PortalDevice)
            {
                return Trinity.Settings.Loot.Pickup.InfernalKeys;
            }

            if (itemType == TrinityItemType.UberReagent)
            {
                return true;
            }

            // Rift Keystone Fragments == LootRunkey
            if (itemType == TrinityItemType.LootRunKey)
            {
                return Trinity.Settings.Loot.Pickup.LootRunKey;
            }

            // Blood Shards == HoradricRelic
            if (itemType == TrinityItemType.HoradricRelic && ZetaDia.PlayerData.BloodshardCount < Trinity.Player.MaxBloodShards)
            {
                return Trinity.Settings.Loot.Pickup.BloodShards;
            }

            if (itemType == TrinityItemType.ProgressionGlobe)
                return true;

            if (itemType == TrinityItemType.CraftingMaterial && (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality || !Trinity.Settings.Loot.Pickup.CraftMaterials))
            {
                return false;
            }

            // Plans
            if (item.InternalName.ToLower().StartsWith("craftingplan_smith") && (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality || !Trinity.Settings.Loot.Pickup.Plans))
            {
                return false;
            }

            // Designs
            if (item.InternalName.ToLower().StartsWith("craftingplan_jeweler") && (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality || !Trinity.Settings.Loot.Pickup.Designs))
            {
                return false;
            }

            if (itemType == TrinityItemType.CraftingPlan && item.Quality >= ItemQuality.Legendary && Trinity.Settings.Loot.Pickup.LegendaryPlans)
            {
                return true;
            }

            if (item.IsUpgrade && Trinity.Settings.Loot.Pickup.PickupUpgrades)
            {
                return true;
            }

            switch (baseType)
            {
                case TrinityItemBaseType.WeaponTwoHand:
                case TrinityItemBaseType.WeaponOneHand:
                case TrinityItemBaseType.WeaponRange:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueWeapons, Trinity.Settings.Loot.Pickup.PickupYellowWeapons);
                case TrinityItemBaseType.Armor:
                case TrinityItemBaseType.Offhand:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueArmor, Trinity.Settings.Loot.Pickup.PickupYellowArmor);
                case TrinityItemBaseType.Jewelry:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueJewlery, Trinity.Settings.Loot.Pickup.PickupYellowJewlery);
                case TrinityItemBaseType.FollowerItem:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaryFollowerItems;

                    if (item.Quality >= ItemQuality.Magic1 && item.Quality <= ItemQuality.Magic3)
                        return Trinity.Settings.Loot.Pickup.PickupBlueFollowerItems;

                    if (item.Quality >= ItemQuality.Rare4 && item.Quality <= ItemQuality.Rare6)
                        return Trinity.Settings.Loot.Pickup.PickupYellowFollowerItems;
                    // not matched above, ignore it
                    return false;
                case TrinityItemBaseType.Gem:
                    if (item.Level < Trinity.Settings.Loot.Pickup.GemLevel ||
                        (itemType == TrinityItemType.Ruby && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Ruby)) ||
                        (itemType == TrinityItemType.Emerald && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Emerald)) ||
                        (itemType == TrinityItemType.Amethyst && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Amethyst)) ||
                        (itemType == TrinityItemType.Topaz && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Topaz)) ||
                        (itemType == TrinityItemType.Diamond && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Diamond)))
                    {
                        return false;
                    }
                    break;
                case TrinityItemBaseType.Misc:
                    if (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality)
                        return false;

                    // Potion filtering
                    if (itemType == TrinityItemType.HealthPotion && item.Quality < ItemQuality.Legendary)
                    {
                        long potionsInBackPack = ZetaDia.Me.Inventory.Backpack.Where(p => p.ItemType == ItemType.Potion).Sum(p => p.ItemStackQuantity);

                        if (potionsInBackPack >= Trinity.Settings.Loot.Pickup.PotionCount)
                            return false;
                        return true;
                    }
                    break;
                case TrinityItemBaseType.HealthGlobe:
                    return true;
                case TrinityItemBaseType.ProgressionGlobe:
                    return true;
                case TrinityItemBaseType.Unknown:
                    return false;
                default:
                    return false;
            }

            // Didn't cancel it, so default to true!
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool IdentifyItemValidation(PickupItem item)
        {
            if (Trinity.Settings.Loot.TownRun.KeepLegendaryUnid)
                return false;
            return true;
        }

        internal static bool ItemRulesIdentifyValidation(ACDItem item)
        {
            //ItemEvents.ResetTownRun();

            var pickupItem = new PickupItem(
                item.Name,
                item.InternalName,
                item.Level,
                item.ItemQualityLevel,
                item.GameBalanceId,
                item.ItemBaseType,
                item.ItemType,
                item.IsOneHand,
                item.IsTwoHand,
                item.FollowerSpecialType,
                item.ACDId,
                item.AnnId);

            Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation,
                "Incoming Identification Request: {0}, {1}, {2}, {3}, {4}",
                pickupItem.Quality, pickupItem.Level, pickupItem.DBBaseType,
                pickupItem.DBItemType, pickupItem.IsOneHand ? "1H" : pickupItem.IsTwoHand ? "2H" : "NH");

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules && Trinity.StashRule != null)
            {
                // using ItemEvaluationType.Identify isn't available so we are abusing Sell for that manner
                Interpreter.InterpreterAction action = Trinity.StashRule.checkPickUpItem(pickupItem, ItemEvaluationType.Sell);

                Logger.Log(TrinityLogLevel.Debug, LogCategory.ItemValuation, "Action is: {0}", action);

                switch (action)
                {
                    case Interpreter.InterpreterAction.IDENTIFY:
                        return true;
                    case Interpreter.InterpreterAction.UNIDENT:
                        return false;
                    default:
                        Logger.Log(TrinityLogLevel.Info, LogCategory.ScriptRule, "Trinity, item is unhandled by ItemRules (Identification)!");
                        return IdentifyItemValidation(pickupItem);
                }
            }

            return IdentifyItemValidation(pickupItem);
        }

        /// <summary>
        ///     Checks if current item's level is according to min level for Pickup.
        /// </summary>
        /// <param name="level">The current item's level.</param>
        /// <param name="quality">The item's quality.</param>
        /// <param name="pickupBlue">Pickup Blue items</param>
        /// <param name="pickupYellow">Pikcup Yellow items</param>
        /// <returns></returns>
        internal static bool CheckLevelRequirements(int level, ItemQuality quality, bool pickupBlue, bool pickupYellow)
        {
            // Gray Items
            if (quality < ItemQuality.Normal)
            {
                if (Trinity.Settings.Loot.Pickup.PickupGrayItems)
                    return true;
                return false;
            }

            // White Items
            if (quality == ItemQuality.Normal || quality == ItemQuality.Superior)
            {
                if (Trinity.Settings.Loot.Pickup.PickupWhiteItems)
                    return true;
                return false;
            }

            if (quality < ItemQuality.Normal && Trinity.Player.Level > 5 && !Trinity.Settings.Loot.Pickup.PickupLowLevel)
            {
                // Grey item, ignore if we're over level 5
                return false;
            }

            // Ignore Gray/White if player level is <= 5 and we are picking up white
            if (quality <= ItemQuality.Normal && Trinity.Player.Level <= 5 && !Trinity.Settings.Loot.Pickup.PickupLowLevel)
            {
                return false;
            }

            if (quality < ItemQuality.Magic1 && Trinity.Player.Level > 10)
            {
                // White item, ignore if we're over level 10
                return false;
            }

            // PickupLowLevel setting
            if (quality <= ItemQuality.Magic1 && Trinity.Player.Level <= 10 && !Trinity.Settings.Loot.Pickup.PickupLowLevel)
            {
                // ignore if we don't have the setting enabled
                return false;
            }

            // Blue/Yellow get scored
            if (quality >= ItemQuality.Magic1 && quality < ItemQuality.Rare4 && !pickupBlue)
            {
                return false;
            }
            if (quality >= ItemQuality.Rare4 && quality < ItemQuality.Legendary && !pickupYellow)
            {
                return false;
            }
            return true;
        }

        internal static TrinityItemType DetermineItemType(ACDItem item)
        {
            return DetermineItemType(item.InternalName, item.ItemType);
        }

        private static readonly Regex ItemExpansionRegex = new Regex(@"^[xp]\d_", RegexOptions.Compiled);

        /// <summary>
        ///     DetermineItemType - Calculates what kind of item it is from D3 internalnames
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbItemType"></param>
        /// <param name="dbFollowerType"></param>
        /// <returns></returns>
        internal static TrinityItemType DetermineItemType(string name, ItemType dbItemType, FollowerType dbFollowerType = FollowerType.None)
        {
            name = name.ToLower();
            if (name.StartsWith("x1_")) name = name.Substring(3, name.Length - 3);
            if (name.StartsWith("p1_")) name = name.Substring(3, name.Length - 3);
            if (name.StartsWith("p2_")) name = name.Substring(3, name.Length - 3);
            if (ItemExpansionRegex.IsMatch(name)) name = name.Substring(3, name.Length - 3);
            
            if (name.StartsWith("demonorgan_")) return TrinityItemType.UberReagent;
            if (name.StartsWith("infernalmachine_")) return TrinityItemType.PortalDevice;
            if (name.StartsWith("a1_")) return TrinityItemType.SpecialItem;
            if (name.StartsWith("amethyst")) return TrinityItemType.Amethyst;
            if (name.StartsWith("amulet_")) return TrinityItemType.Amulet;
            if (name.StartsWith("axe_")) return TrinityItemType.Axe;
            if (name.StartsWith("barbbelt_")) return TrinityItemType.MightyBelt;
            if (name.StartsWith("blacksmithstome")) return TrinityItemType.CraftTome;
            if (name.StartsWith("boots_")) return TrinityItemType.Boots;
            if (name.StartsWith("bow_")) return TrinityItemType.TwoHandBow;
            if (name.StartsWith("bracers_")) return TrinityItemType.Bracer;
            if (name.StartsWith("ceremonialdagger_")) return TrinityItemType.CeremonialKnife;
            if (name.StartsWith("cloak_")) return TrinityItemType.Cloak;
            if (name.StartsWith("combatstaff_")) return TrinityItemType.TwoHandDaibo;
            if (name.StartsWith("crafting_")) return TrinityItemType.CraftingMaterial;
            if (name.StartsWith("craftingmaterials_")) return TrinityItemType.CraftingMaterial;
            if (name.StartsWith("craftingplan_")) return TrinityItemType.CraftingPlan;
            if (name.StartsWith("craftingreagent_legendary_")) return TrinityItemType.CraftingMaterial;
            if (name.StartsWith("crushield_")) return TrinityItemType.CrusaderShield;
            if (name.StartsWith("dagger_")) return TrinityItemType.Dagger;
            if (name.StartsWith("diamond_")) return TrinityItemType.Diamond;
            if (name.StartsWith("dye_")) return TrinityItemType.Dye;
            if (name.StartsWith("emerald_")) return TrinityItemType.Emerald;
            if (name.StartsWith("fistweapon_")) return TrinityItemType.FistWeapon;
            if (name.StartsWith("flail1h_")) return TrinityItemType.Flail;
            if (name.StartsWith("flail2h_")) return TrinityItemType.TwoHandFlail;
            if (name.StartsWith("followeritem_enchantress_") || dbFollowerType == FollowerType.Enchantress) return TrinityItemType.FollowerEnchantress;
            if (name.StartsWith("followeritem_scoundrel_") || dbFollowerType == FollowerType.Scoundrel) return TrinityItemType.FollowerScoundrel;
            if (name.StartsWith("followeritem_templar_") || dbFollowerType == FollowerType.Templar) return TrinityItemType.FollowerTemplar;
            if (name.StartsWith("gloves_")) return TrinityItemType.Gloves;
            if (name.StartsWith("handxbow_")) return TrinityItemType.HandCrossbow;
            if (name.StartsWith("healthglobe")) return TrinityItemType.HealthGlobe;
            if (name.StartsWith("healthpotion")) return TrinityItemType.HealthPotion;
            if (name.StartsWith("horadriccache")) return TrinityItemType.HoradricCache;
            if (name.StartsWith("lore_book_")) return TrinityItemType.CraftTome;
            if (name.StartsWith("lootrunkey")) return TrinityItemType.LootRunKey;
            if (name.StartsWith("mace_")) return TrinityItemType.Mace;
            if (name.StartsWith("mightyweapon_1h_")) return TrinityItemType.MightyWeapon;
            if (name.StartsWith("mightyweapon_2h_")) return TrinityItemType.TwoHandMighty;
            if (name.StartsWith("mojo_")) return TrinityItemType.Mojo;
            if (name.StartsWith("orb_")) return TrinityItemType.Orb;
            if (name.StartsWith("page_of_")) return TrinityItemType.CraftTome;
            if (name.StartsWith("pants_")) return TrinityItemType.Legs;
            if (name.StartsWith("polearm_") || dbItemType == ItemType.Polearm) return TrinityItemType.TwoHandPolearm;
            if (name.StartsWith("quiver_")) return TrinityItemType.Quiver;
            if (name.StartsWith("ring_")) return TrinityItemType.Ring;
            if (name.StartsWith("ruby_")) return TrinityItemType.Ruby;
            if (name.StartsWith("shield_")) return TrinityItemType.Shield;
            if (name.StartsWith("shoulderpads_")) return TrinityItemType.Shoulder;
            if (name.StartsWith("spear_")) return TrinityItemType.Spear;
            if (name.StartsWith("spiritstone_")) return TrinityItemType.SpiritStone;
            if (name.StartsWith("staff_")) return TrinityItemType.TwoHandStaff;
            if (name.StartsWith("staffofcow")) return TrinityItemType.StaffOfHerding;
            if (name.StartsWith("sword_")) return TrinityItemType.Sword;
            if (name.StartsWith("topaz_")) return TrinityItemType.Topaz;
            if (name.StartsWith("twohandedaxe_")) return TrinityItemType.TwoHandAxe;
            if (name.StartsWith("twohandedmace_")) return TrinityItemType.TwoHandMace;
            if (name.StartsWith("twohandedsword_")) return TrinityItemType.TwoHandSword;
            if (name.StartsWith("voodoomask_")) return TrinityItemType.VoodooMask;
            if (name.StartsWith("wand_")) return TrinityItemType.Wand;
            if (name.StartsWith("wizardhat_")) return TrinityItemType.WizardHat;
            if (name.StartsWith("xbow_")) return TrinityItemType.TwoHandCrossbow;
            if (name.StartsWith("console_powerglobe")) return TrinityItemType.PowerGlobe;
            if (name.StartsWith("tiered_rifts_orb")) return TrinityItemType.ProgressionGlobe;
            if (name.StartsWith("normal_rifts_orb")) return TrinityItemType.ProgressionGlobe;            
            if (name.StartsWith("consumable_add_sockets")) return TrinityItemType.ConsumableAddSockets; // Ramaladni's Gift
            if (name.StartsWith("tieredlootrunkey_")) return TrinityItemType.TieredLootrunKey;
            if (name.StartsWith("demonkey_") || name.StartsWith("demontrebuchetkey") || name.StartsWith("quest_")) return TrinityItemType.InfernalKey;
            if (name.StartsWith("offhand_")) return TrinityItemType.Mojo;
            if (name.StartsWith("horadricrelic")) return TrinityItemType.HoradricRelic;


            // Follower item types
            if (name.StartsWith("jewelbox_") || dbItemType == ItemType.FollowerSpecial)
            {
                if (dbFollowerType == FollowerType.Scoundrel)
                    return TrinityItemType.FollowerScoundrel;
                if (dbFollowerType == FollowerType.Templar)
                    return TrinityItemType.FollowerTemplar;
                if (dbFollowerType == FollowerType.Enchantress)
                    return TrinityItemType.FollowerEnchantress;
            }

            // Fall back on some partial DB item type checking 
            if (name.StartsWith("crafting_"))
            {
                if (dbItemType == ItemType.CraftingPage)
                    return TrinityItemType.CraftTome;
                return TrinityItemType.CraftingMaterial;
            }
            if (name.StartsWith("chestarmor_"))
            {
                if (dbItemType == ItemType.Cloak)
                    return TrinityItemType.Cloak;
                return TrinityItemType.Chest;
            }
            if (name.StartsWith("helm_"))
            {
                if (dbItemType == ItemType.SpiritStone)
                    return TrinityItemType.SpiritStone;
                if (dbItemType == ItemType.VoodooMask)
                    return TrinityItemType.VoodooMask;
                if (dbItemType == ItemType.WizardHat)
                    return TrinityItemType.WizardHat;
                return TrinityItemType.Helm;
            }
            if (name.StartsWith("helmcloth_"))
            {
                if (dbItemType == ItemType.SpiritStone)
                    return TrinityItemType.SpiritStone;
                if (dbItemType == ItemType.VoodooMask)
                    return TrinityItemType.VoodooMask;
                if (dbItemType == ItemType.WizardHat)
                    return TrinityItemType.WizardHat;
                return TrinityItemType.Helm;
            }
            if (name.StartsWith("belt_"))
            {
                if (dbItemType == ItemType.MightyBelt)
                    return TrinityItemType.MightyBelt;
                return TrinityItemType.Belt;
            }
            return TrinityItemType.Unknown;
        }

        /// <summary>
        ///     DetermineBaseType - Calculates a more generic, "basic" type of item
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static TrinityItemBaseType DetermineBaseType(TrinityItemType itemType)
        {
            // One Handed Weapons
            switch (itemType)
            {
                case TrinityItemType.Axe:
                case TrinityItemType.CeremonialKnife:
                case TrinityItemType.Dagger:
                case TrinityItemType.Flail:
                case TrinityItemType.FistWeapon:
                case TrinityItemType.Mace:
                case TrinityItemType.MightyWeapon:
                case TrinityItemType.Spear:
                case TrinityItemType.Sword:
                case TrinityItemType.Wand:
                    return TrinityItemBaseType.WeaponOneHand;
                case TrinityItemType.TwoHandDaibo:
                case TrinityItemType.TwoHandMace:
                case TrinityItemType.TwoHandFlail:
                case TrinityItemType.TwoHandMighty:
                case TrinityItemType.TwoHandPolearm:
                case TrinityItemType.TwoHandStaff:
                case TrinityItemType.TwoHandSword:
                case TrinityItemType.TwoHandAxe:
                     return TrinityItemBaseType.WeaponTwoHand;
                case TrinityItemType.TwoHandCrossbow:
                case TrinityItemType.HandCrossbow:
                case TrinityItemType.TwoHandBow:
                    return TrinityItemBaseType.WeaponRange;
                case TrinityItemType.Mojo:
                case TrinityItemType.Orb:
                case TrinityItemType.CrusaderShield:
                case TrinityItemType.Quiver:
                case TrinityItemType.Shield:
                    return TrinityItemBaseType.Offhand;
                case TrinityItemType.Boots:
                case TrinityItemType.Bracer:
                case TrinityItemType.Chest:
                case TrinityItemType.Cloak:
                case TrinityItemType.Gloves:
                case TrinityItemType.Helm:
                case TrinityItemType.Legs:
                case TrinityItemType.Shoulder:
                case TrinityItemType.SpiritStone:
                case TrinityItemType.VoodooMask:
                case TrinityItemType.WizardHat:
                case TrinityItemType.Belt:
                case TrinityItemType.MightyBelt:
                    return TrinityItemBaseType.Armor;
                case TrinityItemType.Amulet:
                case TrinityItemType.Ring:
                    return TrinityItemBaseType.Jewelry;
                case TrinityItemType.FollowerEnchantress:
                case TrinityItemType.FollowerScoundrel:
                case TrinityItemType.FollowerTemplar:
                    return TrinityItemBaseType.FollowerItem;
                case TrinityItemType.CraftingMaterial:
                case TrinityItemType.CraftTome:
                case TrinityItemType.LootRunKey:
                case TrinityItemType.HoradricRelic:
                case TrinityItemType.SpecialItem:
                case TrinityItemType.CraftingPlan:
                case TrinityItemType.HealthPotion:
                case TrinityItemType.HoradricCache:
                case TrinityItemType.Dye:
                case TrinityItemType.StaffOfHerding:
                case TrinityItemType.InfernalKey:
                case TrinityItemType.ConsumableAddSockets:
                case TrinityItemType.TieredLootrunKey:
                    return TrinityItemBaseType.Misc;
                case TrinityItemType.Ruby:
                case TrinityItemType.Emerald:
                case TrinityItemType.Topaz:
                case TrinityItemType.Amethyst:
                case TrinityItemType.Diamond:
                    return TrinityItemBaseType.Gem;
                case TrinityItemType.HealthGlobe:
                    return TrinityItemBaseType.HealthGlobe;
                case TrinityItemType.PowerGlobe:
                    return TrinityItemBaseType.PowerGlobe;
                case TrinityItemType.ProgressionGlobe:
                    return TrinityItemBaseType.ProgressionGlobe;
            }
            return TrinityItemBaseType.Unknown;
        }

        internal static ItemType GItemTypeToItemType(TrinityItemType itemType)
        {
            switch (itemType)
            {
                case TrinityItemType.Axe:
                    return ItemType.Axe;

                case TrinityItemType.Dagger:
                    return ItemType.Dagger;

                case TrinityItemType.Flail:
                    return ItemType.Flail;

                case TrinityItemType.FistWeapon:
                    return ItemType.FistWeapon;

                case TrinityItemType.Mace:
                    return ItemType.Mace;

                case TrinityItemType.MightyWeapon:
                    return ItemType.MightyWeapon;

                case TrinityItemType.Spear:
                    return ItemType.Spear;

                case TrinityItemType.Sword:
                    return ItemType.Sword;

                case TrinityItemType.Wand:
                    return ItemType.Wand;

                case TrinityItemType.HandCrossbow:
                    return ItemType.HandCrossbow;

                case TrinityItemType.CeremonialKnife:
                    return ItemType.CeremonialDagger;

                case TrinityItemType.TwoHandDaibo:
                    return ItemType.Daibo;

                case TrinityItemType.TwoHandMace:
                    return ItemType.Mace;

                case TrinityItemType.TwoHandFlail:
                    return ItemType.Flail;

                case TrinityItemType.TwoHandMighty:
                    return ItemType.MightyWeapon;

                case TrinityItemType.TwoHandPolearm:
                    return ItemType.Polearm;

                case TrinityItemType.TwoHandStaff:
                    return ItemType.Staff;

                case TrinityItemType.TwoHandSword:
                    return ItemType.Sword;

                case TrinityItemType.TwoHandAxe:
                    return ItemType.Axe;

                case TrinityItemType.TwoHandCrossbow:
                    return ItemType.Crossbow;

                case TrinityItemType.TwoHandBow:
                    return ItemType.Bow;

                case TrinityItemType.FollowerEnchantress:
                case TrinityItemType.FollowerScoundrel:
                case TrinityItemType.FollowerTemplar:
                    return ItemType.FollowerSpecial;

                case TrinityItemType.CraftingMaterial:
                    return ItemType.CraftingReagent;

                case TrinityItemType.CraftTome:
                    return ItemType.CraftingPlan;

                case TrinityItemType.HealthPotion:
                case TrinityItemType.Dye:
                case TrinityItemType.ConsumableAddSockets:
                case TrinityItemType.ProgressionGlobe:
                case TrinityItemType.PowerGlobe:
                case TrinityItemType.HealthGlobe:
                    return ItemType.Consumable;

                case TrinityItemType.Ruby:
                case TrinityItemType.Emerald:
                case TrinityItemType.Topaz:
                case TrinityItemType.Amethyst:
                case TrinityItemType.Diamond:
                    return ItemType.Gem;

                case TrinityItemType.LootRunKey:
                case TrinityItemType.HoradricRelic:
                case TrinityItemType.SpecialItem:
                case TrinityItemType.CraftingPlan:
                case TrinityItemType.HoradricCache:
                case TrinityItemType.StaffOfHerding:
                case TrinityItemType.InfernalKey:
                case TrinityItemType.TieredLootrunKey:
                    return ItemType.Unknown;
            }

            ItemType newType;
            if (Enum.TryParse(itemType.ToString(), true, out newType))
                return newType;

            return ItemType.Unknown;
        }

        internal static bool IsEquipment(CachedACDItem i)
        {
            return (i.BaseType == ItemBaseType.Armor || i.BaseType == ItemBaseType.Jewelry || i.BaseType == ItemBaseType.Weapon);
        }

        internal static InventoryItemType GetCraftingMaterialType(CachedACDItem item)
        {
            if (item.ActorSNO == 361989 || item.ActorSNO == 449044)
                return InventoryItemType.DeathsBreath;

            return (InventoryItemType)item.ActorSNO;
        }

        internal static InventoryItemType GetCraftingMaterialType(int actorSnoId)
        {
            if(actorSnoId == 361989 || actorSnoId == 449044)
                return InventoryItemType.DeathsBreath;

            return (InventoryItemType)actorSnoId;

        }

        internal static Func<ACDItem, CachedACDItem, bool> StackItemMatchFunc = (item, cItem) => item.IsValid && item.ActorSnoId == cItem.ActorSNO;
        /// <summary>
        /// Gets the number of items combined in all stacks
        /// </summary>
        /// <param name="cItem">The c item.</param>
        /// <param name="inventorySlot">The inventory slot.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ArgumentException">InventorySlot  + inventorySlot +  is not supported for GetStackCount method</exception>
        internal static int GetItemStackCount(CachedACDItem cItem, InventorySlot inventorySlot)
        {
            try
            {
                switch (inventorySlot)
                {
                    case InventorySlot.BackpackItems:
                        return ZetaDia.Me.Inventory.Backpack.Where(i => StackItemMatchFunc(i, cItem)).Sum(i => i.GetItemStackQuantity());
                    case InventorySlot.SharedStash:
                        return ZetaDia.Me.Inventory.StashItems.Where(i => StackItemMatchFunc(i, cItem)).Sum(i => i.GetItemStackQuantity());
                }
                throw new ArgumentException("InventorySlot " + inventorySlot + " is not supported for GetStackCount method");
            }
            catch (Exception ex)
            {
                Logger.LogDebug("Error Getting ItemStackQuantity {0}", ex);
                return -1;
            }
        }

        internal static void TrinityOnItemStashed(CachedItem item)
        {
            try
            {
                switch (item.ItemBaseType)
                {
                    case ItemBaseType.Gem:
                    case ItemBaseType.Misc:
                        break;
                    default:
                        Helpers.Notifications.LogGoodItems(item, item.TrinityItemBaseType, item.TrinityItemType, 0);
                        ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item.GetAcdItem()), "Stashed");
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex is CoroutineStoppedException)
                    throw;
            }
        }

        internal static void TrinityOnItemSalvaged(CachedItem item)
        {
            try
            {
                switch (item.ItemBaseType)
                {
                    case ItemBaseType.Gem:
                    case ItemBaseType.Misc:
                        break;
                    default:
                        Helpers.Notifications.LogJunkItems(item, item.TrinityItemBaseType, item.TrinityItemType, 0);
                        ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item.GetAcdItem()), "Salvaged");
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex is CoroutineStoppedException)
                    throw;
            }
        }

        internal static void TrinityOnItemSold(CachedItem item)
        {
            try
            {
                switch (item.ItemBaseType)
                {
                    case ItemBaseType.Gem:
                    case ItemBaseType.Misc:
                        break;
                    default:
                        Helpers.Notifications.LogJunkItems(item, item.TrinityItemBaseType, item.TrinityItemType, 0);
                        ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item.GetAcdItem()), "Sold");
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex is CoroutineStoppedException)
                    throw;
            }
        }

    //internal static void TrinityOnOnItemIdentificationRequest(object sender, ItemIdentifyRequestEventArgs e)
    //{
    //    Logger.Log(LogCategory.ItemValuation, "DB is requesting Identification Item={0} IsInTown={1} UseBook={2} ItemMode={3}", 
    //        e.Item.InternalName, ZetaDia.IsInTown, CharacterSettings.Instance.UseBookOfCain, 
    //        Trinity.Settings.Loot.ItemFilterMode);

    //    if (Trinity.Settings.Loot.TownRun.DropInTownOption == Settings.Loot.DropInTownOption.All)
    //    {
    //        ItemDropper.Drop(e.Item);
    //    }                

    //    e.IgnoreIdentification = !TrinityItemManager.ItemRulesIdentifyValidation(e.Item);
    //}

    //internal static void ResetTownRun()
    //{
    //    ItemValuation.ResetValuationStatStrings();
    //    TownRun.TownRunCheckTimer.Reset();
    //    Trinity.ForceVendorRunASAP = false;
    //    Trinity.WantToTownRun = false;
    //}

    //internal static void TrinityOnItemDropped(object sender, ItemEventArgs e)
    //{
    //    //Logger.Log("Dropped {0} ({1})", e.Item.Name, e.Item.ActorSnoId);          
    //}


}
}