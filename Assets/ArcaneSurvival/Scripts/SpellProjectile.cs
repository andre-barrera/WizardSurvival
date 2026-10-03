using System.Collections.Generic;
using UnityEngine;

namespace ArcaneSurvival
{
    public sealed class SpellProjectile
    {
        public Vector2 Position;
        public readonly Vector2 Direction;
        public readonly SpriteRenderer View;
        public float Lifetime = 1.6f;
        public const float Speed = 13;

        public SpellProjectile(PixelArt art, Transform parent, Vector2 position, Vector2 direction)
        {
            Position = position;
            Direction = direction.normalized;
            View = art.Draw("Arcane bolt", art.Circle, parent, position,
                new Vector2(0.38f, 0.18f), new Color32(174, 255, 242, 255), 50);
            View.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg);
        }

        public EnemyController Tick(float dt, List<EnemyController> enemies)
        {
            Vector2 previous = Position;
            Position += Direction * (Speed * dt);
            Lifetime -= dt;
            EnemyController nearest = null;
            float earliest = float.MaxValue;
            foreach (EnemyController enemy in enemies)
            {
                if (!enemy.Alive || enemy.SpawnDelay > 0) continue;
                if (GameRules.SegmentHitsCircle(previous, Position, enemy.Position,
                    enemy.Radius + 0.13f, out float along) && along < earliest)
                {
                    earliest = along;
                    nearest = enemy;
                }
            }
            View.transform.position = Position;
            if (nearest != null) Lifetime = 0;
            return nearest;
        }
    }
}
