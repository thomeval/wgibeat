using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Notes;

namespace WGiBeat.Screens
{
    public class EvaluationScreen : GameScreen
    {
        private readonly string[] _lines = {"Ideal","Cool","Ok","Bad","Fail","Miss","Fault"};
        private readonly int[] _evaluationCutoffs = {95, 90, 86, 82, 78, 75, 70, 65, 60, 55, 50, 45, 40, 35, 30, 25,20};
        private readonly int[] _grades = {0, 0, 0, 0};
        private int _highScorePlayer;
        private const int NUM_EVALUATIONS = 19;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Sprite _background;
        private SpriteMap _gradeSpriteMap;
        private Sprite _maxSprite;
        private Sprite _recordSprite;
        private Sprite _gradeBaseSprite;
        private LifeGraph _lifeGraph;
        private double _lastCycle;
        private TeamScoreMeter _teamScoreMeter;
        private Sprite _coopScoreDisplay;
        private readonly ProfileLevelDisplay[] _profileLevelDisplays;
        private readonly long[] _xpAwarded;
        private PlayerOptionsSet _playerOptionsSet;
        private List<RoundLine> _lineList;


        public EvaluationScreen(GameCore core) : base(core)
        {
            _profileLevelDisplays = new ProfileLevelDisplay[4];
            _xpAwarded = new long[4];
            
        }

        #region Overrides
        public override void Initialize()
        {
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

                                 Position = new Vector2(-1000,-1000),
                                 CPUPlayerID = GetCPUPlayerID()
                             };
            for (int x = 0; x < 4; x++)
            {
                _profileLevelDisplays[x] = new ProfileLevelDisplay
                                               {
                                                   Player = Core.Players[x],
                                                   Width = 400,
                                                   Position = Core.Metrics["EvaluationLevelDisplay",x]
                                               };
                if (Core.Players[x].Playing)
                {
                    _lifeGraph[x] = Core.Players[x].LifeHistory.ToArray();       
                }
                else
                {
                    _profileLevelDisplays[x].Player = null;
                    _lifeGraph.Position = Core.Metrics["LifeGraph", x];
                }
            }

    
            _teamScoreMeter = new TeamScoreMeter
                                  {
                                      Position = (Core.Metrics["EvaluationTeamScoreMeter", 0])
                                  };
            _teamScoreMeter.InitSprites();

            _playerOptionsSet = new PlayerOptionsSet
                                    {Players = Core.Players, Positions = Core.Metrics["EvaluationPlayerOptionsFrames"], DrawAttract = false};
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

          
            _background = new Sprite
                              {
                                  Height = 600,
                                  Width = 800,
                                  SpriteTexture = TextureManager.Textures("AllBackground")
                              };

