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
        private readonly Dictionary<int, Channel> _channels;
        private readonly Dictionary<int, Sound> _sounds;
        private Sound _fmodSound = new Sound();
        private GameSong _currentSong;
        private const int CHANNEL_COUNT = 8;
        private float _masterVolume = 1.0f;

        public SongManager()
        {
            _channels = new Dictionary<int, Channel>();
            _sounds = new Dictionary<int, Sound>();
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

        public void SetPosition(int soundChannel, double position)
        {
            var result = _channels[soundChannel].setPosition((uint)(position * 1000), TIMEUNIT.MS);
            CheckFMODErrors(result);
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
            int nextIdx = (_channels.Keys.Count == 0) ? 0 : _channels.Keys.Max() + 1;
            _channels.Add(nextIdx,  new Channel());
            _sounds.Add(nextIdx, new Sound());
            return _channels.Count - 1;
        }

        public void StopChannel(int index)
        {
            if (!_channels.Keys.Contains(index))
            {
                return;
            }
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


        public void SetChannelVolume(int index, float volume)
        {
            RESULT result;
            result = _channels[index].setVolume(_masterVolume * volume);
            CheckFMODErrors(result);
        }
        public void SetMasterVolume(float volume)
        {
            _masterVolume = volume;
        }
    }
}