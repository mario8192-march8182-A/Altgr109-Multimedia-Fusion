using System;
using System.Collections.Generic;

namespace AkpEngine.Core
{
    /// <summary>
    /// Representa uma cena (nível) no jogo
    /// </summary>
    public abstract class Scene
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsInitialized { get; set; }

        protected List<IGameComponent> _gameObjects;
        protected Camera _camera;

        public Scene(string name)
        {
            Name = name;
            IsActive = true;
            IsInitialized = false;
            _gameObjects = new List<IGameComponent>();
            _camera = new Camera();
        }

        /// <summary>
        /// Inicializa a cena
        /// </summary>
        public virtual void Initialize()
        {
            Console.WriteLine($"[Scene] Inicializando cena: {Name}");
            IsInitialized = true;
        }

        /// <summary>
        /// Carrega conteúdo da cena
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Descarrega conteúdo da cena
        /// </summary>
        public virtual void UnloadContent()
        {
            _gameObjects.Clear();
        }

        /// <summary>
        /// Atualiza a lógica da cena
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            foreach (var obj in _gameObjects)
            {
                if (obj is { Enabled: true })
                {
                    obj.Update(gameTime);
                }
            }
        }

        /// <summary>
        /// Renderiza a cena
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {
            foreach (var obj in _gameObjects)
            {
                if (obj is { Visible: true })
                {
                    obj.Draw(gameTime);
                }
            }
        }

        /// <summary>
        /// Adiciona um objeto de jogo à cena
        /// </summary>
        public void AddGameObject(IGameComponent gameObject)
        {
            _gameObjects.Add(gameObject);
            gameObject.Initialize();
            Console.WriteLine($"[Scene] GameObject adicionado à cena '{Name}'");
        }

        /// <summary>
        /// Remove um objeto de jogo da cena
        /// </summary>
        public void RemoveGameObject(IGameComponent gameObject)
        {
            _gameObjects.Remove(gameObject);
        }

        /// <summary>
        /// Obtém a câmera da cena
        /// </summary>
        public Camera GetCamera()
        {
            return _camera;
        }

        /// <summary>
        /// Define a câmera da cena
        /// </summary>
        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }
    }

    /// <summary>
    /// Representa a câmera de visualização
    /// </summary>
    public class Camera
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Zoom { get; set; } = 1f;

        public Camera()
        {
            X = 0;
            Y = 0;
            Width = 1024;
            Height = 768;
        }

        public void Follow(float targetX, float targetY)
        {
            X = targetX - Width / 2;
            Y = targetY - Height / 2;
        }

        public void SetZoom(float zoom)
        {
            Zoom = Math.Max(0.1f, zoom);
        }
    }
}
