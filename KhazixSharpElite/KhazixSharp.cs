using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

/*
 * ToDo:
 * 
 * Hydra <-- done
 * 
 * overkill <--done
 * 
 * 
 * tower dives
 * 
 * 
 * ult only close
 * 
 * */

namespace KhazixSharp
{
    internal class KhazixSharp
    {
        public const string CharName = "Khazix";
        public static Menu Config;
        public static HpBarIndicator Hpi = new HpBarIndicator();

        public KhazixSharp()
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
            Game.PrintChat("Khazix - Sharp by DeTuKs");

            try
            {
                Config = new Menu("KhazixSharp", "Khazix", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                Khazix.Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

                //TS
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);

                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);

                //LastHit
                Config.AddSubMenu(new Menu("LastHit Sharp", "lHit"));

                //LaneClear
                Config.AddSubMenu(new Menu("LaneClear Sharp", "lClear"));

                //Harass
                Config.AddSubMenu(new Menu("Harass Sharp", "harass"));
                Config.SubMenu("harass")
                    .AddItem(new MenuItem("harassBtn", "Harass Target"))
                    .SetValue(new KeyBind('A', KeyBindType.Press));

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

                Khazix.SetSkillshots();
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with KhazixSharp");
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            try
            {
                if (Khazix.Orbwalker.ActiveMode.ToString() == "Combo")
                {
                    var target = TargetSelector.GetTarget(Khazix.GetBestRange(), TargetSelector.DamageType.Physical);
                    Khazix.CheckUpdatedSpells();
                    Khazix.DoCombo(target);
                    // Console.WriteLine(target.NetworkId);
                }
                if (Config.Item("harassBtn").GetValue<KeyBind>().Active)
                {
                    var target = TargetSelector.GetTarget(Khazix.GetBestRange(), TargetSelector.DamageType.Physical);
                    Khazix.DoHarass(target);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
            {
                Hpi.Unit = enemy;
                Hpi.DrawDmg(Khazix.FullComboDmgOn(enemy), Color.Yellow);
            }
            Drawing.DrawCircle(Khazix.Player.Position, Khazix.Q.Range, Color.Pink);
            Drawing.DrawCircle(Khazix.Player.Position, Khazix.W.Range, Color.Pink);
            Drawing.DrawCircle(Khazix.Player.Position, Khazix.E.Range, Color.Pink);
        }
    }
}