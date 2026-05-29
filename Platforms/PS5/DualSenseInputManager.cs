// CORRIGIDO: Erro #7 — clamp RGB exige Math
using System; 
using AkpEngine.Platforms.PS5;

namespace AkpEngine.Platforms.PS5
{
    // CORRIGIDO: Erro #10 — implementar interface IInputManager
    /// <summary>
    /// Interface para gerenciadores de input.
    /// </summary>
    public interface IInputManager
    {
        bool GetButtonState(PS5Button button);
        (float X, float Y) GetStickValue(StickSide stick);
        void SetAdaptiveTrigger(TriggerSide side, TriggerEffect effect,
                                float startPosition, float endPosition,
                                float strength);
        void PlayHapticAudio(byte[] pcmData, int sampleRate);
        void SetLightBarColor(float r, float g, float b);
    }

    /// <summary>
    /// Gerencia input e feedback do controle DualSense.
    /// </summary>
    public class DualSenseInputManager : IInputManager
    {
        private readonly DualSenseMapping _mapping;

        // CORRIGIDO: Erro #9 — verificar SDK presente
        private static readonly bool _sdkPresent = ProsperoSdkDetector.IsAvailable();

        public DualSenseInputManager(DualSenseMapping mapping)
        {
            _mapping = mapping;
        }

        /// <summary>
        /// Lê o estado atual de um botão do DualSense.
        /// </summary>
        [SonySDKCall]
        public bool GetButtonState(PS5Button button)
        {
            // CORRIGIDO: Erro #9 — lançar SdkNotFoundException
            if (!_sdkPresent)
                throw new SdkNotFoundException("Prospero SDK não encontrado. Instale o PS5 SDK licenciado e configure a variável SCE_PROSPERO_SDK_DIR.");
            return false;
        }

        /// <summary>
        /// Lê o valor analógico de um stick.
        /// </summary>
        // CORRIGIDO: Erro #4 — usar StickSide em vez de PS5Button
        [SonySDKCall]
        public (float X, float Y) GetStickValue(StickSide stick)
        {
            if (!_sdkPresent)
                throw new SdkNotFoundException("Prospero SDK não encontrado. Instale o PS5 SDK licenciado e configure a variável SCE_PROSPERO_SDK_DIR.");
            return (0f, 0f);
        }

        /// <summary>
        /// Configura os gatilhos adaptativos do DualSense.
        /// </summary>
        [SonySDKCall]
        public void SetAdaptiveTrigger(TriggerSide side,
                                       TriggerEffect effect,
                                       float startPosition,
                                       float endPosition,
                                       float strength)
        {
            if (!_sdkPresent)
                throw new SdkNotFoundException("Prospero SDK não encontrado. Instale o PS5 SDK licenciado e configure a variável SCE_PROSPERO_SDK_DIR.");

            // CORRIGIDO: Erro #5 — validar ranges
            if (startPosition < 0f || startPosition > 1f)
                throw new ArgumentOutOfRangeException(nameof(startPosition), "startPosition deve estar entre 0.0 e 1.0");
            if (endPosition < 0f || endPosition > 1f)
                throw new ArgumentOutOfRangeException(nameof(endPosition), "endPosition deve estar entre 0.0 e 1.0");
            if (strength < 0f || strength > 1f)
                throw new ArgumentOutOfRangeException(nameof(strength), "strength deve estar entre 0.0 e 1.0");
            if (startPosition >= endPosition)
                throw new ArgumentException("startPosition deve ser menor que endPosition");
        }

        /// <summary>
        /// Reproduz áudio háptico no speaker do controle.
        /// </summary>
        [SonySDKCall]
        public void PlayHapticAudio(byte[] pcmData, int sampleRate)
        {
            if (!_sdkPresent)
                throw new SdkNotFoundException("Prospero SDK não encontrado. Instale o PS5 SDK licenciado e configure a variável SCE_PROSPERO_SDK_DIR.");

            // CORRIGIDO: Erro #6 — validar pcmData e sampleRate
            if (pcmData == null)
                throw new ArgumentNullException(nameof(pcmData));
            if (pcmData.Length == 0)
                throw new ArgumentException("pcmData não pode ser vazio.", nameof(pcmData));
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate), "sampleRate deve ser maior que zero.");
        }

        /// <summary>
        /// Define a cor da barra de luz do DualSense.
        /// </summary>
        [SonySDKCall]
        public void SetLightBarColor(float r, float g, float b)
        {
            if (!_sdkPresent)
                throw new SdkNotFoundException("Prospero SDK não encontrado. Instale o PS5 SDK licenciado e configure a variável SCE_PROSPERO_SDK_DIR.");

            // CORRIGIDO: Erro #7 — clamp RGB
            r = Math.Clamp(r, 0f, 1f);
            g = Math.Clamp(g, 0f, 1f);
            b = Math.Clamp(b, 0f, 1f);
        }
    }

    /// <summary>
    /// Mapeamento padrão dos botões do DualSense.
    /// </summary>
    public class DualSenseMapping
    {
        public PS5Button Confirm    { get; set; } = PS5Button.Cross;
        public PS5Button Cancel     { get; set; } = PS5Button.Circle;
        public PS5Button Action1    { get; set; } = PS5Button.Square;
        public PS5Button Action2    { get; set; } = PS5Button.Triangle;
        public PS5Button Pause      { get; set; } = PS5Button.Options;
        public PS5Button DpadUp     { get; set; } = PS5Button.Up;
        public PS5Button DpadDown   { get; set; } = PS5Button.Down;
        public PS5Button DpadLeft   { get; set; } = PS5Button.Left;
        public PS5Button DpadRight  { get; set; } = PS5Button.Right;

        // CORRIGIDO: Erro #8 — separar press e eixo analógico
        public PS5Button LeftStickPress  { get; set; } = PS5Button.L3;
        public PS5Button RightStickPress { get; set; } = PS5Button.R3;
        public StickSide LeftAnalog      { get; set; } = StickSide.Left;
        public StickSide RightAnalog     { get; set; } = StickSide.Right;
    }

    /// <summary>
    /// Enumeração dos botões do DualSense.
    /// </summary>
    public enum PS5Button
    {
        Cross, Circle, Square, Triangle,
        Options, Up, Down, Left, Right,
        L1, R1, L2, R2, L3, R3
    }

    /// <summary>
    /// Enumeração dos sticks analógicos.
    /// </summary>
    // CORRIGIDO: Erro #4 — novo enum StickSide
    public enum StickSide { Left, Right }

    /// <summary>
    /// Lado do gatilho adaptativo.
    /// </summary>
    public enum TriggerSide { Left, Right }

    /// <summary>
    /// Tipos de efeito de gatilho adaptativo.
    /// </summary>
    public enum TriggerEffect { Resistance, Vibration, Weapon, Custom }
}
