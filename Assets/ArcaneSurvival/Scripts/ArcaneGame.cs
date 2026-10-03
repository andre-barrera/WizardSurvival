using System.Collections.Generic;
using UnityEngine;

namespace ArcaneSurvival
{
    public enum GameState { Title, Playing, Paused, GameOver }

    // Properties that store the games state
    public sealed class ArcaneGame : MonoBehaviour
    {
        public Camera WorldCamera { get; private set; }
        public GameState State { get; private set; } = GameState.Title;
        public float Elapsed { get; private set; }
        public int Score { get; private set; }
        public int Best { get; private set; }
        public int Kills { get; private set; }
        public int Wave => GameRules.WaveAt(Elapsed);
        public PlayerController Player { get; private set; }
        public SoundBank Sounds { get; private set; }
        public string Notice { get; private set; } = "";
        public float NoticeTime { get; private set; }

        private PixelArt art;
        private Transform dynamicRoot;
        private readonly List<EnemyController> enemies = new List<EnemyController>();
        private readonly List<SpellProjectile> spells = new List<SpellProjectile>();
        private readonly List<Spark> sparks = new List<Spark>();
        private float spawnTimer;
        private int lastWave = 1;
        private const int EnemyLimit = 70;
        private const string BestKey = "ArcaneSurvival.BestScore.v1";
        private readonly Color mint = new Color32(113, 237, 211, 255);

        private sealed class Spark
        {
            public SpriteRenderer View;
            public Vector2 Velocity;
            public Color Color;
            public float Life = 0.4f;
        }

        // Creates the camera, player, arena, etc every time the game starts
        private void Awake()
        {
            art = new PixelArt();
            var cameraObject = new GameObject("Arcane camera");
            cameraObject.transform.SetParent(transform);
            cameraObject.transform.position = new Vector3(0, 0, -10);
            WorldCamera = cameraObject.AddComponent<Camera>();
            WorldCamera.orthographic = true;
            WorldCamera.clearFlags = CameraClearFlags.SolidColor;
            WorldCamera.backgroundColor = new Color32(12, 17, 32, 255);
            cameraObject.AddComponent<AudioListener>();
            FitCamera();
            dynamicRoot = new GameObject("Runtime actors").transform;
            dynamicRoot.SetParent(transform, false);
            BuildArena();
            Player = new PlayerController(art, dynamicRoot);
            Sounds = new SoundBank(gameObject);
            Best = PlayerPrefs.GetInt(BestKey, 0);
            gameObject.AddComponent<GameHUD>().Initialize(this);
        }

        // Adjust the camera depending on the screen size
        private void FitCamera()
        {
            WorldCamera.orthographicSize = Mathf.Max(6.5f, 11.4f / Mathf.Max(0.1f, WorldCamera.aspect));
        }

        // Creates the background and other elements
        private void BuildArena()
        {
            Transform floor = new GameObject("Arena artwork").transform;
            floor.SetParent(transform, false);
            art.Draw("Floor", art.Square, floor, Vector2.zero, new Vector2(21, 11),
                new Color32(22, 33, 47, 255), -20);
            for (int x = -10; x <= 10; x++)
                for (int y = -5; y <= 5; y++)
                    art.Draw("Stone tile", art.Square, floor, new Vector2(x, y), Vector2.one * 0.95f,
                        (x + y) % 2 == 0 ? new Color32(26, 39, 53, 255) : new Color32(29, 43, 56, 255), -19);
            Border(floor, new Vector2(0, 5.45f), new Vector2(21.2f, 0.12f));
            Border(floor, new Vector2(0, -5.45f), new Vector2(21.2f, 0.12f));
            Border(floor, new Vector2(10.55f, 0), new Vector2(0.12f, 11));
            Border(floor, new Vector2(-10.55f, 0), new Vector2(0.12f, 11));
            for (int i = 0; i < 40; i++)
            {
                float angle = i * Mathf.PI * 2 / 40;
                Vector2 p = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.15f;
                var rune = art.Draw("Summoning ring", art.Square, floor, p,
                    new Vector2(0.16f, 0.05f), new Color32(54, 79, 91, 255), -18);
                rune.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            }
            foreach (Vector2 p in new[] { new Vector2(-9, -4), new Vector2(9, -4), new Vector2(-9, 4), new Vector2(9, 4) })
            {
                art.Draw("Pillar base", art.Square, floor, p, new Vector2(0.7f, 0.4f), new Color32(49, 62, 81, 255), -17);
                var crystal = art.Draw("Crystal", art.Square, floor, p + Vector2.up * 0.4f,
                    Vector2.one * 0.35f, mint, -16);
                crystal.transform.rotation = Quaternion.Euler(0, 0, 45);
            }
        }

