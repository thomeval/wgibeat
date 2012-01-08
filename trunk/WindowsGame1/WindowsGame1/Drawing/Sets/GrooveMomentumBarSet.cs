using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class GrooveMomentumBarSet : DrawableObjectSet
    {
        private double _displayedGrooveMomentum;
        private GrooveMomentumBar _gmbar;
        public GrooveMomentumBarSet(MetricsManager metrics, Player[] players, GameType gameType) : base(metrics, players, gameType)
        {
            _gmbar = new GrooveMomentumBar {Position = metrics["GrooveMomentumBar", 0], Width = 275};
        }

        public void UpdateDisplayedGM()
        {
            var diff = Player.GrooveMomentum - _displayedGrooveMomentum;
            if (Math.Abs(diff) < 0.001)
            {
                _displayedGrooveMomentum = Player.GrooveMomentum;
            }
            else
            {
                _displayedGrooveMomentum += diff / 8.0;
            }

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            UpdateDisplayedGM();
            _gmbar.DisplayedGrooveMomentum = _displayedGrooveMomentum;
            _gmbar.Draw(spriteBatch);
        }
    }
}
