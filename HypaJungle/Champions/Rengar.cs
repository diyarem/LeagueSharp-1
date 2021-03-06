﻿using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle.Champions
{
    internal class Rengar : Jungler
    {
        public Rengar()
        {
            SetUpSpells();
            SetUpItems();
            LevelUpSeq = new[] {Q, E, W, Q, Q, R, Q, E, Q, E, R, E, E, W, W, R, W, W};
            BuffPriority = 5;
            GotMana = false;
            startCamp = StartCamp.Golems;
        }

        public override sealed void SetUpSpells()
        {
            Recall = new Spell(SpellSlot.Recall);
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 500);
            E = new Spell(SpellSlot.E, 1000);
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
            if (!Q.IsReady())
            {
                return;
            }

            var dmg = Q.GetDamage(minion);
            if (minion.Health >= dmg)
            {
                Q.Cast();
            }
        }

        public override void UseW(Obj_AI_Minion minion)
        {
            if (W.IsReady() && GetEnchCount() != 5 && minion.Distance(Player) < 340 + minion.BoundingRadius &&
                W.GetDamage(minion)*0.7f < minion.Health)
            {
                W.Cast();
            }
        }

        public override void UseE(Obj_AI_Minion minion)
        {
            Console.WriteLine(GetEnchCount());
            if (E.IsReady() && GetEnchCount() != 5 && minion.Distance(Player) < 400)
            {
                E.Cast(minion.Position);
            }
        }

        public override void UseR(Obj_AI_Minion minion)
        {
        }

        public override void AttackMinion(Obj_AI_Minion minion, bool onlyAa)
        {
            if (JungleOrbwalker.CanAttack())
            {
                if (minion.Distance(Player) < 250)
                {
                    if (GetEnchCount() == 5)
                    {
                        UseQ(minion);
                    }

                    UseW(minion);
                    UseE(minion);
                    UseR(minion);
                }
            }
            JungleOrbwalker.AttackMinion(minion, minion.Position.To2D().Extend(Player.Position.To2D(), 150).To3D());
        }

        public override void CastWhenNear(JungleCamp camp)
        {
            if (JungleClearer.FocusedCamp == null || !Q.IsReady() || GetEnchCount() == 5)
            {
                return;
            }

            var dist = Player.Distance(JungleClearer.FocusedCamp.Position);
            if (dist < E.Range*0.9f && dist > 200)
            {
                E.Cast(camp.Position);
            }
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
            if (Q.Level != 0)
            {
                dps += Q.GetDamage(minion)/(Qdata.Cooldown);
            }

            if (W.Level != 0)
            {
                dps += W.GetDamage(minion)/(Qdata.Cooldown);
            }

            if (E.Level != 0)
            {
                dps += E.GetDamage(minion)/(Qdata.Cooldown);
            }

            dps += (float) Player.GetAutoAttackDamage(minion)*Player.AttackSpeedMod;
            DpsFix = dps*1.0f;

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

        public float GetEnchCount()
        {
            return Player.Mana;
        }
    }
}