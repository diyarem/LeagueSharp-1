﻿using System;
using LeagueSharp;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace RivenSharpV2
{
    internal class HpBarIndicator
    {
        public static Device DxDevice = Drawing.Direct3DDevice;
        public static Line DxLine;
        public float Height = 9;
        public float Width = 104;

        public HpBarIndicator()
        {
            DxLine = new Line(DxDevice) {Width = 9};

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_OnDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_OnDomainUnload;
        }

        public Obj_AI_Hero Unit { get; set; }

        private Vector2 Offset
        {
            get
            {
                if (Unit != null)
                {
                    return Unit.IsAlly ? new Vector2(34, 9) : new Vector2(10, 20);
                }

                return new Vector2();
            }
        }

        public Vector2 StartPosition
        {
            get { return new Vector2(Unit.HPBarPosition.X + Offset.X, Unit.HPBarPosition.Y + Offset.Y); }
        }

        private static void CurrentDomain_OnDomainUnload(object sender, EventArgs eventArgs)
        {
            DxLine.Dispose();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            DxLine.OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            DxLine.OnLostDevice();
        }

        private float GetHpProc(float dmg = 0)
        {
            var health = ((Unit.Health - dmg) > 0) ? (Unit.Health - dmg) : 0;
            return (health/Unit.MaxHealth);
        }

        private Vector2 GetHpPosAfterDmg(float dmg)
        {
            var w = GetHpProc(dmg)*Width;
            return new Vector2(StartPosition.X + w, StartPosition.Y);
        }

        public void DrawDmg(float dmg, Color color)
        {
            var hpPosNow = GetHpPosAfterDmg(0);
            var hpPosAfter = GetHpPosAfterDmg(dmg);

            FillHpBar(hpPosNow, hpPosAfter);
            // FillHpBar((int)(hpPosNow.X - startPosition.X), (int)(hpPosAfter.X- startPosition.X), color);
        }

        private void FillHpBar(int to, int from, Color color)
        {
            var sPos = StartPosition;

            for (var i = from; i < to; i++)
            {
                Drawing.DrawLine(sPos.X + i, sPos.Y, sPos.X + i, sPos.Y + 9, 1, color);
            }
        }

        private static void FillHpBar(Vector2 from, Vector2 to)
        {
            DxLine.Begin();

            DxLine.Draw(new[]
            {
                new Vector2((int) from.X, (int) from.Y + 4f),
                new Vector2((int) to.X, (int) to.Y + 4f)
            }, new ColorBGRA(255, 255, 00, 90));
            // Vector2 sPos = startPosition;
            //Drawing.DrawLine((int)from.X, (int)from.Y + 9f, (int)to.X, (int)to.Y + 9f, 9f, color);

            DxLine.End();
        }
    }
}