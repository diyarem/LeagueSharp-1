using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace TryndSharp
{
    internal class Trynd
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q = new Spell(SpellSlot.Q, 0);
        public static Spell W = new Spell(SpellSlot.W, 400);
        public static Spell E = new Spell(SpellSlot.E, 660);
        public static Spell R = new Spell(SpellSlot.R, 0);

        public static void DoCombo(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget())
            {
                return;
            }

            // Console.WriteLine("Double COmbo");
            // if (TryndSharp.Config.Item("useQ").GetValue<bool>())
            UseQSmart();
            if (TryndSharp.Config.Item("useW").GetValue<bool>())
            {
                UseWSmart(target);
            }

            if (TryndSharp.Config.Item("useE").GetValue<bool>())
            {
                UseESmart(target);
            }
        }

        public static void SetSkillShots()
        {
            E.SetSkillshot(0.5f, 225f, 700f, false, SkillshotType.SkillshotLine);
        }

        public static void UseQSmart()
        {
            if (!Q.IsReady())
            {
                return;
            }

            if (MyHpProc() <= TryndSharp.Config.Item("QonHp").GetValue<Slider>().Value)
            {
                Q.Cast();
            }
        }

        public static void UseWSmart(Obj_AI_Hero target)
        {
            if (!W.IsReady())
            {
                return;
            }

            // Console.WriteLine("use W");
            var trueAaRange = Player.AttackRange + target.BoundingRadius;
            var trueERange = target.BoundingRadius + W.Range;
            var dist = Player.Distance(target);
            var dashPos = new Vector2();
            if (target.IsMoving)
            {
                var tpos = target.Position.To2D();
                var path = target.Path[0].To2D() - tpos;
                path.Normalize();
                dashPos = tpos + (path*100);
            }

            var targMs = (target.IsMoving && Player.Distance(dashPos) > dist) ? target.MoveSpeed : 0;
            var msDif = (Player.MoveSpeed - targMs) == 0 ? 0.0001f : (Player.MoveSpeed - targMs);
            var timeToReach = (dist - trueAaRange)/msDif;
            // Console.WriteLine(timeToReach);
            if (!(dist > trueAaRange) || !(dist < trueERange))
            {
                return;
            }

            if (timeToReach > 1.7f || timeToReach < 0.0f)
            {
                W.Cast();
            }
        }

        public static void UseESmart(Obj_AI_Hero target)
        {
            if (!E.IsReady())
            {
                return;
            }

            // Console.WriteLine("use E");
            var trueAaRange = Player.AttackRange + target.BoundingRadius;
            var trueERange = target.BoundingRadius + E.Range;
            var dist = Player.Distance(target);
            var movePos = new Vector2();
            if (target.IsMoving)
            {
                var tpos = target.Position.To2D();
                var path = target.Path[0].To2D() - tpos;
                path.Normalize();
                movePos = tpos + (path*100);
            }

            var targMs = (target.IsMoving && Player.Distance(movePos) > dist) ? target.MoveSpeed : 0;
            var msDif = (Player.MoveSpeed - targMs) == 0 ? 0.0001f : (Player.MoveSpeed - targMs);
            var timeToReach = (dist - trueAaRange)/msDif;
            // Console.WriteLine(timeToReach);
            if (!(dist > trueAaRange) || !(dist < trueERange))
            {
                return;
            }

            if (timeToReach > 1.7f || timeToReach < 0.0f)
            {
                E.Cast(target);
            }
        }

        public static int MyHpProc()
        {
            return (int) ((Player.Health/Player.MaxHealth)*100);
        }

        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spellbook SBook = Player.Spellbook;
        public static SpellDataInst Qdata = SBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = SBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = SBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = SBook.GetSpell(SpellSlot.R);
    }
}