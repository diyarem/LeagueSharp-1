using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle.Champions
{
    internal class LeeSin : Jungler
    {
        public LeeSin()
        {
            SetUpSpells();
            SetUpItems();
            LevelUpSeq = new[] {Q, E, W, Q, Q, R, Q, W, Q, W, R, W, W, E, E, R, E, E};
            BuffPriority = 5;
            GotMana = false;
            startCamp = StartCamp.Golems;
        }

        public override sealed void SetUpSpells()
        {
            Recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 700);
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
                    ItemIds = new List<int> {1039, 2003, 2003, 3166}
                },
                new ItemToShop
                {
                    GoldReach = 350,
                    ItemsMustHave = new List<int> {1039},
                    ItemIds = new List<int> {3713}
                },
                new ItemToShop
                {
                    GoldReach = 350,
                    ItemsMustHave = new List<int> {3713},
                    ItemIds = new List<int> {1001}
                },
                new ItemToShop
                {
                    GoldReach = 900,
                    ItemsMustHave = new List<int> {3713, 1001},
                    ItemIds = new List<int> {1042, 1042}
                },
                new ItemToShop
                {
                    GoldReach = 700,
                    ItemsMustHave = new List<int> {1042, 1042},
                    ItemIds = new List<int> {3726}
                },
                new ItemToShop
                {
                    GoldReach = 600,
                    ItemsMustHave = new List<int> {1042, 1042, 3715},
                    ItemIds = new List<int> {3726}
                },
                new ItemToShop
                {
                    GoldReach = 999999,
                    ItemsMustHave = new List<int> {3726},
                    ItemIds = new List<int>()
                }
            };

            #endregion

            CheckItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
            if (!Q.IsReady())
            {
                return;
            }

            if (Q.Instance.Name == "BlindMonkQOne" && BuffCount() < 2)
            {
                if ((minion.Health/GetDps(minion) < 2.3f))
                {
                    return;
                }

                var po = Q.GetPrediction(minion);
                if (po.Hitchance >= HitChance.Low)
                {
                    Q.Cast(po.CastPosition);
                }

                if (po.Hitchance != HitChance.Collision)
                {
                    return;
                }

                if (Player.Distance(minion) > 500)
                {
                    Q.Cast(po.CastPosition);
                }
                Player.IssueOrder(GameObjectOrder.MoveTo, minion.Position);
            }
            else if (BuffCount() == 0 || minion.Distance(Player) > 250)
            {
                Q.Cast();
            }
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (!W.IsReady())
            {
                return;
            }

            if (W.Instance.Name == "BlindMonkWOne" && BuffCount() == 0)
            {
                W.Cast(Player);
            }
            else if (BuffCount() == 0)
            {
                W.Cast();
            }
        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (!E.IsReady() || !(minion.Distance(Player) < 300))
            {
                return;
            }

            if (E.Instance.Name == "BlindMonkEOne" && BuffCount() == 0)
            {
                E.Cast();
            }
            else if (BuffCount() == 0)
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

            if (Q.Instance.Name == "BlindMonkQOne")
            {
                var dist = Player.Distance(JungleClearer.FocusedCamp.Position);
                if (dist < Q.Range*0.9f && dist > 200)
                {
                    Q.Cast(camp.Position);
                }
            }
            else
            {
                Q.Cast();
            }
        }

        public override void DoAfterAttack(Obj_AI_Base minion)
        {
        }

        public override void DoWhileRunningIdlin()
        {
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
                dps += E.GetDamage(minion)/Qdata.Cooldown;
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

        /*
         * Check methods
         */


        public static int BuffCount()
        {
            var buff = Player.Buffs.FirstOrDefault(b => b.Name == "blindmonkpassive_cosmetic");
            return buff == null ? 0 : buff.Count;
        }
    }
}