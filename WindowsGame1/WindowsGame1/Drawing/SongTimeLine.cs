using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class SongTimeLine : DrawableObject 
    {
        private SpriteMap _barParts;
        private SpriteMap _barEdges;

        public GameSong Song { get; set; }
        public double AudioEnd { get; set; }
        public SongTimeLine()
        {
            InitSprites();
        }

        private void InitSprites()
        {
            _barParts = new SpriteMap
                            {Columns = 1, Rows = 5, SpriteTexture = TextureManager.Textures("SongTimeLineParts")};
            _barEdges = new SpriteMap { Columns = 2, Rows = 1, SpriteTexture = TextureManager.Textures("SongTimeLineEdges") };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Song == null)
            {
                return;
            }
            DrawEdges(spriteBatch);
            DrawBars(spriteBatch);
            DrawLabels(spriteBatch);
        }

        private void DrawLabels(SpriteBatch spriteBatch)
        {

            var position = new Vector2(_labelPositions[0] + this.X + 10, this.Y);
          if (_labelPositions[0] != 0.0)
          {

              if (position.X < 50)
              {
                  position.X = 50;
              }        
              TextureManager.DrawString(spriteBatch, "AudioStart", "DefaultFont", position, Color.Black, FontAlign.CENTER);
          }
            position.X = (_labelPositions[1] + this.X + 10);
            position.Y = this.Y + 40;

            if (position.X < 35)
            {
                position.X = 35;
            }
            TextureManager.DrawString(spriteBatch, "Offset", "DefaultFont", position, Color.Black, FontAlign.CENTER);

            position.X = (_labelPositions[2] + this.X + 10);
            position.Y = this.Y;
          TextureManager.DrawString(spriteBatch, "Length", "DefaultFont", position, Color.Black, FontAlign.RIGHT);
        }

        private int _totalBarWidth;
        private int[] _labelPositions = new int[4];
        private void DrawBars(SpriteBatch spriteBatch)
        {
            var barHeight = this.Height - 40;
            var position = this.Position.Clone();
            position.X += 10;
            position.Y += 20;

            _totalBarWidth = this.Width - 20;
            var ending = Math.Max(Song.ConvertPhraseToMS(Song.GetEndingTimeInPhrase())/1000.0, AudioEnd);

            //Draw outro section.
            var width = AudioEnd / ending * _totalBarWidth;
            width = Math.Min(width, _totalBarWidth);
            _barParts.Draw(spriteBatch, 3,(int) width, barHeight,position);

            //Draw playable section.
            width = Song.Length /ending * _totalBarWidth;
            width = Math.Min(width, _totalBarWidth);
            _labelPositions[2] = (int) width;
            _barParts.Draw(spriteBatch, 2, (int)width, barHeight, position);

            //Draw intro section.
            width = Song.Offset / ending * _totalBarWidth;
            width = Math.Min(width, _totalBarWidth);
            _labelPositions[1] = (int)width;
            _barParts.Draw(spriteBatch, 1, (int)width, barHeight, position);

            //Draw skipped section.
            width = Song.AudioStart / ending * _totalBarWidth;
            width = Math.Min(width, _totalBarWidth);
            _labelPositions[0] = (int)width;
            _barParts.Draw(spriteBatch, 0, (int)width, barHeight, position);
        }

        private void DrawEdges(SpriteBatch spriteBatch)
        {
            var position = this.Position.Clone();
            position.Y += 20;
            _barEdges.Draw(spriteBatch,0,10,this.Height - 40,position);
            position.X += this.Width - 10;
            _barEdges.Draw(spriteBatch,1,10,this.Height-40,position);
        }
    }
}
