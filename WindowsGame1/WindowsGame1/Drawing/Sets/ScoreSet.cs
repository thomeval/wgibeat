using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class ScoreSet: DrawableObjectSet
    {
        private const int SCORE_UPDATE_SPEED = 12;

        private readonly double[] _displayedScores;
        private SpriteMap _iconSpriteMap;
        private SpriteMap _coopBaseSprite;
        private Sprite _coopPulseSprite;
        private Sprite _individualBaseSprite;
        private Sprite _individualPulseSprite;
        private SpriteMap _playerIdentifierSpriteMap;

        private TeamScoreMeter _teamScoreMeter;
        private SpriteMap _iconSyncBaseSpriteMap;
        private readonly Color[] _pulseColors = {new Color(255,128,128),new Color(128,128,255), new Color(128,255,128),new Color(255,255,128)    };

        public ScoreSet(MetricsManager metrics, Player[] players, GameType type)
            : base(metrics,players,type)
        {
            _displayedScores = new double[4];
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
                SpriteTexture = TextureManager.Textures("PlayerDifficulties")
            };

            _iconSyncBaseSpriteMap = new SpriteMap
                                         {
                                             Columns = 1,
                                             Rows = 2,
                                             SpriteTexture = TextureManager.Textures("SyncDifficultyBar")
                                         };
            _coopBaseSprite = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures("ScoreBaseCombined"),
                Columns = 1,
                Rows = 1
            };
            _teamScoreMeter = new TeamScoreMeter();
            _teamScoreMeter.InitSprites();
            _playerIdentifierSpriteMap = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures("PlayerIdentifiers"),
                Columns = 1,
                Rows = 5
            };
            _individualBaseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("ScoreBase")
            };
            _individualPulseSprite = new Sprite
                                         {
                                             SpriteTexture = TextureManager.Textures("ScorePulse")
                                         };
            _coopPulseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("ScorePulse")
            };
        }
        private void DrawPlayerDifficulties(SpriteBatch spriteBatch)
        {
            if (_gameType == GameType.SYNC)
            {
                DrawPlayerDifficultiesSync(spriteBatch);
                return;
            }
            for (int x = 0; x < 4; x++)
            {
                if (! Players[x].Playing)
                {
                    continue;
                }
                int idx = 1 + (int)(Players[x].PlayerOptions.PlayDifficulty);
 
                _iconSpriteMap.Draw(spriteBatch, idx, 30, 30, _metrics["GameScreenPlayerDifficulties", x]);
            }
        }

        private void DrawPlayerDifficultiesSync(SpriteBatch spriteBatch)
        {
            int idx = 1 + (int)(Players[0].PlayerOptions.PlayDifficulty);

            for (int x = 0; x < 2; x++)
            {
                if (Players[x * 2].Playing || Players[(x * 2) + 1].Playing)
                {
                    _iconSyncBaseSpriteMap.Draw(spriteBatch, x, _metrics["SyncPlayerDifficultiesBase", x]);
                    _iconSpriteMap.Draw(spriteBatch, idx, 30, 30, _metrics["SyncPlayerDifficulties", x]);
                }
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
                    case GameType.SYNC:
                    DrawSyncCombinedScore(spriteBatch);
                    break;
            }
        }

        private void DrawSyncCombinedScore(SpriteBatch spriteBatch)
        {

            var scoreText = _displayedScores[0];

            for (int x = 0; x < 2; x++)
            {
                if ((Players[2 * x].Playing) || (Players[(2 * x) + 1].Playing))
                {

                    _coopBaseSprite.Draw(spriteBatch, 0, 240, 40, _metrics["ScoreCombinedBase", x]);
                    _coopPulseSprite.Position = _metrics["ScoreCombinedBase", x];
                    _coopPulseSprite.ColorShading.A = (byte)Math.Min(255, 2 * Math.Sqrt(Players[0].Score - scoreText));
                    _coopPulseSprite.Draw(spriteBatch);
                    TextureManager.DrawString(spriteBatch, "" + (long) scoreText, "LargeFont",
                                           _metrics["ScoreCombinedText", x], Color.White, FontAlign.RIGHT);
                }
            }
        }

        private void DrawCoopCombinedScore(SpriteBatch spriteBatch)
        {
            double scoreText = 0;
            for (int x = 0; x < 4; x++)
            {
                if (Players[x].Playing)
                {
                    scoreText += _displayedScores[x];
                }
            }

            long totalScore = (from e in Players where e.Playing select e.Score).Sum();
            for (int x = 0; x < 2; x++)
            {
                if ((Players[2 * x].Playing) || (Players[(2 * x) + 1].Playing))
                {

                    _coopBaseSprite.Draw(spriteBatch, 0, 240, 40, _metrics["ScoreCombinedBase", x]);
                    _coopPulseSprite.Position = _metrics["ScoreCombinedBase", x];
                    _coopPulseSprite.ColorShading.A = (byte) Math.Min(255, 2* Math.Sqrt(totalScore - scoreText));
                    _coopPulseSprite.Draw(spriteBatch);

                    TextureManager.DrawString(spriteBatch, "" + (long) scoreText, "LargeFont",
                                           _metrics["ScoreCombinedText", x], Color.White, FontAlign.RIGHT);
                }
            }
        }
        private void DrawTeamCombinedScores(SpriteBatch spriteBatch)
        {
            double blueScore = 0, redScore = 0;
            for (int x = 0; x < 4; x++)
            {
                if (Players[x].Playing)
                {
                    if (Players[x].Team == 1)
                    {
                        blueScore += _displayedScores[x];
                    }
                    else if (Players[x].Team == 2)
                    {
                        redScore += _displayedScores[x];
                    }
                }
            }

            _teamScoreMeter.BlueScore = (long) blueScore;
            _teamScoreMeter.RedScore = (long) redScore;

            if (Players[0].Playing || Players[1].Playing)
            {
                _teamScoreMeter.Position = (_metrics["ScoreCombinedBase", 0]);
                _teamScoreMeter.Draw(spriteBatch);
            }
            if (Players[2].Playing || (Players[3].Playing))
            {
                _teamScoreMeter.Position = (_metrics["ScoreCombinedBase", 1]);
                _teamScoreMeter.Draw(spriteBatch);
            }
            _teamScoreMeter.Update();

        }

        private void DrawIndividualScores(SpriteBatch spriteBatch)
        {
            if (_gameType == GameType.SYNC)
            {
                return;
            }
            for (int x = 0; x < 4; x++)
            {

                if (!Players[x].Playing)
                {
                    continue;
                }
                var idx = (Players[x].IsCPUPlayer) ? 4 : x;
                _individualBaseSprite.Position = _metrics["ScoreBase", x];
                _individualBaseSprite.Draw(spriteBatch);
                _individualPulseSprite.Position = _metrics["ScoreBase", x];
                _individualPulseSprite.ColorShading = _pulseColors[x];
                _individualPulseSprite.ColorShading.A = (byte) Math.Min(255, 4* Math.Sqrt(Players[x].Score - _displayedScores[x]));
                _individualPulseSprite.Draw(spriteBatch);
                var identifierPosition = _metrics["ScoreBase", x].Clone();
                identifierPosition.X += 12;
                identifierPosition.Y += 5;
                _playerIdentifierSpriteMap.Draw(spriteBatch,idx,55,30,identifierPosition);

                TextureManager.DrawString(spriteBatch, "" + (long) _displayedScores[x], "LargeFont",
                                      _metrics["ScoreText", x], Color.White,FontAlign.RIGHT);
            }
        }

        private void AdjustDisplayedScores()
        {
            for (int x = 0; x < 4; x++)
            {

                var diff = Players[x].Score - _displayedScores[x];
                if (diff <= 10)
                {
                    _displayedScores[x] = Players[x].Score;
                    diff = 0;
                }
                var changeMx = Math.Min(0.5, TextureManager.LastDrawnPhraseDiff * SCORE_UPDATE_SPEED);
                _displayedScores[x] += (diff * (changeMx));


            }
        }

        public void AdjustScore(long amount, int player)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.COOPERATIVE:
                case GameType.TEAM:
                case GameType.VS_CPU:
                case GameType.SYNC:
                    Players[player].Score += amount;
                        break;
            }
        }
    }
}
