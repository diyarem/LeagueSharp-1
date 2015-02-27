using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace KhazixSharp
{
    internal class HpBarIndicator
    {
        public float Hight = 9;
        public float Width = 104;
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

            FillHpBar(hpPosNow, hpPosAfter, color);
            // FillHpBar((int)(hpPosNow.X - startPosition.X), (int)(hpPosAfter.X- startPosition.X), color);
        }

        private void FillpPBar(int to, int from, Color color)
        {
            var sPos = StartPosition;

            for (var i = from; i < to; i++)
            {
                Drawing.DrawLine(sPos.X + i, sPos.Y, sPos.X + i, sPos.Y + 9, 1, color);
            }
        }

        private static void FillHpBar(Vector2 from, Vector2 to, Color color)
        {
            Drawing.DrawLine((int) from.X, (int) from.Y + 9f, (int) to.X, (int) to.Y + 9f, 9f, color);
        }
    }
}