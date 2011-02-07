using WGiBeat.Managers;

namespace WGiBeat.AudioSystem.Loaders
{
    public abstract class SongFileLoader
    {
        public LogManager Log { get; set; }
        public string Pattern { get; set; }
        public double OffsetAdjust { get; set; }
        public abstract GameSong LoadFromFile(string filename);

    }
}