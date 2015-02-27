using System;
using System.Drawing;
using System.Net;
using LeagueSharp;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace JayceSharpV2
{
    internal class HpBarIndicator
    {
        public static Device DxDevice = Drawing.Direct3DDevice;
        public static Line DxLine;
        public static Sprite Sprite;
        public static Texture Suprise;
        public float Hight = 9;
        public float Width = 104;

        public HpBarIndicator()
        {
            DxLine = new Line(DxDevice) {Width = 9};
            Sprite = new Sprite(DxDevice);
            Suprise = Texture.FromMemory(
                Drawing.Direct3DDevice,
                (byte[])
                    new ImageConverter().ConvertTo(
                        LoadPicture("http://i.gyazo.com/b94246fcd45ead6c3e9a0f2a585bb655.png"), typeof (byte[])), 513,
                744, 0,
                Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
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

        private static Bitmap LoadPicture(string url)
        {
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
            {
                return null;
            }

            var bitmap2 = new Bitmap(responseStream);
            Console.WriteLine(bitmap2.Size);
            return (bitmap2);
        }

        private static void CurrentDomain_OnDomainUnload(object sender, EventArgs eventArgs)
        {
            Sprite.Dispose();
            DxLine.Dispose();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            DxLine.OnResetDevice();
            Sprite.OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            Sprite.OnLostDevice();
            DxLine.OnLostDevice();
        }

        public void DrawAwsomee()
        {
            Sprite.Begin();

            Sprite.Draw(Suprise, new ColorBGRA(255, 255, 255, 255), null, new Vector3(-200, -20, 0));

            Sprite.End();
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
            //fillHPBar((int)(hpPosNow.X - startPosition.X), (int)(hpPosAfter.X- startPosition.X), color);
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