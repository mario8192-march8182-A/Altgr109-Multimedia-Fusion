using System;
using System.Collections.Generic;

namespace AkpEngine.Core
{
    /// <summary>
    /// Gerenciador centralizado de cenas
    /// </summary>
    public class SceneManager
    {
        private Dictionary<string, Scene> _scenes;
        private Scene _currentScene;
        private Scene _nextScene;
        private bool _isTransitioning;

        public Scene CurrentScene => _currentScene;

        public SceneManager()
        {
            _scenes = new Dictionary<string, Scene>();
            _currentScene = null;
            _nextScene = null;
            _isTransitioning = false;
        }

        /// <summary>
        /// Registra uma nova cena
        /// </summary>
        public void RegisterScene(Scene scene)
        {
            if (_scenes.ContainsKey(scene.Name))
            {
                Console.WriteLine($"[SceneManager] Cena '{scene.Name}' já registrada");
                return;
            }

            _scenes.Add(scene.Name, scene);
            Console.WriteLine($"[SceneManager] Cena '{scene.Name}' registrada");
        }

        /// <summary>
        /// Carrega uma cena
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (!_scenes.TryGetValue(sceneName, out var scene))
            {
                Console.WriteLine($"[SceneManager] Cena '{sceneName}' não encontrada");
                return;
            }

            Console.WriteLine($"[SceneManager] Carregando cena: {sceneName}");

            if (_currentScene != null)
            {
                _currentScene.UnloadContent();
            }

            _nextScene = scene;
            _isTransitioning = true;
        }

        /// <summary>
        /// Carrega uma cena de forma assíncrona
        /// </summary>
        public void LoadSceneAsync(string sceneName)
        {
            LoadScene(sceneName);
        }

        /// <summary>
        /// Atualiza o gerenciador de cenas
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (_isTransitioning && _nextScene != null)
            {
                _currentScene = _nextScene;
                _nextScene = null;

                if (!_currentScene.IsInitialized)
                {
                    _currentScene.Initialize();
                    _currentScene.LoadContent();
                }

                _isTransitioning = false;
                Console.WriteLine($"[SceneManager] Cena '{_currentScene.Name}' carregada");
            }

            _currentScene?.Update(gameTime);
        }

        /// <summary>
        /// Renderiza a cena atual
        /// </summary>
        public void Draw(GameTime gameTime)
        {
            _currentScene?.Draw(gameTime);
        }

        /// <summary>
        /// Obtém uma cena registrada
        /// </summary>
        public Scene GetScene(string sceneName)
        {
            _scenes.TryGetValue(sceneName, out var scene);
            return scene;
        }

        /// <summary>
        /// Remove uma cena
        /// </summary>
        public void UnregisterScene(string sceneName)
        {
            if (_scenes.Remove(sceneName))
            {
                Console.WriteLine($"[SceneManager] Cena '{sceneName}' removida");
            }
        }

        /// <summary>
        /// Obtém todas as cenas registradas
        /// </summary>
        public Dictionary<string, Scene> GetAllScenes()
        {
            return new Dictionary<string, Scene>(_scenes);
        }
    }
}
