using System;
using System.Collections.Generic;

namespace AkpEngine.Input
{
    /// <summary>
    /// Gerenciador de entrada (teclado, mouse, gamepad)
    /// </summary>
    public class InputManager
    {
        private HashSet<KeyCode> _currentlyPressedKeys;
        private HashSet<KeyCode> _previouslyPressedKeys;

        public InputManager()
        {
            _currentlyPressedKeys = new HashSet<KeyCode>();
            _previouslyPressedKeys = new HashSet<KeyCode>();
        }

        /// <summary>
        /// Atualiza o estado de entrada
        /// </summary>
        public void Update()
        {
            _previouslyPressedKeys = new HashSet<KeyCode>(_currentlyPressedKeys);
        }

        /// <summary>
        /// Registra uma tecla pressionada
        /// </summary>
        public void RegisterKeyDown(KeyCode key)
        {
            _currentlyPressedKeys.Add(key);
        }

        /// <summary>
        /// Registra uma tecla liberada
        /// </summary>
        public void RegisterKeyUp(KeyCode key)
        {
            _currentlyPressedKeys.Remove(key);
        }

        /// <summary>
        /// Verifica se uma tecla está sendo pressionada
        /// </summary>
        public bool IsKeyPressed(KeyCode key)
        {
            return _currentlyPressedKeys.Contains(key);
        }

        /// <summary>
        /// Verifica se uma tecla foi pressionada neste frame
        /// </summary>
        public bool IsKeyJustPressed(KeyCode key)
        {
            return _currentlyPressedKeys.Contains(key) && !_previouslyPressedKeys.Contains(key);
        }

        /// <summary>
        /// Verifica se uma tecla foi liberada neste frame
        /// </summary>
        public bool IsKeyJustReleased(KeyCode key)
        {
            return !_currentlyPressedKeys.Contains(key) && _previouslyPressedKeys.Contains(key);
        }
    }

    /// <summary>
    /// Enum com os códigos de tecla suportados
    /// </summary>
    public enum KeyCode
    {
        // Letras
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        // Números
        D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
        // Setas
        Up, Down, Left, Right,
        // Especiais
        Space, Enter, Escape, Tab, Shift, Control, Alt,
        // F Keys
        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12
    }
}
