using System;
using System.Collections.Generic;
using System.Linq;
using HypaJungle.Champions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    internal class JungleClearer
    {
        public enum JungleCleanState
        {
            AttackingMinions,
            WaitingMinions,
            RunningToCamp,
            SearchingBestCamp,
            GoingToShop,
            DoingDragon,
            RecallForHeal,
            ThinkAfterFinishCamp
        }

        public static List<String> SupportedChamps = new List<string>
        {
            "MasterYi",
            "Udyr",
            "Warwick",
            "Shyvana",
            "LeeSin",
            "Amumu",
            "Rengar"
        };

        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static JungleCamp FocusedCamp;
        public static bool RecalCasted = true;
        public static JungleCamp SkipCamp;
        public static JungleCleanState JcState = JungleCleanState.GoingToShop;
        public static Jungler Jungler = new MasterYi();

        public static void SetUpJCleaner()
        {
            switch (Player.ChampionName.ToLower())
            {
                case ("warwick"):
                    Jungler = new Warwick();
                    Game.PrintChat("Warwick loaded");
                    break;
                case "masteryi":
                    Jungler = new MasterYi();
                    Game.PrintChat("MasterYi loaded");
                    break;
                case "udyr":
                    Jungler = new Udyr();
                    Game.PrintChat("Udyr loaded");
                    break;
                case "shyvana":
                    Jungler = new Shyvana();
                    Game.PrintChat("Shyvana loaded");
                    break;
                case "leesin":
                    Jungler = new LeeSin();
                    Game.PrintChat("LeeSin loaded");
                    break;
                case "amumu":
                    Jungler = new Amumu();
                    Game.PrintChat("Amumu loaded");
                    break;
                case "rengar":
                    Jungler = new Rengar();
                    Game.PrintChat("Rengar loaded");
                    break;
            }

            Game.PrintChat("Other junglers coming soon!");
        }

        public static void UpdateJungleCleaner()
        {
            if (Player.IsDead)
            {
                JcState = JungleCleanState.RecallForHeal;
                return;
            }

            if (JcState == JungleCleanState.SearchingBestCamp)
            {
                FocusedCamp = GetBestCampToGo();
                if (FocusedCamp != null)
                {
                    //puss out or kill?
                    if (FocusedCamp.WillKillMe || (Player.Health/Player.MaxHealth < 0.5f && FocusedCamp.TimeToCamp > 12))
                    {
                        Console.WriteLine(@"Gona diee!!");
                        JcState = JungleCleanState.RecallForHeal;
                    }
                    else
                    {
                        JcState = JungleCleanState.RunningToCamp;
                    }
                }
                else
                {
                    JcState = JungleCleanState.RecallForHeal;
                }
            }

            if (JcState == JungleCleanState.RunningToCamp)
            {
                if (FocusedCamp != null && (FocusedCamp.State != JungleCampState.Dead && FocusedCamp.Team != 3))
                {
                    Jungler.CastWhenNear(FocusedCamp);
                }
                Jungler.CheckItems();
                LogicRunToCamp();
            }

            if (FocusedCamp != null && (JcState == JungleCleanState.RunningToCamp &&
                                        (Geometry.Distance(HypaJungle.Player.Position, FocusedCamp.Position) < 200 ||
                                         IsCampVisible())))
            {
                JcState = JungleCleanState.WaitingMinions;
            }

            if (JcState == JungleCleanState.WaitingMinions)
            {
                DoWhileIdling();
            }

            if (JcState == JungleCleanState.WaitingMinions && (IsCampVisible()))
            {
                JcState = JungleCleanState.AttackingMinions;
            }

            if (JcState == JungleCleanState.AttackingMinions)
            {
                AttackCampMinions();
            }

            if (JcState == JungleCleanState.AttackingMinions && IsCampFinished())
            {
                JcState = HypaJungle.Config.Item("autoBuy").GetValue<bool>()
                    ? JungleCleanState.GoingToShop
                    : JungleCleanState.SearchingBestCamp;
            }

            if (JcState == JungleCleanState.ThinkAfterFinishCamp)
            {
                JcState = JungleCleanState.SearchingBestCamp;
            }

            if (JcState == JungleCleanState.RecallForHeal)
            {
                if (Jungler.Recall.IsReady() && !Player.Spellbook.IsChanneling && !Jungler.InSpwan() && !RecalCasted)
                {
                    Jungler.Recall.Cast();
                    RecalCasted = true;
                }

                if (Jungler.InSpwan())
                {
                    if (HypaJungle.Config.Item("autoBuy").GetValue<bool>())
                    {
                        JcState = JungleCleanState.GoingToShop;
                    }
                    else
                    {
                        if (Jungler.InSpwan() && Player.Health > Player.MaxHealth*0.7f &&
                            (!Jungler.GotMana || Player.Mana > Player.MaxMana*0.7f))
                        {
                            JcState = JungleCleanState.SearchingBestCamp;
                        }
                    }
                }
            }

            if (JcState == JungleCleanState.GoingToShop)
            {
                if (!HypaJungle.Config.Item("autoBuy").GetValue<bool>())
                {
                    JcState = JungleCleanState.SearchingBestCamp;
                }

                if (Jungler.InSpwan())
                {
                    Jungler.GetItemPassiveBoostDps();
                    Jungler.SetupSmite();
                }

                if (Jungler.InSpwan() && Player.Spellbook.IsChanneling)
                {
                    var stopRecPos = new Vector3(6, 30, 2);
                    Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position + stopRecPos);
                }

                if (Jungler.NextItem != null && Player.GoldCurrent >= Jungler.NextItem.GoldReach)
                {
                    if (Jungler.Recall.IsReady() && !Player.Spellbook.IsChanneling && !Jungler.InSpwan() && !RecalCasted)
                    {
                        Jungler.Recall.Cast();
                        RecalCasted = true;
                    }
                }
                else
                {
                    if (Jungler.InSpwan() && Player.Health > Player.MaxHealth*0.8f &&
                        (!Jungler.GotMana || Player.Mana > Player.MaxMana*0.8f))
                    {
                        JcState = JungleCleanState.SearchingBestCamp;
                    }

                    if (!Player.Spellbook.IsChanneling && !Jungler.InSpwan())
                    {
                        JcState = JungleCleanState.SearchingBestCamp;
                    }
                }
            }
            else if (JcState != JungleCleanState.RecallForHeal)
            {
                RecalCasted = false;
            }

            if (JcState != JungleCleanState.GoingToShop || !Jungler.InSpwan())
            {
                return;
            }

            if (Jungler.NextItem != null && Player.GoldCurrent >= Jungler.NextItem.GoldReach)
            {
                Jungler.BuyItems();
            }

            if (Player.Health > Player.MaxHealth*0.75f && Player.Mana > Player.MaxMana*0.75f)
            {
                JcState = JungleCleanState.SearchingBestCamp;
            }
        }

        public static bool CanLeaveBase()
        {
            if (Jungler.InSpwan() && Player.Health > Player.MaxHealth*0.7f &&
                (!Jungler.GotMana || Player.Mana > Player.MaxMana*0.7f))
            {
                if (Jungler.NextItem.GoldReach - Player.GoldCurrent > 16)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool NoEnemiesAround()
        {
            return (MinionManager.GetMinions(Player.Position, 500).Count == 0);
        }

        public static bool IsCampVisible()
        {
            GetJungleMinionsManualy();

            return FocusedCamp.Minions.Any(min => min.Unit != null && min.Unit.IsVisible);
        }

        //will need to impliment all shortcuts here
        public static void LogicRunToCamp()
        {
            Jungler.DoWhileRunningIdlin();

            if (!Jungler.CanMove())
            {
                JcState = JungleCleanState.SearchingBestCamp;
                return;
            }

            if (!HypaJungle.Player.IsMoving || Enumerable.Count(HypaJungle.Player.Path) == 0
                || Enumerable.Last(HypaJungle.Player.Path).Distance(FocusedCamp.Position) > 50)
            {
                HypaJungle.Player.IssueOrder(GameObjectOrder.MoveTo, FocusedCamp.Position);
            }
        }

        public static void AttackCampMinions()
        {
            if (FocusedCamp == null || FocusedCamp.Minions == null)
            {
                return;
            }

            GetJungleMinionsManualy();
            if (!Jungler.GotOverTime || !HypaJungle.Config.Item("getOverTime").GetValue<bool>())
            {
                var campMinions =
                    FocusedCamp.Minions.Where(
                        min => min != null && min.Unit is Obj_AI_Minion && !min.Unit.IsDead)
                        .OrderByDescending(min => ((Obj_AI_Minion) min.Unit).MaxHealth).FirstOrDefault();
                if (campMinions != null && campMinions.Unit is Obj_AI_Minion)
                {
                    Jungler.StartAttack((Obj_AI_Minion) campMinions.Unit, false);
                }
            }
            else
            {
                var campMinions =
                    FocusedCamp.Minions.Where(
                        min => min != null && min.Unit is Obj_AI_Minion && !min.Unit.IsDead)
                        .OrderBy(min => MinHasOvertime(((Obj_AI_Minion) min.Unit)))
                        .ThenByDescending(min => ((Obj_AI_Minion) min.Unit).MaxHealth)
                        .FirstOrDefault();
                // .OrderByDescending(min => ((Obj_AI_Minion)min.Unit).MaxHealth).First();

                if (campMinions != null && campMinions.Unit is Obj_AI_Minion)
                    Jungler.StartAttack((Obj_AI_Minion) campMinions.Unit, false);
            }
        }

        public static int MinHasOvertime(Obj_AI_Base min)
        {
            return min.Buffs.Any(buf => buf.Name == "itemmonsterburn") ? 5 : 0;
        }

        public static void GetJungleMinionsManualy()
        {
            var jungles =
                MinionManager.GetMinions(HypaJungle.Player.Position, 1000, MinionTypes.All, MinionTeam.Neutral).ToList();
            foreach (var jun in jungles)
            {
                HypaJungle.JTimer.SetUpMinionsPlace((Obj_AI_Minion) jun);
            }
        }

        public static bool IsCampFinished()
        {
            return FocusedCamp.State == JungleCampState.Dead && FocusedCamp.Minions.All(min => min == null || min.Dead);
            // return focusedCamp.Minions.All(min => min == null || min.Dead);
        }

        public static void DoWhileIdling()
        {
            Jungler.DoWhileRunningIdlin();
        }

        /*
         *  is buff +5
         *  is in way of needed buff +5
         *  is close priority +10 +8 +6
         *  is spawning till get + 5 sec +4
         *  if smite ebtter get to buff then other camps
         * 
         */

        public static JungleCamp GetBestCampToGo()
        {
            var minPriority = GetPriorityNumber(Enumerable.First(HypaJungle.JTimer.JungleCamps));
            JungleCamp bestCamp = null;
            foreach (var jungleCamp in HypaJungle.JTimer.JungleCamps)
            {
                if (SkipCamp != null && SkipCamp.CampId == jungleCamp.CampId)
                {
                    continue;
                }

                var piro = GetPriorityNumber(jungleCamp);
                if (minPriority <= piro)
                {
                    continue;
                }

                bestCamp = jungleCamp;
                minPriority = piro;
            }
            SkipCamp = null;
            return bestCamp;
        }

        public static int GetPriorityNumber(JungleCamp camp)
        {
            if (camp.IsDragBaron)
            {
                return 999;
            }

            if (((camp.Team == 0 && HypaJungle.Player.Team == GameObjectTeam.Chaos)
                 || (camp.Team == 1 && HypaJungle.Player.Team == GameObjectTeam.Order)) &&
                !HypaJungle.Config.Item("enemyJung").GetValue<bool>())
            {
                return 999;
            }

            if (camp.Team == 3 && !HypaJungle.Config.Item("doCrabs").GetValue<bool>())
            {
                return 999;
            }

            var priority = 0;

            var distTillCamp = GetPathLenght(HypaJungle.Player.GetPath(camp.Position));
            var timeToCamp = distTillCamp/HypaJungle.Player.MoveSpeed;
            var spawnTime = (Game.Time < camp.SpawnTime.TotalSeconds)
                ? camp.SpawnTime.TotalSeconds
                : camp.RespawnTimer.TotalSeconds;

            var revOn = camp.ClearTick + (float) spawnTime;
            var timeTillSpawn = (camp.State == JungleCampState.Dead)
                ? ((revOn - Game.Time > 0) ? (revOn - Game.Time) : 0)
                : 0;

            camp.WillKillMe = false;
            if (!Jungler.CanKill(camp, timeToCamp) && HypaJungle.Config.Item("checkKillability").GetValue<bool>())
            {
                priority += 999;
                camp.WillKillMe = true;
            }
            priority -= camp.BonusPrio;
            priority += (int) timeToCamp;
            priority += (int) timeTillSpawn;
            priority -= (camp.IsBuff) ? Jungler.BuffPriority : 0;
            //priority -= (int)(timeTillSpawn - timeToCamp);
            //alive on come is better ;)
            //Priority focus!!
            if (Player.Level <= 3)
            {
                if ((camp.CampId == 10 || camp.CampId == 4) & Jungler.startCamp == Jungler.StartCamp.Red)
                {
                    priority -= 5;
                }

                if ((camp.CampId == 1 || camp.CampId == 7) & Jungler.startCamp == Jungler.StartCamp.Blue)
                {
                    priority -= 5;
                }

                if ((camp.CampId == 11 || camp.CampId == 5) & Jungler.startCamp == Jungler.StartCamp.Golems)
                {
                    priority -= 5;
                }

                if ((camp.CampId == 14 || camp.CampId == 13) & Jungler.startCamp == Jungler.StartCamp.Frog)
                {
                    priority -= 5;
                }
            }


            camp.Priority = priority;
            camp.TimeToCamp = timeToCamp;

            //if(!camp.isBuff)
            //  priority -= (isInBuffWay(camp)) ? 10 : 0;

            return priority;
        }

        public static bool IsInBuffWay(JungleCamp camp)
        {
            var bestBuff = GetBestBuffCamp();
            if (bestBuff == null)
            {
                return false;
            }

            var distTobuff = bestBuff.Position.Distance(HypaJungle.Player.Position, true);
            var distToCamp = camp.Position.Distance(HypaJungle.Player.Position, true);
            var distCampToBuff = camp.Position.Distance(bestBuff.Position, true);
            return distTobuff > distToCamp + 800 && distTobuff > distCampToBuff;
        }

        public static JungleCamp GetBestBuffCamp()
        {
            if (!Enumerable.Any(HypaJungle.JTimer.JungleCamps, cp => cp.IsBuff))
            {
                return null;
            }

            var bestCamp =
                Enumerable.Where(HypaJungle.JTimer.JungleCamps, cp => cp.IsBuff)
                    .OrderByDescending(GetPriorityNumber)
                    .First();
            return bestCamp;
        }

        public static float GetPathLenght(Vector3[] vecs)
        {
            float dist = 0;
            var from = vecs[0];
            foreach (var vec in vecs)
            {
                dist += Vector3.Distance(from, vec);
                from = vec;
            }
            return dist;
        }
    }
}