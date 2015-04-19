using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Veigar // rl244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return ALL_IN_ONE_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 950f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 1040f);
            R = new Spell(SpellSlot.R, 650f, TargetSelector.DamageType.Magical);

			
            Q.SetSkillshot(0.25f, 70f, 2000f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.25f, 112.5f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.75f, 340f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetTargetted(0.25f, 1400f);
            
            ALL_IN_ONE_Menu.Champion.Combo.addUseQ();
            ALL_IN_ONE_Menu.Champion.Combo.addUseW();
            ALL_IN_ONE_Menu.Champion.Combo.addUseE();
            ALL_IN_ONE_Menu.Champion.Combo.addUseR();

            ALL_IN_ONE_Menu.Champion.Harass.addUseQ();
            ALL_IN_ONE_Menu.Champion.Harass.addUseW(false);
            ALL_IN_ONE_Menu.Champion.Harass.addUseE(false);

            ALL_IN_ONE_Menu.Champion.Laneclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseW(false);

            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseW(false);
			
	    ALL_IN_ONE_Menu.Champion.Lasthit.isEmpty();

            ALL_IN_ONE_Menu.Champion.Misc.addHitchanceSelector();
            ALL_IN_ONE_Menu.Champion.Misc.addItem("Made by RL244", true);
            ALL_IN_ONE_Menu.Champion.Misc.addItem("KillstealQ", true);
            ALL_IN_ONE_Menu.Champion.Misc.addItem("KillstealR", true);
            ALL_IN_ONE_Menu.Champion.Misc.addUseAntiGapcloser();
            ALL_IN_ONE_Menu.Champion.Misc.addUseInterrupter();

            ALL_IN_ONE_Menu.Champion.Drawings.addQrange();
            ALL_IN_ONE_Menu.Champion.Drawings.addWrange();
            ALL_IN_ONE_Menu.Champion.Drawings.addErange(false);
            ALL_IN_ONE_Menu.Champion.Drawings.addItem("E Real Range", new Circle(true, Color.Green));
            ALL_IN_ONE_Menu.Champion.Drawings.addRrange();

            ALL_IN_ONE_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
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

            if (ALL_IN_ONE_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (ALL_IN_ONE_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
				
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = ALL_IN_ONE_Menu.Champion.Drawings.DrawQRange;
            var drawW = ALL_IN_ONE_Menu.Champion.Drawings.DrawWRange;
            var drawE = ALL_IN_ONE_Menu.Champion.Drawings.DrawERange;
	    var drawEr = ALL_IN_ONE_Menu.Champion.Drawings.getCircleValue("E Real Range");
            var drawR = ALL_IN_ONE_Menu.Champion.Drawings.DrawRRange;
	    var etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed * E.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (E.IsReady() && drawEr.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range - etarget.MoveSpeed*E.Delay, drawE.Color);
				
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady()
		&& Player.Distance(gapcloser.Sender.Position) <= E.Range + Player.MoveSpeed*E.Delay)
                castE(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.IsReady()
		&& Player.Distance(sender.Position) <= E.Range)
                castE(sender);
        }

		
        static void Combo()
        {

            if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
		var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                CastQ(Qtarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseE && E.IsReady())
            {
		var Etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed*E.Delay, TargetSelector.DamageType.Magical);
                castE(Etarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
            {
		var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
		var pred = W.GetPrediction(Wtarget);
			if (pred.Hitchance == HitChance.Immobile || Wtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay - 0.3f && W.IsReady())
			W.Cast(Wtarget, false, true);

            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseR && R.IsReady())
            {
		var Rtarget = TargetSelector.GetTarget(R.Range, R.DamageType);
		
		if(ALL_IN_ONE_Func.isKillable(Rtarget, R) || Rtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= R.Delay && R.IsReady())
               { 
		if (HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
        	 R.Cast(Rtarget);
		}
            }
        }

        static void Harass()
        {
            if (ALL_IN_ONE_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
		var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                CastQ(Qtarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseE && E.IsReady())
            {
		var Etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed*E.Delay, TargetSelector.DamageType.Magical);
                castE(Etarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseW && W.IsReady())
            {
		var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
		var pred = W.GetPrediction(Wtarget);
		if (pred.Hitchance == HitChance.Immobile || Wtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay - 0.3f && W.IsReady())
		W.Cast(Wtarget, false, true);
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
		var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);			
                if (_m != null)
                    CastQ(_m);
            }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(W.Range)))
                    W.Cast(Minions[0], false, true);
            }

        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    CastQ(Mobs.FirstOrDefault());
            }

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(W.Range)))
                    W.Cast(Mobs[0], false, true);
            }

        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, Q))
                    CastQ(target);
            }
        }
		
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }
		
        static void CastQ(Obj_AI_Hero target)
        {
            var qpred = Q.GetPrediction(target, true);
            var qcollision = Q.GetCollision(Player.ServerPosition.To2D(), new List<Vector2> { qpred.CastPosition.To2D() });
            var minioncol = qcollision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
			if (target.IsValidTarget(Q.Range - target.MoveSpeed*(Q.Delay +Player.Distance(target.Position)/Q.Speed) + 50) && minioncol <= 1 && qpred.Hitchance >= ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance)
            {
            Q.Cast(qpred.CastPosition);
            }
	}

        static void CastQ(Obj_AI_Base target)
        {
            var prediction = Q.GetPrediction(target, true);
            var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);

            if (minions <= 1 && prediction.Hitchance >= ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance)
		{
            	Q.Cast(prediction.CastPosition);
		}
	}
		
        static void castE(Obj_AI_Base target) //E CAST(base)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() +
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
            Vector2 castVec2 = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
							  
            if (pred.Hitchance >= ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance && E.IsReady() && Vector3.Distance(Player.Position, pred.UnitPosition) <= 700 - E.Width)
            {
                E.Cast(castVec, false);
            }
            if (pred.Hitchance >= ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance && E.IsReady() && Vector3.Distance(Player.Position, pred.UnitPosition) > 700 - E.Width)
            {
                E.Cast(castVec2, false);
            }
        }

        
        static void castE(Obj_AI_Hero target) //E CAST(hero)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() +
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
            Vector2 castVec2 = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
							  
            if (pred.Hitchance >= ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance && E.IsReady() && Vector3.Distance(Player.Position, pred.UnitPosition) <= 700 - E.Width)
            {
                E.Cast(castVec, false);
            }
            if (pred.Hitchance >= ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance && E.IsReady() && Vector3.Distance(Player.Position, pred.UnitPosition) > 700 - E.Width)
            {
                E.Cast(castVec2, false);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if (W.IsReady())
                damage += W.GetDamage(enemy);

            if (R.IsReady())
                damage += R.GetDamage(enemy);
				
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
            return damage;
        }
		
		
    }
}