        // Creates a rectangular border
        private void Border(Transform root, Vector2 position, Vector2 size)
        {
            art.Draw("Arena border", art.Square, root, position, size, new Color32(70, 117, 130, 255), -16);
        }

        // Resets the game
        public void StartRun()
        {
            ClearActors();
            Player.Reset();
            Score = 0;
            Kills = 0;
            Elapsed = 0;
            lastWave = 1;
            spawnTimer = 1;
            Notice = "WAVE 01";
            NoticeTime = 2;
            Sounds.Stop();
            State = GameState.Playing;
        }

        // Pause the game or resume it
        public void TogglePause()
        {
            if (State == GameState.Playing) { State = GameState.Paused; Sounds.Stop(); }
            else if (State == GameState.Paused) State = GameState.Playing;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && State == GameState.Playing) TogglePause();
        }

        // Handles menu and other inputs
        private void Update()
        {
            FitCamera();
            if (GameInput.Mute) Sounds.ToggleMute();
            if (GameInput.Confirm && (State == GameState.Title || State == GameState.GameOver))
            {
                StartRun();
                return;
            }
            if (GameInput.Restart && (State == GameState.GameOver || State == GameState.Paused))
            {
                StartRun();
                return;
            }
            if (GameInput.Pause) TogglePause();
            if (State != GameState.Playing) return;

            // Deals with spell collisions
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Elapsed += dt;
            NoticeTime = Mathf.Max(0, NoticeTime - dt);
            if (Wave != lastWave)
            {
                lastWave = Wave;
                Notice = "WAVE " + Wave.ToString("00");
                NoticeTime = 2;
            }
            Player.Tick(dt, this);
            spawnTimer -= dt;
            if (spawnTimer <= 0)
            {
                if (enemies.Count < EnemyLimit) SpawnEnemy();
                spawnTimer = GameRules.SpawnInterval(Wave);
            }
            foreach (EnemyController enemy in enemies) enemy.Tick(dt, Player.Position, Elapsed);
            TickSpells(dt);
            TickEnemyContact();
            TickSparks(dt);
        }

        // Spawns a wisp at an arena edge, choosing its type and keeping it away from the player.
        private void SpawnEnemy()
        {
            Vector2 position;
            int edge = Random.Range(0, 4);
            if (edge < 2) position = new Vector2(edge == 0 ? -10 : 10, Random.Range(-4.8f, 4.8f));
            else position = new Vector2(Random.Range(-9.7f, 9.7f), edge == 2 ? -5.1f : 5.1f);
            // Give a nearby player extra room and a visible spawn grace period.
            if (Vector2.Distance(position, Player.Position) < 3) position = -position;
            enemies.Add(new EnemyController(art, dynamicRoot, position, Wave, Wave >= 3 && Random.value < 0.3f));
        }

        // Returns a direction toward the closest active enemy, or the fallback if none exist.
        public Vector2 DirectionToNearestEnemy(Vector2 origin, Vector2 fallback)
        {
            Vector2 direction = fallback;
            float closest = float.MaxValue;
            foreach (EnemyController enemy in enemies)
            {
                if (!enemy.Alive || enemy.SpawnDelay > 0) continue;
                float distance = (enemy.Position - origin).sqrMagnitude;
                if (distance >= closest) continue;
                closest = distance;
                direction = (enemy.Position - origin).normalized;
            }
            return direction;
        }

