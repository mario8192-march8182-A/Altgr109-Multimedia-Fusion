namespace AkpEngine.Platforms.PS5
{
    /// <summary>
    /// Pipeline gráfico 2D usando GNM/GNMX.
    /// </summary>
    public class GnmRenderer
    {
        [SonySDKCall]
        public void InitializeGnmDevice() { }

        [SonySDKCall]
        public void CreateSwapChain(int width, int height) { }

        [SonySDKCall]
        public void SetRenderTarget(object target) { }

        [SonySDKCall]
        public void Submit(object buffer) { }
    }
}
