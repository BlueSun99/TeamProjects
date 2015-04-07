﻿using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE
{
    static class xcsoftFunc//by xcsoft
    {
        internal static float getHealthPercent(Obj_AI_Base unit)
        {
            return unit.Health / unit.MaxHealth * 100;
        }

        internal static float getManaPercent(Obj_AI_Base unit)
        {
            return unit.Mana / unit.MaxMana * 100;
        }

        internal static List<Obj_AI_Base> getCollisionMinions(Obj_AI_Hero source, SharpDX.Vector3 targetPos, float predDelay, float predWidth, float predSpeed)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = predWidth,
                Delay = predDelay,
                Speed = predSpeed,
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return Collision.GetCollision(new List<SharpDX.Vector3> { targetPos }, input).OrderBy(obj => obj.Distance(source, false)).ToList();
        }

        internal static BuffInstance getBuffInstance(string buffName)
        {
            return ObjectManager.Player.Buffs.Find(x => x.Name == buffName && x.IsValidBuff());
        }

        internal static BuffInstance getBuffInstance(string buffName, Obj_AI_Base buffCaster)
        {
            return ObjectManager.Player.Buffs.Find(x => x.Name == buffName && x.Caster.NetworkId == buffCaster.NetworkId && x.IsValidBuff());
        }

        internal static bool isKillable(Obj_AI_Base target, float damage)
        {
            return target.Health + target.HPRegenRate <= damage;
        }

        internal static bool isKillable(Obj_AI_Base target, Spell spell, int stage = 0)
        {
            return target.Health + (target.HPRegenRate/2) <= spell.GetDamage(target, stage);
        }

        internal static void sendDebugMsg(string message)
        {
            Console.WriteLine(message);
        }

        internal static bool anyoneValidInRange(float range)
        {
            return HeroManager.Enemies.Any(x => x.IsValidTarget(range));
        }

        internal static String colorChat(System.Drawing.Color color, String text) 
        { 
            return "<font color = \"" + colorToHex(color) + "\">" + text + "</font>"; 
        }

        internal static String colorToHex(System.Drawing.Color c)
        { 
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2"); 
        }

        internal static void champSupportedCheck(string tag, string checkNamespace)
        {
            try
            {
                xcsoftFunc.sendDebugMsg(tag + Type.GetType(checkNamespace + ObjectManager.Player.ChampionName).Name + " Supported.");
            }
            catch
            {
                xcsoftFunc.sendDebugMsg(tag + ObjectManager.Player.ChampionName + " Not supported.");
                Game.PrintChat(xcsoftFunc.colorChat(System.Drawing.Color.MediumBlue, tag) + xcsoftFunc.colorChat(System.Drawing.Color.DarkGray, ObjectManager.Player.ChampionName) + " Not supported.");

                xcsoftMenu.addItem("Sorry, " + ObjectManager.Player.ChampionName + " Not supported");
                return;
            }
        }
    }
}
