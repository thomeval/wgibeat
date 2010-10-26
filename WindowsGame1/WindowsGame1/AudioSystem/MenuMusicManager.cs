using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WGiBeat.AudioSystem
{

    public class MenuMusicManager
    {
        private readonly Dictionary<string, string> _musicList;
        private int _musicChannel = -1;
        private string _currentMusic;

        public AudioManager AudioManager { get; set; }
        public CrossfaderManager Crossfader { get; set; }
        public string MusicFilePath { get; set; }
        public MenuMusicManager()
        {
            _musicList = new Dictionary<string, string>();
        }

        public void LoadMusicList(string filepath)
        {
            var sr = File.ReadAllText(filepath);
            sr = sr.Replace("\r\n", "");
            var lines = sr.Split(';');
            if (lines[0] != "#MUSICLIST-1.0")
            {
                throw new Exception("Music list does not have the correct header. This file must start with '#MUSICLIST-1.0;'");
            }


            for (int x = 1; x < lines.Count(); x++)
            {
                if (((lines[x].Length == 0) || lines[x][0] == '#') || (lines[x].IndexOf('=') == -1))
                {
                    continue;
                }
                try
                {
                    var name = lines[x].Substring(0, lines[x].IndexOf('='));
                    var path = lines[x].Substring(lines[x].IndexOf('=') + 1);
                    AddMenuMusic(name, path);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception parsing menu list file: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine("Tried parsing: " + lines[x]);
                }

            }
        }
        public void AddMenuMusic(string name, string path)
        {
            _musicList.Add(name, path);
        }

        public void ChangeMusic(string name)
        {

            if (_musicList.ContainsKey(name))
            {
                if ((!Directory.Exists(MusicFilePath)) || (!File.Exists(MusicFilePath + _musicList[name])))
                {
                    System.Diagnostics.Debug.WriteLine("MenuMusicManager: Skipping load of '" + name + "' as the file was not found.");
                    System.Diagnostics.Debug.WriteLine("Searched in: " + MusicFilePath + _musicList[name]);
                    StopExistingMusic();
                    return;
                }
                if (_currentMusic == _musicList[name])
                {
                    System.Diagnostics.Debug.WriteLine("MenuMusicManager: Selected music is already playing. Skipping load.");
                    return;
                }

                //StopExistingMusic();
                _currentMusic = _musicList[name];
                _musicChannel = AudioManager.PlaySoundEffect(MusicFilePath + _musicList[name],true,false);
                Crossfader.PreviewDuration = 0;
                Crossfader.SetNewChannel(_musicChannel);
            }
            else
            {
                StopExistingMusic();
            }
        }

        private void StopExistingMusic()
        {


            //AudioManager.StopChannel(_musicChannel);
            _musicChannel = -1;
            _currentMusic = "";
            if (Crossfader.ChannelIndexCurrent != -1)
            {
                Crossfader.SetNewChannel(_musicChannel);
            }
        }
    }
}
