using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Players;

namespace WGiBeat.Drawing
{
    public class ProfileLevelDisplay : DrawableObject
    {

        public Player Player { get; set; }
        public byte Opacity { get; set; }

        private Sprite _levelBaseSprite;
        private Sprite _levelFrontSprite;

        public ProfileLevelDisplay()
        {
            this.Width = 318;
            this.Opacity = 255;
            InitSprites();
        }

        private void InitSprites()
        {
            _levelBaseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("PlayerLevelBarBase"),

            };
            _levelFrontSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("PlayerLevelBarFront"),

            };
        }

        private Vector2 _levelTextPosition;
        private Color _drawColor = Color.Black;

        public override void Draw(SpriteBatch spriteBatch)
        {
           
            if (Player.Profile == null)
            {
                return;
            }
            _drawColor.A = Opacity;
            _levelBaseSprite.Position = new Vector2(this.X, this.Y);
            _levelFrontSprite.Position = new Vector2(this.X, this.Y + 12);
            _levelBaseSprite.ColorShading.A = this.Opacity;
            _levelFrontSprite.ColorShading.A = this.Opacity;
            _levelTextPosition = new Vector2(this.X + this.Width - 3,this.Y-17);
      
            var progress = Player.GetLevelProgressSafe();
            progress = Math.Min(1, progress);

            _levelFrontSprite.Width = (int)(this.Width * progress);
            _levelFrontSprite.Draw(spriteBatch);
            _levelBaseSprite.Width = this.Width;
            _levelBaseSprite.Draw(spriteBatch);


            //Draw level text.
            var playerlevel = String.Format("{0:00}", Player.GetLevel());
            var scale = TextureManager.ScaleTextToFit(playerlevel, "TwoTech36", 34, 40);
            TextureManager.DrawString(spriteBatch,playerlevel, "TwoTech36", _levelTextPosition, scale, _drawColor,
   FontAlign.RIGHT);

        
        }


    }
}
