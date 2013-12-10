using System;
using System.Collections.Generic;
using System.Linq;
using WGiBeat.Notes;

namespace WGiBeat.Players
{
    /// <summary>
    /// Represents a single player currently playing the game. Multiple players can play simultaneously.
    /// </summary>
    public class Player
    {

        public Player()
        {
            Judgements = new int[8];
            PlayerOptions = new PlayerOptions();
            ApplyDefaultOptions();

        }

        public Profile Profile { get; set; }
        public double Life { get; set; }
        public long Score { get; set; }
        public bool IsBlazing { get; set; }
        public int Team { get; set; }
        public long PlayTime { get; set; }
        public BeatlineNoteJudgement NextCPUJudgement { get; set; }
        public PlayerOptions PlayerOptions { get; set; }
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
        public int TotalHits { get; set; }
        public long Momentum { get; set; }

        private static double _grooveMomentum;
        public static double GrooveMomentum
        {
            get { return _grooveMomentum; }
            set
            {
                _grooveMomentum = Math.Max(0.5, value);
                PeakGrooveMomentum = Math.Max(PeakGrooveMomentum, GrooveMomentum);
            }
        }

        public static double PeakGrooveMomentum {get; set; }

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
        public bool CPU { get; set; }
        public bool Remote { get; set; }

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

                double x = Math.Log((Momentum + 150) / 100.0, 1.5);
                x = Math.Min(MaxArrowLevel(PlayerOptions.PlayDifficulty), x);

