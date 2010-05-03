using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame1.AudioSystem
{
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

        public bool Equals(GameSong other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Title, Title) && Equals(other.Artist, Artist) && Equals(other.SongFile, SongFile) && other.Bpm == Bpm && other.Offset == Offset && other.Length == Length;
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
    }
}