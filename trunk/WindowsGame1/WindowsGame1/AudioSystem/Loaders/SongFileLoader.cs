﻿using System;
using System.IO;
using WGiBeat.Managers;

namespace WGiBeat.AudioSystem.Loaders
{

    /// <summary>
    /// An abstract class designed to handle common aspects of loading song files into WGiBeat. Although
    /// WGiBeat is designed for the .sng file format, subclasses can be made to handle any file type that
    /// provides the necessary information for a song to be playable (offset, bpm, length, title, artist, etc)
    /// </summary>
    public abstract class SongFileLoader
    {
        public LogManager Log { get; set; }
        public string Pattern { get; set; }
        public double OffsetAdjust { get; set; }
        public bool AllowProblematic { get; set; }
        public bool ConvertToSNG { get; set; }
        public string LastLoadError { get; protected set; }

        public abstract GameSong LoadFromFile(string filename, out bool valid);

        protected string FindUndefinedAudioFile(string path, string defFile)
        {
            Log.AddMessage(String.Format("{0} has no defined audio file. Attempting to locate it automatically.",defFile),LogLevel.DEBUG);
            string[] validExtensions = { "*.mp3", "*.ogg", "*.wma" };

            foreach (string ext in validExtensions)
            {
                var possibility = Path.GetFileNameWithoutExtension(defFile) + ext;
                if (File.Exists(path + "\\" + possibility))
                {
                    return Path.GetFileName(possibility);
                }
            }

            foreach (string ext in validExtensions)
            {
                var files = Directory.GetFiles(path, ext);
                if (files.Length > 0)
                {
                    return Path.GetFileName(files[0]);
                }
            }

            Log.AddMessage(String.Format("FindUndefinedAudioFile: Nothing of value has been found."), LogLevel.DEBUG);
            return "";
        }

        public void SaveToFile(GameSong song)
        {
            if (!ConvertToSNG)
            {
                return;
            }
            if (song.ReadOnly)
            {
                return;
            }

            song.DefinitionFile = Path.GetFileNameWithoutExtension(song.DefinitionFile) + ".sng";

            var file = new FileStream(song.Path + "\\" + song.DefinitionFile, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(file);

            sw.WriteLine("#SONG-1.0;");
            sw.WriteLine("Title={0};", song.Title);
            sw.WriteLine("Subtitle={0};", song.Subtitle);
            sw.WriteLine("Artist={0};", song.Artist);
            sw.WriteLine("Bpm={0};", Math.Round(song.Bpm, 2));
            sw.WriteLine("Offset={0};", Math.Round(song.Offset, 3));
            sw.WriteLine("AudioStart={0};", Math.Round(song.AudioStart, 3));
            sw.WriteLine("Length={0};", Math.Round(song.Length, 3));
            sw.WriteLine("AudioFile={0};", song.AudioFile);
            sw.WriteLine("AudioFileMD5={0};", song.AudioFileMD5);
            sw.Close();
            
            Log.AddMessage(String.Format("Song file saved successfully: {0}\\{1}",song.Path,song.DefinitionFile),LogLevel.INFO);
        }
    }
}