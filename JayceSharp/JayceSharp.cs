using System;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

/*
 * ToDo:
 * Q doesnt shoot much
 * Full combo burst
 * Useles gate <-- fixed
 * 
 * Add Fulldmg combo starting from hamer
 * 
 * kOCK ANY ENEMY UNDER TOWER
 * */

namespace JayceSharp
{
    internal class JayceSharp
    {
        public const string CharName = "Jayce";
        public static Menu Config;

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
            Game.PrintChat("Jayce - Sharp by DeTuKs DOnate if you love my assams :)");
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
                Config.SubMenu("combo")
                    .AddItem(new MenuItem("fullDMG", "Do full damage"))
                    .SetValue(new KeyBind('A', KeyBindType.Press));
                Config.SubMenu("combo")
                    .AddItem(new MenuItem("injTarget", "Tower Injection"))
                    .SetValue(new KeyBind('G', KeyBindType.Press));

                //Extra
                Config.AddSubMenu(new Menu("Drawing Sharp", "drawing"));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawStuff", "Draw on/off")).SetValue(true);

                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));
                Config.SubMenu("extra")
                    .AddItem(new MenuItem("shoot", "Shoot manual Q"))
                    .SetValue(new KeyBind('T', KeyBindType.Press));

                //Debug
                Config.AddSubMenu(new Menu("Debug", "debug"));
                Config.SubMenu("debug")
                    .AddItem(new MenuItem("db_targ", "Debug Target"))
                    .SetValue(new KeyBind('N', KeyBindType.Press));

                //Donate
                Config.AddSubMenu(new Menu("Donate", "Donate"));
                Config.SubMenu("debug").AddItem(new MenuItem("domateMe", "PayPal:")).SetValue(true);
                Config.SubMenu("debug").AddItem(new MenuItem("domateMe2", "dtk600@gmail.com")).SetValue(true);
                Config.SubMenu("debug").AddItem(new MenuItem("domateMe3", "Tnx ^.^")).SetValue(true);


                Config.AddToMainMenu();
                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += GameObject_OnCreate;
                GameObject.OnDelete += GameObject_OnDelete;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Jayce - Sharp");
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            Jayce.CheckForm();
            Jayce.ProcessCDs();
            if (Config.Item("shoot").GetValue<KeyBind>().Active) // fullDMG
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
            // Console.Clear();
            // Console.WriteLine(Jayce.isHammer +" "+Jayce.Qdata.SData.Name);

            if (Jayce.CastEonQ != null && (Jayce.CastEonQ.TimeSpellEnd - 2) > Game.Time)
            {
                Jayce.CastEonQ = null;
            }

            if (Jayce.Orbwalker.ActiveMode.ToString() == "Combo")
            {
                var target = TargetSelector.GetTarget(Jayce.GetBestRange(), TargetSelector.DamageType.Physical);
                Jayce.DoCombo(target);
            }

            if (Jayce.Orbwalker.ActiveMode.ToString() == "Mixed")
            {
                // Hmm..
            }

            if (Jayce.Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                // Hmm..
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Config.Item("drawStuff").GetValue<bool>())
            {
                return;
            }

            // Obj_AI_Hero target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);

            // Utility.DrawCircle(Jayce.getBestPosToHammer(target), 70, Color.LawnGreen);
            // Utility.DrawCircle(Jayce.Player.Position, 400, Color.Violet);
            if (!Jayce.IsHammer)
            {
                Utility.DrawCircle(Jayce.Player.Position, 1550, Color.Violet);
                Utility.DrawCircle(Jayce.Player.Position, 1100, Color.Red);
            }
            else
            {
                Utility.DrawCircle(Jayce.Player.Position, 600, Color.Red);
            }

            //Draw CD
            Jayce.DrawCd();
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as Obj_SpellMissile;
            if (missile == null)
            {
                return;
            }

            var missle = missile;
            if (missle.SpellCaster.IsMe && missle.SData.Name == "JayceShockBlastMis")
            {
                // Console.WriteLine("Created " +  missle.SData.Name );
                Jayce.MyCastedQ = missle;
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (Jayce.MyCastedQ == null || Jayce.MyCastedQ.NetworkId != sender.NetworkId)
            {
                return;
            }

            Jayce.MyCastedQ = null;
            Jayce.CastEonQ = null;
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {
            if (!obj.IsMe)
            {
                return;
            }

            switch (arg.SData.Name)
            {
                case "jayceshockblast":
                    Jayce.CastEonQ = arg;
                    break;
                case "jayceaccelerationgate":
                    Jayce.CastEonQ = null;
                    break;
            }

            Jayce.GetCDs(arg);
        }
    }
}