using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame1.Drawing;

namespace WindowsGame1.Screens
{
    public class EvaluationScreen : GameScreen
    {
        private string[] _lines = {"Ideal","Cool","Ok","Bad","Fail","Fault","Miss"};
        private const int NUM_EVALUATIONS = 8;

        public EvaluationScreen(GameCore core) : base(core)
        {
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBorders(spriteBatch);

           for (int x = 0; x < Core.Players.Count(); x++)
           {
               if (!Core.Players[x].Playing)
               {
                   continue;
               }
               int y = 0;
               foreach (string line in _lines)
               {
                   spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], line + ":",
                                          Core.Metrics["EvaluationLabel" + line,x],Color.White);
                   spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + Core.Players[x].Judgements[y],
                       Core.Metrics["Evaluation" + line, x], Color.White);
                   y++;
               }

               spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Score:",
    Core.Metrics["EvaluationLabelScore", x], Color.White);
               spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + Core.Players[x].Score,
                   Core.Metrics["EvaluationScore", x], Color.White);

               var headerSprite = new Sprite
                                      {
                                          Height = 30,
                                          Width = 250,
                                          SpriteTexture = TextureManager.Textures["evaluationHeader"]
                                      };
               headerSprite.SetPosition(Core.Metrics["EvaluationHeader",x]);
               headerSprite.Draw(spriteBatch);


           }

            DrawGrades(spriteBatch);
            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Press Start to continue.",
                                   Core.Metrics["EvaluationInstruction", 0], Color.White);
        }

        private void DrawGrades(SpriteBatch spriteBatch)
        {
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

            if (percentage >= 95)
            {
                return 0;
            }
            if (percentage >= 90)
            {
                //S
                return 1;
            }
            if (percentage >= 75)
            {
                //A
                return 2;
            }
            if (percentage >= 60)
            {
                //B
                return 3;
            }
            if (percentage >= 45)
            {
                //C
                return 4;
            }
            if (percentage >= 30)
            {
                //D
                return 5;
            }
            //E
            return 6;
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
