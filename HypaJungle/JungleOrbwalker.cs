using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace HypaJungle
{
    internal class JungleOrbwalker
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static int _lastAaTick;
        private static Spell _movementPrediction;
        private static int _lastMovement;

        public static void AttackMinion(Obj_AI_Base target, Vector3 moveTo)
        {
            if (target != null && CanAttack())
            {
                if (Player.IssueOrder(GameObjectOrder.AttackUnit, target))
                {
                    _lastAaTick = Environment.TickCount + Game.Ping/2;
                }
            }
            MoveTo(moveTo);
        }

        public static bool CanAttack(int inMs = 0)
        {
            if (_lastAaTick <= Environment.TickCount)
            {
                return Environment.TickCount + Game.Ping/2 + 25 + inMs >= _lastAaTick + Player.AttackDelay*1000 + 130;
            }
            return false;
        }

        public static float GetAutoAttackRange(Obj_AI_Base source = null, Obj_AI_Base target = null)
        {
            if (source == null)
            {
                source = Player;
            }

            var ret = source.AttackRange + Player.BoundingRadius;
            if (target != null)
            {
                ret += target.BoundingRadius;
            }

            return ret;
        }

        private static void MoveTo(Vector3 position, float holdAreaRadius = -1)
        {
            const int delay = 100;
            if (Environment.TickCount - _lastMovement < delay)
            {
                return;
            }

            _lastMovement = Environment.TickCount;

            if (!CanMove())
            {
                return;
            }

            if (Player.Position.Distance(position) > 50)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, position);
            }

            return;
            if (holdAreaRadius < 0)
            {
                holdAreaRadius = 20;
            }

            if (Player.ServerPosition.Distance(position) < holdAreaRadius)
            {
                if (Player.Path.Count() > 1)
                {
                    Player.IssueOrder(GameObjectOrder.HoldPosition, Player.ServerPosition);
                }

                return;
            }
            if (position.Distance(Player.Position) < 200)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, position);
            }

            var point = Player.ServerPosition +
                        200*(position.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();
            Player.IssueOrder(GameObjectOrder.MoveTo, point);
        }

        public static bool CanMove()
        {
            const int extraWindup = 70;
            if (_lastAaTick <= Environment.TickCount && !Player.Spellbook.IsChanneling)
            {
                return Environment.TickCount + Game.Ping/2 >= _lastAaTick + Player.AttackCastDelay*1000 + extraWindup;
            }

            return false;
        }
    }
}