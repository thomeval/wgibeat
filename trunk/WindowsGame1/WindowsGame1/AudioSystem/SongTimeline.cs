using System;
using System.Collections.Generic;
using System.Linq;

namespace WGiBeat.AudioSystem
{
    public class SongTimingMap
    {
        public List<SongTimingPoint> TimingPoints { get; set; }

        public SongTimingMap()
        {
            TimingPoints = new List<SongTimingPoint>();
        }
        /// <summary>
        /// Calculates a phrase number from the given milliseconds. Note that the amount of
        /// time given should be the time elapsed since the start of the audio playback.
        /// The calculation uses the GameSong's BPM and offset.
        /// </summary>
        /// <param name="milliseconds">The amount of milliseconds to convert (since the start of
        /// playback.</param>
        /// <returns>The phrase number converted from the given milliseconds.</returns>
        public double ConvertMSToPhrase(double milliseconds)
        {

            var msLeft = milliseconds;


            //Find the last past SongTimingPoint, and subtract the previously 
            //calculated milliseconds from that point.
            var activeKey = GetLastPassedTimingPointByMS(msLeft);
            var currentBPM = GetCurrentBpmByMS(msLeft);

            var result = 0.0;
            if (activeKey != null)
            {
                result += activeKey.Phrase;
                msLeft -= activeKey.MS;
            }

            //Factor in Stops of the song (by subtracting them from time elapsed).
            msLeft -= GetTotalPassedStops(milliseconds);

            //Apply the phrase calculations for the current BPM active.
            //Determine the current Phrase by applying the normal MS to phrase
            //formula for the remainder of MS (past the last SongTimingPoint.
            result += (msLeft) / 1000.0 * (currentBPM / 240.0);

            return result;

        }


        public double ConvertPhraseToMS(double phrase)
        {
            var phraseLeft = phrase;

           

            var lastPassedPoint = GetLastPassedTimingPointByPhrase(phrase);
            var currentBPM = GetCurrentBpmByPhrase(phrase);
            var result = 0.0;
            
            //Find the most recently passed TimingPoint, and calculate forward from there.
            //If the last past point is a stop, remember to add the Stop amount as well.
            if (lastPassedPoint != null)
            {
                phraseLeft -= lastPassedPoint.Phrase;
                result +=  lastPassedPoint.MS;
                if (lastPassedPoint.PointType == PointType.STOP)
                {
                    result += lastPassedPoint.Amount * 1000;
                }
            }

            result += phraseLeft * 1000 * 240.0 / currentBPM;

            return result;
        }

        private double GetCurrentBpmByMS(double milliseconds)
        {
            var lastBPMPoint =
                (from e in TimingPoints where (e.PointType == PointType.BPM_CHANGE) && (e.MS <= milliseconds) select e).
                    LastOrDefault();
            if (TimingPoints.Count == 0)
            {
                return 0.0;
            }
            if (lastBPMPoint == null)
            {
                lastBPMPoint = (from e in TimingPoints where (e.PointType == PointType.BPM_CHANGE) select e).First();
            }
            return lastBPMPoint.Amount;
        }
        private double GetCurrentBpmByPhrase(double phrase)
        {
            var lastBPMPoint =
                (from e in TimingPoints where (e.PointType == PointType.BPM_CHANGE) && (e.Phrase < phrase) select e).
                    LastOrDefault();
            if (TimingPoints.Count == 0)
            {
                return 0.0;
            }
            if (lastBPMPoint == null)
            {
                lastBPMPoint = (from e in TimingPoints where (e.PointType == PointType.BPM_CHANGE) select e).First();
            }
            return lastBPMPoint.Amount;
        }

        private SongTimingPoint GetLastPassedTimingPointByMS(double milliseconds)
        {
            return (from e in TimingPoints where e.MS < milliseconds select e).LastOrDefault();
        }

        private SongTimingPoint GetLastPassedTimingPointByPhrase(double phrase)
        {
            return (from e in TimingPoints where e.Phrase < phrase select e).LastOrDefault();

        }

        public double GetTotalPassedStops(double milliseconds)
        {
            var result = 0.0;

            var lastPassedPoint = (from e in TimingPoints where e.MS < milliseconds select e).LastOrDefault();

            if (lastPassedPoint == null || lastPassedPoint.PointType == PointType.BPM_CHANGE)
            {
                return result;
            }

            var amount = lastPassedPoint.Amount * 1000;
            if (lastPassedPoint.MS + amount <= milliseconds)
            {
                //The current time is past the current stop.
                result += amount;
                //    System.Diagnostics.Debug.WriteLine("OUTSIDE Stop: Result = " + result);
            }
            else
            {
                //The current time is 'inside' the current stop.
                result = (milliseconds - lastPassedPoint.MS);
                //   System.Diagnostics.Debug.WriteLine("INSIDE Stop: Result = " + result);
            } 

            /*
            var lastStopPoint = (from e in TimingPoints where e.PointType == PointType.STOP && e.MS < milliseconds select e).LastOrDefault();
            if (lastStopPoint != null)
            {
                var amount = lastStopPoint.Amount * 1000;
                if (lastStopPoint.MS + amount <= milliseconds)
                {
                    //The current time is past the current stop.
                    result += amount;
                //    System.Diagnostics.Debug.WriteLine("OUTSIDE Stop: Result = " + result);
                }
                else
                {
                    //The current time is 'inside' the current stop.
                    result = (milliseconds - lastStopPoint.MS);
                 //   System.Diagnostics.Debug.WriteLine("INSIDE Stop: Result = " + result);
                } 
            }
            else
            {
              //  System.Diagnostics.Debug.WriteLine("NO Stop: Result = " + result);
            }
             */
            return result;
 
        }


        public static SongTimingMap CreateSongTimingMap(Dictionary<double, double> stops, Dictionary<double, double> bpmChanges)
        {
            var result = new SongTimingMap();


            var bpmKeys = bpmChanges.Keys.ToArray();
            var stopKeys = stops.Keys.ToArray();

            foreach (var bpmKey in bpmKeys)
            {
                result.TimingPoints.Add(new SongTimingPoint{Amount = bpmChanges[bpmKey],Phrase = bpmKey,PointType = PointType.BPM_CHANGE});
            }
            foreach (var stopKey in stopKeys)
            {
                result.TimingPoints.Add(new SongTimingPoint{Amount = stops[stopKey],Phrase = stopKey,PointType = PointType.STOP});
            }

            result.SortByPhrase();
            result.CalculateMSTable();
            //TODO: How much memory does this use?
            return result;
        }

        private void CalculateMSTable()
        {
            foreach (var timingPoint in TimingPoints)
            {
                //This should be pseudo-recursive (this method affects the MS table,
                //but also relies on it to calculate the MS for a specific point).
                timingPoint.MS = ConvertPhraseToMS(timingPoint.Phrase); 
            }
        }

        private void SortByPhrase()
        {
           TimingPoints.Sort(CompareByPhrase);
        }

        private int CompareByPhrase(SongTimingPoint first, SongTimingPoint second)
        {
            if ((first == null) || (second == null))
            {
                return 0;
            }
            return first.Phrase.CompareTo(second.Phrase);
        }
    }

}

