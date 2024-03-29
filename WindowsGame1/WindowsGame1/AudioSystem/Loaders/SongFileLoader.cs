﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
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

            if (song.ReadOnly)
            {
                return;
            }

            song.DefinitionFile = Path.GetFileNameWithoutExtension(song.DefinitionFile) + ".sng";

            var file = new FileStream(song.Path + "\\" + song.DefinitionFile, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(file);

            sw.WriteLine("#SONG-1.1;");
            sw.WriteLine("Title={0};", song.Title);
            sw.WriteLine("Subtitle={0};", song.Subtitle);
            sw.WriteLine("Artist={0};", song.Artist);

            sw.WriteLine(SaveSongBPMs(song));
            sw.WriteLine("Offset={0};", Math.Round(song.Offset, 3).ToString(CultureInfo.InvariantCulture));
            sw.WriteLine("AudioStart={0};", Math.Round(song.AudioStart, 3).ToString(CultureInfo.InvariantCulture));
            sw.WriteLine("Length={0};", Math.Round(song.Length, 3).ToString(CultureInfo.InvariantCulture));
            sw.WriteLine("AudioFile={0};", song.AudioFile);
            sw.WriteLine("AudioFileMD5={0};", song.AudioFileMD5);

            if (song.RequiredLevel > 1)
            {
                sw.WriteLine("RequiredLevel={0};",song.RequiredLevel);
            }

            sw.WriteLine(SaveSongStops(song));
            sw.WriteLine(SaveSongBackground(song));
            sw.WriteLine(SaveSongNotes(song));
            sw.Close();
            
            Log.AddMessage(String.Format("Song file saved successfully: {0}\\{1}",song.Path,song.DefinitionFile),LogLevel.INFO);
        }

        private string SaveSongNotes(GameSong song)
        {
            var result = "";
            if (song.AddNotes.Count > 0)
            {
                result += "AddNotes=";
                result = song.AddNotes.Aggregate(result, (current, note) => current + string.Format("{0:F3},", note));
                result = result.TrimEnd(',');
                result += ";\n";
            }

            if (song.RemoveNotes.Count > 0)
            {
                result += "RemoveNotes=";
                result = song.RemoveNotes.Aggregate(result, (current, note) => current + string.Format("{0:F3},", note));
                result = result.TrimEnd(',');
                result += ";\n";
            }
            if (song.SuperNotes.Count > 0)
            {
                result += "SuperNotes=";
                result = song.SuperNotes.Aggregate(result, (current, note) => current + string.Format("{0:F3},", note));
                result = result.TrimEnd(',');
                result += ";\n";
            }
            return result;
        }

        private string SaveSongBackground(GameSong song)
        {
            var result = "Background=";
            if (String.IsNullOrEmpty(song.BackgroundFile))
            {
                return "";
            }
            
            result += song.BackgroundFile + ";";
            return result;
        }

        private string SaveSongBPMs(GameSong song)
        {
            var result = "Bpm=";

            foreach (double key in song.BPMs.Keys)
            {
                result += Math.Round(key, 3).ToString(CultureInfo.InvariantCulture);
                result += ":" + Math.Round(song.BPMs[key], 3).ToString(CultureInfo.InvariantCulture);
                result += ",";
            }
            result = result.TrimEnd(',');
            result += ";";
            return result;
        }

        private string SaveSongStops(GameSong song)
        {
            if (song.Stops.Keys.Count <= 0)
            {
                return "";
            }
            var result = "Stops=";

            foreach (double key in song.Stops.Keys)
            {
                result += Math.Round(key, 3).ToString(CultureInfo.InvariantCulture); 
                result += ":" + Math.Round(song.Stops[key], 3).ToString(CultureInfo.InvariantCulture); 
                result += ",";
            }
            result = result.TrimEnd(',');
            result += ";";
            return result;
        }
    }
}