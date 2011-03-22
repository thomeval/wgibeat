using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// The name of the audio file that accompanies this song - such as an .mp3 file.
        /// </summary>
        public string AudioFile { get; set; }

        /// <summary>
        /// The name of the definition file (.sng) that defines this song.
        /// </summary>
        public string DefinitionFile { get; set; }


        public double Offset { get; set; }
        public double Length { get; set; }
        public double AudioStart { get; set; }
        public string Path { get; set; }

        public bool ReadOnly { get; set; }

        public SortedDictionary<double, double> BPMs
        {
            get; set;
        }
        public double StartBPM
        {
            get
            {
                return BPMs.Values.First();
            }
            set
            {
                BPMs[0.0] = value;
            }
        }

        public double CurrentBPM(double phraseNumber)
        {
            //Return the last BPM in the BPM collection that has a phrase number
            //before or equal to the phrase number given (i.e. the most recent BPM change).
            var key = BPMs.Keys.LastOrDefault(e => e <= phraseNumber);
            return BPMs[key];
        }

        public void SetCurrentBPM(double phraseNumber, double newBPM)
        {
            var key = BPMs.Keys.LastOrDefault(e => e <= phraseNumber);
            BPMs[key] = newBPM;
        }
        /// <summary>
        /// The MD5 hash of the correct MD5 file. This is checked with the actual MD5 calculated at runtime, and
        /// mismatches are reported.
        /// </summary>
        public string AudioFileMD5 { get; set; }

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

        /// <summary>
        /// Returns a hashcode of this GameSong, for uniqueness. The hashcode only considers
        /// the Title, Subtitle, Artist, BPM, Length and Offset fields. There are 2^32 
        /// possible combinations.
        /// </summary>
        /// <returns>A calculated hashcode of this GameSong.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Title != null ? Title.GetHashCode() : 0);
                result = (result * 397) ^ (Subtitle != null ? Subtitle.GetHashCode() : 0);
                result = (result*397) ^ (Artist != null ? Artist.GetHashCode() : 0);
                result = (result * 397) ^ BPMs.GetHashCode();
                result = (result*397) ^ Offset.GetHashCode();
                result = (result*397) ^ Length.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Returns a 'blank' GameSong with all fields written with Default values.
        /// Note that no fields are returned null EXCEPT for the DefinitionFile, by design.
        /// </summary>
        /// <returns></returns>
        public static GameSong LoadDefaults()
        {
            var gs = new GameSong
                         {
                             BPMs = new SortedDictionary<double, double>(),
                             DefinitionFile = null,
                             Length = 0.0,
                             Offset = 0.0,
                             AudioStart = 0.0,
                             Path = "",
                             AudioFile = "",
                             AudioFileMD5 = "",
                             Subtitle = "",
                             Title = ""
                         };
            gs.BPMs.Add(0.0, 0.0);
            return gs;
        }

        /// <summary>
        /// Calculates the ending time of this song. The value is returned as a decimal
        /// phrase number (phrase 0.0 is the beginning of the song, and beatlines are
        /// typically spaced 1 phrase apart).
        /// </summary>
        /// <returns>The ending time of this song, as a decimal phrase number.</returns>
        public double GetEndingTimeInPhrase()
        {
            return ConvertMSToPhrase(Length  *1000);
        }

        /// <summary>
        /// Calculates the MD5 of the GameSong's AudioFile. Note that the ID3 tag of 
        /// the audio file WILL affect the MD5, but the filename will not. The value
        /// returned should be checked against the stored MD5 in the AudioFileMD5
        /// field.
        /// </summary>
        /// <returns>The calculated MD5 of the GameSong's AudioFile, or "FAIL" if the
        /// MD5 could not be calculated due to an error.</returns>
        private string CalculateMD5()
        {
            try
            {
                var md5 = MD5.Create();
                var fs = File.Open(Path + "\\" + AudioFile, FileMode.Open);
                var temp = md5.ComputeHash(fs);
                var output = "";
                foreach (Byte b in temp)
                {
                    output += b.ToString("X2");
                }

                fs.Close();
                return output;
            }
            catch (Exception ex)
            {
                return "FAIL";
            }

        }

        /// <summary>
        /// Sets the stored MD5 value in AudioFileMD5 to the correct value by
        /// calculating the actual MD5 of the audio file listed in the AudioFile
        /// field.
        /// </summary>
        public void SetMD5()
        {
            AudioFileMD5 = CalculateMD5();
        }

        /// <summary>
        /// Verifies whether the stored MD5 in the AudioFileMD5 field matches the 'real' 
        /// MD5 of the audio file listed in the AudioFile field.
        /// </summary>
        /// <returns>Whether the stored MD5 matches the actual audio file's MD5.</returns>
        public bool VerifyMD5()
        {
            var actualMD5 = CalculateMD5();
            return actualMD5 == AudioFileMD5;
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
            msLeft -= Offset*1000;
            var keys = BPMs.Keys.ToArray();
            var msList = CreateMSCache(keys);
            var lastPassedPhrase = 0.0;
            var currentKeyIndex = 0;
            //Subtract previous BPM change points.
            var activeMSKey = (from e in msList where e <= msLeft select e).LastOrDefault();

                msLeft -= activeMSKey;
                 currentKeyIndex = msList.IndexOf(activeMSKey);
                lastPassedPhrase  = keys[currentKeyIndex];        


            //Apply the phrase calculations for the current BPM active.
            
  
            var result = lastPassedPhrase;
            result += (msLeft) / 1000.0 * (BPMs[keys[currentKeyIndex]] / 240.0);

            return result;

            //TODO: Convert to using multiple BPMs.
            return (milliseconds) / 1000.0 * (StartBPM / 240.0);
        }

        private double[] CreateMSCache(double[] bpmKeys)
        {
            //TODO: Fix.
            var result = new double[bpmKeys.Length];
            for (int x = 0; x < bpmKeys.Length; x++)
            {
                double key = bpmKeys[x];
                result[x] = ConvertPhraseToMS(key) - (Offset*1000);
            }
            return result;
        }

        public double ConvertPhraseToMS(double phrase)
        {
            var keys = BPMs.Keys.ToArray();
            var activeKey = (from e in keys where e <= phrase select e).LastOrDefault();

            var totalMS = (Offset * 1000);

            var relevantPhrase = 0.0;
            for (int x = 0; keys[x] != activeKey; x++ )
            {
                relevantPhrase = keys[x + 1] - keys[x];
                totalMS += relevantPhrase* 1000 * 240.0 / BPMs[keys[x]];
            }

            relevantPhrase = phrase - activeKey;
            totalMS += relevantPhrase*1000*240.0/BPMs[activeKey];

            return totalMS;
        }
    }
}