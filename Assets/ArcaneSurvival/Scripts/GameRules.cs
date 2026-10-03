using UnityEngine;

namespace ArcaneSurvival
{
    // Small, deterministic functions kept separate from rendering and input.
    public static class GameRules
    {
        public const float ArenaHalfWidth = 9.7f;
        public const float ArenaHalfHeight = 4.8f;
        public const int MaxHealth = 5;

        public static int WaveAt(float seconds) => 1 + Mathf.FloorToInt(Mathf.Max(0, seconds) / 20f);
        public static float SpawnInterval(int wave) => Mathf.Max(0.32f, 1.3f - (wave - 1) * 0.11f);
        public static float EnemySpeed(int wave, bool fast) =>
            Mathf.Min(3.6f, 1.25f + (wave - 1) * 0.12f + (fast ? 0.65f : 0));

        public static Vector2 ClampToArena(Vector2 position)
        {
            return new Vector2(Mathf.Clamp(position.x, -ArenaHalfWidth, ArenaHalfWidth),
                Mathf.Clamp(position.y, -ArenaHalfHeight, ArenaHalfHeight));
        }

        // Swept collision: a fast spell cannot skip an enemy between frames.
        public static bool SegmentHitsCircle(Vector2 start, Vector2 end, Vector2 center,
            float radius, out float along)
        {
            Vector2 movement = end - start;
            float lengthSquared = movement.sqrMagnitude;
            along = lengthSquared < 0.000001f ? 0 :
                Mathf.Clamp01(Vector2.Dot(center - start, movement) / lengthSquared);
            return (start + movement * along - center).sqrMagnitude <= radius * radius;
        }
    }
}
