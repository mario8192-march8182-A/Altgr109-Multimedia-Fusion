using System.Numerics;
using AkpEngine.Renderer;

namespace AkpEngine.Physics
{
    /// <summary>
    /// Componente de física para sprites 2D
    /// </summary>
    public class PhysicsBody
    {
        public Sprite Sprite { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float Mass { get; set; } = 1f;
        public float Friction { get; set; } = 0.1f;
        public float Restitution { get; set; } = 0.8f; // Elasticidade
        public bool IsStatic { get; set; }
        public bool UseGravity { get; set; } = true;

        private const float GRAVITY = 9.81f;

        public PhysicsBody(Sprite sprite)
        {
            Sprite = sprite;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
        }

        /// <summary>
        /// Aplica força ao corpo
        /// </summary>
        public void ApplyForce(Vector2 force)
        {
            if (IsStatic) return;
            Acceleration += force / Mass;
        }

        /// <summary>
        /// Atualiza a física
        /// </summary>
        public void Update(float deltaTime)
        {
            if (IsStatic) return;

            // Aplicar gravidade
            if (UseGravity)
            {
                Acceleration += new Vector2(0, GRAVITY);
            }

            // Atualizar velocidade
            Velocity += Acceleration * deltaTime;

            // Aplicar atrito
            Velocity *= (1 - Friction);

            // Atualizar posição
            Sprite.Position += Velocity * deltaTime;

            // Resetar aceleração
            Acceleration = Vector2.Zero;
        }

        /// <summary>
        /// Detecta colisão com outro corpo
        /// </summary>
        public bool IsCollidingWith(PhysicsBody other)
        {
            return Sprite.GetBounds().Intersects(other.Sprite.GetBounds());
        }
    }
}
