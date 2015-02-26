using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle.Champions
{
    internal class Warwick : Jungler
    {
        public Warwick()
        {
            SetUpSpells();
            SetUpItems();
            LevelUpSeq = new[] {W, Q, W, E, W, R, W, Q, W, Q, R, Q, Q, E, E, R, E, E};
        }

        public override sealed void SetUpSpells()
        {
            Recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 1250);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 700);
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
            if (!Q.IsReady())
            {
                return;
            }

            var dmg = Q.GetDamage(minion);
            if ((Player.Level <= 7 && (Player.MaxHealth - Player.Health) > dmg) || Player.Level > 7)
            {
                Q.Cast(minion);
            }
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (W.IsReady() && minion.Health/GetDps(minion) > 7 && Player.Distance(minion) < 300)
            {
                W.Cast();
            }
        }

        public override void UseE(Obj_AI_Minion minion)
        {
        }

        public override void UseR(Obj_AI_Minion minion)
        {
        }

        public override void AttackMinion(Obj_AI_Minion minion, bool onlyAa)
        {
            if (JungleOrbwalker.CanAttack())
            {
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
            UseQ((Obj_AI_Minion) minion);
        }

        public override void DoWhileRunningIdlin()
        {
        }

        public override float GetDps(Obj_AI_Minion minion)
        {
            float dps = 0;
            dps += (float) Player.GetAutoAttackDamage(minion)*Player.AttackSpeedMod;
            dps += 30;
            DpsFix = dps;

            return dps;
        }

        public override bool CanMove()
        {
            return true;
        }

        public override float CanHeal(float inTime, float killtime)
        {
            var heal = killtime*Player.AttackSpeedMod*(2.5f + 0.5f*Player.Level)*3;

            if (Q.Level != 0)
            {
                heal += 25 + 50*Q.Level;
            }

            return Player.HPRegenRate*inTime + heal;
        }
    }
}