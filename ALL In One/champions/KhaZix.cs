﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class KhaZix// By RL244
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float WD = 50f;
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 325f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 600f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.25f, float.MaxValue);
            W.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 150f, 1400f, false, SkillshotType.SkillshotCircle);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR(false);

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE(false);
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            #endregion
            #region AfterAttack
            AIO_Func.AASkill(Q);
            if(AIO_Func.AfterAttack())
            AA();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        }
        
        static void AA()
        {
            AIO_Func.AACb(Q);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;
            AIO_Func.AALcJc(Q);
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                if(Qtarget.Distance(Player.ServerPosition) > Orbwalking.GetRealAutoAttackRange(Player) + 50)
                Q.Cast(Qtarget);
            }
            
            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                AIO_Func.LCast(W,Wtarget,WD,0f);
            }
            
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, E.DamageType);
                AIO_Func.CCast(E,Etarget);
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                if(AIO_Func.getHealthPercent(Player) < 60 || Rtarget.Distance(Player.ServerPosition) < 600)
                R.Cast();
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                if(Qtarget.Distance(Player.ServerPosition) > Orbwalking.GetRealAutoAttackRange(Player) + 50)
                Q.Cast(Qtarget);
            }
            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                AIO_Func.LCast(W,Wtarget,WD,0f);
            }
            
            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, E.DamageType);
                AIO_Func.CCast(E,Etarget);
            }
        }
        
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;
            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
                AIO_Func.LCast(W,Minions[0],WD,0f);
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
                AIO_Func.LCast(W,Mobs[0],WD,0f);
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);
            }
        }
        
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    AIO_Func.LCast(W,target,WD,0f);
            }
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    AIO_Func.CCast(E,target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);
            
            if (W.IsReady())
                damage += W.GetDamage(enemy);
            
            if (E.IsReady())
                damage += E.GetDamage(enemy);
                
            return damage;
        }
    }
}
