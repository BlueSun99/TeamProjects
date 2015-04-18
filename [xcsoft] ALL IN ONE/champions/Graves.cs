﻿using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;


using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Graves
    {
        
        static Orbwalking.Orbwalker Orbwalker { get { return ALL_IN_ONE_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

      
        static Spell Q, W, E, R;

        public static void Load()
        {
            
            Q = new Spell(SpellSlot.Q, 720f);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, 1100f);

           
            Q.SetSkillshot(0.25f, 15f * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            W.SetSkillshot(0.25f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);


            ALL_IN_ONE_Menu.Champion.Combo.addUseQ();
            ALL_IN_ONE_Menu.Champion.Combo.addUseW();
            ALL_IN_ONE_Menu.Champion.Combo.addUseR();


            ALL_IN_ONE_Menu.Champion.Harass.addUseQ();
            ALL_IN_ONE_Menu.Champion.Harass.addIfMana(70);

            ALL_IN_ONE_Menu.Champion.Laneclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Laneclear.addIfMana(80);

            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addIfMana(80);

            ALL_IN_ONE_Menu.Champion.Misc.addItem("Use E on Gap Closer", true);
            ALL_IN_ONE_Menu.Champion.Misc.addUseKillsteal();

            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addERange();
            ALL_IN_ONE_Menu.Champion.Drawings.addRRange();

           
            ALL_IN_ONE_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);


            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
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

           
            if (ALL_IN_ONE_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {

            if (Player.IsDead)
                return;

            var drawQ = ALL_IN_ONE_Menu.Champion.Drawings.QRange;
            var drawW = ALL_IN_ONE_Menu.Champion.Drawings.WRange;
            var drawE = ALL_IN_ONE_Menu.Champion.Drawings.ERange;
            var drawR = ALL_IN_ONE_Menu.Champion.Drawings.RRange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (target.Type != GameObjectType.obj_AI_Hero)
                return;

            var Target = (Obj_AI_Base)target;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.CanCast(Target) && !Player.IsDashing())
                {
                    Q.Cast(Target);
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.CanCast(Target) && !Player.IsDashing() && !(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Harass.IfMana))
                    Q.Cast(Target);
            }
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) 
            // cradits to Asuna & ijabba
        {
            if (!ALL_IN_ONE_Menu.Champion.Misc.getBoolValue("Use E on Gap Closer") || Player.IsDead)
                return;

            var extended = gapcloser.Start.Extend(Player.Position, gapcloser.Start.Distance(Player.ServerPosition) + E.Range);

            if (IsSafePosition(extended))
            {
                E.Cast(extended);
            }
        }

        static void Combo()
        {
            

            if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
               
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);

        
                if (qTarget != null && !Player.IsDashing())
                    Q.Cast(qTarget);       
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var wTarget = TargetSelector.GetTarget(W.Range, W.DamageType, true);

                if (wTarget != null && !Player.IsDashing())
                    W.Cast(wTarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = HeroManager.Enemies.Where(x => R.CanCast(x) && x.Health + (x.HPRegenRate / 2) <= R.GetDamage(x) && R.GetPrediction(x).Hitchance >= HitChance.VeryHigh).OrderByDescending(x => x.Health).FirstOrDefault();

                if (R.CanCast(Rtarget))
                    R.Cast(Rtarget);
            }
        }

        static void Harass()
        {
           
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Harass.IfMana))
                return;

            if (ALL_IN_ONE_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);


                if (qTarget != null && !Player.IsDashing())
                    Q.Cast(qTarget);       
            }
        }

        static void Laneclear()
        {

            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var Farmloc = Q.GetLineFarmLocation(Minions);

                if (Farmloc.MinionsHit >= 6)
                    Q.Cast(Farmloc.Position);
            }

        }

        static void Jungleclear()
        {
            
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Jungleclear.IfMana))
                return;


            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
               
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault());
            }

        }

        static void Killsteal()
        {
           
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                
                if (Q.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, W))
                    W.Cast(target);

                if (R.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        public static bool IsSafePosition(Vector3 position) // cradits to Asuna & ijabba
        {
            if (position.UnderTurret(true) && !ObjectManager.Player.UnderTurret(true))
            {
                return false;
            }
            var allies = position.CountAlliesInRange(ObjectManager.Player.AttackRange);
            var enemies = position.CountEnemiesInRange(ObjectManager.Player.AttackRange);
            var lhEnemies = GetLhEnemiesNearPosition(position, ObjectManager.Player.AttackRange).Count();

            if (enemies == 1) 
            {
                return true;
            }

           
            return (allies + 1 > enemies - lhEnemies);
        }
        public static List<Obj_AI_Hero> GetLhEnemiesNearPosition(Vector3 position, float range)
        // cradits to Asuna & ijabba
        {
            return
                HeroManager.Enemies.Where(
                    hero => hero.IsValidTarget(range, true, position) && ALL_IN_ONE_Func.getHealthPercent(hero) <= 15).ToList();
        }


        static float getComboDamage(Obj_AI_Base enemy)
        {
            
            float damage = 0;


            if (R.IsReady())
                damage += R.GetDamage(enemy);

            return damage;
        }
    }
}
