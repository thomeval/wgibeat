using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private int _songChannelIndex;
        public AudioManager AudioManager { get; set; }
        public SettingsManager SettingsManager { get; set; }
        #endregion

        public SongManager(LogManager log, AudioManager audioManager, SettingsManager settings)
        {
            Log = log;
            AudioManager = audioManager;
            SettingsManager = settings;
            Log.AddMessage("Initializing Song Manager...",LogLevel.INFO);
        }
        /// <summary>
        /// Returns all songs stored in the Manager.
        /// </summary>
        /// <returns>A list of all stored GameSongs.</returns>
        public List<GameSong> Songs
        {
            get { return _songs;}
        }

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
            AudioManager.SetPosition(_songChannelIndex,Math.Max(0.0,1000 * (song.AudioStart - 0.5)));
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
                throw new Exception("SongManager already contains song with hashcode: " + song.GetHashCode());
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
        /// Creates a new SongManager object, and populates its song database by
        /// parsing all valid song files (.sng) from a folder. Subfolders are also parsed.
        /// </summary>
        /// <param name="path">The path containing song files to parse.</param>
        /// <returns>A new SongManager instance, containing GameSongs for any .sng files parsed.</returns>
        public void LoadFromFolder(string path)
        {
            var folders = new List<string>();
            folders.Add(path);

            while (folders.Count > 0)
            {
                string currentFolder = folders[0];
                Log.AddMessage("Loading songfiles in " + currentFolder,LogLevel.INFO);
                folders.AddRange(Directory.GetDirectories(currentFolder));
                foreach (string file in Directory.GetFiles(currentFolder, "*.sng"))
                {
                    var newSong = LoadFromFile(file,true);
                    if (newSong == null)
                    {
                        continue;
                    }
                    
                    AddSong(newSong);
                }
                folders.RemoveAt(0);
            }
            if (_songs.Count == 0)
            {
                Log.AddMessage(String.Format("No valid song files loaded. WGiBeat is not playable without one!"),LogLevel.ERROR);
            }
            Log.AddMessage(String.Format("Song load completed. {0} songs loaded.", _songs.Count),LogLevel.NOTE);
        }

        /// <summary>
        /// Saves a GameSong object to a file. Note that the filename is taken from the GameSong
        /// object itself. Most useful for saving edits made to song file during runtime
        /// (such as Song Debugging).
        /// </summary>
        /// <param name="song">The GameSong to save to file.</param>
        public void SaveToFile(GameSong song)
        {
            var file = new FileStream(song.Path + "\\" + song.DefinitionFile, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(file);         

            sw.WriteLine("#SONG-1.0;");
            sw.WriteLine("Title={0};", song.Title);
            sw.WriteLine("Subtitle={0};", song.Subtitle);
            sw.WriteLine("Artist={0};", song.Artist);
            sw.WriteLine("Bpm={0};", Math.Round(song.Bpm, 2));
            sw.WriteLine("Offset={0};", Math.Round(song.Offset, 3));
            sw.WriteLine("AudioStart={0};", Math.Round(song.AudioStart,3));
            sw.WriteLine("Length={0};", Math.Round(song.Length, 3));
            sw.WriteLine("AudioFile={0};", song.AudioFile);
            sw.WriteLine("AudioFileMD5={0};",song.AudioFileMD5);
            sw.Close();
        }

        /// <summary>
        /// Loads a single GameSong from a file. The file should be a .sng file, and conform
        /// to the song file standard. See the readme.txt in the /Songs folder for more
        /// details.
        /// </summary>
        /// <param name="filename">The name of the file to load.</param>
        /// <param name="validate">Whether validation checks should be done on the loaded song file.</param>
        /// <returns>A GameSong loaded from the file provided.</returns>
        public GameSong LoadFromFile(string filename, bool validate)
        {
            var newSong = GameSong.LoadDefaults();

            newSong.Path = filename.Substring(0,filename.LastIndexOf("\\"));
            newSong.DefinitionFile = Path.GetFileName(filename);
            try
            {
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
                        case "AUDIOSTART":
                            newSong.AudioStart = Convert.ToDouble(value);
                            break;
                        case "LENGTH":
                            newSong.Length = Convert.ToDouble(value);
                            break;
                        case "BPM":
                            newSong.Bpm = Convert.ToDouble(value);
                            break;
                        case "SONGFILE":
                        case "AUDIOFILE":
                            newSong.AudioFile = value;
                            break;
                        case "SONGFILEMD5":
                        case "AUDIOFILEMD5":
                            newSong.AudioFileMD5 = value;
                            break;
                    }
                }
                if (validate)
                {
                    string message;
                    bool validated = ValidateSongFile(newSong, out message);
                    if (validated)
                    {
                        if (message != "No errors found.")
                        {
                            Log.AddMessage("" + message + " In: " + newSong.Path + "\\" + newSong.DefinitionFile,LogLevel.NOTE);
                        }

                    }
                    else
                    {
                        Log.AddMessage("" + message + " In: " + newSong.Path + "\\" + newSong.DefinitionFile,LogLevel.WARN);
                        return null;
                    }
                }
                Log.AddMessage("Loaded " + newSong.Title + " successfully.\n",LogLevel.INFO);
            }
            catch (Exception)
            {
                Log.AddMessage("Failed to load song: " + filename + "\n",LogLevel.WARN);
                return null;
            }

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
