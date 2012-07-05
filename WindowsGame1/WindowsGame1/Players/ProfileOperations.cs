using System;
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
            player.Profile.LastPlayerOptions = player.PlayerOptions;

        }

        public static void LoadPreferences(this Player player)
        {
            if (player.Profile == null)
            {
                return;
            }
            player.PlayerOptions = player.Profile.LastPlayerOptions;

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
            
            if (player.GetLevel() >= 50)
            {
                return 5;
            }
              
            return 4;
        }
   
        public static double GetMaxLife(this Player player)
        {
            if (player.Profile == null)
            {
                return 100;
            }

            if (player.PlayerOptions.DisableExtraLife)
            {
                return 100;
            }
            var maxLife = 100;

            maxLife += (player.GetLevel() - 1)*5;
            maxLife = Math.Min(300, maxLife);
            return maxLife;
        }

        public static long MaxLevelEXP()
        {
            return (Profile.Levels.Max());
        }
    }
}
