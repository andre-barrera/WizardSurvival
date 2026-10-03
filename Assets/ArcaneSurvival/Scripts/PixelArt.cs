using System.Collections.Generic;
using UnityEngine;

namespace ArcaneSurvival
{
    public sealed class PixelArt
    {
        public readonly Sprite Wizard;
        public readonly Sprite Enemy;
        public readonly Sprite Circle;
        public readonly Sprite Square;
        public readonly Material Material;
        private readonly List<Object> owned = new List<Object>();

        // Creates the game art using pixel patterns
        public PixelArt()
        {
            Material = new Material(Resources.Load<Shader>("ArcaneSprite"));
            owned.Add(Material);
            Wizard = Pattern(new[] {
                "........p.......", ".......ppp......", "......pppp......",
                "......pPppp.....", ".....ppPpppp....", "....ppPPppppp...",
                "...ppppppppppp..", "..PPPPPPPPPPPPP.", ".....hhhh.......",
                "....hhEhEh...cc.", ".....hhhh....CC.", ".....wwww....bb.",
                "....pwwwpp...bb.", "...pppwwppph.bb.", "...pppppppphhbb.",
                "....ppPPpp...bb.", "...pppPPppp..bb.", "...pppPPppp..bb.",
                "....bb..bb......", "...bbb..bbb....." });
            Enemy = Pattern(new[] {
                ".....ggggg......", "...ggGGGGGgg....", "..gGGGGGGGGGg...",
                ".gGGGGGGGGGGGg..", ".gGGEEGGGEEGGg..", "gGGGEEGGGEEGGGg.",
                "gGGGGGGGGGGGGGg.", "gGGGGGgggGGGGGg.", "gGGGGGGGGGGGGGg.",
                ".gGGGGGGGGGGGg..", ".ggGGGGGGGGGgg..", "..gggGGGGGggg...",
                "...ggg.ggg.gg...", "....gg..gg......" });
            Square = Shape(false);
            Circle = Shape(true);
        }

        // Converts rows of palette characters into a pixel texture and a centered sprite.
        private Sprite Pattern(string[] rows)
        {
            int width = rows[0].Length;
            var texture = new Texture2D(width, rows.Length, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < width; x++)
                    texture.SetPixel(x, rows.Length - y - 1, Ink(rows[y][x]));
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, rows.Length),
                new Vector2(0.5f, 0.5f), 16);
            owned.Add(texture);
            owned.Add(sprite);
            return sprite;
        }

        // creates circle sprite
        private Sprite Shape(bool circle)
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    texture.SetPixel(x, y, !circle || new Vector2(x - 15.5f, y - 15.5f).sqrMagnitude < 240
                        ? Color.white : Color.clear);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
            owned.Add(texture);
            owned.Add(sprite);
            return sprite;
        }

        private static Color Ink(char c)
        {
            switch (c)
            {
                case 'p': return new Color32(99, 64, 174, 255);
                case 'P': return new Color32(175, 131, 242, 255);
                case 'h': return new Color32(246, 199, 154, 255);
                case 'E': return new Color32(16, 27, 46, 255);
                case 'w': return new Color32(226, 239, 246, 255);
                case 'b': return new Color32(90, 67, 75, 255);
                case 'c': return new Color32(104, 246, 226, 255);
                case 'C': return new Color32(221, 255, 247, 255);
                case 'g': return new Color32(40, 120, 124, 255);
                case 'G': return new Color32(99, 228, 173, 255);
                default: return Color.clear;
            }
        }

        // Creates and objet and set how the sprite appears
        public SpriteRenderer Draw(string name, Sprite sprite, Transform parent,
            Vector2 position, Vector2 scale, Color color, int order)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = new Vector3(scale.x, scale.y, 1);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = Material;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        // Restarts sprites
        public void Dispose()
        {
            foreach (Object asset in owned) Object.Destroy(asset);
            owned.Clear();
        }
    }
}
