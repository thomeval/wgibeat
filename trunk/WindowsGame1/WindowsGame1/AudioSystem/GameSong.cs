using System;
using System.IO;
using System.Security.Cryptography;
namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// Represents a single playable song in the game. This class includes all metadata
    /// loaded from a song file (.sng).
    /// </summary>
    public class GameSong
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Artist { get; set; }
        public string SongFile { get; set; }
        public string DefinitionFile { get; set; }
        public double Bpm { get; set; }
        public double Offset { get; set; }
        public double Length { get; set; }
        public string Path { get; set; }
        public string SongFileMD5 { get; set; }

        public bool Equals(GameSong other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.GetHashCode() == this.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (GameSong)) return false;
            return Equals((GameSong) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Title != null ? Title.GetHashCode() : 0);
                result = (result * 397) ^ (Subtitle != null ? Subtitle.GetHashCode() : 0);
                result = (result*397) ^ (Artist != null ? Artist.GetHashCode() : 0);
                result = (result*397) ^ (SongFile != null ? SongFile.GetHashCode() : 0);
                result = (result*397) ^ Bpm.GetHashCode();
                result = (result*397) ^ Offset.GetHashCode();
                result = (result*397) ^ Length.GetHashCode();
                return result;
            }
        }

        public static GameSong LoadDefaults()
        {
            var gs = new GameSong
                         {
                             Bpm = 0.0,
                             DefinitionFile = null,
                             Length = 0.0,
                             Offset = 0.0,
                             Path = "",
                             SongFile = "",
                             SongFileMD5 = "",
                             Subtitle = "",
                             Title = ""
                         };

            return gs;
        }

        public double GetEndingTimeInPhrase()
        {
            return ((Length - Offset) * 1000) / 1000 * (Bpm / 240);
        }

        private string CalculateMD5()
        {
            var md5 = MD5.Create();
            var fs = File.Open(Path + "\\" + SongFile, FileMode.Open);
            var temp = md5.ComputeHash(fs);
            var output = "";
            foreach (Byte b in temp)
            {
                output += b.ToString("X2");
            }

            fs.Close();
            return output;
        }
        public void SetMD5()
        {
            SongFileMD5 = CalculateMD5();
        }

        public bool VerifyMD5()
        {
            var actualMD5 = CalculateMD5();
            return actualMD5 == SongFileMD5;
        }

    }
}