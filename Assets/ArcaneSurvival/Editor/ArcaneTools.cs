using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArcaneSurvival.Editor
{
    public static class ArcaneTools
    {
        [MenuItem("Tools/Arcane Survival/Open Game Scene")]
        public static void OpenScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/ArcaneSurvival/Scenes/ArcaneSurvival.unity");
        }

        [MenuItem("Tools/Arcane Survival/Run Logic Tests")]
        public static void RunLogicTests()
        {
            int passed = 0;
            Check(GameRules.WaveAt(0) == 1, "First wave", ref passed);
            Check(GameRules.WaveAt(19.99f) == 1, "Wave before boundary", ref passed);
            Check(GameRules.WaveAt(20) == 2, "Wave at boundary", ref passed);
            Check(GameRules.WaveAt(60) == 4, "Later wave", ref passed);
            Check(GameRules.WaveAt(-1) == 1, "Negative time clamped", ref passed);
            Check(Mathf.Approximately(GameRules.SpawnInterval(1), 1.3f), "Initial spawn interval", ref passed);
            Check(Mathf.Approximately(GameRules.SpawnInterval(100), 0.32f), "Spawn interval floor", ref passed);
            Check(GameRules.EnemySpeed(100, true) <= 3.6f, "Speed cap", ref passed);
            Check(GameRules.EnemySpeed(3, true) > GameRules.EnemySpeed(3, false), "Fast enemy", ref passed);
            Check(GameRules.ClampToArena(new Vector2(100, -100)) ==
                new Vector2(GameRules.ArenaHalfWidth, -GameRules.ArenaHalfHeight), "Arena bounds", ref passed);
            Check(GameRules.SegmentHitsCircle(Vector2.zero, new Vector2(10, 0), new Vector2(5, 0), 0.4f, out float along)
                && Mathf.Approximately(along, 0.5f), "Fast spell hits", ref passed);
            Check(!GameRules.SegmentHitsCircle(Vector2.zero, new Vector2(10, 0), new Vector2(5, 2), 0.4f, out _),
                "Spell misses", ref passed);
            Check(GameRules.SegmentHitsCircle(Vector2.zero, Vector2.zero, Vector2.zero, 0.4f, out _),
                "Stationary overlap", ref passed);
            Check(!GameRules.SegmentHitsCircle(Vector2.zero, Vector2.zero, Vector2.one, 0.4f, out _),
                "Stationary miss", ref passed);
            Debug.Log("Arcane Survival: " + passed + " logic checks passed. Play-test checklist: TESTING.md.");
        }

        private static void Check(bool condition, string name, ref int passed)
        {
            if (!condition) throw new System.Exception("Arcane Survival test failed: " + name);
            passed++;
        }
    }
}
