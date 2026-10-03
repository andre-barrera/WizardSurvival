using UnityEngine;

namespace ArcaneSurvival
{
    public sealed class PlayerController
    {
        public Vector2 Position { get; private set; }
        public Vector2 Facing { get; private set; } = Vector2.right;
        public int Health { get; private set; } = GameRules.MaxHealth;
        public float Invulnerability { get; private set; }
        public readonly SpriteRenderer View;
        private float castCooldown;
        public const float Speed = 4.5f;

        public PlayerController(PixelArt art, Transform parent)
        {
            View = art.Draw("Wizard", art.Wizard, parent, Vector2.zero, Vector2.one, Color.white, 10);
        }

        public void Reset()
        {
            Position = Vector2.zero;
            Facing = Vector2.right;
            Health = GameRules.MaxHealth;
            Invulnerability = 0;
            castCooldown = 0;
            View.transform.position = Position;
            View.color = Color.white;
            View.flipX = false;
        }

        public void Tick(float dt, ArcaneGame game)
        {
            Invulnerability = Mathf.Max(0, Invulnerability - dt);
            castCooldown -= dt;
            Vector2 movement = GameInput.Move;
            Position = GameRules.ClampToArena(Position + movement * (Speed * dt));
            if (movement.sqrMagnitude > 0) Facing = movement;

            // Mouse casts toward the pointer; Space aims toward the nearest enemy.
            Vector2 aim = GameInput.MouseFire
                ? ((Vector2)game.WorldCamera.ScreenToWorldPoint(GameInput.MousePosition) - Position).normalized
                : game.DirectionToNearestEnemy(Position, Facing);
            if (aim.sqrMagnitude < 0.01f) aim = Facing;
            if ((GameInput.MouseFire || GameInput.SpaceHeld) && castCooldown <= 0)
            {
                game.Cast(Position, aim);
                castCooldown = 0.24f;
            }

            float bob = movement.sqrMagnitude > 0 ? Mathf.Sin(game.Elapsed * 18) * 0.045f : 0;
            View.transform.position = Position + Vector2.up * bob;
            View.flipX = Facing.x < 0;
            View.sortingOrder = 20 - Mathf.RoundToInt(Position.y * 2);
            View.color = Invulnerability > 0 && Mathf.FloorToInt(Invulnerability * 12) % 2 == 0
                ? new Color(1, 1, 1, 0.35f) : Color.white;
        }

        public bool TakeDamage()
        {
            if (Invulnerability > 0 || Health <= 0) return false;
            Health--;
            Invulnerability = 1.15f;
            return true;
        }

        public void Heal() => Health = Mathf.Min(GameRules.MaxHealth, Health + 1);
    }
}
