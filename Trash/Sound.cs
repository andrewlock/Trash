using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using NetEscapades.EnumGenerators;

namespace Trash
{
    /// <summary>
    /// An enum for all of the Trash sounds
    /// </summary>
    [EnumExtensions]
    public enum MusicType
    {
        /// <summary>
        /// Silence
        /// </summary>
        [Description("")]
        NoSound,
        /// <summary>
        /// Title Screen music
        /// </summary>
        [Description("")]
        MusicTitle,
        /// <summary>
        /// In game music
        /// </summary>
        [Description(@"Audio\wav\one big rush")]
        MusicGame,
        /// <summary>
        /// GameOver
        /// </summary>
        [Description("")]
        MusicGameOver,
    }

    /// <summary>
    /// An enum for all of the Trash sounds
    /// </summary>
    [EnumExtensions]
    public enum SoundEffectType
    {
        /// <summary>
        /// Start game
        /// </summary>
        [Description(@"Audio\wav\start_3")]
        StartGame,
        /// <summary>
        /// Move cursor
        /// </summary>
        [Description(@"Audio\wav\navigate_1")]
        Navigate,
        /// <summary>
        /// Break Pills
        /// </summary>
        [Description(@"Audio\wav\navigate_1")]
        BreakPills,
        /// <summary>
        /// Bonus sound for large clear
        /// </summary>
        [Description(@"Audio\wav\clear_illegal")]
        TrashDrop,
        /// <summary>
        /// Pills landing after breaking
        /// </summary>
        [Description(@"Audio\wav\drop1")]
        LandPills1,
        [Description(@"Audio\wav\drop2")]
        LandPills2,
        /// <summary>
        /// Board cleared
        /// </summary>
        [Description(@"Audio\wav\clear_bonus")]
        MusicBoardCleared
    }

    /// <summary>
    /// Abstracts away the sounds for a simple interface using the Sounds enum
    /// </summary>
    public static class Sound
    {
        private static SoundEffect[] _musicBacking;
        private static SoundEffectInstance[] _music;
        private static SoundEffect[] _effects;

        /// <summary>
        /// Starts up the sound code, if the standard wave bank can't be used, load the 
        /// LowWaveBank (which does not include music)
        /// </summary>
        public static void Initialize(ContentManager content)
        {
            _effects = new SoundEffect[SoundEffectTypeExtensions.Length];
            for (int i = 0; i < _effects.Length; i++)
            {
                var soundEffect = ((SoundEffectType)i).ToStringFast(useMetadataAttributes: true);
                _effects[i] = content.Load<SoundEffect>(soundEffect);
            }

            _musicBacking = new SoundEffect[MusicTypeExtensions.Length];
            _music = new SoundEffectInstance[MusicTypeExtensions.Length];
            for (int i = 0; i < _music.Length; i++)
            {
                var musicType = (MusicType)i;
                var music = musicType.ToStringFast(useMetadataAttributes: true);
                if (string.IsNullOrEmpty(music))
                {
                    _musicBacking[i] = null;
                    _music[i] = null;
                    continue;
                }

                var effect = content.Load<SoundEffect>(music);
                _musicBacking[i] = effect;
                var instance = effect.CreateInstance();
                instance.IsLooped = true;
                _music[i] = instance;
            }
        }

        public static void Play(SoundEffectType soundEffect)
        {
            _effects[(int)soundEffect].Play();
        }

        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="sound">Which sound to play</param>
        /// <returns>XACT cue to be used if you want to stop this particular looped 
        /// sound. Can be ignored for one shot sounds</returns>
        public static SoundEffectInstance Play(MusicType sound)
        {
            var music = _music[(int)sound];
            if (music is null)
            {
                return null;
            }

            music.Play();
            return music;
        }

        /// <summary>
        /// Shuts down the sound code tidily
        /// </summary>
        public static void Shutdown()
        {
            foreach (var soundEffect in _effects)
            {
                soundEffect.Dispose();
            }

            foreach (var soundEffectInstance in _music)
            {
                if (soundEffectInstance is not null)
                {
                    soundEffectInstance.Stop(immediate: true);
                    soundEffectInstance.Dispose();
                }
            }

            foreach (var backing in _musicBacking)
            {
                if (backing is not null)
                {
                    backing.Dispose();
                }
            }
        }

        /// <summary>
        /// Stops a previously playing cue
        /// </summary>
        /// <param name="sound">The cue to stop that you got returned from Play(sound)
        /// </param>
        public static void Stop(SoundEffectInstance sound)
        {
            if (sound is not null)
            {
                sound.Stop(immediate: true);
            }
        }
    }
}
