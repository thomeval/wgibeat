﻿using System;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing.Sets
{
    public class HitsBarSet : DrawableObjectSet
    {
        private readonly byte[] _opacity;
        private Color _textColor = Color.Black;
        private SpriteMap _baseSprite;


        public HitsBarSet(MetricsManager metrics, Player[] players, GameType type)
            :base(metrics,players,type)
        {
            _opacity = new byte[4];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int width, height;
            if (_baseSprite == null)
            {
                _baseSprite = new SpriteMap {Columns = 1, Rows = 4};
            }
            switch (_gameType)
            {

                case GameType.COOPERATIVE:
                    width = 80;
                    height = 25;
                    break;
                default:
                    width = 60;
                    height = 55;
                    break;
            }
            for (int x = 0; x < 4; x++)
            {
                if (!Players[x].Playing)
                {
                    continue;
                }
                if (Players[x].Hits < 25)
                {
                    _opacity[x] = (byte) Math.Max(_opacity[x] - 10, 0);
                }
                else
                {
                    _opacity[x] = (byte) Math.Min(_opacity[x] + 10, 255);
                }
                _baseSprite.SpriteTexture = TextureManager.Textures("HitsBar" + DetermineSuffix());
                _baseSprite.ColorShading.A = _opacity[x];
                _textColor.A = _opacity[x];
                _baseSprite.Draw(spriteBatch, x, width, height, _metrics["HitsBar" + DetermineSuffix(), x]);

                spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), String.Format("{0:D3}", Players[x].Hits),
                                       _metrics["HitsText" + DetermineSuffix(), x], _textColor);

            }
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