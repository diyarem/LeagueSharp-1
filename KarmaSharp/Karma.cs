using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace KarmaSharp
{
    internal class Karma
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q = new Spell(SpellSlot.Q, 920);
        public static Spell W = new Spell(SpellSlot.W, 675);
        public static Spell E = new Spell(SpellSlot.E, 800);
        public static Spell R = new Spell(SpellSlot.R, 0);

        public static void SetSkillShots()
        {
            Q.SetSkillshot(0.5f, 90f, 1800f, true, SkillshotType.SkillshotLine);
        }

        public static void DoCombo(Obj_AI_Hero target)
        {
            if (KarmaSharp.Config.Item("useQ").GetValue<bool>())
            {
                UseQSmart(target, R.IsReady());
            }

            if (KarmaSharp.Config.Item("useW").GetValue<bool>())
            {
                UseWSmart(target);
            }

            if (KarmaSharp.Config.Item("useE").GetValue<bool>())
            {
                UseESmart();
            }
        }

        public static void DoHarass(Obj_AI_Hero target)
        {
            if (KarmaSharp.Config.Item("useQHar").GetValue<bool>())
            {
                UseQSmart(target, R.IsReady());
            }
        }

        public static void UseQSmart(Obj_AI_Hero target, bool usedR = false)
        {
            if (usedR)
            {
                Q.Range += 210;
            }

            if (!Q.IsReady())
            {
                return;
            }

            var predict = Q.GetPrediction(target);
            if (predict.Hitchance == HitChance.Collision)
            {
                if (KarmaSharp.Config.Item("useMinions").GetValue<bool>())
                {
                    /* List<Obj_AI_Base> enemHeros = new List<Obj_AI_Base>();
                    foreach (Obj_AI_Hero enem in ObjectManager.Get<Obj_AI_Hero>().Where(enem => enem.IsEnemy && enem.IsValidTarget()))
                    {
                        // Vector3 predPos = Q.GetPrediction(enem).Position;
                        enemHeros.Add((Obj_AI_Base)enem);
                    }

                    // Console.WriteLine("minions");
                    predict.CollisionUnitsList.AddRange(enemHeros);
                      Obj_AI_Base fistCol = predict.CollisionUnitsList.OrderBy(unit => (unit is Obj_AI_Hero) ? Player.Distance(Q.GetPrediction(unit).Position) : unit.Distance(Player.ServerPosition)).First();
                   
                     */

                    var fistCol = predict.CollisionObjects.OrderBy(unit => unit.Distance(Player.ServerPosition)).First();
                    if (fistCol.Distance(predict.CastPosition) < (180 - fistCol.BoundingRadius/2) &&
                        fistCol.Distance(target.ServerPosition) < (200 - fistCol.BoundingRadius/2))
                    {
                        // Console.WriteLine("Casted in minions");
                        UseRSmart();
                        Q.Cast(predict.CastPosition);
                        return;
                    }
                }
            }
            else if (predict.Hitchance != HitChance.Impossible && predict.CollisionObjects.Count == 0)
            {
                // Console.WriteLine("Casted " + predict.HitChance);
                UseRSmart();
                Q.Cast(predict.CastPosition);
                return;
            }
            Q.Range = 950;
        }

        public static void UseWSmart(Obj_AI_Hero target)
        {
            if (!W.IsReady())
            {
                return;
            }

            if (Player.Distance(target.ServerPosition) < 675)
            {
                W.Cast(target);
            }
        }

        public static void UseESmart()
        {
            if (!E.IsReady())
            {
                return;
            }

            E.Cast(Player);
            // foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && hero.Distance(Player.ServerPosition)< W.Range))
            // {

            // }
        }

        public static bool UseRSmart()
        {
            if (!R.IsReady() || !KarmaSharp.Config.Item("useR").GetValue<bool>())
            {
                return false;
            }

            R.Cast();
            return true;
        }

        public static List<Vector2> EntitiesAroundTarget(Obj_AI_Hero target)
        {
            var minionsAround = MinionManager.GetMinions(target.ServerPosition, 450, MinionTypes.All, MinionTeam.NotAlly,
                MinionOrderTypes.None);
            var entities = MinionManager.GetMinionsPredictedPositions(minionsAround, 0.5f, 90f, 1800f,
                Player.ServerPosition, 250, true, SkillshotType.SkillshotLine, target.ServerPosition);
            return entities;
        }

        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spellbook SBook = Player.Spellbook;
        public static SpellDataInst Qdata = SBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = SBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = SBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = SBook.GetSpell(SpellSlot.R);
    }
}