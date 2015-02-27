using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using MasterSharp.Evade;
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

namespace MasterSharp
{
    internal class MasterSharp
    {
        public const string CharName = "MasterYi";
        public static Menu Config;
        public static Menu SkillShotMenuq;
        public static Menu SkillShotMenuw;
        public static List<Skillshot> DetectedSkillshots = new List<Skillshot>();

        public MasterSharp()
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

            Game.PrintChat("MasterYi - by DeTuKs");
            MasterYi.SetSkillShots();
            try
            {
                TargetedSkills.SetUpSkills();

                Config = new Menu("MasterYi - Sharp", "MasterYi", true);
                var orbwalkerMenu = new Menu("LX Orbwalker", "my_Orbwalker");
                LxOrbwalker.AddToMenu(orbwalkerMenu);
                Config.AddSubMenu(orbwalkerMenu);

                //TS
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);

                //Combo
                Config.AddSubMenu(new Menu("Combo Sharp", "combo"));
                Config.SubMenu("combo")
                    .AddItem(new MenuItem("comboItems", "Meh everything is fine here"))
                    .SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("comboWreset", "AA reset W")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q to gap")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("useSmite", "Use Smite")).SetValue(true);

                //Extra
                Config.AddSubMenu(new Menu("Extra Sharp", "extra"));
                Config.SubMenu("extra").AddItem(new MenuItem("packets", "Use Packet cast")).SetValue(false);

                Config.AddSubMenu(new Menu("Anti Skillshots", "aShots"));

                //SmartW
                Config.SubMenu("aShots").AddItem(new MenuItem("smartW", "Smart W if cantQ")).SetValue(true);
                Config.SubMenu("aShots").AddItem(new MenuItem("smartQDogue", "Q use dogue")).SetValue(true);
                Config.SubMenu("aShots").AddItem(new MenuItem("wqOnDead", "W or Q if will kill")).SetValue(false);
                SkillShotMenuq = GetSkilshotMenuQ();
                Config.SubMenu("aShots").AddSubMenu(SkillShotMenuq);
                SkillShotMenuw = GetSkilshotMenuW();
                Config.SubMenu("aShots").AddSubMenu(SkillShotMenuw);

                //Debug
                Config.AddSubMenu(new Menu("Drawing", "draw"));
                Config.SubMenu("draw").AddItem(new MenuItem("drawCir", "Draw circles")).SetValue(true);
                Config.SubMenu("draw")
                    .AddItem(new MenuItem("debugOn", "Debug stuff"))
                    .SetValue(new KeyBind('A', KeyBindType.Press));

                Config.AddToMainMenu();
                Drawing.OnDraw += Drawing_OnDraw;

                Game.OnGameUpdate += Game_OnGameUpdate;

                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

