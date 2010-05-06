using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FMOD;

namespace WindowsGame1.AudioSystem
{
    public class SongManager
    {
        private readonly List<GameSong> _songs = new List<GameSong>();

        private FMOD.System _fmodSystem = new FMOD.System();
        private FMOD.Channel _fmodChannel = new FMOD.Channel();
        private FMOD.Sound _fmodSound = new FMOD.Sound();

        public SongManager()
        {
              FMOD.RESULT result;
            result = FMOD.Factory.System_Create(ref _fmodSystem);
            CheckFMODErrors(result);
            result = _fmodSystem.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
            CheckFMODErrors(result);


        }

        public List<GameSong> AllSongs()
        {
            return _songs;
        }
        public void LoadSong(GameSong song)
        {
            StopSong();
            RESULT result;
            result = _fmodSystem.createSound(song.Path + "\\" + song.SongFile, FMOD.MODE.HARDWARE, ref _fmodSound);
            CheckFMODErrors(result);
        }
        public void PlaySong()
        {
            RESULT result;
            result = _fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, _fmodSound, false, ref _fmodChannel);
            CheckFMODErrors(result);
        }

        private void CheckFMODErrors(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
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

        public GameSong GetByTitle(string title)
        {
            return (from e in _songs where e.Title == title select e).FirstOrDefault();
        }
        public GameSong GetByHashCode(int hashCode)
        {
            return (from e in _songs where e.GetHashCode() == hashCode select e).SingleOrDefault();
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
                //Also works if value is blank... for some reason.
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
    }
}