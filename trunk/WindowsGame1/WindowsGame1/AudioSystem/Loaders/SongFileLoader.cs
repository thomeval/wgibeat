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

        public abstract GameSong LoadFromFile(string filename);

        protected string FindUndefinedAudioFile(string path, string defFile)
        {
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

            return "";
        }
    }
}