using UnityEngine;

namespace ArcaneSurvival
{
    public sealed class EnemyController
    {
        public Vector2 Position;
        public readonly SpriteRenderer View;
        public readonly bool Fast;
        public readonly float Speed;
        public float SpawnDelay = 0.7f;
        public bool Alive = true;
        public float Radius => Fast ? 0.34f : 0.43f;
        public int Points => Fast ? 20 : 10;
        private readonly Color tint;

        public EnemyController(PixelArt art, Transform parent, Vector2 position, int wave, bool fast)
        {
            Position = position;
            Fast = fast;
            Speed = GameRules.EnemySpeed(wave, fast);
            tint = fast ? new Color(1, 0.65f, 0.8f) : Color.white;
            View = art.Draw(fast ? "Swift wisp" : "Forest wisp", art.Enemy, parent,
                position, Vector2.one * (fast ? 0.8f : 1), tint, 10);
        }

        public void Tick(float dt, Vector2 target, float elapsed)
        {
            SpawnDelay = Mathf.Max(0, SpawnDelay - dt);
            if (SpawnDelay <= 0)
                Position = Vector2.MoveTowards(Position, target, Speed * dt);
            View.transform.position = Position + Vector2.up * (Mathf.Sin(elapsed * 5 + Position.x) * 0.06f);
            View.color = SpawnDelay > 0 ? new Color(tint.r, tint.g, tint.b, 0.35f) : tint;
            View.sortingOrder = 20 - Mathf.RoundToInt(Position.y * 2);
        }
    }
}
