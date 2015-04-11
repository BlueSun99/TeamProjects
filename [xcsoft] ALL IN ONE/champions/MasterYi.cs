using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class MasterYi
    {
        static Menu Menu { get { return xcsoftMenu.Menu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Items.Item tiamatItem, hydraItem;
		
        static Spell Q, W, E, R;

        static void Wcancel() { Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos); }

        static float getRBuffDuration { get { var buff = xcsoftFunc.getBuffInstance(Player, "Highlander"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.25f, float.MaxValue);
            hydraItem = new Items.Item((int)ItemId.Ravenous_Hydra_Melee_Only, 250f);
            tiamatItem = new Items.Item((int)ItemId.Tiamat_Melee_Only, 250f);

            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseW", "Use W (Auto-Attack Reset)", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseH", "Use Hydra", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseH", "Use Hydra", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseH", "Use Hydra", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawRTimer", "R Timer", true).SetValue(new Circle(true, Color.LightGreen)));

			xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

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

			Orbwalker.SetAttack(Player.IsTargetable);

            #region Killsteal
            if (Menu.Item("miscKs", true).GetValue<bool>())
                Killsteal();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = Menu.Item("drawQ", true).GetValue<Circle>();
            var drawRTimer = Menu.Item("drawRTimer", true).GetValue<Circle>();

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (drawRTimer.Active && getRBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
            }
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && args.Target.Type != GameObjectType.obj_AI_Minion)
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.W).Name
                    && HeroManager.Enemies.Any(x => x.IsValidTarget(Q.Range)))
                {
                    if (Menu.Item("CbUseW", true).GetValue<bool>())
                    {
                        Utility.DelayAction.Add(50, Orbwalking.ResetAutoAttackTimer);
                        Utility.DelayAction.Add(50, Wcancel);
                    }
                }

                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.Q).Name)
                {
                    if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
                        E.Cast();
                }
            }

        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;

            if (!unit.IsMe || Target == null)
                return;
                
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
		
		if(Mobs.Count + Minions.Count <= 0)
            	return;
            	if(Mobs.Count >= 1)
            	AAJungleclear();
            	if(Minions.Count >= 1)
            	AALaneclear();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
	                if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady()
	                    && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x))
				&& !tiamatItem.IsReady() && !hydraItem.IsReady())
	                    W.Cast();
	
	                if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
						R.Cast();
					
			if (Menu.Item("CbUseH", true).GetValue<bool>()
			&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
			{
				if(tiamatItem.IsReady())
					tiamatItem.Cast();
				else if(hydraItem.IsReady())
					hydraItem.Cast();
			}
		}
        }

        static void AALaneclear()
        {
        	 if (!(xcsoftFunc.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

		var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

		if (Minions.Count <= 0)
                return;
				

		if (Menu.Item("LcUseH", true).GetValue<bool>())
		{
			if(tiamatItem.IsReady())
				tiamatItem.Cast();
			else if(hydraItem.IsReady())
				hydraItem.Cast();
		}
        }

        static void AAJungleclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
				
	
		if (Menu.Item("JcUseH", true).GetValue<bool>())
		{
			if(tiamatItem.IsReady())
				tiamatItem.Cast();
			else if(hydraItem.IsReady())
				hydraItem.Cast();
		}	
        }

        static void Combo()
        {
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.CastOnBestTarget();

            if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady() 
                && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();
        }

        static void Harass()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;

            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.CastOnBestTarget();
        }

        static void Laneclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(Minions[0]);
        }

        static void Jungleclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(Mobs[0]);

            if (Menu.Item("JcUseE", true).GetValue<bool>() && E.IsReady() 
                && Mobs.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true) + (float)Player.GetAutoAttackDamage(enemy, false) * 5;
				
            return damage;
        }
    }
}

