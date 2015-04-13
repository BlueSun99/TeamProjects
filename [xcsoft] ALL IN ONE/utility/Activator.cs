﻿using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE.utility
{
    class Activator
    {
        //아이템 목록 && 설명
        //http://lol.inven.co.kr/dataninfo/item/list.php
        //http://www.lolking.net/items/
        //https://mirror.enha.kr/wiki/%EB%A6%AC%EA%B7%B8%20%EC%98%A4%EB%B8%8C%20%EB%A0%88%EC%A0%84%EB%93%9C/%EA%B3%B5%EA%B2%A9%20%EC%95%84%EC%9D%B4%ED%85%9C
        //http://leagueoflegends.wikia.com/wiki/Category:Items

        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        internal static Menu Menu;

        internal static void Load()
        {
            Menu = new Menu("[xcsoft] AIO: Activator", "xcsoft_AIOActivator", true);
            Menu.AddToMainMenu();

            Menu.AddSubMenu(new Menu("Activator: Auto-Potion", "AutoPotion"));
            Menu.AddSubMenu(new Menu("Activator: Auto-Spell", "AutoSpell"));
            Menu.AddSubMenu(new Menu("Activator: ComboMode", "ComboMode"));
            Menu.AddSubMenu(new Menu("Activator: BeforeAttack", "BeforeAttack"));
            Menu.AddSubMenu(new Menu("Activator: AfterAttack", "AfterAttack"));

            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.Use Health Potion", "Use Health Potion")).SetValue(true);
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.HealthPercent", "Use Health Potion")).SetValue(new Slider(50, 0, 100));
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.Use Mana Potion", "Use Mana Potion")).SetValue(true);
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.ManaPercent", "Use Mana Potion")).SetValue(new Slider(50,0,100));

            Menu.SubMenu("AutoSpell").AddItem(new MenuItem("AutoSpell.Use Heal", "Use Heal")).SetValue(true);
            Menu.SubMenu("AutoSpell").AddItem(new MenuItem("AutoSpell.Use Ignite", "Use Ignite")).SetValue(true);

            additems();
            addPotions();

            Game.OnUpdate += OnUpdate.Game_OnUpdate;
            Orbwalking.BeforeAttack += BeforeAttack.Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack.Orbwalking_AfterAttack;
        }

        static void additems()
        {
            BeforeAttack.additem("Youmuu", (int)ItemId.Youmuus_Ghostblade, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player));

            AfterAttack.additem("Tiamat", (int)ItemId.Tiamat_Melee_Only, 250f);
            AfterAttack.additem("Hydra", (int)ItemId.Ravenous_Hydra_Melee_Only, 250f);
            AfterAttack.additem("Bilgewater", (int)ItemId.Bilgewater_Cutlass, 450f);
            AfterAttack.additem("BoTRK", (int)ItemId.Blade_of_the_Ruined_King, 450f);
        }

        static void addPotions()
        {
            potions = new List<Potion>
            {
                new Potion
                {
                    Name = "ItemCrystalFlask",
                    MinCharges = 1,
                    ItemId = (ItemId) 2041,
                    Priority = 1,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "RegenerationPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2003,
                    Priority = 2,
                    TypeList = new List<PotionType> {PotionType.Health}
                },
                new Potion
                {
                    Name = "ItemMiniRegenPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2010,
                    Priority = 4,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "FlaskOfCrystalWater",
                    MinCharges = 0,
                    ItemId = (ItemId) 2004,
                    Priority = 3,
                    TypeList = new List<PotionType> {PotionType.Mana}
                }
            };
        }

        //PotionManager part of Marksman
        static List<Potion> potions;
        static List<Items.Item> PotionList = new List<Items.Item>();
        
        enum PotionType
        {
            Health, Mana
        };

        class Potion
        {
            internal string Name { get; set; }
            internal int MinCharges { get; set; }
            internal ItemId ItemId { get; set; }
            internal int Priority { get; set; }
            internal List<PotionType> TypeList { get; set; }
        }

        static InventorySlot GetPotionSlot(PotionType type)
        {
            return (from potion in potions
                    where potion.TypeList.Contains(type)
                    from item in ObjectManager.Player.InventoryItems
                    where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
                    select item).FirstOrDefault();
        }

        static bool IsBuffActive(PotionType type)
        {
            return (from potion in potions
                    where potion.TypeList.Contains(type)
                    from buff in ObjectManager.Player.Buffs
                    where buff.Name == potion.Name && buff.IsActive
                    select potion).Any();
        }

        internal class OnUpdate
        {
            internal static List<Items.Item> itemsList = new List<Items.Item>();

            internal static void additem(string itemName, int itemId, float itemRange)
            {
                itemsList.Add(new Items.Item(itemId, itemRange));

                Menu.SubMenu("OnUpdate").AddItem(new MenuItem("OnUpdate.Use " + itemId.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Game_OnUpdate(EventArgs args)
            {
                if (ObjectManager.Player.IsDead)
                    return;

                if(!ObjectManager.Player.IsRecalling() && !ObjectManager.Player.InFountain())
                {
                    if (Menu.Item("AutoPotion.Use Health Potion").GetValue<bool>())
                    {
                        if (ObjectManager.Player.HealthPercent <= Menu.Item("AutoPotion.HealthPercent").GetValue<Slider>().Value)
                        {
                            var healthSlot = GetPotionSlot(PotionType.Health);

                            if (!IsBuffActive(PotionType.Health))
                                ObjectManager.Player.Spellbook.CastSpell(healthSlot.SpellSlot);
                        }
                    }

                    if (Menu.Item("AutoPotion.Use Mana Potion").GetValue<bool>())
                    {
                        if (ObjectManager.Player.ManaPercent <= Menu.Item("AutoPotion.ManaPercent").GetValue<Slider>().Value)
                        {
                            var manaSlot = GetPotionSlot(PotionType.Mana);

                            if (!IsBuffActive(PotionType.Mana))
                                ObjectManager.Player.Spellbook.CastSpell(manaSlot.SpellSlot);
                        }
                    }
                }

            }
        }

        internal class BeforeAttack
        {
            internal static List<Items.Item> itemsList = new List<Items.Item>();

            internal static void additem(string itemName, int itemId, float itemRange)
            {
                itemsList.Add(new Items.Item(itemId, itemRange));

                Menu.SubMenu("BeforeAttack").AddItem(new MenuItem("BeforeAttack.Use " + itemId.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
            {
                if (!args.Unit.IsMe || args.Target == null || args.Unit.IsDead || args.Target.IsDead || args.Target.Type != GameObjectType.obj_AI_Hero)
                    return;

                foreach (var item in BeforeAttack.itemsList.Where(x => x.IsReady() && x.IsInRange(args.Target.Position) && Menu.Item("BeforeAttack.Use " + x.Id.ToString()).GetValue<bool>()))
                {
                    if (!item.Cast()) 
                        item.Cast((Obj_AI_Base)args.Target);
                }
            }
        }

        internal class AfterAttack
        {
            internal static List<Items.Item> itemsList = new List<Items.Item>();
            internal static bool AllitemsAreCasted { get { return !utility.Activator.AfterAttack.itemsList.Any(x => x.IsReady() && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>()); } }

            internal static void additem(string itemName, int itemId, float itemRange)
            {
                itemsList.Add(new Items.Item(itemId, itemRange));

                Menu.SubMenu("AfterAttack").AddItem(new MenuItem("AfterAttack.Use " + itemId.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
            {
                if (!unit.IsMe || target == null || target.IsDead || unit.IsDead || (target.Type != GameObjectType.obj_AI_Minion && target.Type != GameObjectType.obj_AI_Hero))
                    return;

                var itemone = AfterAttack.itemsList.FirstOrDefault(x => x.IsReady() && x.IsInRange(target.Position) && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>());

                if (itemone != null)
                {
                    if (!itemone.Cast())
                        itemone.Cast((Obj_AI_Base)target);
                }
            }
        }
    }
}
