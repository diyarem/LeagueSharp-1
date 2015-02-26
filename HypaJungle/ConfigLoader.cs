using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace HypaJungle
{
    internal class ConfigLoader
    {
        public static string Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                    "\\LeagueSharp\\HypaJungle\\";

        public static StringList GetChampionConfigs(string champName)
        {
            var files = Directory.GetFiles(Path + champName + "\\", "*.hypa", SearchOption.AllDirectories);

            var fileNames = new string[files.Count() + 1];
            fileNames[0] = "default";
            for (var i = 1; i < files.Count() + 1; i++)
            {
                fileNames[i] = System.IO.Path.GetFileName(files[i - 1]);
            }

            Console.WriteLine(files.Count());
            var sl = new StringList(fileNames);

            return sl;
        }

        public static void SetupFolders(List<string> names)
        {
            foreach (
                var name in
                    from name in names let exists = Directory.Exists(Path + name + "\\") where !exists select name)
            {
                Directory.CreateDirectory(Path + name + "\\");
            }
        }

        public static void LoadNewConfigHypa(string configName)
        {
            try
            {
                if (configName == "default")
                {
                    return;
                }

                var lvlSeq = new List<Spell>();
                var buyThings = new List<Jungler.ItemToShop>();
                var fPath = Path + HypaJungle.Player.ChampionName + "\\" + configName;
                Console.WriteLine(fPath);
                var lines = File.ReadLines(fPath);
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                    if (line.StartsWith("--"))
                    {
                        continue;
                    }

                    //load level seq
                    if (line.StartsWith("LVL"))
                    {
                        lvlSeq.Clear();
                        var spells = line.Split(' ');
                        string[] allowSpells = {"Q", "W", "E", "R"};
                        lvlSeq.AddRange(from spell in spells[1].Split(',')
                            where allowSpells.Contains(spell)
                            select (SpellSlot) Enum.Parse(typeof (SpellSlot), spell, false)
                            into ss
                            select new Spell(ss));
                        Console.WriteLine(@"Spells found: " + lvlSeq.Count);
                        JungleClearer.Jungler.LevelUpSeq = lvlSeq.ToArray();
                    }

                    if (!line.StartsWith("ITEM"))
                    {
                        continue;
                    }

                    var things = line.Split(' ');
                    if (things.Count() != 4)
                    {
                        continue;
                    }

                    var cost = int.Parse(things[1]);
                    var itemIds = new List<int>();
                    if (things[2] != "NONE")
                    {
                        itemIds.AddRange(things[2].Split(';').Select(int.Parse));
                    }

                    var itemsMustHave = new List<int>();
                    if (things[3] != "NONE")
                    {
                        itemsMustHave.AddRange(things[3].Split(';').Select(int.Parse));
                    }

                    var its = new Jungler.ItemToShop
                    {
                        GoldReach = cost,
                        ItemIds = itemIds,
                        ItemsMustHave = itemsMustHave
                    };

                    buyThings.Add(its);
                }

                JungleClearer.Jungler.BuyThings = buyThings;

                Console.WriteLine(@"Custom config (" + configName + @") loaded!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}