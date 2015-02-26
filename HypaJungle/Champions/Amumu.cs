using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle.Champions
{
    internal class Amumu : Jungler
    {
        public Amumu()
        {
            SetUpSpells();
            SetUpItems();
            LevelUpSeq = new[] {W, E, Q, E, E, R, E, E, Q, Q, R, Q, Q, W, W, R, W, W};
            BuffPriority = 10;
        }

        public override sealed void SetUpSpells()
        {
            Recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 375);
        }

        public override sealed void SetUpItems()
        {
            #region itemsToBuyList

            BuyThings = new List<ItemToShop>
            {
                new ItemToShop
                {
                    GoldReach = 475,
                    ItemsMustHave = new List<int>(),
                    ItemIds = new List<int> {1039, 2003, 2003, 2003, 2003, 3340}
                },
                new ItemToShop
                {
                    GoldReach = 470,
                    ItemsMustHave = new List<int> {1039},
                    ItemIds = new List<int> {1080, 2003, 2003}
                },
                new ItemToShop
                {
                    GoldReach = 890,
                    ItemsMustHave = new List<int> {1080},
                    ItemIds = new List<int> {3108, 2003, 2003}
                },
                new ItemToShop
                {
                    GoldReach = 805,
                    ItemsMustHave = new List<int> {3108},
                    ItemIds = new List<int> {3206, 1001}
                },
                new ItemToShop
                {
                    GoldReach = 9999999,
                    ItemsMustHave = new List<int> {3206},
                    ItemIds = new List<int>()
                }
            };

            #endregion

            CheckItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
            /* if (Q.IsReady())
            {
                if ((minion.Health / getDPS(minion) < 2.3f))
                    return;

                PredictionOutput po = Q.GetPrediction(minion);
                if (po.Hitchance >= HitChance.Low)
                {
                    Q.Cast(po.CastPosition);
                }
                if (po.Hitchance == HitChance.Collision)
                {
                    player.IssueOrder(GameObjectOrder.MoveTo, minion.Position);
                }
            }*/
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (!W.IsReady())
            {
                return;
            }

            if (W.Instance.ToggleState == 1)
            {
                W.Cast();
            }
        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && minion.Distance(Player) < 340 + minion.BoundingRadius &&
                E.GetDamage(minion)*0.7f < minion.Health)
            {
                E.Cast();
            }
        }

        public override void UseR(Obj_AI_Minion minion)
        {
        }

        public override void AttackMinion(Obj_AI_Minion minion, bool onlyAa)
        {
            if (JungleOrbwalker.CanAttack())
            {
                UseQ(minion);
                UseW(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.AttackMinion(minion, minion.Position.To2D().Extend(Player.Position.To2D(), 150).To3D());
        }

        public override void CastWhenNear(JungleCamp camp)
        {
            if (JungleClearer.FocusedCamp == null || !Q.IsReady())
            {
                return;
            }

            var dist = Player.Distance(JungleClearer.FocusedCamp.Position);
            if (dist < Q.Range*0.9f && dist > 200)
            {
                Q.Cast(camp.Position);
            }
        }

        public override void DoAfterAttack(Obj_AI_Base minion)
        {
        }

        public override void DoWhileRunningIdlin()
        {
            //disable W
            if (!W.IsReady())
            {
                return;
            }

            if (W.Instance.ToggleState == 2)
            {
                W.Cast();
            }
        }

        public override float GetDps(Obj_AI_Minion minion)
        {
            float dps = 0;
            if (Q.Level != 0)
            {
                dps += Q.GetDamage(minion)/Qdata.Cooldown;
            }

            if (E.Level != 0)
            {
                dps += E.GetDamage(minion)/(Qdata.Cooldown - 2);
            }

            dps += (float) Player.GetAutoAttackDamage(minion)*Player.AttackSpeedMod;
            DpsFix = dps;

            return (dps == 0) ? 999 : dps;
        }

        public override bool CanMove()
        {
            return true;
        }

        public override float CanHeal(float inTime, float killtime)
        {
            return Player.HPRegenRate*inTime;
        }
    }
}