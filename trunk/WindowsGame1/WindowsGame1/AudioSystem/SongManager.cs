using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WGiBeat.AudioSystem.Loaders;
using WGiBeat.Managers;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// A manager that specifically handles parsing, saving, storage and validation of Song Files (.sng files). 
    /// These files are loaded and a collection of loaded songs is managed here. Game songs can then be played 
    /// by using the PlaySong() method. The actual audio component is handled by the AudioManager.
    /// </summary>
    public class SongManager : Manager
    {

        #region Fields

        private readonly List<GameSong> _songs = new List<GameSong>();
        private int _songChannelIndex = -1;
        public AudioManager AudioManager { get; set; }
        public SettingsManager SettingsManager { get; set; }
        public readonly Dictionary<string, SongFileLoader> Loaders = new Dictionary<string, SongFileLoader>();

        /// <summary>
        /// Returns all songs stored in the Manager.
        /// </summary>
        /// <returns>A list of all stored GameSongs.</returns>
        public List<GameSong> Songs
        {
            get { return _songs; }
        }

        #endregion

        #region Initialization
        public SongManager(LogManager log, AudioManager audioManager, SettingsManager settings)
        {
            Log = log;
            AudioManager = audioManager;
            SettingsManager = settings;
            Log.AddMessage("Initializing Song Manager...", LogLevel.INFO);
            CreateLoaders();
        }

        private void CreateLoaders()
        {
            Loaders.Add("*.sng",new SNGFileLoader {Log = this.Log, Pattern = "*.sng", ConvertToSNG = true});
            Loaders.Add("*.sm",
                        new SMFileLoader
                            {
                                Log = this.Log,
                                Pattern = "*.sm",
                                OffsetAdjust = SettingsManager.Get<double>("LoaderOffsetAdjustment"),
                                AllowProblematic = SettingsManager.Get<bool>("AllowProblematicSongs"),
                                ConvertToSNG = SettingsManager.Get<bool>("ConvertToSNG")
                            });
            Loaders.Add("*.dwi",
                        new DWIFileLoader
                            {
                                Log = this.Log,
                                Pattern = "*.dwi",
                                OffsetAdjust = SettingsManager.Get<double>("LoaderOffsetAdjustment"),
                                AllowProblematic = SettingsManager.Get<bool>("AllowProblematicSongs"),
                                ConvertToSNG = SettingsManager.Get<bool>("ConvertToSNG")
                            });
        }

        #endregion

        #region Song Operations

        /// <summary>
        /// Begins playback of a specific GameSong. The song's audio is loaded into memory, which will
        /// cause noticable loading time but improve performance during gameplay.
        /// </summary>
        /// <param name="song">The GameSong to begin playback.</param>
        /// <returns>The Channel ID used by the AudioManager for playback of the song's audio.</returns>
        public int PlaySong(GameSong song)
        {
            if (_songChannelIndex != -1)
            {
                StopCurrentSong();
            }
            _songChannelIndex = AudioManager.PlaySoundEffect(song.Path + "\\" + song.AudioFile, false, true);
            //TODO: Fade in if appropriate.
            AudioManager.SetPosition(_songChannelIndex, Math.Max(0.0, 1000 * (song.AudioStart - 0.5)));
            return _songChannelIndex;
        }

        /// <summary>
        /// Stops playback of the currently playing GameSong, instantly.
        /// </summary>
        public void StopCurrentSong()
        {
            if (_songChannelIndex != -1)
            {
                AudioManager.StopChannel(_songChannelIndex);
            }
            _songChannelIndex = -1;
        }

        /// <summary>
        /// Returns the current progress of the currently playing GameSong's audio.
        /// </summary>
        /// <returns>The current position of the currently playing GameSong's audio, in milliseconds.</returns>
        public uint GetCurrentSongProgress()
        {
            if (_songChannelIndex != -1)
            {
                return AudioManager.GetChannelPosition(_songChannelIndex);
            }
            return 0;
        }

        public bool IsCurrentSongPlaying()
        {
            if (_songChannelIndex == -1)
            {
                return false;
            }
            return AudioManager.IsChannelPlaying(_songChannelIndex);
        }

        /// <summary>
        /// Automatically splits a song's title into a title and subtitle, by checking if any
        /// brackets are used. A song with a title "Example (Awesome Mix)" would have its title
        /// changed to "Example" with "(Awesome Mix)" moved to the Subtitle field. This method
        /// has no effect on very long titles with no brackets.
        /// </summary>
        /// <param name="song">The game that needs to have its title split.</param>
        public static void SplitTitle(GameSong song)
        {         
            var title = song.Title;
            title = title.Replace("[", "(");
            title = title.Replace("]", ")");

            if (title.IndexOf("(") < title.IndexOf(")"))
            {
                var length = title.LastIndexOf(")") - title.IndexOf("(") + 1;
                song.Subtitle = title.Substring(title.IndexOf("("), length);
                song.Title = title.Substring(0, title.IndexOf("("));
            }
        }
        #endregion

        #region File Operations
        /// <summary>
        /// Adds a GameSong to the list of songs available. GameSongs are checked for uniqueness by their Hashcode.
        /// </summary>
        /// <param name="song">The GameSong to add.</param>
        public void AddSong(GameSong song)
        {
            if (!_songs.Contains(song))
            {
                _songs.Add(song);
            }
            else
            {
                Log.AddMessage("Cannot load songfile because another copy is already loaded: " + song.Path + "\\" + song.DefinitionFile, LogLevel.WARN);
            }
        }

        /// <summary>
        /// Removes a GameSong from the list of playable songs. The given GameSong's Hashcode is used to find which 
        /// song should be removed.
        /// </summary>
        /// <param name="song">The GameSong to remove.</param>
        public void RemoveSong(GameSong song)
        {
            if (_songs.Contains(song))
            {
                _songs.Remove(song);
            }
            else
            {
                throw new Exception("SongManager does not contain song with hashcode: " + song.GetHashCode());
            }
        }

        /// <summary>
        /// Returns that GameSong loaded into the list of songs that has the same physical location and definition file (.sng) 
        /// as the given path and filename.
        /// </summary>
        /// <param name="path">The path of the definition file.</param>
        /// <param name="songfilename">The name of the definition (.sng) file.</param>
        /// <returns>The GameSong loaded into the list of songs that has the same physical location and definition file as the given
        /// path and filename, or null if no match is found.</returns>
        public GameSong GetBySongFile(string path, string songfilename)
        {
            return (from e in _songs where (e.Path == path) && (e.AudioFile == songfilename) select e).SingleOrDefault();
        }

        /// <summary>
        /// Populates this SongManager's song database by parsing all valid song files 
        /// (file types for which it has a loader) from a folder. Subfolders are also parsed.
        /// </summary>
        /// <param name="path">The path containing song files to parse.</param>
        public void LoadFromFolder(params string[] path)
        {
            var folders = new List<string>();
            folders.AddRange(path);

            while (folders.Count > 0)
            {
                string currentFolder = folders[0];
                Log.AddMessage("Loading songfiles in " + currentFolder, LogLevel.INFO);
                folders.InsertRange(0,Directory.GetDirectories(currentFolder));
                var selectedPattern = "";

                //Determine which Loader to use. WGiBeat only loads one type of file from
                //a folder to prevent duplicates. The .sng format is its first choice. 
                foreach (SongFileLoader sfl in Loaders.Values)
                {
                    if (Directory.GetFiles(currentFolder,sfl.Pattern).Count() != 0)
                    {
                        selectedPattern = sfl.Pattern;
                        break;
                    }
                }

                //Load whatever it found. Skip the folder otherwise.
                if (!String.IsNullOrEmpty(selectedPattern))
                {
                    LoadFromFolder(currentFolder, selectedPattern);
                }
                folders.Remove(currentFolder);
            }

            if (_songs.Count == 0)
            {
                Log.AddMessage(String.Format("No valid song files loaded. WGiBeat is not playable without one!"), LogLevel.ERROR);
            }
            Log.AddMessage(String.Format("Finished loading from song folders. {0} songs total.", _songs.Count), LogLevel.NOTE);
        }

        private void LoadFromFolder(string path, string pattern)
        {
            foreach (string file in Directory.GetFiles(path, pattern))
            {
                var newSong = LoadFromFile(file, true);
                if (newSong == null)
                {
                    continue;
                }

                AddSong(newSong);
            }
        }

        /// <summary>
        /// Saves a GameSong object to a file. Note that the filename is taken from the GameSong
        /// object itself. 
        /// </summary>
        /// <param name="song">The GameSong to save to file.</param>
        public void SaveToFile(GameSong song)
        {
            Loaders["*.sng"].SaveToFile(song);
        }

        /// <summary>
        /// Loads a single GameSong from a file. The file should be a file that WGiBeat can recognize, and conform
        /// to the appropriate song file standard. See the readme.txt in the /Songs folder for more
        /// details. WGiBeat's standard for song files are '.sng' files.
        /// </summary>
        /// <param name="filename">The name of the file to load.</param>
        /// <param name="validate">Whether validation checks should be done on the loaded song file.</param>
        /// <returns>A GameSong loaded from the file provided.</returns>
        public GameSong LoadFromFile(string filename, bool validate)
        {
            var pattern = "*" + Path.GetExtension(filename);
            bool isValid;
            var newSong = Loaders[pattern].LoadFromFile(filename, out isValid);

            if (!isValid)
            {
                return null;
            }
                if (validate)
                {
                    string message;
                    bool validated = ValidateSongFile(newSong, out message);

                    //Even if the song file is validated, there may be important information.
                    //Log a warning message if validation failed, or a note if validation succeeded with a
                    //message.
                    if (validated)
                    {
                        if (message != "No errors found.")
                        {
                            Log.AddMessage("" + message + " In: " + newSong.Path + "\\" + newSong.DefinitionFile, LogLevel.NOTE);
                        }

                    }
                    else
                    {
                        Log.AddMessage("" + message + " In: " + newSong.Path + "\\" + newSong.DefinitionFile, LogLevel.WARN);
                        return null;
                    }
                }
                Log.AddMessage("Loaded " + newSong.Title + " successfully.\n", LogLevel.INFO);


            return newSong;
        }

        /// <summary>
        /// Physically deletes a song file. This will also remove it from the list of
        /// loaded GameSongs. The song file is located by the GameSong's DefinitionFile
        /// and Path fields.
        /// </summary>
        /// <param name="song">The GameSong to remove and delete.</param>
        /// <param name="deleteAudio">Whether the GameSong's audio file should be deleted as well.</param>
        /// <remarks>If deleteAudio is set to true and the folder containing the SongFile is empty
        /// after the deletion, that folder will be deleted as well.</remarks>
        /// <returns>"" if deletion was successful, or the error message encountered if something
        /// went wrong.</returns>
        public string DeleteSongFile(GameSong song, bool deleteAudio)
        {
            try
            {
                var existingSongFile = GetBySongFile(song.Path, song.AudioFile);
                if (existingSongFile != null)
                {
                    RemoveSong(existingSongFile);
                }
                File.Delete(song.Path + "\\" + song.DefinitionFile);
                if ((deleteAudio) && File.Exists(song.Path + "\\" + song.AudioFile))
                {
                    AudioManager.ReleaseSound(song.Path + "\\" + song.AudioFile);
                    File.Delete(song.Path + "\\" + song.AudioFile);

                    if (Directory.GetFiles(song.Path).Length == 0)
                    {
                        Directory.Delete(song.Path);
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /// <summary>
        /// Performs a variety of validation checks on a given GameSong, to determine whether it is valid and
        /// playable. The physical location of the .sng file is determined from the provided GameSong's 
        /// Path and DefinitionFile properties.
        /// </summary>
        /// <param name="song">The GameSong to check for validity.</param>
        /// <param name="message">A message explaining why the song is invalid (output parameter)</param>
        /// <returns>Whether the GameSong provided is valid and playable.</returns>
        public bool ValidateSongFile(GameSong song, out string message)
        {
            message = "No errors found.";
            if (string.IsNullOrEmpty(song.AudioFile))
            {
                message = "No audio file specified.";
                return false;
            }
            var path = song.Path + "\\" + song.AudioFile;
            if (!File.Exists(path))
            {
                message = "Couldn't find audio file specified." + path;
                return false;
            }
            if (String.IsNullOrEmpty(song.Title))
            {
                message = "Song does not have a title.";
                return false;
            }
            if (String.IsNullOrEmpty(song.Artist))
            {
                message = "Song does not have an artist.";
            }
            if (song.Offset < 0)
            {
                message = "Offset position is invalid.";
                return false;
            }
            if ((song.Length <= 0) || (song.Length <= song.Offset))
            {
                message = "Length is not specified or invalid.";
                return false;
            }
            if (song.Bpm <= 0)
            {
                message = "BPM is not specified or invalid.";
                return false;
            }

            if ((song.AudioStart < 0.0) || (song.AudioStart > song.Offset))
            {
                message = "Audio start position is invalid. Must be earlier than offset.";
            }

            switch (SettingsManager.Get<int>("SongMD5Behaviour"))
            {
                //WARN ONLY
                case 1:
                    if (!(song.VerifyMD5()))
                    {
                        message =
                            "Audio File MD5 checksum is invalid. This most likely means that the wrong audio file is being used.";
                    }
                    break;
                //WARN AND EXCLUDE
                case 2:
                    if (!(song.VerifyMD5()))
                    {
                        message =
                            "Audio File MD5 checksum is invalid and was not loaded. This most likely means that the wrong audio file is being used.";
                        return false;
                    }
                    break;
                //AUTO CORRECT
                case 3:
                    if (!(song.VerifyMD5()))
                    {
                        song.SetMD5();
                        SaveToFile(song);
                        message =
                            "Audio File MD5 checksum is invalid and has been overridden, since 'Song Audio Validation' has been set to 'auto correct'.";
                    }
                    break;
            }
            return true;
        }

        public bool ValidateSongFile(GameSong song)
        {
            string dummy;
            return ValidateSongFile(song, out dummy);
        }
        #endregion

    }
}
