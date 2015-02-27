using System;
using System.Drawing;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

/*
 * ToDo:
 * 
 * */

namespace KarmaSharp
{
    internal class KarmaSharp
    {
        public const string CharName = "Karma";
        public static Menu Config;
        public static Obj_AI_Hero Target;

        public KarmaSharp()
        {
            if (ObjectManager.Player.BaseSkinName != CharName)
            {
                return;
            }

            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoad = new Thread(Game_OnGameLoad);
                onGameLoad.Start();
            };
        }

        private static void Game_OnGameLoad()
        {
            Game.PrintChat("Karma - Sharp by DeTuKs");

            try
            {
                Config = new Menu("Karma - Sharp by DeTuKs Donate if you love my assams :)", "Karma", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                Karma.Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

                //TS
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);

                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E on Myself")).SetValue(false);
                Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R on Q(Harass too)")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));

                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));

                //Harass
                Config.AddSubMenu(new Menu("Harass Sharp", "harass"));
                Config.SubMenu("harass")
                    .AddItem(new MenuItem("harP", "Harass Enemy press"))
                    .SetValue(new KeyBind('T', KeyBindType.Press));
                Config.SubMenu("harass")
                    .AddItem(new MenuItem("harT", "Harass Enemy toggle"))
                    .SetValue(new KeyBind('H', KeyBindType.Toggle));
                Config.SubMenu("harass").AddItem(new MenuItem("useQHar", "Use Q with R")).SetValue(true);

                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));
                Config.SubMenu("extra").AddItem(new MenuItem("useMinions", "Use minions on Q")).SetValue(true);

                //Donate
                Config.AddSubMenu(new Menu("Donate", "Donate"));
                Config.SubMenu("Donate").AddItem(new MenuItem("domateMe", "PayPal:")).SetValue(true);
                Config.SubMenu("Donate").AddItem(new MenuItem("domateMe2", "dtk600@gmail.com")).SetValue(true);
                Config.SubMenu("Donate").AddItem(new MenuItem("domateMe3", "Tnx ^.^")).SetValue(true);

                //Debug
                //  Config.AddSubMenu(new Menu("Debug", "debug"));
                //  Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "Debug Target")).SetValue(new KeyBind('T', KeyBindType.Press, false));


                Config.AddToMainMenu();
                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnGameUpdate += Game_OnGameUpdate;

                Karma.SetSkillShots();
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Yasuo- Sharpino");
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Karma.Orbwalker.ActiveMode.ToString() == "Combo")
            {
                Target = TargetSelector.GetTarget(1150, TargetSelector.DamageType.Magical);
                Karma.DoCombo(Target);
            }

            if (Karma.Orbwalker.ActiveMode.ToString() == "Mixed")
            {
                // Hmm..
            }

            if (Karma.Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                // Hmm..
            }

            if (!Config.Item("harP").GetValue<KeyBind>().Active && !Config.Item("harT").GetValue<KeyBind>().Active)
            {
                return;
            }

            Target = TargetSelector.GetTarget(1150, TargetSelector.DamageType.Magical);
            Karma.DoHarass(Target);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawCircle(Karma.Player.Position, 950, Color.Blue);
        }
    }
}