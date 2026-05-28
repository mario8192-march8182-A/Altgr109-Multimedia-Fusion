namespace AkpEngine.Core
{
    /// <summary>
    /// Interface para componentes do jogo
    /// </summary>
    public interface IGameComponent
    {
        void Initialize();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
        bool Enabled { get; set; }
        bool Visible { get; set; }
    }
}
