using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;

namespace WGiBeat.Screens
{
    public class EvaluationScreen : GameScreen
    {
        private string[] _lines = {"Ideal","Cool","Ok","Bad","Fail","Fault","Miss"};
        private int[] _evaluationCutoffs = {96, 92, 88, 85, 80, 75, 70, 65, 60, 55, 50, 45, 40, 35, 30, 25,20};
        private const int NUM_EVALUATIONS = 19;

        public EvaluationScreen(GameCore core) : base(core)
        {
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBorders(spriteBatch);
            DrawJudgementLines(spriteBatch);
            DrawMax(spriteBatch);
            DrawGrades(spriteBatch);
            DrawModeSpecific(spriteBatch);
            DrawMisc(spriteBatch,gameTime);


        }

        private void DrawModeSpecific(SpriteBatch spriteBatch)
        {
           switch (Core.Settings.Get<GameType>("CurrentGameType"))
           {
               case GameType.NORMAL:
                   break;
               case GameType.COOPERATIVE:
                   var totalScore = (from e in Core.Players where e.Playing select e.Score).Sum();
                   spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "Team:",
                                   Core.Metrics["EvaluationLabelTotalScore", 0], Color.White);
                   spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + totalScore,
                                   Core.Metrics["EvaluationTotalScore", 0], Color.White);
                   break;
           }
        }

        private void DrawMisc(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var headerSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures["evaluationHeader"]
            };

            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                headerSprite.SetPosition(Core.Metrics["EvaluationHeader", x]);
                headerSprite.Draw(spriteBatch);
            }


            DrawHighScoreNotification(spriteBatch, gameTime);
            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Press Start to continue.",
                                   Core.Metrics["EvaluationInstruction", 0], Color.White);
        }



        private void DrawMax(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }

                var maxSprite = new Sprite
                {

                    Width = 160,
                    SpriteTexture = TextureManager.Textures["evaluationMaxBase"]
                };
                maxSprite.SetPosition(Core.Metrics["EvaluationMaxBase", x]);
                maxSprite.Draw(spriteBatch);

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + Core.Players[x].MaxHits,
Core.Metrics["EvaluationMaxHits", x], Color.Black);
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + Core.Players[x].MaxStreak,
 Core.Metrics["EvaluationMaxStreak", x], Color.White);
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
                    spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], line + ":",
                                           Core.Metrics["EvaluationLabel" + line, x], Color.White);
                    spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + Core.Players[x].Judgements[y],
                                           Core.Metrics["Evaluation" + line, x], Color.White);
                    y++;
                }

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Score:",
                                       Core.Metrics["EvaluationLabelScore", x], Color.White);
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + Core.Players[x].Score,
                                       Core.Metrics["EvaluationScore", x], Color.White);
            }
        }

        private void DrawHighScoreNotification(SpriteBatch spriteBatch, GameTime gameTime)
        {

                if (Core.Settings.Get<int>("HighScorePlayer") != -1)
                {
                    var recordSprite = new Sprite
                                           {
                                               SpriteTexture = TextureManager.Textures["evaluationHighScore"],
                                               Height = 25,
                                               Width = 130
                                           };

                    recordSprite.ColorShading.A = (byte) (255*Math.Abs(Math.Sin(gameTime.TotalRealTime.TotalSeconds * 2)));
                    recordSprite.SetPosition(Core.Metrics["EvaluationHighScore", Core.Settings.Get<int>("HighScorePlayer")]);
                    recordSprite.Draw(spriteBatch);
                }
        }

        private void DrawGrades(SpriteBatch spriteBatch)
        {

            var gradeBaseSprite = new Sprite
            {
                Height = 90,
                Width = 160,
                SpriteTexture = TextureManager.Textures["evaluationGradeBase"]
            };


            var gradeSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = NUM_EVALUATIONS,
                SpriteTexture = TextureManager.Textures["evaluationGrades"]
            };

            for (int x = 0; x < Core.Players.Count(); x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                gradeBaseSprite.SetPosition(Core.Metrics["EvaluationGradeBase", x]);
                gradeBaseSprite.Draw(spriteBatch);

                int gradeIndex = CalculateGradeIndex(x);

                gradeSpriteMap.Draw(spriteBatch, gradeIndex, 150, 52, Core.Metrics["EvaluationGrade",x]);
            }
        }

        private int CalculateGradeIndex(int player)
        {
            if (Core.Players[player].KO)
            {
                //Fail
                return NUM_EVALUATIONS - 1;
            }
            double percentage = CalculatePercentage(player);

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

        private double CalculatePercentage(int playerindex)
        {
            int[] judgements = Core.Players[playerindex].Judgements;

            // Ideal + Cool + OK + Bad + Fail + Miss
            int maxPossible = judgements[0] + judgements[1] + judgements[2] + judgements[3] + judgements[4] +
                              judgements[6];
            maxPossible *= 8;

            //Ideals
            int playerScore = judgements[0]*8;
            //Cools
            playerScore += judgements[1]*6;
            //OKs
            playerScore += judgements[2]*3;
            //Bads
            playerScore += judgements[3];
            //Fails
            playerScore += judgements[4]*-4;
            //Faults
            playerScore += judgements[5]*-1;

            return 100.0*playerScore/maxPossible;
        }

        private double CalculateTeamPercentage()
        {
            double totalPerc = 0;
            int participants = 0;
            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    totalPerc += CalculatePercentage(x);
                    participants += 1;
                }
            }

            return totalPerc/participants;

    }
        public override void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.P1_START:
                case Action.P2_START:
                case Action.P3_START:
                case Action.P4_START:
                case Action.SYSTEM_BACK:
                    Core.Songs.StopSong();
                    Core.Settings.SaveToFile("settings.txt");
                    Core.ScreenTransition("SongSelect");
                    break;
            }
        }



        private void DrawBorders(SpriteBatch spriteBatch)
        {
            var brush = new PrimitiveLine(Core.GraphicsDevice) { Colour = Color.White };
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
    }
}
