using System;
using System.Collections.Generic;

namespace AkpEngine.Audio
{
    /// <summary>
    /// Gerenciador centralizado de áudio
    /// </summary>
    public class AudioManager
    {
        private Dictionary<string, AudioClip> _audioClips;
        private Dictionary<string, float> _volumes;
        private float _masterVolume = 1f;

        public float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = Math.Clamp(value, 0f, 1f);
        }

        public AudioManager()
        {
            _audioClips = new Dictionary<string, AudioClip>();
            _volumes = new Dictionary<string, float>();
        }

        /// <summary>
        /// Carrega um arquivo de áudio
        /// </summary>
        public void LoadAudio(string name, string filePath)
        {
            if (_audioClips.ContainsKey(name))
            {
                Console.WriteLine($"[AudioManager] Áudio '{name}' já carregado");
                return;
            }

            var clip = new AudioClip(name, filePath);
            _audioClips.Add(name, clip);
            _volumes.Add(name, 1f);
            Console.WriteLine($"[AudioManager] Áudio '{name}' carregado");
        }

        /// <summary>
        /// Toca um áudio
        /// </summary>
        public void Play(string name, bool loop = false)
        {
            if (_audioClips.TryGetValue(name, out var clip))
            {
                clip.Play(loop);
                Console.WriteLine($"[AudioManager] Tocando '{name}'");
            }
            else
            {
                Console.WriteLine($"[AudioManager] Áudio '{name}' não encontrado");
            }
        }

        /// <summary>
        /// Para um áudio
        /// </summary>
        public void Stop(string name)
        {
            if (_audioClips.TryGetValue(name, out var clip))
            {
                clip.Stop();
            }
        }

        /// <summary>
        /// Define o volume de um áudio
        /// </summary>
        public void SetVolume(string name, float volume)
        {
            if (_volumes.ContainsKey(name))
            {
                _volumes[name] = Math.Clamp(volume, 0f, 1f);
            }
        }
    }

    /// <summary>
    /// Representa um clip de áudio
    /// </summary>
    public class AudioClip
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public bool IsPlaying { get; private set; }

        public AudioClip(string name, string filePath)
        {
            Name = name;
            FilePath = filePath;
            IsPlaying = false;
        }

        public void Play(bool loop = false)
        {
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
        }
    }
}
