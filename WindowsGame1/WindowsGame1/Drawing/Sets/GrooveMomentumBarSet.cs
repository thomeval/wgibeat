using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class GrooveMomentumBarSet : DrawableObjectSet
    {
        private double _displayedGrooveMomentum;
        private readonly GrooveMomentumBar _gmbar;
        public GrooveMomentumBarSet(MetricsManager metrics, Player[] players, GameType gameType) : base(metrics, players, gameType)
        {
            _gmbar = new GrooveMomentumBar {Position = metrics["GrooveMomentumBar", 0], Size = metrics["GrooveMomentumBar.Size",0], BarOffset = metrics["GrooveMomentumBar.Offset",0]};
        }

        private const int GM_CHANGE_SPEED = 12;
        public void UpdateDisplayedGM()
        {

            var diff = Player.GrooveMomentum - _displayedGrooveMomentum;
            if (Math.Abs(diff) < 0.001)
            {
                _displayedGrooveMomentum = Player.GrooveMomentum;
            }
            else
            {
              
                var changeMx = Math.Min(1, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * GM_CHANGE_SPEED);
                _displayedGrooveMomentum += diff * (changeMx);
            }

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_gameType != GameType.COOPERATIVE)
            {
                return;
            }

            UpdateDisplayedGM();
            _gmbar.DisplayedGrooveMomentum = _displayedGrooveMomentum;
             _gmbar.Position = _metrics["GrooveMomentumBar", 0];
             _gmbar.Draw(spriteBatch);

        
        }
    }
}
