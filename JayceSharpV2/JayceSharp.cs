using System;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

/*
 * ToDo:
 * Q doesnt shoot much < fixed
 * Full combo burst <-- done
 * Useles gate <-- fixed
 * Add Fulldmg combo starting from hammer <-- done
 * Auto ignite if killabe/burst <-- done
 * More advanced Q calc area on hit
 * MuraMune support <-- done
 * Auto gapclosers E <-- done
 * GhostBBlade active <-- done
 * packet cast E <-- done 
 * 
 * 
 * Auto ks with QE <-done
 * Interupt channel spells <-done
 * Omen support <- done
 * 
 * 
 * */

namespace JayceSharpV2
{
    internal class JayceSharp
    {
        public const string CharName = "Jayce";
        public static Menu Config;
        public static HpBarIndicator Hpi = new HpBarIndicator();

        public JayceSharp()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoad = new Thread(Game_OnGameLoad);
                onGameLoad.Start();
            };
        }

        private static void Game_OnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != CharName)
            {
                return;
            }

            Game.PrintChat("Jayce - SharpV2 by DeTuKs");
            Jayce.SetSkillShots();
            try
            {
                Config = new Menu("Jayce - Sharp", "Jayce", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                Jayce.Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

                //TS
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);

                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("hammerKill", "Hammer if killable")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("parlelE", "use pralel gate")).SetValue(true);
                Config.SubMenu("combo")
                    .AddItem(new MenuItem("fullDMG", "Do full damage"))
                    .SetValue(new KeyBind('A', KeyBindType.Press));
                Config.SubMenu("combo")
                    .AddItem(new MenuItem("injTarget", "Tower Injection"))
                    .SetValue(new KeyBind('G', KeyBindType.Press));
                Config.SubMenu("combo")
                    .AddItem(new MenuItem("awsPress", "Press for awsomeee!!"))
                    .SetValue(new KeyBind('Z', KeyBindType.Press));
                Config.SubMenu("combo")
                    .AddItem(new MenuItem("eAway", "Gate distance from side"))
                    .SetValue(new Slider(20, 3, 60));

                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));
                Config.SubMenu("extra")
                    .AddItem(new MenuItem("shoot", "Shoot manual Q"))
                    .SetValue(new KeyBind('T', KeyBindType.Press));
                Config.SubMenu("extra").AddItem(new MenuItem("gapClose", "Kick Gapclosers")).SetValue(true);
                Config.SubMenu("extra").AddItem(new MenuItem("autoInter", "Interupt spells")).SetValue(true);
                Config.SubMenu("extra").AddItem(new MenuItem("useMunions", "Q use Minion colision")).SetValue(true);
                Config.SubMenu("extra").AddItem(new MenuItem("killSteal", "Killsteal")).SetValue(false);
                Config.SubMenu("extra").AddItem(new MenuItem("packets", "Use Packet cast")).SetValue(false);

                //Debug
                Config.AddSubMenu(new Menu("Drawing", "draw"));
                Config.SubMenu("draw").AddItem(new MenuItem("drawCir", "Draw circles")).SetValue(true);
                Config.SubMenu("draw").AddItem(new MenuItem("drawCD", "Draw CD")).SetValue(true);
                Config.SubMenu("draw").AddItem(new MenuItem("drawFull", "Draw full combo dmg")).SetValue(true);

                Config.AddToMainMenu();
                Drawing.OnDraw += Drawing_OnDraw;
                Drawing.OnEndScene += Drawing_OnEndScene;

                Game.OnGameUpdate += Game_OnGameUpdate;

                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpell;
                AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
                Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Jayce - Sharp");
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Config.Item("awsPress").GetValue<KeyBind>().Active)
            {
                Hpi.DrawAwsomee();
            }

            if (!Config.Item("drawFull").GetValue<bool>())
            {
                return;
            }

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
            {
                Hpi.Unit = enemy;
                Hpi.DrawDmg(Jayce.GetJayceFullComoDmg(enemy), Color.Yellow);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("gapClose").GetValue<bool>())
            {
                Jayce.KnockAway(gapcloser.Sender);
            }
        }

        public static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (Config.Item("autoInter").GetValue<bool>() && (int) spell.DangerLevel > 0)
            {
                Jayce.KnockAway(unit);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Jayce.CheckForm();
            Jayce.ProcessCDs();
            if (Config.Item("shoot").GetValue<KeyBind>().Active)
            {
                Jayce.ShootQe(Game.CursorPos);
            }


            if (!Jayce.E1.IsReady())
            {
                Jayce.CastQon = new Vector3(0, 0, 0);
            }
            else if (Jayce.CastQon.X != 0)
            {
                Jayce.ShootQe(Jayce.CastQon);
            }

            if (Config.Item("fullDMG").GetValue<KeyBind>().Active) // fullDMG
            {
                Jayce.ActivateMura();
                var target = TargetSelector.GetTarget(Jayce.GetBestRange(), TargetSelector.DamageType.Physical);
                if (Jayce.LockedTarg == null)
                {
                    Jayce.LockedTarg = target;
                }

                Jayce.DoFullDmg(Jayce.LockedTarg);
            }
            else
            {
                Jayce.LockedTarg = null;
            }

            if (Config.Item("injTarget").GetValue<KeyBind>().Active) // fullDMG
            {
                Jayce.ActivateMura();
                var target = TargetSelector.GetTarget(Jayce.GetBestRange(), TargetSelector.DamageType.Physical);
                if (Jayce.LockedTarg == null)
                {
                    Jayce.LockedTarg = target;
                }

                Jayce.DoJayceInj(Jayce.LockedTarg);
            }
            else
            {
                Jayce.LockedTarg = null;
            }

            if (Jayce.CastEonQ != null && (Jayce.CastEonQ.TimeSpellEnd - 2) > Game.Time)
            {
                Jayce.CastEonQ = null;
            }

            if (Jayce.Orbwalker.ActiveMode.ToString() == "Combo")
            {
                Jayce.ActivateMura();
                var target = TargetSelector.GetTarget(Jayce.GetBestRange(), TargetSelector.DamageType.Physical);
                Jayce.DoCombo(target);
            }

            if (Config.Item("killSteal").GetValue<bool>())
            {
                Jayce.DoKillSteal();
            }

            if (Jayce.Orbwalker.ActiveMode.ToString() == "Mixed")
            {
                Jayce.DeActivateMura();
            }

            if (Jayce.Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                Jayce.DeActivateMura();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // Draw CD
            if (Config.Item("drawCD").GetValue<bool>())
            {
                Jayce.DrawCd();
            }

            if (!Config.Item("drawCir").GetValue<bool>())
            {
                return;
            }

            Utility.DrawCircle(Jayce.Player.Position, !Jayce.IsHammer ? 1100 : 600, Color.Red);
            Utility.DrawCircle(Jayce.Player.Position, 1550, Color.Violet);
        }

        public static void Obj_AI_Base_OnProcessSpell(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {
            if (!obj.IsMe)
            {
                return;
            }

            switch (arg.SData.Name)
            {
                case "jayceshockblast":
                    //  Jayce.castEonQ = arg;
                    break;
                case "jayceaccelerationgate":
                    Jayce.CastEonQ = null;
                    break;
                // Console.WriteLine("Cast dat E on: " + arg.SData.Name);
            }
            Jayce.GetCDs(arg);
        }
    }
}