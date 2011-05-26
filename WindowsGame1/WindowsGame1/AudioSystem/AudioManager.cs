using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FMOD;
using WGiBeat.Managers;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// A manager that handles all FMOD related functionality in the game. Audio files are
    /// loaded as Sound Effects, which are streamed and handled by FMOD. The primary method for
    /// interacting with playing audio is through channel ID numbers.
    /// </summary>
    public class AudioManager : Manager
    {
        #region Fields

        private readonly FMOD.System _fmodSystem = new FMOD.System();
        private readonly Dictionary<string, Sound> _sounds = new Dictionary<string, Sound>();

        private const int CHANNEL_COUNT = 8;
        private float _masterVolume = 1.0f;
        private Channel _tmpChannel = new Channel();

        public string FallbackSound { get; set; }
        #endregion 

        public AudioManager(LogManager log)
        {
            Log = log;
            Log.AddMessage("Initializing Audio Manager...",LogLevel.INFO);
            CheckFMODErrors(Factory.System_Create(ref _fmodSystem));
            CheckFMODErrors(_fmodSystem.init(CHANNEL_COUNT, INITFLAGS.NORMAL, (IntPtr)null));
        }

        #region Helpers

        /// <summary>
        /// Checks if FMOD is reporting any errors, and throws an Exception if one is found.
        /// </summary>
        /// <param name="result">The FMOD result code to check.</param>
        private void CheckFMODErrors(RESULT result)
        {
            switch (result)
            {
                case RESULT.OK:
                    //Everything is fine.
                    break;
                default:
                    throw new Exception("FMOD error: " + Error.String(result));
            }
        }

        #endregion

        #region Sound Effect System

        /// <summary>
        /// Loads and immediately plays any audio file given, as a stream. Each sound effect is
        /// given its own channel - the channel allocated to the provided audio file is the return
        /// value. Because it is loaded as a stream, this occurs quickly.
        /// </summary>
        /// <param name="soundPath">The path and filename to the audio file to play.</param>
        /// <param name="loop">Whether the audio file should loop. Set to false for the audio file to play only once.</param>
        /// <param name="dontStream">Whether the audio file should be loaded fully into memory before being played
        /// (recommended for song audio), but will cause delays.</param>
        /// <param name="loadPaused">Whether the audio file should be loaded but not played immediately. Useful for
        /// 'caching' audio large audio files.</param>
        /// <returns>The channel ID allocated by Fmod to the channel. Use this to control the playback.</returns>
        public int PlaySoundEffect(string soundPath, bool loop, bool dontStream, bool loadPaused)
        {
            if (!File.Exists(soundPath))
            {
                return PlayFallbackSoundEffect();
            }

            var mySound = GetOrCreateSound(soundPath, !dontStream);
            var myChannel = new Channel();

            var mode = (uint)MODE.SOFTWARE;

            if (loop)
            {
                mode += (uint)MODE.LOOP_NORMAL;

            }
            if (!dontStream)
            {
                mode += (uint)MODE.CREATESTREAM;

            }

            CheckFMODErrors(mySound.setMode((MODE)mode));
            CheckFMODErrors(_fmodSystem.playSound(CHANNELINDEX.FREE, mySound, loadPaused, ref myChannel));
            CheckFMODErrors(myChannel.setVolume(_masterVolume));
            int index = -1;
            CheckFMODErrors(myChannel.getIndex(ref index));

            return index;
        }

        /// <summary>
        /// Loads and immediately plays any audio file given, as a stream. Each sound effect is
        /// given its own channel - the channel allocated to the provided audio file is the return
        /// value. Because it is loaded as a stream, this occurs quickly.
        /// </summary>
        /// <param name="soundPath">The path and filename to the audio file to play.</param>
        /// <param name="loop">Whether the audio file should loop. Set to false for the audio file to play only once.</param>
        /// <param name="dontStream">Whether the audio file should be loaded fully into memory before being played
        /// (recommended for song audio), but will cause delays.</param>
        /// <returns>The channel ID allocated by Fmod to the channel. Use this to control the playback.</returns>
        public int PlaySoundEffect(string soundPath, bool loop, bool dontStream)
        {
            return PlaySoundEffect(soundPath, loop, dontStream, false);
        }


        public int PlayFallbackSoundEffect()
        {
            if (!File.Exists(FallbackSound))
            {
                return -1;
            }
            const MODE MODE = (MODE.SOFTWARE + (uint)MODE.LOOP_NORMAL + (uint)MODE.CREATESTREAM);

            var mySound = new Sound();
            CheckFMODErrors(_fmodSystem.createSound(FallbackSound, MODE, ref mySound));
            CheckFMODErrors(mySound.setMode(MODE));

            var myChannel = new Channel();

            CheckFMODErrors(_fmodSystem.playSound(CHANNELINDEX.FREE, mySound, false, ref myChannel));
            CheckFMODErrors(myChannel.setVolume(_masterVolume));
            
            int index = -1;
            CheckFMODErrors(myChannel.getIndex(ref index));
            return index;
        }

        public Dictionary<string,string> GetAudioFileMetadata(string filename)
        {
            var result = new Dictionary<string, string>();
            var mySound = new Sound();
            CheckFMODErrors(_fmodSystem.createStream(filename, MODE.SOFTWARE, ref mySound));

            
            int tagsCount = 0, tagsUpdated = 0;
            CheckFMODErrors(mySound.getNumTags(ref tagsCount, ref tagsUpdated));
            var tagResult = new TAG();

            for (int x = 0; x < tagsCount; x++)
            {
                CheckFMODErrors(mySound.getTag(null,x,ref tagResult));
                string data = "";
                switch (tagResult.datatype)
                {
                    case TAGDATATYPE.STRING:
                        data = Marshal.PtrToStringAnsi(tagResult.data);
                        break;
                    case TAGDATATYPE.STRING_UTF16:
                        data = Marshal.PtrToStringUni(tagResult.data);
                        //Strip out non-ASCII chars.
                        data = new string((from e in data where  e < 129 select e).ToArray());
                        break;

                }

                result[tagResult.name.ToUpper()] =  data;
            }

            uint length = 0;
            CheckFMODErrors(mySound.getLength(ref length, TIMEUNIT.MS));
            result["LENGTH"] = length.ToString();
            CheckFMODErrors(mySound.release());

            return result;

        }
        /// <summary>
        /// Loads and immediately plays any audio file given, as a stream. Each sound effect is
        /// given its own channel - the channel allocated to the provided audio file is the return
        /// value. Because it is loaded as a stream, this occurs quickly.
        /// </summary>
        /// <param name="path">The path and filename to the audio file to play.</param>
        /// <returns>The channel ID allocated by Fmod to the channel. Use this to control the playback.</returns>
        public int PlaySoundEffect(string path)
        {
            return PlaySoundEffect(path, false,false);
        }
        /// <summary>
        /// Sets the position of the sound channel given, to the position given (in milliseconds).
        /// Will throw an ArgumentException if an invalid channel ID is provided.
        /// </summary>
        /// <param name="index">The Channel ID to adjust the position.</param>
        /// <param name="position">The position, in milliseconds, to set the channel to.</param>
        public void SetPosition(int index, double position)
        {
            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));
            CheckFMODErrors(_tmpChannel.setPosition((uint)(position), TIMEUNIT.MS));
        }

        /// <summary>
        /// Iterates through the channel dictionary, to find a free ID number.
        /// A channel is considered free if its sound has stopped playing. Otherwise, a new one
        /// is created.
        /// </summary>
        /// <returns>The ID of the first free channel found.</returns>
        private Sound GetOrCreateSound(string soundPath, bool stream)
        {
            if (_sounds.ContainsKey(soundPath))
            {
                _sounds.Remove(soundPath);
            }

                var mySound = new Sound();
                var mode = (uint) MODE.SOFTWARE;
                if (stream)
                {
                    mode += (uint) MODE.CREATESTREAM;
                }

                mode += (uint)MODE.ACCURATETIME;
                CheckFMODErrors(_fmodSystem.createSound(soundPath, (MODE) mode, ref mySound));
                _sounds.Add(soundPath, mySound);
                return mySound;

        }

        /// <summary>
        /// Releases a loaded audio file from FMOD's control. A previously loaded sound file cannot be deleted until released in this way.
        /// </summary>
        /// <param name="soundPath">The path and filename of the audio file to release.</param>
        public void ReleaseSound(string soundPath)
        {
            var sound = _sounds[soundPath];

            if (sound != null)
            {
                CheckFMODErrors(sound.release());
            }
        }
        /// <summary>
        /// Stops playback of the a channel (and hence its audio), given its channel ID.
        /// </summary>
        /// <param name="index">The ID of the channel to stop.</param>
        public void StopChannel(int index)
        {

            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));

            Monitor.Enter(_tmpChannel);
            bool isPlaying = false;
            
            CheckFMODErrors(_tmpChannel.isPlaying(ref isPlaying));
            var tmpSound = new Sound();
            CheckFMODErrors(_tmpChannel.getCurrentSound(ref tmpSound));

            if (isPlaying)
            {

                CheckFMODErrors(_tmpChannel.stop());
            }

            CheckFMODErrors(tmpSound.release());

            Monitor.Exit(_tmpChannel);
        }

        /// <summary>
        /// Returns the volume of a specific channel. Volume should be between 0.0 (mute) and 1.0 (maximum).
        /// Channel volume is also affected by the master volume.
        /// </summary>
        /// <param name="index">The ID of the channel to retrieve the volume for.</param>
        /// <returns>The current volume of the channel, between 0.0 and 1.0.</returns>
        public float GetChannelVolume(int index)
        {
            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));

            float volume = 0.0f;
            CheckFMODErrors(_tmpChannel.getVolume(ref volume));
            return volume;
        }

        /// <summary>
        /// Adjusts the volume of a specific channel (and hence the sound playing on it).
        /// Note that the actual volume is also affected (attenuated) by the master volume.
        /// </summary>
        /// <param name="index">The ID of the channel to adjust the channel on.</param>
        /// <param name="volume">The volume to set this channel to. 0 to mute, 1.0 for maximum volume.</param>
        public void SetChannelVolume(int index, float volume)
        {
            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));
            CheckFMODErrors(_tmpChannel.setVolume(_masterVolume * volume));
        }

        /// <summary>
        /// Adjusts the master volume of the game. All audio, including GameSongs and sound effects
        /// are affected by this volume.
        /// </summary>
        /// <param name="volume">The new master volume to use. 0 to mute, 1.0 for maximum volume.</param>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = volume;

            var masterGroup = new ChannelGroup();
            CheckFMODErrors(_fmodSystem.getMasterChannelGroup(ref masterGroup));
            CheckFMODErrors(masterGroup.setVolume(_masterVolume));
        }


        /// <summary>
        /// Gets the position of the currently playing audio file in a given channel, in milliseconds. Used for syncronization.
        /// </summary>
        /// <param name="index">The ID of the desired channel.</param>
        /// <returns>The current song position, in milliseconds.</returns>
        public uint GetChannelPosition(int index)
        {
            uint position = 0;
            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));
            CheckFMODErrors(_tmpChannel.getPosition(ref position, TIMEUNIT.MS));
            return position;
        }

        public bool IsChannelPlaying(int index)
        {
            bool playing = false;
            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));
            CheckFMODErrors(_tmpChannel.isPlaying(ref playing));
            return playing;
        }

        public void SetChannelPause(int index, bool paused)
        {
            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));
            CheckFMODErrors(_tmpChannel.setPaused(paused));
        }

        /// <summary>
        /// Gets the length of the sound playing in a given Channel ID.
        /// </summary>
        /// <param name="index">The ID of the desired channel.</param>
        /// <returns>The length of the sound playing in the given Channel ID, in milliseconds.</returns>
        public uint GetChannelLength(int index)
        {
            uint length = 0;
            var tmpSound = new Sound();
            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));
            CheckFMODErrors(_tmpChannel.getCurrentSound(ref tmpSound));
            if (tmpSound != null)
            {
                CheckFMODErrors(tmpSound.getLength(ref length, TIMEUNIT.MS));
            }
            return length;
        }
        #endregion

        /// <summary>
        /// Calculates and returns a sound spectrum of the a given Channel ID, based on the sound currently
        /// playing in that channel.
        /// </summary>
        /// <param name="index">The ID of the desired channel.</param>
        /// <param name="numPoints">The number of points of the spectrum to return. Must be a power of 2. Higher numbers will take
        /// more memory and have more lag, but will be more accurate.</param>
        /// <returns>The sound spectrum of the currently playing sound in the given Channel ID, as an array of decimal numbers.</returns>
        public float[] GetChannelSpectrum(int index, int numPoints)
        {
            var returnData = new float[numPoints];

            CheckFMODErrors(_fmodSystem.getChannel(index, ref _tmpChannel));
            CheckFMODErrors(_tmpChannel.getSpectrum(returnData, numPoints, 0, DSP_FFT_WINDOW.RECT));

            return returnData;
        }

        public double GetFileLength(string filePath)
        {
            Sound tmpSound = new Sound();
            CheckFMODErrors(_fmodSystem.createSound(filePath, MODE.SOFTWARE + (uint) MODE.CREATESTREAM, ref tmpSound));
            uint result = 0;
            CheckFMODErrors(tmpSound.getLength(ref result,TIMEUNIT.MS));
            CheckFMODErrors(tmpSound.release());
            return result;

        }



    }
}