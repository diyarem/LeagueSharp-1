using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

/*TODO
 * Combo calc and choose best <-- kinda
 * Farming
 * Interupt
 * 
 * gap close with q < -- done
 * 
 * mash q if les hp < -- done
 * 
 * smart cancel combos < -- yup
 * 
 * gap kill <-- yup
 * 
 * overkill 
 * 
 * harass to trade good <-- done
 * 
 * 
 * fix ignite
 * 
 * R KS
 * 
 */

namespace RivenSharpV2
{
    internal class RivenSharp
    {
        public const string CharName = "Riven";
        public static Menu Config;
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static int LastTargetId = 0;

        public RivenSharp()
        {
            Console.WriteLine("Riven sharp starting...");
            try
            {
                // if (ObjectManager.Player.BaseSkinName != CharName)
                //    return;
                /* CallBAcks */
                CustomEvents.Game.OnGameLoad += delegate
                {
                    var onGameLoad = new Thread(Game_OnGameLoad);
                    onGameLoad.Start();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Game_OnGameLoad()
        {
            try
            {
                if (Riven.Player.ChampionName != "Riven")
                {
                    return;
                }

                Game.PrintChat("RivenSharp by DeTuKs");
                Config = new Menu("Riven - Sharp", "Riven", true);
                // Orbwalkervar menu = new Menu("My Mainmenu", "my_mainmenu", true);
                var orbwalkerMenu = new Menu("LX Orbwalker", "my_Orbwalker");
                LxOrbwalker.AddToMenu(orbwalkerMenu);
                Config.AddSubMenu(orbwalkerMenu);

                //TS
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);

                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R on combo (Shuld be on)")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("forceQE", "Use Q after E")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("packets", "Use packet cast")).SetValue(true);

                //Haras
                Config.AddSubMenu(new Menu("Harass Sharp", "haras"));
                Config.SubMenu("haras")
                    .AddItem(new MenuItem("doHarasE", "Harass enemy E"))
                    .SetValue(new KeyBind('G', KeyBindType.Press));
                Config.SubMenu("haras")
                    .AddItem(new MenuItem("doHarasQ", "Harass enemy Q"))
                    .SetValue(new KeyBind('T', KeyBindType.Press));

                //Drawing
                Config.AddSubMenu(new Menu("Drawing Sharp", "draw"));
                Config.SubMenu("draw").AddItem(new MenuItem("doDraw", "Dissable drawings")).SetValue(false);
                Config.SubMenu("draw").AddItem(new MenuItem("drawHp", "Draw pred hp")).SetValue(true);

                //Debug
                Config.AddSubMenu(new Menu("Debug", "debug"));
                Config.SubMenu("debug")
                    .AddItem(new MenuItem("db_targ", "Debug Target"))
                    .SetValue(new KeyBind('0', KeyBindType.Press));

                Config.AddToMainMenu();

                Game.OnGameUpdate += Game_OnGameUpdate;
                Game.OnGameSendPacket += Game_OnGameSendPacket;
                Game.OnGameProcessPacket += Game_OnGameProcessPacket;

                Drawing.OnDraw += Drawing_OnDraw;
                Drawing.OnEndScene += Drawing_OnEndScene;

                Obj_AI_Base.OnNewPath += Obj_AI_Base_OnNewPath;
                Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;

                Riven.SetSkillshots();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Obj_AI_Base_OnPlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe && args.Animation.Contains("Spell") && IsComboing())
            {
                Riven.CancelAnim();
            }
        }

        private static void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (sender.IsMe)
            {
                LxOrbwalker.ResetAutoAttackTimer();
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Config.Item("drawHp").GetValue<bool>())
            {
                return;
            }

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
            {
                Hpi.Unit = enemy;
                Hpi.DrawDmg(Riven.RushDmgBasedOnDist(enemy), Color.Yellow);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            /*
                RivenFengShuiEngine
                rivenwindslashready
             */
            try
            {
                if (Config.Item("doHarasE").GetValue<KeyBind>().Active)
                {
                    var target = TargetSelector.GetTarget(1400, TargetSelector.DamageType.Physical);
                    LxOrbwalker.ForcedTarget = target;
                    Riven.DoHarasE(target);
                }
                else if (Config.Item("doHarasQ").GetValue<KeyBind>().Active)
                {
                    var target = TargetSelector.GetTarget(1400, TargetSelector.DamageType.Physical);
                    LxOrbwalker.ForcedTarget = target;
                    Riven.DoHarasQ(target);
                }


                if (LxOrbwalker.CurrentMode == LxOrbwalker.Mode.Combo)
                {
                    var target = TargetSelector.GetTarget(1400, TargetSelector.DamageType.Physical);
                    LxOrbwalker.ForcedTarget = target;
                    Riven.DoCombo(target);
                    //Console.WriteLine(target.NetworkId);
                }
            }
            catch (Exception)
            {
                // Console.WriteLine(ex);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            try
            {
                if (Config.Item("doDraw").GetValue<bool>())
                {
                    return;
                }

                if (Config.Item("drawHp").GetValue<bool>())
                {
                    foreach (
                        var enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                    {
                        Hpi.Unit = enemy;
                        Hpi.DrawDmg(Riven.RushDmgBasedOnDist(enemy), Color.Yellow);
                    }
                }

                foreach (
                    var enHero in
                        ObjectManager.Get<Obj_AI_Hero>().Where(enHero => enHero.IsEnemy && enHero.Health > 0))
                {
                    Utility.DrawCircle(enHero.Position,
                        enHero.BoundingRadius + Riven.E.Range + Riven.Player.AttackRange,
                        (Riven.RushDown) ? Color.Red : Color.Blue);
                    // Drawing.DrawCircle(enHero.Position, enHero.BoundingRadius + Riven.E.Range+Riven.Player.AttackRange, Color.Blue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static bool IsComboing()
        {
            return Config.Item("doHarasE").GetValue<KeyBind>().Active ||
                   Config.Item("doHarasQ").GetValue<KeyBind>().Active
                   || LxOrbwalker.CurrentMode == LxOrbwalker.Mode.Combo ||
                   LxOrbwalker.CurrentMode == LxOrbwalker.Mode.LaneClear;
        }

        public static void Obj_AI_Base_OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs arg)
        {
            if (!Config.Item("forceQE").GetValue<bool>() || !sender.IsMe || !arg.SData.Name.Contains("RivenFeint") ||
                !Riven.Q.IsReady() || LxOrbwalker.GetPossibleTarget() == null)
            {
                return;
            }

            Console.WriteLine("force q");
            Riven.Q.Cast(LxOrbwalker.GetPossibleTarget().Position);
            Riven.ForceQ = true;
            // Riven.timer = new System.Threading.Timer(obj => { Riven.Player.IssueOrder(GameObjectOrder.MoveTo, Riven.difPos()); }, null, (long)100, System.Threading.Timeout.Infinite);
        }

        public static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            try
            {
                if (!IsComboing())
                {
                    return;
                }

                if (args.PacketData[0] == 35 && Riven.Q.IsReady())
                {
                    Console.WriteLine("Gott");
                    var gp = new GamePacket(args.PacketData) {Position = 2};
                    var netId = gp.ReadInteger();
                    if (LxOrbwalker.GetPossibleTarget() == null ||
                        LxOrbwalker.GetPossibleTarget().NetworkId != netId)
                    {
                        return;
                    }

                    if (!LxOrbwalker.CanAttack())
                    {
                        Riven.Q.Cast(LxOrbwalker.GetPossibleTarget().Position);
                    }
                }

                if (args.PacketData[0] == 0x17)
                {
                    Console.WriteLine("cancel");

                    var packet = new GamePacket(args.PacketData) {Position = 2};
                    var sourceId = packet.ReadInteger();
                    if (sourceId == Riven.Player.NetworkId)
                    {
                        Console.WriteLine("cancel wawf");
                        Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(Game.CursorPos.X, Game.CursorPos.Y))
                            .Send();
                        if (LxOrbwalker.GetPossibleTarget() != null)
                        {
                            Riven.MoveTo(LxOrbwalker.GetPossibleTarget().Position);
                            //Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(LXOrbwalker.GetPossibleTarget().Position.X, LXOrbwalker.GetPossibleTarget().Position.Y)).Send();

                            // LXOrbwalker.ResetAutoAttackTimer();
                            Riven.CancelAnim(true);
                        }
                    }
                }

                if (args.PacketData[0] == 0xDF && false)
                {
                    Console.WriteLine("cancel");

                    var packet = new GamePacket(args.PacketData) {Position = 2};
                    var sourceId = packet.ReadInteger();
                    if (sourceId == Riven.Player.NetworkId)
                    {
                        Console.WriteLine("cancel wawf");
                        Riven.MoveTo(Game.CursorPos);
                        Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(Game.CursorPos.X, Game.CursorPos.Y))
                            .Send();
                        LxOrbwalker.ResetAutoAttackTimer();
                        Riven.CancelAnim();
                    }
                }

                if (args.PacketData[0] == 0x61) // Move
                {
                    var packet = new GamePacket(args.PacketData) {Position = 12};
                    var sourceId = packet.ReadInteger();
                    if (sourceId != Riven.Player.NetworkId)
                    {
                        return;
                    }

                    if (LxOrbwalker.GetPossibleTarget() != null)
                    {
                        // Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(LXOrbwalker.GetPossibleTarget().Position.X, LXOrbwalker.GetPossibleTarget().Position.Y)).Send();
                        LxOrbwalker.ResetAutoAttackTimer();
                    }
                }
                else if (args.PacketData[0] == 0x38) // Animation2
                {
                    var packet = new GamePacket(args.PacketData) {Position = 1};
                    var sourceId = packet.ReadInteger();
                    if (packet.Size() != 9 || sourceId != Riven.Player.NetworkId)
                    {
                        return;
                    }

                    Riven.MoveTo(Game.CursorPos);
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(Game.CursorPos.X, Game.CursorPos.Y))
                        .Send();
                    LxOrbwalker.ResetAutoAttackTimer();
                    Riven.CancelAnim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (args.PacketData[0] == 119)
                {
                    args.Process = false;
                }

                //if (Riven.orbwalker.ActiveMode.ToString() == "Combo")
                //   LogPacket(args);
                if (args.PacketData[0] != 154 || LxOrbwalker.CurrentMode != LxOrbwalker.Mode.Combo)
                {
                    return;
                }

                var cast = Packet.C2S.Cast.Decoded(args.PacketData);
                if ((int) cast.Slot > -1 && (int) cast.Slot < 5)
                {
                    Utility.DelayAction.Add(Game.Ping + LxOrbwalker.GetCurrentWindupTime(),
                        delegate { Riven.CancelAnim(true); });

                    //Game.Say("/l");
                }

                if (cast.Slot == SpellSlot.E && Riven.R.IsReady() && Config.Item("useR").GetValue<bool>())
                {
                    Utility.DelayAction.Add(Game.Ping + 50,
                        delegate { Riven.UseRSmart(LxOrbwalker.GetPossibleTarget()); });
                }
                //Console.WriteLine(cast.Slot + " : " + Game.Ping);
                /* if (cast.Slot == SpellSlot.Q)
                        Orbwalking.ResetAutoAttackTimer();
                    else if (cast.Slot == SpellSlot.W && Riven.Q.IsReady())
                        Utility.DelayAction.Add(Game.Ping+200, delegate { Riven.useHydra(Riven.orbwalker.GetTarget()); });
                    else if (cast.Slot == SpellSlot.E && Riven.W.IsReady())
                    {
                        Console.WriteLine("cast QQQQ");
                        Utility.DelayAction.Add(Game.Ping+200, delegate { Riven.useWSmart(Riven.orbwalker.GetTarget()); });
                    }
                    else if ((int)cast.Slot == 131 && Riven.W.IsReady())
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        Utility.DelayAction.Add(Game.Ping +200, delegate { Riven.useWSmart(Riven.orbwalker.GetTarget()); });
                    }*/
                // LogPacket(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}