using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace RivenSharpV2
{
    internal class Riven
    {
        public static Spell Q = new Spell(SpellSlot.Q, 280);
        public static Spell Q2 = new Spell(SpellSlot.Q, 280);
        public static Spell Q3 = new Spell(SpellSlot.Q, 280);
        public static Spell W = new Spell(SpellSlot.W, 260);
        public static Spell E = new Spell(SpellSlot.E, 390);
        public static Spell R = new Spell(SpellSlot.R, 900);
        public static SummonerItems SumItems;
        public static int QStage = 0;
        public static bool RushDown;
        public static bool RushDownQ;
        public static bool ForceQ = false;

        public static void SetSkillshots()
        {
            R.SetSkillshot(0.25f, 300f, 1400f, false, SkillshotType.SkillshotCone);
            SumItems = new SummonerItems(Player);
        }

        #region laneClear

        public static void DoLaneClear()
        {
            var target = LxOrbwalker.GetPossibleTarget();
            if (!(target is Obj_AI_Minion))
            {
                return;
            }

            UseESmart(target);
            UseWSmart(target);
            UseHydra(target);
        }

        #endregion

        public static void DoCombo(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (target.Distance(Player) < 500)
            {
                SumItems.cast(SummonerItems.ItemIds.Ghostblade);
            }

            if (target.Distance(Player) < 500 && (Player.Health/Player.MaxHealth)*100 < 85)
            {
                SumItems.cast(SummonerItems.ItemIds.BotRk, target);
            }

            IgniteIfKIllable(target);
            RushDownQ = RushDmgBasedOnDist(target)*0.7f > target.Health;
            RushDown = RushDmgBasedOnDist(target)*1.1f > target.Health;
            if (RivenSharp.Config.Item("useR").GetValue<bool>())
            {
                UseRSmart(target);
            }

            if (RushDown)
            {
                SumItems.CastIgnite((Obj_AI_Hero) target);
            }

            UseESmart(target);
            UseWSmart(target);
            UseHydra(target);
            GapWithQ(target);
        }

        public static void DoHarasQ(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            var dist = Player.Distance(target.ServerPosition);

            // W Logic
            if (dist < (W.Range + 50) && W.IsReady())
            {
                W.Cast();
                UseHydra(target);
            }

            // Q Logic
            if (GetQJumpCount() > 0)
            {
                // Player.IssueOrder(GameObjectOrder.AttackUnit, target);

                if (!Player.Spellbook.IsChanneling && Q.IsReady() && dist > Player.AttackRange + Player.BoundingRadius
                    /* && (getQJumpCount()*175 + target.BoundingRadius + 100) < dist*/)
                {
                    Q.Cast(target.Position);
                }
            } // Get away logic
            else if (dist < (target.AttackRange + target.BoundingRadius + 150))
            {
                var closestTower =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(tur => tur.IsAlly)
                        .OrderBy(tur => tur.Distance(Player.Position))
                        .First();
                Player.IssueOrder(GameObjectOrder.MoveTo, closestTower.Position);
                if (E.IsReady())
                {
                    E.Cast(closestTower.Position);
                }
            }
        }

        public static void DoHarasE(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            var dist = Player.Distance(target.ServerPosition);
            if (E.IsReady() && dist < 540)
            {
                E.Cast(target.Position);
            }

            if (dist < (W.Range + 50) && W.IsReady() && !E.IsReady())
            {
                W.Cast();
                UseHydra(target);
            }

            if (GetQJumpCount() > 1 && !E.IsReady())
            {
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);

                if (!Player.Spellbook.IsChanneling && Q.IsReady() && dist > Player.AttackRange + Player.BoundingRadius
                    /* && (getQJumpCount()*175 + target.BoundingRadius + 100) < dist*/)
                {
                    Q.Cast(target.Position);
                }
            } // Get away
            else if (GetQJumpCount() == 1 && !W.IsReady())
            {
                var closestTower =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(tur => tur.IsAlly)
                        .OrderBy(tur => tur.Distance(Player.Position))
                        .First();
                Player.IssueOrder(GameObjectOrder.MoveTo, closestTower.ServerPosition);
                if (Q.IsReady() && IsRunningTo(closestTower))
                {
                    Q.Cast(closestTower.Position);
                }
            }
        }

        public static void GapWithQ(Obj_AI_Base target)
        {
            if ((E.IsReady() || !Q.IsReady()) && !RushDownQ)
            {
                return;
            }

            ReachWithQ(target);
        }

        public static void UseWSmart(Obj_AI_Base target, bool aaRange = false)
        {
            if (!W.IsReady())
            {
                return;
            }

            float range;
            if (aaRange)
            {
                range = Player.AttackRange + target.BoundingRadius;
            }
            else
            {
                range = W.Range + target.BoundingRadius - 40;
            }

            if (W.IsReady() && target.Distance(Player.ServerPosition) < range)
            {
                W.Cast();
            }
        }

        public static void UseESmart(Obj_AI_Base target)
        {
            if (!E.IsReady())
            {
                return;
            }

            var trueAaRange = Player.AttackRange + target.BoundingRadius;
            var trueERange = target.BoundingRadius + E.Range;
            var dist = Player.Distance(target);
            var path = Player.GetPath(target.Position);
            if (!target.IsMoving && dist < trueERange)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, target.Position);
                E.Cast(path.Count() > 1 ? path[1] : target.ServerPosition);
            }

            if ((dist > trueAaRange && dist < trueERange) || RushDown)
            {
                E.Cast(path.Count() > 1 ? path[1] : target.ServerPosition);
            }
        }

        public static void UseRSmart(Obj_AI_Base target)
        {
            if (!R.IsReady())
            {
                return;
            }

            if (!UltIsOn() && !E.IsReady() && target.Distance(Player.ServerPosition) < (Q.Range + target.BoundingRadius))
            {
                R.Cast();
            }
            else if (CanUseWindSlash() && target is Obj_AI_Hero &&
                     (!(E.IsReady() && Player.IsDashing()) || Player.Distance(target) > 150))
            {
                var targ = (Obj_AI_Hero) target;
                var po = R.GetPrediction(targ, true);
                if (!(GetTrueRDmgOn(targ) > ((targ.Health))) && !RushDown)
                {
                    return;
                }

                if (po.Hitchance > HitChance.Medium && Player.Distance(po.UnitPosition) > 30)
                {
                    R.Cast(Player.Distance(po.UnitPosition) < 150 ? target.Position : po.UnitPosition);
                }
            }
        }

        public static bool UseHydra(Obj_AI_Base target)
        {
            if (!(target.Distance(Player.ServerPosition) < (400 + target.BoundingRadius - 20)))
            {
                return false;
            }

            SumItems.cast(SummonerItems.ItemIds.Tiamat);
            SumItems.cast(SummonerItems.ItemIds.Hydra);
            return true;
        }

        public static Vector3 DifPos()
        {
            var pPos = Player.ServerPosition;
            return pPos + new Vector3(300, 300, 0);
        }

        public static void ReachWithQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || Player.IsDashing())
            {
                return;
            }

            var trueAaRange = Player.AttackRange + target.BoundingRadius + 20;
            var trueQRange = target.BoundingRadius + Q.Range + 30;

            var dist = Player.Distance(target);
            var walkPos = new Vector2();
            if (target.IsMoving && target.Path.Count() != 0)
            {
                var tpos = target.Position.To2D();
                var path = target.Path[0].To2D() - tpos;
                path.Normalize();
                walkPos = tpos + (path*100);
            }

            var targMs = (target.IsMoving && Player.Distance(walkPos) > dist) ? target.MoveSpeed : 0;
            var msDif = (Player.MoveSpeed - targMs) == 0 ? 0.0001f : (Player.MoveSpeed - targMs);
            var timeToReach = (dist - trueAaRange)/msDif;
            if ((!(dist > trueAaRange) || !(dist < trueQRange)) && !RushDown)
            {
                return;
            }

            if (!(timeToReach > 2.5) && !(timeToReach < 0.0f) && !RushDown)
            {
                return;
            }

            // var to = Player.Position.To2D().Extend(target.Position.To2D(), 50);
            // Player.IssueOrder(GameObjectOrder.MoveTo,to.To3D());
            Q.Cast(target.ServerPosition);
        }

        public static void ReachWithE(Obj_AI_Base target)
        {
            if (!E.IsReady())
            {
                return;
            }

            var trueAaRange = Player.AttackRange + target.BoundingRadius;
            var trueERange = target.BoundingRadius + E.Range;
            var dist = Player.Distance(target);
            var walkPos = new Vector2();
            if (target.IsMoving)
            {
                var tpos = target.Position.To2D();
                var path = target.Path[0].To2D() - tpos;
                path.Normalize();
                walkPos = tpos + (path*100);
            }

            var targMs = (target.IsMoving && Player.Distance(walkPos) > dist) ? target.MoveSpeed : 0;
            var msDif = (Player.MoveSpeed - targMs) == 0 ? 0.0001f : (Player.MoveSpeed - targMs);
            var timeToReach = (dist - trueAaRange)/msDif;
            if (!(dist > trueAaRange) || (!(dist < trueERange) && !RushDown))
            {
                return;
            }

            if (timeToReach > 1.7f || timeToReach < 0.0f)
            {
                E.Cast(target.ServerPosition);
            }
        }

        public static float GetTrueRDmgOn(Obj_AI_Base target, float minus = 0)
        {
            var baseDmg = 40 + 40*R.Level + 0.6f*Player.FlatPhysicalDamageMod;
            var eneMissHpProc = ((((target.MaxHealth - target.Health - minus)/target.MaxHealth)*100f) > 75f)
                ? 75f
                : (((target.MaxHealth - target.Health)/target.MaxHealth)*100f);
            var multiplier = 1 + (eneMissHpProc*2.66f)/100;
            return (float) Player.CalcDamage(target, Damage.DamageType.Physical, baseDmg*multiplier);
        }

        public static void MoveTo(Vector3 pos)
        {
            if (RivenSharp.Config.Item("forceQE").GetValue<bool>())
            {
                Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pos.X, pos.Y)).Send();
            }
            else
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, pos);
            }
        }

        public static void CancelAnim(bool aaToo = false)
        {
            if (aaToo)
            {
                LxOrbwalker.ResetAutoAttackTimer();
            }

            if (LxOrbwalker.GetPossibleTarget() != null && !UseHydra(LxOrbwalker.GetPossibleTarget()))
            {
                if (W.IsReady())
                {
                    UseWSmart(LxOrbwalker.GetPossibleTarget());
                }
            }

            MoveTo(Game.CursorPos);
            // Game.Say("/l");
            // Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(fill iterator up)).Send();
        }

        public static bool IsRunningTo(Obj_AI_Base target)
        {
            var closestTower =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsAlly)
                    .OrderBy(tur => tur.Distance(Player.Position))
                    .First();
            var dist = Player.Distance(closestTower);
            if (Player.Path.Length <= 0)
            {
                return false;
            }

            var run = Player.Position + Vector3.Normalize(Player.Path[0] - Player.Position)*100;
            return (dist > run.Distance(closestTower.Position));
        }

        public static bool IsImmobileTarg(Obj_AI_Base target)
        {
            return
                target.Buffs.Any(
                    buff =>
                        buff.Type == BuffType.Knockback || buff.Type == BuffType.Knockup || buff.Type == BuffType.Fear ||
                        buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt);
        }

        public static bool UltIsOn()
        {
            return Player.Buffs.Any(buf => buf.Name == "RivenFengShuiEngine");
        }

        public static bool CanUseWindSlash()
        {
            return Player.Buffs.Any(buf => buf.Name == "rivenwindslashready");
        }

        public static int GetQJumpCount()
        {
            try
            {
                var buff = Player.Buffs.First(buf => buf.Name == "RivenTriCleave");
                return 3 - buff.Count;
            }
            catch (Exception)
            {
                return !Q.IsReady() ? 0 : 3;
            }
        }

        public static float GetTrueQDmOn(Obj_AI_Base target)
        {
            return (float) Player.CalcDamage(target, Damage.DamageType.Physical, -10 + (Q.Level*20) +
                                                                                 (0.35 + (Q.Level*0.05))*
                                                                                 (Player.FlatPhysicalDamageMod +
                                                                                  Player.BaseAttackDamage));
        }

        public static float RushDmgBasedOnDist(Obj_AI_Base target)
        {
            var multi = 1.0f;
            if (!UltIsOn() && R.IsReady())
            {
                multi = 1.2f;
            }

            var qDmg = GetTrueQDmOn(target);
            var wDmg = (E.IsReady()) ? (float) Player.GetSpellDamage(target, SpellSlot.W) : 0;
            var adDmg = (float) Player.GetAutoAttackDamage(target);
            var rDmg = (R.IsReady() && (CanUseWindSlash() || !UltIsOn())) ? GetTrueRDmgOn(target) : 0;
            var trueAaRange = Player.AttackRange + target.BoundingRadius - 15;
            var dist = Player.Distance(target.ServerPosition);
            var eCan = (E.IsReady()) ? E.Range : 0;
            var qTimes = GetQJumpCount();
            var adTimes = 0;
            if (E.IsReady())
            {
                adTimes++;
            }

            dist -= eCan;
            dist -= trueAaRange;
            while (dist > 0 && qTimes > 0)
            {
                dist -= Player.AttackRange + 50;
                qTimes--;
            }

            if (dist < 0)
            {
                adTimes++;
            }

            // Console.WriteLine("times: "+Qtimes);
            // Console.WriteLine("Q: " + Qdmg );
            // Console.WriteLine("W: " + Wdmg);
            // Console.WriteLine("AD: " + ADdmg * ADtimes);
            return (qDmg*qTimes + wDmg + adDmg*adTimes + rDmg)*multi;
        }

        public static void IgniteIfKIllable(Obj_AI_Base target)
        {
            // Console.WriteLine("cast ignite");
            if (!(target is Obj_AI_Hero))
            {
                return;
            }

            if (target.Health < 50 + 20*Player.Level)
            {
                SumItems.CastIgnite((Obj_AI_Hero) target);
            }
        }

        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spellbook SBook = Player.Spellbook;
        public static SpellDataInst Qdata = SBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = SBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = SBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = SBook.GetSpell(SpellSlot.R);
    }
}