﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Talon //RL144
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} //메뉴얼 오브워커 넣기는 했지만. 음.. 
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

		
        static float getQBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "TalonNoxianDiplomacyBuff"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getEBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "TalonDamageAmp"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "TalonDisappear"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 500f, TargetSelector.DamageType.Physical);

            W.SetSkillshot(0.25f, 60f * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            E.SetTargetted(0.25f, float.MaxValue);
			
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();


            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addERange();
            AIO_Menu.Champion.Drawings.addRRange();
            AIO_Menu.Champion.Drawings.addItem("Q Timer", new Circle(true, Color.LightGreen));
            AIO_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.LightGreen));
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.LightGreen));
			
			AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
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
            if (AIO_Menu.Champion.Misc.UseKillsteal)
            Killsteal();
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

		var drawW = AIO_Menu.Champion.Drawings.WRange;
		var drawE = AIO_Menu.Champion.Drawings.ERange;
		var drawR = AIO_Menu.Champion.Drawings.RRange;
		var drawQTimer = AIO_Menu.Champion.Drawings.getCircleValue("Q Timer");
		var drawETimer = AIO_Menu.Champion.Drawings.getCircleValue("E Timer");
		var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
	
		if (W.IsReady() && drawW.Active)
		Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
	
		if (E.IsReady() && drawE.Active)
		Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
		
		if (R.IsReady() && drawR.Active)
		Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
		
		var pos_temp = Drawing.WorldToScreen(Player.Position);
		
		if (drawQTimer.Active && getQBuffDuration > 0)
		Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));
		
		if (drawETimer.Active && getEBuffDuration > 0)
		Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E: " + getEBuffDuration.ToString("0.00"));
		
		if (drawRTimer.Active && getRBuffDuration > 0)
		Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
        }
		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;


        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;


        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;
				

        }
		
		static void AA() // 챔피언 대상 평캔 ( 빼낸 이유는 AA방식 두개로 할시 두번 적어야 해서 단순화하기 위함.
		{
			var target = TargetSelector.GetTarget(Player.AttackRange + 50,TargetSelector.DamageType.Physical, true); //탈론은 타겟이 필요한건 아니지만. Base로 쓰기 위해.
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
			{
				if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady() && utility.Activator.AfterAttack.ALLCancleItemsAreCasted
					&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
					Q.Cast();
			}
				
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
			{
				if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady() && utility.Activator.AfterAttack.ALLCancleItemsAreCasted
					&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
					Q.Cast();					
			}
		}
		
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
			AIO_Func.AALcJc(Q);
			if(!utility.Activator.AfterAttack.AIO)
			AA();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, W.DamageType, true);
				if(eTarget.Distance(Player.Position) > 600 || eTarget.Distance(Player.Position) <= 600 && !W.IsReady())
                E.Cast(eTarget);
            }
			
            if (AIO_Menu.Champion.Combo.UseW && W.IsReady() && !AIO_Func.AfterAttack())
            {
                var wTarget = TargetSelector.GetTarget(W.Range, W.DamageType, true);
        
                if (wTarget != null && !Player.IsDashing())
                    W.Cast(wTarget);       
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady()
			&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
            {
                R.Cast();
            }
				
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
				
            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
               
                var wTarget = TargetSelector.GetTarget(W.Range, W.DamageType, true);

        
                if (wTarget != null && !Player.IsDashing())
                    W.Cast(wTarget);       
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
                W.Cast(Minions[0]);

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
                E.Cast(Minions[0]);
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
                W.Cast(Mobs[0]);

            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
                E.Cast(Mobs[0]);
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
			{
                damage += Q.GetDamage(enemy);
                damage += (float)Player.GetAutoAttackDamage(enemy, true) * 1.1f;
			}
			
            if (Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
			{
			damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
			damage += (float)Player.GetAutoAttackDamage(enemy, true) * 1.1f;
			}
			
            if (Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
			{
			damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
			damage += (float)Player.GetAutoAttackDamage(enemy, true) * 1.1f;
			}

            if (W.IsReady())
                damage += W.GetDamage(enemy);

            if (R.IsReady() && AIO_Menu.Champion.Combo.UseR)
                damage += R.GetDamage(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true) * 1.1f;

            if (E.IsReady())

                damage = damage *(1 + 0.03f * E.Level);
				
            return damage;
        }
    }
}
