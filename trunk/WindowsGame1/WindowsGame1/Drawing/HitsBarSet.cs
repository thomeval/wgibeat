using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class HitsBarSet : DrawableObject
    {
        private readonly Player[] _players;
        private readonly MetricsManager _metrics;
        private readonly GameType _gameType;
        private readonly byte[] _opacity;
        private Color _textColor = Color.Black;
        private Sprite _baseSprite;

        public HitsBarSet()
        {
            _opacity = new byte[4];
        }

        public HitsBarSet(MetricsManager metrics, Player[] players, GameType type)
            :this()
        {
            _metrics = metrics;
            _players = players;
            _gameType = type;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_baseSprite == null)
            {
                _baseSprite = new Sprite
                    {Width = 80};
            }
            for (int x = 0; x < 4; x++)
            {
                if (!_players[x].Playing)
                {
                    continue;
                }
                if (_players[x].Hits < 0)
                {
                    _opacity[x] = (byte) Math.Max(_opacity[x] - 10, 0);
                }
                else
                {
                    _opacity[x] = (byte) Math.Min(_opacity[x] + 10, 255);
                }
                _baseSprite.SpriteTexture = TextureManager.Textures[DetermineSpriteName(x)];
                _baseSprite.ColorShading.A = _opacity[x];
                _textColor.A = _opacity[x];
                _baseSprite.SetPosition(_metrics["HitsBar" + DetermineSuffix(), x]);
                _baseSprite.Draw(spriteBatch);
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("{0:D3}", _players[x].Hits),
       _metrics["HitsText" + DetermineSuffix(), x], _textColor);

            }
        }

        private string DetermineSpriteName(int player)
        {
            var result = "hitsBar";

            result += DetermineSuffix();
                result += (player % 2 == 0) ? "Left" : "Right";
            return result;
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