                SkillshotDetector.OnDetectSkillshot += SkillshotDetector_OnDetectSkillshot;
                SkillshotDetector.OnDeleteMissile += SkillshotDetector_OnDeleteMissile;
                Game.OnGameProcessPacket += Game_OnGameProcessPacket;
                CustomEvents.Unit.OnDash += Unit_OnDash;
            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Master - Sharp");
            }
        }

        private static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (MasterYi.SelectedTarget != null && sender.NetworkId == MasterYi.SelectedTarget.NetworkId &&
                MasterYi.Q.IsReady() && LxOrbwalker.CurrentMode == LxOrbwalker.Mode.Combo
                && sender.Distance(MasterYi.Player) <= 600)
            {
                MasterYi.Q.Cast(sender);
            }
        }

        public static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!Config.Item("comboWreset").GetValue<bool>() || args.PacketData[0] != 0x65 || !MasterYi.W.IsReady() ||
                LxOrbwalker.CurrentMode != LxOrbwalker.Mode.Combo)
            {
                return;
            }

            // LogPacket(args);
            var gp = new GamePacket(args.PacketData) {Position = 1};
            var dmg = Packet.S2C.Damage.Decoded(args.PacketData);

            var targetId = gp.ReadInteger();
            int dType = gp.ReadByte();
            int unknown = gp.ReadShort();
            var damageAmount = gp.ReadFloat();
            var targetNetworkIdCopy = gp.ReadInteger();
            var sourceNetworkId = gp.ReadInteger();
            var dmga =
                (float)
                    MasterYi.Player.GetAutoAttackDamage(
                        ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(targetId));
            if (dmga - 10 > damageAmount || dmga + 10 < damageAmount)
            {
                return;
            }

            if (MasterYi.Player.NetworkId != dmg.SourceNetworkId && MasterYi.Player.NetworkId == targetId)
            {
                return;
            }

            var targ = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(dmg.TargetNetworkId);
            if ((int) dmg.Type != 12 && (int) dmg.Type != 4 && (int) dmg.Type != 3)
            {
                return;
            }

            if (MasterYi.W.IsReady() && LxOrbwalker.InAutoAttackRange(targ))
            {
                MasterYi.W.Cast(targ.Position);
                // LXOrbwalker.ResetAutoAttackTimer();
            }
            // Console.WriteLine("dtyoe: " + dType);
        }

        public static Menu GetSkilshotMenuQ()
        {
            // Create the skillshots submenus.
            var skillShotsQ = new Menu("Evade with Q", "aShotsSkillsQ");

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.Team == ObjectManager.Player.Team)
                {
                    continue;
                }

                foreach (var spell in SpellDatabase.Spells)
                {
                    if (spell.ChampionName != hero.ChampionName)
                    {
                        continue;
                    }

                    var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);
                    subMenu.AddItem(
                        new MenuItem("qEvadeAll" + spell.MenuItemName, "Evade with Q Allways").SetValue(
                            spell.IsDangerous));
                    subMenu.AddItem(
                        new MenuItem("qEvade" + spell.MenuItemName, "Evade with Q Combo").SetValue(
                            spell.IsDangerous));

                    skillShotsQ.AddSubMenu(subMenu);
                }
            }
            return skillShotsQ;
        }

        public static Menu GetSkilshotMenuW()
        {
            // Create the skillshots submenus.
            var skillShotsW = new Menu("Evade with W", "aShotsSkillsW");

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.Team == ObjectManager.Player.Team)
                {
                    continue;
                }

                foreach (var spell in SpellDatabase.Spells)
                {
                    if (spell.ChampionName != hero.ChampionName)
                    {
                        continue;
                    }

                    var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);
                    subMenu.AddItem(
                        new MenuItem("wEvadeAll" + spell.MenuItemName, "Evade with W allways").SetValue(
                            spell.IsDangerous));
                    subMenu.AddItem(
                        new MenuItem("wEvade" + spell.MenuItemName, "Evade with W COmbo").SetValue(
                            spell.IsDangerous));

                    skillShotsW.AddSubMenu(subMenu);
                }
            }
            return skillShotsW;
        }

        public static bool SkillShotMustBeEvaded(string name)
        {
            return SkillShotMenuq.Item("qEvade" + name) == null || SkillShotMenuq.Item("qEvade" + name).GetValue<bool>();
        }

        public static bool SkillShotMustBeEvadedAllways(string name)
        {
            return SkillShotMenuq.Item("qEvadeAll" + name) == null ||
                   SkillShotMenuq.Item("qEvadeAll" + name).GetValue<bool>();
        }

        public static bool SkillShotMustBeEvadedW(string name)
        {
            return SkillShotMenuw.Item("wEvade" + name) == null || SkillShotMenuw.Item("wEvade" + name).GetValue<bool>();
        }

        public static bool SkillShotMustBeEvadedWAllways(string name)
        {
            return SkillShotMenuw.Item("wEvade" + name) == null || SkillShotMenuw.Item("wEvade" + name).GetValue<bool>();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("debugOn").GetValue<KeyBind>().Active) // fullDMG
            {
                foreach (var buf in MasterYi.Player.Buffs)
                {
                    Console.WriteLine(buf.Name);
                }
            }
            if (LxOrbwalker.CurrentMode == LxOrbwalker.Mode.Combo)
            {
                var target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);
                LxOrbwalker.ForcedTarget = target;
                if (target != null)
                {
                    MasterYi.SelectedTarget = target;
                }

                MasterYi.SlayMaderDuker(target);
            }

            DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());
            foreach (
                var skillShot in DetectedSkillshots.Where(skillShot => skillShot.IsAboutToHit(250, MasterYi.Player)))
            {
                MasterYi.EvadeSkillShot(skillShot);
            }

            // Anti Buferino
            foreach (var buf in MasterYi.Player.Buffs)
            {
                var skill = TargetedSkills.DagerousBuffs.FirstOrDefault(ob => ob.SName == buf.Name);
                if (skill != null)
                {
                    // Console.WriteLine("Evade: " + buf.Name);
                    MasterYi.EvadeBuff(buf, skill);
                }
                // if(buf.EndTime-Game.Time<0.2f)
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Config.Item("drawCir").GetValue<bool>())
            {
                return;
            }

            Utility.DrawCircle(MasterYi.Player.Position, 600, Color.Green);
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {
            if (arg.Target == null || arg.Target.NetworkId != MasterYi.Player.NetworkId)
            {
                return;
            }

            // Console.WriteLine(arg.SData.Name);
            if (!(obj is Obj_AI_Hero))
            {
                return;
            }

            var skill = TargetedSkills.TargetedSkillsAll.FirstOrDefault(ob => ob.SName == arg.SData.Name);
            if (skill != null)
            {
                // Console.WriteLine("Evade: " + arg.SData.Name);
                MasterYi.EvadeDamage(skill.UseQ, skill.UseW, arg, skill.Delay);
            }
        }

        private static void SkillshotDetector_OnDeleteMissile(Skillshot skillshot, Obj_SpellMissile missile)
        {
            if (skillshot.SpellData.SpellName != "VelkozQ")
            {
                return;
            }

            var spellData = SpellDatabase.GetByName("VelkozQSplit");
            var direction = skillshot.Direction.Perpendicular();
            if (DetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") != 0)
            {
                return;
            }

            for (var i = -1; i <= 1; i = i + 2)
            {
                var skillshotToAdd = new Skillshot(
                    DetectionType.ProcessSpell, spellData, Environment.TickCount, missile.Position.To2D(),
                    missile.Position.To2D() + i*direction*spellData.Range, skillshot.Unit);
                DetectedSkillshots.Add(skillshotToAdd);
            }
        }

        private static void SkillshotDetector_OnDetectSkillshot(Skillshot skillshot)
        {
            var alreadyAdded = false;
            foreach (
                var item in
                    DetectedSkillshots.Where(item => item.SpellData.SpellName == skillshot.SpellData.SpellName &&
                                                     (item.Unit.NetworkId == skillshot.Unit.NetworkId &&
                                                      (skillshot.Direction).AngleBetween(item.Direction) < 5 &&
                                                      (skillshot.Start.Distance(item.Start) < 100 ||
                                                       skillshot.SpellData.FromObjects.Length == 0))))
            {
                alreadyAdded = true;
            }

            // Check if the skillshot is from an ally.
            if (skillshot.Unit.Team == ObjectManager.Player.Team)
            {
                return;
            }

            // Check if the skillshot is too far away.
            if (skillshot.Start.Distance(ObjectManager.Player.ServerPosition.To2D()) >
                (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000)*1.5)
            {
                return;
            }

            // Add the skillshot to the detected skillshot list.
            if (alreadyAdded)
            {
                return;
            }

            // Multiple skillshots like twisted fate Q.
            if (skillshot.DetectionType == DetectionType.ProcessSpell)
            {
                if (skillshot.SpellData.MultipleNumber != -1)
                {
                    var originalDirection = skillshot.Direction;
                    for (var i = -(skillshot.SpellData.MultipleNumber - 1)/2;
                        i <= (skillshot.SpellData.MultipleNumber - 1)/2;
                        i++)
                    {
                        var end = skillshot.Start +
                                  skillshot.SpellData.Range*
                                  originalDirection.Rotated(skillshot.SpellData.MultipleAngle*i);
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                            skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                    }

                    return;
                }

                if (skillshot.SpellData.SpellName == "UFSlash")
                {
                    skillshot.SpellData.MissileSpeed = 1600 + (int) skillshot.Unit.MoveSpeed;
                }

                if (skillshot.SpellData.Invert)
                {
                    var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                    var end = skillshot.Start + newDirection*skillshot.Start.Distance(skillshot.End);
                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                        skillshot.Unit);
                    DetectedSkillshots.Add(skillshotToAdd);
                    return;
                }

                if (skillshot.SpellData.Centered)
                {
                    var start = skillshot.Start - skillshot.Direction*skillshot.SpellData.Range;
                    var end = skillshot.Start + skillshot.Direction*skillshot.SpellData.Range;
                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                        skillshot.Unit);
                    DetectedSkillshots.Add(skillshotToAdd);
                    return;
                }

                if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                {
                    const int angle = 60;
                    var edge1 =
                        (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(
                            -angle/2*(float) Math.PI/180);
                    var edge2 = edge1.Rotated(angle*(float) Math.PI/180);

                    foreach (var skillshotToAdd in from minion in ObjectManager.Get<Obj_AI_Minion>()
                        let v = minion.ServerPosition.To2D() - skillshot.Unit.ServerPosition.To2D()
                        where
                            minion.Name == "Seed" && edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 &&
                            minion.Distance(skillshot.Unit) < 800 && (minion.Team != ObjectManager.Player.Team)
                        let start = minion.ServerPosition.To2D()
                        let end = skillshot.Unit.ServerPosition.To2D()
                            .Extend(
                                minion.ServerPosition.To2D(),
                                skillshot.Unit.Distance(minion) > 200 ? 1300 : 1000)
                        select new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit))
                    {
                        DetectedSkillshots.Add(skillshotToAdd);
                    }

                    return;
                }

                if (skillshot.SpellData.SpellName == "AlZaharCalloftheVoid")
                {
                    var start = skillshot.End - skillshot.Direction.Perpendicular()*400;
                    var end = skillshot.End + skillshot.Direction.Perpendicular()*400;
                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                        skillshot.Unit);
                    DetectedSkillshots.Add(skillshotToAdd);
                    return;
                }

                if (skillshot.SpellData.SpellName == "ZiggsQ")
                {
                    var d1 = skillshot.Start.Distance(skillshot.End);
                    var d2 = d1*0.4f;
                    var d3 = d2*0.69f;


                    var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                    var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");

                    var bounce1Pos = skillshot.End + skillshot.Direction*d2;
                    var bounce2Pos = bounce1Pos + skillshot.Direction*d3;

                    bounce1SpellData.Delay =
                        (int) (skillshot.SpellData.Delay + d1*1000f/skillshot.SpellData.MissileSpeed + 500);
                    bounce2SpellData.Delay =
                        (int) (bounce1SpellData.Delay + d2*1000f/bounce1SpellData.MissileSpeed + 500);

                    var bounce1 = new Skillshot(
                        skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                        skillshot.Unit);
                    var bounce2 = new Skillshot(
                        skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                        skillshot.Unit);

                    DetectedSkillshots.Add(bounce1);
                    DetectedSkillshots.Add(bounce2);
                }

                if (skillshot.SpellData.SpellName == "ZiggsR")
                {
                    skillshot.SpellData.Delay =
                        (int) (1500 + 1500*skillshot.End.Distance(skillshot.Start)/skillshot.SpellData.Range);
                }

                if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                {
                    var endPos = new Vector2();

                    foreach (
                        var s in
                            DetectedSkillshots.Where(
                                s => s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E))
                    {
                        endPos = s.End;
                    }

                    foreach (
                        var m in
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(m => m.BaseSkinName == "jarvanivstandard" && m.Team == skillshot.Unit.Team &&
                                            skillshot.IsDanger(m.Position.To2D())))
                    {
                        endPos = m.Position.To2D();
                    }

                    if (!endPos.IsValid())
                    {
                        return;
                    }

                    skillshot.End = endPos + 200*(endPos - skillshot.Start).Normalized();
                    skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                }
            }

            if (skillshot.SpellData.SpellName == "OriannasQ")
            {
                var endCSpellData = SpellDatabase.GetByName("OriannaQend");

                var skillshotToAdd = new Skillshot(
                    skillshot.DetectionType, endCSpellData, skillshot.StartTick, skillshot.Start, skillshot.End,
                    skillshot.Unit);

                DetectedSkillshots.Add(skillshotToAdd);
            }


            // Don't allow fow detection.
            if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
            {
                return;
            }
#if DEBUG
            Console.WriteLine(Environment.TickCount + "Adding new skillshot: " + skillshot.SpellData.SpellName);
#endif

            DetectedSkillshots.Add(skillshot);
        }
    }
}