using UnityEngine;

namespace ArcaneSurvival
{
    public static class GameRules
    {
        public const float ArenaHalfWidth = 9.7f;
        public const float ArenaHalfHeight = 4.8f;
        public const int MaxHealth = 5;

        // Calculates the wave number as it changes every 20 seconds
        public static int WaveAt(float seconds) => 1 + Mathf.FloorToInt(Mathf.Max(0, seconds) / 20f);
        // Calculates the delay of spawning of enemies
        public static float SpawnInterval(int wave) => Mathf.Max(0.32f, 1.3f - (wave - 1) * 0.11f);
        // Calculates enemy speed depending of the actual wave
        public static float EnemySpeed(int wave, bool fast) =>
            Mathf.Min(3.6f, 1.25f + (wave - 1) * 0.12f + (fast ? 0.65f : 0));

        // Prevents player to move out of the arena
        public static Vector2 ClampToArena(Vector2 position)
        {
            return new Vector2(Mathf.Clamp(position.x, -ArenaHalfWidth, ArenaHalfWidth),
                Mathf.Clamp(position.y, -ArenaHalfHeight, ArenaHalfHeight));
        }

        // Checks that the spell hits the enemy along its path
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
