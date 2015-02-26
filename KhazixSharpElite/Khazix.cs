using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace KhazixSharp
{
    internal class Khazix
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q = new Spell(SpellSlot.Q, 325);
        public static Spell W = new Spell(SpellSlot.W, 1000);
        public static Spell E = new Spell(SpellSlot.E, 700);
        public static Spell R = new Spell(SpellSlot.R, 0);
        public static SummonerItems SumItems;

        public static void SetSkillshots()
        {
            W.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.250f, 100f, 1000f, false, SkillshotType.SkillshotCircle);
            SumItems = new SummonerItems(Player);
        }

        public static void CheckUpdatedSpells()
        {
            if (Qdata.Name == "khazixqlong")
            {
                Q.Range = 375;
            }

            if (Edata.Name == "khazixelong")
            {
                E.Range = 1000;
            }

            /* foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(Rdata))
                {
                    string name=descriptor.Name;
                    object value = descriptor.GetValue(Rdata);
                    Console.WriteLine("{0}={1}",name,value);
                }*/
        }

        public static void DoCombo(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
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

            if (IsStealthed() && TargIsKillabe(target) && EnemiesNear() > 2)
            {
                Orbwalking.Attack = false;
            }
            else
            {
                UseHydra(target);
                DoQ(target);
                Orbwalking.Attack = true;
                DoSmartW(target);
                if (target.Health < FullComboDmgOn(target)*1.3f || IsStealthed())
                {
                    DoSmartE(target, true);
                }
                else
                {
                    ReachWithE(target);
                }
            }
            DoSmartR(target);
        }

        public static void DoHarass(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            UseHydra(target);
            DoQ(target);
            DoSmartW(target);
        }

        public static void DoQ(Obj_AI_Base target)
        {
            if (InSpellRange(target, Q) && Q.IsReady())
            {
                Q.Cast(target);
            }
        }

        public static void DoSmartW(Obj_AI_Base target)
        {
            if (!W.IsReady() || !target.IsValidTarget())
            {
                return;
            }

            var po = W.GetPrediction(target);
            if (po.Hitchance > HitChance.Low)
            {
                W.Cast(po.CastPosition);
            }
        }

        public static void DoSmartE(Obj_AI_Base target, bool kill = false)
        {
            if (!E.IsReady() || TimeToReachAa(target) < 0.3f || !target.IsValidTarget())
            {
                return;
            }

            Console.WriteLine("do some jumpy");
            var po = E.GetPrediction(target);
            if (po.Hitchance > HitChance.Medium)
            {
                E.Cast((kill) ? po.CastPosition : po.UnitPosition);
            }
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
            var timeToReach = TimeToReachAa(target);
            if (!(dist > trueAaRange) || (!(dist < trueERange)))
            {
                return;
            }

            if (timeToReach > 2.2f)
            {
                DoSmartE(target);
            }
        }

        public static float TimeToReachAa(Obj_AI_Base target)
        {
            var trueAaRange = Player.AttackRange + target.BoundingRadius;
            var dist = Player.Distance(target);
            var walkPos = new Vector2();
            if (target.IsMoving && target.Path.Count() > 0)
            {
                var tpos = target.Position.To2D();
                var path = target.Path[0].To2D() - tpos;
                path.Normalize();
                walkPos = tpos + (path*100);
            }
            var targMs = (target.IsMoving && Player.Distance(walkPos) > dist) ? target.MoveSpeed : 0;
            var msDif = (Player.MoveSpeed - targMs) == 0 ? 0.0001f : (Player.MoveSpeed - targMs);
            var timeToReach = (dist - trueAaRange)/msDif;

            return (timeToReach >= 0) ? timeToReach : float.MaxValue;
        }

        public static void DoSmartR(Obj_AI_Base target)
        {
            if (!R.IsReady())
            {
                return;
            }

            var dist = Player.Distance(target);
            if (EnemiesNear() <= 2 && GotPassiveDmg() && (!(TimeToReachAa(target) > 1f) || !TargIsKillabe(target)))
            {
                return;
            }

            if (Player.Distance(target) < 375 && (!Q.IsReady() || E.IsReady()) && (dist > Q.Range || !Q.IsReady()))
            {
                R.Cast();
            }
        }

        public static bool UseHydra(Obj_AI_Base target)
        {
            if (!(target.Distance(Player.ServerPosition) < (400 + target.BoundingRadius - 20)))
            {
                return false;
            }

            Items.UseItem(3074, target);
            Items.UseItem(3077, target);
            return true;
        }

        public static float GetBestRange()
        {
            if (E.IsReady())
            {
                return E.Range + Orbwalking.GetRealAutoAttackRange(Player);
            }

            return W.IsReady() ? W.Range : Q.Range;
        }

        public static float FullComboDmgOn(Obj_AI_Base target)
        {
            var dmg = 0f;
            if (GotPassiveDmg())
            {
                dmg +=
                    (float)
                        Player.CalcDamage(target, Damage.DamageType.Magical,
                            10 + 10*Player.Level + 0.5*Player.FlatMagicDamageMod);
                // DamageLib.CalcMagicDmg(10 + 10* Player.Level + 0.5*Player.FlatMagicDamageMod,target);
            }

            if (Q.IsReady())
            {
                if (TargetIsIsolated(target))
                {
                    dmg += Q.GetDamage(target, 1);
                    // DamageLib.getDmg(target, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage);
                }
                else
                {
                    dmg += Q.GetDamage(target);
                }
            }

            if (W.IsReady())
            {
                dmg += W.GetDamage(target);
            }

            if (E.IsReady())
            {
                dmg += E.GetDamage(target);
            }

            return dmg;
        }

        public static bool TargIsKillabe(Obj_AI_Base target)
        {
            return target.Health > FullComboDmgOn(target)*1.2f;
        }

        public static bool GotPassiveDmg()
        {
            return Player.Buffs.Any(buf => buf.Name == "khazixpdamage");
        }

        public static bool IsStealthed()
        {
            return Player.Buffs.Any(buf => buf.Name == "khazixrstealth");
        }

        public static int EnemiesNear()
        {
            return ObjectManager.Get<Obj_AI_Base>().Count(ene => ene.IsEnemy && ene.IsValidTarget(600));
        }

        public static float TargIsReach(Obj_AI_Base target)
        {
            var dist = target.Distance(Player);
            var range = Orbwalking.GetRealAutoAttackRange(target);
            if (Q.IsReady())
            {
                range = Q.Range;
            }

            range += E.Range;
            return dist - range;
        }

        public static bool TargetIsIsolated(Obj_AI_Base target)
        {
            var enes = ObjectManager.Get<Obj_AI_Base>()
                .Where(her => her.IsEnemy && her.NetworkId != target.NetworkId && target.Distance(her) < 500)
                .ToArray();
            return !enes.Any();
        }

        public static bool InSpellRange(Obj_AI_Base target, Spell s)
        {
            var targBb = target.BoundingRadius*0.9f;
            var dist = Vector3.DistanceSquared(target.Position, Player.Position);
            return dist < (targBb + s.Range)*(targBb + s.Range);
        }

        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spellbook SBook = Player.Spellbook;
        public static SpellDataInst Qdata = SBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = SBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = SBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = SBook.GetSpell(SpellSlot.R);
    }
}