        // Creates a spell at the supplied position and plays the casting sound.
        public void Cast(Vector2 origin, Vector2 direction)
        {
            spells.Add(new SpellProjectile(art, dynamicRoot, origin, direction));
            Sounds.Cast();
        }

        // Moves spells, awards points for hits, applies earned healing, and removes expired spells.
        private void TickSpells(float dt)
        {
            for (int i = spells.Count - 1; i >= 0; i--)
            {
                SpellProjectile spell = spells[i];
                EnemyController hit = spell.Tick(dt, enemies);
                if (hit != null)
                {
                    hit.Alive = false;
                    Score += hit.Points;
                    Kills++;
                    Burst(hit.Position, mint, 7);
                    Sounds.Defeat();
                    if (Kills % 15 == 0 && Player.Health < GameRules.MaxHealth)
                    {
                        Player.Heal();
                        Sounds.Heal();
                        Notice = "+1 HEALTH";
                        NoticeTime = 1.5f;
                    }
                }
                if (spell.Lifetime <= 0)
                {
                    Destroy(spell.View.gameObject);
                    spells.RemoveAt(i);
                }
            }
        }

        // Removes defeated enemies, checks player contact damage, and saves a new best score at game over.
        private void TickEnemyContact()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyController enemy = enemies[i];
                if (!enemy.Alive)
                {
                    Destroy(enemy.View.gameObject);
                    enemies.RemoveAt(i);
                    continue;
                }
                if (enemy.SpawnDelay > 0 || Vector2.Distance(enemy.Position, Player.Position) > enemy.Radius + 0.32f)
                    continue;
                if (!Player.TakeDamage()) continue;
                Sounds.Hurt();
                Burst(Player.Position, new Color32(255, 133, 153, 255), 10);
                Vector2 push = (enemy.Position - Player.Position).normalized;
                if (push.sqrMagnitude < 0.01f) push = Vector2.up;
                enemy.Position = GameRules.ClampToArena(enemy.Position + push * 1.4f);
                if (Player.Health <= 0)
                {
                    State = GameState.GameOver;
                    if (Score > Best)
                    {
                        Best = Score;
                        PlayerPrefs.SetInt(BestKey, Best);
                        PlayerPrefs.Save();
                    }
                    break;
                }
            }
        }

        // Creates a group of short-lived particles with random velocities at the hit position.
        private void Burst(Vector2 position, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Random.insideUnitCircle * 3;
                sparks.Add(new Spark { View = art.Draw("Magic spark", art.Square, dynamicRoot,
                    position, Vector2.one * 0.09f, color, 55), Velocity = velocity, Color = color });
            }
        }

        // Moves and fades hit particles, removing them when their lifetime ends.
        private void TickSparks(float dt)
        {
            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                Spark spark = sparks[i];
                spark.Life -= dt;
                if (spark.Life <= 0) { Destroy(spark.View.gameObject); sparks.RemoveAt(i); continue; }
                spark.View.transform.position += (Vector3)(spark.Velocity * dt);
                spark.View.color = new Color(spark.Color.r, spark.Color.g, spark.Color.b, spark.Life / 0.4f);
            }
        }

        // Destroys the current enemies, spells, and particles and clears their lists.
        private void ClearActors()
        {
            foreach (EnemyController enemy in enemies) Destroy(enemy.View.gameObject);
            foreach (SpellProjectile spell in spells) Destroy(spell.View.gameObject);
            foreach (Spark spark in sparks) Destroy(spark.View.gameObject);
            enemies.Clear();
            spells.Clear();
            sparks.Clear();
        }

        // Releases the generated sound clips, textures, sprites, and material when this game is destroyed.
        private void OnDestroy()
        {
            Sounds?.Dispose();
            art?.Dispose();
        }
    }
}
