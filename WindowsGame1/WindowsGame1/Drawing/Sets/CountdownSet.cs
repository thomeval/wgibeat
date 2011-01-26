using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing.Sets
{
    public class CountdownSet : DrawableObjectSet
    {

        private SpriteMap _countdownSpriteMap;
        private readonly double[] _threshholds = { -1.00, -0.75, -0.5, -0.25, 0.0 };

        public CountdownSet(MetricsManager metrics, Player[] players, GameType type)
            :base(metrics,players,type)
        {

            InitSprites();
        }
        public void InitSprites()
        {
            _countdownSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = 5,
                SpriteTexture = TextureManager.Textures("Countdown")
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch,0.0);
        }
        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            for (int x = 0; x < Players.Count(); x++)
            {
                if (!Players[x].Playing)
                {
                    continue;
                }
 

                for (int y = 0; y < _threshholds.Count(); y++)
                {
                    if (phraseNumber < _threshholds[y])
                    {
                        _countdownSpriteMap.ColorShading.A = (byte)Math.Min(255, (_threshholds[y] - phraseNumber) * 255 * 4);
                        _countdownSpriteMap.Draw(spriteBatch, y, 200, 60, _metrics["Countdown", x]);
                        break;
                    }
                }

            }
        }
    }
}