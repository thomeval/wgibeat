using System.Linq;

namespace WGiBeat.Players
{
    public static class ProfileOperations
    {


        public static int GetLevel(this Player player)
        {
            if (player.Profile == null)
            {
                return 1;
            }
            return player.Profile.GetLevel();
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
            if (player.Profile.EXP > MaxLevelEXP())
            {
                return MaxLevelEXP();
            }
            return Profile.Levels[player.GetLevel()];
        }

        public static double GetLevelProgressSafe(this Player player)
        {
            if (player.Profile == null)
            {
                return 0.0;
            }

            var currentLevelExp = Profile.Levels[player.GetLevel() - 1];

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

        public static long MaxLevelEXP()
        {
            return (Profile.Levels.Max());
        }
    }
}
