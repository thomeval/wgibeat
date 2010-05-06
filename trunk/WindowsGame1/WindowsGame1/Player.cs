using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame1
{
    public class Player
    {

        public Player()
        {
            Judgements = new int[8];
        }
        public string Name { get; set; }
        private double _life;

        private const double LIFE_MAX = 200;
        public double Life
        {
            get { return _life; }
            set
            {
                _life = Math.Min(LIFE_MAX, value);
                if (_life <= 0)
                {
                    KO = true;
                    _life = 0;
                }
            }
        }

        public long Score { get; set; }
        public long DisplayedScore { get; set; }
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
        //0 = Ideal, 1 = Cool, 2 = Ok, 3 = Bad, 4 = Fail, 5 = Fault, 6 = Miss;
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

        public void AddLife(double amount)
        {
            if (Life + amount > 100)
            {
                if (Life >= 100)
                {
                    Life += amount/3;
                }
                else
                {
                    double over = Life + amount - 100;
                    Life = 100 + (over/3);
                }


            }
            else
            {
                Life += amount;
            }
        }
        public Difficulty PlayDifficulty { get; set; }

        public void MissedArrow()
        {
            Hits = 0;
            Momentum = (long)(Momentum * 0.95);
            Judgements[5]++;
            switch (PlayDifficulty)
            {
                case Difficulty.BEGINNER:
                    break;
                    case Difficulty.EASY:
                    Life -= 1;
                    break;
                case Difficulty.MEDIUM:
                    Life -= 2;
                    break;
                case Difficulty.HARD:
                    Life -= 4;
                    break;
            }
        }

        public void MissedBeat()
        {
            Momentum = (long)(Momentum * 0.8);
            Judgements[6]++;
            switch (PlayDifficulty)
            {
                case Difficulty.BEGINNER:
                    Life -= 2;
                    break;
                case Difficulty.EASY:
                    Life -= 4;
                    break;
                case Difficulty.MEDIUM:
                    Life -= 8;
                    break;
                case Difficulty.HARD:
                    Life -= 12;
                    break;
            }
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