            _maxSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("EvaluationMaxBase")
            };
            _recordSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("EvaluationHighScore"),
                Height = 25,
                Width = 130
            };
            _gradeBaseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("EvaluationGradeBase")
            };

            _gradeSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = NUM_EVALUATIONS,
                SpriteTexture = TextureManager.Textures("EvaluationGrades")
            };
            _coopScoreDisplay = new Sprite
                                    {
                                        SpriteTexture = TextureManager.Textures("ScoreBaseCombined"),
                                        Position = Core.Metrics["EvaluationTeamScoreMeter",0]
                                    };
       
        }

        private void SaveHighScore()
        {
            //Evaluation screen needs this setting to be able to display the high score indicator.
            _highScorePlayer = Core.HighScores.UpdateHighScore(Core.Settings.Get<int>("LastSongPlayed"), Core.Players, (GameType)Core.Cookies["CurrentGameType"], _grades);
            Core.HighScores.SaveToFile("Scores.conf");
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
                Core.Players[x].UpdateToProfile();
                Core.Profiles.SaveToFolder(Core.WgibeatRootFolder + "\\" + Core.Settings["ProfileFolder"]);
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
                    for (int x = 0; x < 4; x++ )
                    {
                        _grades[x] = CalculateGradeIndex(x);
                    }
                        break;
                case GameType.COOPERATIVE:
                    _grades[0] = PercentageToGradeIndex(CalculateTeamPercentage());
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
            for (int x = 0; x < 4; x++ )
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
            DrawBackground(spriteBatch, gameTime);
            DrawBorders();
            DrawJudgementLines(spriteBatch);
            DrawMax(spriteBatch);
            DrawGrades(spriteBatch);
            DrawModeSpecific(spriteBatch);
            DrawMisc(spriteBatch,gameTime);
            DrawGraphs(spriteBatch,gameTime);
            DrawEXPGains(spriteBatch);
        }

        private void DrawEXPGains(SpriteBatch spriteBatch)
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
                 _profileLevelDisplays[x].Draw(spriteBatch);

                 TextureManager.DrawString(spriteBatch,String.Format("+{0} EXP",_xpAwarded[x]),"LargeFont",Core.Metrics["EvaluationEXPAwarded",x],Color.Black,FontAlign.CENTER);
             }
        }

        private void DrawGraphs(SpriteBatch spriteBatch, GameTime time)
        {

            if (time.TotalRealTime.TotalSeconds - _lastCycle > 1)
            {
                _lastCycle = time.TotalRealTime.TotalSeconds;
                _lifeGraph.CycleTopLine();
            }
            if (_lifeGraph.X > -100)
            {
                _lifeGraph.Draw(spriteBatch);
            }
        }

        private void DrawBackground(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _background.Draw(spriteBatch);
            _field.Draw(gameTime);
        }

        private void DrawModeSpecific(SpriteBatch spriteBatch)
        {
            switch ((GameType)Core.Cookies["CurrentGameType"])
           {
               case GameType.NORMAL:
                   break;
               case GameType.COOPERATIVE:
                   var totalScore = (from e in Core.Players where e.Playing select e.Score).Sum();
                   _coopScoreDisplay.Draw(spriteBatch);
                   TextureManager.DrawString(spriteBatch, "" + totalScore,"LargeFont",
                                   Core.Metrics["EvaluationTotalScore", 0], Color.White,FontAlign.RIGHT);

                       int gradeIndex = _grades[0];
                       _gradeSpriteMap.Draw(spriteBatch, gradeIndex, 150, 52, Core.Metrics["EvaluationTotalGrade", 0]);

                   break;
                    case GameType.TEAM:
                    case GameType.VS_CPU:
                   var teamAScore = (from e in Core.Players where (e.Playing && e.Team == 1) select e.Score).Sum();
                   var teamBScore = (from e in Core.Players where (e.Playing && e.Team == 2) select e.Score).Sum();
                   _teamScoreMeter.BlueScore = teamAScore;
                   _teamScoreMeter.RedScore = teamBScore;
                   _teamScoreMeter.Draw(spriteBatch);
                   _teamScoreMeter.Update();
                   break;
           }
        }

        private void DrawMisc(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _playerOptionsSet.Draw(spriteBatch);
            /*
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                
                
                var idx = Core.Players[x].IsCPUPlayer ? 4 : x;
                _headerSprite.Position = (Core.Metrics["EvaluationHeader", x]);
                _headerSprite.Draw(spriteBatch);
                _identifiersSpriteMap.Draw(spriteBatch,idx,55,30,_headerSprite.X + 10, _headerSprite.Y);
                 
            }
            */
            DrawHighScoreNotification(spriteBatch, gameTime);
            TextureManager.DrawString(spriteBatch, "Press Start to continue.","LargeFont",
                                   Core.Metrics["EvaluationInstruction", 0], Color.Black,FontAlign.LEFT);
        }

        private void DrawMax(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }

                _maxSprite.Position = (Core.Metrics["EvaluationMaxBase", x]);
                _maxSprite.Draw(spriteBatch);

                TextureManager.DrawString(spriteBatch, "" + Core.Players[x].MaxHits, "LargeFont",
Core.Metrics["EvaluationMaxHits", x], Color.Black, FontAlign.CENTER);
                TextureManager.DrawString(spriteBatch, "" + Core.Players[x].MaxStreak, "LargeFont",
 Core.Metrics["EvaluationMaxStreak", x], Color.Black, FontAlign.CENTER);
            }
        }

        private void DrawJudgementLines(SpriteBatch spriteBatch)
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
                var totalBeatlines = Core.Players[x].Judgements.Sum() - Core.Players[x].Judgements[(int)BeatlineNoteJudgement.COUNT];

                foreach (string line in _lines)
                {

                    TextureManager.DrawString(spriteBatch, line + ":", "DefaultFont",
                                           labelPosition, Color.Black, FontAlign.LEFT);
                    TextureManager.DrawString(spriteBatch, "" + Core.Players[x].Judgements[y], "DefaultFont",
                                           valuePosition, Color.Black, FontAlign.LEFT);
                    if (line != "Fault")
                    {
                        TextureManager.DrawString(spriteBatch,
                                                  String.Format("{0:P0}",
                                                                1.0*Core.Players[x].Judgements[y]/totalBeatlines),
                                                  "DefaultFont",
                                                  percentagePosition, Color.Black, FontAlign.RIGHT);
                    }
                    y++;
                    labelPosition.Y += 18;
                    valuePosition.Y += 18;
                    percentagePosition.Y += 18;

                }

                TextureManager.DrawString(spriteBatch, "Score:", "LargeFont",
                                       Core.Metrics["EvaluationLabelScore", x], Color.Black, FontAlign.LEFT);
                TextureManager.DrawString(spriteBatch, "" + Core.Players[x].Score, "LargeFont",
                                       Core.Metrics["EvaluationScore", x], Color.Black, FontAlign.RIGHT);
            }
        }

        private void DrawHighScoreNotification(SpriteBatch spriteBatch, GameTime gameTime)
        {
                if (_highScorePlayer != -1)
                {
                    _recordSprite.ColorShading.A = (byte) (255*Math.Abs(Math.Sin(gameTime.TotalRealTime.TotalSeconds * 2)));
                    _recordSprite.Position = (Core.Metrics["EvaluationHighScore", _highScorePlayer]);
                    _recordSprite.Draw(spriteBatch);
                }
        }

        private void DrawGrades(SpriteBatch spriteBatch)
        {

            for (int x = 0; x < Core.Players.Count(); x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }

                _gradeBaseSprite.Position = (Core.Metrics["EvaluationGradeBase", x]);
                _gradeBaseSprite.Draw(spriteBatch);
                var gradeIndex = CalculateGradeIndex(x);
                _gradeSpriteMap.Draw(spriteBatch, gradeIndex, 150, 52, Core.Metrics["EvaluationGrade",x]);
            }
        }

        private void DrawBorders()
        {
            
           if (_lineList == null)
           {
               _lineList = new List<RoundLine>();
               _lineList.Add(new RoundLine(400,0,400,600));
               _lineList.Add(new RoundLine(0,275,800,275));
               _lineList.Add(new RoundLine(0,325,800,325));
           }
            RoundLineManager.Instance.Draw(_lineList,1,Color.Black);
            
        }

        #endregion

    }
}
