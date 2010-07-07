using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FMOD;

namespace WGiBeat.AudioSystem
{
    public class SongManager
    {
        private readonly List<GameSong> _songs = new List<GameSong>();

        private readonly FMOD.System _fmodSystem = new FMOD.System();
        private Channel _fmodChannel = new Channel();
        private readonly List<Channel> _channels;
        private readonly List<Sound> _sounds;
        private Sound _fmodSound = new Sound();
        private GameSong _currentSong;
        private const int CHANNEL_COUNT = 8;
        private int _previewChannelIndex = -1;
        private float _masterVolume = 1.0f;

        //TODO: Consider refactoring into a separate manager.
        private Dictionary<int, HighScoreEntry> _highScoreEntries = new Dictionary<int, HighScoreEntry>();

        public SongManager()
        {
            _channels = new List<Channel>();
            _sounds = new List<Sound>();
              RESULT result;
            result = Factory.System_Create(ref _fmodSystem);
            CheckFMODErrors(result);
            result = _fmodSystem.init(CHANNEL_COUNT, INITFLAGS.NORMAL, (IntPtr)null);
            CheckFMODErrors(result);
        }

        public List<GameSong> AllSongs()
        {
            return _songs;
        }
        public void LoadSong(GameSong song)
        {
            StopSong();
            _currentSong = song;
            RESULT result;
            result = _fmodSystem.createSound(song.Path + "\\" + song.SongFile, MODE.SOFTWARE, ref _fmodSound);
            CheckFMODErrors(result);
        }

        public void PlaySong()
        {
            RESULT result;
            result = _fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, _fmodSound, false, ref _fmodChannel);
            _fmodChannel.setVolume(_masterVolume);
            CheckFMODErrors(result);
        }

        public void PlaySongPreview(GameSong song)
        {
            if (_previewChannelIndex != -1)
            {
                StopChannel(_previewChannelIndex);
            }
            _previewChannelIndex = PlaySoundEffect(song.Path + "\\" + song.SongFile);
            var result = _channels[_previewChannelIndex].setPosition((uint)((song.Offset - 0.1) * 1000),TIMEUNIT.MS);
            CheckFMODErrors(result);
        }

        public int PlaySoundEffect(string soundPath)
        {
            RESULT result;
            int freeSlot = GetFreeSlot();
            Sound mySound = _sounds[freeSlot];
            Channel myChannel = _channels[freeSlot];
            result = _fmodSystem.createSound(soundPath, MODE.CREATESTREAM, ref mySound);
           
            CheckFMODErrors(result);
            result = _fmodSystem.playSound(CHANNELINDEX.FREE, mySound, false, ref myChannel);
            CheckFMODErrors(result);
            return freeSlot;
        }
        private int GetFreeSlot()
        {
            bool isPlaying = false;
            for (int x = 0; x < _channels.Count(); x++)
            {
                _channels[x].isPlaying(ref isPlaying);
                if (!isPlaying)
                {
                    return x;
                }
            }
            _channels.Add(new Channel());
            _sounds.Add(new Sound());
            return _channels.Count - 1;
        }

        private void StopChannel(int index)
        {
            bool isPlaying = false;
            _channels[index].isPlaying(ref isPlaying);
            if (isPlaying)
            {
                var result = _channels[index].stop();
                CheckFMODErrors(result);
            }
        }
        public GameSong CurrentSong()
        {
            return _currentSong;
        }
        public uint GetCurrentSongProgress()
        {
            RESULT result;
            uint position = 0;
            result = _fmodChannel.getPosition(ref position, TIMEUNIT.MS);
            CheckFMODErrors(result);
            return position;
        }


        private void CheckFMODErrors(RESULT result)
        {
            switch (result)
            {
                case RESULT.OK:
                    //Everything is fine.
                    break;
                case RESULT.ERR_FILE_NOTFOUND:
                    throw new FileNotFoundException("Unable to find GameSong file " + _currentSong.SongFile + " in " + _currentSong.Path);
                    default:
                    throw new Exception("FMOD error: " + result);
            }
        }
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

