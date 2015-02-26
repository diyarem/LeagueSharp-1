using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle.Champions
{
    internal class MasterYi : Jungler
    {
        public bool StartedMedi;

        public MasterYi()
        {
            SetUpSpells();
            SetUpItems();
            LevelUpSeq = new[] {Q, W, E, Q, Q, R, Q, E, Q, E, R, E, E, W, W, R, W};
            BuffPriority = 3;
        }

        public override sealed void SetUpSpells()
        {
            Recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 600);
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
                    ItemIds = new List<int> {1039, 2003, 2003, 3166}
                },
                new ItemToShop
                {
                    GoldReach = 350,
                    ItemsMustHave = new List<int> {1039},
                    ItemIds = new List<int> {3715}
                },
                new ItemToShop
                {
                    GoldReach = 350,
                    ItemsMustHave = new List<int> {3715},
                    ItemIds = new List<int> {1001}
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
            if (Q.IsReady() && minion.Health > Q.GetDamage(minion))
            {
                Q.Cast(minion);
            }
        }

        public override void UseW(Obj_AI_Minion minion)
        {
        }

        public override void UseE(Obj_AI_Minion minion)
        {
            if (E.IsReady() && minion.Health/GetDps(minion) > 4)
            {
                E.Cast();
            }
        }

        public override void UseR(Obj_AI_Minion minion)
        {
        }

        public override void AttackMinion(Obj_AI_Minion minion, bool onlyAa)
        {
            //  if (onlyAA)return;

            if (JungleOrbwalker.CanAttack())
            {
                if (minion.Distance(Player) > 300)
                {
                    UseQ(minion);
                }

                UseW(minion);
                UseE(minion);
                UseR(minion);
            }
            JungleOrbwalker.AttackMinion(minion, minion.Position.To2D().Extend(Player.Position.To2D(), 100).To3D());
        }

        public override void CastWhenNear(JungleCamp camp)
        {
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
            if (!W.IsReady() || !(Player.Health < Player.MaxHealth*0.7f))
            {
                return;
            }

            StartedMedi = true;
            W.Cast();
        }

        public override float GetDps(Obj_AI_Minion minion)
        {
            float dps = 0;
            dps += Q.GetDamage(minion)*2/Qdata.Cooldown;
            dps += (float) Player.GetAutoAttackDamage(minion)*1.15f*Player.AttackSpeedMod;
            DpsFix = dps;

            return dps;
        }

        public override bool CanMove()
        {
            if (!Player.HasBuff("Meditate") || Player.Health == Player.MaxHealth)
            {
                return !StartedMedi;
            }

            StartedMedi = false;
            return false;
        }

        public override float CanHeal(float inTime, float killtime)
        {
            float heal = 0;
            if (W.IsReady((int) (inTime*1000)))
            {
                heal = 4*(W.Level*20 + 10 + 0.3f*Player.FlatMagicDamageMod);
            }

            return Player.HPRegenRate*inTime + heal;
        }
    }
}