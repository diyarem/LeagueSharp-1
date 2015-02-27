using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace JayceSharpV2
{
    internal class Jayce
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q1 = new Spell(SpellSlot.Q, 1050); // Emp 1470
        public static Spell QEmp1 = new Spell(SpellSlot.Q, 1600); // Emp 1470
        public static Spell W1 = new Spell(SpellSlot.W, 0);
        public static Spell E1 = new Spell(SpellSlot.E, 650);
        public static Spell R1 = new Spell(SpellSlot.R, 0);
        public static Spell Q2 = new Spell(SpellSlot.Q, 600);
        public static Spell W2 = new Spell(SpellSlot.W, 285);
        public static Spell E2 = new Spell(SpellSlot.E, 240);
        public static Spell R2 = new Spell(SpellSlot.R, 0);
        public static GameObjectProcessSpellCastEventArgs CastEonQ = null;
        public static Obj_SpellMissile MyCastedQ = null;
        public static Obj_AI_Hero LockedTarg;
        public static Vector3 CastQon = new Vector3(0, 0, 0);
        /* COOLDOWN STUFF */
        public static float[] RangTrueQcd = {8, 8, 8, 8, 8};
        public static float[] RangTrueWcd = {14, 12, 10, 8, 6};
        public static float[] RangTrueEcd = {16, 16, 16, 16, 16};
        public static float[] HamTrueQcd = {16, 14, 12, 10, 8};
        public static float[] HamTrueWcd = {10, 10, 10, 10, 10};
        public static float[] HamTrueEcd = {14, 12, 12, 11, 10};
        public static float RangQcd, RangWcd, RangEcd;
        public static float HamQcd, HamWcd, HamEcd;
        public static float RangQcdRem, RangWcdRem, RangEcdRem;
        public static float HamQcdRem, HamWcdRem, HamEcdRem;
        /* COOLDOWN STUFF END */
        public static bool IsHammer;

        public static void SetSkillShots()
        {
            Q1.SetSkillshot(0.3f, 70f, 1500, true, SkillshotType.SkillshotLine);
            QEmp1.SetSkillshot(0.3f, 70f, 2180, true, SkillshotType.SkillshotLine);
            // QEmp1.SetSkillshot(0.25f, 70f, float.MaxValue, false, Prediction.SkillshotType.SkillshotLine);
        }

        public static void DoCombo(Obj_AI_Hero target)
        {
            CastOmen(target);
            if (!IsHammer)
            {
                if (CastEonQ != null)
                {
                    CastEonSpell(target);
                }

                // DO QE combo first
                if (E1.IsReady() && Q1.IsReady() && GotManaFor(true, false, true))
                {
                    CastQePred(target);
                }
                else if (Q1.IsReady() && GotManaFor(true))
                {
                    CastQPred(target);
                }
                else if (W1.IsReady() && GotManaFor(false, true) && TargetInRange(GetClosestEnem(), 650f))
                {
                    W1.Cast();
                    SumItems.cast(SummonerItems.ItemIds.Ghostblade);
                } // And wont die wih 1 AA
                else if (!Q1.IsReady() && !W1.IsReady() && R1.IsReady() && HammerWillKill(target) && HamQcdRem == 0 &&
                         HamEcdRem == 0) // Will need to add check if other form skills ready
                {
                    R1.Cast();
                }
            }
            else
            {
                if (!Q2.IsReady() && R2.IsReady() && Player.Distance(GetClosestEnem()) > 350)
                {
                    SumItems.cast(SummonerItems.ItemIds.Ghostblade);
                    R2.Cast();
                }

                if (Q2.IsReady() && GotManaFor(true) && TargetInRange(target, Q2.Range) && Player.Distance(target) > 300)
                {
                    SumItems.cast(SummonerItems.ItemIds.Ghostblade);
                    Q2.Cast(target);
                }

                if (E2.IsReady() && GotManaFor(false, false, true) && TargetInRange(target, E2.Range) &&
                    ShouldIKnockDatMadaFaka(target))
                {
                    E2.Cast(target);
                }

                if (W2.IsReady() && GotManaFor(false, true) && TargetInRange(target, W2.Range))
                {
                    W2.Cast();
                }
            }
        }

        public static void DoFullDmg(Obj_AI_Hero target)
        {
            CastIgnite(target);
            if (!IsHammer)
            {
                if (CastEonQ != null)
                {
                    CastEonSpell(target);
                }

                // DO QE combo first
                if (E1.IsReady() && Q1.IsReady() && GotManaFor(true, false, true))
                {
                    CastQePred(target);
                }
                else if (Q1.IsReady() && GotManaFor(true))
                {
                    CastQPred(target);
                }
                else if (W1.IsReady() && GotManaFor(false, true) && TargetInRange(GetClosestEnem(), 1000f))
                {
                    SumItems.cast(SummonerItems.ItemIds.Ghostblade);
                    W1.Cast();
                }
                else if (!Q1.IsReady() && !W1.IsReady() && R1.IsReady() && HamQcdRem == 0 && HamEcdRem == 0)
                    // Will need to add check if other form skills ready
                {
                    R1.Cast();
                }
            }
            else
            {
                if (!Q2.IsReady() && R2.IsReady() && Player.Distance(GetClosestEnem()) > 350)
                {
                    SumItems.cast(SummonerItems.ItemIds.Ghostblade);
                    R2.Cast();
                }

                if (Q2.IsReady() && GotManaFor(true) && TargetInRange(target, Q2.Range))
                {
                    Q2.Cast(target);
                }

                if (E2.IsReady() && GotManaFor(false, false, true) && TargetInRange(target, E2.Range) &&
                    (!GotSpeedBuff()) || (GetJayceEHamDmg(target) > target.Health))
                {
                    E2.Cast(target);
                }

                if (W2.IsReady() && GotManaFor(false, true) && TargetInRange(target, W2.Range))
                {
                    W2.Cast();
                }
            }
        }

        public static void DoJayceInj(Obj_AI_Hero target)
        {
            if (LockedTarg != null)
            {
                target = LockedTarg;
            }
            else
            {
                LockedTarg = target;
            }


            if (IsHammer)
            {
                CastIgnite(target);

                if ( /* inMyTowerRange(posAfterHammer(target)) && */ E2.IsReady())
                    E2.Cast(target);

                // If not in flash range  Q to get in it
                if (Player.Distance(target) > 400 && TargetInRange(target, 600f))
                {
                    Q2.Cast(target);
                }

                if (!E2.IsReady() && !Q2.IsReady())
                {
                    R2.Cast();
                }

                Obj_AI_Base tower =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(tur => tur.IsAlly && tur.Health > 0)
                        .OrderBy(tur => Player.Distance(tur))
                        .First();
                if (Player.Distance(GetBestPosToHammer(target.ServerPosition)) < 400 && tower.Distance(target) < 1500)
                {
                    Player.Spellbook.CastSpell(Player.GetSpellSlot("SummonerFlash"),
                        GetBestPosToHammer(target.ServerPosition));
                }
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            else
            {
                if (E1.IsReady() && Q1.IsReady() && GotManaFor(true, false, true))
                {
                    var po = QEmp1.GetPrediction(target);
                    if (po.Hitchance >= HitChance.Low &&
                        Player.Distance(po.UnitPosition) < (QEmp1.Range + target.BoundingRadius))
                    {
                        CastQon = po.CastPosition;
                    }

                    // QEmp1.CastIfHitchanceEquals(target, Prediction.HitChance.HighHitchance);
                }
                else if (Q1.IsReady() && GotManaFor(true))
                {
                    Q1.Cast(target.Position);
                }
                else if (W1.IsReady() && GotManaFor(false, true) && TargetInRange(GetClosestEnem(), 1000f))
                {
                    W1.Cast();
                }
            }
        }

        /*  public static Vector3 posAfterInj(Obj_AI_Base target)
        {
            Vector3 ve = getBestPosToHammer(target.ServerPosition);
            return posAfterHammer()
        }*/


        public static void DoKillSteal()
        {
            try
            {
                if (RangQcdRem == 0 && RangEcdRem == 0 && GotManaFor(true, false, true))
                {
                    var deadEnes =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                ene =>
                                    GetJayceEqDmg(ene) > ene.Health && ene.IsEnemy && ene.IsValid &&
                                    ene.Distance(Player.ServerPosition) < 1800)
                            .ToList();
                    foreach (var enem in deadEnes.Where(enem => !(Player.Distance(enem) < 300)))
                    {
                        if (IsHammer && R2.IsReady())
                        {
                            R2.Cast();
                        }

                        CastQePred(enem);
                    }
                }
                else if (RangQcdRem == 0 && GotManaFor(true))
                {
                    var deadEnes =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                ene =>
                                    GetJayceQDmg(ene) > ene.Health && ene.IsEnemy && ene.IsValid &&
                                    ene.Distance(Player.ServerPosition) < 1200)
                            .ToList();
                    foreach (var enem in deadEnes)
                    {
                        if (IsHammer && R2.IsReady())
                        {
                            R2.Cast();
                        }

                        CastQPred(enem);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void CastQePred(Obj_AI_Hero target)
        {
            if (IsHammer)
            {
                return;
            }

            var po = QEmp1.GetPrediction(target);
            if (po.Hitchance >= HitChance.Low &&
                Player.Distance(po.UnitPosition) < (QEmp1.Range + target.BoundingRadius))
            {
                CastQon = po.CastPosition;
            }
            else if (po.Hitchance == HitChance.Collision && JayceSharp.Config.Item("useMunions").GetValue<bool>())
            {
                var fistCol = po.CollisionObjects.OrderBy(unit => unit.Distance(Player.ServerPosition)).First();
                if (fistCol.Distance(po.UnitPosition) < (180 - fistCol.BoundingRadius/2) &&
                    fistCol.Distance(target.ServerPosition) < (180 - fistCol.BoundingRadius/2))
                {
                    CastQon = po.CastPosition;
                }
            }
        }

        public static void CastQPred(Obj_AI_Hero target)
        {
            if (IsHammer)
            {
                return;
            }

            var po = Q1.GetPrediction(target);
            if (po.Hitchance >= HitChance.High && Player.Distance(po.UnitPosition) < (Q1.Range + target.BoundingRadius))
            {
                Q1.Cast(po.CastPosition);
            }
            else if (po.Hitchance == HitChance.Collision && JayceSharp.Config.Item("useMunions").GetValue<bool>())
            {
                var fistCol = po.CollisionObjects.OrderBy(unit => unit.Distance(Player.ServerPosition)).First();
                if (fistCol.Distance(po.UnitPosition) < (180 - fistCol.BoundingRadius/2) &&
                    fistCol.Distance(target.ServerPosition) < (100 - fistCol.BoundingRadius/2))
                {
                    Q1.Cast(po.CastPosition);
                }
            }
        }

        public static Vector3 GetBestPosToHammer(Vector3 target)
        {
            Obj_AI_Base tower =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsAlly && tur.Health > 0)
                    .OrderBy(tur => Player.Distance(tur))
                    .First();
            return target + Vector3.Normalize(tower.ServerPosition - target)*(-120);
        }

        public static Vector3 PosAfterHammer(Obj_AI_Base target)
        {
            return GetBestPosToHammer(target.ServerPosition) +
                   Vector3.Normalize(GetBestPosToHammer(target.ServerPosition) - Player.ServerPosition)*600;
        }

        public static Obj_AI_Hero GetClosestEnem()
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ene => ene.IsEnemy && ene.IsValidTarget())
                    .OrderBy(ene => Player.Distance(ene))
                    .First();
        }

        public static float GetBestRange()
        {
            float range;
            if (!IsHammer)
            {
                if (Q1.IsReady() && E1.IsReady() && GotManaFor(true, false, true))
                {
                    range = 1750;
                }
                else if (Q1.IsReady() && GotManaFor(true))
                {
                    range = 1150;
                }
                else
                {
                    range = 500;
                }
            }
            else
            {
                if (Q1.IsReady() && GotManaFor(true))
                {
                    range = 600;
                }
                else
                {
                    range = 300;
                }
            }
            return range + 50;
        }

        public static bool ShootQe(Vector3 pos)
        {
            try
            {
                if (IsHammer && R2.IsReady())
                {
                    R2.Cast();
                }

                if (!E1.IsReady() || !Q1.IsReady() || IsHammer)
                {
                    return false;
                }

                if (JayceSharp.Config.Item("packets").GetValue<bool>())
                {
                    PacketCastQ(pos.To2D());
                    PacketCastE(GetParalelVec(pos));
                }
                else
                {
                    var bPos = Player.ServerPosition - Vector3.Normalize(pos - Player.ServerPosition)*50;

                    Player.IssueOrder(GameObjectOrder.MoveTo, bPos);
                    Q1.Cast(pos);

                    E1.Cast(GetParalelVec(pos));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return true;
        }

        public static bool ShouldIKnockDatMadaFaka(Obj_AI_Hero target)
        {
            //if (useSmartKnock(target) && R2.IsReady() && target.CombatType == GameObjectCombatType.Melee)
            // {
            //  return true;
            // }
            var damageOn = GetJayceEHamDmg(target);
            if (damageOn > target.Health*0.9f)
            {
                return true;
            }

            if (((Player.Health/Player.MaxHealth) < 0.15f) /*&& target.CombatType == GameObjectCombatType.Melee*/)
            {
                return true;
            }

            var posAfter = target.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition)*450;
            return InMyTowerRange(posAfter);
        }

        public static bool UseSmartKnock(Obj_AI_Hero target)
        {
            var trueAaRange = Player.BoundingRadius + target.AttackRange;
            var trueERange = target.BoundingRadius + E2.Range;
            var dist = Player.Distance(target);
            var movePos = new Vector2();
            if (target.IsMoving)
            {
                var tpos = target.Position.To2D();
                var path = target.Path[0].To2D() - tpos;
                path.Normalize();
                movePos = tpos + (path*100);
            }
            var targMs = (target.IsMoving && Player.Distance(movePos) < dist) ? target.MoveSpeed : 0;
            var msDif = (Player.MoveSpeed*0.7f - targMs) == 0 ? 0.0001f : (targMs - Player.MoveSpeed*0.7f);
            var timeToReach = (dist - trueAaRange)/msDif;
            if (!(dist > trueAaRange) || !(dist < trueERange) || !target.IsMoving)
            {
                return false;
            }

            return timeToReach > 1.7f || timeToReach < 0.0f;
        }

        public static bool InMyTowerRange(Vector3 pos)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsAlly && tur.Health > 0)
                    .Any(tur => pos.Distance(tur.Position) < (850 + Player.BoundingRadius));
        }

        public static void CastEonSpell(Obj_AI_Hero mis)
        {
            if (IsHammer || !E1.IsReady())
            {
                return;
            }

            if (Player.Distance(MyCastedQ.Position) < 250)
            {
                E1.Cast(GetParalelVec(mis.Position));
            }
        }

        public static bool TargetInRange(Obj_AI_Base target, float range)
        {
            var dist2 = Vector2.DistanceSquared(target.ServerPosition.To2D(), Player.ServerPosition.To2D());
            var range2 = range*range + target.BoundingRadius*target.BoundingRadius;
            return dist2 < range2;
        }

        public static void CheckForm()
        {
            IsHammer = !Qdata.SData.Name.Contains("jayceshockblast");
        }

        public static bool GotSpeedBuff() //jaycehypercharge
        {
            return Player.Buffs.Any(bi => bi.Name.Contains("jaycehypercharge"));
        }

        public static Vector2 GetParalelVec(Vector3 pos)
        {
            if (JayceSharp.Config.Item("parlelE").GetValue<bool>())
            {
                var rnd = new Random();
                var neg = rnd.Next(0, 1);
                var away = JayceSharp.Config.Item("eAway").GetValue<Slider>().Value;
                away = (neg == 1) ? away : -away;
                var v2 = Vector3.Normalize(pos - Player.ServerPosition)*away;
                var bom = new Vector2(v2.Y, -v2.X);
                return Player.ServerPosition.To2D() + bom;
            }
            else
            {
                var v2 = Vector3.Normalize(pos - Player.ServerPosition)*300;
                var bom = new Vector2(v2.X, v2.Y);
                return Player.ServerPosition.To2D() + bom;
            }
        }

        //Need to fix!!
        public static bool GotManaFor(bool q = false, bool w = false, bool e = false)
        {
            float manaNeeded = 0;
            if (q)
            {
                manaNeeded += Qdata.ManaCost;
            }

            if (w)
            {
                manaNeeded += Wdata.ManaCost;
            }

            if (e)
            {
                manaNeeded += Edata.ManaCost;
            }

            // Console.WriteLine("Mana: " + manaNeeded);
            return manaNeeded <= Player.Mana;
        }

        public static float CalcRealCd(float time)
        {
            return time + (time*Player.PercentCooldownMod);
        }

        public static void ProcessCDs()
        {
            HamQcdRem = ((HamQcd - Game.Time) > 0) ? (HamQcd - Game.Time) : 0;
            HamWcdRem = ((HamWcd - Game.Time) > 0) ? (HamWcd - Game.Time) : 0;
            HamEcdRem = ((HamEcd - Game.Time) > 0) ? (HamEcd - Game.Time) : 0;

            RangQcdRem = ((RangQcd - Game.Time) > 0) ? (RangQcd - Game.Time) : 0;
            RangWcdRem = ((RangWcd - Game.Time) > 0) ? (RangWcd - Game.Time) : 0;
            RangEcdRem = ((RangEcd - Game.Time) > 0) ? (RangEcd - Game.Time) : 0;
        }

        public static void GetCDs(GameObjectProcessSpellCastEventArgs spell)
        {
            try
            {
                // Console.WriteLine(spell.SData.Name + ": " + Q2.Level);
                if (spell.SData.Name == "JayceToTheSkies")
                {
                    HamQcd = Game.Time + CalcRealCd(HamTrueQcd[Q2.Level - 1]);
                }

                if (spell.SData.Name == "JayceStaticField")
                {
                    HamWcd = Game.Time + CalcRealCd(HamTrueWcd[W2.Level - 1]);
                }

                if (spell.SData.Name == "JayceThunderingBlow")
                {
                    HamEcd = Game.Time + CalcRealCd(HamTrueEcd[E2.Level - 1]);
                }

                if (spell.SData.Name == "jayceshockblast")
                {
                    RangQcd = Game.Time + CalcRealCd(RangTrueQcd[Q1.Level - 1]);
                }

                if (spell.SData.Name == "jaycehypercharge")
                {
                    RangWcd = Game.Time + CalcRealCd(RangTrueWcd[W1.Level - 1]);
                }

                if (spell.SData.Name == "jayceaccelerationgate")
                {
                    RangEcd = Game.Time + CalcRealCd(RangTrueEcd[E1.Level - 1]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void DrawCd()
        {
            var pScreen = Drawing.WorldToScreen(Player.Position);

            // Drawing.DrawText(Drawing.WorldToScreen(Player.Position)[0], Drawing.WorldToScreen(Player.Position)[1], System.Drawing.Color.Green, "Q: wdeawd ");
            pScreen[0] -= 20;

            if (IsHammer)
            {
                if (RangQcdRem == 0)
                {
                    Drawing.DrawText(pScreen.X - 60, pScreen.Y, Color.Green, "Q: Rdy");
                }
                else
                {
                    Drawing.DrawText(pScreen.X - 60, pScreen.Y, Color.Red, "Q: " + RangQcdRem.ToString("0.0"));
                }

                if (RangWcdRem == 0)
                {
                    Drawing.DrawText(pScreen.X, pScreen.Y, Color.Green, "W: Rdy");
                }
                else
                {
                    Drawing.DrawText(pScreen.X, pScreen.Y, Color.Red, "W: " + RangWcdRem.ToString("0.0"));
                }

                if (RangEcdRem == 0)
                {
                    Drawing.DrawText(pScreen.X + 60, pScreen.Y, Color.Green, "E: Rdy");
                }
                else
                {
                    Drawing.DrawText(pScreen.X + 60, pScreen.Y, Color.Red, "E: " + RangEcdRem.ToString("0.0"));
                }
            }
            else
            {
                // pScreen.Y += 30;
                if (HamQcdRem == 0)
                {
                    Drawing.DrawText(pScreen.X - 60, pScreen.Y, Color.Green, "Q: Rdy");
                }
                else
                {
                    Drawing.DrawText(pScreen.X - 60, pScreen.Y, Color.Red, "Q: " + HamQcdRem.ToString("0.0"));
                }

                if (HamWcdRem == 0)
                {
                    Drawing.DrawText(pScreen.X, pScreen.Y, Color.Green, "W: Rdy");
                }
                else
                {
                    Drawing.DrawText(pScreen.X, pScreen.Y, Color.Red, "W: " + HamWcdRem.ToString("0.0"));
                }

                if (HamEcdRem == 0)
                {
                    Drawing.DrawText(pScreen.X + 60, pScreen.Y, Color.Green, "E: Rdy");
                }
                else
                {
                    Drawing.DrawText(pScreen.X + 60, pScreen.Y, Color.Red, "E: " + HamEcdRem.ToString("0.0"));
                }
            }
        }

        public static void PacketCastQ(Vector2 pos)
        {
            Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, SpellSlot.Q, Player.NetworkId, pos.X, pos.Y,
                Player.ServerPosition.X, Player.ServerPosition.Y)).Send();
        }

        public static void PacketCastE(Vector2 pos)
        {
            Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, SpellSlot.E, Player.NetworkId, pos.X, pos.Y,
                Player.Position.X, Player.Position.Y)).Send();
        }

        public static void KnockAway(Obj_AI_Base target)
        {
            if (!TargetInRange(target, 270) || HamEcdRem != 0 || E1.Level == 0)
            {
                return;
            }

            if (!IsHammer && R2.IsReady())
            {
                R1.Cast();
            }

            if (IsHammer && E2.IsReady() && TargetInRange(target, 260))
            {
                E2.Cast(target);
            }
        }

        public static bool HammerWillKill(Obj_AI_Base target)
        {
            if (!JayceSharp.Config.Item("hammerKill").GetValue<bool>())
            {
                return false;
            }

            var damage = (float) Player.GetAutoAttackDamage(target) + 50;
            damage += GetJayceEHamDmg(target);
            damage += GetJayceQHamDmg(target);

            return (target.Health < damage);
        }

        public static float GetJayceFullComoDmg(Obj_AI_Base target)
        {
            float dmg = 0;
            // Ranged
            if (!IsHammer || R1.IsReady())
            {
                if (RangEcdRem == 0 && RangQcdRem == 0 && Q1.Level != 0 && E1.Level != 0)
                {
                    dmg += GetJayceEqDmg(target);
                }
                else if (RangQcdRem == 0 && Q1.Level != 0)
                {
                    dmg += GetJayceQDmg(target);
                }

                var hyperMulti = W1.Level*0.15f + 0.7f;
                if (RangWcdRem == 0 && W1.Level != 0)
                {
                    dmg += GetJayceAaDmg(target)*3*hyperMulti;
                }
            }

            // Hamer
            if (!IsHammer && !R1.IsReady())
            {
                return dmg;
            }

            if (HamEcdRem == 0 && E2.Level != 0)
            {
                dmg += GetJayceEHamDmg(target);
            }

            if (HamQcdRem == 0 && Q2.Level != 0)
            {
                dmg += GetJayceQHamDmg(target);
            }

            return dmg;
        }

        public static float GetJayceAaDmg(Obj_AI_Base target)
        {
            return (float) Player.GetAutoAttackDamage(target);
        }

        public static float GetJayceEqDmg(Obj_AI_Base target)
        {
            return
                (float)
                    Player.CalcDamage(target, Damage.DamageType.Physical,
                        (7 + (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level*77)) +
                        (1.68*ObjectManager.Player.FlatPhysicalDamageMod));
        }

        public static float GetJayceQDmg(Obj_AI_Base target)
        {
            return (float) Player.CalcDamage(target, Damage.DamageType.Physical,
                (5 + (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level*55)) +
                (1.2*ObjectManager.Player.FlatPhysicalDamageMod));
        }

        public static float GetJayceEHamDmg(Obj_AI_Base target)
        {
            double percentage = 5 + (3*Player.Spellbook.GetSpell(SpellSlot.E).Level);
            return (float) Player.CalcDamage(target, Damage.DamageType.Magical,
                ((target.MaxHealth/100)*percentage) + (ObjectManager.Player.FlatPhysicalDamageMod));
        }

        public static float GetJayceQHamDmg(Obj_AI_Base target)
        {
            return (float) Player.CalcDamage(target, Damage.DamageType.Physical,
                (-25 + (Player.Spellbook.GetSpell(SpellSlot.Q).Level*45)) +
                (1.0*Player.FlatPhysicalDamageMod));
        }

        public static void CastIgnite(Obj_AI_Hero target)
        {
            if (TargetInRange(target, 600) && (target.Health/target.MaxHealth)*100 < 25)
            {
                SumItems.CastIgnite(target);
            }
        }

        public static void CastOmen(Obj_AI_Hero target)
        {
            if (Player.Distance(target) < 430)
            {
                SumItems.cast(SummonerItems.ItemIds.Omen);
            }
        }

        public static void ActivateMura()
        {
            if (Player.Buffs.Count(buf => buf.Name == "Muramana") == 0)
            {
                SumItems.cast(SummonerItems.ItemIds.Muramana);
            }
        }

        public static void DeActivateMura()
        {
            if (Player.Buffs.Count(buf => buf.Name == "Muramana") != 0)
            {
                SumItems.cast(SummonerItems.ItemIds.Muramana);
            }
        }

        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static SummonerItems SumItems = new SummonerItems(Player);
        public static Spellbook SBook = Player.Spellbook;
        public static SpellDataInst Qdata = SBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = SBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = SBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = SBook.GetSpell(SpellSlot.R);
    }
}