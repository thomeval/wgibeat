using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Screens
{
    public class EvaluationScreen : GameScreen
    {
        private readonly string[] _lines = { "Ideal", "Cool", "Ok", "Bad", "Fail", "Miss", "Fault" };
        private readonly int[] _evaluationCutoffs = { 95, 90, 86, 82, 78, 75, 70, 65, 60, 55, 50, 45, 40, 35, 30, 25, 20 };
        private readonly int[] _grades = { 0, 0, 0, 0 };
        private int _highScorePlayer;
        private const int NUM_EVALUATIONS = 19;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Sprite3D _background;
        private SpriteMap3D _gradeSpriteMap;
        private Sprite3D _maxSprite;
        private Sprite3D _recordSprite;
        private Sprite3D _gradeBaseSprite;
        private LifeGraph _lifeGraph;
        private double _lastCycle;
        private TeamScoreMeter _teamScoreMeter;
        private Sprite3D _coopScoreDisplay;
        private readonly ProfileLevelDisplay[] _profileLevelDisplays;
        private readonly long[] _xpAwarded;
        private bool[] _levelledUp;
        private PlayerOptionsSet _playerOptionsSet;
        private List<RoundLine> _lineList;


        public EvaluationScreen(GameCore core)
            : base(core)
        {
            _profileLevelDisplays = new ProfileLevelDisplay[4];
            _xpAwarded = new long[4];
            _levelledUp = new bool[4];
        }

        #region Overrides
        public override void Initialize()
        {
            _levelledUp = new bool[4];
            CalculateGrades();
            SaveHighScore();
            SaveProfiles();
            InitSprites();
            InitObjects();
            base.Initialize();
        }

        private void InitObjects()
        {
            _lifeGraph = new LifeGraph
                             {

                                 Position = new Vector2(-1000, -1000),
                                 CPUPlayerID = GetCPUPlayerID(),
                                 Size = Core.Metrics["LifeGraph.Size", 0]
                             };
            SetGraphData();
            for (int x = 0; x < 4; x++)
            {
                _profileLevelDisplays[x] = new ProfileLevelDisplay
                                               {
                                                   Player = Core.Players[x],
                                                   Width = GameCore.INTERNAL_WIDTH / 2,
                                                   Position = Core.Metrics["EvaluationLevelDisplay", x]
                                               };

                if (Core.Players[x].Playing)
                {
                    continue;
                }
                _profileLevelDisplays[x].Player = null;
                _lifeGraph.Position = Core.Metrics["LifeGraph", x];
            }


            _teamScoreMeter = new TeamScoreMeter
                                  {
                                      Position = (Core.Metrics["EvaluationTeamScoreMeter", 0]),
                                      Size = Core.Metrics["EvaluationTeamScoreMeter.Size",0]
                                  };
            _teamScoreMeter.InitSprites();

            _playerOptionsSet = new PlayerOptionsSet { Players = Core.Players, Positions = Core.Metrics["EvaluationPlayerOptionsFrames"], Size = Core.Metrics["PlayerOptionsFrame.Size",0], DrawAttract = false };
            _playerOptionsSet.CreatePlayerOptionsFrames();
        }

        private int GetCPUPlayerID()
        {

            for (var x = 0; x < 4; x++)
            {
                if (Core.Players[x].IsCPUPlayer)
                {
                    return x;
                }
            }
            return -1;
        }

        private void InitSprites()
        {


            _background = new Sprite3D
                              {
                                  Size = Core.Metrics["ScreenBackground.Size", 0],
                                  Position = Core.Metrics["ScreenBackground", 0],
                                  Texture = TextureManager.Textures("AllBackground")
                              };

            _maxSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("EvaluationMaxBase")
            };
            _recordSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("EvaluationHighScore"),
                Height = 25,
                Width = 130
            };
            _gradeBaseSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("EvaluationGradeBase")
            };

            _gradeSpriteMap = new SpriteMap3D
            {
                Columns = 1,
                Rows = NUM_EVALUATIONS,
                Texture = TextureManager.Textures("EvaluationGrades")
            };
            _coopScoreDisplay = new Sprite3D
                                    {
                                        Texture = TextureManager.Textures("ScoreBaseCombined"),
                                        Position = Core.Metrics["EvaluationTeamScoreMeter", 0],
                                        Size = Core.Metrics["EvaluationTeamScoreMeter.Size",0]
                                    };

        }

        private void SaveHighScore()
        {
            //Evaluation screen needs this setting to be able to display the high score indicator.
            _highScorePlayer = Core.HighScores.UpdateHighScore(Core.Settings.Get<int>("LastSongPlayed"), Core.Players, (GameType)Core.Cookies["CurrentGameType"], _grades);
            Core.HighScores.SaveToFile("Scores.conf");
            SaveRecordReplay();

        }

        private void SaveRecordReplay()
        {
            if (_highScorePlayer == -1)
            {
                return;
            }
            List<long> scores;
            switch (_highScorePlayer)
            {
                case 4:
                    scores = (from e in CombineScoreData(0) select (long) e).ToList();
                    break;
                case 5:
                    scores = Core.Players[0].ScoreHistory;
                    break;
                default:
                    scores = Core.Players[_highScorePlayer].ScoreHistory;
                    break;

            }
            RecordReplayer.SaveRecord(scores, Core.Settings.Get<int>("LastSongPlayed"), HighScoreManager.TranslateGameType((GameType)Core.Cookies["CurrentGameType"]));
        }

        private void SaveProfiles()
        {
        
            for (int x = 0; x < 4; x++)
            {
                _xpAwarded[x] = Core.Players[x].AwardXP();

                
                if (Core.Players[x].Profile == null)
                {
                    continue;
                }
                if (Core.Players[x].KO)
                {
                    Core.Players[x].Profile.SongsFailed++;
                }
                else
                {
                    Core.Players[x].Profile.SongsCleared++;
                }
                
                //Is the player's current EXP plus the amount just earned more than the next level threshhold? But not the player's current EXP only?
                _levelledUp[x] = Core.Players[x].GetNextEXPSafe() <= Core.Players[x].GetEXP() + _xpAwarded[x] && Core.Players[x].GetEXP() < Core.Players[x].GetNextEXPSafe();
    
                Core.Players[x].UpdateToProfile();
                Core.Profiles.SaveToFolder(Core.WgibeatRootFolder + "\\" + Core.Settings["ProfileFolder"]);

            }

            if (_levelledUp.Any(e => e))
            {
                Core.Sounds.PlaySoundEffect(SoundEvent.PLAYER_LEVEL_UP);
            }
        }

        public override void PerformAction(InputAction inputAction)
        {
            switch (inputAction.Action)
            {
                case "START":
                case "BACK":
                    Core.Songs.StopCurrentSong();
                    Core.Settings.SaveToFile("settings.txt");
                    Core.ScreenTransition("SongSelect");
                    break;
                case "BEATLINE":
                    _graphMode = (_graphMode + 1) % 3;
                    SetGraphData();
                    break;
            }
        }
        #endregion

        #region Calculations
        private void CalculateGrades()
        {
            switch ((GameType)Core.Cookies["CurrentGameType"])
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                case GameType.VS_CPU:
                    for (int x = 0; x < 4; x++)
                    {
                        _grades[x] = CalculateGradeIndex(x);
                    }
                    break;
                case GameType.COOPERATIVE:
                    if ((from e in Core.Players where e.Playing && e.KO select e).
                    Any())
                    {
                        //Fail
                        _grades[0] = NUM_EVALUATIONS - 1;
                        
                    }
                    else
                    {
                        _grades[0] = PercentageToGradeIndex(CalculateTeamPercentage());  
                    }
                    
                    break;
                case GameType.SYNC_PRO:
                case GameType.SYNC_PLUS:
                    for (int x = 0; x < 4; x++)
                    {
                        _grades[x] = GetSyncGradeIndex();
                    }
                    break;
            }
        }

        private int GetSyncGradeIndex()
        {
            var result = 0;
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].IsHumanPlayer)
                {
                    continue;
                }
                if (Core.Players[x].KO)
                {
                    return NUM_EVALUATIONS - 1;
                }

                int idx = PercentageToGradeIndex(Core.Players[x].CalculatePercentage());

                result = Math.Max(result, idx);
            }

            return result;
        }

        private int CalculateGradeIndex(int player)
        {
            if (Core.Players[player].KO)
            {
                //Fail
                return NUM_EVALUATIONS - 1;
            }
            double percentage = Core.Players[player].CalculatePercentage();

            return PercentageToGradeIndex(percentage);
        }

        private int PercentageToGradeIndex(double percentage)
        {
            for (int x = 0; x < _evaluationCutoffs.Count(); x++)
            {
                if (percentage >= _evaluationCutoffs[x])
                {
                    return x;
                }
            }
            return NUM_EVALUATIONS - 2;
        }


        private double CalculateTeamPercentage()
        {
            double totalPerc = 0;
            int participants = 0;
            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    totalPerc += Core.Players[x].CalculatePercentage();
                    participants += 1;
                }
            }

            return totalPerc / participants;

        }
        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(gameTime);
            DrawBorders();
            DrawJudgementLines();
            DrawMax();
            DrawGrades();
            DrawModeSpecific();
            DrawMisc( gameTime);
            DrawGraphs( gameTime);
            DrawEXPGains();
        }

        private void DrawEXPGains()
        {
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                if (Core.Players[x].Profile == null)
                {
                    continue;
                }
                _profileLevelDisplays[x].Draw();

                FontManager.DrawString(String.Format("+{0} EXP", _xpAwarded[x]), "LargeFont", Core.Metrics["EvaluationEXPAwarded", x], Color.Black, FontAlign.Center);
                
                if (_levelledUp[x])
                {
                    FontManager.DrawString("LEVEL UP!", "LargeFont", Core.Metrics["EvaluationLevelUpIndicator",x]);
                }
                
            }
        }

        private int _graphMode;
        private void DrawGraphs( GameTime time)
        {

            if (time.TotalRealTime.TotalSeconds - _lastCycle > 1)
            {
                _lastCycle = time.TotalRealTime.TotalSeconds;
                _lifeGraph.CycleTopLine();
            }

            if (_lifeGraph.X > -100)
            {

                _lifeGraph.Draw();
            }
        }

        private void SetGraphData()
        {
            for (int x = 0; x < 4; x++)
            {
             
                switch (_graphMode)
                {
                    case 0:
                        _lifeGraph[x] = Core.Players[x].Playing ? Core.Players[x].LifeHistory.ToArray(): null;
                        _lifeGraph.SetMinMaxTick(-100, 400, 50);
                        _lifeGraph.LegendStyle = LegendStyle.Normal;
                        break;
                    case 1:
                 
                        var highScore = GetHighestScore();
                        _lifeGraph.SetMinMaxTick(0, 15000000, GetScoreTickAmount(highScore));
                        SetScoreData();
                        //Note the Return statement. Don't do this case block for each player.
                        return;

                    case 2:
                        _lifeGraph[x] = Core.Players[x].Playing ? Core.Players[x].LevelHistory.ToArray() : null;
                        _lifeGraph.SetMinMaxTick(1, 15, 2);
                        _lifeGraph.LegendStyle = LegendStyle.Normal;
                        break;
                    case 3:
                        break;
                }
            }
        }

        private void SetScoreData()
        {
            switch ((GameType)Core.Cookies["CurrentGameType"])
            {
                case GameType.COOPERATIVE:
                    _lifeGraph[0] = CombineScoreData(0);
                    _lifeGraph[1] = _lifeGraph[2] = _lifeGraph[3] = null;
                    _lifeGraph.LegendStyle = LegendStyle.None;
                    
                    break;
                case GameType.TEAM:
                case GameType.VS_CPU:
                    _lifeGraph[0] = CombineScoreData(2);
                    _lifeGraph[1] = CombineScoreData(1);
                    _lifeGraph[2] = _lifeGraph[3] = null;
                    _lifeGraph.LegendStyle = LegendStyle.Teams;
                    break;
                default:
                    for (int x = 0; x < 4; x++)
                    {
                        _lifeGraph[x] = Core.Players[x].Playing ? (from e in Core.Players[x].ScoreHistory select (double)e).ToArray() : null;
                    }
                    break;
            }
        }

        private double[] CombineScoreData(int team)
        {
            var maxLength = (from e in Core.Players select e.LifeHistory.Count).Max();

            var result = new double[maxLength];
            for (var y = 0; y < maxLength; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    if ((!Core.Players[x].Playing) || (Core.Players[x].ScoreHistory.Count <= y) || Core.Players[x].Team != team)
                    {
                        continue;
                    }

                    result[y] += Core.Players[x].ScoreHistory[y];
                }
            }
            return result;
        }

        private long GetHighestScore()
        {
            switch ((GameType)Core.Cookies["CurrentGameType"])
            {
                case GameType.COOPERATIVE:
                    return (from e in Core.Players where e.Playing select e.Score).Sum();
                case GameType.TEAM:
                case GameType.VS_CPU:
                    var blueScore = (from e in Core.Players where e.Team == 1 && e.Playing select e.Score).Sum();
                    var redScore = (from e in Core.Players where e.Team == 2 && e.Playing select e.Score).Sum();
                    return Math.Max(blueScore, redScore);
                default:
                    return (from e in Core.Players where e.Playing select e.Score).Max();
            }
        }

        private long GetScoreTickAmount(long score)
        {
            if (score > 3000000)
            {
                return 1000000;
            }
            if (score > 1000000)
            {
                return 500000;
            }
            if (score > 500000)
            {
                return 250000;
            }
            if (score > 200000)
            {
                return 100000;
            }
            return 50000;
        }

        private void DrawBackground(GameTime gameTime)
        {
            _background.Draw();
            _field.Draw(gameTime);
        }

        private void DrawModeSpecific()
        {
            DrawModeSpecificPerPlayer();
            switch ((GameType)Core.Cookies["CurrentGameType"])
            {
                case GameType.NORMAL:
                   
                        break;
                case GameType.COOPERATIVE:
                    var totalScore = (from e in Core.Players where e.Playing select e.Score).Sum();
                    _coopScoreDisplay.Draw();
                    FontManager.DrawString("" + totalScore, "LargeFont",
                                    Core.Metrics["EvaluationTotalScore", 0], Color.White, FontAlign.Right);

                    int gradeIndex = _grades[0];
                    _gradeSpriteMap.Draw( gradeIndex, 150, 52, Core.Metrics["EvaluationTotalGrade", 0]);

                    break;
                case GameType.TEAM:
                case GameType.VS_CPU:
                    var teamAScore = (from e in Core.Players where (e.Playing && e.Team == 1) select e.Score).Sum();
                    var teamBScore = (from e in Core.Players where (e.Playing && e.Team == 2) select e.Score).Sum();
                    _teamScoreMeter.BlueScore = teamAScore;
                    _teamScoreMeter.RedScore = teamBScore;
                    _teamScoreMeter.Draw();
                    _teamScoreMeter.Update();
                    break;
            }
        }

        private void DrawModeSpecificPerPlayer()
        {
              var teamAScore = (from e in Core.Players where (e.Playing && e.Team == 1) select e.Score).Sum();
              var teamBScore = (from e in Core.Players where (e.Playing && e.Team == 2) select e.Score).Sum();
              for (int x = 0; x < 4; x++)
              {
                  if (!Core.Players[x].Playing)
                  {
                      continue;
                  }

                  switch ((GameType)Core.Cookies["CurrentGameType"])
                  {
                      case GameType.NORMAL:
                          FontManager.DrawString("Overall", "LargeFont", Core.Metrics["EvaluationLabelModeSpecific", x], Color.Black, FontAlign.Left);
                          FontManager.DrawString(string.Format("{0:0.0}%", Core.Players[x].CalculatePercentage()), "LargeFont", Core.Metrics["EvaluationModeSpecific", x], Color.Black, FontAlign.Right);
                          break;
                      case GameType.COOPERATIVE:
                          FontManager.DrawString("Groove", "LargeFont", Core.Metrics["EvaluationLabelModeSpecific", x], Color.Black, FontAlign.Left);
                          FontManager.DrawString(string.Format("{0:0.0}x", Player.PeakGrooveMomentum), "LargeFont", Core.Metrics["EvaluationModeSpecific", x], Color.Black, FontAlign.Right);
                          break;
                      case GameType.TEAM:
                      case GameType.VS_CPU:
                          if ((teamAScore > teamBScore) ^ Core.Players[x].Team == 1)
                          {
                              FontManager.DrawString("Defeat", "LargeFont",
                                                        Core.Metrics["EvaluationLabelModeSpecific", x], Color.Black,
                                                        FontAlign.Left);
                          }
                          else
                          {
                              FontManager.DrawString("Victory!", "LargeFont",
                                                        Core.Metrics["EvaluationLabelModeSpecific", x], Color.Black,
                                                        FontAlign.Left);
                          }
                          break;
                  } 
              }
        }

        private void DrawMisc( GameTime gameTime)
        {
            _playerOptionsSet.Draw();
       
            DrawHighScoreNotification(gameTime);
            FontManager.DrawString("Press Start to continue.", "LargeFont",
                                   Core.Metrics["EvaluationInstruction", 0], Color.Black, FontAlign.Left);
        }

        private void DrawMax()
        {
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }

                _maxSprite.Position = (Core.Metrics["EvaluationMaxBase", x]);
                _maxSprite.Draw();

                FontManager.DrawString("" + Core.Players[x].MaxHits, "LargeFont",
Core.Metrics["EvaluationMaxHits", x], Color.Black, FontAlign.Center);
                FontManager.DrawString("" + Core.Players[x].MaxStreak, "LargeFont",
 Core.Metrics["EvaluationMaxStreak", x], Color.Black, FontAlign.Center);
            }
        }

        private void DrawJudgementLines()
        {

            for (int x = 0; x < 4; x++)
            {
                var labelPosition = Core.Metrics["EvaluationLabelColumn", x].Clone();
                var valuePosition = Core.Metrics["EvaluationValueColumn", x].Clone();
                var percentagePosition = Core.Metrics["EvaluationPercentageColumn", x].Clone();

                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                int y = 0;
                var totalBeatlines = Core.Players[x].Judgements.Sum() - Core.Players[x].Judgements[(int)BeatlineNoteJudgement.Count];

                foreach (string line in _lines)
                {

                    FontManager.DrawString(line + ":", "DefaultFont",
                                           labelPosition, Color.Black, FontAlign.Left);
                    FontManager.DrawString("" + Core.Players[x].Judgements[y], "DefaultFont",
                                           valuePosition, Color.Black, FontAlign.Left);
                    if (line != "Fault")
                    {
                        FontManager.DrawString(String.Format("{0:P0}",
                                                                1.0 * Core.Players[x].Judgements[y] / totalBeatlines),
                                                  "DefaultFont",
                                                  percentagePosition, Color.Black, FontAlign.Right);
                    }
                    y++;
                    labelPosition.Y += 18;
                    valuePosition.Y += 18;
                    percentagePosition.Y += 18;

                }

                FontManager.DrawString("Score:", "LargeFont",
                                       Core.Metrics["EvaluationLabelScore", x], Color.Black, FontAlign.Left);
                FontManager.DrawString("" + Core.Players[x].Score, "LargeFont",
                                       Core.Metrics["EvaluationScore", x], Color.Black, FontAlign.Right);
            }
        }

        private void DrawHighScoreNotification(GameTime gameTime)
        {
            if (_highScorePlayer == -1)
            {
                return;
            }
            _recordSprite.ColorShading.A = (byte)(255 * Math.Abs(Math.Sin(gameTime.TotalRealTime.TotalSeconds * 2)));
            _recordSprite.Position = (Core.Metrics["EvaluationHighScore", _highScorePlayer]);
            _recordSprite.Draw();
        }

        private void DrawGrades()
        {

            for (int x = 0; x < Core.Players.Count(); x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }

                _gradeBaseSprite.Position = (Core.Metrics["EvaluationGradeBase", x]);
                _gradeBaseSprite.Draw();
                var gradeIndex = CalculateGradeIndex(x);
                _gradeSpriteMap.Draw( gradeIndex, 150, 52, Core.Metrics["EvaluationGrade", x]);
            }
        }

        private void DrawBorders()
        {

            if (_lineList == null)
            {
                _lineList = new List<RoundLine>();
                _lineList.Add(new RoundLine(GameCore.INTERNAL_WIDTH / 2, 0, GameCore.INTERNAL_WIDTH / 2, GameCore.INTERNAL_HEIGHT));
                _lineList.Add(new RoundLine(0, 330, GameCore.INTERNAL_WIDTH, 330));
                _lineList.Add(new RoundLine(0, 390, GameCore.INTERNAL_WIDTH, 390));
            }
            RoundLineManager.Instance.Draw(_lineList, 1, Color.Black);

        }

        #endregion

    }
}
