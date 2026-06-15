using AkpEngine.Core;
using AkpEngine.Renderer;
using AkpEngine.Physics;
using AkpEngine.Audio;
using AkpEngine.Input;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AkpEngine.Examples
{
    /// <summary>
    /// Exemplo FNaF 2 - Five Nights at Freddy's 2 simplificado
    /// </summary>
    public class FNaF2Game : AkpGame
    {
        public FNaF2Game()
        {
            Title = "Five Nights at Freddy's 2 - AkpEngine";
            Width = 1280;
            Height = 720;
            TargetFrameRate = 60f;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Criar cenas
            var officeScene = new FNaF2OfficeScene("OfficeScene");
            var menuScene = new FNaF2MenuScene("MenuScene");

            SceneManager.RegisterScene(menuScene);
            SceneManager.RegisterScene(officeScene);
            
            // Carregar cena de menu
            SceneManager.LoadScene("MenuScene");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }

    /// <summary>
    /// Cena de Menu do FNaF 2
    /// </summary>
    public class FNaF2MenuScene : Scene
    {
        private Sprite _titleSprite;
        private Sprite _buttonSprite;
        private bool _startGamePressed = false;

        public FNaF2MenuScene(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Criar título
            _titleSprite = new Sprite("title", "Assets/fnaf2_title.png", 400, 100)
            {
                Position = new Vector2(440, 150)
            };

            // Criar botão de iniciar
            _buttonSprite = new Sprite("button_start", "Assets/button_start.png", 200, 80)
            {
                Position = new Vector2(540, 400)
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Verificar clique no botão (simulado com Enter)
            var inputManager = new InputManager();
            inputManager.Update();

            if (inputManager.IsKeyJustPressed(KeyCode.Enter) || inputManager.IsKeyJustPressed(KeyCode.Space))
            {
                _startGamePressed = true;
                Console.WriteLine("[FNaF2Menu] Iniciando jogo...");
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            // Renderizar título e botão aqui
        }
    }

    /// <summary>
    /// Cena principal do escritório - FNaF 2
    /// </summary>
    public class FNaF2OfficeScene : Scene
    {
        // Câmeras de segurança
        private Dictionary<string, SecurityCamera> _cameras;
        private SecurityCamera _currentCamera;
        
        // Animatrôs
        private List<Animatronic> _animatronics;
        private Animatronic _toy_freddy;
        private Animatronic _toy_bonnie;
        private Animatronic _toy_chica;
        private Animatronic _mangle;
        private Animatronic _balloon_boy;
        private Animatronic _puppet;

        // UI e controles
        private int _power = 100;
        private int _night = 1;
        private float _nightTimer = 0f;
        private const float NIGHT_DURATION = 480f; // 8 minutos = 1 noite
        private bool _gameover = false;
        private float _powerDrainRate = 0.1f;

        // Áudio
        private AudioManager _audioManager;
        
        // Jumpscare
        private bool _jumpscare = false;
        private float _jumpsacareTimer = 0f;

        public FNaF2OfficeScene(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();
            _cameras = new Dictionary<string, SecurityCamera>();
            _animatronics = new List<Animatronic>();
            _audioManager = new AudioManager();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Console.WriteLine("[FNaF2] Carregando cenário...");

            // Inicializar câmeras
            InitializeCameras();

            // Inicializar animatrôs
            InitializeAnimatronics();

            // Carregar áudio
            LoadAudio();

            // Câmera padrão
            _currentCamera = _cameras["office"];
            _nightTimer = 0f;
            _gameover = false;

            Console.WriteLine("[FNaF2] Noite 1 - Comece!");
        }

        private void InitializeCameras()
        {
            // Câmera do escritório
            var officeCamera = new SecurityCamera("office", "Office", 0, 0);
            _cameras.Add("office", officeCamera);

            // Câmera da sala de estar
            var showStageCamera = new SecurityCamera("show_stage", "Show Stage", 1, 1);
            _cameras.Add("show_stage", showStageCamera);

            // Câmera da sala de pirates cove
            var piratesCamera = new SecurityCamera("pirates_cove", "Pirate's Cove", 2, 2);
            _cameras.Add("pirates_cove", piratesCamera);

            // Câmera da cozinha
            var kitchenCamera = new SecurityCamera("kitchen", "Kitchen", 3, 1);
            _cameras.Add("kitchen", kitchenCamera);

            // Câmera do saguão
            var hallwayCamera = new SecurityCamera("hallway", "Hallway", 4, 0);
            _cameras.Add("hallway", hallwayCamera);
        }

        private void InitializeAnimatronics()
        {
            // Toy Freddy - Começa no Show Stage
            _toy_freddy = new Animatronic("Toy Freddy", "ShowStage", 
                new Vector2(300, 200), 100, "fnaf2_toy_freddy");
            _toy_freddy.AILevel = 3;
            _animatronics.Add(_toy_freddy);

            // Toy Bonnie - Começa na cozinha
            _toy_bonnie = new Animatronic("Toy Bonnie", "Kitchen", 
                new Vector2(800, 300), 95, "fnaf2_toy_bonnie");
            _toy_bonnie.AILevel = 4;
            _animatronics.Add(_toy_bonnie);

            // Toy Chica - Começa no Show Stage
            _toy_chica = new Animatronic("Toy Chica", "ShowStage", 
                new Vector2(500, 250), 98, "fnaf2_toy_chica");
            _toy_chica.AILevel = 3;
            _animatronics.Add(_toy_chica);

            // Mangle - Começa no Pirate's Cove
            _mangle = new Animatronic("Mangle", "PiratesCove", 
                new Vector2(200, 150), 110, "fnaf2_mangle");
            _mangle.AILevel = 5;
            _animatronics.Add(_mangle);

            // Balloon Boy - Começa no Hallway
            _balloon_boy = new Animatronic("Balloon Boy", "Hallway", 
                new Vector2(1000, 400), 90, "fnaf2_balloon_boy");
            _balloon_boy.AILevel = 2;
            _animatronics.Add(_balloon_boy);

            // The Puppet - Começa na caixa de música
            _puppet = new Animatronic("The Puppet", "MusicBox", 
                new Vector2(640, 100), 120, "fnaf2_puppet");
            _puppet.AILevel = 5;
            _animatronics.Add(_puppet);
        }

        private void LoadAudio()
        {
            _audioManager.LoadAudio("ambient_night", "Assets/fnaf2_ambient.wav");
            _audioManager.LoadAudio("jumpscare", "Assets/fnaf2_jumpscare.wav");
            _audioManager.LoadAudio("camera_switch", "Assets/camera_switch.wav");
            _audioManager.LoadAudio("breathing", "Assets/breathing.wav");
            _audioManager.LoadAudio("animatronic_sound", "Assets/animatronic_sound.wav");

            // Iniciar áudio ambiente
            _audioManager.Play("ambient_night", loop: true);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_gameover)
                return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Atualizar timer da noite
            _nightTimer += deltaTime;
            
            // Dreno de energia
            _power -= (int)(_powerDrainRate * deltaTime);
            if (_power < 0) _power = 0;

            // Verificar se é fim da noite
            if (_nightTimer >= NIGHT_DURATION)
            {
                Console.WriteLine($"[FNaF2] Noite {_night} completada!");
                _nightTimer = 0f;
                _night++;
                _power = 100;
            }

            // Atualizar câmeras
            UpdateCameras(deltaTime);

            // Atualizar animatrôs
            UpdateAnimatronics(deltaTime);

            // Processar entrada
            ProcessInput();

            // Checar se há jumpscare
            CheckJumpscares();
        }

        private void UpdateCameras(float deltaTime)
        {
            foreach (var camera in _cameras.Values)
            {
                camera.Update(deltaTime);
            }
        }

        private void UpdateAnimatronics(float deltaTime)
        {
            foreach (var animatronic in _animatronics)
            {
                animatronic.Update(deltaTime, _night);

                // Debug
                if (animatronic.IsActive)
                {
                    Console.WriteLine($"[FNaF2] {animatronic.Name} - Sala: {animatronic.CurrentLocation}, " +
                        $"Posição: X={animatronic.Position.X:F0}, Agressividade: {animatronic.AILevel}");
                }
            }
        }

        private void ProcessInput()
        {
            var inputManager = new InputManager();
            inputManager.Update();

            // Navegar entre câmeras
            if (inputManager.IsKeyJustPressed(KeyCode.Left) || inputManager.IsKeyJustPressed(KeyCode.A))
            {
                SwitchCamera("previous");
                _audioManager.Play("camera_switch");
            }

            if (inputManager.IsKeyJustPressed(KeyCode.Right) || inputManager.IsKeyJustPressed(KeyCode.D))
            {
                SwitchCamera("next");
                _audioManager.Play("camera_switch");
            }

            // Número das câmeras
            if (inputManager.IsKeyJustPressed(KeyCode.D1))
                SelectCamera("office");
            if (inputManager.IsKeyJustPressed(KeyCode.D2))
                SelectCamera("show_stage");
            if (inputManager.IsKeyJustPressed(KeyCode.D3))
                SelectCamera("pirates_cove");
            if (inputManager.IsKeyJustPressed(KeyCode.D4))
                SelectCamera("kitchen");
            if (inputManager.IsKeyJustPressed(KeyCode.D5))
                SelectCamera("hallway");
        }

        private void SwitchCamera(string direction)
        {
            var cameraList = new List<string> { "office", "show_stage", "pirates_cove", "kitchen", "hallway" };
            int currentIndex = cameraList.FindIndex(c => c == _currentCamera.ID);

            if (direction == "next")
            {
                currentIndex = (currentIndex + 1) % cameraList.Count;
            }
            else if (direction == "previous")
            {
                currentIndex = (currentIndex - 1 + cameraList.Count) % cameraList.Count;
            }

            SelectCamera(cameraList[currentIndex]);
        }

        private void SelectCamera(string cameraID)
        {
            if (_cameras.TryGetValue(cameraID, out var camera))
            {
                _currentCamera = camera;
                Console.WriteLine($"[FNaF2] Câmera selecionada: {camera.Name}");
            }
        }

        private void CheckJumpscares()
        {
            foreach (var animatronic in _animatronics)
            {
                // Se o animatrônico está no escritório e não há defesa ativa
                if (animatronic.CurrentLocation == "Office" && !_gameover)
                {
                    _jumpscare = true;
                    _jumpsacareTimer = 2f;
                    _gameover = true;
                    _audioManager.Play("jumpscare");
                    Console.WriteLine($"[FNaF2] JUMPSCARE! {animatronic.Name} entrou!");
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Renderizar informações na tela
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         FIVE NIGHTS AT FREDDY'S 2 - AkpEngine         ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Noite: {_night,-49} ║");
            Console.WriteLine($"║ Energia: {_power:D3}% {new string('█', (_power / 10)),-39} ║");
            Console.WriteLine($"║ Hora: {(_nightTimer / NIGHT_DURATION * 12):F1}:00 {new string(' ', 41)} ║");
            Console.WriteLine($"║ Câmera: {_currentCamera.Name,-44} ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
            
            foreach (var animatronic in _animatronics)
            {
                string status = animatronic.IsActive ? "ATIVO" : "Inativo";
                Console.WriteLine($"║ {animatronic.Name,-15} - {animatronic.CurrentLocation,-20} [{status}] ║");
            }

            Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ Controles: [1-5] Câmeras | [Setas/WASD] Navegar         ║");
            if (_gameover)
            {
                Console.WriteLine("║                    *** GAME OVER ***                      ║");
            }
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        }
    }

    /// <summary>
    /// Classe para câmeras de segurança
    /// </summary>
    public class SecurityCamera
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public bool IsActive { get; set; }

        public SecurityCamera(string id, string name, int gridX, int gridY)
        {
            ID = id;
            Name = name;
            GridX = gridX;
            GridY = gridY;
            IsActive = false;
        }

        public void Update(float deltaTime)
        {
            // Lógica de câmera
        }
    }

    /// <summary>
    /// Classe para animatrônicos
    /// </summary>
    public class Animatronic
    {
        public string Name { get; set; }
        public string CurrentLocation { get; set; }
        public Vector2 Position { get; set; }
        public int AILevel { get; set; }
        public bool IsActive { get; set; }
        private int _aggression;
        private float _moveTimer;
        private Random _random;
        private Sprite _sprite;

        private List<string> _locations = new List<string>
        {
            "ShowStage", "Kitchen", "PiratesCove", "Hallway", "Office", "MusicBox"
        };

        public Animatronic(string name, string startLocation, Vector2 position, int aggression, string spritePath)
        {
            Name = name;
            CurrentLocation = startLocation;
            Position = position;
            _aggression = aggression;
            AILevel = 1;
            IsActive = true;
            _moveTimer = 0f;
            _random = new Random();
            _sprite = new Sprite(name.ToLower(), spritePath, 100, 150)
            {
                Position = position
            };
        }

        public void Update(float deltaTime, int nightNumber)
        {
            _moveTimer += deltaTime;

            // Aumentar agressividade baseado na noite
            int nightAggression = (int)(AILevel * (1 + (nightNumber * 0.5f)));

            // Chance de movimento a cada segundo
            if (_moveTimer >= 1f)
            {
                _moveTimer = 0f;

                // Determinar próxima localização baseado em agressividade
                if (_random.Next(0, 100) < nightAggression * 15)
                {
                    MoveToNextLocation();
                }
            }
        }

        private void MoveToNextLocation()
        {
            // Seleção de próxima localização (simplificada)
            int nextIndex = (_locations.IndexOf(CurrentLocation) + 1) % _locations.Count;
            CurrentLocation = _locations[nextIndex];

            Console.WriteLine($"[Animatronic] {Name} se moveu para {CurrentLocation}");
        }
    }
}
