using System;
using System.Drawing;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

/*
 * ToDo:
 * 
 * */

namespace TryndSharp
{
    internal class TryndSharp
    {
        public const string CharName = "Tryndamere";
        public static Menu Config;
        public static Obj_AI_Hero Target;

        public TryndSharp()
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
            Game.PrintChat("Tryndamere - Sharp by DeTuKs");

            try
            {
                Config = new Menu("Trynd - SharpSwrod", "Tryndamere", true);

                //Orbwalker
                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                Trynd.Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

                //TS
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);

                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("QonHp", "Q on % hp")).SetValue(new Slider(25, 100, 0));
                // Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R on %")).SetValue(new Slider(25, 100, 0));

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));

                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));

                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));

                //Debug
                Config.AddSubMenu(new Menu("Debug", "debug"));
                Config.SubMenu("debug")
                    .AddItem(new MenuItem("db_targ", "Debug Target"))
                    .SetValue(new KeyBind('T', KeyBindType.Press));


                Config.AddToMainMenu();
                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnGameUpdate += Game_OnGameUpdate;
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Tryndamere - Sharp");
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Trynd.Orbwalker.ActiveMode.ToString() == "Combo")
            {
                // Console.WriteLine("emm");
                if (Trynd.E.IsReady())
                {
                    Target = TargetSelector.GetTarget(950, TargetSelector.DamageType.Physical);
                }
                else if (Trynd.W.IsReady())
                {
                    Target = TargetSelector.GetTarget(450, TargetSelector.DamageType.Physical);
                }
                else
                {
                    Target = TargetSelector.GetTarget(250, TargetSelector.DamageType.Physical);
                }

                Trynd.DoCombo(Target);
            }

            if (Trynd.Orbwalker.ActiveMode.ToString() == "Mixed")
            {
                // Hmm..
            }

            if (Trynd.Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                // Hmm..
            }


            if (Config.Item("harassOn").GetValue<bool>() && Trynd.Orbwalker.ActiveMode.ToString() == "None")
            {
                // Hmm..
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawCircle(Trynd.Player.Position, Trynd.E.Range, Color.Blue);
        }
    }
}