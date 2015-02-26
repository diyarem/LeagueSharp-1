using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    internal abstract class Jungler
    {
        //Ty tc-crew

        public enum StartCamp
        {
            Any = 0,
            Blue = 1,
            Red = 2,
            Frog = 3,
            Golems = 4
        }

        public static bool CanBuyItems = true;
        public float BonusItemDps;
        public int BuffPriority = 7;
        public List<ItemToShop> BuyThings;
        public float DamageTaken = 1.0f;
        public float DpsFix = 0;
        public Spell E;
        public SpellDataInst Edata = SBook.GetSpell(SpellSlot.E);
        public int ExtraWindUp = 150;
        public bool GotMana = true;
        public bool GotOverTime;
        public Spell[] LevelUpSeq;
        public ItemToShop NextItem;
        public string OverTimeName = string.Empty;
        public Spell Q; //Emp 1470
        public SpellDataInst Qdata = SBook.GetSpell(SpellSlot.Q);
        public Spell R;
        public SpellDataInst Rdata = SBook.GetSpell(SpellSlot.R);
        public Spell Recall;
        public SpellSlot Smite = SpellSlot.Unknown;
        public Spell SmiteSpell;
        public StartCamp startCamp = StartCamp.Any;
        public Spell W;
        public SpellDataInst Wdata = SBook.GetSpell(SpellSlot.W);

        protected Jungler()
        {
            SetupSmite();
        }

        public abstract void SetUpSpells();
        public abstract void SetUpItems();
        public abstract void UseQ(Obj_AI_Minion minion);
        public abstract void UseW(Obj_AI_Minion minion);
        public abstract void UseE(Obj_AI_Minion minion);
        public abstract void UseR(Obj_AI_Minion minion);
        public abstract void AttackMinion(Obj_AI_Minion minion, bool onlyAa);
        public abstract void CastWhenNear(JungleCamp camp);
        public abstract void DoAfterAttack(Obj_AI_Base minion);
        public abstract void DoWhileRunningIdlin();
        public abstract float GetDps(Obj_AI_Minion minion);
        public abstract bool CanMove();
        public abstract float CanHeal(float inTime, float toKillCamp);

        public void SetupSmite()
        {
            if (Player.Spellbook.GetSpell(SpellSlot.Summoner1).SData.Name.ToLower().Contains("smite"))
            {
                Smite = SpellSlot.Summoner1;
                SmiteSpell = new Spell(Smite);
            }
            else if (Player.Spellbook.GetSpell(SpellSlot.Summoner2).SData.Name.ToLower().Contains("smite"))
            {
                Smite = SpellSlot.Summoner2;
                SmiteSpell = new Spell(Smite);
            }
        }

        private void DoSmite(Obj_AI_Base target)
        {
            if (Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready)
            {
                Player.Spellbook.CastSpell(Smite, target);
            }
        }

        public void StartAttack(Obj_AI_Minion minion, bool onlyAa)
        {
            UsePots();
            GetDps(minion);

            if (minion == null || !minion.IsValid || !minion.IsVisible)
            {
                return;
            }

            if (HypaJungle.Config.Item("smiteToKill").GetValue<bool>())
            {
                if (Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite) >= minion.Health)
                {
                    if (((!HypaJungle.JTimer.JungleCamps.Any(cp => cp.IsBuff && cp.State == JungleCampState.Alive)) &&
                         minion.MaxHealth >= 800) ||
                        (JungleClearer.FocusedCamp.IsBuff && minion.MaxHealth >= 1400))
                    {
                        DoSmite(minion);
                    }
                }
            }
            else
            {
                if (minion.Health/GetDps(minion) >
                    ((!HypaJungle.JTimer.JungleCamps.Any(cp => cp.IsBuff)) ? 8 : 5)*
                    (Player.Health/Player.MaxHealth) || (JungleClearer.FocusedCamp.IsBuff && minion.MaxHealth >= 1400))
                {
                    DoSmite(minion);
                }
            }

            AttackMinion(minion, onlyAa);
        }

        public void UsePots()
        {
            if (Player.Health/Player.MaxHealth <= 0.6f && !Player.HasBuff("Health Potion"))
            {
                CastPotion(PotionType.Health);
            }

            // Mana Potion
            if (!GotMana)
            {
                return;
            }

            if (Player.Mana/Player.MaxMana <= 0.3f && !Player.HasBuff("Mana Potion"))
            {
                CastPotion(PotionType.Mana);
            }
        }

        private static void CastPotion(PotionType type)
        {
            try
            {
                Player.Spellbook.CastSpell(Player.InventoryItems.First(
                    item =>
                        item.Id == (type == PotionType.Health ? (ItemId) 2003 : (ItemId) 2004) ||
                        (item.Id == (ItemId) 2010) || (item.Id == (ItemId) 2041 && item.Charges > 0)).SpellSlot);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void SetFirstLvl()
        {
            SBook.LevelUpSpell(LevelUpSeq[0].Slot);
            BuyItems();
            CheckItems();
        }

        public void LevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (sender.NetworkId == Player.NetworkId)
            {
                SBook.LevelUpSpell(LevelUpSeq[args.NewLevel - 1].Slot);
            }
        }

        public bool CanKill(JungleCamp camp, float timeTo)
        {
            if (DpsFix == 0 || camp.Dps == 0)
            {
                return true;
            }

            var realDps = DpsFix + BonusItemDps;
            float bonusDmg = 0;
            var aproxSecTk = (camp.Health - bonusDmg)/realDps;

            var secTillIDie = (Player.Health + CanHeal(timeTo, aproxSecTk))/
                              (camp.Dps*0.8f*DamageTaken*(100 + Player.Level*3)/100);
            if (SmiteSpell.IsReady((int) (secTillIDie + timeTo)*1000))
            {
                bonusDmg += new float[]
                {390, 410, 430, 450, 480, 510, 540, 570, 600, 640, 680, 720, 760, 800, 850, 900, 950, 1000}[
                    Player.Level - 1];
            }

            var secToKill = (camp.Health - bonusDmg)/realDps;
            return !(secToKill*1.1f > secTillIDie);
        }

        public void CheckItems()
        {
            GetItemPassiveBoostDps();
            if (!CanBuyItems)
            {
                return;
            }

            for (var i = BuyThings.Count - 1; i >= 0; i--)
            {
                if (!HasAllItems(BuyThings[i]))
                {
                    continue;
                }

                NextItem = BuyThings[i];
                if (i == BuyThings.Count - 1)
                {
                    CanBuyItems = false;
                }

                return;
            }
        }

        public bool HasAllItems(ItemToShop its)
        {
            var usedItems = new bool[7];
            var itemsMatch = 0;
            foreach (var t in its.ItemsMustHave)
            {
                for (var i = 0; i < Player.InventoryItems.Count(); i++)
                {
                    if (usedItems[i])
                    {
                        continue;
                    }

                    if (t != (int) Player.InventoryItems[i].Id)
                    {
                        continue;
                    }

                    usedItems[i] = true;
                    itemsMatch++;
                    break;
                }
            }
            return itemsMatch == its.ItemsMustHave.Count;
        }

        public void BuyItems()
        {
            if (InSpwan())
            {
                foreach (var item in NextItem.ItemIds.Where(item => !Items.HasItem(item)))
                {
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(item, ObjectManager.Player.NetworkId))
                        .Send();
                }
            }
            CheckItems();
        }

        public bool InSpwan()
        {
            var spawnPos1 = new Vector3(14286f, 14382f, 172f);
            var spawnPos0 = new Vector3(416f, 468f, 182f);
            return Player.Distance(spawnPos1) < 600 || Player.Distance(spawnPos0) < 600;
        }

        public void GetItemPassiveBoostDps()
        {
            float dps = 0;
            //  int[] listNewItemsJng = new[] {3726, 3725, 3723, 3722, 3721, 3720, 3719, 3718, 3717, 3716, 3714,3713,3711, 3710, 3709, 3708, 3707, 3706};
            //3706-3726
            if (JunglerGotItemRange(3706, 3726))
            {
                dps += 45/2;
                GotOverTime = true;
                OverTimeName = "itemmonsterburn";
            }
            BonusItemDps = dps;


            if (JunglerGotItemRange(3714, 3718))
            {
                DamageTaken = 0.8f;
            }
        }

        private static bool JunglerGotItemRange(int from, int to)
        {
            return Player.InventoryItems.Any(item => (int) item.Id >= @from && (int) item.Id <= to);
        }

        public float GetSpellDmgRaw(SpellSlot slot, int stage = 0)
        {
            var spell = Damage.Spells[Player.ChampionName].FirstOrDefault(s => s.Slot == slot && s.Stage == stage);
            if (spell == null)
            {
                return 0;
            }

            var rawDamage =
                (float)
                    spell.Damage(Player, Player, Math.Max(1, Math.Min(Player.Spellbook.GetSpell(slot).Level - 1, 6)));

            return rawDamage;
        }

        private enum PotionType
        {
            Health = 2003,
            Mana = 2004,
            Biscuit = 2009,
            CrystalFlask = 2041
        }

        internal class ItemToShop
        {
            public int GoldReach;
            public List<int> ItemIds;
            public List<int> ItemsMustHave;
        }

        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spellbook SBook = Player.Spellbook;
    }
}