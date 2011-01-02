using System;
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
        private SpriteMap _coopBaseSprite;
        private SpriteMap _individualBaseSprite;

        private TeamScoreMeter _teamScoreMeter;
        private ScoreSet()
        {
            _displayedScores = new long[4];
        }

        public ScoreSet(MetricsManager metrics, Player[] players, GameType type)
            : this()
        {
            _metrics = metrics;
            _players = players;
            _gameType = type;
            InitSprites();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            AdjustDisplayedScores();

            DrawIndividualScores(spriteBatch);
            DrawCombinedScores(spriteBatch);
            DrawPlayerDifficulties(spriteBatch);

        }

        private void InitSprites()
        {
            _iconSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = (int)Difficulty.COUNT + 1,
                SpriteTexture = TextureManager.Textures["playerDifficulties"]
            };

            _coopBaseSprite = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures["scoreBaseCombined"],
                Columns = 1,
                Rows = 1
            };
            _teamScoreMeter = new TeamScoreMeter();
            _teamScoreMeter.InitSprites();
            _individualBaseSprite = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures["scoreBase"],
                Columns = 1,
                Rows = 4
            };
        }
        private void DrawPlayerDifficulties(SpriteBatch spriteBatch)
        {

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
            switch (_gameType)
            {
                case GameType.NORMAL:
                    return;
                case GameType.COOPERATIVE:
                    DrawCoopCombinedScore(spriteBatch);
                    break;
                case GameType.TEAM:
                case GameType.VS_CPU:
                    DrawTeamCombinedScores(spriteBatch);
                    break;
            }
        }

        private void DrawCoopCombinedScore(SpriteBatch spriteBatch)
        {
            long scoreText = 0;
            for (int x = 0; x < 4; x++)
            {
                if (_players[x].Playing)
                {
                    scoreText += _displayedScores[x];
                }
            }

            for (int x = 0; x < 2; x++)
            {
                if ((_players[2 * x].Playing) || (_players[(2 * x) + 1].Playing))
                {

                    _coopBaseSprite.Draw(spriteBatch, 0, 240, 40, _metrics["ScoreCombinedBase", x]);
                    TextureManager.DrawString(spriteBatch, "" + scoreText, "LargeFont",
                                           _metrics["ScoreCombinedText", x], Color.White, FontAlign.RIGHT);
                }
            }
        }
        private void DrawTeamCombinedScores(SpriteBatch spriteBatch)
        {
            long blueScore = 0, redScore = 0;
            for (int x = 0; x < 4; x++)
            {
                if (_players[x].Playing)
                {
                    if (_players[x].Team == 1)
                    {
                        blueScore += _displayedScores[x];
                    }
                    else if (_players[x].Team == 2)
                    {
                        redScore += _displayedScores[x];
                    }
                }
            }

            _teamScoreMeter.BlueScore = blueScore;
            _teamScoreMeter.RedScore = redScore;

            if (_players[0].Playing || _players[1].Playing)
            {
                _teamScoreMeter.Position = (_metrics["ScoreCombinedBase", 0]);
                _teamScoreMeter.Draw(spriteBatch);
            }
            if (_players[2].Playing || (_players[3].Playing))
            {
                _teamScoreMeter.Position = (_metrics["ScoreCombinedBase", 1]);
                _teamScoreMeter.Draw(spriteBatch);
            }
            _teamScoreMeter.Update();

        }

        private void DrawIndividualScores(SpriteBatch spriteBatch)
        {

            for (int x = 0; x < 4; x++)
            {

                if (!_players[x].Playing)
                {
                    continue;
                }
                _individualBaseSprite.Draw(spriteBatch, x, 240, 40, _metrics["ScoreBase", x]);
                TextureManager.DrawString(spriteBatch, "" + _displayedScores[x], "LargeFont",
                                      _metrics["ScoreText", x], Color.White,FontAlign.RIGHT);
            }
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
