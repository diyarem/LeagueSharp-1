using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

namespace HiddenObj
{
    internal class HiddenObj
    {
        public static List<ListedHO> AllObjects = new List<ListedHO>();

        public HiddenObj()
        {
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoad = new Thread(Game_OnGameLoad);
                onGameLoad.Start();
            };

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("missile") || sender.Name.Contains("Minion"))
            {
                return;
            }

            var objis = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(sender.NetworkId);
            //Console.WriteLine(sender.Name+" - "+objis.SkinName);
            //Console.WriteLine(sender.Name + " - " + sender.Type + " - " + sender.Flags);
            var ho = HidObjects.IsHidObj(objis.SkinName);
            if (ho != null)
            {
                AllObjects.Add(new ListedHO(sender.NetworkId, sender.Name, ho.Duration, ho.ObjColor, ho.Range,
                    sender.Position, Game.Time));
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var i = 0;
            foreach (var lho in AllObjects)
            {
                if (lho.NetworkId == sender.NetworkId)
                {
                    AllObjects.RemoveAt(i);
                    return;
                }
                i++;
            }
        }

        private static void Game_OnGameLoad()
        {
            Game.PrintChat("Hidden Objects 0.1 by DeTuKs");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Utility.DrawCircle(ObjectManager.Player.Position, 500, System.Drawing.Color.FromArgb(255, 186, 201, 46));
            //Drawing.DrawText(ObjectManager.Player.Position.X, ObjectManager.Player.Position.Z, Color.FromArgb(255, 0, 0, 0), "awdawawd");
            foreach (
                var lho in
                    AllObjects.Where(
                        lho => lho.Duration == -1 || (int) ((lho.CreatedAt + lho.Duration + 1) - Game.Time) > 0))
            {
                Utility.DrawCircle(lho.Position, 50, lho.ObjColor);
                if (lho.Duration <= 0)
                {
                    continue;
                }

                var locOnScreen = Drawing.WorldToScreen(lho.Position);
                Drawing.DrawText(locOnScreen.X - 10, locOnScreen.Y - 10, lho.ObjColor,
                    string.Empty + (int) ((lho.CreatedAt + lho.Duration + 1) - Game.Time));
            }
        }
    }
}