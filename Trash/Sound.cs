using System;
using Microsoft.Xna.Framework.Audio;

namespace Trash
{
    /// <summary>
    /// An enum for all of the Trash sounds
    /// </summary>
    public enum SoundEntry
    {
        /// <summary>
        /// Silence
        /// </summary>
        NoSound,
        /// <summary>
        /// Title Screen music
        /// </summary>
        MusicTitle,
        /// <summary>
        /// In game music
        /// </summary>
        MusicGame,
        /// <summary>
        /// GameOver
        /// </summary>
        MusicGameOver,
        /// <summary>
        /// Board cleared
        /// </summary>
        MusicBoardCleared,
        /// <summary>
        /// Start game
        /// </summary>
        StartGame,
        /// <summary>
        /// Move cursor
        /// </summary>
        Navigate,
        /// <summary>
        /// Break Pills
        /// </summary>
        BreakPills,
        /// <summary>
        /// Bonus sound for large clear
        /// </summary>
        TrashDrop,
        /// <summary>
        /// Pills landing after breaking
        /// </summary>
        LandPills,
    }

    /// <summary>
    /// Abstracts away the sounds for a simple interface using the Sounds enum
    /// </summary>
    public static class Sound
    {
        /// <summary>
        /// The cue names in xact corresponding to the SoundEntry enums
        /// </summary>
        private static string[] cueNames = new string[]
        {
            "Silence", //No sound
            "Silence", //Title Screen
            "Music_Game", //In-Game Music
            "Silence", //Game Over
            "Clear_Board", //Clear Board
            "Game_Start", //start Game
            "Navigate", //Cursor Move
            "Pill_Break", //Pills Breaking
            "Trash", //Trash Drop sound
            "Pill_Land", //Pill impact sound (after fall)

        };

        private static AudioEngine engine;
        private static SoundBank soundbank;
        private static WaveBank wavebank;

        /// <summary>
        /// Starts up the sound code, if the standard wave bank can't be used, load the 
        /// LowWaveBank (which does not include music)
        /// </summary>
        public static void Initialize()
        {
            engine = new AudioEngine(@"Content\Audio\Trash.xgs");
            try
            {
                wavebank = new WaveBank(engine, @"Content\Audio\Wave Bank.xwb");
            }
            catch (Exception)
            {
                wavebank = new WaveBank(engine, @"Content\Audio\LowWaveBank.xwb");
            }
            try
            {
                soundbank = new SoundBank(engine, @"Content\audio\Sound Bank.xsb");
            }
            catch (Exception)
            {
                soundbank = new SoundBank(engine, @"Content\audio\LowSoundBank.xsb");
            }

        }

        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="cueName">Which sound to play</param>
        /// <returns>XACT cue to be used if you want to stop this particular looped 
        /// sound. Can be ignored for one shot sounds</returns>
        public static Cue Play(string cueName)
        {
            Cue returnValue = soundbank.GetCue(cueName);
            returnValue.Play();
            return returnValue;
        }

        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="sound">Which sound to play</param>
        /// <returns>XACT cue to be used if you want to stop this particular looped 
        /// sound. Can be ignored for one shot sounds</returns>
        public static Cue Play(SoundEntry sound)
        {
            return Play(cueNames[(int)sound]);
        }

        /// <summary>
        /// Shuts down the sound code tidily
        /// </summary>
        public static void Shutdown()
        {
            if (soundbank != null) soundbank.Dispose();
            if (wavebank != null) wavebank.Dispose();
            if (engine != null) engine.Dispose();
        }

        /// <summary>
        /// Stops a previously playing cue
        /// </summary>
        /// <param name="cue">The cue to stop that you got returned from Play(sound)
        /// </param>
        public static void Stop(Cue cue)
        {
            if (cue != null)
            {
                cue.Stop(AudioStopOptions.Immediate);
            }
        }
    }
}
