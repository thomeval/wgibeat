using System;
using System.Linq;
using WGiBeat.Notes;

namespace WGiBeat
{
    public class Player
    {

        public Player()
        {
            Judgements = new int[8];
        }
        public string Name { get; set; }

        public double Life { get; set; }

        public long Score { get; set; }

        private long _hits;
        public long Hits
        {
            get { return _hits; }
            set
            {
                _hits = value;
                if (_hits > MaxHits)
                {
                    MaxHits = _hits;
                }
            }
        }

        public long MaxHits { get; set; }
        public long Momentum { get; set; }

        private int _streak;
        public int Streak
        {
            get { return _streak; }
            set
            {
                _streak = value;
                if (_streak > MaxStreak)
                {
                    MaxStreak = _streak;
                }
            }
        }

        public int MaxStreak { get; set; }

        public bool KO { get; set; }
        public bool Playing { get; set; }

        //0 = Ideal, 1 = Cool, 2 = Ok, 3 = Bad, 4 = Fail, 5 = Miss, 6 = Fault;
        public int[] Judgements { get; set; }

        public double Level
        {
            get
            {
                if (Momentum == 0)
                {
                    return 1;
                }
 
                double x = Math.Log((Momentum + 150)/100.0, 1.5);
                x = Math.Min(MaxDifficulty(PlayDifficulty), x);
                
                return Math.Max(1,x);

            }
        }

        private int MaxDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.BEGINNER:
                    return 3;
                case Difficulty.EASY:
                    return 5;
                case Difficulty.MEDIUM:
                    return 7;
                case Difficulty.HARD:
                    return 10;
                    default:
                    return 10;
            }
        }

        public int MaxDifficulty()
        {
                return MaxDifficulty(PlayDifficulty);
        }


        public Difficulty PlayDifficulty { get; set; }

        public double MissedArrow()
        {
            Hits = 0;
            Momentum = (long)(Momentum * 0.95);
            Judgements[6]++;
            int result = 0;
            switch (PlayDifficulty)
            {
                case Difficulty.BEGINNER:
                    break;
                    case Difficulty.EASY:
                    result = -1;
                    break;
                case Difficulty.MEDIUM:
                    result = -2;
                    break;
                case Difficulty.HARD:
                    result = -4;
                    break;
            }
            return result;
        }

        public double MissedBeat()
        {
            Momentum = (long)(Momentum * 0.8);
            Judgements[(int) BeatlineNoteJudgement.MISS]++;
            var result = 0;
            switch (PlayDifficulty)
            {
                case Difficulty.BEGINNER:
                    result = -2;
                    break;
                case Difficulty.EASY:
                    result = -4;
                    break;
                case Difficulty.MEDIUM:
                    result = -8;
                    break;
                case Difficulty.HARD:
                    result = -12;
                    break;
            }
            return result;
        }

        public void ResetStats()
        {
            for (int x = 0; x < Judgements.Count(); x++)
            {
                Judgements[x] = 0;
            }
            Score = 0;
            Hits = 0;
            MaxHits = 0;
            Streak = -1;
            MaxStreak = 0;
            Momentum = 0;
            Life = 50;
            KO = false;
            
        }
        
    }

    public enum Difficulty
    {
        BEGINNER = 0,
        EASY = 1,
        MEDIUM = 2,
        HARD = 3
    }
}
