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
        private SpriteMap _iconSpriteMap;

        private ScoreSet()
        {
            _displayedScores = new long[4];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            AdjustDisplayedScores();

            DrawIndividualScores(spriteBatch);
            DrawCombinedScores(spriteBatch);
            DrawPlayerDifficulties(spriteBatch);

        }

        private void DrawPlayerDifficulties(SpriteBatch spriteBatch)
        {
            if (_iconSpriteMap == null)
            {
                _iconSpriteMap = new SpriteMap
                                        {
                                            Columns = 1,
                                            Rows = (int) Difficulty.COUNT + 1,
                                            SpriteTexture = TextureManager.Textures["playerDifficulties"]
                                        };
            }
            for (int x = 0; x < 4; x++)
            {
                if (! _players[x].Playing)
                {
                    continue;
                }
                int idx = 1 + (int)(_players[x].PlayDifficulty);
                _iconSpriteMap.Draw(spriteBatch, idx, 30, 30, _metrics["GameScreenPlayerDifficulties", x]);
            }
        }

        private void DrawCombinedScores(SpriteBatch spriteBatch)
        {
            long scoreText = 0;

            var baseSprite = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures["scoreBaseCombined"],
                Columns = 1,
                Rows = 1
            };

            switch (_gameType)
            {
                case GameType.NORMAL:
                    return;
                    case GameType.COOPERATIVE:
                    for (int x = 0; x < 4; x++ )
                    {
                        if (_players[x].Playing)
                        {
                            scoreText +=_displayedScores[x];
                        }
                    }
                        break;
            }
            for (int x = 0; x < 2; x++)
            {
                if ((_players[2*x].Playing) || (_players[(2*x) + 1].Playing))
                {

                   

                    baseSprite.Draw(spriteBatch, 0, 240, 40, _metrics["ScoreCombinedBase", x]);
                    TextureManager.DrawString(spriteBatch,"" + scoreText, "LargeFont",
                                           _metrics["ScoreCombinedText", x], Color.White,FontAlign.RIGHT);
                }
            }
        }

        private void DrawIndividualScores(SpriteBatch spriteBatch)
        {
            var baseSprite = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures["scoreBase"],
                Columns = 1,
                Rows = 4
            };

            for (int x = 0; x < 4; x++)
            {

                if (!_players[x].Playing)
                {
                    continue;
                }
                baseSprite.Draw(spriteBatch, x, 240, 40, _metrics["ScoreBase", x]);
                TextureManager.DrawString(spriteBatch, "" + _displayedScores[x], "LargeFont",
                                      _metrics["ScoreText", x], Color.White,FontAlign.RIGHT);
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
                _displayedScores[x] = Math.Min(_players[x].Score, _displayedScores[x] + amount);
            }
        }
    }
}
