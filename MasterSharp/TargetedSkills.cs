using System.Collections.Generic;
using LeagueSharp;

namespace MasterSharp
{
    internal class TargetedSkills
    {
        public static List<TargSkill> TargetedSkillsAll = new List<TargSkill>();
        public static List<TargSkill> DagerousBuffs = new List<TargSkill>();
        /*{
            "timebombenemybuff",
            "",
            "NocturneUnspeakableHorror"
        };*/


        public static void SetUpSkills()
        {
            //Bufs
            DagerousBuffs.Add(new TargSkill("timebombenemybuff", 1, 1, 1, 300));
            DagerousBuffs.Add(new TargSkill("karthusfallenonetarget", 1, 1, 1, 300));
            DagerousBuffs.Add(new TargSkill("NocturneUnspeakableHorror", 1, 0, 1, 500));

            // Name of spellName, Q use, W use --- 2-prioritize more , 1- prioritize less 0 dont use
            TargetedSkillsAll.Add(new TargSkill("SyndraR", 0, 1, 1));
            TargetedSkillsAll.Add(new TargSkill("VayneCondemn", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Dazzle", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Overload", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("IceBlast", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("LeblancChaosOrb", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("JudicatorReckoning", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("KatarinaQ", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("NullLance", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("FiddlesticksDarkWind", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("CaitlynHeadshotMissile", 2, 1, 1));
            TargetedSkillsAll.Add(new TargSkill("BrandWildfire", 2, 1, 1, 150));
            TargetedSkillsAll.Add(new TargSkill("Disintegrate", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("Frostbite", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("AkaliMota", 2, 1, 0));

            //infiniteduresschannel  InfiniteDuress
            TargetedSkillsAll.Add(new TargSkill("InfiniteDuress", 2, 0, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("PantheonW", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("blindingdart", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("JayceToTheSkies", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("dariusexecute", 2, 1, 1));
            TargetedSkillsAll.Add(new TargSkill("ireliaequilibriumstrike", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("maokaiunstablegrowth", 2, 1, 1));
            TargetedSkillsAll.Add(new TargSkill("missfortunericochetshot", 2, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("nautilusgandline", 2, 1, 1));
            TargetedSkillsAll.Add(new TargSkill("runeprison", 2, 1, 1));
            TargetedSkillsAll.Add(new TargSkill("goldcardpreattack", 2, 0, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("vir", 2, 1, 1));
            TargetedSkillsAll.Add(new TargSkill("zedult", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("AkaliMota", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("AkaliShadowDance", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Headbutt", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Frostbite", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Disintegrate", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("PowerFist", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("BrandConflagration", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("BrandWildfire", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("CaitlynAceintheHole", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("CassiopeiaTwinFang", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Feast", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("DariusNoxianTacticsONH", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("DariusExecute", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("DianaTeleport", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("dravenspinning", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("EliseHumanQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("EvelynnE", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("EzrealArcaneShift", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Terrify", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("FiddlesticksDarkWind", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("FioraQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("FioraDance", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("FizzPiercingStrike", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Parley", 2, 0, 1, 0));
            TargetedSkillsAll.Add(new TargSkill("GarenQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("GarenR", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("IreliaGatotsu", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("IreliaEquilibriumStrike", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SowTheWind", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("JarvanIVCataclysm", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("JaxLeapStrike", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("JaxEmpowerTwo", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("JayceToTheSkies", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("JayceThunderingBlow", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("KarmaSpiritBind", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("NullLance", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("NetherBlade", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("KatarinaQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("KatarinaE", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("JudicatorReckoning", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("JudicatorRighteousFury", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("KennenBringTheLight", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("khazixqlong", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("KhazixQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("LeblancChaosOrb", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("LeblancChaosOrbM", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("BlindMonkRKick", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("LeonaShieldOfDaybreak", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("LissandraR", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("LucianQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("LuluW", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("LuluE", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SeismicShard", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("AlZaharMaleficVisions", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("AlZaharNetherGrasp", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("MaokaiUnstableGrowth", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("AlphaStrike", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("MissFortuneRicochetShot", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("MordekaiserMaceOfSpades", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("MordekaiserChildrenOfTheGrave", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SoulShackles", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("NamiW", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("NasusQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("NasusW", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("NautilusGandLine", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Takedown", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("NocturneUnspeakableHorror", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("NocturneParanoia", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("IceBlast", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("OlafRecklessStrike", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("PantheonQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("PantheonW", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("PoppyDevastatingBlow", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("PoppyHeroicCharge", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("QuinnE", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("RengarQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("PuncturingTaunt", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("RenektonPreExecute", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Overload", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SpellFlux", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SejuaniWintersClaw", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("TwoShivPoisen", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("ShenVorpalStar", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("ShyvanaDoubleAttack", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("shyvanadoubleattackdragon", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Fling", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SkarnerImpale", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SonaHymnofValor", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SwainTorment", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SwainDecrepify", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("SyndraR", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("TalonNoxianDiplomacy", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("TalonCutthroat", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Dazzle", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("BlindingDart", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("DetonatingShot", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("BusterShot", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("TrundleTrollSmash", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("TrundlePain", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("MockingShout", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("goldcardpreattack", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("redcardpreattack", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("bluecardpreattack", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("Expunge", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("UdyrBearStance", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("UrgotHeatseekingLineMissile", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("UrgotHeatseekingLineqqMissile", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("UrgotSwap2", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("VayneCondemm", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("VeigarBalefulStrike", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("VeigarPrimordialBurst", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("ViR", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("ViktorPowerTransfer", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("VladimirTransfusion", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("VolibearQ", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("HungeringStrike", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("InfiniteDuress", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("XenZhaoComboTarget", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("XenZhaoSweep", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("YasuoDashWrapper", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("YasuoRKnockUpComboW", 2, 0, 1));
            TargetedSkillsAll.Add(new TargSkill("zedult", 2, 0, 1));
            // targetedSkillsAll.Add(new TargSkill("NocturneUnspeakableHorror", 2, 0, 1,0));
        }

        internal class TargSkill
        {
            public int Danger;
            public int Delay = 250;
            public string SName;
            public GameObjectProcessSpellCastEventArgs Spell;
            public int UseQ;
            public int UseW;

            public TargSkill(string name, int q, int w, int d)
            {
                SName = name;
                UseQ = q;
                UseW = w;
                Danger = d;
            }

            public TargSkill(string name, int q, int w, int d, int del)
            {
                SName = name;
                UseQ = q;
                UseW = w;
                Danger = d;
                Delay = del;
            }
        }
    }
}