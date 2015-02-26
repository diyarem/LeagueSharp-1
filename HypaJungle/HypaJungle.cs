using System;
using System.Drawing;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    /*
     * Jungle
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     */


    internal class HypaJungle
    {
        public static JungleTimers JTimer;
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static float LastSkip;

        public HypaJungle()
        {
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoad = new Thread(Game_OnGameLoad);
                onGameLoad.Start();
            };
        }

        private static void Game_OnGameLoad()
        {
            Game.PrintChat("HypaJungle by DeTuKs");
            try
            {
                ConfigLoader.SetupFolders(JungleClearer.SupportedChamps);

                if (!JungleClearer.SupportedChamps.Contains(Player.ChampionName))
                {
                    Game.PrintChat("Sory this champion is not supported yet! go vote for it in forum ;)");
                    return;
                }

                JTimer = new JungleTimers();

                Config = new Menu("HypeJungle", "hype", true);

                SetChampMenu(Player.ChampionName);

                //  Config.AddSubMenu(new Menu("Jungler Config", "junglerCon"));
                // Config.SubMenu("junglerCon").AddItem(new MenuItem("blabla", "Relead to work!")).SetValue(true);
                //  Config.SubMenu("junglerCon").AddItem(new MenuItem("useDefConf", "Use Default Config")).SetValue(true);
                //  Config.SubMenu("junglerCon").AddItem(new MenuItem("fileConfigHypa", "")).SetValue(ConfigLoader.getChampionConfigs(player.ChampionName));
                Config.AddSubMenu(new Menu("Jungler", "jungler"));
                Config.SubMenu("jungler")
                    .AddItem(new MenuItem("doJungle", "Do jungle"))
                    .SetValue(new KeyBind('J', KeyBindType.Toggle));
                Config.SubMenu("jungler")
                    .AddItem(new MenuItem("skipSpawn", "Debug skip"))
                    .SetValue(new KeyBind('G', KeyBindType.Press));
                Config.SubMenu("jungler").AddItem(new MenuItem("autoLVL", "Auto Level")).SetValue(true);
                Config.SubMenu("jungler").AddItem(new MenuItem("autoBuy", "Auto Buy")).SetValue(true);

                Config.AddSubMenu(new Menu("Jungle CLeaning", "jungleCleaning"));
                Config.SubMenu("jungleCleaning").AddItem(new MenuItem("smiteToKill", "smite to kill")).SetValue(false);
                Config.SubMenu("jungleCleaning").AddItem(new MenuItem("enemyJung", "do Enemy jungle")).SetValue(false);
                Config.SubMenu("jungleCleaning").AddItem(new MenuItem("doCrabs", "do Crabs")).SetValue(false);
                Config.SubMenu("jungleCleaning")
                    .AddItem(new MenuItem("getOverTime", "Get everyone OverTimeDmg"))
                    .SetValue(false);
                Config.SubMenu("jungleCleaning")
                    .AddItem(new MenuItem("checkKillability", "Check if can kill camps"))
                    .SetValue(false);

                Config.AddSubMenu(new Menu("Drawings", "draw"));
                Config.SubMenu("draw").AddItem(new MenuItem("drawStuff", "Draw??")).SetValue(false);


                Config.AddSubMenu(new Menu("Debug stuff", "debug"));
                Config.SubMenu("debug")
                    .AddItem(new MenuItem("debugOn", "Debug stuff"))
                    .SetValue(new KeyBind('A', KeyBindType.Press));
                Config.SubMenu("debug").AddItem(new MenuItem("showPrio", "Show priorities")).SetValue(false);

                Config.AddToMainMenu();


                Game.OnGameUpdate += Game_OnGameUpdate;
                Drawing.OnDraw += Drawing_OnDraw;
                CustomEvents.Unit.OnLevelUp += Unit_OnLevelUp;

                Game.OnGameProcessPacket += Game_OnGameProcessPacket;
                JungleClearer.SetUpJCleaner();

                //Load custom stuff
                if (!Config.Item("useDefConf_" + Player.ChampionName).GetValue<bool>())
                {
                    ConfigLoader.LoadNewConfigHypa(
                        Config.Item("fileConfigHypa2_" + Player.ChampionName).GetValue<StringList>().SList[
                            Config.Item("fileConfigHypa2_" + Player.ChampionName).GetValue<StringList>().SelectedIndex]);
                }

                JungleClearer.Jungler.SetFirstLvl();
            }
            catch (Exception ex)
            {
                Game.PrintChat("Oops. Something went wrong with HypaJungle");
                Console.WriteLine(ex);
            }
        }

        public static void SetChampMenu(string champ)
        {
            Config.AddSubMenu(new Menu(champ + " Config", "junglerCon" + champ));
            Config.SubMenu("junglerCon" + champ)
                .AddItem(new MenuItem("blabla_" + champ, "Relead to work!"))
                .SetValue(true);
            Config.SubMenu("junglerCon" + champ)
                .AddItem(new MenuItem("useDefConf_" + champ, "Use Default Config"))
                .SetValue(true);
            Config.SubMenu("junglerCon" + champ)
                .AddItem(new MenuItem("fileConfigHypa2_" + champ, ""))
                .SetValue(ConfigLoader.GetChampionConfigs(Player.ChampionName));
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.S2C.EmptyJungleCamp.Header)
            {
                var camp = Packet.S2C.EmptyJungleCamp.Decoded(args.PacketData);
                Console.WriteLine(@"Disable camp: " + camp.CampId);
                JTimer.DisableCamp((byte) camp.CampId);
            }

            if (args.PacketData[0] == 0xE9)
            {
                var gp = new GamePacket(args.PacketData) {Position = 21};
                var campId = gp.ReadByte();
                Console.WriteLine(@"Enable camp: " + campId);
                JTimer.EnableCamp(campId);
            }

            //AfterAttack
            if (args.PacketData[0] == 0x65 && Config.Item("doJungle").GetValue<KeyBind>().Active)
            {
                var gp = new GamePacket(args.PacketData) {Position = 1};
                var dmg = Packet.S2C.Damage.Decoded(args.PacketData);

                var targetId = gp.ReadInteger();
                int dType = gp.ReadByte();
                int unknown = gp.ReadShort();
                var damageAmount = gp.ReadFloat();
                var targetNetworkIdCopy = gp.ReadInteger();
                var sourceNetworkId = gp.ReadInteger();
                var dmga = (float) Player.GetAutoAttackDamage(ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(targetId));
                if (dmga - 10 > damageAmount || dmga + 10 < damageAmount)
                {
                    return;
                }

                if (Player.NetworkId != dmg.SourceNetworkId || Player.NetworkId == targetId ||
                    Player.NetworkId == targetNetworkIdCopy)
                {
                    return;
                }

                var targ = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(dmg.TargetNetworkId);
                if ((int) dmg.Type != 12 && (int) dmg.Type != 4 && (int) dmg.Type != 3)
                {
                    return;
                }

                Console.WriteLine(@"Dmg: " + damageAmount + @" : " + dmga);

                JungleClearer.Jungler.DoAfterAttack(targ);
            }
        }

        private static void Unit_OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (Config.Item("autoLVL").GetValue<bool>())
            {
                JungleClearer.Jungler.LevelUp(sender, args);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("skipSpawn").GetValue<KeyBind>().Active) //fullDMG
            {
                if (JungleClearer.FocusedCamp != null && LastSkip + 1 < Game.Time)
                {
                    LastSkip = Game.Time;
                    JungleClearer.SkipCamp = JungleClearer.FocusedCamp;
                    JTimer.DisableCamp(JungleClearer.FocusedCamp.CampId);
                }
            }

            if (Config.Item("debugOn").GetValue<KeyBind>().Active) //fullDMG
            {
                /* foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(player))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(player);
                    if (name.Contains("cent"))
                        Console.WriteLine("{0}={1}", name, value);
                }*/

                foreach (var item in Player.InventoryItems)
                {
                    Console.WriteLine(item.Id + @" : " + item.Name);
                }

                Console.WriteLine(Player.Mana);

                foreach (var buf in Player.Buffs)
                {
                    Console.WriteLine(buf.Name);
                }

                /* foreach (SpellDataInst spell in player.Spellbook.Spells)
                {
                    Console.WriteLine(spell.Name.ToLower());
                }

                foreach (SpellDataInst spell in player.Spellbook.Spells)
                {
                    Console.WriteLine(spell.Name.ToLower()+"  "+spell.Slot);
                }*/
            }
            if (Config.Item("doJungle").GetValue<KeyBind>().Active) //fullDMG
            {
                try
                {
                    JungleClearer.UpdateJungleCleaner();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                JungleClearer.JcState = JungleClearer.JungleCleanState.GoingToShop;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Config.Item("drawStuff").GetValue<bool>())
            {
                return;
            }

            Drawing.DrawText(200, 200, Color.Green,
                JungleClearer.JcState + ": " + JungleClearer.Jungler.DpsFix + " : " + Player.Position.X + " : " +
                Player.Position.Y + " : "
                + Player.Position.Z + " : ");
            Drawing.DrawText(200, 220, Color.Green,
                "DoOver: " + JungleClearer.Jungler.OverTimeName + " : " + JungleClearer.Jungler.GotOverTime);

            if (JungleClearer.Jungler.NextItem != null)
            {
                Drawing.DrawText(200, 250, Color.Green, "Gold: " + JungleClearer.Jungler.NextItem.GoldReach);
            }

            if (JungleClearer.FocusedCamp != null)
            {
                Drawing.DrawCircle(JungleClearer.FocusedCamp.Position, 300, Color.BlueViolet);
            }

            foreach (var min in MinionManager.GetMinions(Player.Position, 800, MinionTypes.All, MinionTeam.Neutral))
            {
                if (JungleClearer.Jungler.OverTimeName != string.Empty && JungleClearer.MinHasOvertime(min) != 0)
                {
                    Drawing.DrawCircle(min.Position, 100, Color.Brown);
                }

                var pScreen = Drawing.WorldToScreen(min.Position);
                Drawing.DrawText(pScreen.X, pScreen.Y, Color.Red, min.Name + " : " + min.MaxHealth);
                var bufCount = 10;
                foreach (var buff in min.Buffs)
                {
                    Drawing.DrawText(pScreen.X, pScreen.Y + bufCount, Color.Red, buff.Name);
                    bufCount += 10;
                }
            }


            Drawing.DrawCircle(JungleClearer.GetBestBuffCamp().Position, 500, Color.BlueViolet);

            /* foreach (var camp in jTimer._jungleCamps)
            {
                var pScreen = Drawing.WorldToScreen(camp.Position);

                if(JungleClearer.isInBuffWay(camp))
                    Drawing.DrawCircle(camp.Position, 200, Color.Red);
                   // Drawing.DrawText(pScreen.X, pScreen.Y, Color.Red, camp.State.ToString() + " : " + JungleClearer.getPriorityNumber(camp));

                //Order = 0 chaos =1
            }*/

            if (!Config.Item("showPrio").GetValue<bool>())
            {
                return;
            }

            foreach (var camp in JTimer.JungleCamps)
            {
                var pScreen = Drawing.WorldToScreen(camp.Position);

                Drawing.DrawText(pScreen.X, pScreen.Y, Color.Red,
                    camp.State + " : " + camp.Team + " : " + camp.Priority);

                //Order = 0 chaos =1
            }
        }
    }
}