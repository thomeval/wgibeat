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
                   return;
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

            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Press Start to continue.",
                                   Core.Metrics["EvaluationInstruction", 0], Color.White);
        }

        public override void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.P1_START:
                case Action.P2_START:
                case Action.P3_START:
                case Action.P4_START:
                    Core.Songs.StopSong();
                    Core.ScreenTransition("MainMenu");
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
