using System.Linq;

namespace WGiBeat.Players
{
    public static class ProfileOperations
    {
        public static readonly long[] Levels = {
                                                   0, 50, 110, 180, 260, 360, 480, 640, 820, 1000, 1200, 1450, 1700, 2000, 2300, 2600, 3000,
                                                   3400, 3900, 4400, 5000,5750,6500,7500,8500,9600,10700,11800,13000,14500,16000,17750,20000,
                                                   22500,25000,28000,31000,34000,36500,40000
                                               };

        public static int GetLevel(this Player player)
        {
            if (player.Profile == null)
            {
                return 1;
            }
            for (int x = 0; x < Levels.Length; x++)
            {
                if (player.Profile.EXP < Levels[x])
                {
                    return x;
                }
            }
            return Levels.Length;
        }

        public static long GetEXP(this Player player)
        {
            if (player.Profile == null)
            {
                return 0;
            }
            return player.Profile.EXP;

        }

        public static void UpdatePreferences(this Player player)
        {
            if (player.Profile == null)
            {
                return;
            }
            player.Profile.LastBeatlineSpeed = player.BeatlineSpeed;
            player.Profile.LastDifficulty = player.PlayDifficulty;
            player.Profile.DisableKO = player.DisableKO;

        }

        public static void LoadPreferences(this Player player)
        {
            if (player.Profile == null)
            {
                return;
            }
            player.BeatlineSpeed = player.Profile.LastBeatlineSpeed;
            player.PlayDifficulty = player.Profile.LastDifficulty;
            player.DisableKO = player.Profile.DisableKO;
        }

        public static long GetNextEXPSafe(this Player player)
        {
            if (player.Profile == null)
            {
                return 1;
            }
            if (player.Profile.EXP > player.MaxLevelEXP())
            {
                return player.MaxLevelEXP();
            }
            return Levels[player.GetLevel()];
        }

        public static double GetLevelProgressSafe(this Player player)
        {
            if (player.Profile == null)
            {
                return 0.0;
            }

            var currentLevelExp = Levels[player.GetLevel() - 1];

            return 1.0 * (player.Profile.EXP - currentLevelExp) /
                   (player.GetNextEXPSafe() - currentLevelExp);
        }

        public static int GetMaxDifficulty(this Player player)
        {
            if ((player.Profile == null) || (player.GetLevel() < 10))
            {
                return 3;
            }

            return 4;

        }

        private static readonly int[] _lifeMilestones = { 5, 10, 15, 20, 25, 30, 35, 40 };
        public static double GetMaxLife(this Player player)
        {
            if (player.Profile == null)
            {
                return 100;
            }

            var maxLife = 100;

            foreach (int milestone in _lifeMilestones)
            {
                if (player.GetLevel() >= milestone)
                {
                    maxLife += 25;
                }
            }
            return maxLife;
        }

        public static long MaxLevelEXP(this Player player)
        {
            return (Levels.Max());
        }
    }
}
