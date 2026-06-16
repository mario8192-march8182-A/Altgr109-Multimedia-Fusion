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
    /// Exemplo FNaF 4 - Five Nights at Freddy's 4
    /// Sistema de portas, ventilador e detecção de presença
    /// </summary>
    public class FNaF4Game : AkpGame
    {
        public FNaF4Game()
        {
            Title = "Five Nights at Freddy's 4 - AkpEngine";
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

            var gameScene = new FNaF4BedroomScene("BedroomScene");
            SceneManager.RegisterScene(gameScene);
            SceneManager.LoadScene("BedroomScene");
        }
    }

    /// <summary>
    /// Cena do quarto - FNaF 4
    /// Sistema: Portas, Ventilador, Luz, Som
    /// </summary>
    public class FNaF4BedroomScene : Scene
    {
        // Variáveis de estado
        private int _power = 100;
        private int _night = 1;
        private float _nightTimer = 0f;
        private const float NIGHT_DURATION = 480f;
        private bool _gameover = false;

        // Animatrônicos
        private List<Animatronic4> _animatronics;
        private Animatronic4 _freddy;
        private Animatronic4 _bonnie;
        private Animatronic4 _chica;
        private Animatronic4 _foxy;

        // Controles de quarto
        private DoorController _leftDoor;
        private DoorController _rightDoor;
        private FanController _fan;
        private bool _lightsOn = false;
        private bool _jumpscareTriggered = false;

        // UI
        private AudioManager _audioManager;
        private int _jumpscares = 0;

        public FNaF4BedroomScene(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();
            _animatronics = new List<Animatronic4>();
            _audioManager = new AudioManager();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Console.WriteLine("[FNaF4] Iniciando Noite 1...");
            Console.WriteLine("[FNaF4] Você está em seu quarto. Não se mova.");

            // Inicializar portas
            _leftDoor = new DoorController("left", 0.5f);
            _rightDoor = new DoorController("right", 0.5f);
            _fan = new FanController();

            // Inicializar animatrônicos
            InitializeAnimatronics4();

            // Carregar áudio
            LoadAudio4();

            _nightTimer = 0f;
            _gameover = false;
        }

        private void InitializeAnimatronics4()
        {
            // Freddy
            _freddy = new Animatronic4("Freddy", "Outside", 85, "fnaf4_freddy");
            _freddy.Door = "left";
            _animatronics.Add(_freddy);

            // Bonnie
            _bonnie = new Animatronic4("Bonnie", "Outside", 80, "fnaf4_bonnie");
            _bonnie.Door = "left";
            _animatronics.Add(_bonnie);

            // Chica
            _chica = new Animatronic4("Chica", "Outside", 75, "fnaf4_chica");
            _chica.Door = "right";
            _animatronics.Add(_chica);

            // Foxy
            _foxy = new Animatronic4("Foxy", "Outside", 90, "fnaf4_foxy");
            _foxy.Door = "right";
            _animatronics.Add(_foxy);
        }

        private void LoadAudio4()
        {
            _audioManager.LoadAudio("ambient_bedroom", "Assets/fnaf4_ambient.wav");
            _audioManager.LoadAudio("door_knock", "Assets/door_knock.wav");
            _audioManager.LoadAudio("breathing", "Assets/breathing.wav");
            _audioManager.LoadAudio("jumpscare", "Assets/fnaf4_jumpscare.wav");
            _audioManager.LoadAudio("fan_loop", "Assets/fan_loop.wav");
            _audioManager.LoadAudio("static", "Assets/static.wav");

            _audioManager.Play("ambient_bedroom", loop: true);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_gameover)
                return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Timer da noite
            _nightTimer += deltaTime;

            // Dreno de energia
            float doorPowerUsage = (_leftDoor.IsClosed ? 0.5f : 0) + (_rightDoor.IsClosed ? 0.5f : 0);
            float fanPowerUsage = _fan.IsRunning ? 0.3f : 0;
            _power -= (int)((doorPowerUsage + fanPowerUsage) * deltaTime);
            if (_power < 0) _power = 0;

            // Fim da noite
            if (_nightTimer >= NIGHT_DURATION)
            {
                Console.WriteLine($"[FNaF4] Você sobreviveu à Noite {_night}!");
                _nightTimer = 0f;
                _night++;
                _power = 100;
            }

            // Processar entrada
            ProcessInput4();

            // Atualizar animatrônicos
            UpdateAnimatronics4(deltaTime);

            // Checar ataques
            CheckAttacks4();
        }

        private void ProcessInput4()
        {
            var inputManager = new InputManager();
            inputManager.Update();

            // Controle de portas
            if (inputManager.IsKeyJustPressed(KeyCode.Left) || inputManager.IsKeyJustPressed(KeyCode.A))
            {
                _leftDoor.IsClosed = !_leftDoor.IsClosed;
                Console.WriteLine($"[FNaF4] Porta esquerda: {(_leftDoor.IsClosed ? "FECHADA" : "ABERTA")}");
            }

            if (inputManager.IsKeyJustPressed(KeyCode.Right) || inputManager.IsKeyJustPressed(KeyCode.D))
            {
                _rightDoor.IsClosed = !_rightDoor.IsClosed;
                Console.WriteLine($"[FNaF4] Porta direita: {(_rightDoor.IsClosed ? "FECHADA" : "ABERTA")}");
            }

            // Ventilador
            if (inputManager.IsKeyJustPressed(KeyCode.Space))
            {
                _fan.IsRunning = !_fan.IsRunning;
                Console.WriteLine($"[FNaF4] Ventilador: {(_fan.IsRunning ? "LIGADO" : "DESLIGADO")}");
                if (_fan.IsRunning)
                    _audioManager.Play("fan_loop", loop: true);
            }

            // Luz
            if (inputManager.IsKeyJustPressed(KeyCode.L))
            {
                _lightsOn = !_lightsOn;
                Console.WriteLine($"[FNaF4] Luz: {(_lightsOn ? "LIGADA" : "DESLIGADA")}");
            }
        }

        private void UpdateAnimatronics4(float deltaTime)
        {
            foreach (var animatronic in _animatronics)
            {
                animatronic.Update(deltaTime, _night);

                // Calcular proximidade
                float proximity = animatronic.CalculateProximity();

                // Se está perto da porta
                if (proximity > 70)
                {
                    _audioManager.Play("door_knock");
                    
                    // Checar se porta está fechada
                    DoorController relevantDoor = animatronic.Door == "left" ? _leftDoor : _rightDoor;
                    
                    if (!relevantDoor.IsClosed)
                    {
                        // JUMPSCARE!
                        TriggerJumpscare(animatronic);
                    }
                    else
                    {
                        // Porta protege
                        animatronic.HasBeenStoppedByDoor = true;
                        Console.WriteLine($"[FNaF4] {animatronic.Name} tentou entrar mas foi bloqueado pela porta!");
                    }
                }
            }
        }

        private void CheckAttacks4()
        {
            foreach (var animatronic in _animatronics)
            {
                if (animatronic.CanAttack && !animatronic.HasBeenStoppedByDoor)
                {
                    TriggerJumpscare(animatronic);
                }
            }
        }

        private void TriggerJumpscare(Animatronic4 animatronic)
        {
            if (_jumpscareTriggered)
                return;

            _jumpscareTriggered = true;
            _gameover = true;
            _jumpscares++;

            _audioManager.Play("jumpscare");
            Console.WriteLine($"\n🚨 JUMPSCARE! {animatronic.Name} ENTROU NO QUARTO! 🚨\n");
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║    FIVE NIGHTS AT FREDDY'S 4 - Bedroom Scenario           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Noite: {_night}                                              ║");
            Console.WriteLine($"║ Energia: {_power:D3}% {new string('█', (_power / 10))}                         ║");
            Console.WriteLine($"║ Hora: {(_nightTimer / NIGHT_DURATION * 12):F1}:00                                  ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Porta Esquerda: {(_leftDoor.IsClosed ? "🔒 FECHADA" : "🔓 ABERTA ")}                       ║");
            Console.WriteLine($"║ Porta Direita:  {(_rightDoor.IsClosed ? "🔒 FECHADA" : "🔓 ABERTA ")}                       ║");
            Console.WriteLine($"║ Ventilador:     {(_fan.IsRunning ? "⚙️  LIGADO" : "⭕ DESLIGADO")}                        ║");
            Console.WriteLine($"║ Luz:            {(_lightsOn ? "💡 LIGADA" : "⭕ DESLIGADA")}                        ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ ANIMATRÔNICOS:                                             ║");

            foreach (var animatronic in _animatronics)
            {
                float proximity = animatronic.CalculateProximity();
                string proximityBar = new string('█', (int)(proximity / 10));
                Console.WriteLine($"║ • {animatronic.Name,-12} [{proximityBar,-10}] Porta: {animatronic.Door}      ║");
            }

            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ CONTROLES:                                                 ║");
            Console.WriteLine("║ [A/←] Porta Esquerda | [D/→] Porta Direita                ║");
            Console.WriteLine("║ [ESPAÇO] Ventilador  | [L] Luz                            ║");

            if (_gameover)
            {
                Console.WriteLine("║                                                            ║");
                Console.WriteLine("║              ⚠️  VOCÊ FOI DESCOBERTO!  ⚠️                   ║");
            }

            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        }
    }

    /// <summary>
    /// Controlador de porta
    /// </summary>
    public class DoorController
    {
        public string Side { get; set; }
        public bool IsClosed { get; set; }
        public float PowerUsage { get; set; }

        public DoorController(string side, float powerUsage)
        {
            Side = side;
            IsClosed = false;
            PowerUsage = powerUsage;
        }
    }

    /// <summary>
    /// Controlador de ventilador
    /// Mascara sons dos animatrônicos
    /// </summary>
    public class FanController
    {
        public bool IsRunning { get; set; }
        public float Efficiency { get; set; } = 0.7f; // 70% de chance de mascarar

        public bool MascarSound()
        {
            if (!IsRunning)
                return false;

            var random = new Random();
            return random.NextDouble() < Efficiency;
        }
    }

    /// <summary>
    /// Animatrônico para FNaF 4
    /// Sistema de proximidade e ataques
    /// </summary>
    public class Animatronic4
    {
        public string Name { get; set; }
        public string CurrentLocation { get; set; }
        public int AILevel { get; set; }
        public string Door { get; set; }
        public bool HasBeenStoppedByDoor { get; set; }
        public bool CanAttack { get; set; }

        private float _proximityValue;
        private float _moveTimer;
        private Random _random;
        private List<string> _path;
        private int _pathIndex;

        public Animatronic4(string name, string startLocation, int aiLevel, string spritePath)
        {
            Name = name;
            CurrentLocation = startLocation;
            AILevel = aiLevel;
            _proximityValue = 0f;
            _moveTimer = 0f;
            _random = new Random();
            HasBeenStoppedByDoor = false;
            CanAttack = false;

            // Definir caminho
            _path = new List<string> { "Outside", "Hallway", "Doorway", "In Room" };
            _pathIndex = 0;
        }

        public void Update(float deltaTime, int nightNumber)
        {
            _moveTimer += deltaTime;

            int nightMultiplier = (int)(1 + (nightNumber * 0.3f));
            int chance = AILevel * nightMultiplier * 15;

            if (_moveTimer >= 1f)
            {
                _moveTimer = 0f;

                if (_random.Next(0, 100) < chance)
                {
                    MoveForward();
                    UpdateProximity();
                }
            }

            // Aumentar proximidade se estiver se aproximando
            if (CurrentLocation == "In Room")
            {
                CanAttack = true;
            }
        }

        private void MoveForward()
        {
            _pathIndex = Math.Min(_pathIndex + 1, _path.Count - 1);
            CurrentLocation = _path[_pathIndex];
            Console.WriteLine($"[Animatronic] {Name} moveu-se para: {CurrentLocation}");
        }

        private void UpdateProximity()
        {
            _proximityValue = (_pathIndex / (float)_path.Count) * 100f;
        }

        public float CalculateProximity()
        {
            return _proximityValue;
        }
    }

    /// <summary>
    /// Exemplo FNaF World - Arena de batalha
    /// Sistema de luta por turnos com animatrônicos
    /// </summary>
    public class FNaFWorldGame : AkpGame
    {
        public FNaFWorldGame()
        {
            Title = "Five Nights at Freddy's: World - AkpEngine";
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

            var battleScene = new FNaFWorldBattleScene("BattleScene");
            SceneManager.RegisterScene(battleScene);
            SceneManager.LoadScene("BattleScene");
        }
    }

    /// <summary>
    /// Cena de Batalha - FNaF World
    /// </summary>
    public class FNaFWorldBattleScene : Scene
    {
        private CharacterParty _playerParty;
        private CharacterParty _enemyParty;
        private int _currentTurn = 0;
        private bool _playerTurn = true;
        private int _currentPlayerCharacter = 0;
        private AudioManager _audioManager;

        public FNaFWorldBattleScene(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();
            _audioManager = new AudioManager();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Console.WriteLine("[FNaF World] Iniciando Batalha!");

            // Criar time do jogador
            _playerParty = new CharacterParty();
            _playerParty.AddCharacter(new BattleCharacter("Freddy", 100, 20, 10));
            _playerParty.AddCharacter(new BattleCharacter("Bonnie", 90, 22, 8));
            _playerParty.AddCharacter(new BattleCharacter("Chica", 85, 18, 12));

            // Criar time inimigo
            _enemyParty = new CharacterParty();
            _enemyParty.AddCharacter(new BattleCharacter("Shadow Freddy", 110, 25, 15));
            _enemyParty.AddCharacter(new BattleCharacter("Nightmare Bonnie", 100, 28, 10));

            _audioManager.LoadAudio("battle_music", "Assets/fnaf_world_battle.wav");
            _audioManager.Play("battle_music", loop: true);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Checar vitória/derrota
            if (_playerParty.IsDefeated())
            {
                Console.WriteLine("[FNaF World] Você foi derrotado!");
                return;
            }

            if (_enemyParty.IsDefeated())
            {
                Console.WriteLine("[FNaF World] Você venceu!");
                return;
            }

            ProcessInput();

            if (_playerTurn)
            {
                // Turno do jogador
            }
            else
            {
                // Turno do inimigo
                ExecuteEnemyTurn();
                _playerTurn = true;
            }
        }

        private void ProcessInput()
        {
            var inputManager = new InputManager();
            inputManager.Update();

            var currentCharacter = _playerParty.Characters[_currentPlayerCharacter];

            if (inputManager.IsKeyJustPressed(KeyCode.D1))
            {
                // Ataque normal
                var target = _enemyParty.Characters[0];
                int damage = currentCharacter.Attack - target.Defense;
                target.Health -= damage;
                Console.WriteLine($"{currentCharacter.Name} atacou {target.Name} causando {damage} dano!");
                _playerTurn = false;
            }

            if (inputManager.IsKeyJustPressed(KeyCode.D2))
            {
                // Ataque especial
                if (currentCharacter.SpecialAttackReady)
                {
                    var target = _enemyParty.Characters[0];
                    int damage = (int)(currentCharacter.Attack * 1.5f) - target.Defense;
                    target.Health -= damage;
                    Console.WriteLine($"{currentCharacter.Name} usou ataque especial! {damage} dano!");
                    currentCharacter.SpecialAttackReady = false;
                    _playerTurn = false;
                }
            }

            if (inputManager.IsKeyJustPressed(KeyCode.D3))
            {
                // Trocar personagem
                _currentPlayerCharacter = (_currentPlayerCharacter + 1) % _playerParty.Characters.Count;
                Console.WriteLine($"Trocado para: {_playerParty.Characters[_currentPlayerCharacter].Name}");
            }
        }

        private void ExecuteEnemyTurn()
        {
            var enemy = _enemyParty.Characters[0];
            if (enemy.Health > 0)
            {
                var target = _playerParty.Characters[_currentPlayerCharacter];
                int damage = enemy.Attack - target.Defense;
                target.Health -= Math.Max(1, damage);
                Console.WriteLine($"{enemy.Name} atacou {target.Name} causando {damage} dano!");
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         FIVE NIGHTS AT FREDDY'S: WORLD - Battle           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");

            Console.WriteLine("║ INIMIGOS:                                                  ║");
            foreach (var character in _enemyParty.Characters)
            {
                if (character.Health > 0)
                    Console.WriteLine($"║ • {character.Name,-20} HP: {character.Health}/100                    ║");
            }

            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ SEU TIME:                                                  ║");
            for (int i = 0; i < _playerParty.Characters.Count; i++)
            {
                var character = _playerParty.Characters[i];
                string marker = (i == _currentPlayerCharacter) ? ">" : " ";
                if (character.Health > 0)
                    Console.WriteLine($"║{marker} {character.Name,-19} HP: {character.Health}/100                    ║");
            }

            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ AÇÕES:                                                     ║");
            Console.WriteLine("║ [1] Ataque Normal    [2] Ataque Especial                   ║");
            Console.WriteLine("║ [3] Trocar Personagem                                      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        }
    }

    /// <summary>
    /// Time de personagens
    /// </summary>
    public class CharacterParty
    {
        public List<BattleCharacter> Characters { get; set; }

        public CharacterParty()
        {
            Characters = new List<BattleCharacter>();
        }

        public void AddCharacter(BattleCharacter character)
        {
            Characters.Add(character);
        }

        public bool IsDefeated()
        {
            return Characters.TrueForAll(c => c.Health <= 0);
        }
    }

    /// <summary>
    /// Personagem em batalha
    /// </summary>
    public class BattleCharacter
    {
        public string Name { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public bool SpecialAttackReady { get; set; }

        public BattleCharacter(string name, int health, int attack, int defense)
        {
            Name = name;
            MaxHealth = health;
            Health = health;
            Attack = attack;
            Defense = defense;
            SpecialAttackReady = true;
        }
    }
}
