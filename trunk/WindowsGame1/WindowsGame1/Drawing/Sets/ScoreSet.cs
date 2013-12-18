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
        private SpriteMap3D _iconSpriteMap;
        private Sprite3D _coopBaseSprite;
        private Sprite3D _coopPulseSprite;
        private Sprite3D _individualBaseSprite;
        private Sprite3D _individualPulseSprite;
        private SpriteMap3D _playerIdentifierSpriteMap;

        private TeamScoreMeter _teamScoreMeter;
        private Sprite3D _iconSyncBaseSpriteMap;
        private readonly Color[] _pulseColors = {new Color(255,128,128),new Color(128,128,255), new Color(128,255,128),new Color(255,255,128)    };

        public ScoreSet(MetricsManager metrics, Player[] players, GameType type)
            : base(metrics,players,type)
        {
            _displayedScores = new double[4];
            InitSprites();
        }

        public override void Draw()
        {
            AdjustDisplayedScores();
            DrawIndividualScores();
            DrawCombinedScores();
            DrawPlayerDifficulties();

        }

        private void InitSprites()
        {
            _iconSpriteMap = new SpriteMap3D
            {
                Columns = 1,
                Rows = (int)Difficulty.Count + 1,
                Texture = TextureManager.Textures("PlayerDifficulties")
            };

            _iconSyncBaseSpriteMap = new Sprite3D
                                         {
                                             Texture = TextureManager.Textures("SyncDifficultyBar"),
                                             Position = _metrics["SyncPlayerDifficultiesBase", 0],
                                             Size = _metrics["SyncPlayerDifficultiesBase.Size",0]
                                         };
            _coopBaseSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("ScoreBaseCombined"),
                Size = _metrics["ScoreBaseCombined.Size", 0]
            };
            _teamScoreMeter = new TeamScoreMeter { Size = _metrics["TeamScoreBase.Size", 0] };
            _teamScoreMeter.InitSprites();
            _playerIdentifierSpriteMap = new SpriteMap3D
            {
                Texture = TextureManager.Textures("PlayerIdentifiers"),
                Columns = 1,
                Rows = 5
            };
            _individualBaseSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("ScoreBase"),
                Size =  _metrics["ScoreBase.Size", 0]
            };
            _individualPulseSprite = new Sprite3D
                                         {
                                             Texture = TextureManager.Textures("ScorePulse"),
                                             Size = _metrics["ScoreBase.Size", 0]
                                         };
            _coopPulseSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("ScorePulse"),
                Size = _metrics["ScoreBaseCombined.Size", 0]
            };
        }
        private void DrawPlayerDifficulties()
        {
   
            if (SyncGameType)
            {
                return;
            }
            for (int x = 0; x < 4; x++)
            {
                if (! Players[x].Playing)
                {
                    continue;
                }
                int idx = 1 + (int)(Players[x].PlayerOptions.PlayDifficulty);
 
                _iconSpriteMap.Draw( idx, 30, 30, _metrics["GameScreenPlayerDifficulties", x]);
            }
        }

        private void DrawPlayerDifficultiesSync()
        {

            var idx = (from e in Players where e.Playing select e.PlayerOptions.PlayDifficulty).Min() + 1;
                    _iconSyncBaseSpriteMap.Draw();
                    _iconSpriteMap.Draw( (int) idx, _metrics["SyncPlayerDifficulties.Size",0], _metrics["SyncPlayerDifficulties", 0]);                           
        }

        private void DrawCombinedScores()
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                    return;
                case GameType.COOPERATIVE:
                    DrawCoopCombinedScore();
                    break;
                case GameType.TEAM:
                case GameType.VS_CPU:
                    DrawTeamCombinedScores();
                    break;
                    case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
                    DrawSyncCombinedScore();
                    break;
            }
        }

        private void DrawSyncCombinedScore()
        {
            DrawPlayerDifficultiesSync();
            var scoreText = _displayedScores[0];


            _coopBaseSprite.Position = _metrics["SyncScoreBase", 0];
            _coopBaseSprite.Size = _metrics["SyncScoreBase.Size", 0];
                    _coopBaseSprite.Draw();
                    _coopPulseSprite.Position = _metrics["SyncScoreBase", 0];
            _coopPulseSprite.Size = _metrics["SyncScoreBase.Size", 0];
                    _coopPulseSprite.ColorShading.A = (byte)Math.Min(255, 2 * Math.Sqrt(Players[0].Score - scoreText));
                    _coopPulseSprite.Draw();
                    FontManager.DrawString("" + Math.Ceiling(scoreText), "LargeFont",
                                           _metrics["SyncScoreText", 0], Color.White, FontAlign.Right);
                
            
        }

        private void DrawCoopCombinedScore()
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
                if ((!Players[2*x].Playing) && (!Players[(2*x) + 1].Playing))
                {
                    continue;
                }
                _coopBaseSprite.Position = _metrics["ScoreCombinedBase", x];
                _coopBaseSprite.Size = _metrics["ScoreCombinedBase.Size", 0];
                _coopBaseSprite.Draw();
                _coopPulseSprite.Position = _metrics["ScoreCombinedBase", x];
                _coopPulseSprite.Size = _metrics["ScoreCombinedBase.Size", 0];
                _coopPulseSprite.ColorShading.A = (byte) Math.Min(255, 2* Math.Sqrt(totalScore - scoreText));
                _coopPulseSprite.Draw();

                FontManager.DrawString("" + Math.Ceiling(scoreText), "LargeFont",
                                       _metrics["ScoreCombinedText", x], Color.White, FontAlign.Right);
            }
        }
        private void DrawTeamCombinedScores()
        {
            double blueScore = 0, redScore = 0;
            for (int x = 0; x < 4; x++)
            {
                if (!Players[x].Playing)
                {
                    continue;
                }
                if (Players[x].Team == 1)
                {
                    blueScore += _displayedScores[x];
                }
                else if (Players[x].Team == 2)
                {
                    redScore += _displayedScores[x];
                }
            }

            _teamScoreMeter.BlueScore = (long) Math.Ceiling(blueScore);
            _teamScoreMeter.RedScore = (long) Math.Ceiling(redScore);
            _teamScoreMeter.Size = _metrics["TeamScoreBase.Size", 0];

            if (Players[0].Playing || Players[1].Playing)
            {
                _teamScoreMeter.Position = (_metrics["TeamScoreBase", 0]);                 
                _teamScoreMeter.Draw();
            }
            if (Players[2].Playing || (Players[3].Playing))
            {
                _teamScoreMeter.Position = (_metrics["TeamScoreBase", 1]);
                _teamScoreMeter.Draw();
            }
            _teamScoreMeter.Update();

        }

        private void DrawIndividualScores()
        {
            if (SyncGameType)
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
                _individualBaseSprite.Size = _metrics["ScoreBase.Size", 0];
                _individualBaseSprite.Draw();
                _individualPulseSprite.Position = _metrics["ScoreBase", x];
                _individualPulseSprite.ColorShading = _pulseColors[x];
                _individualPulseSprite.ColorShading.A = (byte) Math.Min(255, 4* Math.Sqrt(Players[x].Score - _displayedScores[x]));
                _individualPulseSprite.Draw();
                var identifierPosition = _metrics["ScoreBase", x].Clone();
                identifierPosition.X += 12;
                identifierPosition.Y += 5;
                _playerIdentifierSpriteMap.Draw(idx,_metrics["MainGamePlayerIdentifiers.Size",0],identifierPosition);

                FontManager.DrawString("" + Math.Ceiling(_displayedScores[x]), "LargeFont",
                                      _metrics["ScoreText", x], Color.White,FontAlign.Right);
            }
        }

        private void AdjustDisplayedScores()
        {
            for (int x = 0; x < 4; x++)
            {

                var diff = Players[x].Score - _displayedScores[x];
             
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
                    Players[player].Score += amount;
                    break;
                case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
                    Players[player].Score += amount;
                    for (int x = 1; x < 4; x++ )
                    {
                        Players[x].Score = Players[0].Score;
                    }
                        break;
            }
        }
    }
}
