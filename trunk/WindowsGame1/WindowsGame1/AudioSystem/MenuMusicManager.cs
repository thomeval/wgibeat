﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WGiBeat.Managers;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// A Manager that is responsible for playing music during the different screens of the game.
    /// The music to be played during each screen is determined by the contents of the MusicFilePath
    /// field. The manager also uses the Crossfader to facilitate crossfading when the menu music is
    /// scheduled to change.
    /// </summary>
    public class MenuMusicManager : Manager
    {
        private readonly Dictionary<string, string> _musicList;
        private int _musicChannel = -1;
        private string _currentMusic;

        public AudioManager AudioManager { get; set; }
        public CrossfaderManager Crossfader { get; set; }
        public string MusicFilePath { get; set; }

        public MenuMusicManager(LogManager log)
        {
            _musicList = new Dictionary<string, string>();
            log.AddMessage("INFO: Initializing Menu Music Manager...");
            Log = log;
        }

        /// <summary>
        /// Loads the list of Menu music files from the given filepath. This file must begin
        /// with this header: #MUSICLIST-1.0; See MusicList.txt included for an example of the
        /// file's format.
        /// </summary>
        /// <param name="filepath">The path of the file to load as a menu music list.</param>
        public void LoadMusicList(string filepath)
        {
            Log.AddMessage("INFO: Loading Menu Music list from "+filepath+" ...");
            var sr = File.ReadAllText(filepath);
            sr = sr.Replace("\n", "");
            sr = sr.Replace("\r", "");
            var lines = sr.Split(';');
            if (lines[0] != "#MUSICLIST-1.0")
            {
                Log.AddMessage("ERROR: Menu Music list does not have the correct header. This file must start with '#MUSICLIST-1.0;'");
                return;
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
            Log.AddMessage("INFO: Menu music list loaded successfully.");
        }

        /// <summary>
        /// Adds a single audio file as a menu music item, with the given screen name and file path.
        /// </summary>
        /// <param name="name">The name of the screen during which the music should play.</param>
        /// <param name="path">The path and filename of the audio file to play as menu music.</param>
        public void AddMenuMusic(string name, string path)
        {
            _musicList.Add(name, path);
        }

        /// <summary>
        /// Changes the currently playing menu music to the music assigned to the given (screen) name.
        /// </summary>
        /// <param name="name">The (screen) name to use as the new playing menu music.</param>
        public void ChangeMusic(string name)
        {
            if (name == "SongSelect")
            {
                //The Song Select screen takes over the Crossfader at this point for song previews.
                _currentMusic = "";
                _musicChannel = -1;
                return;
            }

            if (_musicList.ContainsKey(name))
            {
                if ((!Directory.Exists(MusicFilePath)) || (!File.Exists(MusicFilePath + _musicList[name])))
                {
                    Log.AddMessage("WARN: MenuMusicManager: Skipping load of '" + name + "' as the file was not found.");
                    Log.AddMessage("WARN: Searched in: " + MusicFilePath + _musicList[name]);
                    StopExistingMusic();
                    return;
                }
                if (_currentMusic == _musicList[name])
                {
                    Log.AddMessage("INFO: MenuMusicManager: Selected music is already playing. Skipping load.");
                    return;
                }

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

        /// <summary>
        /// Stops the currently playing menu music.
        /// </summary>
        private void StopExistingMusic()
        {

            _musicChannel = -1;
            _currentMusic = "";
            if (Crossfader.ChannelIndexCurrent != -1)
            {
                Crossfader.SetNewChannel(_musicChannel);
            }
        }
    }
}