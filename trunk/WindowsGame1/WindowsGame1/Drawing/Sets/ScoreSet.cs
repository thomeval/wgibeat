using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing.Sets
{
    public class ScoreSet: DrawableObject
    {
        private readonly Player[] _players;
        private long[] _displayedScores;
        private readonly MetricsManager _metrics;
        private readonly GameType _gameType;

        private ScoreSet()
        {
            _displayedScores = new long[4];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            AdjustDisplayedScores();
            for (int x = 0; x < 4; x ++ )
            {
                if (!_players[x].Playing)
                {
                    continue;
                }
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "P" + (x + 1), _metrics["PlayerText", x], Color.Black);
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + _displayedScores[x],
                                       _metrics["ScoreText", x], Color.Black);
            }

        }

        public ScoreSet(MetricsManager metrics, Player[] players, GameType type)
            :this()
        {
            _metrics = metrics;
            _players = players;
            _gameType = type;
        }

        private void AdjustDisplayedScores()
        {
            for (int x = 0; x < 4; x++)
            {
                if (!_players[x].Playing)
                {
                    continue;
                }

                var amount = Math.Max(25, (_players[x].Score - _displayedScores[x]) / 10);
                Console.WriteLine(amount);
                _displayedScores[x] = Math.Min(_players[x].Score, _displayedScores[x] + amount);
            }
        }
    }
}
