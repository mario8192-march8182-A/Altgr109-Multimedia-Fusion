using System;

namespace AkpEngine.Core
{
    /// <summary>
    /// Gerencia o tempo de execução do jogo
    /// </summary>
    public class GameTime
    {
        public TimeSpan ElapsedGameTime { get; set; }
        public TimeSpan TotalGameTime { get; set; }
        public float DeltaTime => (float)ElapsedGameTime.TotalSeconds;

        public GameTime()
        {
            ElapsedGameTime = TimeSpan.Zero;
            TotalGameTime = TimeSpan.Zero;
        }
    }
}
