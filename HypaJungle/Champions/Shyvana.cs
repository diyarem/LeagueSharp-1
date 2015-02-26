using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle.Champions
{
    internal class Shyvana : Jungler
    {
        public Shyvana()
        {
            SetUpSpells();
            SetUpItems();
            LevelUpSeq = new[] {W, Q, E, W, W, R, W, E, W, E, R, E, E, Q, Q, R, Q, Q};
            BuffPriority = 5;
            GotMana = false;
        }

        public override sealed void SetUpSpells()
        {
            Recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 925);
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
                    GoldReach = 999999,
                    ItemsMustHave = new List<int> {3718},
                    ItemIds = new List<int>()
                }
            };

            #endregion

            CheckItems();
        }

        public override void UseQ(Obj_AI_Minion minion)
        {
            if (Q.IsReady())
            {
                Q.Cast();
            }
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (W.IsReady())
            {
                W.Cast();
            }
        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && Player.Distance(minion) < E.Range)
            {
                E.Cast(minion.Position);
            }
        }

        public override void UseR(Obj_AI_Minion minion)
        {
        }

        public override void AttackMinion(Obj_AI_Minion minion, bool onlyAa)
        {
            UseW(minion);
            if (JungleOrbwalker.CanAttack())
            {
                UseQ(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.AttackMinion(minion, minion.Position.To2D().Extend(Player.Position.To2D(), 100).To3D());
        }

        public override void CastWhenNear(JungleCamp camp)
        {
            if (JungleClearer.FocusedCamp == null || !E.IsReady())
            {
                return;
            }

            var dist = Player.Distance(JungleClearer.FocusedCamp.Position);
            if (dist < E.Range*0.8f && dist > 200)
            {
                E.Cast(camp.Position);
            }
        }

        public override void DoAfterAttack(Obj_AI_Base minion)
        {
            var aiMinion = minion as Obj_AI_Minion;
            if (aiMinion != null)
            {
                UseQ(aiMinion);
            }
        }

        public override void DoWhileRunningIdlin()
        {
            if (JungleClearer.FocusedCamp == null || !E.IsReady())
            {
                return;
            }

            var dist = Player.Distance(JungleClearer.FocusedCamp.Position);
            if (dist/Player.MoveSpeed > 8)
            {
                UseW(null);
            }
        }

        public override float GetDps(Obj_AI_Minion minion)
        {
            float dps = 0;
            if (Q.Level != 0)
            {
                dps += Q.GetDamage(minion)/Qdata.Cooldown;
            }

            if (W.Level != 0)
            {
                dps += W.GetDamage(minion)/Qdata.Cooldown;
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
    }
}