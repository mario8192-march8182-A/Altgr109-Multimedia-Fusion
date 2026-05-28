using AkpEngine.Core;
using AkpEngine.Renderer;
using AkpEngine.Physics;
using AkpEngine.Audio;
using AkpEngine.Input;
using System.Numerics;

namespace AkpEngine.Examples
{
    /// <summary>
    /// Exemplo simples de um jogo 2D com AkpEngine
    /// </summary>
    public class SimpleGame : AkpGame
    {
        private Sprite _playerSprite;
        private PhysicsBody _playerBody;
        private InputManager _inputManager;
        private AudioManager _audioManager;

        public SimpleGame()
        {
            Title = "Simple Game - AkpEngine";
            Width = 1024;
            Height = 768;
            TargetFrameRate = 60f;
        }

        public override void Initialize()
        {
            base.Initialize();
            _inputManager = new InputManager();
            _audioManager = new AudioManager();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Criar sprite do player
            _playerSprite = new Sprite("player", "Assets/player.png", 64, 64)
            {
                Position = new Vector2(512, 384)
            };

            // Criar corpo de física
            _playerBody = new PhysicsBody(_playerSprite)
            {
                UseGravity = true,
                Mass = 1f
            };

            // Carregar áudio
            _audioManager.LoadAudio("jump", "Assets/jump.wav");
            _audioManager.LoadAudio("background", "Assets/background_music.mp3");
            _audioManager.Play("background", loop: true);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _inputManager.Update();

            // Movimento
            if (_inputManager.IsKeyPressed(KeyCode.Left))
            {
                _playerBody.ApplyForce(new Vector2(-5f, 0));
            }
            if (_inputManager.IsKeyPressed(KeyCode.Right))
            {
                _playerBody.ApplyForce(new Vector2(5f, 0));
            }
            if (_inputManager.IsKeyJustPressed(KeyCode.Space))
            {
                _playerBody.ApplyForce(new Vector2(0, -15f));
                _audioManager.Play("jump");
            }

            // Atualizar física
            _playerBody.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            // Colisão com limites da tela
            if (_playerSprite.Position.X < 0)
                _playerSprite.Position = new Vector2(0, _playerSprite.Position.Y);
            if (_playerSprite.Position.X > Width - _playerSprite.Width)
                _playerSprite.Position = new Vector2(Width - _playerSprite.Width, _playerSprite.Position.Y);
            if (_playerSprite.Position.Y > Height)
            {
                _playerSprite.Position = new Vector2(512, 384);
                _playerBody.Velocity = Vector2.Zero;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            // Renderização do sprite aqui
        }
    }
}
