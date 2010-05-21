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
            var baseSprite = new SpriteMap
                                 {
                                     SpriteTexture = TextureManager.Textures["scoreBase"],
                                     Columns = 1,
                                     Rows = 4
                                 };

            AdjustDisplayedScores();
            for (int x = 0; x < 4; x ++ )
            {
                var tempVector = _metrics["ScoreText", x];
                tempVector.X -= 13*_displayedScores[x].ToString().Length;
                if (!_players[x].Playing)
                {
                    continue;
                }
                baseSprite.Draw(spriteBatch, x, 240, 40, _metrics["ScoreBase", x]);
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + _displayedScores[x],
                                      tempVector, Color.White);

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
