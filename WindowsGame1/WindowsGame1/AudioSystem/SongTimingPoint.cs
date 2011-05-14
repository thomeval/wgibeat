namespace WGiBeat.AudioSystem
{
    public class SongTimingPoint
    {
        public double Amount { get; set; }
        public PointType PointType {get;set; }
        public double Phrase { get; set; }
        public double MS { get; set; }

        public override string ToString()
        {
            return string.Format("{0} of {1} at {2:F3} ({3:F0}ms)", PointType,Amount,Phrase,MS);     
        }
    }

    public enum PointType
    {
        STOP = 1,
        BPM_CHANGE = 0
    }

    
}
