using UnityEngine;

namespace ArcaneSurvival
{
    // IMGUI keeps setup dependency-free. It is sufficient for this small desktop game.
    public sealed class GameHUD : MonoBehaviour
    {
        private ArcaneGame game;
        private GUIStyle label;
        private GUIStyle button;
        private readonly Color mint = new Color32(130, 242, 219, 255);
        private readonly Color pale = new Color32(232, 239, 253, 255);
        private readonly Color muted = new Color32(153, 172, 197, 255);
        public void Initialize(ArcaneGame owner) => game = owner;

        private void OnGUI()
        {
            if (game == null || game.Player == null) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { wordWrap = true };
                button = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
            }
            Matrix4x4 previous = GUI.matrix;
            Color previousColor = GUI.color;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2,
                (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);

            Panel(new Rect(26, 20, 1228, 75), new Color(0.035f, 0.055f, 0.10f, 0.94f));
            Text(new Rect(47, 29, 300, 25), "ARCANE / SURVIVAL", 19, mint, TextAnchor.MiddleLeft);
            Text(new Rect(47, 55, 280, 24), "HOLD THE CIRCLE. KEEP THE LIGHT.", 11, muted, TextAnchor.MiddleLeft);
            Text(new Rect(425, 29, 200, 24), "SCORE  " + game.Score.ToString("0000"), 21, pale);
            Text(new Rect(640, 29, 180, 24), "WAVE  " + game.Wave.ToString("00"), 21, pale);
            Text(new Rect(835, 29, 150, 24), TimeLabel(game.Elapsed), 21, pale);
            for (int i = 0; i < GameRules.MaxHealth; i++)
                Panel(new Rect(1050 + i * 33, 38, 24, 14), i < game.Player.Health ? mint : new Color(0.18f, 0.24f, 0.32f));
            Text(new Rect(425, 58, 600, 22), "BEST " + game.Best + "    /    WISPS DEFEATED " + game.Kills, 12, muted);
            Text(new Rect(1048, 57, 185, 24), "HEALTH", 11, muted);
            Panel(new Rect(26, 660, 1228, 40), new Color(0.035f, 0.055f, 0.10f, 0.94f));
            Text(new Rect(43, 665, 1190, 28), "WASD / ARROWS  move     SPACE  auto-aim cast     LEFT MOUSE  aim + cast     P / ESC  pause     M  " +
                (game.Sounds.Muted ? "unmute" : "mute"), 14, muted, TextAnchor.MiddleCenter);

            if (game.State == GameState.Playing && game.NoticeTime > 0)
                Text(new Rect(400, 112, 480, 45), game.Notice, 26, mint, TextAnchor.MiddleCenter);
            if (game.State != GameState.Playing) DrawOverlay();
            GUI.matrix = previous;
            GUI.color = previousColor;
        }

        private void DrawOverlay()
        {
            Panel(new Rect(0, 100, 1280, 550), new Color(0.02f, 0.03f, 0.07f, 0.72f));
            Panel(new Rect(350, 150, 580, 440), new Color32(20, 29, 48, 250));
            Panel(new Rect(350, 150, 580, 4), mint);
            bool title = game.State == GameState.Title;
            bool paused = game.State == GameState.Paused;
            Text(new Rect(390, 180, 500, 24), "A SMALL SPELL. AN ENDLESS NIGHT.", 13, mint, TextAnchor.MiddleCenter);
            Text(new Rect(380, 215, 520, 65), title ? "ARCANE SURVIVAL" : paused ? "TAKE A BREATH" : "THE LIGHT FADES",
                34, pale, TextAnchor.MiddleCenter);
            string body = title
                ? "You are the last wizard in the circle.\nKeep moving. Cast spells. Survive the wisps.\n\nNew waves arrive every 20 seconds.\nEvery 15 defeats restore one health, if needed."
                : paused ? "Your run is paused.\nThe arena will wait for you.\n\nPress P or Escape to resume."
                : "Score  " + game.Score + "     |     Best  " + game.Best +
                  "\nSurvived  " + TimeLabel(game.Elapsed) + "     |     Wave  " + game.Wave +
                  "\n\nKeep moving and hold Space to cast.";
            Text(new Rect(398, 290, 484, 155), body, 19, muted, TextAnchor.MiddleCenter);
            GUI.backgroundColor = mint;
            if (GUI.Button(new Rect(435, 468, 410, 51), paused ? "RESUME" : title ? "ENTER THE CIRCLE" : "TRY AGAIN", button))
            {
                if (paused) game.TogglePause(); else game.StartRun();
            }
            GUI.backgroundColor = Color.white;
            Text(new Rect(395, 534, 490, 30), paused ? "R  restart from wave 1" : "ENTER  " + (title ? "start" : "play again") + "     /     M  toggle sound",
                14, muted, TextAnchor.MiddleCenter);
        }

        private void Panel(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void Text(Rect rect, string value, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft)
        {
            label.fontSize = size;
            label.normal.textColor = color;
            label.alignment = align;
            GUI.Label(rect, value, label);
        }

        private static string TimeLabel(float time) => ((int)time / 60).ToString("00") + ":" + ((int)time % 60).ToString("00");
    }
}
