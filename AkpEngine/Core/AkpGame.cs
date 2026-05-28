using System;
using System.Collections.Generic;

namespace AkpEngine.Core
{
    /// <summary>
    /// Classe base para todas as aplicações AkpEngine
    /// </summary>
    public abstract class AkpGame : IDisposable
    {
        public string Title { get; set; } = "AkpEngine Game";
        public int Width { get; set; } = 1024;
        public int Height { get; set; } = 768;
        public float TargetFrameRate { get; set; } = 60f;

        private bool _isRunning;
        private GameTime _gameTime;
        private List<IGameComponent> _components;

        public AkpGame()
        {
            _components = new List<IGameComponent>();
            _gameTime = new GameTime();
        }

        /// <summary>
        /// Inicializa a engine e os componentes
        /// </summary>
        public virtual void Initialize()
        {
            Console.WriteLine($"[AkpEngine] Inicializando {Title}");
            Console.WriteLine($"[AkpEngine] Resolução: {Width}x{Height}");
            Console.WriteLine($"[AkpEngine] FPS Target: {TargetFrameRate}");
        }

        /// <summary>
        /// Carrega conteúdo (texturas, sons, etc)
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Descarrega conteúdo
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Lógica de atualização do jogo
        /// </summary>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Renderização da cena
        /// </summary>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// Loop principal do jogo
        /// </summary>
        public void Run()
        {
            Initialize();
            LoadContent();
            _isRunning = true;

            DateTime lastFrameTime = DateTime.Now;

            while (_isRunning)
            {
                DateTime now = DateTime.Now;
                double elapsedMilliseconds = (now - lastFrameTime).TotalMilliseconds;
                lastFrameTime = now;

                _gameTime.ElapsedGameTime = TimeSpan.FromMilliseconds(elapsedMilliseconds);
                _gameTime.TotalGameTime = _gameTime.TotalGameTime.Add(_gameTime.ElapsedGameTime);

                Update(_gameTime);
                Draw(_gameTime);

                // Frame rate limiting
                double frameTime = 1000f / TargetFrameRate;
                double sleepTime = frameTime - elapsedMilliseconds;
                if (sleepTime > 0)
                {
                    System.Threading.Thread.Sleep((int)sleepTime);
                }
            }

            UnloadContent();
            Dispose();
        }

        public void Exit()
        {
            _isRunning = false;
        }

        public void AddComponent(IGameComponent component)
        {
            _components.Add(component);
            component.Initialize();
        }

        public void RemoveComponent(IGameComponent component)
        {
            _components.Remove(component);
        }

        public void Dispose()
        {
            Console.WriteLine("[AkpEngine] Encerrando aplicação");
            GC.Collect();
        }
    }
}
