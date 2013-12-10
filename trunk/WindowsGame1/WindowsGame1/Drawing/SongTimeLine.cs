using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class SongTimeLine : DrawableObject 
    {
        private SpriteMap3D _barParts;
        private SpriteMap3D _barEdges;
        private Sprite3D _currentPosition;


        public double? Offset { get; set; }
        public double? Length { get; set; }
        public double? AudioStart { get; set; }
        public double? CurrentPosition { get; set; }

        public GameSong Song { get; set; }
        public double AudioEnd { get; set; }

        public bool ShowLabels { get; set; }
        public SongTimeLine()
        {
            InitSprites();
        }

        private void InitSprites()
        {
            _barParts = new SpriteMap3D
                            {Columns = 1, Rows = 5, Texture = TextureManager.Textures("SongTimeLineParts")};
            _barEdges = new SpriteMap3D { Columns = 2, Rows = 1, Texture = TextureManager.Textures("SongTimeLineEdges") };
            _currentPosition = new Sprite3D
                                   {
                                      Texture = TextureManager.Textures("SongTimeLineCurrentPosition"),
                                      Width= 6
                                   };
        }

        public override void Draw()
        {
            if (Song == null)
            {
                return;
            }
            DrawEdges();
            DrawBars();
            DrawCurrentPosition();
            DrawLabels();
        }

        private void DrawCurrentPosition()
        {
            if (CurrentPosition == null || CurrentPosition.Value == 0)
            {
                return;
            }
            _currentPosition.Height = this.Height - 40;
            var position = this.Position.Clone();
            position.X += 10;
            position.Y += 20;
            var ending = Math.Max(Song.ConvertPhraseToMS(Song.GetEndingTimeInPhrase()) / 1000.0, AudioEnd);

            position.X += (float) (CurrentPosition.Value/ ending * _totalBarWidth);
            position.X -= _currentPosition.Width/2;
            _currentPosition.Position = position;
            _currentPosition.Draw();
        }

        private void DrawLabels()
        {
            if (!ShowLabels)
            {
                return;
            }
            var position = new Vector2(_labelPositions[0] + this.X + 10, this.Y);
          if (_labelPositions[0] != 0.0)
          {

              if (position.X < 50)
              {
                  position.X = 50;
              }        
              FontManager.DrawString("AudioStart", "DefaultFont", position, Color.Black, FontAlign.Center);
          }
            position.X = (_labelPositions[1] + this.X + 10);
            position.Y = this.Y + 40;

            if (position.X < 35)
            {
                position.X = 35;
            }
            FontManager.DrawString("Offset", "DefaultFont", position, Color.Black, FontAlign.Center);

            position.X = (_labelPositions[2] + this.X + 10);
            position.Y = this.Y;
          FontManager.DrawString("Length", "DefaultFont", position, Color.Black, FontAlign.Right);
        }

        private float _totalBarWidth;
        private readonly int[] _labelPositions = new int[4];
        private void DrawBars()
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
            _barParts.Draw( 3,(int) width, barHeight,position);

            //Draw playable section.
            width = (Length ?? Song.Length) /ending * _totalBarWidth;
            width = Math.Min(width, _totalBarWidth);
            _labelPositions[2] = (int) width;
            _barParts.Draw( 2, (int)width, barHeight, position);

            //Draw intro section.
            width = (Offset ?? Song.Offset) / ending * _totalBarWidth;
            width = Math.Min(width, _totalBarWidth);
            _labelPositions[1] = (int)width;
            _barParts.Draw( 1, (int)width, barHeight, position);

            //Draw skipped section.
            width = (AudioStart ?? Song.AudioStart) / ending * _totalBarWidth;
            width = Math.Min(width, _totalBarWidth);
            _labelPositions[0] = (int)width;
            _barParts.Draw( 0, (int)width, barHeight, position);
        }

        private void DrawEdges()
        {
            var position = this.Position.Clone();
            position.Y += 20;
            _barEdges.Draw(0,10,this.Height - 40,position);
            position.X += this.Width - 10;
            _barEdges.Draw(1,10,this.Height-40,position);
        }
    }
}