        public static void SaveToFile(GameSong song)
        {

            var file = new FileStream(song.Path + "\\" + song.DefinitionFile, FileMode.Create, FileAccess.Write);

            var sw = new StreamWriter(file);

            sw.WriteLine("#SONG-1.0;");
            sw.WriteLine("Title={0};", song.Title);
            sw.WriteLine("Subtitle={0};", song.Subtitle);
            sw.WriteLine("Artist={0};", song.Artist);
            sw.WriteLine("Bpm={0};",Math.Round(song.Bpm,2));
            sw.WriteLine("Offset={0};", Math.Round(song.Offset,3));
            sw.WriteLine("Length={0};", Math.Round(song.Length,3));
            sw.WriteLine("SongFile={0};", song.SongFile);
            
            sw.Close();
        }

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
                        newSong.Offset =  Convert.ToDouble(value);
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

      public HighScoreEntry GetHighScoreEntry(int songHashCode)
      {
         if (!_highScoreEntries.ContainsKey(songHashCode))
         {
             return null;
         }
          return _highScoreEntries[songHashCode]; 
      }

        public void SetHighScoreEntry(int songHashCode, GameType gameType, long score, int grade, Difficulty difficulty)
        {
            if (!_highScoreEntries.ContainsKey(songHashCode))
            {
                _highScoreEntries[songHashCode] = new HighScoreEntry();
            }
            _highScoreEntries[songHashCode].Difficulties[gameType] = difficulty;
            _highScoreEntries[songHashCode].Grades[gameType] = grade;
            _highScoreEntries[songHashCode].Scores[gameType] = score;

        }

        /// <summary>
        /// Determines which player, if any, has earned a high score entry.
        /// </summary>
        /// <param name="players">The players of the current game.</param>
        /// <param name="gameType">The GameType used in the current game.</param>
        /// <param name="grades">The grades awarded to each player.</param>
        /// <returns>-1 if no player beat the stored high score, 4 if all players as a team
        /// beat the high score, or the player index (0 to 3) if a single player beat the high score.</returns>
        public int DetermineHighScore(Player[] players, GameType gameType, int[] grades)
        {
            var entry = GetHighScoreEntry(_currentSong.GetHashCode());
            long highest;

            if ((entry == null) || (!entry.Scores.ContainsKey(gameType)))
            {
                highest = 0;
            }
            else
            {
                highest = entry.Scores[gameType];
            }

            int awardedPlayer = -1;
            switch (gameType)
            {
                case GameType.NORMAL:
                    for (int x = 0; x < 4; x++)
                    {
                        if ((players[x].Playing) && (players[x].Score > highest))
                        {
                            
                            highest = Math.Max(highest, players[x].Score);
                            awardedPlayer = x;
                        }
                    }

                    if (awardedPlayer != -1)
                    {
                        SetHighScoreEntry(_currentSong.GetHashCode(), gameType, highest, grades[awardedPlayer], players[awardedPlayer].PlayDifficulty);
                        SaveHighScores("Scores.conf");
                    }
                    break;
                case GameType.COOPERATIVE:
                    var currentTotal = (from e in players where e.Playing select e.Score).Sum();
                    if (currentTotal > highest)
                    {
                        awardedPlayer = 4;
                        SetHighScoreEntry(_currentSong.GetHashCode(), gameType, currentTotal,grades[0],LowestDifficulty(players));
                        SaveHighScores("Scores.conf");
                    }
                    break;
            }

            return awardedPlayer;
            
        }

        private Difficulty LowestDifficulty(Player[] players)
        {
            return (from e in players where e.Playing select e.PlayDifficulty).Min();
        }

        public void SaveHighScores(string filename)
        {
            var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var bf = new BinaryFormatter();
            bf.Serialize(fs, _highScoreEntries);

            fs.Close();
        }

        public bool LoadHighScores(string filename)
        {
            try
            {
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var bf = new BinaryFormatter();
                _highScoreEntries = (Dictionary<int, HighScoreEntry>)bf.Deserialize(fs);
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading high scores." + ex.Message);
                return false;
            }
        }

        public void StopSongPreview()
        {
            if (_previewChannelIndex != -1)
            {
                StopChannel(_previewChannelIndex);
            }
        }

        public void SetPreviewVolume(float volume)
        {
            RESULT result;
            result = _channels[_previewChannelIndex].setVolume(_masterVolume * volume);
            CheckFMODErrors(result);
        }
        public void SetMasterVolume(float volume)
        {
            _masterVolume = volume;
        }
    }
}