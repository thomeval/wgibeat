using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Managers;

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
        private Sprite _headerSprite;
        private Sprite _maxSprite;
        private Sprite _recordSprite;
        private Sprite _gradeBaseSprite;
        private LifeGraph _lifeGraph;
        private double _lastCycle;
        private TeamScoreMeter _teamScoreMeter;
        private Sprite _coopScoreDisplay;

        public EvaluationScreen(GameCore core) : base(core)
        {
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
                                 LineDrawer = new PrimitiveLine(Core.GraphicsDevice),
                                 Location = -1
                             };
            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    _lifeGraph[x] = Core.Players[x].LifeHistory.ToArray();
                }
                else
                {
                    _lifeGraph.Location = x;
                }
            }

            if (_lifeGraph.Location > -1)
            {
                _lifeGraph.Position = (Core.Metrics["LifeGraph", _lifeGraph.Location]);
            }
            _teamScoreMeter = new TeamScoreMeter
                                  {
                                      Position = (Core.Metrics["EvaluationTeamScoreMeter", 0])
                                  };
            _teamScoreMeter.InitSprites();
        }

        private void InitSprites()
        {

            _headerSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("EvaluationHeader")
            };
            _background = new Sprite
                              {
                                  Height = Core.Window.ClientBounds.Height,
                                  Width = Core.Window.ClientBounds.Width,
                                  SpriteTexture = TextureManager.Textures("AllBackground")
                              };

            _maxSprite = new Sprite
            {
                Width = 160,
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
            }
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
            DrawBackground(spriteBatch);
            DrawBorders(spriteBatch);
            DrawJudgementLines(spriteBatch);
            DrawMax(spriteBatch);
            DrawGrades(spriteBatch);
            DrawModeSpecific(spriteBatch);
            DrawMisc(spriteBatch,gameTime);
            DrawGraphs(spriteBatch,gameTime);
        }


        private void DrawGraphs(SpriteBatch spriteBatch, GameTime time)
        {

            if (time.TotalRealTime.TotalSeconds - _lastCycle > 1)
            {
                _lastCycle = time.TotalRealTime.TotalSeconds;
                _lifeGraph.CycleTopLine();
            }
            if (_lifeGraph.Location > -1)
            {
                _lifeGraph.Draw(spriteBatch);
            }
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch);
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
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                _headerSprite.Position = (Core.Metrics["EvaluationHeader", x]);
                _headerSprite.Draw(spriteBatch);
            }

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

                spriteBatch.DrawString(TextureManager.Fonts("LargeFont"), "" + Core.Players[x].MaxHits,
Core.Metrics["EvaluationMaxHits", x], Color.Black);
                spriteBatch.DrawString(TextureManager.Fonts("LargeFont"), "" + Core.Players[x].MaxStreak,
 Core.Metrics["EvaluationMaxStreak", x], Color.Black);
            }
        }

        private void DrawJudgementLines(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                int y = 0;
                foreach (string line in _lines)
                {
                    spriteBatch.DrawString(TextureManager.Fonts("LargeFont"), line + ":",
                                           Core.Metrics["EvaluationLabel" + line, x], Color.Black);
                    spriteBatch.DrawString(TextureManager.Fonts("LargeFont"), "" + Core.Players[x].Judgements[y],
                                           Core.Metrics["Evaluation" + line, x], Color.Black);
                    y++;
                }

                spriteBatch.DrawString(TextureManager.Fonts("LargeFont"), "Score:",
                                       Core.Metrics["EvaluationLabelScore", x], Color.Black);
                spriteBatch.DrawString(TextureManager.Fonts("LargeFont"), "" + Core.Players[x].Score,
                                       Core.Metrics["EvaluationScore", x], Color.Black);
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

        private void DrawBorders(SpriteBatch spriteBatch)
        {
            var brush = new PrimitiveLine(Core.GraphicsDevice) { Colour = Color.Black };
            brush.AddVector(new Vector2(400, 0));
            brush.AddVector(new Vector2(400, 600));
            brush.Render(spriteBatch);
            brush.ClearVectors();
            brush.AddVector(new Vector2(0, 275));
            brush.AddVector(new Vector2(800, 275));
            brush.Render(spriteBatch);
            brush.ClearVectors();
            brush.AddVector(new Vector2(0, 325));
            brush.AddVector(new Vector2(800, 325));
            brush.Render(spriteBatch);
            brush.ClearVectors();
        }

        #endregion

    }
}
