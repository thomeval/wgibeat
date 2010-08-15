using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FMOD;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// A manager that handles all audio in the game. This is generally separated into Songs,
    /// of which only one is allowed at a time, and Sound Effects, of which multiple simultanious
    /// channels are provided. This manager also handles loading and saving of song files.
    /// </summary>
    public class SongManager
    {
        #region Fields
        private readonly List<GameSong> _songs = new List<GameSong>();

        private readonly FMOD.System _fmodSystem = new FMOD.System();
        private Channel _fmodChannel = new Channel();
        private readonly Dictionary<string, Sound> _sounds;
        private Sound _fmodSound = new Sound();
        private GameSong _currentSong;
        private const int CHANNEL_COUNT = 8;
        private float _masterVolume = 1.0f;

        private Channel tmpChannel = new Channel();
        #endregion 

        public SongManager()
        {
            _sounds = new Dictionary<string, Sound>();
              RESULT result;
            result = Factory.System_Create(ref _fmodSystem);
            CheckFMODErrors(result);
            result = _fmodSystem.init(CHANNEL_COUNT, INITFLAGS.NORMAL, (IntPtr)null);
            CheckFMODErrors(result);
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
                case RESULT.ERR_CHANNEL_STOLEN:
                    System.Diagnostics.Debug.WriteLine("Channel steal detected. Ignoring");
                    break;
                case RESULT.ERR_FILE_NOTFOUND:
                    throw new FileNotFoundException("Unable to find GameSong file " + _currentSong.SongFile + " in " + _currentSong.Path);
                default:
                    throw new Exception("FMOD error: " + result);
            }
        }

        /// <summary>
        /// Returns all songs stored in the Manager.
        /// </summary>
        /// <returns>A list of all stored GameSongs.</returns>
        public List<GameSong> AllSongs()
        {
            return _songs;
        }

        /// <summary>
        /// Returns the currently playing song.
        /// </summary>
        /// <returns>The currently playing song.</returns>
        public GameSong CurrentSong()
        {
            return _currentSong;
        }
        #endregion

        #region File Operations
        /// <summary>
        /// Adds a GameSong to the list of songs available.
        /// </summary>
        /// <param name="song">The GameSong to add.</param>
        private void AddSong(GameSong song)
        {
            if (!_songs.Contains(song))
            {
                _songs.Add(song);
            }
            else
            {
                throw new Exception("SongManager already contains song with title: " + song.Title);
            }
        }

        /// <summary>
        /// Creates a new SongManager object, and populates its song database by
        /// parsing all valid song files (.sng) from a folder. Subfolders are also parsed.
        /// </summary>
        /// <param name="path">The path containing song files to parse.</param>
        /// <returns>A new SongManager instance, containing GameSongs for any .sng files parsed.</returns>
        public static SongManager LoadFromFolder(string path)
        {
            var newManager = new SongManager();
            var folders = new List<string>();
            folders.Add(path);

            while (folders.Count > 0)
            {
                string currentFolder = folders[0];

                folders.AddRange(Directory.GetDirectories(currentFolder));
                foreach (string file in Directory.GetFiles(currentFolder, "*.sng"))
                {
                    var newSong = LoadFromFile(file);
                    newSong.Path = currentFolder;
                    newSong.DefinitionFile = Path.GetFileName(file);
                    newManager.AddSong(newSong);

                }
                folders.RemoveAt(0);
            }

            return newManager;
        }

        /// <summary>
        /// Saves a GameSong object to a file. Note that the filename is taken from the GameSong
        /// object itself. Most useful for saving edits made to song file during runtime
        /// (such as Song Debugging).
        /// </summary>
        /// <param name="song">The GameSong to save to file.</param>
        public static void SaveToFile(GameSong song)
        {
            var file = new FileStream(song.Path + "\\" + song.DefinitionFile, FileMode.Create, FileAccess.Write);

            var sw = new StreamWriter(file);

            sw.WriteLine("#SONG-1.0;");
            sw.WriteLine("Title={0};", song.Title);
            sw.WriteLine("Subtitle={0};", song.Subtitle);
            sw.WriteLine("Artist={0};", song.Artist);
            sw.WriteLine("Bpm={0};", Math.Round(song.Bpm, 2));
            sw.WriteLine("Offset={0};", Math.Round(song.Offset, 3));
            sw.WriteLine("Length={0};", Math.Round(song.Length, 3));
            sw.WriteLine("SongFile={0};", song.SongFile);

            sw.Close();
        }

        /// <summary>
        /// Loads a single GameSong from a file. The file should be a .sng file, and conform
        /// to the song file standard. See the readme.txt in the /Songs folder for more
        /// details.
        /// </summary>
        /// <param name="filename">The name of the file to load.</param>
        /// <returns>A GameSong loaded from the file provided.</returns>
        public static GameSong LoadFromFile(string filename)
        {
            var newSong = new GameSong();

            string songText = File.ReadAllText(filename);

            songText = songText.Replace("\r", "");
            songText = songText.Replace("\n", "");

            string[] rules = songText.Split(';');

            foreach (string rule in rules)
            {
                if ((rule.Length == 0) || (rule[0] == '#'))
                {
                    continue;
                }

                string field = rule.Substring(0, rule.IndexOf("=")).ToUpper();
                string value = rule.Substring(rule.IndexOf("=") + 1);

                switch (field.ToUpper())
                {
                    case "TITLE":
                        newSong.Title = value;
                        break;
                    case "SUBTITLE":
                        newSong.Subtitle = value;
                        break;
                    case "ARTIST":
                        newSong.Artist = value;
                        break;
                    case "OFFSET":
                        newSong.Offset = Convert.ToDouble(value);
                        break;
                    case "LENGTH":
                        newSong.Length = Convert.ToDouble(value);
                        break;
                    case "BPM":
                        newSong.Bpm = Convert.ToDouble(value);
                        break;
                    case "SONGFILE":
                        newSong.SongFile = value;
                        break;
                }
            }
            return newSong;
        }
        #endregion

        #region Sound Effect System
        /// <summary>
        /// Loads and immediately plays any audio file given, as a stream. Each sound effect is
        /// given its own channel - the channel allocated to the provided audio file is the return
        /// value. Because it is loaded as a stream, this occurs quickly.
        /// </summary>
        /// <param name="soundPath">The path and filename to the audio file to play.</param>
        /// <returns>The channel ID allocated by Fmod to the channel. Use this to control the playback.</returns>
        public int PlaySoundEffect(string soundPath)
        {
            RESULT result;
            Sound mySound = GetOrCreateSound(soundPath);
            Channel myChannel = new Channel();
            
           // result = _fmodSystem.createStream(soundPath, MODE.SOFTWARE, ref mySound);
           
          //  CheckFMODErrors(result);
            result = _fmodSystem.playSound(CHANNELINDEX.FREE, mySound, false, ref myChannel);
            CheckFMODErrors(result);
            result = myChannel.setVolume(_masterVolume);
            CheckFMODErrors(result);

            int index = -1;
            result = myChannel.getIndex(ref index);
            CheckFMODErrors(result);
            return index;
        }

        /// <summary>
        /// Sets the position of the sound channel given, to the position given (in milliseconds).
        /// Will throw an ArgumentException if an invalid channel ID is provided.
        /// </summary>
        /// <param name="index">The Channel ID to adjust the position.</param>
        /// <param name="position">The position, in milliseconds, to set the channel to.</param>
        public void SetPosition(int index, double position)
        {
            var resultCode = _fmodSystem.getChannel(index, ref tmpChannel);
            CheckFMODErrors(resultCode);


            var result = tmpChannel.setPosition((uint)(position * 1000), TIMEUNIT.MS);
            CheckFMODErrors(result);
        }

        /// <summary>
        /// Iterates through the channel dictionary, to find a free ID number.
        /// A channel is considered free if its sound has stopped playing. Otherwise, a new one
        /// is created.
        /// </summary>
        /// <returns>The ID of the first free channel found.</returns>
        private Sound GetOrCreateSound(string soundPath)
        {
            if (!_sounds.ContainsKey(soundPath))
            {
                var mySound = new Sound();

                var resultCode = _fmodSystem.createStream(soundPath, MODE.SOFTWARE, ref mySound);
                CheckFMODErrors(resultCode);
                return mySound;
            }
            return _sounds[soundPath];
        }

        /// <summary>
        /// Stops playback of the a channel (and hence its audio), given its channel ID.
        /// </summary>
        /// <param name="index">The ID of the channel to stop.</param>
        public void StopChannel(int index)
        {
            var resultCode = _fmodSystem.getChannel(index, ref tmpChannel);
            CheckFMODErrors(resultCode);

            Monitor.Enter(tmpChannel);
            bool isPlaying = false;
            tmpChannel.isPlaying(ref isPlaying);
            CheckFMODErrors(resultCode);

            if (isPlaying)
            {
                resultCode = tmpChannel.stop();
                CheckFMODErrors(resultCode);
            }
            Monitor.Exit(tmpChannel);
        }

        /// <summary>
        /// Returns the volume of a specific channel. Volume should be between 0.0 (mute) and 1.0 (maximum).
        /// Channel volume is also affected by the master volume.
        /// </summary>
        /// <param name="index">The ID of the channel to retrieve the volume for.</param>
        /// <returns>The current volume of the channel, between 0.0 and 1.0.</returns>
        public float GetChannelVolume(int index)
        {
            var resultCode = _fmodSystem.getChannel(index, ref tmpChannel);
            CheckFMODErrors(resultCode);

            float volume = 0.0f;
            resultCode = tmpChannel.getVolume(ref volume);
            CheckFMODErrors(resultCode);
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
            var resultCode = _fmodSystem.getChannel(index, ref tmpChannel);
            CheckFMODErrors(resultCode);

            resultCode = tmpChannel.setVolume(_masterVolume * volume);
            CheckFMODErrors(resultCode);
        }

        /// <summary>
        /// Adjusts the master volume of the game. All audio, including GameSongs and sound effects
        /// are affected by this volume.
        /// </summary>
        /// <param name="volume">The new master volume to use. 0 to mute, 1.0 for maximum volume.</param>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = volume;

            ChannelGroup masterGroup = new ChannelGroup();
            var resultCode = _fmodSystem.getMasterChannelGroup(ref masterGroup);
            CheckFMODErrors(resultCode);

            masterGroup.setVolume(_masterVolume);
        }


        #endregion

        #region Song Playback System

        /// <summary>
        /// Gets the position of the currently playing song, in milliseconds. Used for syncronization.
        /// </summary>
        /// <returns>The current song position, in milliseconds.</returns>
        public uint GetCurrentSongProgress()
        {
            RESULT result;
            uint position = 0;
            result = _fmodChannel.getPosition(ref position, TIMEUNIT.MS);
            CheckFMODErrors(result);
            return position;
        }
 
        /// <summary>
        /// Stops the currently playing GameSong.
        /// </summary>
        public void StopSong()
        {
            bool isPlaying = false;
            _fmodChannel.isPlaying(ref isPlaying);
            if (isPlaying)
            {
                var result = _fmodChannel.stop();
                CheckFMODErrors(result);
            }
        }

        /// <summary>
        /// Loads a GameSong into memory. Only one GameSong is loaded at a time.
        /// Note that this method can take some time to complete.
        /// </summary>
        /// <param name="song"></param>
        public void LoadSong(GameSong song)
        {
            StopSong();
            _currentSong = song;
            RESULT result;
            result = _fmodSystem.createSound(song.Path + "\\" + song.SongFile, MODE.SOFTWARE, ref _fmodSound);
            CheckFMODErrors(result);
        }

        /// <summary>
        /// Starts playing the latest GameSong loaded with LoadSong(), using the master volume.
        /// </summary>
        public void PlaySong()
        {
            RESULT result;
            result = _fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, _fmodSound, false, ref _fmodChannel);
            _fmodChannel.setVolume(_masterVolume);
            CheckFMODErrors(result);
        }
        #endregion

        public float[] GetChannelWaveform(int index)
        {
            var returnData = new float[1024];

            var resultCode = _fmodSystem.getChannel(index, ref tmpChannel);
            CheckFMODErrors(resultCode);

            resultCode = tmpChannel.getSpectrum(returnData, 1024, 0,DSP_FFT_WINDOW.RECT);
            CheckFMODErrors(resultCode);

            return returnData;
        }
    }
}