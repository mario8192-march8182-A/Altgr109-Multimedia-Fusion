# Sistema de Cenas e Animações

## Cenas (Scenes)

O sistema de cenas permite organizar seu jogo em diferentes níveis e estados.

### Criar uma Cena

```csharp
public class GameScene : Scene
{
    public GameScene(string name) : base(name)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        // Inicializar lógica
    }

    public override void LoadContent()
    {
        base.LoadContent();
        // Carregar assets
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // Lógica de atualização
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        // Renderizar
    }
}
```

### Registrar e Carregar Cenas

```csharp
// Registrar cena
var scene = new GameScene("MainGame");
SceneManager.RegisterScene(scene);

// Carregar cena
SceneManager.LoadScene("MainGame");

// Carregar cena de forma assíncrona
SceneManager.LoadSceneAsync("MainGame");
```

### Câmera

Cada cena tem uma câmera para controlar a visualização:

```csharp
var camera = scene.GetCamera();
camera.SetZoom(2f);
camera.Follow(playerX, playerY);
```

## Animações

O sistema de animações permite criar sequências de movimento fluidas.

### Criar uma Animação

```csharp
// Método 1: Manual
var animation = new Animation("Walk");
animation.AddFrame(0, 0.1f, new Rectangle(0, 0, 64, 64));
animation.AddFrame(1, 0.1f, new Rectangle(64, 0, 64, 64));
animation.AddFrame(2, 0.1f, new Rectangle(128, 0, 64, 64));

// Método 2: Spritesheet
var animation = Animation.CreateFromSpriteSheet("Walk", 64, 64, 8, 0.1f);
```

### Usar Animações

```csharp
// Criar animator para um sprite
var animator = new Animator(playerSprite);

// Registrar animações
animator.RegisterAnimation("Walk", walkAnimation);
animator.RegisterAnimation("Jump", jumpAnimation);

// Reproduzir animação
animator.PlayAnimation("Walk", loop: true);

// Controlar animação
animator.PauseAnimation();
animator.ResumeAnimation();
animator.StopAnimation();

// Atualizar no loop do jogo
animator.Update(deltaTime);
```

### Exemplo Completo com Cena

```csharp
public class PlayerGameScene : Scene
{
    private Sprite playerSprite;
    private Animator playerAnimator;

    public override void LoadContent()
    {
        base.LoadContent();

        // Criar sprite
        playerSprite = new Sprite("player", "Assets/player.png", 64, 64);

        // Criar animator
        playerAnimator = new Animator(playerSprite);

        // Registrar animações
        var idleAnim = Animation.CreateFromSpriteSheet("Idle", 64, 64, 4, 0.15f);
        var runAnim = Animation.CreateFromSpriteSheet("Run", 64, 64, 8, 0.1f);

        playerAnimator.RegisterAnimation("Idle", idleAnim);
        playerAnimator.RegisterAnimation("Run", runAnim);

        playerAnimator.PlayAnimation("Idle", loop: true);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Verificar entrada
        if (inputManager.IsKeyPressed(KeyCode.Right))
        {
            playerAnimator.PlayAnimation("Run", loop: true);
        }
        else
        {
            playerAnimator.PlayAnimation("Idle", loop: true);
        }

        // Atualizar animator
        playerAnimator.Update(deltaTime);
    }
}
```

## Arquivo Application.nkp

O arquivo `Application.nkp` é um arquivo XML que contém configurações de exportação:

### Android (res/raw/Application.nkp)
```xml
<?xml version="1.0" encoding="utf-8"?>
<AkpApplication>
    <Project>
        <Name>MeuJogo</Name>
        <Version>1.0.0</Version>
        <Author>Seu Nome</Author>
        <Width>1024</Width>
        <Height>768</Height>
        <StartScene>MainScene</StartScene>
    </Project>
    <Platform>
        <Name>Android</Name>
        <MinSdkVersion>21</MinSdkVersion>
    </Platform>
    <Settings>
        <FullScreen>true</FullScreen>
        <VSync>true</VSync>
        <TargetFPS>60</TargetFPS>
    </Settings>
</AkpApplication>
```

### PC (Application.nkp na raiz)
Similar ao Android, mas com configurações específicas do PC.

### HTML5 (Application.nkp na raiz)
Contém configurações de renderização WebGL e responsividade.

Você pode carregar e processar essas configurações em tempo de execução para customizar o comportamento da aplicação.
