using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class SongSortDisplay : DrawableObject
    {

        public bool Active { get; set; }
        private byte _activeOpacity = 0;

        public SongSortMode SongSortMode { get; set;}
        
        private Sprite _backgroundSprite;
        private Sprite _listBackgroundSprite;
        private SpriteMap _arrowSprites;
        private Vector2 _textPosition;
        public int BaseHeight { get; set; }

        public SongSortDisplay()
        {
            this.Width = 300;
            this.Height = 50;
        }
        public void InitSprites()
        {
            _backgroundSprite = new Sprite {SpriteTexture = TextureManager.Textures["SongSortBackground"]};
            _arrowSprites = new SpriteMap {SpriteTexture = TextureManager.Textures["IndicatorArrows"], Columns=4, Rows = 1};
            _listBackgroundSprite = new Sprite {SpriteTexture = TextureManager.Textures["SongSortListBackground"]};
            _textPosition = new Vector2();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                _activeOpacity = (byte) Math.Min(_activeOpacity + 10, 255);
            }
            else
            {
                _activeOpacity = (byte) Math.Max(_activeOpacity - 10, 0);
            }

            if (BaseHeight == 0)
            {
                BaseHeight = 50;
            }
            SetSpritePositions();
            _backgroundSprite.Draw(spriteBatch);
            _textPosition.X = this.X + (this.Width/2);
            _textPosition.Y = this.Y;
            TextureManager.DrawString(spriteBatch,"" + SongSortMode, "TwoTechLarge",_textPosition,Color.Black, FontAlign.CENTER);

            _arrowSprites.ColorShading.A = _activeOpacity;
            _arrowSprites.Draw(spriteBatch, 1, 35, 35, this.X + 15, this.Y + 8);
            _arrowSprites.Draw(spriteBatch, 0, 35, 35, this.X + this.Width - 40, this.Y + 8);

            if (Active)
            {
                DrawList(spriteBatch);
            }
        }

        private void SetSpritePositions()
        {
            _backgroundSprite.SetPosition(this.X,this.Y);
        }

        private void DrawList(SpriteBatch spriteBatch)
        {
            _listBackgroundSprite.Height = this.Height - BaseHeight;
            _listBackgroundSprite.Width = this.Width;
            _listBackgroundSprite.SetPosition(this.X, this.Y + this.BaseHeight);
         //   _listBackgroundSprite.Draw(spriteBatch);
        }

        public void IncrementSort()
        {
            SongSortMode = (SongSortMode)(((int)(SongSortMode) + 1) % (int)SongSortMode.COUNT);
        }

        public void DecrementSort()
        {
            var current = (int) SongSortMode - 1;
            if (current < 0)
            {
                current += (int) SongSortMode.COUNT;
            }
            SongSortMode = (SongSortMode) current;
        }
    }
       public enum SongSortMode
    {
        TITLE = 0,
        ARTIST = 1,
        BPM = 2,
        COUNT = 3
    }
}
