using System;
using System.Numerics;

namespace AkpEngine.Renderer
{
    /// <summary>
    /// Representa um sprite 2D renderizável
    /// </summary>
    public class Sprite
    {
        public string Name { get; set; }
        public string TexturePath { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public float Rotation { get; set; }
        public float Alpha { get; set; } = 1f;
        public Vector2 Origin { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsVisible { get; set; } = true;

        public Sprite(string name, string texturePath, int width, int height)
        {
            Name = name;
            TexturePath = texturePath;
            Width = width;
            Height = height;
            Position = Vector2.Zero;
        }

        /// <summary>
        /// Calcula o retângulo da hitbox
        /// </summary>
        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)(Width * Scale.X),
                (int)(Height * Scale.Y)
            );
        }
    }

    /// <summary>
    /// Estrutura para representar um retângulo
    /// </summary>
    public struct Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersects(Rectangle other)
        {
            return X < other.X + other.Width &&
                   X + Width > other.X &&
                   Y < other.Y + other.Height &&
                   Y + Height > other.Y;
        }
    }
}
