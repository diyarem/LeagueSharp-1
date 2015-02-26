using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle.Champions
{
    internal class Udyr : Jungler
    {
        public Udyr()
        {
            SetUpSpells();
            SetUpItems();
            LevelUpSeq = new[] {Q, W, Q, E, Q, R, Q, E, Q, E, R, W, E, W, W, R, W, W};
            BuffPriority = 3;
            startCamp = StartCamp.Frog;
        }

        public override sealed void SetUpSpells()
        {
            Recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 0);
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
                    ItemIds = new List<int> {1039, 2003, 2003, 2003}
                },
                new ItemToShop
                {
                    GoldReach = 700,
                    ItemsMustHave = new List<int> {1039},
                    ItemIds = new List<int> {3715, 1001}
                },
                new ItemToShop
                {
                    GoldReach = 900,
                    ItemsMustHave = new List<int> {3715, 1001},
                    ItemIds = new List<int> {1042, 1042}
                },
                new ItemToShop
                {
                    GoldReach = 700,
                    ItemsMustHave = new List<int> {1042, 1042},
                    ItemIds = new List<int> {3718}
                },
                new ItemToShop
                {
                    GoldReach = 600,
                    ItemsMustHave = new List<int> {1042, 1042, 3715},
                    ItemIds = new List<int> {3718}
                },
                new ItemToShop
                {
                    GoldReach = 9999999,
                    ItemsMustHave = new List<int> {3718},
                    ItemIds = new List<int>()
                }
            };

            #endregion

            CheckItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
            if (Q.IsReady() && ((minion.Health/GetDps(minion) > 1.6f) || Player.Level == 1))
            {
                Q.Cast();
            }
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (W.IsReady() && Player.Health < Player.MaxHealth*0.6f)
            {
                W.Cast();
            }
        }

        public override void UseE(Obj_AI_Minion minion)
        {
            // if (E.IsReady())
            //    E.Cast();
        }

        public override void UseR(Obj_AI_Minion minion)
        {
            if (R.IsReady() && (minion.Health/GetDps(minion) > 1.6f))
            {
                R.Cast();
            }
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
        }

        public override void DoAfterAttack(Obj_AI_Base minion)
        {
        }

        public override void DoWhileRunningIdlin()
        {
            if (Player.MaxMana*0.5f < Player.Mana && E.IsReady() && Player.IsMoving)
            {
                E.Cast();
            }
        }

        public override float GetDps(Obj_AI_Minion minion)
        {
            float dps = 0;
            dps += Q.GetDamage(minion)/Qdata.Cooldown;
            dps += R.GetDamage(minion)/Rdata.Cooldown;
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