                return Math.Max(1, x);

            }
        }

        private static int MaxArrowLevel(Difficulty difficulty)
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
                    return 9;
                case Difficulty.INSANE:
                    return 11;
                case Difficulty.RUTHLESS:
                    return 13;
                default:
                    return 10;
            }
        }

        public int MaxArrowLevel()
        {
            return MaxArrowLevel(PlayerOptions.PlayDifficulty);
        }


        private readonly List<double> _lifeHistory = new List<double>();
        public List<double> LifeHistory
        {
            get { return _lifeHistory; }
        }

        private readonly List<double> _levelHistory = new List<double>();
        public List<double> LevelHistory
        {
            get { return _levelHistory; }
        }

        private readonly List<long> _scoreHistory = new List<long>();
        public List<long> ScoreHistory
        {
            get { return _scoreHistory; }
        }

        private readonly static List<double> _gmHistory = new List<double>();
        public static List<double> GMHistory
        {
            get { return _gmHistory; }
        }



        public bool IsHumanPlayer
        {
            get { return Playing && !CPU; }
        }

        public bool IsCPUPlayer
        {
            get { return Playing && CPU; }
        }

        public string SafeName
        {
            get
            {
                if (Profile == null)
                {
                    return "";
                }
                return Profile.Name;
            }

        }

        public bool IsLocalHumanPlayer
        {
            get { return IsHumanPlayer && (!Remote); }
        }

        public void RecordCurrentLife()
        {
            _lifeHistory.Add((float)Life);
            _gmHistory.Add(GrooveMomentum);
            _levelHistory.Add(Level);
            _scoreHistory.Add(Score);
        }

        public double MissedArrow()
        {
            Hits = 0;
            Momentum = (long)(Momentum * 0.95);
            Judgements[6]++;
            int result = 0;
            switch (PlayerOptions.PlayDifficulty)
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
                case Difficulty.INSANE:
                    result -= 6;
                    break;
                    case Difficulty.RUTHLESS:
                    result -= 8;
                    break;
            }
            return result;
        }

        public double MissedBeat()
        {

            var result = 0;
            switch (PlayerOptions.PlayDifficulty)
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
                case Difficulty.INSANE:
                    result -= 16;
                    break;
                case Difficulty.RUTHLESS:
                    result -= 20;
                    break;
            }
            return result;
        }

        public double FailedBeat()
        {

            var result = 0;
            switch (PlayerOptions.PlayDifficulty)
            {
                case Difficulty.BEGINNER:
                    result = -4;
                    break;
                case Difficulty.EASY:
                    result = -8;
                    break;
                case Difficulty.MEDIUM:
                    result = -16;
                    break;
                case Difficulty.HARD:
                    result = -24;
                    break;
                case Difficulty.INSANE:
                    result -= 32;
                    break;
                case Difficulty.RUTHLESS:
                    result -= 40;
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
            TotalHits = 0;
            Streak = 0;
            MaxStreak = 0;
            Momentum = GetStartingMomentum();
            GrooveMomentum = 1.0;
            PeakGrooveMomentum = 1.0;
            Life = this.GetMaxLife() / 2;
            KO = false;
            IsBlazing = false;
            PlayTime = 0;
            _lifeHistory.Clear();
            _scoreHistory.Clear();
            _levelHistory.Clear();
            _gmHistory.Clear();
            NextCPUJudgement = BeatlineNoteJudgement.COUNT;
        }

        private long GetStartingMomentum()
        {
            switch (PlayerOptions.PlayDifficulty)
            {
                case Difficulty.RUTHLESS:
                    return 1000;
                    case Difficulty.INSANE:
                    return 360;
                    case Difficulty.HARD:
                    return 190;
                    case Difficulty.MEDIUM:
                    return 76;
                default:
                    return 0;
            }
        }

        public void UpdateToProfile()
        {
            if (Profile == null)
            {
                return;
            }
            for (int x = 0; x < Judgements.Count(); x++)
            {
                Profile.JudgementCounts[x] += Judgements[x];
            }
            Profile.TotalHits += TotalHits;
            Profile.TotalPlayTime += PlayTime;
            Profile.MostHitsEver = Math.Max(Profile.MostHitsEver, (int) MaxHits);
            Profile.MostStreakEver = Math.Max(Profile.MostStreakEver, MaxStreak);
            Profile.EXP += AwardXP();
        }

        private readonly int[] _gradeBonusCutoffs = { 90, 86, 82, 78, 70 };
        private readonly double[] _gradeBonusAmounts = { 1.4, 1.3, 1.2, 1.1, 1.05 };
        private readonly double[] _difficultyMultipliers = { 0.5, 0.75, 1.0, 1.1, 1.2, 1.25 };
     

        public long AwardXP()
        {
            double result = 0;

            result += Judgements[0] * 3;
            result += Judgements[1] * 2;
            result += Judgements[2] * 1;

            var grade = CalculatePercentage();

            for (int x = 0; x < _gradeBonusCutoffs.Length; x++)
            {
                if (grade >= _gradeBonusCutoffs[x])
                {
                    result *= _gradeBonusAmounts[x];
                    break;
                }
            }

            var difficulty = (int)PlayerOptions.PlayDifficulty;

            result *= _difficultyMultipliers[difficulty];
            return (long)Math.Floor(result);

        }

        public double CalculatePercentage()
        {

            // Ideal + Cool + OK + Bad + Fail + Miss
            int maxPossible = Judgements[0] + Judgements[1] + Judgements[2] + Judgements[3] + Judgements[4] +
                              Judgements[5];
            maxPossible *= 8;

            //Ideals
            int playerScore = Judgements[0] * 8;
            //Cools
            playerScore += Judgements[1] * 6;
            //OKs
            playerScore += Judgements[2] * 3;
            //Bads
            playerScore += Judgements[3];
            //Fails
            playerScore += Judgements[4] * -4;
            //Faults
            playerScore += Judgements[6] * -1;

            return 100.0 * playerScore / maxPossible;
        }

        public void ApplyDefaultOptions()
        {
            PlayerOptions.BeatlineSpeed = 1.0;
            PlayerOptions.DisableKO = false;
            PlayerOptions.ScrollDirectionEast = true;
            PlayerOptions.ScrollDirectionWest = false;
            PlayerOptions.PlayDifficulty = Difficulty.BEGINNER;
        }
    }

    [Serializable]
    public class PlayerOptions
    {
        public bool DisableKO { get; set; }
        public double BeatlineSpeed { get; set;}
        public Difficulty PlayDifficulty { get; set;}
        public bool ScrollDirectionWest { get; set; }
        public bool ScrollDirectionEast { get; set; }
        public bool DisableExtraLife { get; set; }

        public PlayerOptions()
        {
            ScrollDirectionEast = true;
            BeatlineSpeed = 1.0;
            PlayDifficulty = Difficulty.BEGINNER;
     
        }
    }

    public enum Difficulty
    {
        BEGINNER = 0,
        EASY = 1,
        MEDIUM = 2,
        HARD = 3,
        INSANE = 4,
        RUTHLESS =5,
        COUNT = 6
    }
}