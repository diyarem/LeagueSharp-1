using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    internal enum JungleCampState
    {
        Unknown,
        Dead,
        Alive
    }

    internal class CallOnce
    {
        public Action A(Action action)
        {
            var context = new Context();
            Action ret = () =>
            {
                if (context.AlreadyCalled)
                {
                    return;
                }

                action();
                context.AlreadyCalled = true;
            };

            return ret;
        }

        private class Context
        {
            public bool AlreadyCalled;
        }
    }

    internal class JungleCamp
    {
        public int BonusPrio;
        public byte CampId;
        public int Dps;
        public int Health;
        public bool IsBuff;
        public bool IsDragBaron;
        public int Priority = 0;
        public int Team;
        public float TimeToCamp = 0;
        public bool WillKillMe = false;
        public TimeSpan SpawnTime { get; set; }
        public TimeSpan RespawnTimer { get; set; }
        public Vector3 Position { get; set; }
        public List<JungleMinion> Minions { get; set; }
        public JungleCampState State { get; set; }
        public float ClearTick { get; set; }
    }

    internal class JungleMinion
    {
        public JungleMinion(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public bool Dead { get; set; }
        public GameObject Unit { get; set; }
    }

    internal class JungleTimers
    {
        private readonly Action _onLoadAction;

        public readonly List<JungleCamp> JungleCamps = new List<JungleCamp>
        {
            new JungleCamp //Baron
            {
                SpawnTime = TimeSpan.FromSeconds(1200),
                RespawnTimer = TimeSpan.FromSeconds(420),
                Position = new Vector3(4549.126f, 10126.66f, -63.11666f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Worm12.1.1")
                },
                IsBuff = false,
                IsDragBaron = true,
                Team = 2,
                Dps = 99,
                Health = 1500,
                CampId = 12
            },
            new JungleCamp //Dragon
            {
                SpawnTime = TimeSpan.FromSeconds(150),
                RespawnTimer = TimeSpan.FromSeconds(360),
                Position = new Vector3(9606.835f, 4210.494f, -60.30991f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Dragon6.1.1")
                },
                IsBuff = false,
                IsDragBaron = true,
                Team = 2,
                CampId = 6
            },
            //Order
            new JungleCamp //Wight
            {
                SpawnTime = TimeSpan.FromSeconds(125),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(2072.131f, 8450.272f, 51.92376f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Gromp13.1.1")
                },
                IsBuff = false,
                IsDragBaron = false,
                Team = 0,
                Dps = (int) (90*0.64f),
                Health = 1600,
                CampId = 13,
                BonusPrio = 3
            },
            new JungleCamp //Blue
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(310),
                Position = new Vector3(3820.156f, 7920.175f, 52.21874f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Blue1.1.1"),
                    new JungleMinion("SRU_BlueMini1.1.2"),
                    new JungleMinion("SRU_BlueMini21.1.3")
                },
                IsBuff = true,
                IsDragBaron = false,
                Dps = (int) (80*0.64f),
                Health = 2000,
                Team = 0,
                CampId = 1
            },
            new JungleCamp //Wolfs
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(3842.77f, 6462.637f, 52.60973f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Murkwolf2.1.1"),
                    new JungleMinion("SRU_MurkwolfMini2.1.2"),
                    new JungleMinion("SRU_MurkwolfMini2.1.3")
                },
                IsBuff = false,
                IsDragBaron = false,
                Dps = (int) (73*0.59),
                Health = 1320,
                Team = 0,
                CampId = 2
            },
            new JungleCamp //Wraith
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(6926.0f, 5400.0f, 51.0f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Razorbeak3.1.1"),
                    new JungleMinion("SRU_RazorbeakMini3.1.2"),
                    new JungleMinion("SRU_RazorbeakMini3.1.3"),
                    new JungleMinion("SRU_RazorbeakMini3.1.4")
                },
                IsBuff = false,
                IsDragBaron = false,
                Dps = (int) (70*0.64f),
                Health = 1600,
                Team = 0,
                CampId = 3
            },
            new JungleCamp //Red
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(300),
                Position = new Vector3(7772.412f, 4108.053f, 53.867f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Red4.1.1"),
                    new JungleMinion("SRU_RedMini4.1.2"),
                    new JungleMinion("SRU_RedMini4.1.3")
                },
                IsBuff = true,
                IsDragBaron = false,
                Dps = (int) (104*0.60f),
                Health = 1800,
                Team = 0,
                CampId = 4
            },
            new JungleCamp //Golems
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(8404.148f, 2726.269f, 51.2764f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Krug5.1.2"),
                    new JungleMinion("SRU_KrugMini5.1.1")
                },
                IsBuff = false,
                IsDragBaron = false,
                Dps = (int) (100*0.60f),
                Health = 1440,
                Team = 0,
                CampId = 5,
                BonusPrio = 7
            },
            //Chaos
            new JungleCamp //Golems
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(6424.0f, 12156.0f, 56.62551f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Krug11.1.2"),
                    new JungleMinion("SRU_KrugMini11.1.1")
                },
                IsBuff = true,
                IsDragBaron = false,
                Dps = (int) (100*0.60f),
                Health = 1440,
                Team = 1,
                CampId = 11,
                BonusPrio = 3
            },
            new JungleCamp //Red
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(300),
                Position = new Vector3(7086.157f, 10866.92f, 56.63499f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Red10.1.1"),
                    new JungleMinion("SRU_RedMini10.1.2"),
                    new JungleMinion("SRU_RedMini10.1.3")
                },
                IsBuff = true,
                IsDragBaron = false,
                Dps = (int) (104*0.60f),
                Health = 1800,
                Team = 1,
                CampId = 10
            },
            new JungleCamp //Wraith
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(7970.319f, 9410.513f, 52.50048f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Razorbeak9.1.1"),
                    new JungleMinion("SRU_RazorbeakMini9.1.2"),
                    new JungleMinion("SRU_RazorbeakMini9.1.3"),
                    new JungleMinion("SRU_RazorbeakMini9.1.4")
                },
                IsBuff = false,
                IsDragBaron = false,
                Dps = (int) (70*0.64f),
                Health = 1600,
                Team = 1,
                CampId = 9
            },
            new JungleCamp //Wolfs
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(10972.0f, 8306.0f, 62.5235f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Murkwolf8.1.1"),
                    new JungleMinion("SRU_MurkwolfMini8.1.2"),
                    new JungleMinion("SRU_MurkwolfMini8.1.3")
                },
                IsBuff = false,
                IsDragBaron = false,
                Dps = (int) (73*0.59),
                Health = 1320,
                Team = 1,
                CampId = 8
            },
            new JungleCamp //Blue
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(310),
                Position = new Vector3(10938.95f, 7000.918f, 51.8691f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Blue7.1.1"),
                    new JungleMinion("SRU_BlueMini7.1.2"),
                    new JungleMinion("SRU_BlueMini27.1.3")
                },
                IsBuff = true,
                IsDragBaron = false,
                Dps = (int) (80*0.64f),
                Health = 2000,
                Team = 1,
                CampId = 7
            },
            new JungleCamp //Wight
            {
                SpawnTime = TimeSpan.FromSeconds(115),
                RespawnTimer = TimeSpan.FromSeconds(100),
                Position = new Vector3(12770.0f, 6468.0f, 51.84151f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("SRU_Gromp14.1.1")
                },
                IsBuff = false,
                IsDragBaron = false,
                Team = 1,
                Dps = (int) (90*0.64f),
                Health = 1600,
                CampId = 14,
                BonusPrio = 3
            },
            new JungleCamp //Crab
            {
                SpawnTime = TimeSpan.FromSeconds(150),
                RespawnTimer = TimeSpan.FromSeconds(180),
                Position = new Vector3(10218.0f, 5296.0f, -62.84151f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Sru_Crab15.1.1")
                },
                IsBuff = false,
                IsDragBaron = false,
                Team = 3,
                CampId = 15,
                BonusPrio = 3
            },
            new JungleCamp //Crab
            {
                SpawnTime = TimeSpan.FromSeconds(150),
                RespawnTimer = TimeSpan.FromSeconds(180),
                Position = new Vector3(5118.0f, 9200.0f, -71.84151f),
                Minions = new List<JungleMinion>
                {
                    new JungleMinion("Sru_Crab16.1.1")
                },
                IsBuff = false,
                IsDragBaron = false,
                Team = 3,
                CampId = 16,
                BonusPrio = 3
            }
        };

        public JungleTimers()
        {
            Console.WriteLine(@"Jungle timers onn");
            _onLoadAction = new CallOnce().A(OnLoad);
            Game.OnGameUpdate += OnGameUpdate;
        }

        private void OnLoad()
        {
            GameObject.OnCreate += ObjectOnCreate;
            GameObject.OnDelete += ObjectOnDelete;
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                _onLoadAction();
                UpdateCamps();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool ClosestJcUp(Vector3 pos)
        {
            var closest = JungleCamps.OrderBy(jc => Vector3.DistanceSquared(pos, jc.Position)).First();
            var delta = Game.Time - closest.ClearTick;
            return !(delta < closest.RespawnTimer.TotalSeconds);
        }

        public JungleCamp GetBestCampToGo()
        {
            var lessDist = float.MaxValue;
            JungleCamp bestCamp = null;
            foreach (var jungleCamp in JungleCamps)
            {
                var distTillCamp = GetPathLenght(HypaJungle.Player.GetPath(jungleCamp.Position));
                var timeToCamp = distTillCamp/HypaJungle.Player.MoveSpeed;
                var timeTillSpawn = Game.Time - jungleCamp.ClearTick;
                Console.WriteLine(jungleCamp.ClearTick + @" : " + Game.Time);
                if (!(timeTillSpawn + timeToCamp > jungleCamp.RespawnTimer.TotalSeconds) || !(lessDist > distTillCamp))
                {
                    continue;
                }

                lessDist = distTillCamp;
                bestCamp = jungleCamp;
            }
            return bestCamp;
        }

        public float GetPathLenght(Vector3[] vecs)
        {
            float dist = 0;
            var from = vecs[0];
            foreach (var vec in vecs)
            {
                dist += Vector3.Distance(from, vec);
                from = vec;
            }
            return dist;
        }

        private void ObjectOnDelete(GameObject sender, EventArgs args)
        {
            try
            {
                if (sender.Type != GameObjectType.obj_AI_Minion)
                {
                    return;
                }

                var neutral = (Obj_AI_Minion) sender;
                if (neutral.Name.Contains("Minion") || !neutral.IsValid)
                {
                    return;
                }

                foreach (
                    var minion in
                        from camp in JungleCamps
                        from minion in camp.Minions
                        where minion.Name == neutral.Name
                        select minion)
                {
                    minion.Dead = neutral.IsDead;
                    minion.Unit = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void EnableCamp(byte id)
        {
            foreach (var camp in JungleCamps.Where(camp => camp.CampId == id))
            {
                camp.ClearTick = 0;
                camp.State = JungleCampState.Alive;
            }
        }

        public void DisableCamp(byte id)
        {
            foreach (var camp in JungleCamps)
            {
                if (camp.CampId == id)
                {
                    camp.ClearTick = Game.Time;
                    camp.State = JungleCampState.Dead;
                }

                if (JungleClearer.FocusedCamp == null)
                {
                    continue;
                }

                if (camp.CampId == JungleClearer.FocusedCamp.CampId)
                {
                    JungleClearer.JcState = HypaJungle.Config.Item("autoBuy").GetValue<bool>()
                        ? JungleClearer.JungleCleanState.GoingToShop
                        : JungleClearer.JungleCleanState.SearchingBestCamp;
                }
            }
        }

        private void ObjectOnCreate(GameObject sender, EventArgs args)
        {
            try
            {
                if (sender.Type != GameObjectType.obj_AI_Minion)
                {
                    return;
                }

                var neutral = (Obj_AI_Minion) sender;
                if (neutral.Name.Contains("Minion") || !neutral.IsValid)
                {
                    return;
                }

                foreach (
                    var minion in
                        from camp in JungleCamps
                        from minion in camp.Minions
                        where minion.Name == neutral.Name
                        select minion)
                {
                    minion.Unit = neutral;
                    minion.Dead = neutral.IsDead;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void SetUpMinionsPlace(Obj_AI_Minion neutral)
        {
            foreach (
                var minion in
                    from camp in JungleCamps
                    from minion in camp.Minions
                    where minion.Name == neutral.Name
                    select minion)
            {
                minion.Unit = neutral;
                minion.Dead = neutral.IsDead;
            }
        }

        private void UpdateCamps()
        {
            foreach (var camp in JungleCamps)
            {
                var allDead = true;

                foreach (var minion in camp.Minions)
                {
                    if (minion.Unit != null)
                    {
                        minion.Dead = minion.Unit.IsDead;
                    }

                    if (NavMesh.LineOfSightTest(camp.Position, camp.Position) && minion.Unit == null)
                    {
                        // allAlive = false;
                    }


                    if (minion.Dead)
                        ;
                    else
                    {
                        allDead = false;
                    }
                }

                switch (camp.State)
                {
                    case JungleCampState.Alive:
                        if (allDead && camp.Position.Distance(HypaJungle.Player.Position) < 600)
                        {
                            camp.State = JungleCampState.Dead;
                            camp.ClearTick = Game.Time;
                            foreach (var min in camp.Minions)
                            {
                                min.Unit = null;
                                min.Dead = true;
                            }
                        }
                        break;
                }
            }
        }
    }
}