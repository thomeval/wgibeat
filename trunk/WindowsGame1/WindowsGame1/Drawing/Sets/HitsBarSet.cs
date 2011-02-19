using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class HitsBarSet : DrawableObjectSet
    {
        private readonly byte[] _opacity;
        private Color _textColor = Color.Black;
        private SpriteMap _baseSprite;
        private SpriteMap _baseOvermaskSprite;

        private readonly byte[] _overmaskOpacity;
        private readonly long[] _lastMilestone;
        private readonly Color[] _overmaskColors = {Color.Red,  Color.Blue, Color.Lime, Color.Yellow};

        public HitsBarSet(MetricsManager metrics, Player[] players, GameType type)
            : base(metrics, players, type)
        {
            _opacity = new byte[4];
            _overmaskOpacity = new byte[4];
            _lastMilestone = new long[4];
            SetupSprites();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            for (int x = 0; x < 4; x++)
            {

                if (!Players[x].Playing)
                {
                    continue;
                }
                if (Players[x].Hits < 25)
                {
                    _opacity[x] = (byte)Math.Max(_opacity[x] - 8, 0);
                    _lastMilestone[x] = 0;
                }
                else
                {
                    _opacity[x] = (byte)Math.Min(_opacity[x] + 8, 255);
                }
                _overmaskOpacity[x] = (byte) Math.Max(0,  _overmaskOpacity[x] - 8);
                _baseSprite.SpriteTexture = TextureManager.Textures("HitsBar" + DetermineSuffix());
                _baseSprite.ColorShading.A = _opacity[x];
                _textColor.A = _opacity[x];
                _baseSprite.Draw(spriteBatch, x, _metrics["HitsBar" + DetermineSuffix(), x]);

                DrawOvermask(spriteBatch,x);

                TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", Players[x].Hits), "DefaultFont",
                       _metrics["HitsText" + DetermineSuffix(), x], _textColor,FontAlign.CENTER);
            }
        }

        private void DrawOvermask(SpriteBatch spriteBatch, int x)
        {
            if (PlayerAtNewMilestone(x))
            {
                _overmaskOpacity[x] = 255;
                _lastMilestone[x] = Players[x].Hits;
            }

            _baseOvermaskSprite.ColorShading = _overmaskColors[x];
            _baseOvermaskSprite.ColorShading.A = _overmaskOpacity[x];
            _baseOvermaskSprite.Draw(spriteBatch, x, _metrics["HitsBar" + DetermineSuffix(), x]);
        }

        private bool PlayerAtNewMilestone(int x)
        {
            return ((Players[x].Hits > _lastMilestone[x]) && Players[x].Hits > 0) && ((Players[x].Hits == 50) || (Players[x].Hits%100 == 0));
        }

        private void SetupSprites()
        {
            _baseSprite = new SpriteMap
            {
                Columns = 1,
                Rows = 4,
                SpriteTexture = TextureManager.Textures("HitsBar" + DetermineSuffix())
            };
            _baseOvermaskSprite = new SpriteMap
                                      {
                                          Columns = 1,
                                          Rows = 4,
                                          SpriteTexture = TextureManager.CreateWhiteMask("HitsBar" + DetermineSuffix()),
                                          ColorShading = {A = 128}
                                      };
        }


        private string DetermineSuffix()
        {
            switch (_gameType)
            {
                case GameType.COOPERATIVE:
                    return "Coop";
                default:
                    return "";
            }
        }
    }
}